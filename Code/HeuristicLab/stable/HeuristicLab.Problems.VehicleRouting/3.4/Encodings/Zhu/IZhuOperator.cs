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

using HeuristicLab.Problems.VehicleRouting.Variants;
using HEAL.Attic;

namespace HeuristicLab.Problems.VehicleRouting.Encodings.Zhu {
  [StorableType("CD11FA3C-3B50-4710-AAFC-32C9C61B3321")]
  public interface IZhuOperator :
    ISingleDepotOperator, IHomogenousCapacitatedOperator, ITimeWindowedOperator {
  }
}
