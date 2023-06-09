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

using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [Item("SymbolicDataAnalysisBottomUpDiversityAnalyzer", "A diversity analyzer based on the bottom-up distance between trees.")]
  [StorableType("822FB7E4-3CFF-4413-90EB-96E90CE1D1D4")]
  public class SymbolicDataAnalysisBottomUpDiversityAnalyzer : PopulationSimilarityAnalyzer {
    [StorableConstructor]
    protected SymbolicDataAnalysisBottomUpDiversityAnalyzer(StorableConstructorFlag _) : base(_) { }

    protected SymbolicDataAnalysisBottomUpDiversityAnalyzer(SymbolicDataAnalysisBottomUpDiversityAnalyzer original, Cloner cloner)
      : base(original, cloner) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicDataAnalysisBottomUpDiversityAnalyzer(this, cloner);
    }

    public SymbolicDataAnalysisBottomUpDiversityAnalyzer(SymbolicExpressionTreeBottomUpSimilarityCalculator similarityCalculator)
      : base(new[] { similarityCalculator }) {
      DiversityResultName = "Genotypic Diversity";
      UpdateCounterParameter.ActualName = "GenotypicDiversityAnalyzerUpdateCounter";
    }
  }
}
