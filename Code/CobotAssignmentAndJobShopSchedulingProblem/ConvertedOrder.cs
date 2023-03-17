using System;
using System.Collections.Generic;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ConvertedOrder :  ICloneable
    {

        public Guid Id { get; private set; }

        public string OrderNumber { get; set; }
        public string OrderGroupNumber { get; set; }
        public string Description { get; set; }
        public DateTime EarliestStartDate { get; set; }
        public DateTime LatestEndDate { get; set; }

        public bool IsFinished { get; set; }

        public ConvertedData ConvertedData { get; set; }

        public List<ConvertedWorkstep> WorkstepsInOrder { get; set; } = new List<ConvertedWorkstep>();
        public List<ConvertedWorkstep> FinishedWorksteps { get; set; } = new List<ConvertedWorkstep>();

        public ConvertedWorkstep GetNextWorkstep()
        {
            return null;
            //return WorkstepsInOrder.OrderBy(x => x.Value.OrderRelationNumber).Select(x => x.Value).FirstOrDefault();
        }



        #region Ctor

        public ConvertedOrder() => Initialize();
        #endregion


        private void Initialize()
        {
            IsFinished = false;
            Id = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            if (obj is ConvertedOrder order)
                return Id == order.Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        public object Clone()
        {
            ConvertedOrder result = new ConvertedOrder();
            result.EarliestStartDate = EarliestStartDate;
            result.Description = Description;
            result.LatestEndDate = LatestEndDate;
            result.OrderGroupNumber = OrderGroupNumber;
            result.OrderNumber = OrderNumber;
            result.ConvertedData = ConvertedData;
            foreach (ConvertedWorkstep workstep in WorkstepsInOrder)
            {
                result.WorkstepsInOrder.Add((ConvertedWorkstep)workstep.Clone());
            }

            result.IsFinished = IsFinished;
            return result;
        }
    }
}
