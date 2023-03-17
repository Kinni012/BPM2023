using System;
using System.Collections.Generic;
using System.Linq;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Representation of one job in the evaluation
    /// A job represents a sequence of operations that have to be produced in order
    /// </summary>
    public class EvaluationJob : ICloneable
    {
        /// <summary>
        /// Id of the job
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// If all operations are finished, the job is finished
        /// </summary>
        public bool IsFinished => !Operations.Any();

        /// <summary>
        /// List of all operations that have to be produced in order in the job
        /// </summary>
        public List<EvaluationOperation> Operations { get; set; }
        /// <summary>
        /// List of all operations that have to be produced in order in the job
        /// </summary>
        public List<EvaluationOperation> FinishedOperations { get; set; }

        /// <summary>
        /// Remaining operations in this job
        /// </summary>
        public int OperationsRemaining => Operations.Count;

        /// <summary>
        /// Total work remaining for all jobs on all possible machines
        /// </summary>
        public int WorkRemaining
        {
            get
            {
                int jobSum = 0;
                foreach (EvaluationOperation operation in Operations)
                    foreach (MachineProcessingTimePair pair in operation.MachineProcessingTimePairs)
                        jobSum += pair.ProcessingTime;
                return jobSum;
            }
        }

        /// <summary>
        /// Sum of all production times of operations already finished in this job
        /// </summary>
        /// <param name="workstations"></param>
        /// <returns></returns>
        public long PreviousProcessingTimes(List<Workstation> workstations)
        {
            long sum = 0;
            foreach (EvaluationOperation operation in FinishedOperations)
            {
                foreach (Workstation workstation in workstations)
                {
                    WorkstationLog log = workstation.Logs.FirstOrDefault(x => x.Operation.Id == operation.Id);
                    if (log != null)
                    {
                        sum += (log.EndTime - log.StartTime);
                        break;
                    }
                }
            }

            return sum;
        }

        /// <summary>
        /// Maximum end time of all previous finished operations
        /// </summary>
        /// <param name="workstations"></param>
        /// <returns></returns>
        public long MaxEndTimePreviousTasks(List<Workstation> workstations)
        {
            long maxEndtime = 0;
            foreach (EvaluationOperation operation in FinishedOperations)
            {
                foreach (Workstation workstation in workstations)
                {
                    WorkstationLog log = workstation.Logs.FirstOrDefault(x => x.Operation.Id == operation.Id);
                    if (log != null && log.EndTime > maxEndtime)
                    {
                        maxEndtime = log.EndTime;
                        break;
                    }
                }
            }

            return maxEndtime;
        }




        #region ctor
        /// <summary>
        /// Default constructor for the clone method, should not be sued otherwise
        /// </summary>
        public EvaluationJob()
        {
            Operations = new List<EvaluationOperation>();
            FinishedOperations = new List<EvaluationOperation>();
        }

        /// <summary>
        /// Constructor with a job parameter, this is the constructor that should be used in the evaluation
        /// </summary>
        /// <param name="job"></param>
        public EvaluationJob(Job job)
        {
            Operations = new List<EvaluationOperation>();
            FinishedOperations = new List<EvaluationOperation>();
            Id = job.Id;
            foreach (Operation o in job.Operations)
                Operations.Add(new EvaluationOperation(o));
        }
        #endregion ctor

        /// <summary>
        /// Simple string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Finished: {FinishedOperations.Count}, Operations: {Operations.Count}";
        }

        public object Clone()
        {
            EvaluationJob result = new EvaluationJob();
            result.Id = Id;
            foreach (EvaluationOperation operation in Operations)
                result.Operations.Add(operation);

            foreach (EvaluationOperation operation in FinishedOperations)
                result.FinishedOperations.Add(operation);

            return result;
        }
    }
}
