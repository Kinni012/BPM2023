using System;
using System.Collections.Generic;
using System.Text;

namespace DataStore
{
    public class SALBP_Task : ICloneable, IEquatable<SALBP_Task>
    {
        public int TaskNumber { get; set; }
        public double Priority { get; set; }
        public double ProducedBy { get; set; }
        public int HumanProductionTime { get; set; }
        public int RobotProductionTime { get; set; }
        public string ProductionTypeInformation { get; set; }
        public int CollaborativeProductionTime { get; set; }
        public List<int> PreviousTasks { get; set; } = new List<int>();
        public List<int> NextTasks { get; set; } = new List<int>();
        public SALBP_Task() { }

        public bool CanBeDoneByRobot
        {
            get
            {
                if (RobotProductionTime != 0)
                    return true;
                return false;
            }
        }
        public bool CanBeDoneCooperative
        {
            get
            {
                if (CollaborativeProductionTime != 0)
                    return true;
                return false;
            }
        }
        public void SetProducedBy(int productionTypeFromTask)
        {
            switch (productionTypeFromTask)
            {
                case 1:
                    ProductionTypeInformation = "Worker";
                    break;
                case 2:
                    ProductionTypeInformation = "Cooperative";
                    break;
                case 3:
                    ProductionTypeInformation = "Robot";
                    break;
            }
        }
        public string ProductionTypeAsLetter
        {
            get
            {
                if (ProductionTypeInformation == "Robot")
                    return "R";
                if (ProductionTypeInformation == "Cooperative")
                    return "C";
                return "";
            }
        }

        public int AmountOfProductionMods
        {
            get
            {
                int result = 1;
                if (CanBeDoneByRobot)
                    result++;
                if (CanBeDoneCooperative)
                    result++;
                return result;
            }
        }

        public int ProductionType()
        {
            if (!(CanBeDoneByRobot || CanBeDoneCooperative))
                return 1;
            List<int> productionTypes = new List<int>() { };
            productionTypes.Add(1);
            if (CanBeDoneCooperative)
                productionTypes.Add(2);
            if (CanBeDoneByRobot)
                productionTypes.Add(3);
            int index = (int)(ProducedBy * productionTypes.Count);
            return productionTypes[index];
        }

        public object Clone()
        {
            SALBP_Task result = new SALBP_Task();
            result.TaskNumber = TaskNumber;
            result.Priority = Priority;
            result.ProducedBy = ProducedBy;
            result.ProductionTypeInformation = ProductionTypeInformation;
            result.HumanProductionTime = HumanProductionTime;
            result.RobotProductionTime = RobotProductionTime;
            result.CollaborativeProductionTime = CollaborativeProductionTime;
            result.PreviousTasks = new List<int>();
            foreach (int i in PreviousTasks)
                result.PreviousTasks.Add(i);

            result.NextTasks = new List<int>();
            foreach (int i in NextTasks)
                result.NextTasks.Add(i);
            return result;
        }

        public bool Equals(SALBP_Task other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TaskNumber == other.TaskNumber &&
                   Priority.Equals(other.Priority) &&
                   ProducedBy.Equals(other.ProducedBy) &&
                   HumanProductionTime == other.HumanProductionTime &&
                   RobotProductionTime == other.RobotProductionTime &&
                   ProductionTypeInformation == other.ProductionTypeInformation &&
                   CollaborativeProductionTime == other.CollaborativeProductionTime &&
                   Equals(PreviousTasks, other.PreviousTasks) &&
                   Equals(NextTasks, other.NextTasks);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SALBP_Task)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TaskNumber;
                hashCode = (hashCode * 397) ^ Priority.GetHashCode();
                hashCode = (hashCode * 397) ^ ProducedBy.GetHashCode();
                hashCode = (hashCode * 397) ^ HumanProductionTime;
                hashCode = (hashCode * 397) ^ RobotProductionTime;
                hashCode = (hashCode * 397) ^ (ProductionTypeInformation != null ? ProductionTypeInformation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ CollaborativeProductionTime;
                hashCode = (hashCode * 397) ^ (PreviousTasks != null ? PreviousTasks.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NextTasks != null ? NextTasks.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"{TaskNumber}: H:{HumanProductionTime}");
            if (CanBeDoneByRobot)
                sb.Append($" R:{RobotProductionTime}");
            if (CanBeDoneCooperative)
                sb.Append($" C:{CanBeDoneCooperative}");
            return sb.ToString();
        }
    }
}
