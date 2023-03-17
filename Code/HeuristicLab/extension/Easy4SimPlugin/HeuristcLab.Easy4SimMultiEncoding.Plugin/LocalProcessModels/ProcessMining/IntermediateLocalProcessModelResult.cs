using System;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining
{
    public class IntermediateLocalProcessModelResult : ICloneable
    {
        public QualityMetrics QualityMetric { get; set; }

        public ProcessTree.ProcessTree CurrentProcessTree { get; set; }
        public int Order => CurrentProcessTree.GetLeaves().Count;

        public double MetricSum => QualityMetric.MetricSum;

        public IntermediateLocalProcessModelResult() { }
        public IntermediateLocalProcessModelResult(ProcessTree.ProcessTree tree, QualityMetrics metric)
        {
            QualityMetric = metric;
            CurrentProcessTree = tree;
        }

        public object Clone()
        {
            IntermediateLocalProcessModelResult result = new IntermediateLocalProcessModelResult();
            result.QualityMetric = (QualityMetrics)QualityMetric.Clone();
            result.CurrentProcessTree = (ProcessTree.ProcessTree)CurrentProcessTree.Clone();
            return result;
        }
    }
}
