using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Easy4SimFramework;
using HeuristicLab.Core;

namespace FjspEasy4SimLibrary
{
    public class FjspPriorityRuleSolutionGenerator : CSimBase
    {

        /// <summary>
        /// Delegate for methods that place the cobots on workstations
        /// </summary>
        public delegate ParameterArrayOptimization<int> CobotAssignmentDelegate(List<Workstation> workstations, List<EvaluationJob> jobs, int amountOfCobots, IRandom random);
        /// <summary>
        /// Delegate for methods that are used as priority rules 
        /// </summary>
        /// <param name="workstation"></param>
        /// <param name="allWorkstations"></param>
        /// <param name="operationsAndJobs"></param>
        /// <returns></returns>
        public delegate EvaluationOperation PriorityRuleDelegate(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsAndJobs, List<EvaluationJob> allJobs, SolverSettings settings);

        /// <summary>
        /// List of all available priority rules
        /// </summary>
        public static List<PriorityRuleDelegate> PriorityRules { get; set; } = new List<PriorityRuleDelegate>();
        /// <summary>
        /// List of all available priority rules
        /// </summary>
        public static List<CobotAssignmentDelegate> CobotRules { get; set; } = new List<CobotAssignmentDelegate>();

        /// <summary>
        /// Read data from the problem file in a structured format
        /// </summary>
        public FlexibleJobShopSchedulingData Data;
        /// <summary>
        /// Data from the evaluation
        /// </summary>
        public EvaluationData EvaluationData;


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
        /// Read data  that is filled by the FjspLoader
        /// </summary>
        public InEventObject ReadData { get; set; }

        /// <summary>
        /// Amount of cobots that should be used for the priority rule generated solution
        /// </summary>
        public ParameterReal AmountOfCobots { get; set; }

        /// <summary>
        /// SelectedRuleOutOfAllPossibleRules
        /// </summary>
        public int Rule { get; set; }

        /// <summary>
        /// SelectedRuleOutOfAllPossibleRules
        /// </summary>
        public int CobotRule { get; set; }
        public IRandom Random { get; set; }

        private List<EvaluationOperation> _allOperations;

        private List<Operation> _nonAssignedWorkSteps;


        #region constructor

        static FjspPriorityRuleSolutionGenerator()
        {
            PriorityRules.Add(MostWorkRemaining);
            PriorityRules.Add(ShortestProcessingTime);
            PriorityRules.Add(MostOperationsRemaining);
            PriorityRules.Add(OperationalFlowSlackPerProcessingTime);
            PriorityRules.Add(FlowDueDatePerWorkRemaining);
            PriorityRules.Add(ShortestProcessingTimePerWorkRemaining);
            PriorityRules.Add(ShortestProcessingTimeAndWorkInNextQueue);

            CobotRules.Add(CobotToMostAssignableTasks);
            CobotRules.Add(CobotToMostWorkTasks);
            //CobotRules.Add(CobotToRandom);
        }
        /// <summary>
        /// Do not use the default constructor
        /// </summary>
        public FjspPriorityRuleSolutionGenerator() => InitializeParameters();


        /// <summary>
        /// Constructor for the simulation framework
        /// </summary>
        /// <param name="id">Id in the simulation</param>
        /// <param name="name">Unique name in the simulation</param>
        /// <param name="settings">Settings where the object is inserted to</param>
        public FjspPriorityRuleSolutionGenerator(long id, string name, SolverSettings settings) : base(id, name, settings) => InitializeParameters();
        #endregion constructor

        public static ParameterArrayOptimization<int> CobotToRandom(List<Workstation> workstations, List<EvaluationJob> jobs, int amountOfCobots, IRandom random)
        {
            ParameterArrayOptimization<int> result = new ParameterArrayOptimization<int>(amountOfCobots);
            for (int i = 0; i < amountOfCobots; i++)
            {
                List<Workstation> workstationsWithoutCobots = workstations.Where(x => !x.IsCobotAssigned).ToList();
                int index = random.Next(workstationsWithoutCobots.Count - i);
                result.Value[i] = index;
            }
            return result;
        }


