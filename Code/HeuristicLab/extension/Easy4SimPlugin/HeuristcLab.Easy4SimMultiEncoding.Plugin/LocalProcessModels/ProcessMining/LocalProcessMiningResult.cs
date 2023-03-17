using System;
using System.Collections.Generic;
using System.Linq;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining
{
    public class LocalProcessMiningResult
    {
        public double BestInitialMetric
        {

            get
            {
                List<double> initialMetrics = new List<double>();
                foreach (IntermediateLocalProcessModelResult intermediateLocalProcessModelResult in InitialIntermediates)
                {
                    initialMetrics.Add(intermediateLocalProcessModelResult.MetricSum);
                }

                double max = initialMetrics.Max();

                return Math.Round(max, 2);
            }
        }
        public double BestCurrentIntermediates => Math.Round(CurrentIntermediates.Select(x => x.MetricSum).Max());
        public List<IntermediateLocalProcessModelResult> InitialIntermediates = new List<IntermediateLocalProcessModelResult>();
        public List<IntermediateLocalProcessModelResult> CurrentIntermediates = new List<IntermediateLocalProcessModelResult>();

        public LocalProcessMiningResult()
        {
                
        }
    }
}
