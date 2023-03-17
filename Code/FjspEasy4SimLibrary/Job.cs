using System;
using System.Collections.Generic;
using System.Text;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Simple job representation used by the FjspLoader
    /// </summary>
    public class Job : ICloneable
    {
        /// <summary>
        /// Generated unique ID of the job
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Number of operations => given in the file, not really necessary
        /// </summary>
        public int NumberOfOperations { get; set; }
        /// <summary>
        /// List of all operations read in from the problem file
        /// </summary>
        public List<Operation> Operations { get; set; }

        public Job()
        {
            Operations = new List<Operation>();
        }
        
        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Job {Id}({NumberOfOperations} operations):");

            for (int i = 0; i < Operations.Count; i++)
            {
                sb.Append($"  Operation {i}:" + Environment.NewLine);
                sb.Append(Operations[i].ToString());
            }
            return sb.ToString();
        }
        public object Clone()
        {
            Job result = new Job();
            result.Id = Id;
            foreach (Operation operation in Operations)
                result.Operations.Add((Operation)operation.Clone());

            return result;
        }
    }
}
