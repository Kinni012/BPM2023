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
using System.Linq;
using HeuristicLab.Common;

namespace HeuristicLab.Problems.Instances.DataAnalysis {
  public class SalutowiczFunctionTwoDimensional : ArtificialRegressionDataDescriptor {

    public override string Name { get { return "Vladislavleva-3 F3(X1, X2) = exp(-X1) * X1³ * cos(X1) * sin(X1) * (cos(X1)sin(X1)² - 1)(X2 - 5)"; } }
    public override string Description {
      get {
        return "Paper: Order of Nonlinearity as a Complexity Measure for Models Generated by Symbolic Regression via Pareto Genetic Programming " + Environment.NewLine
        + "Authors: Ekaterina J. Vladislavleva, Member, IEEE, Guido F. Smits, Member, IEEE, and Dick den Hertog" + Environment.NewLine
        + "Function: F3(X1, X2) = exp(-X1) * X1³ * cos(X1) * sin(X1) * (cos(X1)sin(X1)² - 1)(X2 - 5)" + Environment.NewLine
        + "Training Data: 600 points X1 = (0.05:0.1:10), X2 = (0.05:2:10.05)" + Environment.NewLine
        + "Test Data: 221 * 23 points X1 = (-0.5:0.05:10.5), X2 = (-0.5:0.5:10.5)" + Environment.NewLine
        + "Function Set: +, -, *, /, square, e^x, e^-x, sin(x), cos(x), x^eps, x + eps, x + eps";
      }
    }
    protected override string TargetVariable { get { return "Y"; } }
    protected override string[] VariableNames { get { return new string[] { "X1", "X2", "Y" }; } }
    protected override string[] AllowedInputVariables { get { return new string[] { "X1", "X2" }; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 600; } }
    protected override int TestPartitionStart { get { return 600; } }
    protected override int TestPartitionEnd { get { return 600 + (221 * 23); } }

    protected override List<List<double>> GenerateValues() {
      List<List<double>> data = new List<List<double>>();
      List<List<double>> trainingData = new List<List<double>>() {
        SequenceGenerator.GenerateSteps(0.05m, 10, 0.1m).Select(v => (double)v).ToList(),
        SequenceGenerator.GenerateSteps(0.05m, 10.05m, 2).Select(v => (double)v).ToList()
      };

      List<List<double>> testData = new List<List<double>>() {
        SequenceGenerator.GenerateSteps(-0.5m, 10.5m, 0.05m).Select(v => (double)v).ToList(),
        SequenceGenerator.GenerateSteps(-0.5m, 10.5m, 0.5m).Select(v => (double)v).ToList()
      };

      var trainingComb = ValueGenerator.GenerateAllCombinationsOfValuesInLists(trainingData).ToList<IEnumerable<double>>();
      var testComb = ValueGenerator.GenerateAllCombinationsOfValuesInLists(testData).ToList<IEnumerable<double>>();

      for (int i = 0; i < AllowedInputVariables.Count(); i++) {
        data.Add(trainingComb[i].ToList());
        data[i].AddRange(testComb[i]);
      }

      double x1, x2;
      List<double> results = new List<double>();
      for (int i = 0; i < data[0].Count; i++) {
        x1 = data[0][i];
        x2 = data[1][i];
        results.Add(Math.Exp(-x1) * Math.Pow(x1, 3) * Math.Cos(x1) * Math.Sin(x1) * (Math.Cos(x1) * Math.Pow(Math.Sin(x1), 2) - 1) * (x2 - 5));
      }
      data.Add(results);

      return data;
    }
  }
}
