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
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HEAL.Attic;
using HeuristicLab.Problems.VehicleRouting.Encodings.General;
using HeuristicLab.Problems.VehicleRouting.Interfaces;

namespace HeuristicLab.Problems.VehicleRouting.Encodings.Prins {
  [Item("PrinsCrossover", "An operator which crosses two VRP representations.")]
  [StorableType("EAF32C5C-D67E-4EEB-8705-FAA577A40439")]
  public abstract class PrinsCrossover : VRPCrossover, IStochasticOperator, IPrinsOperator {

    public ILookupParameter<IRandom> RandomParameter {
      get { return (LookupParameter<IRandom>)Parameters["Random"]; }
    }

    [StorableConstructor]
    protected PrinsCrossover(StorableConstructorFlag _) : base(_) { }

    public PrinsCrossover()
      : base() {
      Parameters.Add(new LookupParameter<IRandom>("Random", "The pseudo random number generator which should be used for stochastic manipulation operators."));
    }

    protected PrinsCrossover(PrinsCrossover original, Cloner cloner)
      : base(original, cloner) {
    }

    protected abstract PrinsEncoding Crossover(IRandom random, PrinsEncoding parent1, PrinsEncoding parent2);

    public override IOperation InstrumentedApply() {
      ItemArray<IVRPEncoding> parents = new ItemArray<IVRPEncoding>(ParentsParameter.ActualValue.Length);
      for (int i = 0; i < ParentsParameter.ActualValue.Length; i++) {
        IVRPEncoding solution = ParentsParameter.ActualValue[i];

        if (!(solution is PrinsEncoding)) {
          parents[i] = PrinsEncoding.ConvertFrom(solution, ProblemInstance);
        } else {
          parents[i] = solution;
        }
      }
      ParentsParameter.ActualValue = parents;

      ChildParameter.ActualValue =
        Crossover(RandomParameter.ActualValue, parents[0] as PrinsEncoding, parents[1] as PrinsEncoding);

      return base.InstrumentedApply();
    }
  }
}
