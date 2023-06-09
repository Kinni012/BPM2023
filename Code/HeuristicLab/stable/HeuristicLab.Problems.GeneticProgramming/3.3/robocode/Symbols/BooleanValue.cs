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

using HeuristicLab.Common;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HEAL.Attic;

namespace HeuristicLab.Problems.GeneticProgramming.Robocode {
  [StorableType("885EE784-E3EC-44D7-B957-A72EBF556F90")]
  public class BooleanValue : Symbol {
    public override int MinimumArity { get { return 0; } }
    public override int MaximumArity { get { return 0; } }

    [StorableConstructor]
    protected BooleanValue(StorableConstructorFlag _) : base(_) { }
    protected BooleanValue(BooleanValue original, Cloner cloner) : base(original, cloner) { }

    public BooleanValue() : base("BooleanValue", "A Boolean value.") { }

    public override ISymbolicExpressionTreeNode CreateTreeNode() {
      return new BooleanTreeNode(this);
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new BooleanValue(this, cloner);
    }
  }
}
