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
  [Item("PrinsManipulator", "An operator which manipulates a VRP representation.")]
  [StorableType("AF313602-8AFC-4722-AA47-FFDCF39CF3E4")]
  public abstract class PrinsManipulator : VRPManipulator, IStochasticOperator, IPrinsOperator {
    public ILookupParameter<IRandom> RandomParameter {
      get { return (LookupParameter<IRandom>)Parameters["Random"]; }
    }

    [StorableConstructor]
    protected PrinsManipulator(StorableConstructorFlag _) : base(_) { }

    public PrinsManipulator()
      : base() {
      Parameters.Add(new LookupParameter<IRandom>("Random", "The pseudo random number generator which should be used for stochastic manipulation operators."));
    }

    protected PrinsManipulator(PrinsManipulator original, Cloner cloner)
      : base(original, cloner) {
    }

    protected abstract void Manipulate(IRandom random, PrinsEncoding individual);

    public override IOperation InstrumentedApply() {
      IVRPEncoding solution = VRPToursParameter.ActualValue;
      if (!(solution is PrinsEncoding)) {
        VRPToursParameter.ActualValue = PrinsEncoding.ConvertFrom(solution, ProblemInstance);
      }

      Manipulate(RandomParameter.ActualValue, VRPToursParameter.ActualValue as PrinsEncoding);

      return base.InstrumentedApply();
    }
  }
}
