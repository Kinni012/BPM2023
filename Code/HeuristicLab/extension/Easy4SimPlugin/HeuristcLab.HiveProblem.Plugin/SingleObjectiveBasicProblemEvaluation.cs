using System.Threading;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;

namespace HeuristicLab.SimGenOpt
{
    [StorableType("F4FCDA78-EF0D-48D6-A964-CB191E4D981D")]
    public sealed class SingleObjectiveBasicProblemEvaluation : BasicAlgorithm
    {
        public override bool SupportsPause { get { return false; } }

        public new SingleObjectiveBasicProblem<IEncoding> Problem
        {
            get { return (SingleObjectiveBasicProblem<IEncoding>)base.Problem; }
            private set { base.Problem = value; }
        }

        [Storable]
        public IScope IndividualScope { get; private set; }
        [Storable]
        public IRandom Random { get; private set; }
        [Storable]
        public double Quality { get; private set; }

        #region Constructors & Cloning
        [StorableConstructor]
        private SingleObjectiveBasicProblemEvaluation(StorableConstructorFlag _) : base(_) { }
        private SingleObjectiveBasicProblemEvaluation(SingleObjectiveBasicProblemEvaluation original, Cloner cloner) : base(original, cloner)
        {
            IndividualScope = cloner.Clone(original.IndividualScope);
            Random = cloner.Clone(original.Random);
            Quality = original.Quality;
        }
        public SingleObjectiveBasicProblemEvaluation(SingleObjectiveBasicProblem<IEncoding> problem, IScope individualScope, IRandom random)
        {
            Problem = problem;
            IndividualScope = individualScope;
            Random = random;
            Quality = double.NaN;
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new SingleObjectiveBasicProblemEvaluation(this, cloner);
        }
        #endregion

        protected override void Run(CancellationToken cancellationToken)
        {
            var singleEncoding = Problem.Encoding;
            var multiEncoding = Problem.Encoding as MultiEncoding;

            var individual = multiEncoding != null
              ? (Individual)new MultiEncodingIndividual(multiEncoding, IndividualScope)
              : (Individual)new SingleEncodingIndividual(singleEncoding, IndividualScope);

            var evaluationTask = Task.Run(() => Problem.Evaluate(individual, Random));
            evaluationTask.Wait(cancellationToken);
            Quality = evaluationTask.Result;
        }
    }
}
