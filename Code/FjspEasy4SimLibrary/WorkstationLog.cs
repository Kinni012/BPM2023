using System;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// This class is used to log whenever a operation is produced on a machine in the evaluation
    /// </summary>
    public class WorkstationLog : ICloneable
    {
        /// <summary>
        /// Start time of the operation
        /// </summary>
        public long StartTime { get; set; }
        /// <summary>
        /// End time of the operation
        /// </summary>
        public long EndTime { get; set; }

        /// <summary>
        /// Which operation is produced?
        /// </summary>
        public EvaluationOperation Operation { get; set; }
        /// <summary>
        /// Which job is produced?
        /// </summary>
        public EvaluationJob Job { get; set; }
        /// <summary>
        /// On what workstation this production happens?
        /// </summary>
        public Workstation Workstation { get; set; }

        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"WS{Workstation.Id}({StartTime} - {EndTime}): Operation {Operation.Id}";
        }

        public object Clone()
        {
            WorkstationLog result = new WorkstationLog();
            result.StartTime = StartTime;
            result.EndTime = EndTime;
            result.Workstation = (Workstation)Workstation.Clone();
            result.Operation = (EvaluationOperation)Operation.Clone();
            result.Job = (EvaluationJob)Job.Clone();

            return result;
        }
    }
}