        public static ParameterArrayOptimization<int> CobotToMostAssignableTasks(List<Workstation> workstations, List<EvaluationJob> jobs, int amountOfCobots, IRandom random)
        {
            ParameterArrayOptimization<int> result = new ParameterArrayOptimization<int>(amountOfCobots);
            List<Workstation> workstationsWithoutCobots = workstations.Where(x => !x.IsCobotAssigned).ToList();
            for (int i = 0; i < amountOfCobots; i++)
            {
                int maxCounter = 0;
                Workstation toAssignCobot = null;

                foreach (Workstation workstation in workstationsWithoutCobots)
                {
                    int counter = 0;
                    foreach (EvaluationJob job in jobs)
                        foreach (EvaluationOperation operation in job.Operations)
                            foreach (MachineProcessingTimePair pair in operation.MachineProcessingTimePairs)
                                if (pair.Machine == workstation.Id)
                                    counter++;

                    if (counter > maxCounter)
                    {
                        toAssignCobot = workstation;
                        maxCounter = counter;
                    }
                }

                result.Value[i] = workstationsWithoutCobots.IndexOf(toAssignCobot);
                workstationsWithoutCobots.Remove(toAssignCobot);
            }

            return result;
        }


        public static ParameterArrayOptimization<int> CobotToMostWorkTasks(List<Workstation> workstations, List<EvaluationJob> jobs, int amountOfCobots, IRandom random)
        {
            List<Workstation> workstationsWithoutCobots = workstations.Where(x => !x.IsCobotAssigned).ToList();
            ParameterArrayOptimization<int> result = new ParameterArrayOptimization<int>(amountOfCobots);
            for (int i = 0; i < amountOfCobots; i++)
            {
                int maxSum = 0;
                Workstation toAssignCobot = null;

                foreach (Workstation workstation in workstationsWithoutCobots)
                {
                    int sum = 0;
                    foreach (EvaluationJob job in jobs)
                        foreach (EvaluationOperation operation in job.Operations)
                            foreach (MachineProcessingTimePair pair in operation.MachineProcessingTimePairs)
                                if (pair.Machine == workstation.Id)
                                    sum += pair.ProcessingTime;

                    if (sum > maxSum)
                    {
                        toAssignCobot = workstation;
                        maxSum = sum;
                    }
                }

                result.Value[i] = workstationsWithoutCobots.IndexOf(toAssignCobot);
                workstationsWithoutCobots.Remove(toAssignCobot);
            }

            return result;
        }

        public static EvaluationOperation OperationalFlowSlackPerProcessingTime(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            long sum = 0;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }

                long time = settings.Environment.SimulationTime;
                long processingTimeOfNextOperation = value.Key.ProcessingTimeForMachine(workstation.Id).ProcessingTime;
                long arrivalTimeOfJobOnShopFloor = 0;
                long previousProcessingTimes = value.Value.PreviousProcessingTimes(allWorkstations);


                long operationalFlowSlackPerProcessingTime = Math.Max((time + processingTimeOfNextOperation) - (arrivalTimeOfJobOnShopFloor + previousProcessingTimes), 0) / processingTimeOfNextOperation;

