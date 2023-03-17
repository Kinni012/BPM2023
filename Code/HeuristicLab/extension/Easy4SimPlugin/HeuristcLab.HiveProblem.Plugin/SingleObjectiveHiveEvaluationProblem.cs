using System;
using System.Linq;
using System.Threading;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;

namespace HeuristicLab.SimGenOpt
{
    [StorableType("72D372EC-BBBB-4D0F-A11A-1E0ED71934B4")]
    [Creatable(CreatableAttribute.Categories.Problems + "###3$$$SimGenOpt", Priority = int.MaxValue)]
    [Item("SingleObjectiveHiveEvaluationProblem", "A wrapper problem for single-objective problems that should be evaluated using Hive.")]
    public sealed class SingleObjectiveHiveEvaluationProblem : SingleObjectiveBasicProblem<IEncoding>
    {
        public override bool Maximization { get { return false; } }

        #region Parameter Names
        private const string ProblemParameterName = "Problem";
        private const string ProjectResourcesParameterName = "ProjectResources";
        private const string DistributeTasksParameterName = "DistributeTasks";
        #endregion

        #region Parameters
        public IValueParameter<SingleObjectiveBasicProblem<IEncoding>> ProblemParameter
        {
            get { return (IValueParameter<SingleObjectiveBasicProblem<IEncoding>>)Parameters[ProblemParameterName]; }
        }

        public IValueParameter<ProjectResourcesValue> ProjectResourcesParameter
        {
            get { return (IValueParameter<ProjectResourcesValue>)Parameters[ProjectResourcesParameterName]; }
        }

        public IValueParameter<BoolValue> DistributeTasksParameter
        {
            get { return (IValueParameter<BoolValue>)Parameters[DistributeTasksParameterName]; }
        }
        #endregion

        #region Parameter Properties
        public SingleObjectiveBasicProblem<IEncoding> Problem
        {
            get { return ProblemParameter.Value; }
            set { ProblemParameter.Value = value; }
        }

        public ProjectResourcesValue ProjectResources
        {
            get { return ProjectResourcesParameter.Value; }
            set { ProjectResourcesParameter.Value = value; }
        }

        public BoolValue DistributeTasks
        {
            get { return DistributeTasksParameter.Value; }
            set { DistributeTasksParameter.Value = value; }
        }
        #endregion

        #region Constructors & Cloning
        [StorableConstructor]
        private SingleObjectiveHiveEvaluationProblem(StorableConstructorFlag _) : base(_) { }
        private SingleObjectiveHiveEvaluationProblem(SingleObjectiveHiveEvaluationProblem original, Cloner cloner) : base(original, cloner)
        {
            Initialize();
        }
        public SingleObjectiveHiveEvaluationProblem()
        {
            Parameters.Add(new ValueParameter<SingleObjectiveBasicProblem<IEncoding>>(ProblemParameterName, "The actual problem that should used to evaluate individuals."));
            Parameters.Add(new ValueParameter<ProjectResourcesValue>(ProjectResourcesParameterName, "The project/resources where Hive jobs/tasks should be assigned to."));
            Parameters.Add(new ValueParameter<BoolValue>(DistributeTasksParameterName, "The evaluation strategy that should be used."));

            Initialize();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new SingleObjectiveHiveEvaluationProblem(this, cloner);
        }
        #endregion

        [StorableHook(HookType.AfterDeserialization)]
        private void AfterDeserialization()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (Problem != null) Problem.OperatorsChanged += Problem_OperatorsChanged;
            ProblemParameter.ValueChanged += ProblemParameter_ValueChanged;
        }

        private void ProblemParameter_ValueChanged(object sender, EventArgs e)
        {
            if (Problem != null) Problem.OperatorsChanged += Problem_OperatorsChanged;
        }

        private void Problem_OperatorsChanged(object sender, EventArgs e)
        {
            Encoding = Problem.Encoding;
        }

        public override double Evaluate(Individual individual, IRandom random)
        {
            var individualScope = new Scope();
            foreach (var value in individual.Values)
            {
                individualScope.Variables.Add(new Variable(value.Key, value.Value));
            }

            var evaluation = new SingleObjectiveBasicProblemEvaluation(Problem, individualScope, random);

            var options = new HiveApi.Options
            {
                ProjectId = ProjectResources.ProjectId,
                ResourceIds = ProjectResources.ResourceIds,
                Distribute = DistributeTasks.Value,
                JobName = string.Format("{0}+{1}", evaluation, Guid.NewGuid().ToString().ToUpperInvariant())
            };

            try
            {
                var evaluations = HiveApi.ExecuteInHive(new[] { evaluation }, options, CancellationToken.None);
                evaluation = evaluations.SingleOrDefault();
            }
            catch
            {
                return Maximization ? double.MinValue : double.MaxValue;
            }

            foreach (var variable in evaluation.IndividualScope.Variables)
            {
                individual[variable.Name] = variable.Value;
            }

            return evaluation.Quality;
        }

        public override void Analyze(Individual[] individuals, double[] qualities, ResultCollection results, IRandom random)
        {
            Problem.Analyze(individuals, qualities, results, random);
        }
    }
}
