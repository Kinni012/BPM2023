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
using System.Web.Security;

namespace HeuristicLab.Services.Access {
  public interface IUserManager {
    MembershipUser CurrentUser { get; }
    Guid CurrentUserId { get; }
    MembershipUser GetUserByName(string username);
    MembershipUser GetUserById(Guid userId);
    string GetUserNameById(Guid userId);
    IEnumerable<Guid> GetUserGroupIdsOfUser(Guid userId);
    bool VerifyUser(Guid userId, List<Guid> allowedUserGroups);
    IEnumerable<DataTransfer.UserGroupMapping> GetUserGroupMapping();
  }
}
