using Easy4SimFramework;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using FjspEasy4SimLibrary;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessTree;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [StorableType("E824D82A-D8C4-43D1-A58B-8D28D7DE66F7")]
    [Creatable(CreatableAttribute.Categories.ExternalEvaluationProblems, Priority = 150)]
    [Item("Easy4Sim FJSSP", "Parameter optimization for a FJSSP Easy4Sim problem")]
    public class Easy4SimFjsspEncoding : SingleObjectiveBasicProblem<IntegerVectorEncoding>
    {
        #region Const strings for naming
        private const string Easy4SimParameterName = "Easy4SimFile";

        private const string BestSolutionName = "BestSolutionEasy4Sim";

        private const string CobotLocationsName = "CobotLocations";
        private const string BestSolutionQualityName = "BestSolutionQualityEasy4Sim";
        private const string ReplayInfo = "ReplayInfo";
        private const string MultiObjectiveName = "MultiObjective optimization";
        private const string MiningInformationName = "MiningInformation";
        private const string MiningInformationName2 = "MiningInformationResource";

        private const string BestSolutionNameDescription = "Best solution found so far";
        private const string BestSolutionQualityDescription = "Best solution quality found so far";
        private const string CobotLocationsNameDescriptions = "Cobot locations";
        private const string MiningInformationNameDescriptions = "MiningInformation";
        private const string MiningInformationNameDescriptions2 = "MiningInformation resource";
        private const string ReplayInfoDescription = "ReplayInfo to save in file";
        private const string MultiObjectiveDescription = "Use multiobjective function?";
        #endregion

        private string FileName { get; set; }
        public static long EvaluatedSolutions { get; set; }
        public static long VnsRuns { get; set; }
        public static long VnsImprovementsFound { get; set; }
        public static long EvaluatedVnsSolutions { get; set; }

        public static int MinimumVnsNeighborhoodSize { get; set; } = 300;

        private bool FirstRun = true;

        public double TimeFactor = 1;
        public double CostFactor = 1;
        public double AmountOfCobots { get; set; }

        public static GeneticAlgorithm Algorithm { get; set; }
        public HeuristicLab.Random.MersenneTwister Twister { get; set; }

        public static double BestSolutionSoFarGa = double.MaxValue;
        public static bool ApplyVNS = false;
        public static int VnsNeighborhood { get; set; }
        public static double VnsPercentage { get; set; }
        public static bool UseNormalizedValue = true;

        private int LpmOrder = -1;
        private int WorkstationAmount = 0;
        private IntermediateLocalProcessModelResult MinedResult = null;
        private Dictionary<int, int> MinedWorkstations = new Dictionary<int, int>();
        private List<IntermediateLocalProcessModelResult> MiningResultList = new List<IntermediateLocalProcessModelResult>();

        private int GenerationCounter = 0;
        private bool CheckHammingDistance = true;
        private List<IntegerVector> SolutionsInCurrentGeneration = new List<IntegerVector>();
        private List<IntegerVector> AlreadyMinedSolutions = new List<IntegerVector>();

        private PriorityRuleSolutionGenerator generator;


        public override bool Maximization { get; }
        //############## Settings of the solver ####################
        [Storable]
        public SolverSettings SolverSettings { get; set; }
        //############## Solver that is cloned for the simulation #################################
        [Storable]
        public Solver Solver { get; set; }


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
        //############### ReplayInfo ####################################
        [Storable]
        public ValueLookupParameter<StringValue> ReplayInformation
        {
            get => (ValueLookupParameter<StringValue>)Parameters[ReplayInfo];
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
        public IntegerVectorEncoding IntegerEncoding { get; set; }

        [Storable]
        public List<ParameterArrayOptimization<int>> IntegerInformation { get; set; }

        /// <summary>
        /// Important to set the EndTime
        /// The evaluation will stop if the EndTime is exceeded
        /// </summary>
        public static DateTime EndTime { get; set; }



        [StorableConstructor]
        public Easy4SimFjsspEncoding(StorableConstructorFlag _) : base(_) { }
        public Easy4SimFjsspEncoding(bool maximization = false, double amountOfCobots = 0, Random.MersenneTwister twister = null)
        {
            //##################### Initialize the parameters that are seen in HeuristicLab #####################################################
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue());
            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription, new StringValue(""));
            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription, new TextValue(""));
            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions, new TextValue(""));
            ReplayInformation = new ValueLookupParameter<StringValue>(ReplayInfo, ReplayInfoDescription, new TextValue(""));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription, new BoolValue(true));
            MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions, new TextValue(""));
            MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2, new TextValue(""));
            //##################### Initialize the encoding #####################################################################################
            IntegerEncoding = new IntegerVectorEncoding();
            Encoding = IntegerEncoding;
            IntegerInformation = new List<ParameterArrayOptimization<int>>();

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
            generator = new PriorityRuleSolutionGenerator();
            SolutionCreator = generator;

        }
        private void FileValueChanged(object sender, EventArgs e)
        {
            FileValueChanged(sender);
        }
        private void FileValueChanged(object sender)
        {

            PriorityRuleSolutionGenerator.ProblemFile = Easy4SimEvaluationScript.ToString();
            PriorityRuleSolutionGenerator.AmountOfCobots = AmountOfCobots;

            SimulationObjects simulationObjects = new SimulationObjects();
            Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

            Logger logger = null; //set log path

            SolverSettings settings = new SolverSettings(environment, simulationObjects, logger);

            DataStore.FileLoader loader = new DataStore.FileLoader();
            string FileContent = loader.LoadFile(Easy4SimEvaluationScript.ToString());

            FjspLoader fJSSP_Loader = new FjspLoader(0, "FJSSP_Loader1", settings);
            fJSSP_Loader.FileContent = new ParameterString(FileContent);

            FjspEvaluation evaluation = new FjspEvaluation(1, "FJSSP_Evaluation1", settings);
            evaluation.ReadData.Connect(fJSSP_Loader.ReadData);



            Link link = new Link();
            link.OutputComponentName = "FJSSP_Loader1";
            link.InputComponentName = "FJSSP_Evaluation1";
            link.OutputConnectorName = "ReadData";
            link.InputConnectorName = "ReadData";

            simulationObjects.AddLink(link);


            SolverSettings = settings;
            Solver = new Solver(settings);
            Solver.SimulationStatistics.AmountOfCobotsPercent = AmountOfCobots;
            Solver.Init();


            //################################ Set maximization to false ########################################################################
            Parameters.Remove("Maximization");
            Parameters.Add(new ValueParameter<BoolValue>("Maximization", new BoolValue(Maximization)));



            //################################ Initialize Encoding         #######################################################################
            //################################ Set bounds for optimization #######################################################################
            IntegerInformation.Clear();


            //intCounter = 0;
            int intCounter = 0;
            List<int> lowerBounds = new List<int>();
            List<int> upperBounds = new List<int>();
            foreach (CSimBase cSimBase in Solver.SimulationObjects.SimulationList.Values)
            {
                foreach (PropertyInfo info in cSimBase.GetType().GetProperties())
                {
                    foreach (Attribute attribute in info.GetCustomAttributes())
                    {
                        if (attribute.GetType().Name == "Optimization")
                        {
                            var value = info.GetValue(cSimBase);
                            if (value is ParameterArrayOptimization<int> parameterIntegerOptimization)
                            {
                                intCounter += parameterIntegerOptimization.Value.Length;
                                lowerBounds.AddRange(parameterIntegerOptimization.LowerBounds);
                                upperBounds.AddRange(parameterIntegerOptimization.UpperBounds);
                                ParameterArrayOptimization<int> temp = parameterIntegerOptimization.Clone();
                                temp.BaseObject = cSimBase;
                                temp.PropertyInfo = info;
                                IntegerInformation.Add(temp);
                            }
                        }
                    }
                }
            }
            IntegerEncoding.Length = intCounter;
            IntegerEncoding.Bounds = new IntMatrix(intCounter, 2);
            for (int i = 0; i < intCounter; i++)
            {
                IntegerEncoding.Bounds[i, 0] = lowerBounds[i];
                IntegerEncoding.Bounds[i, 1] = upperBounds[i];
            }

            OnOperatorsChanged();
        }



        public override double Evaluate(Individual individual, IRandom random)
        {
            try
            {
                Solver s = (Solver)Solver.Clone();
                WorkstationAmount = s.SimulationStatistics.AmountOfWorkstations;


                if (individual?.Values == null) return -2;

                IntegerVector integerVector = null;
                foreach (KeyValuePair<string, IItem> pair in individual.Values)
                {
                    if (pair.Value is IntegerVector intVector)
                        integerVector = intVector;
                }

                if (integerVector == null)
                    return 0;


                int skipCounter = 0;
                bool valuesSet = false;
                foreach (ParameterArrayOptimization<int> information in IntegerInformation)
                {
                    if (information.Amount == 0)
                        continue;
                    SimulationPropertySetter.SetParameterOptimizationInSimulation(s, information,
                        integerVector.Skip(skipCounter).Take(information.Amount));
                    skipCounter += information.Amount;
                    valuesSet = true;
                }

                if (!valuesSet)
                    return 0;



                s.RunFinishEventOnly();

                double currentRunFitness = s.SimulationStatistics.Fitness;


                //if (currentRunFitness < BestSolutionSoFarGa || BestSolutionSoFarGa == double.MaxValue)
                //    BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                int maxK = MinimumVnsNeighborhoodSize / 50;

                bool enteredVns = false;
                try
                {
                    if (ApplyVNS && EvaluatedSolutions > 100)
                    {
                        //Check if the current solution is within 10% of the best solution found so far
                        bool vns = currentRunFitness <= BestSolutionSoFarGa * (1 + VnsPercentage);
                        if (vns)
                        {
                            if (!enteredVns)
                            {
                                enteredVns = true;
                                VnsRuns++;

                            }
                            int i = 0;
                            int k = 0;  
                            while (k < maxK && DateTime.Now < EndTime)
                            {
                                EvaluatedVnsSolutions++;
                                Solver s2 = (Solver)Solver.Clone();
                                IntegerVector intVector2 = new IntegerVector(integerVector);
                                int amountOfChanges = GenericProblemMethods.SetAmountOfChanges(k);

                                //IntegerInformation => List<ParameterArrayOptimization<int>> 
                                //IntegerEncoding => IntegerVectorEncoding
                                switch (VnsNeighborhood)
                                {
                                    case 0:
                                        if (FirstRun)
                                        {
                                            Solver.SimulationStatistics.VnsNeighborhood = $"Basic change";
                                            FirstRun = false;
                                        }
                                        IntegerEncodingNeighborhood.BasicChange(amountOfChanges, Twister, IntegerInformation, IntegerEncoding, intVector2);
                                        break;
                                    case 1:
                                        if (FirstRun)
                                        {
                                            Solver.SimulationStatistics.VnsNeighborhood = $"Greedy change";
                                            FirstRun = false;
                                        }
                                        IntegerEncodingNeighborhood.GreedyChange(amountOfChanges, Twister, IntegerInformation, IntegerEncoding, intVector2, s2);
                                        break;
                                    case 2:
                                        if (FirstRun)
                                        {
                                            Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change V1";
                                            FirstRun = false;
                                        }


                                        if (MinedResult == null)
                                        {
                                            k = maxK;
                                            continue;
                                        }
                                        IntegerEncodingNeighborhood.ProcessMiningChange(amountOfChanges, Twister, MinedResult, IntegerInformation, IntegerEncoding, intVector2, s2);
                                        break;
                                    //case 3:
                                    //    if (FirstRun)
                                    //    {
                                    //        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change V2";
                                    //        FirstRun = false;
                                    //    }


                                    //    if (MinedResult == null)
                                    //    {
                                    //        k = maxK;
                                    //        continue;
                                    //    }
                                    //    IntegerEncodingNeighborhood.ProcessMiningDictionaryChange(amountOfChanges, Twister, MinedResult, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    //    break;
                                    case 3:

                                        if (FirstRun)
                                        {
                                            Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change Dictionary V1";
                                            FirstRun = false;
                                        }
                                        if (MinedWorkstations.Count == 0)
                                        {
                                            k = maxK;
                                            continue;
                                        }
                                        IntegerEncodingNeighborhood.ProcessMiningChangeDictionaryV1(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);
                                        break;
                                    //case 5:
                                    //    if (FirstRun)
                                    //    {
                                    //        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change Dictionary V2";
                                    //        FirstRun = false;
                                    //    }

                                    //    if (MinedWorkstations.Count == 0)
                                    //    {
                                    //        k = maxK;
                                    //        continue;
                                    //    }
                                    //    IntegerEncodingNeighborhood.ProcessMiningChangeDictionaryV2(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    //    break;

                                    case 4:
                                        if (FirstRun)
                                        {
                                            Solver.SimulationStatistics.VnsNeighborhood = $"Process mining experiments V1";
                                            FirstRun = false;
                                        }


                                        if (MinedResult == null)
                                        {
                                            k = maxK;
                                            continue;
                                        }
                                        IntegerEncodingNeighborhood.ProcessMiningExperiment1(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);

                                        break;
                                    //case 7:
                                    //    if (FirstRun)
                                    //    {
                                    //        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining experiments V2 - more mining";
                                    //        FirstRun = false;
                                    //    }


                                    //    if (MinedResult == null)
                                    //    {
                                    //        k = maxK;
                                    //        continue;
                                    //    }
                                    //    IntegerEncodingNeighborhood.ProcessMiningExperiment1(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);

                                    //    break;
                                    //case 8:
                                    //    if (FirstRun)
                                    //    {
                                    //        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change Dictionary V3 - more mining";
                                    //        FirstRun = false;
                                    //    }

                                    //    if (MinedWorkstations.Count == 0)
                                    //    {
                                    //        k = maxK;
                                    //        continue;
                                    //    }
                                    //    IntegerEncodingNeighborhood.ProcessMiningChangeDictionaryV2(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    //    break;

                                    default:
                                        Console.WriteLine("Neighborhood operator out of range");
                                        throw new ArgumentOutOfRangeException();
                                        break;
                                }


                                int skipCounter2 = 0;
                                bool valuesSet2 = false;
                                foreach (ParameterArrayOptimization<int> information in IntegerInformation)
                                {
                                    if (information.Amount == 0)
                                        continue;

                                    //Apply the changes to the clone solver
                                    SimulationPropertySetter.SetParameterOptimizationInSimulation(s2, information,
                                        intVector2.Skip(skipCounter2).Take(information.Amount));


                                    skipCounter2 += information.Amount;
                                    valuesSet2 = true;
                                }

                                if (!valuesSet2)
                                    break;

                                s2.RunFinishEventOnly();

                                double currentRunFitness2 = s2.SimulationStatistics.Fitness;

                                if (currentRunFitness2 == 0)
                                {
                                    Console.WriteLine("fitness should not be 0");
                                }

                                if (currentRunFitness2 < currentRunFitness)
                                {
                                    integerVector = new IntegerVector(intVector2);
                                    currentRunFitness = currentRunFitness2;

                                    VnsImprovementsFound++;
                                    //Restart vns if a improvement has been found
                                    s = s2;
                                    i = 0;
                                    k = 0;
                                    continue;
                                }
                                //Continue with next iteration
                                //After 50 iterations adept the amount of changes
                                i++;
                                if (i == 50)
                                {
                                    i = 0;
                                    k++;
                                }

                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                    throw;
                }

                EvaluatedSolutions++;

                //Check hamming distance to all other already evaluated solutions
                //If a duplicate gets detected replace it with a random initialized solution
                CheckHammingDistanceMethod(ref integerVector, ref currentRunFitness);


                //Apply changes to the scope in HL
                Individual.SetScopeValue("IntegerVector", individual.Scope, integerVector);


                if (BestQuality.Value == null || string.IsNullOrWhiteSpace(BestQuality.Value.Value) ||
                    currentRunFitness < BestSolutionSoFarGa)
                {
                    UpdateBestSolution(currentRunFitness, integerVector);
                    foreach (CSimBase simBase in s.SimulationObjects.SimulationList.Values)
                    {
                        var properties = simBase.GetType().GetProperties();

                        PropertyInfo infoGantt = properties.FirstOrDefault(x => x.Name.Contains("GanttInformationCsv"));

                        if (infoGantt != null)
                        {
                            string gantFileContent = infoGantt.GetValue(simBase).ToString();
                            ReplayInformation = new ValueLookupParameter<StringValue>(ReplayInfo, ReplayInfoDescription, new StringValue(gantFileContent.ToString(CultureInfo.InvariantCulture)));

                        
                        }

                    }


                    bool minedAlready = false;
                    if (VnsNeighborhood > 1)
                    {
                        foreach (IntegerVector minedSolution in AlreadyMinedSolutions)
                        {
                            if (HammingSimilarityCalculator.CalculateSimilarity(minedSolution, integerVector) == 0)
                            {
                                minedAlready = true;
                                break;
                            }
                        }

                        if (!minedAlready)
                        {
                            if (LpmOrder == -1)
                                LpmOrder = Math.Max(IntegerInformation[2].Amount, 2);
                            StringBuilder stringBuilderMiningInformation = new StringBuilder();
                            UpdateMiningInformation(s, stringBuilderMiningInformation);
                            LpmMining(currentRunFitness, stringBuilderMiningInformation);
                            AlreadyMinedSolutions.Add(integerVector);
                        }
                    }
                } else if (currentRunFitness < BestSolutionSoFarGa * 1.05 && VnsNeighborhood >= 7)
                {
                    bool minedAlready = false;
                    foreach (IntegerVector minedSolution in AlreadyMinedSolutions)
                    {
                        if (HammingSimilarityCalculator.CalculateSimilarity(minedSolution, integerVector) == 0)
                        {
                            minedAlready = true;
                            break;
                        }
                    }

                    if (!minedAlready)
                    {
                        if (LpmOrder == -1)
                            LpmOrder = Math.Max(IntegerInformation[2].Amount, 2);
                        
                        StringBuilder stringBuilderMiningInformation = new StringBuilder();
                        UpdateMiningInformation(s, stringBuilderMiningInformation);
                        LpmMining(currentRunFitness, stringBuilderMiningInformation);
                        AlreadyMinedSolutions.Add(integerVector);
                    }
                }

                if (string.IsNullOrEmpty(FileName))
                    SetFileName();


                string[] outputDirectoryParts = new string[] { Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..", "Results", "RunFjssp" };

                string directoryOutput = Path.Combine(outputDirectoryParts);
                if (!Directory.Exists(directoryOutput))
                    Directory.CreateDirectory(directoryOutput);


                return currentRunFitness;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                Console.ReadLine();
                throw;
            }
        }

        private void SetFileName()
        {
            Guid id = Guid.NewGuid();
            FileName = id.ToString();
        }

        private void UpdateBestSolution(double currentRunFitness, IntegerVector integerVector)
        {
            if (currentRunFitness < BestSolutionSoFarGa)
                BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);

            Console.WriteLine("Set best fitness to " + currentRunFitness);
            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName,
                BestSolutionQualityDescription,
                new StringValue(currentRunFitness.ToString(CultureInfo.InvariantCulture)));
            

            if (currentRunFitness == 0)
            {
                Console.WriteLine("fitness should not be 0");
            }

            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription,
                new TextValue(string.Join(";", integerVector)));
        }

        private void UpdateMiningInformation(Solver solver, StringBuilder stringBuilderMiningInformation)
        {
            FjspEvaluation eval = null;
            foreach (CSimBase simBase in solver.SimulationObjects.SimulationList.Values)
            {
                if (simBase is FjspEvaluation e)
                {
                    eval = e;
                    break;
                }
            }


            if (stringBuilderMiningInformation.ToString() != "")
                stringBuilderMiningInformation.Append("======================================");

            stringBuilderMiningInformation.Append(new TextValue(eval.ProcessMiningInformation));

        }

        private void LpmMining(double currentRunFitness, StringBuilder stringBuilderMiningInformation)
        {
            //Whenever a new best solution is found a model is mined
            bool localProcessModels = currentRunFitness <= BestSolutionSoFarGa;

            if (MinedWorkstations.Count == 0)
            {
                for (int i = 1; i <= WorkstationAmount; i++)
                {
                    MinedWorkstations.Add(i, 1);
                }
            }



            if (localProcessModels && !string.IsNullOrWhiteSpace(stringBuilderMiningInformation.ToString()))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(stringBuilderMiningInformation.ToString());
                    MineLocalProcessModel miner = new MineLocalProcessModel(doc, "org:resource", "Job");
                    IntermediateLocalProcessModelResult result = miner.MineRandomLocalProcessModel(false, lpmSize: LpmOrder);
                    MinedResult = result;
                    foreach (ProcessTree leaf in MinedResult.CurrentProcessTree.GetLeaves())
                    {
                        int leafValue = Convert.ToInt32(leaf.Label);
                        MinedWorkstations[leafValue] += 1;
                    }
                    MiningResultList.Add(result);
                    Console.WriteLine("Mined new local process models");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                    throw e;
                }
            }
        }

        private void CheckHammingDistanceMethod(ref IntegerVector integerVector, ref double currentRunFitness)
        {
            if (CheckHammingDistance)
            {
                foreach (IntegerVector solution in SolutionsInCurrentGeneration)
                {
                    double distance = HammingSimilarityCalculator.CalculateSimilarity(solution, integerVector);

                    if (distance == 1)
                    {
                        for (int i = 0; i < integerVector.Count(); i++)
                        {
                            integerVector[i] = Twister.Next(IntegerEncoding.Bounds[i, 1]);
                        }
                        Solver s2 = (Solver)Solver.Clone();

                        int skipCounter2 = 0;
                        foreach (ParameterArrayOptimization<int> information in IntegerInformation)
                        {
                            if (information.Amount == 0)
                                continue;

                            //Apply the changes to the clone solver
                            SimulationPropertySetter.SetParameterOptimizationInSimulation(s2, information,
                                integerVector.Skip(skipCounter2).Take(information.Amount));


                            skipCounter2 += information.Amount;
                        }


                        s2.RunFinishEventOnly();
                        currentRunFitness = s2.SimulationStatistics.Fitness;
                        break;
                    }
                }


                SolutionsInCurrentGeneration.Add(integerVector);


                if (EvaluatedSolutions % Algorithm.PopulationSize.Value == 0)
                {
                    SolutionsInCurrentGeneration.Clear();
                    GenerationCounter++;
                }

            }
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimFjsspEncoding(this, cloner);
        }
        protected Easy4SimFjsspEncoding(Easy4SimFjsspEncoding easy4SimFjsspEncoding, Cloner cloner) : base(easy4SimFjsspEncoding, cloner)
        {
            if (easy4SimFjsspEncoding == null) return;
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue(easy4SimFjsspEncoding.Easy4SimParameter.Value.ToString()));

            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription,
                new StringValue(easy4SimFjsspEncoding.BestQuality.Value.Value));

            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription,
                new TextValue(easy4SimFjsspEncoding.BestSolution.Value.Value));
            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions,
                new TextValue(easy4SimFjsspEncoding.CobotLocations.Value.Value));
            ReplayInformation = new ValueLookupParameter<StringValue>(ReplayInfo, ReplayInfoDescription,
                new TextValue(easy4SimFjsspEncoding.ReplayInformation.Value.Value));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription,
                new BoolValue(easy4SimFjsspEncoding.MultiObjective.Value.Value));


            Maximization = easy4SimFjsspEncoding.Maximization;
            ((ISingleObjectiveHeuristicOptimizationProblem)this).MaximizationParameter.Hidden = false;

            SolverSettings = easy4SimFjsspEncoding.SolverSettings?.Clone();
            Solver = new Solver(SolverSettings);




            BestKnownQuality = easy4SimFjsspEncoding.BestKnownQuality;
            try
            {
                IntegerVectorEncoding encoding = new IntegerVectorEncoding(easy4SimFjsspEncoding.Encoding.Name, easy4SimFjsspEncoding.Encoding.Length);
                encoding.Bounds = easy4SimFjsspEncoding.Encoding.Bounds;
                Encoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }

            try
            {
                IntegerVectorEncoding encoding = new IntegerVectorEncoding(easy4SimFjsspEncoding.Encoding.Name, easy4SimFjsspEncoding.Encoding.Length);
                encoding.Bounds = easy4SimFjsspEncoding.Encoding.Bounds;
                IntegerEncoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }


            IntegerInformation = new List<ParameterArrayOptimization<int>>();

            for (int i = 0; i < easy4SimFjsspEncoding.IntegerInformation.Count; i++)
            {
                ParameterArrayOptimization<int> info = easy4SimFjsspEncoding.IntegerInformation[i].Clone();
                info.BaseObject = (CSimBase)Solver.SimulationObjects.GetSimulationObjectByName(easy4SimFjsspEncoding.IntegerInformation[i].BaseObject.Name);
                info.PropertyInfo = info.BaseObject.GetType().GetProperties().FirstOrDefault(x =>
                    x.Name.Contains(easy4SimFjsspEncoding.IntegerInformation[i].PropertyInfo.Name));
                IntegerInformation.Add(info);
            }
            this.OnOperatorsChanged();

        }
    }
}

