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

using System.Drawing;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HEAL.Attic;

namespace HeuristicLab.Problems.Scheduling {
  [Item("JSMDecodingErrorPolicy", "Represents a policy for handling decoding errors.")]
  [StorableType("835B8FE8-8BEA-4E05-BB68-2538341AC2D0")]
  public sealed class JSMDecodingErrorPolicy : ValueTypeValue<JSMDecodingErrorPolicyTypes> {

    public static new Image StaticItemImage {
      get { return HeuristicLab.Common.Resources.VSImageLibrary.Enum; }
    }

    [StorableConstructor]
    private JSMDecodingErrorPolicy(StorableConstructorFlag _) : base(_) { }
    private JSMDecodingErrorPolicy(JSMDecodingErrorPolicy original, Cloner cloner) : base(original, cloner) { }
    public JSMDecodingErrorPolicy() : base() { }
    public JSMDecodingErrorPolicy(JSMDecodingErrorPolicyTypes types) : base(types) { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new JSMDecodingErrorPolicy(this, cloner);
    }
  }
}
