using System;
using System.Collections.Generic;
using System.Text;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Representation of a operation in the FjspLoader
    /// </summary>
    public class Operation : ICloneable
    {
        /// <summary>
        /// Generated unique id of the operation
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Generated unique id of the parent job
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// List of all machines where the operation can be executed with the production times
        /// </summary>
        public List<MachineProcessingTimePair> MachineProcessingTimePairs
        {
            get; set;
        }


        public Operation()
        {
            MachineProcessingTimePairs = new List<MachineProcessingTimePair>();
        }

        public object Clone()
        {
            Operation result = new Operation();
            result.Id = Id;
            result.JobId = JobId;
            foreach (MachineProcessingTimePair pair in MachineProcessingTimePairs)
            {
                result.MachineProcessingTimePairs.Add((MachineProcessingTimePair)pair.Clone());
            }
            return result;
        }

        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (MachineProcessingTimePair pair in MachineProcessingTimePairs)
            {
                sb.Append("    " + pair + Environment.NewLine);

            }
            return sb.ToString();
        }
    }
}
