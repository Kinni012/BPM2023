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
  [StorableType("b9d7d1f7-c603-45ef-b372-71b82446fa72")]
  public interface IRegressionSolution : IDataAnalysisSolution {
    new IRegressionModel Model { get; }
    new IRegressionProblemData ProblemData { get; set; }

    IEnumerable<double> EstimatedValues { get; }
    IEnumerable<double> EstimatedTrainingValues { get; }
    IEnumerable<double> EstimatedTestValues { get; }
    IEnumerable<double> GetEstimatedValues(IEnumerable<int> rows);

    double TrainingMeanSquaredError { get; }
    double TestMeanSquaredError { get; }
    double TrainingMeanAbsoluteError { get; }
    double TestMeanAbsoluteError { get; }
    double TrainingRSquared { get; }
    double TestRSquared { get; }
    double TrainingRelativeError { get; }
    double TestRelativeError { get; }
    double TrainingNormalizedMeanSquaredError { get; }
    double TestNormalizedMeanSquaredError { get; }
    double TrainingRootMeanSquaredError { get; }
    double TestRootMeanSquaredError { get; }
  }
}
