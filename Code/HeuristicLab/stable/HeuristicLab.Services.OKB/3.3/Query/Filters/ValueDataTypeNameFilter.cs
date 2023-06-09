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
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using HeuristicLab.Services.OKB.DataAccess;

namespace HeuristicLab.Services.OKB.Query.Filters {
  public class ValueDataTypeNameFilter : IFilter {
    public DataTransfer.StringComparison Comparison { get; set; }
    public string Value { get; set; }

    public Expression<Func<Run, bool>> Expression {
      get {
        switch (Comparison) {
          case DataTransfer.StringComparison.Equal:
            return x => x.Values.Any(y => y.DataType.Name == Value);
          case DataTransfer.StringComparison.NotEqual:
            return x => x.Values.All(y => y.DataType.Name != Value);
          case DataTransfer.StringComparison.Contains:
            return x => x.Values.Any(y => y.DataType.Name.Contains(Value));
          case DataTransfer.StringComparison.NotContains:
            return x => x.Values.All(y => !y.DataType.Name.Contains(Value));
          case DataTransfer.StringComparison.Like:
            return x => x.Values.Any(y => SqlMethods.Like(y.DataType.Name, Value));
          case DataTransfer.StringComparison.NotLike:
            return x => x.Values.All(y => !SqlMethods.Like(y.DataType.Name, Value));
          default:
            return x => true;
        }
      }
    }

    public ValueDataTypeNameFilter(DataTransfer.StringComparison comparison, string value) {
      Comparison = comparison;
      Value = value;
    }
    public ValueDataTypeNameFilter(DataTransfer.StringComparisonFilter filter) {
      Comparison = filter.Comparison;
      Value = filter.Value;
    }
  }
}
