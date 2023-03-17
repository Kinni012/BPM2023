using System;
using System.Collections.Generic;

namespace DataStore
{
    public class SALBP_WeckenborgData : ICloneable
    {
        //============================ What to optimize ====================
        public int OptimizationType { get; set; }
        //==================================================================
        public int InstanceNumber { get; set; }
        public int TaskNumbers { get; set; }
        public int AmountOfWorkstations { get; set; }
        public int AmountOfCobots { get; set; }
        public int RobotFlexibility { get; set; }
        public int CooperativeFlexibility { get; set; }
        //=================== Best results found so far =========================
        public int CycleTimeWeckenborg { get; private set; }
        public int CycleTimeWeckenborgMip { get; private set; }
        public int BestCycleTimeKinast { get; set; }
        public int AverageCycleTimeKinast { get; set; }
        public int WorstCycleTimeKinast { get; set; }
        public int BestWorkstationsKinast { get; set; }
        public int BestAmountOfWorkstations { get; set; }
        public string Trickiness { get; set; }
        //=======================================================================
        public int CycleTime { get; set; }
        public int CycleTimeUpperLimit { get; set; }

        public List<SALBP_Task> Tasks { get; set; }
        public int LocalSearchMode { get; set; }

        public SALBP_WeckenborgData()
        {
            Tasks = new List<SALBP_Task>();
        }


        public SALBP_WeckenborgData(int instanceNumber, int taskNumber, int numberOfStations, int numberOfCobots, int rFlexibility,
            int cFlexibility, int bestCycleTimeWeckenborg, int bestCycleTimeWeckenborgMip)
        {
            InstanceNumber = instanceNumber;
            TaskNumbers = taskNumber;
            AmountOfWorkstations = numberOfStations;
            AmountOfCobots = numberOfCobots;
            RobotFlexibility = rFlexibility;
            CooperativeFlexibility = cFlexibility;
            CycleTimeWeckenborg = bestCycleTimeWeckenborg;
            CycleTimeWeckenborgMip = bestCycleTimeWeckenborgMip;
            Tasks = new List<SALBP_Task>();
        }

        public override string ToString()
        {
            return $"Instance: {InstanceNumber} ({Trickiness}); Tasks: {TaskNumbers}; Amount of workstations: {AmountOfWorkstations}; Amount of Cobots: {AmountOfCobots}; Best cycle time Weckenborg";
        }

        public object Clone()
        {
            SALBP_WeckenborgData result = new SALBP_WeckenborgData();
            result.InstanceNumber = InstanceNumber;
            result.TaskNumbers = TaskNumbers;
            result.AmountOfWorkstations = AmountOfWorkstations;
            result.AmountOfCobots = AmountOfCobots;
            result.RobotFlexibility = RobotFlexibility;
            result.CooperativeFlexibility = CooperativeFlexibility;
            result.CycleTimeWeckenborg = CycleTimeWeckenborg;
            result.CycleTime = CycleTime;
            result.LocalSearchMode = LocalSearchMode;
            result.BestCycleTimeKinast = BestCycleTimeKinast;
            result.BestWorkstationsKinast = BestWorkstationsKinast;
            result.CycleTimeUpperLimit = CycleTimeUpperLimit;
            result.CycleTimeWeckenborgMip = CycleTimeWeckenborgMip;
            result.OptimizationType = OptimizationType;
            result.BestAmountOfWorkstations = BestAmountOfWorkstations;
            result.Trickiness = Trickiness;
            foreach (SALBP_Task task in Tasks)
                result.Tasks.Add((SALBP_Task)task.Clone());
            return result;
        }
    }
}
