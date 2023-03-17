using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ConvertedWorkstation :  ICloneable
    {
        public List<WorkstepLog> WorkstepLogs { get; set; } = new List<WorkstepLog>();

        private const int SecondsOfOneHour = 60 * 60;


        public List<Log> Logs { get; set; }
        public List<Log> FullLogs { get; set; }

        public Guid Id { get; set; }
        public int RelativeId { get; set; }
        public int RelativeWorkgroupId { get; set; }

        public string WorkstationGroupNumber { get; set; }
        public string WorkstationNumber { get; set; }
        public string Description { get; set; }
        public string AreaName { get; set; }
        public int CapacityTyp { get; set; }
        public int Capacity { get; set; }
        public int CapacityOffer { get; set; }
        public int CapacityFormula { get; set; }
        public int CostProcessing { get; set; }
        public double CostProcessingPerSecond { get; set; }
        public int CostSetupTool { get; set; }
        public double CostSetupToolPerSecond { get; set; }
        public int SetupToolType { get; set; }
        public double SpeedFactorSetup { get; set; }
        public double SpeedFactorWorking { get; set; }

        /// <summary>
        /// Is a cobot assigned to this workstation that speeds up the production
        /// </summary>
        public bool IsCobotAssigned { get; set; }
        /// <summary>
        /// Total time of all tasks waiting for the workstation to finish production
        /// </summary>
        public long BlockingTime { get; set; }
        public long IdleTime { get; set; }
        public int LongestQueue { get; set; }

        public double BreakingChance { get; set; }



        public ConvertedData ConvertedData { get; set; }



        public string ProductionTypeAsString
        {
            get
            {
                if (CapacityTyp == 0)
                    return "Sequential";
                else if (CapacityTyp == 1)
                    return "Parallel";
                else if (CapacityTyp == 2)
                    return "Infinity";
                else
                    return "Team";
            }
        }


        public string WorkgroupText
        {
            get
            {
                return $"G{RelativeWorkgroupId}";
            }
        }

        #region Ctor

        public ConvertedWorkstation() => Initialize();
        #endregion

        private void Initialize()
        {
            Logs = new List<Log>();
            FullLogs = new List<Log>();
            Id = Guid.NewGuid();
        }




        public override bool Equals(object obj)
        {
            if (obj is ConvertedWorkstation workstation)
                return Id == workstation.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Check if the workstation still has free capacity.
        /// This is done by checking if the capacity is bigger than the amount of currently producing products.
        /// </summary>
        /// <param name="simulationTime"></param>
        /// <returns></returns>
        public bool HasFreeCapacity(long simulationTime, List<ProducedProductLog> producedProducts)
        {
            return Capacity > CurrentlyProducingAmount(simulationTime, producedProducts);
        }

        /// <summary>
        /// Check the currently producing amount of a workstation.
        /// Therefore we go through all logs in the products produced.
        /// </summary>
        /// <param name="simulationTime"></param>
        /// <returns></returns>
        public int CurrentlyProducingAmount(long simulationTime, List<ProducedProductLog> producedProducts)
        {
            int producing = 0;
            foreach (ProducedProductLog productLog in producedProducts.Where(x => x.Workstep.IsProducedOnWorkstation.WorkstationNumber == WorkstationNumber))
            {
                //If start is small than the current simulation time we increase the produced amount
                if (productLog.Start <= simulationTime)
                    producing += productLog.Amount;
                //If end time is smaller than the current simulation time, we decrease the produced amount again, because the task has finished
                if (productLog.End <= simulationTime)
                    producing -= productLog.Amount;
            }
            return producing;
        }





        public int RemainingCapacity(long simulationTime, List<ProducedProductLog> producedProducts)
        {
            return Capacity - CurrentlyProducingAmount(simulationTime, producedProducts);
        }

        public bool CanProduce(long simulationTime, List<ProducedProductLog> producedProducts)
        {
            //Unlimited
            if (CapacityTyp == 2)
                return true;

            //Capacity limited
            if (CapacityTyp == 1 && HasFreeCapacity(simulationTime, producedProducts))
                return true;

            //Only one product at a time
            if ((CapacityTyp == 0 || CapacityTyp == 4) && producedProducts.Count(x => x.Workstep.IsProducedOnWorkstation.WorkstationNumber == WorkstationNumber) == 0)
                return true;
            return false;
        }

        public string Log
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (Log log in Logs)
                    sb.Append(log.ToString());
                return sb.ToString();
            }
        }

        public double Utilization(long simulationTime)
        {
            if (CapacityTyp == 0)
            {
                long sum = 0;
                foreach (Log log in FullLogs)
                {
                    sum += log.End - log.Start;
                }

                return sum / simulationTime;
            }

            if (CapacityTyp == 1 || CapacityTyp == 4)
            {
                long sum = 0;
                foreach (Log log in FullLogs)
                {
                    sum += (log.End - log.Start) * log.AmountProduced;
                }

                return sum / (simulationTime * Capacity);
            }
            if (CapacityTyp == 2)
                return 0;



            return 0;
        }


        public object Clone()
        {
            ConvertedWorkstation result = new ConvertedWorkstation();
            result.Id = Id;
            result.WorkstationGroupNumber = WorkstationGroupNumber;
            result.WorkstationNumber = WorkstationNumber;
            result.Description = Description;
            result.AreaName = AreaName;
            result.CapacityTyp = CapacityTyp;
            result.Capacity = Capacity;
            result.CapacityOffer = CapacityOffer;

            result.BreakingChance = BreakingChance;
            result.CapacityFormula = CapacityFormula;
            result.CostProcessing = CostProcessing;
            result.CostProcessingPerSecond = CostProcessingPerSecond;
            result.ConvertedData = ConvertedData;

            result.CostSetupTool = CostSetupTool;
            result.CostSetupToolPerSecond = CostSetupToolPerSecond;
            result.SetupToolType = SetupToolType;
            result.IsCobotAssigned = IsCobotAssigned;

            result.IdleTime = IdleTime;

            result.RelativeId = RelativeId;
            result.RelativeWorkgroupId = RelativeWorkgroupId;

            result.SpeedFactorWorking = SpeedFactorWorking;
            result.SpeedFactorSetup = SpeedFactorSetup;

            return result;
        }
    }
}
