#region License Information
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
using System.Data.Linq.Mapping;
namespace HeuristicLab.Services.Hive.DataAccess {
  partial class HiveDataContext {
    // source: http://stackoverflow.com/questions/648196/random-row-from-linq-to-sql
    [Function(Name = "NEWID", IsComposable = true)]
    public Guid Random() {
      // to prove not used by our C# code... 
      throw new NotImplementedException();
    }
  }
}