                if (operationalFlowSlackPerProcessingTime > sum)
                {
                    sum = operationalFlowSlackPerProcessingTime;
                    result = value.Key;
                }
            }
            return result;
        }


        public static EvaluationOperation MostOperationsRemaining(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            int sum = 0;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }

                if (value.Value.OperationsRemaining > sum)
                {
                    sum = value.Value.OperationsRemaining;
                    result = value.Key;
                }
            }
            return result;
        }

        public static EvaluationOperation FlowDueDatePerWorkRemaining(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            long sum = long.MaxValue;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }

                long arrivalTimeOfJobOnShopFloor = 0;
                long previousProcessingTimes = value.Value.PreviousProcessingTimes(allWorkstations);
                long workRemaining = value.Value.WorkRemaining;

                long flowDueDatePerWorkRemaining= (arrivalTimeOfJobOnShopFloor + previousProcessingTimes) / workRemaining;
                if(flowDueDatePerWorkRemaining < sum || result == null)
                {
                    sum = flowDueDatePerWorkRemaining;
                    result = value.Key;
                }
            }
            return result;
        }
        public static EvaluationOperation ShortestProcessingTime(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            int sum = int.MaxValue;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }

                if (value.Key.ProcessingTimeForMachine(workstation.Id).ProcessingTime < sum)
                {
                    sum = value.Key.ProcessingTimeForMachine(workstation.Id).ProcessingTime;
                    result = value.Key;
                }
            }
            return result;
        }
        public static EvaluationOperation ShortestProcessingTimePerWorkRemaining(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            long sum = int.MaxValue;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }
                long processingTime = value.Key.ProcessingTimeForMachine(workstation.Id).ProcessingTime;
                long workRemaining = value.Value.WorkRemaining;


                if (processingTime + workRemaining < sum)
                {
                    sum = processingTime + workRemaining;
                    result = value.Key;
                }
            }
            return result;
        }
        public static EvaluationOperation MostWorkRemaining(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            int sum = 0;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }
                if (value.Value.WorkRemaining > sum)
                {
                    sum = value.Value.WorkRemaining;
                    result = value.Key;
                }
            }
            return result;
        }
        public static EvaluationOperation ShortestProcessingTimeAndWorkInNextQueue(Workstation workstation, List<Workstation> allWorkstations, Dictionary<EvaluationOperation, EvaluationJob> operationsJobs, List<EvaluationJob> allJobs, SolverSettings settings)
        {
            EvaluationOperation result = null;
            long minVal = int.MaxValue;
            foreach (KeyValuePair<EvaluationOperation, EvaluationJob> value in operationsJobs)
            {
                if (!value.Key.IsProducibleOnWorkstation(workstation.Id))
                {
                    continue;
                }

                long processingTime = value.Key.ProcessingTimeForMachine(workstation.Id).ProcessingTime;
                long workInNextQueue = CalculateWorkInNextQueue(value, allJobs);


                if (processingTime + workInNextQueue < minVal)
                {
                    minVal = processingTime + workInNextQueue;
                    result = value.Key;
                }
            }
            return result;
        }

        private static long CalculateWorkInNextQueue(KeyValuePair<EvaluationOperation, EvaluationJob> pair, List<EvaluationJob> allJobs)
        {
            if (pair.Value.Operations.Count < 2)
                return 0;

            EvaluationOperation operation = pair.Value.Operations[1];

            Dictionary<int, int> machineQueues = new Dictionary<int, int>();
            foreach (MachineProcessingTimePair machinePair in operation.MachineProcessingTimePairs)
                machineQueues.Add(machinePair.Machine, 0);

            foreach (EvaluationJob job in allJobs)
            {
                EvaluationOperation op = job.Operations.FirstOrDefault();
                if (op == null)
                    continue;
                if (op.IsProducing)
                    continue;
                foreach (MachineProcessingTimePair p in op.MachineProcessingTimePairs)
                {
                    if (machineQueues.ContainsKey(p.Machine))
                    {
                        machineQueues[p.Machine] += p.ProcessingTime;
                    }
                }    
            }

            return machineQueues.Values.Sum();
        }



        /// <summary>
        /// Define bounds of the encoding
        /// </summary>
        public override void Initialize()
        {
            try
            {
                Data = (FlexibleJobShopSchedulingData)ReadData.Value;
                Settings.Statistics.AmountOfWorkstations = Data.NumberOfMachines;
                List<Operation> allOperations = Data.AllOperations;
                _nonAssignedWorkSteps = Data.OperationsWhereWorkstationHasToBeDecided;

                WorkstationAssignment = new ParameterArrayOptimization<int>(_nonAssignedWorkSteps.Count);
                for (int i = 0; i < _nonAssignedWorkSteps.Count; i++)
                    WorkstationAssignment.UpperBounds[i] = _nonAssignedWorkSteps[i].MachineProcessingTimePairs.Count;

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
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }
        }

        /// <summary>
        /// Assign cobots based on the selected priority rule
        /// </summary>
        public override void Start()
        {
            try
            {
                EvaluationData = new EvaluationData();
                //Generate empty workstations for the evaluation
                for (int i = 1; i <= Data.NumberOfMachines; i++)
                {
                    Workstation workstation = new Workstation
                    {
                        Id = i
                    };
                    EvaluationData.Workstations.Add(workstation);
                }

                foreach (Job job in Data.Jobs)
                    EvaluationData.Jobs.Add(new EvaluationJob(job));

                List<EvaluationOperation> allOperations = EvaluationData.AllOperations;
                _allOperations = allOperations;
                CobotLocations = CobotRules[CobotRule].Invoke(EvaluationData.Workstations,
                    EvaluationData.Jobs, Settings.Statistics.AmountOfCobots, Random);
                
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


        public override void DiscreteCalculation()
        {
            try
            {
                CheckForFinishedOrders();
                ApplyPriorityRule();

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                throw;
            }

        }

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

        private void ApplyPriorityRule()
        {

            Dictionary<EvaluationOperation, EvaluationJob> operationsToProduce = new Dictionary<EvaluationOperation, EvaluationJob>();

            foreach (EvaluationJob job in EvaluationData.Jobs)
            {
                EvaluationOperation o = job.Operations[0];
                if (o.IsProducing)
                    continue;
                operationsToProduce.Add(o, job);
            }

            foreach (Workstation workstation in EvaluationData.Workstations)
            {
                if (workstation.CurrentlyProducing != null)
                    continue;

                if (operationsToProduce.Count == 0)
                    break;


                EvaluationOperation produceNext = PriorityRules[Rule].Invoke(workstation, EvaluationData.Workstations, operationsToProduce, EvaluationData.Jobs, Settings);
                if (produceNext == null)
                    continue;
                Operation o = _nonAssignedWorkSteps.FirstOrDefault(x => x.Id == produceNext.Id);
                int solutionIndex = _nonAssignedWorkSteps.IndexOf(o);
                if (solutionIndex != -1)
                {
                    MachineProcessingTimePair pair =
                        produceNext.MachineProcessingTimePairs.FirstOrDefault(x => x.Machine == workstation.Id);
                    int solutionEncoding = produceNext.MachineProcessingTimePairs.IndexOf(pair);
                    int upperBound = WorkstationAssignment.UpperBounds[solutionIndex];
                    if (upperBound == solutionEncoding)
                    {
                        Console.WriteLine($"Error in {this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
                    }
                    WorkstationAssignment.Value[solutionIndex] = solutionEncoding;
                }
                
                workstation.ProducingStart = Settings.Environment.SimulationTime;
                if (workstation.IsCobotAssigned)
                {
                    //In case a cobot is assigned to the workstation we multiply the production time by 0.7 (rounded down)
                    long duration = (long)(produceNext.MachineProcessingTimePairs.First().ProcessingTime * 0.7);
                    if (duration == 0)
                        duration = 1;
                    workstation.ProducingUntil = Settings.Environment.SimulationTime + duration;
                }
                else
                    //No cobot assigned, use normal production time 
                    workstation.ProducingUntil = Settings.Environment.SimulationTime + produceNext.MachineProcessingTimePairs.First().ProcessingTime;
                workstation.CurrentlyProducingJob = operationsToProduce[produceNext];
                workstation.CurrentlyProducing = produceNext;
                operationsToProduce.Remove(produceNext);
                produceNext.IsProducing = true;
                Settings.SimulationObjects.EventQueue.Add(this, workstation.ProducingUntil);
            }

        }

        public override void End()
        {
            
            foreach (Workstation workstation in EvaluationData.Workstations)
            {
                int numberOfLogs = workstation.Logs.Count;
                int currentLog = 0;

                foreach (WorkstationLog log in workstation.Logs.OrderBy(x => x.StartTime))
                {
                    int index = _allOperations.IndexOf(log.Operation);
                    int upperBound = Priority.UpperBounds[index] - 1;
                    double factor = currentLog / (double) numberOfLogs;
                    int value = upperBound - (int)(upperBound * factor);
                    Priority.Value[index] = value;
                    currentLog++;
                }
            }


            SimulationStatistics.EvaluatedSolutions += 1;
            base.End();
        }

        private void InitializeParameters()
        {
            ReadData = new InEventObject(this);
        }
        public override object Clone()
        {
            FjspPriorityRuleSolutionGenerator result = new FjspPriorityRuleSolutionGenerator();
            result.AmountOfCobots = AmountOfCobots;
            result.CobotRule = CobotRule;
            result.Rule = Rule;
            return result;
        }
    }
}
