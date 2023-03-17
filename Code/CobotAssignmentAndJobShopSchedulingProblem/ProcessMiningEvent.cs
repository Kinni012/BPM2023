using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ProcessMiningEvent : ICloneable
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string WorkStepNumber { get; set; }
        public double Cost { get; set; }
        //=================== Workstation properties =========================
        public string WorkStationId { get; set; }
        public double WorkstationCost { get; set; }
        public bool CobotAssigned { get; set; }
        public double WorkstationProductionTime { get; set; }

        // ================== Group properties ===============================
        public string WorkstationGroup { get; set; }
        public double WorkstationGroupCost { get; set; }
        public string WorkstationGroupCobots { get; set; }
        //=====================================================================


        public string Activity { get; set; }
        public long Duration { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Quantity { get; set; }
        public string ProductionType { get; set; }
        public ProcessMiningEvent() { }

        public ProcessMiningEvent(string orderNumber, string workStepNumber, string workStationId, string activity,
            double cost, DateTime timeStamp, int quantity, string productionType, string workstationGroup, bool cobotAssigned, long duration, Guid id)
        {
            OrderNumber = orderNumber;
            WorkStepNumber = workStepNumber;
            WorkStationId = workStationId;
            WorkstationGroup = workstationGroup;
            Activity = activity;
            Cost = cost;
            TimeStamp = timeStamp;
            Quantity = quantity;
            ProductionType = productionType;
            CobotAssigned = cobotAssigned;
            Duration = duration;
            Id = id;

        }
        public object Clone()
        {
            ProcessMiningEvent result = new ProcessMiningEvent();
            result.OrderNumber = OrderNumber;
            result.WorkStepNumber = WorkStepNumber;
            result.WorkStationId = WorkStationId;
            result.Activity = Activity;
            result.Cost = Cost;
            result.TimeStamp = TimeStamp;
            result.Quantity = Quantity;
            result.ProductionType = ProductionType;
            result.WorkstationGroup = WorkstationGroup;
            result.CobotAssigned = CobotAssigned;
            result.Duration = Duration;
            result.Id = Id;
            result.WorkstationProductionTime = WorkstationProductionTime;
            return result;
        }

        public static string Headers
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(nameof(OrderNumber) + ";");
                sb.Append(nameof(WorkStepNumber) + ";");
                sb.Append(nameof(WorkStationId) + ";");
                sb.Append(nameof(WorkstationGroup) + ";");
                sb.Append(nameof(CobotAssigned) + ";");
                sb.Append(nameof(Duration) + ";");
                sb.Append(nameof(Activity) + ";");
                sb.Append(nameof(Cost) + ";");
                sb.Append(nameof(TimeStamp) + ";");
                sb.Append(nameof(Quantity) + ";");
                sb.Append(nameof(ProductionType));
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
        }



        public string XesString(int intend, List<ConvertedWorkstation> convertedDataWorkstations)
        {
            string intendString = "";
            for (int i = 0; i < intend; i++)
            {
                intendString += "\t";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"{intendString}<event>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"id:Id\" value=\"{Id.ToString()}\"/>{Environment.NewLine}");

            sb.Append($"{intendString}\t<string key=\"org:group\" value=\"{WorkstationGroup}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkstationGroupCobots\" value=\"{WorkstationGroupCobots}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkstationGroupCosts\" value=\"{Math.Round(WorkstationGroupCost, 2)}\"/>{Environment.NewLine}");


            sb.Append($"{intendString}\t<string key=\"org:resource\" value=\"{WorkStationId}\"/>{Environment.NewLine}");
            string cobotAssignedToWorkstation = "Human";
            if (CobotAssigned)
                cobotAssignedToWorkstation = "Human and Cobot";

            sb.Append($"{intendString}\t<string key=\"org:role\" value=\"{cobotAssignedToWorkstation}\"/>{Environment.NewLine}");

            sb.Append($"{intendString}\t<string key=\"WorkstationCost\" value=\"{Math.Round(WorkstationCost, 2)}\"/>{Environment.NewLine}");
            ConvertedWorkstation workstation = convertedDataWorkstations.Where(x => x.WorkstationNumber == WorkStationId)
                .FirstOrDefault();
            if (workstation != null)
            {

                sb.Append($"{intendString}\t<string key=\"WorkstationIdleTime\" value=\"{workstation.IdleTime}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationProductionTime\" value=\"{WorkstationProductionTime}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationQueue\" value=\"{workstation.LongestQueue}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationBlockingTime\" value=\"{workstation.BlockingTime}\"/>{Environment.NewLine}");
            }

            sb.Append($"{intendString}\t<string key=\"cost:total\" value=\"{Cost}\"/>{Environment.NewLine}");


            sb.Append($"{intendString}\t<string key=\"lifecycle:transition\" value=\"{Activity}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<date key=\"time:timestamp\" value=\"{TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz")}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Duration\" value=\"{Duration}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Quantity\" value=\"{Quantity}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"ProductionType\" value=\"{ProductionType}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"concept:name\" value=\"{WorkStepNumber}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Order\" value=\"{OrderNumber}\"/>{Environment.NewLine}");
            //======================================== Classifier events ============================================
            //Used if the discovery algorithm can not handle classifiers
            sb.Append($"{intendString}\t<string key=\"WorkstationsSpecific\" value=\"{WorkStationId} {cobotAssignedToWorkstation} {Math.Round(WorkstationCost, 2)}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkGroupSpecific\" value=\"{WorkstationGroup} {WorkstationGroupCobots} {Math.Round(WorkstationGroupCost, 2)}\"/>{Environment.NewLine}");

            //=======================================================================================================
            sb.Append($"{intendString}</event>{Environment.NewLine}");
            return sb.ToString();
        }


        public string XesStringVersionResourceGrouping(int intend, DateTime startTime,
            List<ConvertedWorkstation> convertedDataWorkstations)
        {
            string intendString = "";
            for (int i = 0; i < intend; i++)
            {
                intendString += "\t";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"{intendString}<event>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"id:Id\" value=\"{Id.ToString()}\"/>{Environment.NewLine}");

            sb.Append($"{intendString}\t<string key=\"org:group\" value=\"{WorkstationGroup}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkstationGroupCobots\" value=\"{WorkstationGroupCobots}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkstationGroupCosts\" value=\"{Math.Round(WorkstationGroupCost, 2)}\"/>{Environment.NewLine}");


            sb.Append($"{intendString}\t<string key=\"org:resource\" value=\"{WorkStationId}\"/>{Environment.NewLine}");
            string cobotAssignedToWorkstation = "Human";
            if (CobotAssigned)
                cobotAssignedToWorkstation = "Human and Cobot";

            sb.Append($"{intendString}\t<string key=\"org:role\" value=\"{cobotAssignedToWorkstation}\"/>{Environment.NewLine}");

            sb.Append($"{intendString}\t<string key=\"WorkstationCost\" value=\"{Math.Round(WorkstationCost, 2)}\"/>{Environment.NewLine}");
            ConvertedWorkstation workstation = convertedDataWorkstations.Where(x => x.WorkstationNumber == WorkStationId)
                .FirstOrDefault();
            if (workstation != null)
            {
                sb.Append($"{intendString}\t<string key=\"WorkstationIdleTime\" value=\"{workstation.IdleTime}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationProductionTime\" value=\"{WorkstationProductionTime}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationQueue\" value=\"{workstation.LongestQueue}\"/>{Environment.NewLine}");
                sb.Append($"{intendString}\t<string key=\"WorkstationBlockingTime\" value=\"{workstation.BlockingTime}\"/>{Environment.NewLine}");
            }

            sb.Append($"{intendString}\t<string key=\"cost:total\" value=\"{Cost}\"/>{Environment.NewLine}");

            sb.Append($"{intendString}\t<date key=\"time:timestamp\" value=\"{TimeStamp.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz")}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<date key=\"start_timestamp\" value=\"{startTime.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz")}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Duration\" value=\"{Duration}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Quantity\" value=\"{Quantity}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"ProductionType\" value=\"{ProductionType}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"concept:name\" value=\"{WorkStepNumber}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"Order\" value=\"{OrderNumber}\"/>{Environment.NewLine}");
            //======================================== Classifier events ============================================
            //Used if the discovery algorithm can not handle classifiers
            sb.Append($"{intendString}\t<string key=\"WorkstationsSpecific\" value=\"{WorkStationId} {cobotAssignedToWorkstation} {Math.Round(WorkstationCost, 2)}\"/>{Environment.NewLine}");
            sb.Append($"{intendString}\t<string key=\"WorkGroupSpecific\" value=\"{WorkstationGroup} {WorkstationGroupCobots} {Math.Round(WorkstationGroupCost, 2)}\"/>{Environment.NewLine}");

            //=======================================================================================================
            sb.Append($"{intendString}</event>{Environment.NewLine}");
            return sb.ToString();
        }


        public override string ToString()
        {
            return $"{OrderNumber};{WorkStepNumber};{WorkStationId};{WorkstationGroup};{CobotAssigned};{Activity};{Cost};{TimeStamp};{Quantity};{ProductionType}{Environment.NewLine}";
        }
    }
}
