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
using HeuristicLab.Core;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("D5A42FE7-D21B-48F7-A812-FE69724D0098")]
  [Item(Name = "Constant Classification Solution", Description = "Represents a constant classification solution (model + data).")]
  public class ConstantClassificationSolution : ClassificationSolution {
    public new ConstantModel Model {
      get { return (ConstantModel)base.Model; }
      set { base.Model = value; }
    }

    [StorableConstructor]
    protected ConstantClassificationSolution(StorableConstructorFlag _) : base(_) { }
    protected ConstantClassificationSolution(ConstantClassificationSolution original, Cloner cloner) : base(original, cloner) { }
    public ConstantClassificationSolution(ConstantModel model, IClassificationProblemData problemData)
      : base(model, problemData) {
      RecalculateResults();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new ConstantClassificationSolution(this, cloner);
    }
  }
}
