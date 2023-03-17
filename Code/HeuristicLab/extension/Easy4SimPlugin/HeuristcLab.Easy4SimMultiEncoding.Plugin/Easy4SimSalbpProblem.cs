using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CobotAssignmentAndJobShopSchedulingProblem;
using DataStore;
using Easy4SimFramework;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.RealVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses;
using DoubleMatrix = HeuristicLab.Data.DoubleMatrix;

// ReSharper disable UnusedMember.Global

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [StorableType("AA8D6121-2A71-4ED9-B296-A8EBECA1B85A")]
    [Creatable(CreatableAttribute.Categories.ExternalEvaluationProblems, Priority = 111)]
    [Item("Easy4Sim SALBP problem", "Parameter optimization for a RealEncoding Easy4Sim problem")]
    public class Easy4SimSalbpProblem : SingleObjectiveBasicProblem<RealVectorEncoding>
    {
        #region Const strings for naming
        private const string Easy4SimParameterName = "Easy4SimFile";
        private const string Easy4SimSolutionName = "Easy4SimSolution";

        private const string BestSolutionName = "BestSolutionEasy4Sim";
        private const string CobotLocationsName = "CobotLocations";
        private const string BestSolutionQualityName = "BestSolutionQualityEasy4Sim";
        private const string ReplayInfo = "ReplayInfo";
        private const string Information = "Information";
        private const string Information2 = "Information2";
        private const string MultiObjectiveName = "MultiObjective optimization";

        private const string BestSolutionNameDescription = "Best solution found so far";
        private const string BestSolutionQualityDescription = "Best solution quality found so far";
        private const string CobotLocationsNameDescriptions = "Cobot locations";
        private const string ReplayInfoDescription = "ReplayInfoDescription";
        private const string InformationDescription = "InfoDescription";
        private const string Information2Description = "TaskInformation";
        private const string MultiObjectiveDescription = "Use multiobjective function?";
        #endregion

        public double TimeFactor = 5;

        //Maximize or minimize the problem
        public override bool Maximization { get; }

        //############## Settings of the solver ####################
        [Storable]
        public SolverSettings SolverSettings { get; set; }
        //############## Solver that is cloned for the simulation #################################
        [Storable]
        public Solver Solver { get; set; }
        public List<long> RunTimes { get; set; }

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

        //############### Solution for the simulation ####################################
        [Storable]
        public Easy4SimFileValue Easy4SimGivenProblemSolutions
        {
            get => Easy4SimSolution.Value;
            set => Easy4SimSolution = new ValueLookupParameter<Easy4SimFileValue>(value.Value,
                "Path to the solution of the file.", new Easy4SimFileValue());
        }

        [Storable]
        public ValueLookupParameter<Easy4SimFileValue> Easy4SimSolution
        {
            get => (ValueLookupParameter<Easy4SimFileValue>)Parameters[Easy4SimSolutionName];
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

        //############### Information ####################################
        [Storable]
        public ValueLookupParameter<TextValue> InformationValue
        {
            get => (ValueLookupParameter<TextValue>)Parameters[Information];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }
        [Storable]
        public ValueLookupParameter<TextValue> Information2Value
        {
            get => (ValueLookupParameter<TextValue>)Parameters[Information2];
            set
            {
                if (Parameters.ContainsKey(value.Name))
                    Parameters.Remove(value.Name);
                Parameters.Add(value);
            }
        }

        public string InformationAsString
        {
            get
            {
                return InformationValue.Value.Value;
            }
        }
        public string Information2AsString
        {
            get
            {
                return Information2Value.Value.Value;
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

        [Storable]
        public string FileToOptimize { get; set; }
        [Storable]
        public int Optimum { get; set; }
        [Storable]
        public bool OptimumFound { get; set; }

        [StorableConstructor]
        public Easy4SimSalbpProblem(StorableConstructorFlag _) : base(_) { }
        public Easy4SimSalbpProblem()
        {
            //##################### Initialize the parameters that are seen in HeuristicLab #####################################################
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName, "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());
            Easy4SimSolution = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimSolutionName, "The path to the solution for the problem.", new Easy4SimFileValue());
            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription, new StringValue(""));
            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription, new TextValue(""));
            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions, new TextValue(""));
            ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription, new TextValue(""));
            InformationValue = new ValueLookupParameter<TextValue>(Information, InformationDescription, new TextValue(""));
            Information2Value = new ValueLookupParameter<TextValue>(Information2, InformationDescription, new TextValue(""));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription, new BoolValue(true));
            //##################### Initialize the encoding #####################################################################################
            RealEncoding = new RealVectorEncoding();
            Encoding = RealEncoding;
            RealInformation = new List<ParameterArrayOptimization<double>>();

            //##################### File value and file value changed ############################################################################
            Easy4SimEvaluationScript.FileDialogFilter = @"Easy4Sim Excel files|*.xlsx";

            Easy4SimGivenProblemSolutions.FileDialogFilter = @"CSV|*.csv";

            //##################### General parameters ############################################################################
            Maximization = true;
            BestKnownQuality = 0;
            RunTimes = new List<long>();

        }


        protected Easy4SimSalbpProblem(Easy4SimSalbpProblem easy4SimSalbpProblem, Cloner cloner)
        {
            if (easy4SimSalbpProblem == null) return;
            SolverSettings = easy4SimSalbpProblem.SolverSettings?.Clone();
            Solver = (Solver)easy4SimSalbpProblem.Solver?.Clone();
            Easy4SimParameter = cloner.Clone(easy4SimSalbpProblem.Easy4SimParameter);
            BestQuality = cloner.Clone(easy4SimSalbpProblem.BestQuality);
            BestSolution = cloner.Clone(easy4SimSalbpProblem.BestSolution);
            ReplayInformation = cloner.Clone(easy4SimSalbpProblem.ReplayInformation);
            InformationValue = cloner.Clone(easy4SimSalbpProblem.InformationValue);
            Information2Value = cloner.Clone(easy4SimSalbpProblem.Information2Value);
            MultiObjective = cloner.Clone(easy4SimSalbpProblem.MultiObjective);

            BestKnownQuality = easy4SimSalbpProblem.BestKnownQuality;
            Encoding = cloner.Clone(easy4SimSalbpProblem.Encoding);

            RealEncoding = cloner.Clone(easy4SimSalbpProblem.RealEncoding);
            RealInformation = new List<ParameterArrayOptimization<double>>();
            foreach (ParameterArrayOptimization<double> information in easy4SimSalbpProblem.RealInformation)
                RealInformation.Add(information.Clone());


            Easy4SimEvaluationScript = cloner.Clone(easy4SimSalbpProblem.Easy4SimEvaluationScript);
            Easy4SimEvaluationScript.FileDialogFilter = @"Easy4Sim Excel files|*.xlsx";
            Easy4SimEvaluationScript.ToStringChanged += FileValueChanged;


            Easy4SimGivenProblemSolutions = cloner.Clone(easy4SimSalbpProblem.Easy4SimGivenProblemSolutions);
            TimeFactor = easy4SimSalbpProblem.TimeFactor;
            Maximization = easy4SimSalbpProblem.Maximization;
        }


    

        private void FileValueChanged(object sender, EventArgs e)
        {
            FileValueChanged(sender);
        }

        public void FileValueChanged(object sender)
        {
            try
            {
                //############################### Initialize Solver from file #######################################################################
                if (sender is Easy4SimFileValue fileValue)
                {
                    SimulationObjects simulationObjects = new SimulationObjects();
                    Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

                    Logger logger = null; //set log path

                    SolverSettings settings = new SolverSettings(environment, simulationObjects, logger);

                    DataStore.FileLoader loader = new DataStore.FileLoader();
                    string fileContent = loader.LoadFile(fileValue.StringValue.Value.ToString());

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
                    Solver.Init();
                }

                //################################ Set maximization to false ########################################################################
                Parameters.Remove("Maximization");
                Parameters.Add(new ValueParameter<BoolValue>("Maximization", new BoolValue(false)));

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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);
                Console.WriteLine(" FileValueChanged(object sender)");
            }
        }


        public void FileValueChanged(object sender, SALBP_WeckenborgData data)
        {
            try
            {
                //############################### Initialize Solver from file #######################################################################
                if (sender is Easy4SimFileValue fileValue)
                {
                    SimulationObjects simulationObjects = new SimulationObjects();
                    Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

                    Logger logger = null; //set log path

                    SolverSettings settings = new SolverSettings(environment, simulationObjects, logger);

                    DataStore.FileLoader loader = new DataStore.FileLoader();
                    string fileContent = loader.LoadFile(fileValue.StringValue.Value.ToString());

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
                    Solver.Init();
                }

                //################################ Set maximization to false ########################################################################
                Parameters.Remove("Maximization");
                Parameters.Add(new ValueParameter<BoolValue>("Maximization", new BoolValue(false)));

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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);
                Console.WriteLine(" FileValueChanged(object sender)");
            }
        }


        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimSalbpProblem(this, cloner);
        }
        public override double Evaluate(Individual individual, IRandom random)
        {
            Solver solver = (Solver)Solver.Clone();

            if (solver.SolverSettings == null) return -1;

            solver.Logger = null; //deactivate logger for first tests

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
                SimulationPropertySetter.SetParameterOptimizationInSimulation(solver, information, realVector.Skip(skipCounter).Take(information.Amount));

                skipCounter += information.Amount;
                valuesSet = true;
            }

         
            if (!valuesSet)
                return 0;



            //######### Run the simulation ###############
            solver.RunFinishEventOnly();
            //============ Set changes  ==================

            List<double> newValues = new List<double>();
            foreach (ParameterArrayOptimization<double> information in RealInformation)
            {
                var baseObject = solver.SolverSettings.SimulationObjects.SimulationList.Values.FirstOrDefault();
                var value = information.PropertyInfo.GetMethod.Invoke(baseObject, new object[]{});
                if (value is ParameterArrayOptimization<double> parameterOptimization)
                {
                    foreach (double d in parameterOptimization.Value)
                        newValues.Add(d);
                }
            }

            for (int i = 0; i < newValues.Count; i++)
                realVector[i] = newValues[i];
            

            Individual.SetScopeValue("RealVector", individual.Scope, realVector);
            //============================================
            double currentRunFitness = solver.SimulationStatistics.Fitness;
            if (MultiObjective.Value.Value)
                currentRunFitness += TimeFactor * solver.Environment.SimulationTime;

            long runtime = solver.SimulationStatistics.ExecutionTime;
            RunTimes.Add(runtime);
            if (BestQuality.Value == null || string.IsNullOrWhiteSpace(BestQuality.Value.Value) || Convert.ToDouble(BestQuality.Value.Value, CultureInfo.InvariantCulture) > currentRunFitness)
            {
                BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription, new StringValue(currentRunFitness.ToString(CultureInfo.InvariantCulture)));
                BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription, new TextValue(string.Join(";", realVector)));

                foreach (CSimBase simBase in solver.SimulationObjects.SimulationList.Values)
                {
                    var properties = simBase.GetType().GetProperties();
                    PropertyInfo info = properties.FirstOrDefault(x => x.Name.Contains("CobotInformation"));
                    if (info != null)
                        CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions, new TextValue(info.GetValue(simBase).ToString()));
                    PropertyInfo info2 = properties.FirstOrDefault(x => x.Name.Contains("WorkStepInformation"));
                    if (info2 != null)
                        ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription, new TextValue(info2.GetValue(simBase).ToString()));
                    PropertyInfo info3 = properties.FirstOrDefault(x => x.Name.Contains("Information"));
                    if (info3 != null)
                        InformationValue = new ValueLookupParameter<TextValue>(Information, InformationDescription, new TextValue(info3.GetValue(simBase).ToString()));
                    PropertyInfo info4 = properties.FirstOrDefault(x => x.Name.Contains("TaskInformation"));
                    if (info4 != null)
                        Information2Value = new ValueLookupParameter<TextValue>(Information2, Information2Description, new TextValue(info4.GetValue(simBase).ToString()));
                }
            }

            return currentRunFitness;
        }

    }
}
