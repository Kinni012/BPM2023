﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CobotAssignmentAndJobShopSchedulingProblem;
using Easy4SimFramework;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.RealVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using DoubleMatrix = HeuristicLab.Data.DoubleMatrix;
using Environment = System.Environment;
using HeuristicLab.Random;

// ReSharper disable UnusedMember.Global

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [StorableType("2DBD38DD-3813-43C3-94A5-A6C956CE5924")]
    [Creatable(CreatableAttribute.Categories.ExternalEvaluationProblems, Priority = 111)]
    [Item("Easy4Sim RealEncoding parameter optimization problem", "Parameter optimization for a RealEncoding Easy4Sim problem")]
    public class Easy4SimRealEncodingProblem : SingleObjectiveBasicProblem<RealVectorEncoding>
    {
        #region Const strings for naming
        private const string Easy4SimParameterName = "Easy4SimFile";

        private const string BestSolutionName = "BestSolutionEasy4Sim";
        private const string CobotLocationsName = "CobotLocations";
        private const string MiningInformationName = "MiningInformation";
        private const string MiningInformationName2 = "MiningInformationResource";
        private const string BestSolutionQualityName = "BestSolutionQualityEasy4Sim";

        private const string GanttInfo = "GanttInfo";
        private const string BestMakeSpanName = "BestMakespan";
        private const string BestCostName = "BestCost";
        private const string BestMakeSpanDescription = "Makespan of the best solution found so far";
        private const string BestCostDescription = "Cost of the ebst solution found so far";
        private const string GanttInfoDescription = "Gantt info of the latest run";

        private const string ReplayInfo = "ReplayInfo";
        private const string MultiObjectiveName = "MultiObjective optimization";

        private const string BestSolutionNameDescription = "Best solution found so far";
        private const string BestSolutionQualityDescription = "Best solution quality found so far";
        private const string CobotLocationsNameDescriptions = "Cobot locations";
        private const string MiningInformationNameDescriptions = "MiningInformation";
        private const string MiningInformationNameDescriptions2 = "MiningInformation resource";
        private const string ReplayInfoDescription = "ReplayInfoDescription";
        private const string MultiObjectiveDescription = "Use multiobjective function?";
        #endregion

        //Maximize or minimize the problem
        public static long EvaluatedSolutions { get; set; }
        public static List<double> AverageSolutions { get; set; }
        private string FileName { get; set; }
        public override bool Maximization { get; }
        public static double BestSolutionSoFarGa = double.MaxValue;
        public static bool ApplyVNS = true;
        public static int VnsNeighborhoodOperator { get; set; }
        public static double VnsPercentage { get; set; }
        public static double VnsIntelligent { get; set; }
        public static bool UseNormalizedValue = false;
        public static bool DeterministicOrStochastic { get; set; }
        public static int AmountOfStochasticSolvers { get; set; } = 5;

        //Save process mining to file for analysis in python
        public static bool ApplyProcessMining = true;

        //Live optimization with calls to python
        public static bool ApplyProcessMiningOptimization = false;
        public static bool ProcessMiningBestSolutionChanged = false;
        public MersenneTwister Twister { get; private set; }
        private IntermediateLocalProcessModelResult MinedResult = null;


        public double TimeFactor = 1;
        public double CostFactor = 1;
        public double bestCost = 0;
        public long bestTime = 0;
        public int AmountOfCobots { get; set; }

        //############## Settings of the solver ####################
        [Storable]
        public SolverSettings SolverSettings { get; set; }
        //############## Solver that is cloned for the simulation #################################
        [Storable]
        public Solver Solver { get; set; }

        //############### Gantt info ####################################
        [Storable]
        public ValueLookupParameter<TextValue> GanttInformation
        {
            get => (ValueLookupParameter<TextValue>)Parameters[GanttInfo];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        //############### File that is used for the simulation ####################################
        [Storable]
        public Easy4SimFileValue Easy4SimEvaluationScript
        {
            get => Easy4SimParameter.Value;
            set => Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(value.Value,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());
        }

        [Storable]
        public ValueLookupParameter<Easy4SimFileValue> Easy4SimParameter
        {
            get => (ValueLookupParameter<Easy4SimFileValue>)Parameters[Easy4SimParameterName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        //############### Parameters used for the best solution ####################################
        [Storable]
        public ValueLookupParameter<TextValue> BestSolution
        {
            get => (ValueLookupParameter<TextValue>)Parameters[BestSolutionName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        //############### Time information ####################################
        [Storable]
        public ValueLookupParameter<TextValue> ReplayInformation
        {
            get => (ValueLookupParameter<TextValue>)Parameters[ReplayInfo];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        //############### Location of the cobots in the current solution ####################################
        [Storable]
        public ValueLookupParameter<TextValue> CobotLocations
        {
            get => (ValueLookupParameter<TextValue>)Parameters[CobotLocationsName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        //############### Process Mining information ####################################
        [Storable]
        public ValueLookupParameter<TextValue> MiningInformation
        {
            get => (ValueLookupParameter<TextValue>)Parameters[MiningInformationName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        //############### Process Mining information ####################################
        [Storable]
        public ValueLookupParameter<TextValue> MiningInformationResource
        {
            get => (ValueLookupParameter<TextValue>)Parameters[MiningInformationName2];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        //############### Best Quality found so far ####################################
        [Storable]
        public ValueLookupParameter<StringValue> BestQuality
        {
            get => (ValueLookupParameter<StringValue>)Parameters[BestSolutionQualityName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        //############### Best solution makespan ####################################
        [Storable]
        public ValueLookupParameter<StringValue> BestMakespan
        {
            get => (ValueLookupParameter<StringValue>)Parameters[BestMakeSpanName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        //############### Best solution cost ####################################
        [Storable]
        public ValueLookupParameter<StringValue> BestCost
        {
            get => (ValueLookupParameter<StringValue>)Parameters[BestCostName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        //############### Multiobjective ####################################
        [Storable]
        public ValueLookupParameter<BoolValue> MultiObjective
        {
            get => (ValueLookupParameter<BoolValue>)Parameters[MultiObjectiveName];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        [Storable]
        public RealVectorEncoding RealEncoding { get; set; }

        [Storable]
        public List<ParameterArrayOptimization<double>> RealInformation { get; set; }

        public static DateTime EndTime { get; set; }

        [StorableConstructor]
        public Easy4SimRealEncodingProblem(StorableConstructorFlag _) : base(_) { }
        public Easy4SimRealEncodingProblem(bool maximization = false, int amountOfCobots = 0, Random.MersenneTwister twister = null)
        {
            //##################### Initialize the parameters that are seen in HeuristicLab #####################################################
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());
            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription, new StringValue(""));
            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription, new TextValue(""));

            BestCost = new ValueLookupParameter<StringValue>(BestCostName, BestCostDescription, new StringValue(""));
            BestMakespan = new ValueLookupParameter<StringValue>(BestMakeSpanName, BestMakeSpanDescription, new StringValue(""));
            GanttInformation = new ValueLookupParameter<TextValue>(GanttInfo, GanttInfoDescription, new TextValue(""));

            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions, new TextValue(""));
            MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions, new TextValue(""));
            MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2, new TextValue(""));
            ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription, new TextValue(""));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription, new BoolValue(true));
            //##################### Initialize the encoding #####################################################################################
            RealEncoding = new RealVectorEncoding();
            Encoding = RealEncoding;
            RealInformation = new List<ParameterArrayOptimization<double>>();

            //##################### File value and file value changed ############################################################################
            Easy4SimEvaluationScript.FileDialogFilter = @"Easy4Sim Excel files|*.xlsx";
            Easy4SimEvaluationScript.ToStringChanged += FileValueChanged;
            AmountOfCobots = amountOfCobots;
            if (twister != null)
                Twister = twister;
            else
                Twister = new Random.MersenneTwister();

            //##################### General parameters ############################################################################
            Maximization = maximization;
            BestKnownQuality = 0;

        }

        private void FileValueChanged(object sender, EventArgs e)
        {
            FileValueChanged(sender);
        }

        public void FileValueChanged(object sender)
        {
            //############################### Initialize without file #######################################################################
            if (sender is Easy4SimFileValue file)
            {
                SimulationObjects simulationObjects = new SimulationObjects();
                Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

                Logger logger = null; //set log path

                SolverSettings settings = new SolverSettings(environment, simulationObjects, logger);

                DataStore.FileLoader loader = new DataStore.FileLoader();
                string fileContent = loader.LoadFile(file.StringValue.Value.ToString());

                ProjectLoader projectLoader = new ProjectLoader(1, "ProjectLoader1", settings);
                projectLoader.FileContent = new ParameterString(fileContent);


                ProjectProcessor processor = new ProjectProcessor(2, "ProjectProcessor1", settings);
                processor.ReadData.Connect(projectLoader.ReadData);
                processor.FullLog = new ParameterBool(false);
                processor.InitializeForIntOptimization = new ParameterBool(false);
                processor.InitializeForDoubleOptimization = new ParameterBool(true);

                ProjectStatistics statistics = new ProjectStatistics(3, "ProjectStatistics1", settings);
                statistics.InputData.Connect(processor.OutputData);

                Link link = new Link();
                link.OutputComponentName = "ProjectProcessor1";
                link.InputComponentName = "ProjectStatistics1";
                link.OutputConnectorName = "OutputData";
                link.InputConnectorName = "InputData";

                simulationObjects.AddLink(link);

                Link link2 = new Link();
                link2.OutputComponentName = "ProjectLoader1";
                link2.InputComponentName = "ProjectProcessor1";
                link2.OutputConnectorName = "ReadData";
                link2.InputConnectorName = "ReadData";

                simulationObjects.AddLink(link2);

                SolverSettings = settings;
                Solver = new Solver(settings);
                Solver.SimulationStatistics.AmountOfCobots = AmountOfCobots;
                Solver.Init();
            }

            //################################ Set maximization to false ########################################################################
            Parameters.Remove("Maximization");
            Parameters.Add(new ValueParameter<BoolValue>("Maximization", new BoolValue(Maximization)));

            //################################ Initialize Encoding ##############################################################################
            //################################ Initialize Encoding         #######################################################################
            //################################ Set bounds for optimization #######################################################################
            RealInformation.Clear();


            //intCounter = 0;
            int realCounter = 0;
            List<double> lowerBounds = new List<double>();
            List<double> upperBounds = new List<double>();
            foreach (CSimBase cSimBase in Solver.SimulationObjects.SimulationList.Values)
            {
                foreach (PropertyInfo info in cSimBase.GetType().GetProperties())
                {
                    foreach (Attribute attribute in info.GetCustomAttributes())
                    {
                        if (attribute.GetType().Name == "Optimization")
                        {
                            var value = info.GetValue(cSimBase);
                            if (value is ParameterArrayOptimization<double> parameterArrayOptimization)
                            {
                                realCounter += parameterArrayOptimization.Value.Length;
                                lowerBounds.AddRange(parameterArrayOptimization.LowerBounds);
                                upperBounds.AddRange(parameterArrayOptimization.UpperBounds);
                                ParameterArrayOptimization<double> temp = parameterArrayOptimization.Clone();
                                temp.BaseObject = cSimBase;
                                temp.PropertyInfo = info;
                                RealInformation.Add(temp);
                            }
                        }
                    }
                }
            }
            RealEncoding.Length = realCounter;
            RealEncoding.Bounds = new DoubleMatrix(realCounter, 2);
            for (int i = 0; i < realCounter; i++)
            {
                RealEncoding.Bounds[i, 0] = lowerBounds[i];
                RealEncoding.Bounds[i, 1] = upperBounds[i];
            }

            OnOperatorsChanged();
        }

        public override void Analyze(Individual[] individuals, double[] qualities, ResultCollection results, IRandom random)
        {
            base.Analyze(individuals, qualities, results, random);
        }


        public double GetFitnessOfSolvers(List<Solver> solvers)
        {
            double result = 0;
            if (MultiObjective.Value.Value)
            {
                foreach (Solver solver in solvers)
                {
                    double fitness = solver.SimulationStatistics.Fitness * CostFactor + TimeFactor * solver.Environment.SimulationTime;
                    if (UseNormalizedValue)
                    {
                        NormalizedObjectiveValue normalizedValue = new NormalizedObjectiveValue(solver.Environment.SimulationTime, solver.SimulationStatistics);
                        fitness = normalizedValue.NormalizedCostValue + normalizedValue.NormalizeTimeValue;
                    }

                    result += fitness;
                }
            }
            else
            {
                foreach (Solver solver in solvers)
                    result += solver.SimulationStatistics.Fitness;

                result /= solvers.Count;
            }

            result /= solvers.Count;
            return result;
        }




        public override double Evaluate(Individual individual, IRandom random)
        {
            List<Solver> solvers = new List<Solver>();
            if (!DeterministicOrStochastic)
                solvers.Add((Solver)Solver.Clone());
            else
                for (int i = 0; i < 5; i++)
                    solvers.Add((Solver)Solver.Clone());


            Easy4SimFramework.SimulationStatistics.ApplyVNS = ApplyVNS;

            foreach (Solver solver in solvers)
            {
                if (solver.SolverSettings == null)
                    return -1;
                solver.Logger = null; //deactivate logger for first tests
            }

            if (individual?.Values == null) return -2;

            RealVector realVector = null;
            foreach (KeyValuePair<string, IItem> pair in individual.Values.ToList())
            {
                if (pair.Value is RealVector rVector)
                    realVector = rVector;
            }

            if (realVector == null)
                return 0;


            int skipCounter = 0;
            bool valuesSet = false;
            foreach (ParameterArrayOptimization<double> information in RealInformation)
            {
                if (information.Amount == 0)
                    continue;

                foreach (Solver solver in solvers)
                    SimulationPropertySetter.SetParameterOptimizationInSimulation(solver, information, realVector.Skip(skipCounter).Take(information.Amount));

                skipCounter += information.Amount;
                valuesSet = true;
            }


            if (!valuesSet)
                return 0;
            //######### Run the simulation ###############
            try
            {
                foreach (Solver solver in solvers)
                    solver.RunFinishEventOnly();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }




            double currentRunFitness = GetFitnessOfSolvers(solvers);

            UpdateBestSolution(currentRunFitness);


            //This is set by the user input => should vns be applied
            if (ApplyVNS)
            {
                bool vns = GenericProblemMethods.InRangeForVns(Maximization, currentRunFitness, BestSolutionSoFarGa, VnsPercentage);
                //Check if the solution is in a specified range to the best solution found so far
                //If it is we apply a VNS
                if (vns)
                {
                    //If current solution is within 10% of the best solution found so far 
                    //Start the VNS 
                    int i = 0;
                    int k = 0;
                    while (k < 3 && DateTime.Now < EndTime)
                    {
                        try
                        {
                            //Clone the initialized solvers for the VNS
                            List<Solver> solversVns = GenericProblemMethods.CloneSolvers(Solver, AmountOfStochasticSolvers, DeterministicOrStochastic);

                            //Copy the solution encoding to apply changes later on
                            RealVector realVector2 = new RealVector(realVector);

                            int amountOfChanges = SetAmountOfChanges(k);

                            if (Twister.NextDouble() < (1 - VnsIntelligent))
                                ApplyChanges(realVector2, amountOfChanges);
                            else // 80% change for intelligent change
                                ApplyIntelligentChanges(realVector2, amountOfChanges, Solver, RealInformation);

                            //======================= Apply changes to the solvers ======================================
                            int skipCounter2 = 0;
                            bool valuesSet2 = false;
                            foreach (ParameterArrayOptimization<double> information in RealInformation)
                            {
                                if (information.Amount == 0)
                                    continue;
                                foreach (Solver solver2 in solversVns)
                                {
                                    SimulationPropertySetter.SetParameterOptimizationInSimulation(solver2, information, realVector2.Skip(skipCounter2).Take(information.Amount));
                                }

                                skipCounter2 += information.Amount;
                                valuesSet2 = true;
                            }

                            if (!valuesSet2)
                                break;
                            //======================= Evaluate solvers with changes ======================================
                            foreach (Solver solver2 in solversVns)
                                solver2.RunFinishEventOnly();

                            double currentRunFitness2 = GetFitnessOfSolvers(solversVns);


                            if (Maximization)
                            {
                                if (currentRunFitness2 > currentRunFitness)
                                {
                                    realVector = new RealVector(realVector2);
                                    currentRunFitness = currentRunFitness2;
                                    if (currentRunFitness > BestSolutionSoFarGa)
                                    {
                                        if (UseNormalizedValue)
                                            BestSolutionSoFarGa = currentRunFitness;
                                        else
                                            BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                                    }
                                    solvers = solversVns;
                                    i = 0;
                                    k = 0;
                                    continue;
                                }
                            }
                            else
                            {
                                if (currentRunFitness2 < currentRunFitness)
                                {
                                    realVector = new RealVector(realVector2);
                                    currentRunFitness = currentRunFitness2;
                                    if (currentRunFitness < BestSolutionSoFarGa)
                                    {
                                        if (UseNormalizedValue)
                                            BestSolutionSoFarGa = currentRunFitness;
                                        else
                                            BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                                    }
                                    solvers = solversVns;
                                    i = 0;
                                    k = 0;
                                    continue;
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error in Easy4SimRealEncodingProblem");
                            Console.WriteLine(e);
                        }

                        i++;
                        if (i == 50)
                        {
                            i = 0;
                            k++;
                        }
                    }
                }
            }

            double currentRunMakespan = GenericProblemMethods.GetMakespanOfSolvers(solvers);
            double currentRunCosts = GenericProblemMethods.GetCostsOfSolvers(solvers);

            //Set changes in scope
            Individual.SetScopeValue("RealVector", individual.Scope, realVector);
            long runtime = 0;
            foreach (Solver solver in solvers)
            {
                runtime += solver.SimulationStatistics.ExecutionTime;
            }

            StringBuilder stringBuilderCobotLocations = new StringBuilder();
            StringBuilder stringBuilderReplayInformation = new StringBuilder();

            StringBuilder stringBuilderMiningInformation = new StringBuilder();
            StringBuilder stringBuilderMiningInformationResource = new StringBuilder();

            if (BestQuality.Value == null || 
                string.IsNullOrWhiteSpace(BestQuality.Value.Value) || 
                !Maximization && Convert.ToDouble(BestQuality.Value.Value, CultureInfo.InvariantCulture) > currentRunFitness || 
                Maximization && Convert.ToDouble(BestQuality.Value.Value, CultureInfo.InvariantCulture) < currentRunFitness)
            {
                BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription, new StringValue(currentRunFitness.ToString(CultureInfo.InvariantCulture)));
                BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription, new TextValue(string.Join(";", realVector)));
                BestCost = new ValueLookupParameter<StringValue>(BestCostName, BestCostDescription, new StringValue(Convert.ToInt32(currentRunCosts).ToString()));
                BestMakespan = new ValueLookupParameter<StringValue>(BestMakeSpanName, BestMakeSpanDescription, new StringValue(Convert.ToInt32(currentRunMakespan).ToString()));
                foreach (Solver solver in solvers)
                {
                    foreach (CSimBase simBase in solver.SimulationObjects.SimulationList.Values)
                    {
                        var properties = simBase.GetType().GetProperties();
                        PropertyInfo info = properties.FirstOrDefault(x => x.Name.Contains("CobotInformation"));
                        if (info != null)
                        {

                            if (stringBuilderCobotLocations.ToString() != "")
                                stringBuilderCobotLocations.Append("======================================");

                            stringBuilderCobotLocations.Append(
                                new TextValue(info.GetValue(simBase).ToString()));
                        }
                        PropertyInfo info2 = properties.FirstOrDefault(x => x.Name.Contains("WorkStepInformation"));
                        if (info2 != null)
                        {
                            if (stringBuilderReplayInformation.ToString() != "")
                                stringBuilderReplayInformation.Append("======================================");

                            stringBuilderReplayInformation.Append(
                                new TextValue(info2.GetValue(simBase).ToString()));
                        }


                        if (ApplyProcessMiningOptimization)
                        {
                            PropertyInfo info4 = properties.FirstOrDefault(x => x.Name == "ProcessMiningInformationXesResource");
                            if (info4 != null)
                            {
                                Solver.SimulationStatistics.ProcessMiningInformation =
                                    info4.GetValue(simBase).ToString();
                                ProcessMiningBestSolutionChanged = true;
                            }
                        }

                        if (ApplyProcessMining)
                        {

                            PropertyInfo info3 = properties.FirstOrDefault(x => x.Name == "ProcessMiningInformationXes");
                            if (info3 != null)
                            {

                                if (stringBuilderMiningInformation.ToString() != "")
                                    stringBuilderMiningInformation.Append("======================================");

                                stringBuilderMiningInformation.Append(
                                    new TextValue(info3.GetValue(simBase).ToString()));
                            }
                            PropertyInfo info4 = properties.FirstOrDefault(x => x.Name == "ProcessMiningInformationXesResource");
                            if (info4 != null)
                            {
                                if (stringBuilderMiningInformationResource.ToString() != "")
                                    stringBuilderMiningInformationResource.Append("======================================" + Environment.NewLine);

                                stringBuilderMiningInformationResource.Append(
                                    new TextValue(info4.GetValue(simBase).ToString()));
                            }
                        }

                        PropertyInfo infoGantt = properties.FirstOrDefault(x => x.Name.Contains("GanttInformationCsv"));
                        //PropertyInfo productsProduced = properties.FirstOrDefault(x => x.Name.Contains("ProductsProducedCsv"));

                        if (infoGantt != null)
                        {
                            string test = infoGantt.GetValue(simBase).ToString();
                            //string test2 = productsProduced.GetValue(simBase).ToString();

                            GanttInformation = new ValueLookupParameter<TextValue>(GanttInfo, GanttInfoDescription, new TextValue(test));
                            //string directory = Path.Combine(new[] { Environment.CurrentDirectory, "..", "..", "Results", "Gantt" });
                            //
                            //if (!Directory.Exists(directory))
                            //    Directory.CreateDirectory(directory);

                            //string path = Path.Combine(directory, SolverSettings.Statistics.DataSet + "_" + BestQuality.Value + ".csv");
                            //string path_products = directory + SolverSettings.Statistics.DataSet + "_" + BestQuality.Value + "_productsProduced" +".csv";
                            try
                            {
                                //File.WriteAllText(path, test);
                                //File.WriteAllText(path_products, test2);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                    }
                }
            }


            if (!string.IsNullOrWhiteSpace(stringBuilderCobotLocations.ToString()))
                CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions, new TextValue(stringBuilderCobotLocations.ToString()));
            if (!string.IsNullOrWhiteSpace(stringBuilderReplayInformation.ToString()))
                ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription, new TextValue(stringBuilderReplayInformation.ToString()));
            if (!string.IsNullOrWhiteSpace(stringBuilderMiningInformation.ToString()))
                MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions, new TextValue(stringBuilderMiningInformation.ToString()));
            if (!string.IsNullOrWhiteSpace(stringBuilderMiningInformationResource.ToString()))
                MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2, new TextValue(stringBuilderMiningInformationResource.ToString()));


            if (string.IsNullOrEmpty(FileName))
            {
                Guid id = Guid.NewGuid();
                FileName = id.ToString();
            }

            string directoryOutput = Path.Combine(new [] { Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..", "Results", "testRuns"});
            if (!Directory.Exists(directoryOutput))
                Directory.CreateDirectory(directoryOutput);

            EvaluatedSolutions++;

            if (AverageSolutions == null)
                AverageSolutions = new List<double>();

            AverageSolutions.Add(currentRunFitness);

            if (EvaluatedSolutions % 100 == 0)
            {
                string line = "";
                if (EvaluatedSolutions == 100)
                    line += "Generation,BestSolution, Average in Solution" + Environment.NewLine;
                line += +(long)(EvaluatedSolutions / 100) + "," + BestQuality.Value + "," + Math.Round(AverageSolutions.Average(), 2).ToString() + Environment.NewLine;
                AverageSolutions = new List<double>();
                File.AppendAllText(Path.Combine(new []{ directoryOutput , SolverSettings.Statistics.DataSet + "_" + FileName + ".csv" }), line);
            }

            bool localProcessModels = false;
            if (Maximization)
                localProcessModels = currentRunFitness >= BestSolutionSoFarGa * (1 - VnsPercentage);
            else
                localProcessModels = currentRunFitness <= BestSolutionSoFarGa * (1 + VnsPercentage);


            if (localProcessModels)
            {
                string[] parts = stringBuilderMiningInformationResource.ToString().Split(
                    new[] { "======================================" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(part);
                    LocalProcessModels.MineLocalProcessModel miner = new MineLocalProcessModel(doc);
                    LocalProcessModels.ProcessMining.IntermediateLocalProcessModelResult result = miner.MineRandomLocalProcessModel(false);
                    MinedResult = result;
                }
            }
            return currentRunFitness;
        }

        private void UpdateBestSolution(double currentRunFitness)
        {
            if (Maximization)
            {
                if (currentRunFitness > BestSolutionSoFarGa)
                {
                    if (UseNormalizedValue)
                        BestSolutionSoFarGa = currentRunFitness;
                    else
                        BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                }
            }
            else
            {
                if (currentRunFitness < BestSolutionSoFarGa)
                {
                    if (UseNormalizedValue)
                        BestSolutionSoFarGa = currentRunFitness;
                    else
                        BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);

                }
            }
        }

        private void ApplyIntelligentChanges(RealVector realVector2, int amountOfChanges, Solver solver,
            List<ParameterArrayOptimization<double>> realInformation)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                // Rank based workstation probability
                // The higher the cost times speed factor is, the lower is the rank
                // Lowest possible rank is 1
                // Highest rank depends on the amount of workstations in a group


                int index = Twister.Next(realVector2.Length);
                if (index < realInformation[0].Amount)
                {
                    //Changes to workstation should
                    var baseObject =
                        solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                            x.Name == realInformation[0].BaseObject.Name);

                    if(baseObject is ProjectProcessor projectProcessor)
                    {
                        ConvertedData convertedData = projectProcessor.ConvertedData;
                        List<ConvertedWorkstep> worksteps = convertedData.WorkstepsWhereWorkstationHasToBeDecided;
                        ConvertedWorkstep workstep = worksteps[index];
                        string WorkstationGroup = workstep.WorkstationGroup;

                        ConvertedWorkstation[] workstations = convertedData.WorkstationsInGroup(WorkstationGroup);
                        List<IntelligentWorkstationDecision> summedCostFactor = new List<IntelligentWorkstationDecision>();

                        int currentIndex = 0;
                        foreach (var workstation in workstations)
                        {
                            int relativId = workstation.RelativeId;
                            double speed = workstation.SpeedFactorWorking;
                            double cost = workstation.CostProcessingPerSecond;

                            IntelligentWorkstationDecision intelligentWorkstationDecision = new IntelligentWorkstationDecision(cost, speed, currentIndex, relativId);
                            summedCostFactor.Add(intelligentWorkstationDecision);
                            currentIndex++;
                        }

                        //Order workstations by the cost/speed
                        summedCostFactor = summedCostFactor.OrderBy(x => x.CostTimesSpeed).ToList();

                        //Assign a rank based probability
                        for (int j = summedCostFactor.Count; j > 0; j--)
                        {
                            summedCostFactor[summedCostFactor.Count - j].Probability = j;
                        }

                        //Generate a random value
                        int generatedValue = Twister.Next(summedCostFactor.Select(x => x.Probability).Sum());


                        //Order workstations by the Index again
                        summedCostFactor = summedCostFactor.OrderBy(x => x.Index).ToList();

                        int sum = 0;
                        for (int j = 0; j < summedCostFactor.Count; j++)
                        {
                            sum += summedCostFactor[j].Probability;
                            if (generatedValue < sum)
                            {
                                double lowerBound = (double)(j) / summedCostFactor.Count;
                                double upperBound = (double)(j + 1) / summedCostFactor.Count;
                                realVector2[index] = Twister.NextDouble() * (upperBound - lowerBound) + lowerBound;
                                double test = realVector2[index];
                                return; //Change has been applied
                            }
                        }
                    }
                }
                else if (index < realInformation[0].Amount + realInformation[1].Amount)
                {
                    //changes to priority
                    double newValue = Twister.NextDouble() * 0.99999;
                    realVector2[index] = newValue;
                }
                else
                {
                    try
                    {

                        // For cobot locations two factors are important
                        // - The total cost on the machine
                        // - The total time used on the machine
                        // However, applying a intelligent decision is not easy.
                        // We need to find out on what machine we currently add the cobots and change the one cobot that should be deployed intelligent

                        // Ok let us start by finding out on which machines we currently deploy cobots

                        var baseObject =
                            solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                                x.Name == realInformation[2].BaseObject.Name);

                        if (baseObject is ProjectProcessor projectProcessor)
                        {
                            ConvertedData convertedData = projectProcessor.ConvertedData;
                            double[] rVector = realVector2
                        .Skip(realVector2.Length - solver.SimulationStatistics.AmountOfCobots)
                        .ToArray();

                            int indexInWorkstations = index - realInformation[0].Amount - realInformation[1].Amount;

                            List<ConvertedWorkstation> workstations = convertedData.WorkstationsBasedOnAssignment(rVector);
                            List<FitnessElement> fitnessPerComponent = solver.SolverSettings.Statistics.FitnessElements;


                            bool useFitness = false;
                            if (Twister.NextDouble() >= 0.5)
                            {
                                useFitness = true;
                            }

                            if (useFitness)
                                fitnessPerComponent = fitnessPerComponent.OrderBy(x => x.FitnessRank).ToList();
                            else
                                fitnessPerComponent = fitnessPerComponent.OrderBy(x => x.TimeRank).ToList();

                            for (int j = 0; j < indexInWorkstations - 1; j++)
                            {
                                //workstations where a cobot is already applied
                                var workstation = workstations[j];
                                int relativId = workstation.RelativeId;
                                fitnessPerComponent.Remove(fitnessPerComponent.FirstOrDefault(x => x.Id == relativId));
                                foreach (FitnessElement element in fitnessPerComponent)
                                {
                                    if (element.Id > relativId)
                                        element.Id -= 1;
                                }

                            }

                            //The higher the fitness rank the more costs 
                            //The higher the time rank the more time is used

                            int randomValue = 0;

                            if (useFitness)
                                randomValue = Twister.Next(fitnessPerComponent.Select(x => x.FitnessRank).Sum());
                            else
                                randomValue = Twister.Next(fitnessPerComponent.Select(x => x.TimeRank).Sum());

                            int sum = 0;
                            int workstationToAplyCobot = 0;
                            foreach (FitnessElement element in fitnessPerComponent)
                            {
                                if (useFitness)
                                    sum += element.FitnessRank;
                                else
                                    sum += element.TimeRank;

                                if (randomValue < sum)
                                {
                                    workstationToAplyCobot = element.Id;
                                    break;
                                }
                            }




                            int amountOfWorkstations = convertedData.AmountOfWorkstations;

                            double oldValue = realVector2[index];
                            double rangePerWorkstation = 1 / (double)(amountOfWorkstations - indexInWorkstations + 1);


                            double newValue = rangePerWorkstation * workstationToAplyCobot + Twister.NextDouble() * rangePerWorkstation;
                            realVector2[index] = newValue; //Apply new workstation to the index rank based on costs
                        }
                        // WorkstationsWithoutCobots


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in intelligent change real encoding");
                        Console.WriteLine(e);
                        throw e;
                    }
                }

            }
        }


        private void ApplyChanges(RealVector realVector2, int amountOfChanges)
        {
            for (int j = 0; j < amountOfChanges; j++)
            {
                int index = Twister.Next(realVector2.Length);
                double newValue = Twister.NextDouble() * 0.99999;
                realVector2[index] = newValue;
            }
        }

        private int SetAmountOfChanges(int i)
        {
            switch (i)
            {
                case 0:
                    return 1;
                case 1:
                    return 3;
                case 2:
                    return 5;
                default:
                    return 1;
            }
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimRealEncodingProblem(this, cloner);
        }
        protected Easy4SimRealEncodingProblem(Easy4SimRealEncodingProblem easy4SimRealEncodingProblem, Cloner cloner) : base(easy4SimRealEncodingProblem, cloner)
        {
            if (easy4SimRealEncodingProblem == null) return;
            //##################### Initialize the parameters that are seen in HeuristicLab #####################################################
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue(easy4SimRealEncodingProblem.Easy4SimParameter.Value.ToString()));

            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription,
               new StringValue(easy4SimRealEncodingProblem.BestQuality.Value.Value));

            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription,
                new TextValue(easy4SimRealEncodingProblem.BestSolution.Value.Value));
            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions,
                new TextValue(easy4SimRealEncodingProblem.CobotLocations.Value.Value));
            MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions,
                new TextValue(easy4SimRealEncodingProblem.MiningInformation.Value.Value));
            MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2,
                new TextValue(easy4SimRealEncodingProblem.MiningInformationResource.Value.Value));
            ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription,
                new TextValue(easy4SimRealEncodingProblem.ReplayInformation.Value.Value));

            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription,
                new BoolValue(easy4SimRealEncodingProblem.MultiObjective.Value.Value));


            Maximization = easy4SimRealEncodingProblem.Maximization;
            ((ISingleObjectiveHeuristicOptimizationProblem)this).MaximizationParameter.Hidden = false;

            SolverSettings = easy4SimRealEncodingProblem.SolverSettings?.Clone();
            Solver = new Solver(SolverSettings);
            TimeFactor = easy4SimRealEncodingProblem.TimeFactor;
            CostFactor = easy4SimRealEncodingProblem.CostFactor;
            BestKnownQuality = easy4SimRealEncodingProblem.BestKnownQuality;
            try
            {
                RealVectorEncoding encoding = new RealVectorEncoding(easy4SimRealEncodingProblem.Encoding.Name, easy4SimRealEncodingProblem.Encoding.Length);
                encoding.Bounds = easy4SimRealEncodingProblem.Encoding.Bounds;
                Encoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                RealVectorEncoding encoding = new RealVectorEncoding(easy4SimRealEncodingProblem.Encoding.Name, easy4SimRealEncodingProblem.Encoding.Length);
                encoding.Bounds = easy4SimRealEncodingProblem.Encoding.Bounds;
                RealEncoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            RealInformation = new List<ParameterArrayOptimization<double>>();

            for (int i = 0; i < easy4SimRealEncodingProblem.RealInformation.Count; i++)
            {
                ParameterArrayOptimization<double> info = easy4SimRealEncodingProblem.RealInformation[i].Clone();
                info.BaseObject = (CSimBase)Solver.SimulationObjects.GetSimulationObjectByName(easy4SimRealEncodingProblem.RealInformation[i].BaseObject.Name);
                info.PropertyInfo = info.BaseObject.GetType().GetProperties().FirstOrDefault(x =>
                    x.Name.Contains(easy4SimRealEncodingProblem.RealInformation[i].PropertyInfo.Name));
                RealInformation.Add(info);
            }
            OnOperatorsChanged();

        }

    }
}
