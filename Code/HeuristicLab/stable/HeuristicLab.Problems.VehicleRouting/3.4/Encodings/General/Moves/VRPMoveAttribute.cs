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

namespace HeuristicLab.Problems.VehicleRouting.Encodings.General {
  [Item("VRPMoveAttribute", "Base class for specifying a move attribute")]
  [StorableType("61460C9F-D313-47C1-9894-A6A5E4852DBB")]
  public class VRPMoveAttribute : Item {
    [Storable]
    public double MoveQuality { get; protected set; }

    [StorableConstructor]
    protected VRPMoveAttribute(StorableConstructorFlag _) : base(_) { }
    protected VRPMoveAttribute(VRPMoveAttribute original, Cloner cloner)
      : base(original, cloner) {
      this.MoveQuality = original.MoveQuality;
    }
    public VRPMoveAttribute() : this(0) { }
    public VRPMoveAttribute(double moveQuality)
      : base() {
      MoveQuality = moveQuality;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new VRPMoveAttribute(this, cloner);
    }
  }
}
