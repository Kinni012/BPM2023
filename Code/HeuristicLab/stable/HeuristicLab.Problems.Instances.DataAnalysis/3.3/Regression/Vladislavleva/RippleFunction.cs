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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.DataAnalysis {
  public class RippleFunction : ArtificialRegressionDataDescriptor {

    public override string Name { get { return "Vladislavleva-7 F7(X1, X2) = (X1 - 3)(X2 - 3) + 2 * sin((X1 - 4)(X2 - 4))"; } }
    public override string Description {
      get {
        return "Paper: Order of Nonlinearity as a Complexity Measure for Models Generated by Symbolic Regression via Pareto Genetic Programming " + Environment.NewLine
        + "Authors: Ekaterina J. Vladislavleva, Member, IEEE, Guido F. Smits, Member, IEEE, and Dick den Hertog" + Environment.NewLine
        + "Function: F7(X1, X2) = (X1 - 3)(X2 - 3) + 2 * sin((X1 - 4)(X2 - 4))" + Environment.NewLine
        + "Training Data: 300 points X1, X2 = Rand(0.05, 6.05)" + Environment.NewLine
        + "Test Data: 1000 points X1, X2 = Rand(-0.25, 6.35)" + Environment.NewLine
        + "Function Set: +, -, *, /, square, e^x, e^-x, sin(x), cos(x), x^eps, x + eps, x + eps";
      }
    }
    protected override string TargetVariable { get { return "Y"; } }
    protected override string[] VariableNames { get { return new string[] { "X1", "X2", "Y" }; } }
    protected override string[] AllowedInputVariables { get { return new string[] { "X1", "X2" }; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 300; } }
    protected override int TestPartitionStart { get { return 300; } }
    protected override int TestPartitionEnd { get { return 300 + 1000; } }
    public int Seed { get; private set; }

    public RippleFunction() : this((int)DateTime.Now.Ticks) { }

    public RippleFunction(int seed) : base() {
      Seed = seed;
    }
    protected override List<List<double>> GenerateValues() {
      List<List<double>> data = new List<List<double>>();
      var rand = new MersenneTwister((uint)Seed);
      for (int i = 0; i < AllowedInputVariables.Count(); i++) {
        data.Add(ValueGenerator.GenerateUniformDistributedValues(rand.Next(), 300, 0.05, 6.05).ToList());
      }

      for (int i = 0; i < AllowedInputVariables.Count(); i++) {
        data[i].AddRange(ValueGenerator.GenerateUniformDistributedValues(rand.Next(), 1000, -0.25, 6.35));
      }

      double x1, x2;
      List<double> results = new List<double>();
      for (int i = 0; i < data[0].Count; i++) {
        x1 = data[0][i];
        x2 = data[1][i];
        results.Add((x1 - 3) * (x2 - 3) + 2 * Math.Sin((x1 - 4) * (x2 - 4)));
      }
      data.Add(results);

      return data;
    }
  }
}
