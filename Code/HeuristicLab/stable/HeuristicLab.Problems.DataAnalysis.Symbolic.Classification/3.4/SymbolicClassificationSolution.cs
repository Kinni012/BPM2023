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

using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Classification {
  /// <summary>
  /// Represents a symbolic classification solution (model + data) and attributes of the solution like accuracy and complexity
  /// </summary>
  [StorableType("C2829CEB-8D34-4FAB-B9AE-23937071B53F")]
  [Item(Name = "SymbolicClassificationSolution", Description = "Represents a symbolic classification solution (model + data) and attributes of the solution like accuracy and complexity.")]
  public sealed class SymbolicClassificationSolution : ClassificationSolution, ISymbolicClassificationSolution {
    private const string ModelLengthResultName = "ModelLength";
    private const string ModelDepthResultName = "ModelDepth";

    public new ISymbolicClassificationModel Model {
      get { return (ISymbolicClassificationModel)base.Model; }
      set { base.Model = value; }
    }

    ISymbolicDataAnalysisModel ISymbolicDataAnalysisSolution.Model {
      get { return (ISymbolicDataAnalysisModel)base.Model; }
    }
    public int ModelLength {
      get { return ((IntValue)this[ModelLengthResultName].Value).Value; }
      private set { ((IntValue)this[ModelLengthResultName].Value).Value = value; }
    }

    public int ModelDepth {
      get { return ((IntValue)this[ModelDepthResultName].Value).Value; }
      private set { ((IntValue)this[ModelDepthResultName].Value).Value = value; }
    }

    [StorableConstructor]
    private SymbolicClassificationSolution(StorableConstructorFlag _) : base(_) { }
    private SymbolicClassificationSolution(SymbolicClassificationSolution original, Cloner cloner)
      : base(original, cloner) {
    }
    public SymbolicClassificationSolution(ISymbolicClassificationModel model, IClassificationProblemData problemData)
      : base(model, problemData) {
      foreach (var node in model.SymbolicExpressionTree.Root.IterateNodesPrefix().OfType<SymbolicExpressionTreeTopLevelNode>())
        node.SetGrammar(null);

      Add(new Result(ModelLengthResultName, "Length of the symbolic classification model.", new IntValue()));
      Add(new Result(ModelDepthResultName, "Depth of the symbolic classification model.", new IntValue()));
      RecalculateResults();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicClassificationSolution(this, cloner);
    }

    protected override void RecalculateResults() {
      base.RecalculateResults();
      ModelLength = Model.SymbolicExpressionTree.Length;
      ModelDepth = Model.SymbolicExpressionTree.Depth;
    }
  }
}
