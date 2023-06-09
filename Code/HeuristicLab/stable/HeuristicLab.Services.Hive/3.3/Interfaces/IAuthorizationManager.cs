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
using HeuristicLab.Services.Hive.DataTransfer;
using System.Collections.Generic;

namespace HeuristicLab.Services.Hive {
  public interface IAuthorizationManager {
    /// <summary>
    /// Compares the current UserId with the given userId and takes appropriate actions if the mismatch
    /// </summary>
    void Authorize(Guid userId);

    void AuthorizeForTask(Guid taskId, Permission requiredPermission);

    void AuthorizeForJob(Guid jobId, Permission requiredPermission);

    void AuthorizeForResourceAdministration(Guid resourceId);

    void AuthorizeForProjectAdministration(Guid projectId, bool parentalOwnership);

    void AuthorizeForProjectResourceAdministration(Guid projectId, IEnumerable<Guid> resourceIds);

    void AuthorizeProjectForResourcesUse(Guid projectId, IEnumerable<Guid> resourceIds);

    void AuthorizeUserForProjectUse(Guid userId, Guid projectId);
  }
}
