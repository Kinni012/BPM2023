﻿#region License Information
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

using System;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("BC7E7414-64B5-4428-AA89-270F8EF6E35E")]
  public abstract class SymbolicDataAnalysisSingleObjectiveProblem<T, U, V> : SymbolicDataAnalysisProblem<T, U, V>, ISymbolicDataAnalysisSingleObjectiveProblem
    where T : class,IDataAnalysisProblemData
    where U : class, ISymbolicDataAnalysisSingleObjectiveEvaluator<T>
    where V : class, ISymbolicDataAnalysisSolutionCreator {
    private const string MaximizationParameterName = "Maximization";
    private const string BestKnownQualityParameterName = "BestKnownQuality";

    #region parameter properties
    public IFixedValueParameter<BoolValue> MaximizationParameter {
      get { return (IFixedValueParameter<BoolValue>)Parameters[MaximizationParameterName]; }
    }
    IParameter ISingleObjectiveHeuristicOptimizationProblem.MaximizationParameter {
      get { return MaximizationParameter; }
    }
    public IFixedValueParameter<DoubleValue> BestKnownQualityParameter {
      get { return (IFixedValueParameter<DoubleValue>)Parameters[BestKnownQualityParameterName]; }
    }
    IParameter ISingleObjectiveHeuristicOptimizationProblem.BestKnownQualityParameter {
      get { return BestKnownQualityParameter; }
    }
    #endregion

    #region properties
    public BoolValue Maximization {
      get { return MaximizationParameter.Value; }
    }
    public DoubleValue BestKnownQuality {
      get { return BestKnownQualityParameter.Value; }
    }
    ISingleObjectiveEvaluator ISingleObjectiveHeuristicOptimizationProblem.Evaluator {
      get { return Evaluator; }
    }
    #endregion

    [StorableConstructor]
    protected SymbolicDataAnalysisSingleObjectiveProblem(StorableConstructorFlag _) : base(_) { }
    protected SymbolicDataAnalysisSingleObjectiveProblem(SymbolicDataAnalysisSingleObjectiveProblem<T, U, V> original, Cloner cloner)
      : base(original, cloner) {
      RegisterEventHandler();
      MaximizationParameter.Hidden = true;
    }

    protected SymbolicDataAnalysisSingleObjectiveProblem(T problemData, U evaluator, V solutionCreator)
      : base(problemData, evaluator, solutionCreator) {
      Parameters.Add(new FixedValueParameter<BoolValue>(MaximizationParameterName, "Set to false if the problem should be minimized."));
      Parameters.Add(new FixedValueParameter<DoubleValue>(BestKnownQualityParameterName, "The quality of the best known solution of this problem."));

      MaximizationParameter.Hidden = true;
      InitializeOperators();
      RegisterEventHandler();
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandler();
    }

    private void InitializeOperators() {
      Operators.Add(new SymbolicDataAnalysisAlleleFrequencyAnalyzer());
      ParameterizeOperators();
    }

    private void RegisterEventHandler() {
      Evaluator.QualityParameter.ActualNameChanged += new EventHandler(QualityParameter_ActualNameChanged);
    }

    protected override void OnEvaluatorChanged() {
      base.OnEvaluatorChanged();
      Evaluator.QualityParameter.ActualNameChanged += new EventHandler(QualityParameter_ActualNameChanged);
      Maximization.Value = base.Evaluator.Maximization;
      ParameterizeOperators();
    }

    private void QualityParameter_ActualNameChanged(object sender, EventArgs e) {
      ParameterizeOperators();
    }

    protected override void ParameterizeOperators() {
      base.ParameterizeOperators();

      foreach (var op in Operators.OfType<ISymbolicDataAnalysisSingleObjectiveAnalyzer>()) {
        op.QualityParameter.ActualName = Evaluator.QualityParameter.ActualName;
        op.MaximizationParameter.ActualName = MaximizationParameterName;
      }
    }
  }
}
