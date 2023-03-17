using System;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class WorkstepLog : ICloneable
    {
        public long StartSetup { get; set; }
        public long EndSetup { get; set; }
        public long EndProduction { get; set; }
        public long EndDeSetup { get; set; }
        public int AmountProduced { get; set; }
        public double Cost { get; set; }
        public ConvertedWorkstation Workstation { get; set; }
        public ConvertedWorkstep Workstep { get; set; }

        public WorkstepLog(int amountProduced, ConvertedWorkstation workstation, ConvertedWorkstep workstep, long startSetup, long endSetup, long endProduction, long endDeSetup, double cost)
        {
            EndProduction = endProduction;
            AmountProduced = amountProduced;
            Workstation = workstation;
            Workstep = workstep;
            StartSetup = startSetup;
            EndSetup = endSetup;
            EndDeSetup = endDeSetup;
            Cost = cost;
        }


        public override string ToString()
        {
            return $"{Workstep.RelativeId}({StartSetup}-{EndDeSetup}): Producing {AmountProduced} on Workstation {Workstation.RelativeId}-G{Workstation.RelativeWorkgroupId} ({Workstation.WorkstationNumber}-{Workstation.WorkstationGroupNumber}){Environment.NewLine}";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
