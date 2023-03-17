using System;
using System.Collections.Generic;
using System.Linq;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// A copy of the read in fjssp data with additional information that is necessary for the Easy4Sim evaluation
    /// </summary>
    public class EvaluationData : ICloneable
    {
        /// <summary>
        /// List of available workstations
        /// </summary>
        public List<Workstation> Workstations;

        /// <summary>
        /// List of Jobs that have to be produced
        /// </summary>
        public List<EvaluationJob> Jobs;

        /// <summary>
        /// List of Jobs that have been finished
        /// </summary>
        public List<EvaluationJob> FinishedJobs;

        /// <summary>
        /// Logs of all workstations in increasing start time order
        /// </summary>
        public List<WorkstationLog> AllLogs
        {
            get
            {
                List<WorkstationLog> result = new List<WorkstationLog>();
                foreach (Workstation ws in Workstations)
                {
                    result.AddRange(ws.Logs);
                }
                return result.OrderBy(x => x.StartTime).ToList();
            }
        }

        /// <summary>
        /// Only operations with more than one possible workstations need to be encoded
        /// This method returns all operations that have more than one possible workstation
        /// </summary>
        public List<EvaluationOperation> OperationsWhereWorkstationHasToBeDecided
        {
            get
            {
                List<EvaluationOperation> result = new List<EvaluationOperation>();
                foreach (EvaluationJob j in Jobs)
                    foreach (EvaluationOperation o in j.Operations)
                        if (o.MachineProcessingTimePairs.Count > 1) //More than one machine possible to execute one operation
                            result.Add(o);
                return result;
            }
        }

        /// <summary>
        /// This method returns all operations of all jobs
        /// </summary>
        public List<EvaluationOperation> AllOperations
        {
            get
            {
                List<EvaluationOperation> result = new List<EvaluationOperation>();
                foreach (EvaluationJob j in Jobs)
                    foreach (EvaluationOperation o in j.Operations)
                        result.Add(o);
                return result;
            }
        }

        #region ctor
        public EvaluationData()
        {
            Workstations = new List<Workstation>();
            Jobs = new List<EvaluationJob>();
            FinishedJobs = new List<EvaluationJob>();
        }
        #endregion ctor

        public object Clone()
        {
            EvaluationData result = new EvaluationData();
            foreach (Workstation ws  in Workstations)
                result.Workstations.Add((Workstation)ws.Clone());

            foreach (EvaluationJob job in Jobs)
                result.Jobs.Add((EvaluationJob)job.Clone());

            foreach (EvaluationJob job in FinishedJobs)
                result.FinishedJobs.Add((EvaluationJob)job.Clone());


            return result;
        }
    }
}
