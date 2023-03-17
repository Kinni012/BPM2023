using System;
using System.Collections.Generic;
using System.Text;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ConvertedWorkstep :  ICloneable
    {

        public Guid Id { get; private set; }
        public string OrderNumber { get; set; }

        public int OrderRelationNumber { get; set; }

        public List<int> PreviousWorkSteps { get; set; } = new List<int>();
        public string Description { get; set; }
        public DateTime EarliestStartDate { get; set; }
        public DateTime LatestEndDate { get; set; }
        public string AreaName { get; set; }
        public string WorkstationShould { get; set; }
        public string AlternativeWorkgroup { get; set; }
        public int? WorkstationShouldRelative { get; set; }
        public string WorkstationGroup { get; set; }
        public int SetupTime { get; set; }
        public int DeSetupTime { get; set; }
        public int ProductionTime { get; set; }
        public int Amount { get; set; }

        public int ProducedAmount { get; set; }


        public double Priority { get; set; }
        public int RelativeId { get; set; }


        /// <summary>
        /// Flag that shows if a specific work step has been finished already
        /// </summary>

        public bool IsFinished { get; set; }
        /// <summary>
        /// Flag that shows if a specific work step currently is produced
        /// </summary>

        public bool IsProducing { get; set; }

        public long IsDesetupUntil { get; set; }

        /// <summary>
        /// How many pieces are produced
        /// </summary>

        public int CurrentlyProducingAmount { get; set; }
        /// <summary>
        /// How capacity is set up
        /// </summary>
        public int InitializedAmount { get; set; }


        public ConvertedData ConvertedData { get; set; }


        /// <summary>
        /// Workstation where the workstep is produced
        /// </summary>

        public ConvertedWorkstation IsProducedOnWorkstation { get; set; }


        public int RemainingAmountToProduce => Amount - (CurrentlyProducingAmount + ProducedAmount);

        //public string ButtonText => $"O:{OrderNumber}-{OrderRelationNumber} until {IsDesetupUntil}";

        public string Log(List<WorkstepLog> logs)
        {

            StringBuilder sb = new StringBuilder();
            foreach (WorkstepLog log in logs)
                sb.Append(log);
            return sb.ToString();

        }

        public bool HasStartedProducing { get; set; }


        #region Ctor
        public ConvertedWorkstep() => Initialize();
        #endregion

        public bool IsProducingAndFinished(long simulationTime) => IsProducing && simulationTime >= IsDesetupUntil;
        public bool Finished(long simulationTime) => simulationTime >= IsDesetupUntil;


        private void Initialize()
        {
            Id = Guid.NewGuid();

        }

        public override bool Equals(object obj)
        {
            if (obj is ConvertedWorkstep workstep)
                return Id == workstep.Id;
            return false;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        public object Clone()
        {
            ConvertedWorkstep result = (ConvertedWorkstep)this.MemberwiseClone();
            return result;
        }

    }
}
