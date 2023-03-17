
using System;
using System.Collections.Generic;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Represents one workstation in the evaluation
    /// </summary>
    public class Workstation : ICloneable
    {
        private EvaluationOperation _currentlyProducing;

        /// <summary>
        /// List of logs, from operations that are produced on a specific workstation
        /// </summary>
        public List<WorkstationLog> Logs = new List<WorkstationLog>();
        

        /// <summary>
        /// Unique id of the workstation
        /// </summary>
        public int Id { get; set; }


        /// <summary>
        /// Operation that is currently produced on the machine.
        /// In case a new operation is assigned a log is generated automatically.
        /// </summary>
        public EvaluationOperation CurrentlyProducing
        {
            get => _currentlyProducing;
            set
            {
                _currentlyProducing = value;
                if(value != null)
                {
                    WorkstationLog log = new WorkstationLog();
                    log.Operation = value;
                    log.Workstation = this;
                    log.Job = CurrentlyProducingJob;
                    log.StartTime = ProducingStart;
                    log.EndTime = ProducingUntil;
                    Logs.Add(log);
                }
            }
        }

        /// <summary>
        /// End time of the currently producing operation
        /// </summary>
        public long ProducingUntil { get; set; }
        /// <summary>
        /// Start time of the currently producing operation
        /// </summary>
        public long ProducingStart { get; internal set; }
        /// <summary>
        /// Is a cobot assigned to this workstation
        /// </summary>
        public bool IsCobotAssigned { get; internal set; }

        public EvaluationJob CurrentlyProducingJob { get; set; }
        
        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Workstation {Id}";
        }
        public object Clone()
        {
            Workstation ws = new Workstation();
            ws.Id = Id;
            ws.CurrentlyProducing = (EvaluationOperation)CurrentlyProducing.Clone();
            ws.ProducingUntil = ProducingUntil;
            return ws;
        }
    }
}
