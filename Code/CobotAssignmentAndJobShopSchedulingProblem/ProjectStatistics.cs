using System.Linq;
using Easy4SimFramework;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ProjectStatistics : CSimBase
    {

        public InEventObject InputData { get; set; }
        public ConvertedData Data { get; set; }

        #region constructor
        public ProjectStatistics() => InitializeParameters();
        public ProjectStatistics(long i, string n, SolverSettings settings) : base(i, n, settings) => InitializeParameters();
        #endregion constructor

        private void InitializeParameters()
        {
            InputData = new InEventObject(this);
        }


        public override void Start()
        {
            if (this.OptimizeForSimulation)
                return;

            //######### Write information about the data to the log ##############
            //Starting with orders

            if (!(InputData.Value is ConvertedData data)) return;
            Data = data;
            var temp = data.Orders.OrderBy(x => x.EarliestStartDate.Year.ToString() + " " + x.EarliestStartDate.Month.ToString()).GroupBy(x => x.EarliestStartDate.Month.ToString() + " " + x.EarliestStartDate.Year.ToString());
            foreach (IGrouping<string, ConvertedOrder> grouping in temp)
            {
                //Settings.Environment.NewEventLog(LogName, $"Orders in month {grouping.Key}: {grouping.Count()}",
                //    Easy4SimFramework.Environment.LoggingCategory.Info);
            }

        }


        public override object Clone()
        {
            ProjectStatistics result = new ProjectStatistics(Index, Name, Settings);
            result.CurrentGuid = CurrentGuid;
            result.OptimizeForSimulation = OptimizeForSimulation;
            if (Data != null)
                result.Data = (ConvertedData)Data.Clone();
            if (InputData.Value != null)
                result.InputData.Value = InputData.Value;
            return result;
        }
    }
}
