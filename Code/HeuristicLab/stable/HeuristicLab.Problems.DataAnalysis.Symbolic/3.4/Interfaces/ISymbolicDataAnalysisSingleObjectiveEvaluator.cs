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

using HeuristicLab.Optimization;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using System.Collections.Generic;
using HeuristicLab.Core;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("2564fc97-080d-4153-8f3a-db764bd06d09")]
  public interface ISymbolicDataAnalysisSingleObjectiveEvaluator<T> : ISymbolicDataAnalysisEvaluator<T>, ISingleObjectiveEvaluator
    where T : class,IDataAnalysisProblemData {
    bool Maximization { get; }

    double Evaluate(IExecutionContext context, ISymbolicExpressionTree tree, T problemData, IEnumerable<int> rows);
  }
}
