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

using System.Collections.Generic;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("ecb5d2f6-8041-4d07-aeff-e7fc0b5c355e")]
  public interface IDiscriminantFunctionClassificationSolution : IClassificationSolution {
    new IDiscriminantFunctionClassificationModel Model { get; }

    IEnumerable<double> EstimatedValues { get; }
    IEnumerable<double> EstimatedTrainingValues { get; }
    IEnumerable<double> EstimatedTestValues { get; }
    IEnumerable<double> GetEstimatedValues(IEnumerable<int> rows);
  }
}
