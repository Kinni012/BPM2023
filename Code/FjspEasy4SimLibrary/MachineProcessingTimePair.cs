using System;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// A machine processing time pair describes that a operation can be produced on a specif machine in a give time
    /// </summary>
    public class MachineProcessingTimePair : ICloneable
    {
        /// <summary>
        /// Machine => read in from the problem file
        /// </summary>
        public int Machine { get; set; }
        /// <summary>
        /// Processing time on that specific machien
        /// </summary>
        public int ProcessingTime { get; set; }
        
        public object Clone()
        {
            MachineProcessingTimePair result = new MachineProcessingTimePair();
            result.Machine = Machine;
            result.ProcessingTime = ProcessingTime;
            return result;
        }

        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Machine {Machine} => Time {ProcessingTime}";
        }
    }
}
