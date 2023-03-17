using System;
using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [Item("BestEasy4SimAnalyzer", "An operator for analyzing the best solution of a Easy4Sim Problem.")]
    
    [StorableType("6C3715EC-904A-424D-BC7A-04D7CF41A9C0")]
    public sealed class BestEasy4SimAnalyzer : SingleSuccessorOperator, IAnalyzer, ISingleObjectiveOperator
    {
        public class SingleResult
        {
            public SingleResult(){}
            public SingleResult(double quality, string parameters, string parameterNames)
            {
                Quality = quality;
                Parameters = parameters;
                ParameterNames = parameterNames;
            }
            public double Quality { get; set; }
            public string Parameters { get; set; }
            public string ParameterNames { get; set; }
        }

        public Dictionary<Guid, SingleResult> Results = new Dictionary<Guid, SingleResult>();

        private double _bestQuality = double.MinValue;

        [StorableConstructor]
        private BestEasy4SimAnalyzer(StorableConstructorFlag _) : base(_) { }
        private BestEasy4SimAnalyzer(BestEasy4SimAnalyzer original, Cloner cloner) : base(original, cloner) { }


        public BestEasy4SimAnalyzer()
        {
            Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The qualities of the TSP solutions which should be analyzed."));
            Parameters.Add(new ValueLookupParameter<ResultCollection>("Results", "The result collection where the best TSP solution should be stored."));
            QualityParameter.Hidden = true;
            ResultsParameter.Hidden = true;
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new BestEasy4SimAnalyzer(this, cloner);

        }

        public ScopeTreeLookupParameter<DoubleValue> QualityParameter => (ScopeTreeLookupParameter<DoubleValue>)Parameters["Quality"];
        public ValueLookupParameter<ResultCollection> ResultsParameter => (ValueLookupParameter<ResultCollection>)Parameters["Results"];
        

        public bool EnabledByDefault => true;

        public override IOperation Apply()
        {
            ItemArray<DoubleValue> qualities = QualityParameter.ActualValue;
            
            foreach (DoubleValue doubleValue in qualities)
            {
                if (doubleValue.Value > _bestQuality)
                    _bestQuality = doubleValue.Value;
            }

            ResultCollection results = ResultsParameter.ActualValue;
            foreach (SingleResult result in Results.Values)
            {
                if (Math.Abs(result.Quality - _bestQuality) < 0.001)
                {
                    results.AddOrUpdateResult("Best Parameters Names", new StringValue(result.ParameterNames));
                    results.AddOrUpdateResult("Best Easy4Sim Parameters", new StringValue(result.Parameters));
                    results.AddOrUpdateResult("Best Easy4Sim Quality", new DoubleValue(result.Quality));
                }
            }

            return base.Apply();
        }
    }
}
