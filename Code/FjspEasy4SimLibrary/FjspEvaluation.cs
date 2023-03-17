using Easy4SimFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Evaluation object that assigns operations to workstations and generates a makespan 
    /// </summary>
    public class FjspEvaluation : CSimBase
    {
        /// <summary>
        /// Read data  that is filled by the FjspLoader
        /// </summary>
        public InEventObject ReadData { get; set; }

        /// <summary>
        /// Array that defines the operation to workstation assignment
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization<int> WorkstationAssignment { get; set; }

        /// <summary>
        /// Array that defines the operation priority
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization<int> Priority { get; set; }
        //public ParameterArrayOptimization<int> OperatorSequence { get; set; }

        /// <summary>
        /// Array that defines the locations of the cobots that should be assigned
        /// </summary>
        [Optimization]
        public ParameterArrayOptimization<int> CobotLocations { get; set; }

        /// <summary>
        /// Read data from the problem file in a structured format
        /// </summary>
        public FlexibleJobShopSchedulingData Data;

        /// <summary>
        /// Data from the evaluation
        /// </summary>
        public EvaluationData EvaluationData;


        /// <summary>
        /// Used with reflection in the Easy4SimFjsspEncoding.
        /// Logged to a file in comma separated format.
        /// </summary>
        public string GanttInformationCsv
        {
            get
            {
                string separator = ",";
                StringBuilder sb = new StringBuilder();
                sb.Append($"Job{separator}Operation{separator}Workstation{separator}Cobot on Workstation{separator}Start time{separator}End time{separator}{separator}Makespan:{separator}{Settings.Statistics.Fitness}{ System.Environment.NewLine}");

                //PreProcessLogs();

                foreach (WorkstationLog log in EvaluationData.AllLogs)
                {
                    sb.Append($"{log.Operation.JobId}{separator}" +
                        $"{log.Operation.Id}{separator}" +
                        $"{log.Workstation.Id}{separator}" +
                        $"{log.Workstation.IsCobotAssigned}{separator}" +
                        $"{log.StartTime}{separator}" +
                        $"{log.EndTime}{separator}" +
                        $"{System.Environment.NewLine}");
                }
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
            return $"\t<extension name=\"{name}\" prefix=\"{prefix}\" uri=\"{uri}\"/>{System.Environment.NewLine}";
        }

        /// <summary>
        /// Create a simple header in the xes format
        /// </summary>
        /// <returns></returns>
        private StringBuilder CreateHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>{System.Environment.NewLine}");
            sb.Append($"<log xes.version=\"1.0\" xes.features=\"nested - attributes\" openxes.version=\"1.0RC7\" xmlns=\"http://www.xes-standard.org/\">{System.Environment.NewLine}");
            sb.Append(XesExtension("Lifecycle", "lifecycle", "http://www.xes-standard.org/lifecycle.xesext"));
            sb.Append(XesExtension("Organizational", "org", "http://www.xes-standard.org/org.xesext"));
            sb.Append(XesExtension("Time", "time", "http://www.xes-standard.org/time.xesext"));
            sb.Append(XesExtension("Concept", "concept", "http://www.xes-standard.org/concept.xesext"));
            sb.Append(XesExtension("Cost", "cost", "http://www.xes-standard.org/cost.xesext"));
            sb.Append(XesExtension("ID", "id", "http://www.xes-standard.org/identity.xesext"));


            sb.Append($"\t<global scope=\"trace\">{System.Environment.NewLine}");
            sb.Append($"\t\t<string key=\"concept:name\" value=\"UNKNOWN\"/>{System.Environment.NewLine}");
            sb.Append($"\t\t<string key=\"org:group\" value=\"UNKNOWN\"/>{System.Environment.NewLine}");
            sb.Append($"\t</global>{System.Environment.NewLine}");
            sb.Append($"\t<global scope=\"event\">{System.Environment.NewLine}");
            sb.Append($"\t\t<date key=\"time:timestamp\" value=\"1970-01-01T00:00:00+01:00\"/>{System.Environment.NewLine}");
            sb.Append($"\t\t<string key=\"concept:name\" value=\"UNKNOWN\"/>{System.Environment.NewLine}");
            sb.Append($"\t\t<string key=\"lifecycle:transition\" value=\"complete\"/>{System.Environment.NewLine}");
            sb.Append($"\t</global>{System.Environment.NewLine}");
            sb.Append($"\t<classifier name=\"MXML Legacy Classifier\" keys=\"WorkstationsSpecific\"/>{System.Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Event Name\" keys=\"concept:name\"/>{System.Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Resource\" keys=\"org:resource\"/>{System.Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Order\" keys=\"Order\"/>{System.Environment.NewLine}");
            sb.Append($"\t<classifier name=\"Cobot\" keys=\"org:role\"/>{System.Environment.NewLine}");

            //sb.Append($"\t<classifier name=\"WorkstationOriented\" keys=\"org:resource WorkstationCost org:role\"/>{Environment.NewLine}");
            //sb.Append($"\t<classifier name=\"WorkgroupOriented\" keys=\"org:group WorkstationGroupCosts WorkstationGroupCobots\"/>{Environment.NewLine}");

            sb.Append($"\t<string key=\"description\" value=\"Simulated process\"/>{System.Environment.NewLine}");
            sb.Append($"\t<string key=\"lifecycle:model\" value=\"standard\"/>{System.Environment.NewLine}");

            sb.Append($"\t<string key=\"Fitness cost\" value=\"{Settings.Statistics.Fitness}\"/>{System.Environment.NewLine}");
            sb.Append($"\t<string key=\"Fitness time\" value=\"{Settings.Environment.SimulationTime}\"/>{System.Environment.NewLine}");
            return sb;
        }


        /// <summary>
        /// Process mining information of on FJSSP evaluation
        /// </summary>
        public string ProcessMiningInformation
        {
            get
            {
                //=============== Not resource oriented anymore ===========================================
                //In this version the start and end events are combined to one event

                StringBuilder sb = CreateHeader();
                var listOfLists = EvaluationData.AllLogs.GroupBy(x => x.Operation.JobId).OrderBy(y => y.Key);
                foreach (var innerList in listOfLists)
                {

                    sb.Append($"\t<trace>{System.Environment.NewLine}");
                    sb.Append($"\t\t<string key=\"Job\" value=\"{innerList.Key}\"/>{System.Environment.NewLine}");
                    foreach (WorkstationLog log in innerList.OrderBy(x => x.StartTime))
                    {

                        DateTime start = Settings.Environment.TimeStart.AddSeconds(log.StartTime);
                        DateTime end = start.AddSeconds(log.EndTime);

                        sb.Append($"\t\t<event>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<string key=\"Workstation cobot assigned\" value=\"{log.Workstation.IsCobotAssigned}\"/>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<string key=\"org:resource\" value=\"{log.Workstation.Id}\"/>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<string key=\"concept:name\" value=\"{log.Operation.Id}\"/>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<string key=\"Job\" value=\"{log.Operation.JobId}\"/>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<date key=\"time:timestamp\" value=\"{end.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz")}\"/>{System.Environment.NewLine}");
                        sb.Append($"\t\t\t<date key=\"start_timestamp\" value=\"{start.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz")}\"/>{System.Environment.NewLine}");

                        sb.Append($"\t\t</event>{System.Environment.NewLine}");
                    }
                    sb.Append($"\t</trace>{System.Environment.NewLine}");
                }
                sb.Append($"</log>{System.Environment.NewLine}");
                return sb.ToString();
            }
        }



        #region constructor
        /// <summary>
        /// Default constructor => do not use
        /// </summary>
        public FjspEvaluation() => InitializeParameters();


        /// <summary>
        /// Constructor for a CSimBase, important parameters are the id in the simulation and the component name in the simulation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="settings"></param>
        public FjspEvaluation(long id, string name, SolverSettings settings) : base(id, name, settings) => InitializeParameters();
        #endregion constructor

        /// <summary>
        /// Initialize the input connection of the component
        /// </summary>
        private void InitializeParameters()
        {
            ReadData = new InEventObject(this);
        }



        /// <summary>
        /// Initialize the arrays that are assigned in the HL optimization.
        /// It is important to define the upper bound of each position of the array, so that HL can generate values in this specified range.
        /// </summary>
        public override void Initialize()
        {
            try
            {
                Data = (FlexibleJobShopSchedulingData)ReadData.Value;
                Settings.Statistics.AmountOfWorkstations = Data.NumberOfMachines;
                List<Operation> allOperations = Data.AllOperations;
                List<Operation> nonAssignedWorkSteps = Data.OperationsWhereWorkstationHasToBeDecided;

                WorkstationAssignment = new ParameterArrayOptimization<int>(nonAssignedWorkSteps.Count);
                for (int i = 0; i < nonAssignedWorkSteps.Count; i++)
                    WorkstationAssignment.UpperBounds[i] = nonAssignedWorkSteps[i].MachineProcessingTimePairs.Count;

                //OperatorSequence = new ParameterArrayOptimization<int>(allOperations.Count);
                //for (int i = 0; i < allOperations.Count; i++)
                //    OperatorSequence.UpperBounds[i] = allOperations.Count - i;
                Priority = new ParameterArrayOptimization<int>(allOperations.Count);
                for (int i = 0; i < allOperations.Count; i++)
                    Priority.UpperBounds[i] = int.MaxValue;


                if (Settings.Statistics.AmountOfCobotsPercent != 0)
                {
                    //Round up the amount of cobots. 6 workstations with 20% = 2 cobots
                    int amountOfCobots = (int)Math.Ceiling(Settings.Statistics.AmountOfCobotsPercent * Data.NumberOfMachines);
                    Settings.Statistics.AmountOfCobots = amountOfCobots;
                    CobotLocations = new ParameterArrayOptimization<int>(amountOfCobots);
                    for (int i = 0; i < amountOfCobots; i++)
                        CobotLocations.UpperBounds[i] = Data.NumberOfMachines - i;
                }
                else
                {
                    CobotLocations = new ParameterArrayOptimization<int>(0);
                }
            } catch(Exception e)
            {
                Console.WriteLine($"Error in {GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }
        }


        public override void End()
        {
            SimulationStatistics.EvaluatedSolutions += 1;
            base.End();
        }

        /// <summary>
        /// Assign heuristic lab values to the operations => workstation and priority
        /// </summary>
        public override void Start()
        {
            try
            {
                EvaluationData = new EvaluationData();
                //Generate empty workstations for the evaluation
                for (int i = 1; i <= Data.NumberOfMachines; i++)
                {
                    Workstation workstation = new Workstation();
                    workstation.Id = i;
                    EvaluationData.Workstations.Add(workstation);
                }

                foreach (Job job in Data.Jobs)
                    EvaluationData.Jobs.Add(new EvaluationJob(job));

                List<EvaluationOperation> allOperations = EvaluationData.AllOperations;
                List<EvaluationOperation> operationsWhereWorkstationHasToBeDecided =
                    EvaluationData.OperationsWhereWorkstationHasToBeDecided;

                //Assign workstation assignment from hl
                //Since the workstation is fixed by HL, we can delete other workstation/time pairs
                for (int i = 0; i < WorkstationAssignment.Value.Length; i++)
                {
                    List<MachineProcessingTimePair> productionPairs = new List<MachineProcessingTimePair>();

                    productionPairs.Add(operationsWhereWorkstationHasToBeDecided[i]
                        .MachineProcessingTimePairs[WorkstationAssignment.Value[i]]);
                    operationsWhereWorkstationHasToBeDecided[i].MachineProcessingTimePairs = productionPairs;
                }

                for (int i = 0; i < Priority.Value.Length; i++)
                {
                    allOperations[i].Priority = Priority.Value[i];
                }
                ////Assign priorities from hl
                //List<int> availablePriorities = new List<int>();
                //for (int i = 0; i < allOperations.Count; i++)
                //    availablePriorities.Add(i);
                //
                //
                //for (int i = 0; i < OperatorSequence.Value.Length; i++)
                //{
                //    int index = OperatorSequence.Value[i];
                //    allOperations[i].Priority = availablePriorities[index];
                //    availablePriorities.RemoveAt(index);
                //}

                foreach (int i in CobotLocations.Value)
                {
                    List<Workstation> workstations =
                        EvaluationData.Workstations.Where(x => !x.IsCobotAssigned).ToList();


                    if (workstations.Count == 0)
                        break;

                    if (i < workstations.Count)
                    {
                        workstations[i].IsCobotAssigned = true;
                    }
                    else
                    {
                        Console.WriteLine("Error in cobot assignment => this should not happen!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }
        }

        /// <summary>
        /// Main evaluation loop.
        /// 1: Check for finished orders.
        /// 2: Assign new operations to workstations.
        /// </summary>
        public override void DiscreteCalculation()
        {
            try
            {
                CheckForFinishedOrders();
                //CheckQueueLength();
                AssignWorkStepsToWorkstations();

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }
        }

        /// <summary>
        /// This method checks if machines are free.
        /// The operation with the highest priority (of all operations that can be produced on a free machine) is a produced next.
        /// </summary>
        private void AssignWorkStepsToWorkstations()
        {
            Dictionary<EvaluationOperation, EvaluationJob> operationsToProduce = new Dictionary<EvaluationOperation, EvaluationJob>();

            foreach (EvaluationJob job in EvaluationData.Jobs)
            {
                EvaluationOperation o = job.Operations[0];
                if (o.IsProducing)
                    continue;
                operationsToProduce.Add(o, job);
            }


            List<Workstation> emptyWorkstations = EvaluationData.Workstations.Where(x => x.CurrentlyProducing == null).ToList();
            //Iterate through all workstations and assign operations
            foreach (EvaluationOperation operation in operationsToProduce.Keys.OrderByDescending(x => x.Priority))
            {
                if (!emptyWorkstations.Any())
                    break;

                Workstation ws = emptyWorkstations.FirstOrDefault(x => x.Id == operation.MachineProcessingTimePairs[0].Machine);
                if (ws != null)
                {
                    ws.ProducingStart = Settings.Environment.SimulationTime;
                    if (ws.IsCobotAssigned)
                    {
                        //In case a cobot is assigned to the workstation we multiply the production time by 0.7 (rounded down)
                        long duration = (long)(operation.MachineProcessingTimePairs.First().ProcessingTime * 0.7);
                        if (duration == 0)
                            duration = 1;
                        ws.ProducingUntil = Settings.Environment.SimulationTime + duration;
                    }
                    else
                        //No cobot assigned, use normal production time 
                        ws.ProducingUntil = Settings.Environment.SimulationTime + operation.MachineProcessingTimePairs.First().ProcessingTime;

                    ws.CurrentlyProducingJob = operationsToProduce[operation];
                    ws.CurrentlyProducing = operation;
                    operation.IsProducing = true;
                    Settings.SimulationObjects.EventQueue.Add(this, ws.ProducingUntil);
                }
                emptyWorkstations = EvaluationData.Workstations.Where(x => x.CurrentlyProducing == null).ToList();
            }
        }


        /// <summary>
        /// Checks all workstations if a operation is finished.
        /// If all operations in a job have been finished, the job is finished.
        /// </summary>
        private void CheckForFinishedOrders()
        {
            List<EvaluationJob> changedJobs = new List<EvaluationJob>();
            //Complete operations if the simulation time is larger than the producing until time
            foreach (Workstation workstation in EvaluationData.Workstations)
            {
                if (workstation.CurrentlyProducing != null &&
                    Settings.Environment.SimulationTime >= workstation.ProducingUntil)
                {
                    EvaluationOperation o = workstation.CurrentlyProducing;
                    workstation.CurrentlyProducing = null;
                    o.IsProducing = false;
                    o.IsFinished = true;
                    workstation.CurrentlyProducingJob.Operations.Remove(o);
                    workstation.CurrentlyProducingJob.FinishedOperations.Add(o);
                    changedJobs.Add(workstation.CurrentlyProducingJob);
                    workstation.CurrentlyProducingJob = null;
                }
            }

            //Check all non finished jobs and finish them if all operations completed
            foreach (EvaluationJob job in changedJobs)
            {
                if (job.IsFinished)
                {
                    EvaluationData.Jobs.Remove(job);
                    EvaluationData.FinishedJobs.Add(job);
                }
            }

            //End simulation
            if (!EvaluationData.Jobs.Any())
            {
                Settings.Environment.SimulationFinished = true;

                SimulationStatistics.EvaluatedSolutions += 1;
                List<WorkstationLog> logs = EvaluationData.AllLogs;
                Settings.Statistics.Fitness = logs.Select(x => x.EndTime).Max();
                if (Settings.Statistics.Fitness == 0)
                    Console.WriteLine("This should not happen");
            }
        }

        public override object Clone()
        {
            FjspEvaluation result = new FjspEvaluation(Index, Name, Settings);
            result.Data = (FlexibleJobShopSchedulingData)Data.Clone();

            result.Priority = Priority.Clone();
            result.Priority.BaseObject = result;

            result.WorkstationAssignment = WorkstationAssignment.Clone();
            result.WorkstationAssignment.BaseObject = result;

            result.CobotLocations = CobotLocations.Clone();
            result.CobotLocations.BaseObject = result;

            if (ReadData.Value != null)
                result.ReadData.Value = (FlexibleJobShopSchedulingData)ReadData.Value;
            return result;
        }
    }
}
