using System;
using System.Collections.Generic;
using System.Linq;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Representation of one operation in the evaluation
    /// </summary>
    public class EvaluationOperation : ICloneable
    {
        /// <summary>
        /// Generated ID of the Operation
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Generated ID of the Job
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// Shows if the operation is currently produced on a machine
        /// </summary>
        public bool IsProducing { get; set; }
        /// <summary>
        /// Shows if this operation is finished already
        /// </summary>
        public bool IsFinished { get; set; }
        /// <summary>
        /// Workstation where the operation should be produced on, assigned by HL
        /// </summary>
        public int WorkstationsShould { get; set; }
        /// <summary>
        /// Priority of the operation, assigned by HL
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// List of all machine processing time pairs
        /// A pair describes the processing time that is necessary on a specific machine
        /// </summary>
        public List<MachineProcessingTimePair> MachineProcessingTimePairs { get; set; }

        /// <summary>
        /// Check if this operation can be produced on a given machine
        /// </summary>
        /// <param name="workstation"></param>
        /// <returns></returns>
        public bool IsProducibleOnWorkstation(int workstation)
        {
            return MachineProcessingTimePairs.Any(x => x.Machine == workstation);
        }

        /// <summary>
        /// Return the processing time of this operation for a specific machine
        /// </summary>
        /// <param name="workstation"></param>
        /// <returns></returns>
        public MachineProcessingTimePair ProcessingTimeForMachine(int workstation)
        {
            return MachineProcessingTimePairs.FirstOrDefault(x => x.Machine == workstation);
        }

        #region ctor
        /// <summary>
        /// Default constructor that is used for the clone method
        /// Should not be used otherwise
        /// </summary>
        public EvaluationOperation()
        {
            MachineProcessingTimePairs = new List<MachineProcessingTimePair>();
        }

        /// <summary>
        /// This is the constructor that should be used
        /// A evaluation operation is based on a operation that is read in from a file
        /// </summary>
        /// <param name="operation"></param>
        public EvaluationOperation(Operation operation)
        {
            Id = operation.Id;
            JobId = operation.JobId;
            MachineProcessingTimePairs = new List<MachineProcessingTimePair>();

            foreach (MachineProcessingTimePair pair in operation.MachineProcessingTimePairs)
                MachineProcessingTimePairs.Add((MachineProcessingTimePair)pair.Clone());
        }
        #endregion ctor



        public object Clone()
        {
            EvaluationOperation result = new EvaluationOperation();
            result.Id = Id;
            result.JobId = JobId;
            result.IsProducing = IsProducing;
            result.IsFinished = IsFinished;
            result.WorkstationsShould = WorkstationsShould;
            result.Priority = Priority;
            foreach (MachineProcessingTimePair pair in MachineProcessingTimePairs)
                result.MachineProcessingTimePairs.Add((MachineProcessingTimePair)pair.Clone());

            return result;
        }
    }
}
