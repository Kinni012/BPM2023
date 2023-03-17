using Easy4SimFramework;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    public class NormalizedObjectiveValue
    {
        public long RunTime { get; private set; }
        public SimulationStatistics Statistics { get; private set; }
        public double NormalizedCostValue
        {
            get
            {
                //E.g. Values 40 (lower limit) 45 (actual value) and 60 (upper limit)
                //Value 1 is the range => 20
                double value1 = SimulationStatistics.UpperLimitCost - SimulationStatistics.LowerLimitCost;
                //Value 2 is the distance from the upper limit, in this case 15
                double value2 = SimulationStatistics.UpperLimitCost - Statistics.Fitness;
                //15/20 gives us 0.75
                return value2 / value1;
            }
        }

        public double NormalizeTimeValue
        {
            get
            {
                double value1 = SimulationStatistics.UpperLimitTime - SimulationStatistics.LowerLimitTime;
                double value2 = SimulationStatistics.UpperLimitTime - RunTime;
                return value2 / value1;
            }
        }

        public NormalizedObjectiveValue(long runtime, SimulationStatistics statistics)
        {
            Statistics = statistics;
            RunTime = runtime;
        }
    }
}
