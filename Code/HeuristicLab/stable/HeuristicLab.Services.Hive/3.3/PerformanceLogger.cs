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
using System.Diagnostics;
using HeuristicLab.Services.Hive.DataAccess;

namespace HeuristicLab.Services.Hive {
  public class PerformanceLogger : IDisposable {

    private readonly Stopwatch stopwatch;
    private string name;

    public PerformanceLogger(string name) {
      this.name = name;
      if (Properties.Settings.Default.ProfileServicePerformance) {
        stopwatch = new Stopwatch();
        stopwatch.Start();
      }
    }
    public void Dispose() {
      if (Properties.Settings.Default.ProfileServicePerformance) {
        stopwatch.Stop();
        LogFactory.GetLogger(this.GetType().Namespace)
          .Log(string.Format("{0} took {1}ms", name, stopwatch.ElapsedMilliseconds));
      }
    }
  }
}
