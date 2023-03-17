using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DataStore;
using Easy4SimFramework;
using Environment = System.Environment;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    /// <summary>
    /// This class gets the problem definition from the project loader and the current solution array from HL and returns a solution quality.
    /// </summary>
    public class ProjectProcessor : CSimBase
    {
        public bool IsStochasticSimulation = false;

        //List<string> ManualAssignedCobots = new List<string>(){"BAZ001", "BAZ002" };
        List<string> ManualAssignedCobots = new List<string>() { };
        List<string> AlgorithmicAssignedWorkstations = new List<string>() { };

        //List<string> AvailableWorkstations = new List<string>() { "BAZ001", "FRAES003-HAM", "FRAES003", "EINZELMONT", "BOHR004", "BOHR003", "DREH003", "DREH004", "FRAES001", "FRAES002", "LACK001", "SCHLEIF002", "HAND004" };
        List<string> AvailableWorkstations = new List<string>() { };

        /// <summary>
        /// This array is set by HeuristicLab and defines the task to workstation assignment
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization WorkstationAssignment { get; set; }

        /// <summary>
        /// This array is set by HeuristicLab and defines the task priority
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization Priorities { get; set; }

        /// <summary>
        /// This array is set by HeuristicLab and defines cobot locations
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization CobotLocations { get; set; }


        /// <summary>
        /// Input problem definition from the project loader
        /// </summary>
        public InEventObject ReadData { get; set; }
        /// <summary>
        /// Output data for the project statistics
        /// </summary>
        public OutEventObject OutputData { get; set; }



        public ParameterBool InitializeForIntOptimization { get; set; }
        public ParameterBool InitializeForDoubleOptimization { get; set; }


        public ParameterBool FullLog { get; set; }

        public List<ProcessMiningEvent> ProcessMiningList { get; set; } = new List<ProcessMiningEvent>();

        public ConvertedData ConvertedData { get; set; }
        public string ProcessMiningInformation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ProcessMiningEvent.Headers);
                foreach (ProcessMiningEvent miningEvent in ProcessMiningList)
                    sb.Append(miningEvent.ToString());
                return sb.ToString();
            }
        }

        /// <summary>
        /// Format for xes extensions
        /// </summary>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private string XesExtension(string name, string prefix, string uri)
        {
            return $"\t<extension name=\"{name}\" prefix=\"{prefix}\" uri=\"{uri}\"/>{Environment.NewLine}";
        }

        /// <summary>
        /// Create a simple header in the xes format
        /// </summary>
        /// <returns></returns>
        private StringBuilder CreateHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>{Environment.NewLine}");
            sb.Append($"<log xes.version=\"1.0\" xes.features=\"nested - attributes\" openxes.version=\"1.0RC7\" xmlns=\"http://www.xes-standard.org/\">{Environment.NewLine}");
            sb.Append(XesExtension("Lifecycle", "lifecycle", "http://www.xes-standard.org/lifecycle.xesext"));
            sb.Append(XesExtension("Organizational", "org", "http://www.xes-standard.org/org.xesext"));
            sb.Append(XesExtension("Time", "time", "http://www.xes-standard.org/time.xesext"));
            sb.Append(XesExtension("Concept", "concept", "http://www.xes-standard.org/concept.xesext"));
            sb.Append(XesExtension("Cost", "cost", "http://www.xes-standard.org/cost.xesext"));
            sb.Append(XesExtension("ID", "id", "http://www.xes-standard.org/identity.xesext"));


            sb.Append($"\t<global scope=\"trace\">{Environment.NewLine}");
            sb.Append($"\t\t<string key=\"concept:name\" value=\"UNKNOWN\"/>{Environment.NewLine}");
            sb.Append($"\t\t<string key=\"org:group\" value=\"UNKNOWN\"/>{Environment.NewLine}");
            sb.Append($"\t</global>{Environment.NewLine}");
            sb.Append($"\t<global scope=\"event\">{Environment.NewLine}");
            sb.Append($"\t\t<date key=\"time:timestamp\" value=\"1970-01-01T00:00:00+01:00\"/>{Environment.NewLine}");
            sb.Append($"\t\t<string key=\"concept:name\" value=\"UNKNOWN\"/>{Environment.NewLine}");
            sb.Append($"\t\t<string key=\"lifecycle:transition\" value=\"complete\"/>{Environment.NewLine}");
            sb.Append($"\t</global>{Environment.NewLine}");
            sb.Append($"\t<classifier name=\"MXML Legacy Classifier\" keys=\"WorkstationsSpecific\"/>{Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Event Name\" keys=\"concept:name\"/>{Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Resource\" keys=\"org:resource\"/>{Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Order\" keys=\"Order\"/>{Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Cobot\" keys=\"org:role\"/>{Environment.NewLine}");

            //sb.Append($"\t<classifier name=\"WorkstationOriented\" keys=\"org:resource WorkstationCost org:role\"/>{Environment.NewLine}");
            //sb.Append($"\t<classifier name=\"WorkgroupOriented\" keys=\"org:group WorkstationGroupCosts WorkstationGroupCobots\"/>{Environment.NewLine}");

            sb.Append($"\t<string key=\"description\" value=\"Simulated process\"/>{Environment.NewLine}");
            sb.Append($"\t<string key=\"lifecycle:model\" value=\"standard\"/>{Environment.NewLine}");

            sb.Append($"\t<string key=\"Fitness cost\" value=\"{Settings.Statistics.Fitness}\"/>{Environment.NewLine}");
            sb.Append($"\t<string key=\"Fitness time\" value=\"{Settings.Environment.SimulationTime}\"/>{Environment.NewLine}");
            return sb;
        }

        public string ProcessMiningInformationXesResource
        {
            get
            {
                //=============== Not resource oriented anymore ===========================================
                //In this version the start and end events are combined to one event
                foreach (ProcessMiningEvent miningEvent in ProcessMiningList)
                {
                    if (miningEvent.Activity == "complete")
                    {
                        miningEvent.Cost = 0;
                    }
                }
                StringBuilder sb = CreateHeader();
                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.WorkstationGroup))
                {
                    List<string> workstationNumbers = new List<string>();
                    int numberOfCobotsInGroup = 0;
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        if (miningEvent.CobotAssigned && !(workstationNumbers.Contains(miningEvent.WorkStationId)))
                        {
                            workstationNumbers.Add(miningEvent.WorkStationId);
                            numberOfCobotsInGroup++;
                        }
                    }

                    double groupCost = events.Select(x => x.Cost).Sum();
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        miningEvent.WorkstationGroupCost = groupCost;
                        miningEvent.WorkstationGroupCobots = $"{numberOfCobotsInGroup} cobots";
                    }
                }

                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.WorkStationId))
                {
                    double workstationCost = events.Select(x => x.Cost).Sum();
                    double productionTime = 0;
                    foreach (ProcessMiningEvent e in events.Where(x => x.Activity == "complete"))
                    {
                        //Find the fitting start even for each complete event
                        ProcessMiningEvent eStart = events.FirstOrDefault(x => x.Id == e.Id && x.Activity == "start");

                        TimeSpan duration = e.TimeStamp - eStart.TimeStamp;
                        productionTime += duration.TotalSeconds;
                    }

                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        miningEvent.WorkstationCost = workstationCost;
                        miningEvent.WorkstationProductionTime = productionTime;
                    }
                }


                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.OrderNumber).OrderBy(x => x.Key))
                {
                    sb.Append($"\t<trace>{Environment.NewLine}");
                    sb.Append($"\t\t<string key=\"Order\" value=\"{events.Key}\"/>{Environment.NewLine}");
                    //sb.Append($"\t\t<string key=\"cost:total\" value=\"{events.Select(y => y.Cost).Sum()}\"/>{Environment.NewLine}");
                    //sb.Append($"\t\t<string key=\"Time\" value=\"{events.Select(y => y.Duration).Sum()}\"/>{Environment.NewLine}");
                    sb.Append($"\t\t<string key=\"id:Id\" value=\"{Guid.NewGuid().ToString()}\"/>{Environment.NewLine}");
                    //================= Add all event traces here ==============================
                    List<ProcessMiningEvent> listEvents = new List<ProcessMiningEvent>();
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        listEvents.Add(miningEvent);
                        //sb.Append(miningEvent.XesString(2));
                    }

                    listEvents = listEvents.OrderBy(x => x.TimeStamp).ToList();

                    while (listEvents.Count > 0)
                    {

                        //We need to group events, otherwise the process mining software shows can not track when tasks are finished correctly
                        ProcessMiningEvent e = listEvents.Where(x => x.Activity == "start").OrderBy(x => x.TimeStamp).First();  //Get element with minimum timestamp

                        listEvents.Remove(e);

                        List<ProcessMiningEvent> minEvents = listEvents.Where(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "start" && x.WorkStationId == e.WorkStationId).ToList(); //Get all other events from this machine (start and end)
                        listEvents.RemoveAll(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "start"); //remove all events from the list

                        e.Cost += minEvents.Select(x => x.Cost).Sum();
                        e.Quantity += minEvents.Select(x => x.Quantity).Sum();

                        //sb.Append(e.XesString(2));


                        List<ProcessMiningEvent> completeEvents = listEvents.Where(x => x.WorkStepNumber == e.WorkStepNumber && x.WorkStationId == e.WorkStationId).ToList();
                        listEvents.RemoveAll(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "complete");

                        completeEvents = completeEvents.OrderByDescending(x => x.TimeStamp).ToList();

                        ProcessMiningEvent completeEvent = completeEvents.FirstOrDefault();
                        completeEvents.RemoveAt(0);
                        completeEvent.Cost += completeEvents.Select(x => x.Cost).Sum() + e.Cost;
                        completeEvent.Quantity += completeEvents.Select(x => x.Quantity).Sum();
                        sb.Append(completeEvent.XesStringVersionResourceGrouping(2, e.TimeStamp, ConvertedData.Workstations));
                    }
                    //==========================================================================
                    sb.Append($"\t</trace>{Environment.NewLine}");
                }

                sb.Append($"</log>{Environment.NewLine}");

                return sb.ToString();
            }
        }

        public string ProcessMiningInformationXes
        {
            get
            {
                StringBuilder sb = CreateHeader();
                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.WorkstationGroup))
                {
                    List<string> workstationNumbers = new List<string>();
                    int numberOfCobotsInGroup = 0;
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        if (miningEvent.CobotAssigned && !(workstationNumbers.Contains(miningEvent.WorkStationId)))
                        {
                            workstationNumbers.Add(miningEvent.WorkStationId);
                            numberOfCobotsInGroup++;
                        }
                    }

                    double groupCost = events.Select(x => x.Cost).Sum();
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        miningEvent.WorkstationGroupCost = groupCost;
                        miningEvent.WorkstationGroupCobots = $"{numberOfCobotsInGroup} cobots";
                    }
                }

                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.WorkStationId))
                {
                    double workstationCost = events.Select(x => x.Cost).Sum();
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        miningEvent.WorkstationCost = workstationCost;
                    }
                }


                foreach (IGrouping<string, ProcessMiningEvent> events in ProcessMiningList.GroupBy(x => x.OrderNumber))
                {
                    sb.Append($"\t<trace>{Environment.NewLine}");
                    sb.Append($"\t\t<string key=\"Order\" value=\"{events.Key}\"/>{Environment.NewLine}");
                    sb.Append($"\t\t<string key=\"id:Id\" value=\"{Guid.NewGuid().ToString()}\"/>{Environment.NewLine}");
                    //================= Add all event traces here ==============================
                    List<ProcessMiningEvent> listEvents = new List<ProcessMiningEvent>();
                    foreach (ProcessMiningEvent miningEvent in events)
                    {
                        listEvents.Add(miningEvent);
                        //sb.Append(miningEvent.XesString(2));
                    }

                    listEvents = listEvents.OrderBy(x => x.TimeStamp).ToList();

                    while (listEvents.Count > 0)
                    {
                        //We need to group events, otherwise the process mining software shows can not track when tasks are finished correctly
                        ProcessMiningEvent e = listEvents.Where(x => x.Activity == "start").OrderBy(x => x.TimeStamp).First();  //Get element with minimum timestamp
                        listEvents.Remove(e);

                        List<ProcessMiningEvent> minEvents = listEvents.Where(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "start").ToList(); //Get all other events from this machine (start and end)
                        listEvents.RemoveAll(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "start"); //remove all events from the list

                        e.Cost += minEvents.Select(x => x.Cost).Sum();
                        e.Quantity += minEvents.Select(x => x.Quantity).Sum();

                        sb.Append(e.XesString(2, ConvertedData.Workstations));


                        List<ProcessMiningEvent> completeEvents = listEvents.Where(x => x.WorkStepNumber == e.WorkStepNumber).ToList();
                        listEvents.RemoveAll(x => x.WorkStepNumber == e.WorkStepNumber && x.Activity == "complete");

                        completeEvents = completeEvents.OrderByDescending(x => x.TimeStamp).ToList();

                        ProcessMiningEvent completeEvent = completeEvents.FirstOrDefault();
                        completeEvents.RemoveAt(0);
                        completeEvent.Cost += completeEvents.Select(x => x.Cost).Sum();
                        completeEvent.Quantity += completeEvents.Select(x => x.Quantity).Sum();
                        sb.Append(completeEvent.XesString(2, ConvertedData.Workstations));
                        if (completeEvent.TimeStamp < e.TimeStamp)
                        {
                            Console.WriteLine("Something went wrong");
                        }
                    }
                    //==========================================================================
                    sb.Append($"\t</trace>{Environment.NewLine}");
                }

                sb.Append($"</log>{Environment.NewLine}");

                return sb.ToString();
            }
        }

        // ReSharper disable once UnusedMember.Global
        // Used in optimization
        /// <summary>
        /// Information about cobot locations in the evaluation
        /// Used with reflection in the Easy4SimIntegerEncodingProblem
        /// Used with reflection in the Easy4SimRealEncodingProblem
        /// </summary>
        public string CobotInformation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (ConvertedData?.Workstations == null)
                    return "";
                foreach (ConvertedWorkstation workstation in ConvertedData.Workstations.Where(x => x.IsCobotAssigned).OrderBy(x => x.RelativeId))
                {
                    sb.Append(workstation.RelativeId + Environment.NewLine);
                }
                return sb.ToString();
            }
        }
        // ReSharper disable once UnusedMember.Global
        // Used in optimization
        /// <summary>
        /// Information about worksteps in the evaluation
        /// </summary>
        public string WorkStepInformation
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (ConvertedData?.WorkSteps == null)
                    return "";
                string type = "";

                if (Priorities is ParameterArrayOptimization<int>)
                    type = "int";
                if (Priorities is ParameterArrayOptimization<double>)
                    type = "double";



                sb.Append($"WorkSteps:{type}{Environment.NewLine}");
                foreach (ConvertedWorkstep workStep in ConvertedData.WorkSteps.OrderBy(x => x.RelativeId))
                    sb.Append($"{workStep.RelativeId.ToString().PadLeft(2)};{workStep.WorkstationShouldRelative.ToString().PadLeft(2)};{workStep.Priority};{Environment.NewLine}");


                sb.Append("Workstations" + Environment.NewLine);
                foreach (ConvertedWorkstation workstation in ConvertedData.Workstations.OrderBy(x => x.RelativeId))
                    sb.Append($"{workstation.RelativeId};{workstation.IsCobotAssigned};{Environment.NewLine}");


                return sb.ToString();
            }
        }


        public string ProductsProducedCsv
        {
            get
            {
                string seperator = ",";
                StringBuilder sb = new StringBuilder();
                sb.Append("Start,End,Amount,WorkstepId,WorkstationNumber" + Environment.NewLine);
                foreach (ProducedProductLog productLog in ConvertedData.ProductsProduced)
                {
                    sb.Append(
                        $"{productLog.Start}{seperator}{productLog.End}{seperator}{productLog.Amount}{seperator}{productLog.Workstep.RelativeId}{seperator}{productLog.Workstep.IsProducedOnWorkstation.WorkstationNumber}" +
                        Environment.NewLine);


                }

                return sb.ToString();
            }

        }

        public string GanttInformationCsv
        {
            get
            {
                string seperator = ",";
                StringBuilder sb = new StringBuilder();
                sb.Append($"Order{seperator}Task{seperator}Amount{seperator}Workstation{seperator}WorkstationCapacity{seperator}Cobot Assigned{seperator}" +
                          $"Start setup{seperator}End setup{seperator}End production{seperator}End de setup{seperator}" +
                          $"SetupTime{seperator}ProductionTime{seperator}DeSetupTime{seperator}WS Cost factor{seperator}Cost{seperator}{seperator}Makespan:{seperator}{Settings.Environment.SimulationTime}{seperator}ProductionCost:{seperator}{Settings.Statistics.Fitness}" +
                          $"{Environment.NewLine}");


                //PreProcessLogs();

                foreach (WorkstepLog log in ConvertedData.Logs)
                {
                    int cobotAssigned = 0;
                    if (log.Workstation.IsCobotAssigned)
                        cobotAssigned = 1;
                    sb.Append($"{log.Workstep.OrderNumber}{seperator}" +
                              $"{log.Workstep.RelativeId}{seperator}" +
                              $"{log.AmountProduced}{seperator}" +
                              $"{log.Workstep.IsProducedOnWorkstation.WorkstationNumber}{seperator}" +
                              $"{log.Workstep.IsProducedOnWorkstation.Capacity}{seperator}" +
                              $"{cobotAssigned}{seperator}" +
                              $"{log.StartSetup}{seperator}" +
                              $"{log.EndSetup}{seperator}" +
                              $"{log.EndProduction}{seperator}" +
                              $"{log.EndDeSetup}{seperator}" +
                              $"{log.EndSetup - log.StartSetup}{seperator}" +
                              $"{log.EndProduction - log.EndSetup}{seperator}" +
                              $"{log.EndDeSetup - log.EndProduction}{seperator}" +
                              $"{log.Workstation.CostProcessingPerSecond}{seperator}" +
                              $"{log.Cost}{Environment.NewLine}");

                }
                return sb.ToString();
            }
        }



        #region constructor
        public ProjectProcessor() => InitializeParameters();
        public ProjectProcessor(long i, string n, SolverSettings settings) : base(i, n, settings) => InitializeParameters();
        #endregion constructor

        private void InitializeParameters()
        {
            if (Settings != null && Settings.Environment.TimeStart == DateTime.MinValue)
            {
                Settings.Environment.TimeStart = DateTime.Today;
                Settings.Environment.TimeScale = 1000;
            }

            ReadData = new InEventObject(this);
            OutputData = new OutEventObject(this);
            InitializeForIntOptimization = new ParameterBool(true);
            InitializeForDoubleOptimization = new ParameterBool(true);
            FullLog = new ParameterBool(true);

            WorkstationAssignment = new ParameterArrayOptimization();
            Priorities = new ParameterArrayOptimization();
            CobotLocations = new ParameterArrayOptimization();
        }

        public override void Initialize()
        {
            //############ Create Converted data based on read data ###################
            if (ReadData?.Value != null)
                if (ReadData.Value is ConvertedData convertedData)
                    ConvertedData = convertedData;
            if (ConvertedData == null)
                return;
            Settings.Statistics.AmountOfWorkstations = ConvertedData.AmountOfWorkstations;

            List<string> workstations = new List<string>();
            foreach (ConvertedWorkstation workstation in ConvertedData.Workstations)
            {
                workstations.Add(workstation.WorkstationNumber);
            }

            Settings.Statistics.Workstations = workstations;

            ConvertedData.SetConvertedData();

            //############ Write trimmed data to excel  ###############################
            //if (!this.OptimizeForSimulation)
            //    WriteToExcel(StoreToExcelFilePath, ConvertedData);



            //############### Prepare for optimization ####################################
            if (InitializeForIntOptimization.Value || InitializeForDoubleOptimization.Value)
            {
                Dictionary<string, List<ConvertedWorkstation>> workstationGroups = ConvertedData.GetWorkstationGroups();
                //Fix workstations with only one workstation in group
                foreach (ConvertedWorkstep workStep in ConvertedData.WorkSteps)
                {
                    workStep.WorkstationShould = "";
                    List<ConvertedWorkstation> workstationsInGroup = workstationGroups[workStep.WorkstationGroup];
                    if (workstationsInGroup.Count > 1) continue;
                    workStep.WorkstationShould = workstationsInGroup[0].WorkstationNumber;
                    workStep.WorkstationShouldRelative = workstationsInGroup[0].RelativeId;
                    foreach (ConvertedOrder order in ConvertedData.Orders)
                    {
                        foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder)
                        {
                            if (workstep.Id == workStep.Id)
                            {
                                workstep.WorkstationShouldRelative = workStep.WorkstationShouldRelative;
                                workstep.WorkstationShould = workStep.WorkstationShould;
                            }
                        }
                    }
                }

                //############# Optimization ############################
                if (InitializeForIntOptimization)
                    InitializeForIntegerOptimization(workstationGroups);

                if (InitializeForDoubleOptimization)
                    InitializeForRealOptimization();


            }

        }

        private void InitializeForRealOptimization()
        {
            if (!(WorkstationAssignment is ParameterArrayOptimization<double>))
            {
                WorkstationAssignment = new ParameterArrayOptimization<double>(ConvertedData.AmountOfNonAssignedWorkSteps);
                for (int i = 0; i < ConvertedData.AmountOfNonAssignedWorkSteps; i++)
                    ((ParameterArrayOptimization<double>)WorkstationAssignment).UpperBounds[i] = 0.99999999999;
            }

            if (!(Priorities is ParameterArrayOptimization<double>))
            {
                Priorities = new ParameterArrayOptimization<double>(ConvertedData.AmountOfWorkSteps);
                for (int i = 0; i < ConvertedData.AmountOfWorkSteps; i++)
                    ((ParameterArrayOptimization<double>)Priorities).UpperBounds[i] = 0.99999999999;
            }

            if (CobotLocations is ParameterArrayOptimization<double>) return;

            CobotLocations = new ParameterArrayOptimization<double>(this.Settings.Statistics.AmountOfCobots);
            for (int i = 0; i < this.Settings.Statistics.AmountOfCobots; i++)
                ((ParameterArrayOptimization<double>)CobotLocations).UpperBounds[i] = 0.99999999999;

        }

        private void InitializeForIntegerOptimization(Dictionary<string, List<ConvertedWorkstation>> workstationGroups)
        {
            if (!(WorkstationAssignment is ParameterArrayOptimization<int>))
            {
                List<ConvertedWorkstep> nonAssignedWorkSteps = ConvertedData.WorkstepsWhereWorkstationHasToBeDecided;
                WorkstationAssignment = new ParameterArrayOptimization<int>(nonAssignedWorkSteps.Count);
                for (int i = 0; i < nonAssignedWorkSteps.Count; i++)
                {
                    var workstations = workstationGroups[nonAssignedWorkSteps[i].WorkstationGroup];
                    ((ParameterArrayOptimization<int>)WorkstationAssignment).UpperBounds[i] = workstationGroups[nonAssignedWorkSteps[i].WorkstationGroup].Count;
                }
            }

            if (!(Priorities is ParameterArrayOptimization<int>))
            {
                Priorities = new ParameterArrayOptimization<int>(ConvertedData.AmountOfWorkSteps);
                for (int i = 0; i < ConvertedData.AmountOfWorkSteps; i++)
                    ((ParameterArrayOptimization<int>)Priorities).UpperBounds[i] = int.MaxValue;
            }

            if (CobotLocations is ParameterArrayOptimization<int>) return;

            CobotLocations = new ParameterArrayOptimization<int>(this.Settings.Statistics.AmountOfCobots);
            for (int i = 0; i < this.Settings.Statistics.AmountOfCobots; i++)
                ((ParameterArrayOptimization<int>)CobotLocations).UpperBounds[i] = ConvertedData.AmountOfWorkstations - i;
        }

        public override void End()
        {
            SimulationStatistics.EvaluatedSolutions += 1;
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (SimulationStatistics.ApplyVNS)
            {
                List<string> lines = new List<string>();
                if (SimulationStatistics.LowerLimitCost == -1 ||
                    SimulationStatistics.UpperLimitCost == -1 ||
                    SimulationStatistics.LowerLimitTime == -1 ||
                    SimulationStatistics.UpperLimitTime == -1 ||
                    SimulationStatistics.ReadDataSet != Settings.Statistics.DataSet ||
                    SimulationStatistics.ReadCobots != Settings.Statistics.AmountOfCobots)
                {
                    Console.WriteLine("=================================================================");
                    Console.WriteLine("Read limits file: " + Settings.Statistics.DataSet);

                    FileLoader loader = new FileLoader();
                    string limitsContent = loader.LoadFile("Limits_CobotAndJobShop.txt");
                    lines = limitsContent.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).Where(x => x.StartsWith(Settings.Statistics.DataSet)).ToList();
                    
                    foreach (string line in lines)
                    {
                        string[] lineParts = line.Split();
                        if (lineParts[0] == Settings.Statistics.DataSet)
                        {
                            if (lineParts[1] == "cost")
                            {
                                if (lineParts[2] == "lowerLimit")
                                {
                                    try
                                    {
                                        SimulationStatistics.LowerLimitCost = double.Parse(lineParts[3], CultureInfo.InvariantCulture);
                                        Console.WriteLine("Lower limit cost: " + SimulationStatistics.LowerLimitCost);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error in Project Processor - 1");
                                        Console.WriteLine(e);
                                        throw;
                                    }
                                }
                                else //Upper limit
                                {
                                    try
                                    {
                                        SimulationStatistics.UpperLimitCost =
                                            double.Parse(lineParts[3], CultureInfo.InvariantCulture); 
                                            Console.WriteLine("Upper limit cost: " + SimulationStatistics.UpperLimitCost);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error in Project Processor - 2");
                                        Console.WriteLine(e);
                                        throw;
                                    }
                                }
                            }
                            else //Time
                            {
                                if (lineParts[2] == "lowerLimit")
                                {
                                    try
                                    {
                                        SimulationStatistics.LowerLimitTime =
                                            int.Parse(lineParts[3], CultureInfo.InvariantCulture);
                                        Console.WriteLine("Lower limit time: " + SimulationStatistics.LowerLimitTime);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error in Project Processor - 3");
                                        Console.WriteLine(e);
                                        if (SimulationStatistics.LowerLimitTime == -1)
                                        {

                                            SimulationStatistics.LowerLimitTime =
                                                (int)double.Parse(lineParts[3], CultureInfo.InvariantCulture);
                                        }

                                        throw;
                                    }
                                }
                                else //Upper limit
                                {
                                    try
                                    {

                                        SimulationStatistics.UpperLimitTime =
                                            int.Parse(lineParts[3], CultureInfo.InvariantCulture);
                                        Console.WriteLine("Upper limit time: " + SimulationStatistics.UpperLimitTime);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error in Project Processor - 4");
                                        Console.WriteLine(e);
                                        if (SimulationStatistics.UpperLimitTime == -1)
                                        {
                                            SimulationStatistics.UpperLimitTime =
                                                (int)double.Parse(lineParts[3], CultureInfo.InvariantCulture);
                                        }

                                        throw;
                                    }
                                }
                            }
                        }
                    }

                    SimulationStatistics.ReadDataSet = Settings.Statistics.DataSet;
                    SimulationStatistics.ReadCobots = Settings.Statistics.AmountOfCobots;
                }
            }

            foreach (ConvertedWorkstation workstation in ConvertedData.Workstations)
            {
                List<ProcessMiningEvent> info = ProcessMiningList
                    .Where(x => x.WorkStationId == workstation.WorkstationNumber).ToList();

                double summedProcessingTime = 0;
                if (info.Count != 0)
                {
                    for (int i = 0; i < info.Count; i += 2)
                    {
                        var test = info[i + 1].TimeStamp - info[i].TimeStamp;

                        summedProcessingTime += test.TotalSeconds;
                    }

                }

                workstation.IdleTime = Settings.Environment.SimulationTime - (int)summedProcessingTime;
            }


            base.End();
        }

        public override void Start()
        {
            //if (IsStochasticSimulation)
            //{
            //    Random random = new Random();
            //    //
            //    List<ConvertedWorkstation> brokenWorkstations = new List<ConvertedWorkstation>();
            //
            //    foreach (ConvertedWorkstation workstation in ConvertedData.Workstations)
            //        if (random.NextDouble() < workstation.BreakingChance)
            //            brokenWorkstations.Add(workstation);
            //
            //    foreach (ConvertedWorkstation workstation in brokenWorkstations)
            //        ConvertedData.Workstations.Remove(workstation);
            //
            //
            //    foreach (ConvertedWorkstep step in ConvertedData.WorkSteps)
            //    {
            //        if (step.ProductionTime != 0)
            //            step.ProductionTime = Convert.ToInt32(Poisson.Sample(random, step.ProductionTime));
            //        if (step.DeSetupTime != 0)
            //            step.DeSetupTime = Convert.ToInt32(Poisson.Sample(random, step.DeSetupTime));
            //        if (step.SetupTime != 0)
            //            step.SetupTime = Convert.ToInt32(Poisson.Sample(random, step.SetupTime));
            //    }
            //
            //
            //}

            try
            {
                ConvertedWorkstep[] nonAssignedWorkSteps = ConvertedData.NonAssignedWorkSteps;
                Dictionary<string, List<ConvertedWorkstation>> workstationGroups = ConvertedData.GetWorkstationGroups();
                switch (WorkstationAssignment)
                {
                    case ParameterArrayOptimization<double> doubleOptimization:
                        for (int i = 0; i < doubleOptimization.Value.Length; i++)
                        {
                            try
                            {
                                string group = nonAssignedWorkSteps[i].WorkstationGroup;
                                string alternativeGroup = nonAssignedWorkSteps[i].AlternativeWorkgroup;
                                List<ConvertedWorkstation> workstations = new List<ConvertedWorkstation>();
                                if (workstationGroups.ContainsKey(group))
                                {
                                    workstations.AddRange(workstationGroups[group]);
                                }
                                else
                                {
                                    workstations.AddRange(workstationGroups[alternativeGroup]);
                                    //All workstations in a group have been destroyed
                                    //What to do now
                                }


                                int index = (int)(doubleOptimization.Value[i] * workstations.Count);
                                if (index >= workstations.Count)
                                {
                                    index = 0; //HL generates random values inclusive the upper bound
                                }

                                nonAssignedWorkSteps[i].WorkstationShould = workstations[index].WorkstationNumber;
                                nonAssignedWorkSteps[i].WorkstationShouldRelative = workstations[index].RelativeId;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error in Project Processor - 6");
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                        break;
                    case ParameterArrayOptimization<int> intOptimization:
                        try
                        {
                            for (int i = 0; i < intOptimization.Value.Length; i++)
                            {
                                ConvertedWorkstep ws = nonAssignedWorkSteps[i];
                                string group = ws.WorkstationGroup;
                                string alternativeGroup = ws.AlternativeWorkgroup;

                                List<ConvertedWorkstation> workstations = new List<ConvertedWorkstation>();
                                if (workstationGroups.ContainsKey(group))
                                {
                                    workstations.AddRange(workstationGroups[group]);
                                }
                                else
                                {
                                    workstations.AddRange(workstationGroups[alternativeGroup]);
                                    //All workstations in a group have been destroyed
                                    //What to do now
                                }


                                int optValue = intOptimization.Value[i];


                                if (optValue >= workstations.Count)
                                {
                                    Random rnd = new Random();
                                    intOptimization.Value[i] = rnd.Next(workstations.Count);
                                    optValue = intOptimization.Value[i];
                                }
                                ws.WorkstationShould = workstations[optValue].WorkstationNumber;
                                ws.WorkstationShouldRelative = workstations[optValue].RelativeId;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error in Project Processor - 7");
                            Console.WriteLine(e);
                            throw;
                        }
                        break;
                }


                switch (Priorities)
                {
                    case ParameterArrayOptimization<double> doubleOptimization:
                        for (int i = 0; i < doubleOptimization.Value.Length; i++)
                        {
                            ConvertedData.WorkSteps[i].Priority = doubleOptimization.Value[i];
                        }

                        break;
                    case ParameterArrayOptimization<int> intOptimization:
                        for (int i = 0; i < intOptimization.Value.Length; i++)
                        {
                            ConvertedData.WorkSteps[i].Priority = intOptimization.Value[i];
                        }

                        break;
                }


                if (SimulationStatistics.FixedCobots != null)
                    foreach (string cobot in SimulationStatistics.FixedCobots)
                        AlgorithmicAssignedWorkstations.Add(cobot);


                foreach (string algorithmicWorkstation in AlgorithmicAssignedWorkstations)
                {
                    int id = Convert.ToInt32(algorithmicWorkstation.Replace("Workstation ", ""));
                    ConvertedWorkstation workstation = ConvertedData.Workstations.FirstOrDefault(x => x.RelativeId == id);
                    if (workstation != null)
                        workstation.IsCobotAssigned = true;
                }


                foreach (string cobot in ManualAssignedCobots)
                {
                    ConvertedWorkstation workstation = ConvertedData.Workstations.FirstOrDefault(x => x.WorkstationNumber == cobot);
                    if (workstation != null)
                        workstation.IsCobotAssigned = true;
                }


                switch (CobotLocations)
                {
                    case ParameterArrayOptimization<double> doubleOptimization:
                        foreach (double d in doubleOptimization.Value)
                        {
                            try
                            {
                                List<ConvertedWorkstation> workstations = new List<ConvertedWorkstation>();

                                if (AvailableWorkstations.Count != 0)
                                    workstations = ConvertedData.Workstations.Where(x => AvailableWorkstations.Contains(x.WorkstationNumber)).Where(x => !x.IsCobotAssigned).ToList();
                                else
                                    workstations = ConvertedData.Workstations.Where(x => !x.IsCobotAssigned).ToList();
                                
                                if (workstations.Count == 0)
                                    break;
                                int index = (int)(d * workstations.Count);
                                workstations[index].IsCobotAssigned = true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error in Project Processor - 8");
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                        break;

                    case ParameterArrayOptimization<int> intOptimization:
                        foreach (int i in intOptimization.Value)
                        {
                            List<ConvertedWorkstation> workstations = new List<ConvertedWorkstation>();

                            if (AvailableWorkstations.Count != 0)
                                workstations = ConvertedData.Workstations.Where(x => AvailableWorkstations.Contains(x.WorkstationNumber)).Where(x => !x.IsCobotAssigned).ToList();
                            else
                                workstations = ConvertedData.Workstations.Where(x => !x.IsCobotAssigned).ToList();
                            

                            if (workstations.Count == 0)
                                break;

                            if (i < workstations.Count)
                                workstations[i].IsCobotAssigned = true;
                            else 
                                workstations[i % workstations.Count].IsCobotAssigned = true;
                            
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Project Processor - 9");
                Console.WriteLine(e);
                throw;
            }

            //Set all workstations to capacity type 1
            foreach (ConvertedWorkstation workstation in ConvertedData.Workstations)
            {
                if (workstation.CapacityTyp == 0 || workstation.CapacityTyp == 4)
                {
                    workstation.CapacityTyp = 1;
                    workstation.Capacity = 1;
                }

                if (workstation.CapacityTyp == 2)
                {
                    workstation.CapacityTyp = 1;
                    workstation.Capacity = 1000;
                }
            }


            //############ Set data to output for statistics ##########################
            OutputData.Set(ConvertedData);
        }


        private static int discCounter = 0;
        public override void DiscreteCalculation()
        {
            if (FullLog.Value && !this.OptimizeForSimulation)
                Settings.Environment.NewEventLog(LogName, $"Discrete calculation at {Settings.Environment.SimulationTime}",
                    Easy4SimFramework.Environment.LoggingCategory.Warning);
            //Check for finished orders
            //CheckForFinishedDesetupTasks();
            discCounter++;
            try
            {
                CheckForFinishedOrders();
                //CheckQueueLength();
                AssignWorkStepsToWorkstations();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Project Processor - 10");
                Console.WriteLine(e);
                throw;
            }
        }


        private void AssignWorkStepsToWorkstations()
        {
            try
            {
                //Non producing and non finished worksteps should be assigned to workstations
                List<ConvertedWorkstep> worksteps = new List<ConvertedWorkstep>();
                foreach (ConvertedOrder order in ConvertedData.Orders)
                {
                    foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder)
                    {
                        bool allPreviousWorkStepsFinished = true;
                        foreach (int previousWorkStep in workstep.PreviousWorkSteps)
                        {
                            if (!order.FinishedWorksteps.Select(x => x.RelativeId).Contains(previousWorkStep))
                            {
                                allPreviousWorkStepsFinished = false;
                                break;
                            }
                        }

                        if (!workstep.IsProducing &&
                            !workstep.IsFinished &&
                            allPreviousWorkStepsFinished)
                        {
                            worksteps.Add(workstep);
                        }
                    }
                }

                List<ConvertedWorkstep> startedWorksteps = worksteps.Where(x => x.HasStartedProducing).OrderByDescending(y => y.Priority).ToList();
                List<ConvertedWorkstep> nonStartedWorksteps = worksteps.Where(x => !x.HasStartedProducing).OrderByDescending(y => y.Priority).ToList();


                //Prioritize started worksteps (At least Produced amount 1)
                foreach (ConvertedWorkstep workStep in startedWorksteps)
                {
                    ConvertedWorkstation workstation =
                        ConvertedData.Workstations.FirstOrDefault(x =>
                            x.RelativeId == workStep.WorkstationShouldRelative);

                    if (workstation == null)
                        continue;


                    if (workstation.CanProduce(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced))
                    {
                        AddWorkStepToWorkstation(workstation, workStep);
                    }
                }

                foreach (ConvertedWorkstep workStep in nonStartedWorksteps)
                {
                    ConvertedWorkstation workstation =
                        ConvertedData.Workstations.FirstOrDefault(x =>
                            x.RelativeId == workStep.WorkstationShouldRelative);

                    if (workstation == null)
                        continue;

                    if (workstation.CanProduce(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced))
                    {
                        AddWorkStepToWorkstation(workstation, workStep);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Project Processor - 11 ");
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Check for finished work steps in orders
        /// </summary>
        private void CheckForFinishedOrders()
        {
            try
            {

                List<ConvertedOrder> finishedOrders = new List<ConvertedOrder>();


                List<ConvertedWorkstep> taskstoFinish = new List<ConvertedWorkstep>();
                foreach (ConvertedOrder order in ConvertedData.Orders)
                {
                    foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder)
                    {
                        if (workstep.IsProducing && !workstep.IsFinished && Settings.Environment.SimulationTime >= workstep.IsDesetupUntil)
                        {
                            taskstoFinish.Add(workstep);
                        }
                    }

                }

                try
                {
                    foreach (ConvertedWorkstep workstep in taskstoFinish)
                    {
                        ConvertedOrder order = ConvertedData.Orders.FirstOrDefault(x => x.OrderNumber == workstep.OrderNumber);
                        FinishProducing(workstep, order);

                        if (order.WorkstepsInOrder.Count != 0) continue;
                        //if all workSteps in one order are finished the order is finished
                        order.IsFinished = true;
                        finishedOrders.Add(order);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in Project Processor - 12");
                    Console.WriteLine(e);
                    throw;
                }


                foreach (ConvertedOrder order in finishedOrders)
                {
                    ConvertedData.Orders.Remove(order);
                    ConvertedData.FinishedOrders.Add(order);

                    foreach (ConvertedWorkstep workStep in order.WorkstepsInOrder)
                    {
                        ConvertedData.WorkSteps.Remove(workStep);
                        ConvertedData.FinishedWorkSteps.Add(workStep);
                    }
                }


                if (ConvertedData.Orders.Count == 0)
                    Settings.Environment.SimulationFinished = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Project Processor - 13");
                Console.WriteLine(e);
                throw;
            }
        }

        private void FinishProducing(ConvertedWorkstep workStep, ConvertedOrder order)
        {
            //############ WorkStep parts finished ##############
            workStep.ProducedAmount += workStep.CurrentlyProducingAmount;
            workStep.CurrentlyProducingAmount = 0;
            workStep.IsProducing = false;

            ConvertedWorkstation workstation = workStep.IsProducedOnWorkstation;
            //workstation.RemoveFromWorkstation(workStep);


            if (workStep.ProducedAmount < workStep.Amount) return;
            //############ WorkStep finished ##############
            workStep.IsFinished = true;

            order.WorkstepsInOrder.Remove(workStep);
            order.FinishedWorksteps.Add(workStep);


            if (!this.OptimizeForSimulation)
                Settings.Environment.NewEventLog(LogName,
                    $"Order {order.OrderNumber}: WorkStep {workStep.RelativeId} ({workStep.Description}) finished (Workstation: {workstation.WorkstationNumber})",
                    Easy4SimFramework.Environment.LoggingCategory.Info);
        }


        private void AddWorkStepToWorkstation(ConvertedWorkstation workstation, ConvertedWorkstep workStep)
        {
            //Capacity types:
            //0: Single
            //1: Capacity
            //2: Unlimited
            //4: Team


            workStep.IsProducing = true;
            workStep.HasStartedProducing = true;



            //Production and setup time with/without cobots
            long workStepProductionTime = !workstation.IsCobotAssigned ? workStep.ProductionTime : (long)(workStep.ProductionTime * 0.7);
            long workStepSetupTime = !workstation.IsCobotAssigned ? workStep.SetupTime : (long)(workStep.SetupTime * 0.7);
            long workStepDeSetupTime = !workstation.IsCobotAssigned ? workStep.DeSetupTime : (long)(workStep.DeSetupTime * 0.7);

            //If the previous producing workstation is the workstation where the parts are produced now
            if (workStep.InitializedAmount != 0)
            {
                workStepSetupTime = 0;
            }

            //Production and setup time with/without workstation speed factor
            long setupTime = (long)(workStepSetupTime * workstation.SpeedFactorSetup);
            long productionTime = (long)(workStepProductionTime * workstation.SpeedFactorWorking);
            long deSetupTime = (long)(workStepDeSetupTime * workstation.SpeedFactorSetup);


            int amountToProduce = -1;
            if (workstation.CapacityTyp == 1)
            {
                //Calculate how much amount should be produced
                //If we have more capacity than needed, we produce the rest
                //Otherwise we produce the remaining cpacity
                if (workstation.RemainingCapacity(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced) >= workStep.RemainingAmountToProduce)
                    amountToProduce = workStep.RemainingAmountToProduce;
                else
                    amountToProduce = workstation.RemainingCapacity(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced);

                if (workStep.InitializedAmount != 0)
                {
                    amountToProduce = Math.Min(workStep.InitializedAmount, amountToProduce);
                    //Desetup for initialized workstation capacity that is not used in the final production task of a workstaion
                    if (amountToProduce < workStep.InitializedAmount && deSetupTime > 0)
                    {
                        //Add a desetup task to the workstation
                        long desetupFinish = deSetupTime + Settings.Environment.SimulationTime;
                        int amountToDesetup = workStep.InitializedAmount - amountToProduce;
                        double costForDesetup = amountToDesetup * deSetupTime * workstation.CostSetupToolPerSecond;

                        Settings.Statistics.Fitness += costForDesetup;  //Setup costs

                        if (!Settings.Statistics.FitnessPerComponent.ContainsKey(workstation.RelativeId))
                            Settings.Statistics.FitnessPerComponent.Add(workstation.RelativeId, 0);
                        Settings.Statistics.FitnessPerComponent[workstation.RelativeId] += costForDesetup;

                        if (!Settings.Statistics.ExecutionTimePerComponent.ContainsKey(workstation.RelativeId))
                            Settings.Statistics.ExecutionTimePerComponent.Add(workstation.RelativeId, 0);
                        Settings.Statistics.ExecutionTimePerComponent[workstation.RelativeId] += (deSetupTime * amountToDesetup);

                        if (amountToDesetup +
                            workstation.CurrentlyProducingAmount(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced) >
                            workstation.Capacity)
                        {
                            Console.WriteLine("Error in project processor");
                        }


                        workstation.WorkstepLogs.Add(new WorkstepLog(amountToDesetup, workstation, workStep, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime + deSetupTime, costForDesetup));

                        workStep.InitializedAmount = workStep.InitializedAmount - amountToDesetup;

                        ConvertedData.ProductsProduced.Add(new ProducedProductLog(Settings.Environment.SimulationTime, desetupFinish, amountToDesetup, workStep, "Desetup"));
                        ConvertedData.Logs.Add(new WorkstepLog(amountToDesetup, workstation, workStep, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime, Settings.Environment.SimulationTime + deSetupTime, costForDesetup));
                        Settings.SimulationObjects.EventQueue.Add(this, desetupFinish);
                    }
                }
            }

            if (workStep.Amount - amountToProduce - workStep.ProducedAmount > 0)
            {
                //Not in the last task, we set desetup time to 0
                deSetupTime = 0;
            }

            //#################### Time  ##################################
            long time = setupTime +
                        productionTime +
                        deSetupTime +
                        Settings.Environment.SimulationTime;


            //#################### Settings  ##################################

            workStep.IsDesetupUntil = time;
            workStep.CurrentlyProducingAmount = amountToProduce;

            if (workStep.InitializedAmount == 0)
            {
                //The setup time is calculated here
                workStep.InitializedAmount = workStep.CurrentlyProducingAmount;



            }

            if (amountToProduce +
                workstation.CurrentlyProducingAmount(Settings.Environment.SimulationTime, ConvertedData.ProductsProduced) >
                workstation.Capacity)
            {
                Console.WriteLine("Error in project processor");
            }

            //######################## Add to products that are produced on a workstation ###################################

            string comment = "";
            if (workStep.RemainingAmountToProduce == 0)
                comment = "Desetup";
            else
                comment = "Normal production";
            ConvertedData.ProductsProduced.Add(new ProducedProductLog(Settings.Environment.SimulationTime, time, workStep.CurrentlyProducingAmount, workStep, comment));

            //#################### Add simulation event ##################################
            Settings.SimulationObjects.EventQueue.Add(this, time);
            workStep.IsProducedOnWorkstation = workstation;

            //#################### Costs  ##################################
            double cost = (productionTime * workstation.CostProcessingPerSecond +  //Production costs
                          (setupTime + deSetupTime) * workstation.CostSetupToolPerSecond) * workStep.CurrentlyProducingAmount;  //Setup costs
            Settings.Statistics.Fitness += cost;  //Setup costs


            //############### This is important for the intelligent changes #######################################
            if (!Settings.Statistics.FitnessPerComponent.ContainsKey(workstation.RelativeId))
                Settings.Statistics.FitnessPerComponent.Add(workstation.RelativeId, 0);
            Settings.Statistics.FitnessPerComponent[workstation.RelativeId] += cost;

            if (!Settings.Statistics.ExecutionTimePerComponent.ContainsKey(workstation.RelativeId))
                Settings.Statistics.ExecutionTimePerComponent.Add(workstation.RelativeId, 0);
            Settings.Statistics.ExecutionTimePerComponent[workstation.RelativeId] += (setupTime +
                                                                                     productionTime +
                                                                                     deSetupTime) * workStep.CurrentlyProducingAmount;



            ConvertedData.Logs.Add(new WorkstepLog(workStep.CurrentlyProducingAmount,
                                                    workstation, workStep,
                                                    Settings.Environment.SimulationTime,
                                                    Settings.Environment.SimulationTime + setupTime,
Settings.Environment.SimulationTime + setupTime + productionTime, time, cost));



            //########################## Process mining ##################################################
            DateTime start = Settings.Environment.TimeStart.AddSeconds(Settings.Environment.SimulationTime);
            DateTime end = start.AddSeconds(setupTime +
                                            productionTime +
                                            deSetupTime);
            Guid id = Guid.NewGuid();
            ProcessMiningList.Add(new ProcessMiningEvent(workStep.OrderNumber, workStep.RelativeId.ToString(), workstation.WorkstationNumber, "start", cost, start, workStep.CurrentlyProducingAmount, "4", workstation.WorkstationGroupNumber, workstation.IsCobotAssigned, time, id));
            ProcessMiningList.Add(new ProcessMiningEvent(workStep.OrderNumber, workStep.RelativeId.ToString(), workstation.WorkstationNumber, "complete", 0, end, workStep.CurrentlyProducingAmount, "4", workstation.WorkstationGroupNumber, workstation.IsCobotAssigned, 0, id));


        }


        public override object Clone()
        {
            ProjectProcessor result = new ProjectProcessor(Index, Name, Settings);
            result.InitializeForIntOptimization = InitializeForIntOptimization;
            result.InitializeForDoubleOptimization = InitializeForDoubleOptimization;

            result.WorkstationAssignment = WorkstationAssignment.Clone();
            result.WorkstationAssignment.BaseObject = result;
            result.CobotLocations = CobotLocations.Clone();
            result.CobotLocations.BaseObject = result;
            result.Priorities = Priorities.Clone();
            result.Priorities.BaseObject = result;

            result.OptimizeForSimulation = OptimizeForSimulation;

            result.FullLog = FullLog;
            if (ConvertedData != null)
                result.ConvertedData = (ConvertedData)ConvertedData.Clone();


            result.CurrentGuid = CurrentGuid;
            foreach (ProcessMiningEvent miningEvent in ProcessMiningList)
            {
                result.ProcessMiningList.Add((ProcessMiningEvent)miningEvent.Clone());
            }

            return result;
        }
    }
}
