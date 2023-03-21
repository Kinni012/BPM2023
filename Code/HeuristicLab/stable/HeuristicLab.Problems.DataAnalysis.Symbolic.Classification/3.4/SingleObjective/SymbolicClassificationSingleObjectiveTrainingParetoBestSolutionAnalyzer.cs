#region License Information
/* HeuristicLab
 * Copyright (C) Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Classification {
  /// <summary>
  /// An operator that collects the training Pareto-best symbolic classificatino solutions for single objective symbolic classificatino problems.
  /// </summary>
  [Item("SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer", "An operator that collects the training Pareto-best symbolic classification solutions for single objective symbolic classification problems.")]
  [StorableType("881573CA-7246-4203-B5C9-10793325A008")]
  public sealed class SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer : SymbolicDataAnalysisSingleObjectiveTrainingParetoBestSolutionAnalyzer<IClassificationProblemData, ISymbolicClassificationSolution>, ISymbolicClassificationModelCreatorOperator {
    private const string ModelCreatorParameterName = "ModelCreator";
    #region parameter properties
    public IValueLookupParameter<ISymbolicClassificationModelCreator> ModelCreatorParameter {
      get { return (IValueLookupParameter<ISymbolicClassificationModelCreator>)Parameters[ModelCreatorParameterName]; }
    }
    ILookupParameter<ISymbolicClassificationModelCreator> ISymbolicClassificationModelCreatorOperator.ModelCreatorParameter {
      get { return ModelCreatorParameter; }
    }
    #endregion

    [StorableConstructor]
    private SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer(StorableConstructorFlag _) : base(_) { }
    private SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer(SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer()
      : base() {
      Parameters.Add(new ValueLookupParameter<ISymbolicClassificationModelCreator>(ModelCreatorParameterName, ""));
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicClassificationSingleObjectiveTrainingParetoBestSolutionAnalyzer(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      // BackwardsCompatibility3.4
      #region Backwards compatible code, remove with 3.5
      if (!Parameters.ContainsKey(ModelCreatorParameterName))
        Parameters.Add(new ValueLookupParameter<ISymbolicClassificationModelCreator>(ModelCreatorParameterName, ""));
      #endregion
    }

    protected override ISymbolicClassificationSolution CreateSolution(ISymbolicExpressionTree bestTree) {
      var model = ModelCreatorParameter.ActualValue.CreateSymbolicClassificationModel(ProblemDataParameter.ActualValue.TargetVariable, (ISymbolicExpressionTree)bestTree.Clone(), SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper);
      if (ApplyLinearScalingParameter.ActualValue.Value) model.Scale(ProblemDataParameter.ActualValue);

      model.RecalculateModelParameters(ProblemDataParameter.ActualValue, ProblemDataParameter.ActualValue.TrainingIndices);
      return model.CreateClassificationSolution((IClassificationProblemData)ProblemDataParameter.ActualValue.Clone());
    }
  }
}
