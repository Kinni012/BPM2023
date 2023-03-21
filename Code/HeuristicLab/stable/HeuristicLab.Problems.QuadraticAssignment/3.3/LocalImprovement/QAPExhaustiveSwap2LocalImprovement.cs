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
using System.Threading;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.PermutationEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Problems.QuadraticAssignment {
  [Item("QAPExhaustiveSwap2LocalImprovement", "Takes a solution and finds the local optimum with respect to the swap2 neighborhood by decending along the steepest gradient.")]
  [StorableType("AD82A71D-773A-4CD2-841F-755840656E92")]
  public class QAPExhaustiveSwap2LocalImprovement : SingleSuccessorOperator, ILocalImprovementOperator, ISingleObjectiveOperator {

    public ILookupParameter<IntValue> LocalIterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["LocalIterations"]; }
    }

    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }

    public ILookupParameter<IntValue> EvaluatedSolutionsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["EvaluatedSolutions"]; }
    }

    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters["Results"]; }
    }

    public ILookupParameter<Permutation> AssignmentParameter {
      get { return (ILookupParameter<Permutation>)Parameters["Assignment"]; }
    }

    public ILookupParameter<DoubleValue> QualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters["Quality"]; }
    }

    public ILookupParameter<BoolValue> MaximizationParameter {
      get { return (ILookupParameter<BoolValue>)Parameters["Maximization"]; }
    }

    public ILookupParameter<DoubleMatrix> WeightsParameter {
      get { return (ILookupParameter<DoubleMatrix>)Parameters["Weights"]; }
    }

    public ILookupParameter<DoubleMatrix> DistancesParameter {
      get { return (ILookupParameter<DoubleMatrix>)Parameters["Distances"]; }
    }

    public IValueLookupParameter<BoolValue> UseFastEvaluationParameter {
      get { return (IValueLookupParameter<BoolValue>)Parameters["UseFastEvaluation"]; }
    }

    [StorableConstructor]
    protected QAPExhaustiveSwap2LocalImprovement(StorableConstructorFlag _) : base(_) { }
    protected QAPExhaustiveSwap2LocalImprovement(QAPExhaustiveSwap2LocalImprovement original, Cloner cloner)
      : base(original, cloner) {
    }
    public QAPExhaustiveSwap2LocalImprovement()
      : base() {
      Parameters.Add(new LookupParameter<IntValue>("LocalIterations", "The number of iterations that have already been performed."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "The maximum amount of iterations that should be performed (note that this operator will abort earlier when a local optimum is reached).", new IntValue(10000)));
      Parameters.Add(new LookupParameter<IntValue>("EvaluatedSolutions", "The amount of evaluated solutions (here a move is counted only as 4/n evaluated solutions with n being the length of the permutation)."));
      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The collection where to store results."));
      Parameters.Add(new LookupParameter<Permutation>("Assignment", "The permutation that is to be locally optimized."));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value of the assignment."));
      Parameters.Add(new LookupParameter<BoolValue>("Maximization", "True if the problem should be maximized or minimized."));
      Parameters.Add(new LookupParameter<DoubleMatrix>("Weights", "The weights matrix."));
      Parameters.Add(new LookupParameter<DoubleMatrix>("Distances", "The distances matrix."));
      Parameters.Add(new ValueLookupParameter<BoolValue>("UseFastEvaluation", "Enabling this option will use a NxN double matrix to save the last move qualities. The moves of the first iteration will then be evaluated in O(N) while all further moves will be evaluated in O(1).", new BoolValue(true)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new QAPExhaustiveSwap2LocalImprovement(this, cloner);
    }

    // BackwardsCompatibility3.3
    #region Backwards compatible code, remove with 3.4
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      if (!Parameters.ContainsKey("LocalIterations"))
        Parameters.Add(new LookupParameter<IntValue>("LocalIterations", "The number of iterations that have already been performed."));
      if (!Parameters.ContainsKey("UseFastEvaluation"))
        Parameters.Add(new ValueLookupParameter<BoolValue>("UseFastEvaluation", "Enabling this option will use a NxN double matrix to save the last move qualities. The moves of the first iteration will then be evaluated in O(N) while all further moves will be evaluated in O(1).", new BoolValue(false)));
    }
    #endregion

    public static void Improve(Permutation assignment, DoubleMatrix weights, DoubleMatrix distances, DoubleValue quality, IntValue localIterations, IntValue evaluatedSolutions, bool maximization, int maxIterations, CancellationToken cancellation) {
      double evalSolPerMove = 4.0 / assignment.Length;

      for (int i = localIterations.Value; i < maxIterations; i++) {
        Swap2Move bestMove = null;
        double bestQuality = 0; // we have to make an improvement, so 0 is the baseline
        double evaluations = 0.0;
        foreach (Swap2Move move in ExhaustiveSwap2MoveGenerator.Generate(assignment)) {
          double moveQuality = QAPSwap2MoveEvaluator.Apply(assignment, move, weights, distances);
          evaluations += evalSolPerMove;
          if (maximization && moveQuality > bestQuality
            || !maximization && moveQuality < bestQuality) {
            bestQuality = moveQuality;
            bestMove = move;
          }
        }
        evaluatedSolutions.Value += (int)Math.Ceiling(evaluations);
        if (bestMove == null) break;
        Swap2Manipulator.Apply(assignment, bestMove.Index1, bestMove.Index2);
        quality.Value += bestQuality;
        localIterations.Value++;
        cancellation.ThrowIfCancellationRequested();
      }
    }

    public static void ImproveFast(Permutation assignment, DoubleMatrix weights, DoubleMatrix distances, DoubleValue quality, IntValue localIterations, IntValue evaluatedSolutions, bool maximization, int maxIterations, CancellationToken cancellation) {
      Swap2Move bestMove = null;
      double evaluations = 0.0;
      var lastQuality = new double[assignment.Length, assignment.Length];
      for (int i = localIterations.Value; i < maxIterations; i++) {
        double bestQuality = 0; // we have to make an improvement, so 0 is the baseline
        var lastMove = bestMove;
        bestMove = null;
        foreach (Swap2Move move in ExhaustiveSwap2MoveGenerator.Generate(assignment)) {
          double moveQuality;
          if (lastMove == null) {
            moveQuality = QAPSwap2MoveEvaluator.Apply(assignment, move, weights, distances);
            evaluations += 4.0 / assignment.Length;
          } else {
            moveQuality = QAPSwap2MoveEvaluator.Apply(assignment, move, lastQuality[move.Index1, move.Index2], weights, distances, lastMove);
            if (move.Index1 == lastMove.Index1 || move.Index2 == lastMove.Index1 || move.Index1 == lastMove.Index2 || move.Index2 == lastMove.Index2)
              evaluations += 4.0 / assignment.Length;
            else evaluations += 2.0 / (assignment.Length * assignment.Length);
          }
          lastQuality[move.Index1, move.Index2] = moveQuality;
          if (maximization && moveQuality > bestQuality
            || !maximization && moveQuality < bestQuality) {
            bestQuality = moveQuality;
            bestMove = move;
          }
        }
        if (bestMove == null) break;
        Swap2Manipulator.Apply(assignment, bestMove.Index1, bestMove.Index2);
        quality.Value += bestQuality;
        localIterations.Value++;
        if (cancellation.IsCancellationRequested) {
          evaluatedSolutions.Value += (int)Math.Round(evaluations);
          throw new OperationCanceledException();
        }
      }
      evaluatedSolutions.Value += (int)Math.Round(evaluations);
    }

    public override IOperation Apply() {
      var maxIterations = MaximumIterationsParameter.ActualValue.Value;
      var assignment = AssignmentParameter.ActualValue;
      var maximization = MaximizationParameter.ActualValue.Value;
      var weights = WeightsParameter.ActualValue;
      var distances = DistancesParameter.ActualValue;
      var quality = QualityParameter.ActualValue;
      var localIterations = LocalIterationsParameter.ActualValue;
      var evaluations = EvaluatedSolutionsParameter.ActualValue;
      if (localIterations == null) {
        localIterations = new IntValue(0);
        LocalIterationsParameter.ActualValue = localIterations;
      }

      if (UseFastEvaluationParameter.ActualValue.Value)
        ImproveFast(assignment, weights, distances, quality, localIterations, evaluations, maximization, maxIterations, CancellationToken);
      else Improve(assignment, weights, distances, quality, localIterations, evaluations, maximization, maxIterations, CancellationToken);

      localIterations.Value = 0;
      return base.Apply();
    }
  }
}
