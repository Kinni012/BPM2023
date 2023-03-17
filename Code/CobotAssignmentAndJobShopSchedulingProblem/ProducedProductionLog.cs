using System;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ProducedProductLog : ICloneable
    {
        /// <summary>
        /// Start time of the produced task in the simulation
        /// </summary>
        public long Start { get; set; }
        /// <summary>
        /// End time of the produced task in the simulation
        /// </summary>
        public long End { get; set; }
        /// <summary>
        /// Amount that is produced
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Workstep that is currently produced
        /// </summary>
        public ConvertedWorkstep Workstep { get; set; }


        public string Comment { get; set; }

        public ProducedProductLog() { }

        public ProducedProductLog(long start, long end, int amount, ConvertedWorkstep workstep, string comment)
        {
            Start = start;
            End = end;
            Amount = amount;
            Workstep = workstep;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"{Workstep.RelativeId} => Start: {Start}, End: {End}, Amount: {Amount} ({Comment})";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
