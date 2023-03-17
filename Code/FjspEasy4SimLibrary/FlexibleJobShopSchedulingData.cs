using System;
using System.Collections.Generic;
using System.Text;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Representation of a Fjssp read by the FjspLoader
    /// </summary>
    public class FlexibleJobShopSchedulingData : ICloneable
    {
        /// <summary>
        /// Number of jobs that need to be finished => given in the problem file
        /// </summary>
        public int NumberOfJobs { get; set; }
        /// <summary>
        /// Number of machines  => given in the problem file
        /// </summary>
        public int NumberOfMachines { get; set; }
        /// <summary>
        /// Average number of machines per job => given in the problem file
        /// </summary>
        public int AverageNumberOfMachinesPerJob { get; set; }

        /// <summary>
        /// List of read jobs, each job has a list of operations that need to be finished in order
        /// </summary>
        public List<Job> Jobs;

        public FlexibleJobShopSchedulingData() => Jobs = new List<Job>();

        /// <summary>
        /// Return all operations with more than one possible machine
        /// </summary>
        public List<Operation> OperationsWhereWorkstationHasToBeDecided
        {
            get
            {
                List<Operation> result = new List<Operation>();
                foreach (Job j in Jobs)
                    foreach (Operation o in j.Operations)
                        if (o.MachineProcessingTimePairs.Count > 1) //More than one machine possible to execute one operation
                            result.Add(o);
                return result;
            }
        }

        /// <summary>
        /// Return all operations 
        /// </summary>
        public List<Operation> AllOperations
        {
            get
            {
                List<Operation> result = new List<Operation>();
                foreach (Job j in Jobs)
                    foreach (Operation o in j.Operations)
                        result.Add(o);
                return result;
            }
        }

        public object Clone()
        {
            FlexibleJobShopSchedulingData result = new FlexibleJobShopSchedulingData();
            result.NumberOfJobs = NumberOfJobs;
            result.NumberOfMachines = NumberOfMachines;
            result.AverageNumberOfMachinesPerJob = AverageNumberOfMachinesPerJob;
            foreach (Job job in Jobs)
                result.Jobs.Add((Job)job.Clone());
            
            return result;
        }

        /// <summary>
        /// Simple string representation for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Job job in Jobs)
                sb.Append(job + Environment.NewLine);

            return sb.ToString();
        }
    }
}
