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
using HeuristicLab.Encodings.RealVectorEncoding;
using HEAL.Attic;

namespace HeuristicLab.Problems.TestFunctions {
  [Item("AckleyAdditiveMoveEvaluator", "Class for evaluating an additive move on the Ackley function.")]
  [StorableType("C28EF2FB-0EF2-4A69-910D-210A808A0420")]
  public class AckleyAdditiveMoveEvaluator : AdditiveMoveEvaluator {
    public override System.Type EvaluatorType {
      get { return typeof(AckleyEvaluator); }
    }

    [StorableConstructor]
    protected AckleyAdditiveMoveEvaluator(StorableConstructorFlag _) : base(_) { }
    protected AckleyAdditiveMoveEvaluator(AckleyAdditiveMoveEvaluator original, Cloner cloner) : base(original, cloner) { }
    public AckleyAdditiveMoveEvaluator() : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new AckleyAdditiveMoveEvaluator(this, cloner);
    }

    protected override double Evaluate(double quality, RealVector point, AdditiveMove move) {
      RealVectorAdditiveMoveWrapper wrapper = new RealVectorAdditiveMoveWrapper(move, point);
      return AckleyEvaluator.Apply(wrapper);
    }
  }
}
