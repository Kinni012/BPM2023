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
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("161E4580-A31A-4DD5-926E-38B22E82D65F")]
  [Item("GrowTreeCreator", "An operator that creates new symbolic expression trees using the 'Grow' method")]
  public class SymbolicDataAnalysisExpressionGrowTreeCreator : GrowTreeCreator, ISymbolicDataAnalysisSolutionCreator {
    [StorableConstructor]
    protected SymbolicDataAnalysisExpressionGrowTreeCreator(StorableConstructorFlag _) : base(_) { }
    protected SymbolicDataAnalysisExpressionGrowTreeCreator(SymbolicDataAnalysisExpressionGrowTreeCreator original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) { return new SymbolicDataAnalysisExpressionGrowTreeCreator(this, cloner); }

    public SymbolicDataAnalysisExpressionGrowTreeCreator() : base() { }
  }
}
