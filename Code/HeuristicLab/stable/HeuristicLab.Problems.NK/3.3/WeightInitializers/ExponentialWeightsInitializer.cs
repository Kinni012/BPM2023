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
using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HEAL.Attic;

namespace HeuristicLab.Problems.NK {
  [Item("ExponentialWeightsInitializer", "Assigns exponentially increasing weights.")]
  [StorableType("6F927828-8292-49E6-95CB-F4CFA97018CA")]
  public sealed class ExponentialWeightsInitializer : ParameterizedNamedItem, IWeightsInitializer {
    [StorableConstructor]
    private ExponentialWeightsInitializer(StorableConstructorFlag _) : base(_) { }
    private ExponentialWeightsInitializer(ExponentialWeightsInitializer original, Cloner cloner)
      : base(original, cloner) {
    }
    public ExponentialWeightsInitializer() { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new ExponentialWeightsInitializer(this, cloner);
    }

    public IEnumerable<double> GetWeights(int nComponents) {
      for (int i = 0; i < nComponents; i++)
        yield return Math.Pow(2, i);
    }
  }
}
