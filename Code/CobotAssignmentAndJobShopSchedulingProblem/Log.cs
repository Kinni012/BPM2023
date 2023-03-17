using System;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public struct Log
    {
        /// <summary>
        /// Start of the production
        /// </summary>
        public long Start { get; set; }
        /// <summary>
        /// End of the production
        /// </summary>
        public long End { get; set; }
        /// <summary>
        /// Which order has been produced
        /// </summary>
        public string OrderId { get; set; }
        /// <summary>
        /// Relation number in the order
        /// </summary>
        public int OrderRelationNumber { get; set; }
        /// <summary>
        /// Amount of produced products
        /// </summary>
        public int AmountProduced { get; set; }

        public bool IsInitialized
        {
            get
            {
                if (Start != 0 || End != 0 || OrderRelationNumber != 0 || AmountProduced != 0 || OrderId != "")
                    return true;
                return false;
            }
        }

        public Log(long start, string orderId, int orderRelationNumber, int amountToProduce)
        {
            Start = start;
            End = 0;
            OrderId = orderId;
            OrderRelationNumber = orderRelationNumber;
            AmountProduced = amountToProduce;
        }

        public override string ToString()
        {
            return $"{Start}-{End}: {OrderId} - {OrderRelationNumber} {Environment.NewLine}";
        }
    }
}
