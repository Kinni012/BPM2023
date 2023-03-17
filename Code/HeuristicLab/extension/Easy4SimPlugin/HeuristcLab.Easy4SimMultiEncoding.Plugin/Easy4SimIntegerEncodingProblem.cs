using System;
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
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessTree;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators;
using Environment = System.Environment;
using IntMatrix = HeuristicLab.Data.IntMatrix;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [StorableType("115EF9CB-73A1-4B7E-9BDD-49208E528A9C")]
    [Creatable(CreatableAttribute.Categories.ExternalEvaluationProblems, Priority = int.MaxValue)]
    [Item("Easy4Sim IntegerEncoding parameter optimization problem", "Parameter optimization for a IntegerEncoding Easy4Sim problem")]
    public class Easy4SimIntegerEncodingProblem : SingleObjectiveBasicProblem<IntegerVectorEncoding>
    {
        #region Const strings for naming
        private const string Easy4SimParameterName = "Easy4SimFile";

        private const string BestSolutionName = "BestSolutionEasy4Sim";

        private const string BestMakeSpanName = "BestMakespan";
        private const string BestCostName = "BestCost";
        private const string BestMakeSpanDescription = "Makespan of the best solution found so far";
        private const string BestCostDescription = "Cost of the ebst solution found so far";
        private const string GanttInfo = "GanttInfo";

        private const string CobotLocationsName = "CobotLocations";
        private const string BestSolutionQualityName = "BestSolutionQualityEasy4Sim";
        private const string ReplayInfo = "ReplayInfo";
        private const string MultiObjectiveName = "MultiObjective optimization";
        private const string MiningInformationName = "MiningInformation";
        private const string MiningInformationName2 = "MiningInformationResource";
        private const string GanttInfoDescription = "Gantt info of the latest run";

        private const string BestSolutionNameDescription = "Best solution found so far";
        private const string BestSolutionQualityDescription = "Best solution quality found so far";
        private const string CobotLocationsNameDescriptions = "Cobot locations";
        private const string MiningInformationNameDescriptions = "MiningInformation";
        private const string MiningInformationNameDescriptions2 = "MiningInformation resource";
        private const string ReplayInfoDescription = "ReplayInfo to save in file";
        private const string MultiObjectiveDescription = "Use multiobjective function?";
        #endregion
        public static int MinedProcessModels { get; set; }
        public static long EvaluatedSolutions { get; set; }
        public static long EvaluatedVnsSolutions { get; set; }
        public static long VnsRuns { get; set; }
        public int WorkstationAmount { get; set; }
        public static List<double> AverageSolutions { get; set; }
        private string FileName { get; set; }
        //Maximize or minimize the problem
        public double TimeFactor = 1;
        public double CostFactor = 1;
        public double bestCost = 0;
        public double bestTime = 0;

        public int AmountOfCobots = 0;
        public override bool Maximization { get; }
        public static double BestSolutionSoFarGa = double.MaxValue;
        public static bool ApplyVNS = false;
        public static double VnsIntelligent { get; set; }
        public static double VnsPercentage { get; set; }
        public static int VnsNeighborhoodOperator { get; set; }

        public static bool UseNormalizedValue = true;
        public static int AmountOfStochasticSolvers { get; set; } = 5;
        public static int MinimumVnsNeighborhoodSize { get; set; } = 150;

        public Random.MersenneTwister Twister { get; private set; }

        public Dictionary<int, int> MinedWorkstations { get; set; }
        private IntermediateLocalProcessModelResult MinedResult = null;

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
        // ReSharper disable once UnusedMember.Global
        // Necessary for HEAL.Attic
        public Easy4SimIntegerEncodingProblem(StorableConstructorFlag flag) : base(flag) { }
        public Easy4SimIntegerEncodingProblem(bool maximization = false, int amountOfCobots = 0, Random.MersenneTwister twister = null)
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
            ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription, new TextValue(""));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription, new BoolValue(true));
            MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions, new TextValue(""));
            MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2, new TextValue(""));
            //##################### Initialize the encoding #####################################################################################
            IntegerEncoding = new IntegerVectorEncoding();
            Encoding = IntegerEncoding;
            IntegerInformation = new List<ParameterArrayOptimization<int>>();
            AmountOfCobots = amountOfCobots;
            if (twister != null)
                Twister = twister;
            else
                Twister = new Random.MersenneTwister();


            //##################### File value and file value changed ############################################################################
            Easy4SimEvaluationScript.FileDialogFilter = @"Easy4Sim Excel files|*.xlsx";
            Easy4SimEvaluationScript.ToStringChanged += FileValueChanged;


            //##################### General parameters ############################################################################
            Maximization = maximization;
            BestKnownQuality = 0;
            MinedWorkstations = new Dictionary<int, int>();
        }
        private void FileValueChanged(object sender, EventArgs e)
        {
            FileValueChanged(sender);

        }

        private void FileValueChanged(object sender)
        {  //############################### Initialize without file #######################################################################
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
                processor.InitializeForIntOptimization = new ParameterBool(true);
                processor.InitializeForDoubleOptimization = new ParameterBool(false);


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

        public override void Analyze(Individual[] individuals, double[] qualities, ResultCollection results, IRandom random)
        {
            base.Analyze(individuals, qualities, results, random);
        }


        public void GreedyChange(IntegerVector integerVector, int amountOfChanges, Solver solver, List<ParameterArrayOptimization<int>> integerInformation)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                //Generate a random change for the 
                int index = Twister.Next(integerInformation[0].Amount);


                //Changes to workstation should
                var baseObject =
                    solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                        x.Name == integerInformation[0].BaseObject.Name);

                if (baseObject is ProjectProcessor projectProcessor)
                {
                    ConvertedData data = projectProcessor.ConvertedData;
                    List<ConvertedWorkstep> worksteps = data.WorkstepsWhereWorkstationHasToBeDecided;
                    ConvertedWorkstep workstep = worksteps[index];
                    string workstationGroup = workstep.WorkstationGroup;


                    ConvertedWorkstation[] workstations = data.WorkstationsInGroup(workstationGroup);
                    Dictionary<int, bool> workstationsWithCobots = new Dictionary<int, bool>();
                    int counter = 0;
                    foreach (ConvertedWorkstation workstation in workstations)
                    {
                        workstationsWithCobots.Add(workstation.RelativeId, false);
                        counter++;
                    }


                    Dictionary<int, bool> allWorkstations = new Dictionary<int, bool>();
                    for (int j = 0; j < data.Workstations.Count; j++)
                    {
                        allWorkstations.Add(j, false);
                    }

                    int startIndex = integerInformation[0].Amount + integerInformation[1].Amount;
                    int endIndex = integerInformation[0].Amount + integerInformation[1].Amount + integerInformation[2].Amount;
                    for (int j = startIndex; j < endIndex; j++)
                    {
                        int cobotLocation = integerVector[j];
                        List<int> allKeys = allWorkstations.Where(x => x.Value == false).Select(x => x.Key).ToList();
                        allWorkstations[allKeys[cobotLocation]] = true;
                        if (workstationsWithCobots.ContainsKey(allKeys[cobotLocation]))
                        {
                            workstationsWithCobots[allKeys[cobotLocation]] = true;
                        }
                    }


                    int currentValue = integerVector[index];
                    workstationsWithCobots.Remove(workstationsWithCobots.ElementAt(currentValue).Key);

                    if (workstationsWithCobots.Any(x => x.Value))
                    {
                        //Greedy assignment to workstations with cobots
                        List<int> keys = workstationsWithCobots.Where(x => x.Value).Select(x => x.Key).ToList();
                        int keyIndex = 0;
                        if (keys.Count > 1)
                            keyIndex = Twister.Next(keys.Count);
                        integerVector[index] = workstationsWithCobots.Keys.ToList().IndexOf(keys[keyIndex]);

                    }
                    else
                    {
                        //If no cobot is available, assign to a random other workstation
                        List<int> keys = workstationsWithCobots.Select(x => x.Key).ToList();
                        int keyIndex = 0;
                        if (keys.Count > 1)
                            keyIndex = Twister.Next(keys.Count);
                        integerVector[index] = workstationsWithCobots.Keys.ToList().IndexOf(keys[keyIndex]);
                    }
                }
            }
        }


        private void ProcessMiningChange(IntegerVector intVector2, int amountOfChanges, Solver solver,
            List<ParameterArrayOptimization<int>> integerInformation)
        {
            //Get the workstations from the local process mining 
            //We assume that these workstations are hotspots
            List<string> hotspots = MinedResult.CurrentProcessTree.GetLeaves().Select(x => x.Label).ToList();
            List<string> newHotspots = new List<string>();
            foreach (string s in hotspots)
            {
                string[] parts = s.Split();

                newHotspots.Add(parts[0] + " " + parts[1]);
            }


            //Get the project processor object
            var baseObject =
                solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                    x.Name == integerInformation[0].BaseObject.Name);

            if (baseObject == null) return;
            if (baseObject is ProjectProcessor projectProcessor)
            {
                ConvertedData convertedData = projectProcessor.ConvertedData;
                List<ConvertedWorkstep> worksteps = convertedData.WorkstepsWhereWorkstationHasToBeDecided;
                List<ConvertedWorkstation> workstations = convertedData.Workstations;
                //Dictionary of workstation groups and workstations
                Dictionary<string, List<string>> workGroupDictionary = new Dictionary<string, List<string>>();
                foreach (ConvertedWorkstation workstation in workstations)
                {

                    string WorkstationGroup = workstation.WorkstationGroupNumber;
                    string workstationName = workstation.WorkstationNumber;


                    if (!workGroupDictionary.ContainsKey(WorkstationGroup))
                    {
                        workGroupDictionary.Add(WorkstationGroup, new List<string>());
                    }
                    workGroupDictionary[WorkstationGroup].Add(workstationName);
                }

                //Dictionary of workstations in groups with hotspots
                Dictionary<string, List<string>> workGroupDictionary2 = new Dictionary<string, List<string>>();

                foreach (KeyValuePair<string, List<string>> pair in workGroupDictionary)
                {
                    if (pair.Value.Intersect(newHotspots).Any())
                    {
                        workGroupDictionary2.Add(pair.Key, pair.Value);
                    }
                }

                Dictionary<int, string> toChange = new Dictionary<int, string>();


                for (int i = 0; i < worksteps.Count; i++)
                {
                    ConvertedWorkstep workstep = worksteps[i];
                    string WorkstationGroup = workstep.WorkstationGroup;

                    if (workGroupDictionary2.ContainsKey(WorkstationGroup))
                    {
                        toChange.Add(i, WorkstationGroup);
                    }
                }
                for (int i = 0; i < amountOfChanges; i++)
                {
                    List<int> keyList = toChange.Select(x => x.Key).ToList();
                    int changeIndex = keyList.OrderBy(x => Twister.NextDouble()).FirstOrDefault();

                    string toChangeGroup = toChange[changeIndex];

                    int counter = 0;
                    foreach (string workstation in workGroupDictionary2[toChangeGroup])
                    {
                        if (newHotspots.Contains(workstation))
                        {
                            break;
                        }
                        counter++;
                    }

                    intVector2[changeIndex] = counter;

                }
            }
        }

        private void ApplyIntelligentChanges(IntegerVector intVector2, int amountOfChanges, Solver solver, List<ParameterArrayOptimization<int>> integerInformation)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                // Rank based workstation probability
                // The higher the cost times speed factor is, the lower is the rank
                // Lowest possible rank is 1
                // Highest rank depends on the amount of workstations in a group


                int index = Twister.Next(intVector2.Length);
                if (index < integerInformation[0].Amount)
                {
                    //Changes to workstation should
                    var baseObject =
                        solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                            x.Name == integerInformation[0].BaseObject.Name);

                    if (baseObject is ProjectProcessor processor)
                    {
                        ConvertedData convertedData = processor.ConvertedData;
                        List<ConvertedWorkstep> worksteps = convertedData.WorkstepsWhereWorkstationHasToBeDecided;
                        ConvertedWorkstep workstep = worksteps[index];

                        string WorkstationGroup = workstep.WorkstationGroup;
                        ConvertedWorkstation[] workstations = convertedData.WorkstationsInGroup(WorkstationGroup);
                        List<IntelligentWorkstationDecision> summedCostFactor = new List<IntelligentWorkstationDecision>();

                        int currentIndex = 0;
                        foreach (ConvertedWorkstation workstation in workstations)
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

                                intVector2[index] = summedCostFactor[j].Index;
                                if (intVector2[index] >= IntegerEncoding.Bounds[index, 1])
                                {

                                }
                                return; //Change has been applied
                            }
                        }
                    }
                }
                else if (index < integerInformation[0].Amount + integerInformation[1].Amount)
                {
                    //changes to priority
                    double newValue = Twister.NextDouble() * 0.99999;
                    intVector2[index] = Twister.Next(IntegerEncoding.Bounds[index, 1]); ;
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
                                x.Name == integerInformation[2].BaseObject.Name);
                        if (baseObject is ProjectProcessor processor)
                        {
                            ConvertedData convertedData = processor.ConvertedData;
                            int[] intVector = intVector2
                                .Skip(intVector2.Length - solver.SimulationStatistics.AmountOfCobots)
                                .ToArray();
                            int indexInWorkstations = index - integerInformation[0].Amount - integerInformation[1].Amount;

                            List<ConvertedWorkstation> workstations = convertedData.WorkstationsBasedOnAssignmentInt(intVector);
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
                                ConvertedWorkstation workstation = workstations[j];
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

                            intVector2[index] = workstationToAplyCobot; //Apply new workstation to the index rank based on costs

                            //Ignore shifts based on intelligent assignment
                            //Not that easy workaround, e.g. if the intelligent de selects a workstation that is later encoded 
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                        throw;
                    }
                }

            }
        }

        public override double Evaluate(Individual individual, IRandom random)
        {
            Solver s = (Solver)Solver.Clone();

            if (WorkstationAmount == 0)
                WorkstationAmount = s.SimulationStatistics.AmountOfWorkstations;

            Easy4SimFramework.SimulationStatistics.ApplyVNS = ApplyVNS;

            if (s.SolverSettings == null)
                throw new Exception("SolverSettings should not be null in " + this);
            s.Logger = null; //deactivate logger for performance

            //Get the vector that has been created by Hl
            IntegerVector integerVector = GetVectorFromHlIndividual(individual);

            //Set the parameters in the current solver
            SetIndividuumValues(s, integerVector);

            //Run the evaluation
            s.RunFinishEventOnly();


            double currentRunFitness = GetFitnessOfSolver(s);


            UpdateBestSolution(currentRunFitness);

            int maxK = MinimumVnsNeighborhoodSize / 50;

            //This is set by the user input => should vns be applied
            if (ApplyVNS)
            {
                bool vns = GenericProblemMethods.InRangeForVns(Maximization, currentRunFitness, BestSolutionSoFarGa, VnsPercentage);
                //Check if the solution is in a specified range to the best solution found so far
                //If it is we apply a VNS
                if (vns)
                {
                    VnsRuns++;
                    //If current solution is within x% of the best solution found so far 
                    //Start the VNS 
                    int i = 0;
                    int k = 0;
                    while (k < 3 && DateTime.Now < EndTime)
                    {
                        try
                        {
                            //Clone the initialized solvers for the VNS
                            Solver s2 = (Solver)Solver.Clone();

                            //Copy the solution encoding to apply changes later on
                            IntegerVector intVector2 = new IntegerVector(integerVector);


                            int amountOfChanges = GenericProblemMethods.SetAmountOfChanges(k);

                            //ApplyChanges(intVector2, amountOfChanges, rnd);
                            switch (VnsNeighborhoodOperator)
                            {
                                case 0:
                                    if (string.IsNullOrEmpty(Solver.SimulationStatistics.VnsNeighborhood))
                                        Solver.SimulationStatistics.VnsNeighborhood = $"Basic change";

                                    IntegerEncodingNeighborhood.BasicChange(amountOfChanges, Twister, IntegerInformation, IntegerEncoding, intVector2);
                                    break;
                                case 1:
                                    if (string.IsNullOrEmpty(Solver.SimulationStatistics.VnsNeighborhood))
                                        Solver.SimulationStatistics.VnsNeighborhood = $"Greedy change";

                                    IntegerEncodingNeighborhood.GreedyChange(amountOfChanges, Twister, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    break;
                                case 2:
                                    if (string.IsNullOrEmpty(Solver.SimulationStatistics.VnsNeighborhood))
                                        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining change";

                                    if (MinedResult == null)
                                    {
                                        k = maxK;
                                        continue;
                                    }
                                    IntegerEncodingNeighborhood.ProcessMiningChange(amountOfChanges, Twister, MinedResult, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    break;
                                case 3:
                                    if (string.IsNullOrEmpty(Solver.SimulationStatistics.VnsNeighborhood))
                                        Solver.SimulationStatistics.VnsNeighborhood = $"Process mining dictionary change";

                                    if (MinedWorkstations == null || MinedWorkstations?.Count == 0)
                                    {
                                        k = maxK;
                                        continue;
                                    }
                                    IntegerEncodingNeighborhood.ProcessMiningChangeDictionaryV1(amountOfChanges, Twister, MinedWorkstations, IntegerInformation, IntegerEncoding, intVector2, s2);
                                    break;
                                default:
                                    break;
                            }
                            //======================= Apply changes to the solvers ======================================
                            SetIndividuumValues(s2, intVector2);

                            //======================= Evaluate solvers with changes ======================================
                            s2.RunFinishEventOnly();
                            EvaluatedVnsSolutions++;
                            //======================= Update best solution and reset neighborhood ========================
                            double currentRunFitness2 = GetFitnessOfSolver(s2);
                            if ((currentRunFitness2 > currentRunFitness && Maximization) ||
                                (currentRunFitness2 < currentRunFitness && !Maximization))
                            {
                                integerVector = new IntegerVector(intVector2);
                                currentRunFitness = currentRunFitness2;
                                if ((currentRunFitness > BestSolutionSoFarGa && Maximization) ||
                                    (currentRunFitness < BestSolutionSoFarGa && !Maximization))
                                {
                                    if (UseNormalizedValue)
                                        BestSolutionSoFarGa = currentRunFitness;
                                    else
                                        BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                                }
                                s = s2;
                                i = 0;
                                k = 0;
                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                            throw;
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

            //Set changes in Hl solution
            Individual.SetScopeValue("IntegerVector", individual.Scope, integerVector);

            StringBuilder stringBuilderMiningInformationResource = new StringBuilder();

            //A new best solution has been found in this evaluation
            if (Math.Abs(currentRunFitness - BestSolutionSoFarGa) < 0.001)
            {
                stringBuilderMiningInformationResource = UpdateProblemProperties(s, currentRunFitness, integerVector);

                if (!string.IsNullOrWhiteSpace(stringBuilderMiningInformationResource.ToString()))
                    MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2, new TextValue(stringBuilderMiningInformationResource.ToString()));
            }



            EvaluatedSolutions++;

            if (EvaluatedSolutions % 100 == 0)
            {

            }

            if (VnsNeighborhoodOperator >= 2)
                //Whenever we find a new best solution we mine a process model
                if (Math.Abs(currentRunFitness - BestSolutionSoFarGa) < 0.001)
                {

                    int LpmOrder = Math.Max(IntegerInformation[2].Amount, 2);

                    MinedProcessModels++;
                    if (MinedWorkstations.Count == 0)
                    {
                        for (int i = 1; i <= WorkstationAmount; i++)
                        {
                            MinedWorkstations.Add(i, 1);
                        }
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(stringBuilderMiningInformationResource.ToString());
                    LocalProcessModels.MineLocalProcessModel miner = new MineLocalProcessModel(doc, "org:resource");

                    LocalProcessModels.ProcessMining.IntermediateLocalProcessModelResult result = miner.MineRandomLocalProcessModel(false, lpmSize: LpmOrder);
                    MinedResult = result;
                    foreach (ProcessTree leaf in MinedResult.CurrentProcessTree.GetLeaves())
                    {
                        int index = Solver.SimulationStatistics.Workstations.IndexOf(leaf.Label);
                        MinedWorkstations[index + 1] += 1;
                    }
                }


            return currentRunFitness;
        }


        private StringBuilder UpdateProblemProperties(Solver solver, double currentRunFitness, IntegerVector integerVector)
        {
            StringBuilder stringBuilderMiningInformationResource = new StringBuilder();
            if (BestQuality.Value == null ||
                string.IsNullOrWhiteSpace(BestQuality.Value.Value) ||
                !Maximization && Convert.ToDouble(BestQuality.Value.Value, CultureInfo.InvariantCulture) >
                currentRunFitness ||
                Maximization && Convert.ToDouble(BestQuality.Value.Value, CultureInfo.InvariantCulture) <
                currentRunFitness)
            {
                double currentRunMakespan = solver.Environment.SimulationTime;
                double currentRunCosts = solver.SimulationStatistics.Fitness;


                BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName,
                    BestSolutionQualityDescription,
                    new StringValue(currentRunFitness.ToString(CultureInfo.InvariantCulture)));
                BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription,
                    new TextValue(string.Join(";", integerVector)));
                BestCost = new ValueLookupParameter<StringValue>(BestCostName, BestCostDescription,
                    new StringValue(Convert.ToInt32(currentRunCosts).ToString()));
                BestMakespan = new ValueLookupParameter<StringValue>(BestMakeSpanName, BestMakeSpanDescription,
                    new StringValue(Convert.ToInt32(currentRunMakespan).ToString()));

                foreach (CSimBase simBase in solver.SimulationObjects.SimulationList.Values)
                {
                    var properties = simBase.GetType().GetProperties();
                    PropertyInfo info = properties.FirstOrDefault(x => x.Name.Contains("CobotInformation"));
                    if (info != null)
                    {
                        CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName,
                            CobotLocationsNameDescriptions, new TextValue(info.GetValue(simBase).ToString()));
                    }

                    PropertyInfo info2 = properties.FirstOrDefault(x => x.Name.Contains("WorkStepInformation"));
                    if (info2 != null)
                    {
                        ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription,
                            new TextValue(info2.GetValue(simBase).ToString()));
                    }

                    PropertyInfo infoGantt = properties.FirstOrDefault(x => x.Name.Contains("GanttInformationCsv"));
                    //PropertyInfo productsProduced = properties.FirstOrDefault(x => x.Name.Contains("ProductsProducedCsv"));

                    if (infoGantt != null)
                    {
                        string test = infoGantt.GetValue(simBase).ToString();
                        //string test2 = productsProduced.GetValue(simBase).ToString();
                        string directory = Path.Combine(new[]
                        {
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "Results",
                            "Gantt"
                        });
                        GanttInformation =
                            new ValueLookupParameter<TextValue>(GanttInfo, GanttInfoDescription, new TextValue(test));
                        //if (!Directory.Exists(directory))
                        //    Directory.CreateDirectory(directory);

                        string path = Path.Combine(directory,
                            SolverSettings.Statistics.DataSet + "_" + BestQuality.Value + ".csv");
                        //string path_products = directory + SolverSettings.Statistics.DataSet + "_" + BestQuality.Value + "_productsProduced" +".csv";
                        try
                        {
                            //File.WriteAllText(path, test);
                            //File.WriteAllText(path_products, test2);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(
                                $"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " +
                                e);
                            throw;
                        }
                    }

                    PropertyInfo info4 = properties.FirstOrDefault(x => x.Name == "ProcessMiningInformationXesResource");
                    if (info4 != null)
                    {
                        stringBuilderMiningInformationResource.Append(
                            new TextValue(info4.GetValue(simBase).ToString()));
                    }
                }
            }

            return stringBuilderMiningInformationResource;
        }


        private void SetIndividuumValues(Solver solver, IntegerVector integerVector)
        {
            int skipCounter = 0;
            bool valuesSet = false;
            foreach (ParameterArrayOptimization<int> information in IntegerInformation)
            {
                if (information.Amount == 0)
                    continue;

                SimulationPropertySetter.SetParameterOptimizationInSimulation(solver, information,
                    integerVector.Skip(skipCounter).Take(information.Amount));
                skipCounter += information.Amount;
                valuesSet = true;
            }

            if (!valuesSet)
                throw new Exception("Error in SetIndividuumValues in " + this);
            //######### Run the simulation ###############
        }

        private IntegerVector GetVectorFromHlIndividual(Individual individual)
        {
            if (individual?.Values == null)
                throw new Exception("Error in Individual from HL in " + this);

            IntegerVector integerVector = null;
            foreach (KeyValuePair<string, IItem> pair in individual.Values)
            {
                if (pair.Value is IntegerVector intVector)
                    integerVector = intVector;
            }

            if (integerVector == null)
                throw new Exception("Error in Individual from HL in " + this);

            return integerVector;
        }

        /// <summary>
        /// Check if we have found a new best solution
        /// </summary>
        /// <param name="currentRunFitness"></param>
        private void UpdateBestSolution(double currentRunFitness)
        {
            if (Maximization)
            {
                if (currentRunFitness > BestSolutionSoFarGa || Math.Abs(BestSolutionSoFarGa - double.MaxValue) < 0.001)
                {
                    if (UseNormalizedValue)
                    {
                        BestSolutionSoFarGa = currentRunFitness;
                    }
                    else
                        BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                }

            }
            else
            {
                if (currentRunFitness < BestSolutionSoFarGa || Math.Abs(BestSolutionSoFarGa - double.MaxValue) < 0.001)
                {
                    if (UseNormalizedValue)
                    {
                        BestSolutionSoFarGa = currentRunFitness;
                    }
                    else
                        BestSolutionSoFarGa = Convert.ToInt32(currentRunFitness);
                }
            }
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
        public double GetFitnessOfSolver(Solver solver)
        {
            double result = 0;
            if (MultiObjective.Value.Value)
            {
                double fitness = solver.SimulationStatistics.Fitness * CostFactor + TimeFactor * solver.Environment.SimulationTime;
                if (UseNormalizedValue)
                {
                    NormalizedObjectiveValue normalizedValue = new NormalizedObjectiveValue(solver.Environment.SimulationTime, solver.SimulationStatistics);
                    fitness = normalizedValue.NormalizedCostValue + normalizedValue.NormalizeTimeValue;
                }

                result += fitness;
            }

            else
            {
                result += solver.SimulationStatistics.Fitness;

            }
            return result;
        }


        protected Easy4SimIntegerEncodingProblem(Easy4SimIntegerEncodingProblem easy4SimIntegerEncodingProblem, Cloner cloner) : base(easy4SimIntegerEncodingProblem, cloner)
        {
            if (easy4SimIntegerEncodingProblem == null) return;
            Easy4SimParameter = new ValueLookupParameter<Easy4SimFileValue>(Easy4SimParameterName,
                "The path to the Easy4Sim simulation file.", new Easy4SimFileValue(easy4SimIntegerEncodingProblem.Easy4SimParameter.Value.ToString()));

            BestQuality = new ValueLookupParameter<StringValue>(BestSolutionQualityName, BestSolutionQualityDescription,
                new StringValue(easy4SimIntegerEncodingProblem.BestQuality.Value.Value));

            BestSolution = new ValueLookupParameter<TextValue>(BestSolutionName, BestSolutionNameDescription,
                new TextValue(easy4SimIntegerEncodingProblem.BestSolution.Value.Value));
            CobotLocations = new ValueLookupParameter<TextValue>(CobotLocationsName, CobotLocationsNameDescriptions,
                new TextValue(easy4SimIntegerEncodingProblem.CobotLocations.Value.Value));
            MiningInformation = new ValueLookupParameter<TextValue>(MiningInformationName, MiningInformationNameDescriptions,
                new TextValue(easy4SimIntegerEncodingProblem.MiningInformation.Value.Value));
            MiningInformationResource = new ValueLookupParameter<TextValue>(MiningInformationName2, MiningInformationNameDescriptions2,
                new TextValue(easy4SimIntegerEncodingProblem.MiningInformationResource.Value.Value));
            ReplayInformation = new ValueLookupParameter<TextValue>(ReplayInfo, ReplayInfoDescription,
                new TextValue(easy4SimIntegerEncodingProblem.ReplayInformation.Value.Value));
            MultiObjective = new ValueLookupParameter<BoolValue>(MultiObjectiveName, MultiObjectiveDescription,
                new BoolValue(easy4SimIntegerEncodingProblem.MultiObjective.Value.Value));


            Maximization = easy4SimIntegerEncodingProblem.Maximization;
            ((ISingleObjectiveHeuristicOptimizationProblem)this).MaximizationParameter.Hidden = false;

            SolverSettings = easy4SimIntegerEncodingProblem.SolverSettings?.Clone();
            Solver = new Solver(SolverSettings);

            TimeFactor = easy4SimIntegerEncodingProblem.TimeFactor;
            CostFactor = easy4SimIntegerEncodingProblem.CostFactor;



            BestKnownQuality = easy4SimIntegerEncodingProblem.BestKnownQuality;
            try
            {
                IntegerVectorEncoding encoding = new IntegerVectorEncoding(easy4SimIntegerEncodingProblem.Encoding.Name, easy4SimIntegerEncodingProblem.Encoding.Length);
                encoding.Bounds = easy4SimIntegerEncodingProblem.Encoding.Bounds;
                Encoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }

            try
            {
                IntegerVectorEncoding encoding = new IntegerVectorEncoding(easy4SimIntegerEncodingProblem.Encoding.Name, easy4SimIntegerEncodingProblem.Encoding.Length);
                encoding.Bounds = easy4SimIntegerEncodingProblem.Encoding.Bounds;
                IntegerEncoding = encoding;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }


            IntegerInformation = new List<ParameterArrayOptimization<int>>();

            for (int i = 0; i < easy4SimIntegerEncodingProblem.IntegerInformation.Count; i++)
            {
                ParameterArrayOptimization<int> info = easy4SimIntegerEncodingProblem.IntegerInformation[i].Clone();
                info.BaseObject = (CSimBase)Solver.SimulationObjects.GetSimulationObjectByName(easy4SimIntegerEncodingProblem.IntegerInformation[i].BaseObject.Name);
                info.PropertyInfo = info.BaseObject.GetType().GetProperties().FirstOrDefault(x =>
                    x.Name.Contains(easy4SimIntegerEncodingProblem.IntegerInformation[i].PropertyInfo.Name));
                IntegerInformation.Add(info);
            }
            OnOperatorsChanged();

        }


        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimIntegerEncodingProblem(this, cloner);
        }

    }
}
