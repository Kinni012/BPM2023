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

namespace HeuristicLab.Services.WebApp.Statistics.WebApi.DataTransfer {
  public class Client {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int TotalCores { get; set; }
    public int UsedCores { get; set; }
    public int TotalMemory { get; set; }
    public int UsedMemory { get; set; }
    public double CpuUtilization { get; set; }
    public string State { get; set; }
    public DateTime LastUpdate { get; set; }
    public Guid? GroupId { get; set; }
    public string GroupName { get; set; }
    public bool IsAllowedToCalculate { get; set; }
  }
}