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
using System.Data.Linq;
using System.Linq;
using HeuristicLab.Services.Access.DataAccess;
using HeuristicLab.Services.Hive.DataAccess;
using HeuristicLab.Services.Hive.DataAccess.Manager;

namespace HeuristicLab.Services.Hive {
  public class HiveStatisticsGenerator : IStatisticsGenerator {

    private const string UnknownUserName = "Unknown";
    private static readonly TimeSpan SmallestTimeSpan = new TimeSpan(0, 5, 0);
    private static readonly TaskState[] CompletedStates = { TaskState.Finished, TaskState.Aborted, TaskState.Failed };

    public void GenerateStatistics() {
      using (var pm = new PersistenceManager()) {

        pm.UseTransaction(() => {
          UpdateDimProjectTable(pm);
          pm.SubmitChanges();
        });

        pm.UseTransaction(() => {
          UpdateDimUserTable(pm);
          
          UpdateDimJobTable(pm);
          UpdateDimClientsTable(pm);
          pm.SubmitChanges();
        });

        DimTime time = null;
        pm.UseTransaction(() => {
          time = UpdateDimTimeTable(pm);
          pm.SubmitChanges();
        });

        if (time != null) {
          pm.UseTransaction(() => {
            UpdateFactClientInfoTable(time, pm);
            pm.SubmitChanges();
            UpdateFactProjectInfoTable(time, pm);
            pm.SubmitChanges();
          });

          pm.UseTransaction(() => {
            try {
              UpdateFactTaskTable(pm);
              UpdateExistingDimJobs(pm);
              FlagJobsForDeletion(pm);
              pm.SubmitChanges();
            }
            catch (DuplicateKeyException e) {
              var logger = LogFactory.GetLogger(typeof(HiveStatisticsGenerator).Namespace);
              logger.Log(string.Format(
                @"Propable change from summertime to wintertime, resulting in overlapping times.
                          On wintertime to summertime change, slave timeouts and a fact gap will occur. 
                          Exception Details: {0}", e));
            }
          });
        }
      }
    }

    private DimTime UpdateDimTimeTable(PersistenceManager pm) {
      var dimTimeDao = pm.DimTimeDao;
      var now = DateTime.Now;
      var timeEntry = new DimTime {
        Time = now,
        Minute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0),
        Hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0),
        Day = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0),
        Month = new DateTime(now.Year, now.Month, 1, 0, 0, 0),
        Year = new DateTime(now.Year, 1, 1, 0, 0, 0)
      };
      return dimTimeDao.Save(timeEntry);
    }

    private void UpdateDimUserTable(PersistenceManager pm) {
      var dimUserDao = pm.DimUserDao;
      var resourceDao = pm.ResourceDao;
      var jobDao = pm.JobDao;
      var existingUserIds = dimUserDao.GetAll().Select(x => x.UserId);
      var vaildResourceOwnerIds = resourceDao.GetResourcesWithValidOwner().Select(x => x.OwnerUserId.Value);
      var jobOwnerIds = jobDao.GetAll().Select(x => x.OwnerUserId);
      var newUserIds = vaildResourceOwnerIds
        .Union(jobOwnerIds)
        .Where(id => !existingUserIds.Contains(id))
        .ToList();
      dimUserDao.Save(newUserIds.Select(x => new DimUser {
        UserId = x,
        Name = GetUserName(x)
      }));
    }

    // add new projects
    // delete expired projects
    // update information of existing projects
    private void UpdateDimProjectTable(PersistenceManager pm) {
      var projectDao = pm.ProjectDao;
      var dimProjectDao = pm.DimProjectDao;

      var projects = projectDao.GetAll().ToList();
      var dimProjects = dimProjectDao.GetAllOnlineProjects().ToList();

      var onlineProjects = dimProjects.Where(x => projects.Select(y => y.ProjectId).Contains(x.ProjectId));
      var addedProjects = projects.Where(x => !dimProjects.Select(y => y.ProjectId).Contains(x.ProjectId));
      var removedProjects = dimProjects.Where(x => !projects.Select(y => y.ProjectId).Contains(x.ProjectId));

      // set expiration time of removed projects
      foreach (var p in removedProjects) {
        p.DateExpired = DateTime.Now;
      }

      // add new projects
      dimProjectDao.Save(addedProjects.Select(x => new DimProject {
        ProjectId = x.ProjectId,
        ParentProjectId = x.ParentProjectId,
        Name = x.Name,
        Description = x.Description,
        OwnerUserId = x.OwnerUserId,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        DateCreated = x.DateCreated,
        DateExpired = null
      }));

      // expire project if its parent has changed and create a new entry
      // otherwise perform "normal" update
      foreach (var dimP in onlineProjects) {
        var p = projects.Where(x => x.ProjectId == dimP.ProjectId).SingleOrDefault();
        if (p != null) {
          if (dimP.ParentProjectId == null ? p.ParentProjectId != null : dimP.ParentProjectId != p.ParentProjectId) { // or: (!object.Equals(dimP.ParentProjectId, p.ParentProjectId))
            dimP.DateExpired = DateTime.Now;
            dimProjectDao.Save(new DimProject {
              ProjectId = p.ProjectId,
              ParentProjectId = p.ParentProjectId,
              Name = p.Name,
              Description = p.Description,
              OwnerUserId = p.OwnerUserId,
              StartDate = p.StartDate,
              EndDate = p.EndDate,
              DateCreated = p.DateCreated,
              DateExpired = null
            });
          } else {
            dimP.Name = p.Name;
            dimP.Description = p.Description;
            dimP.OwnerUserId = p.OwnerUserId;
            dimP.StartDate = p.StartDate;
            dimP.EndDate = p.EndDate;
          }
        }
      }
    }

    private void UpdateDimJobTable(PersistenceManager pm) {
      var dimProjectDao = pm.DimProjectDao;
      var dimJobDao = pm.DimJobDao;
      var jobDao = pm.JobDao;
      var taskDao = pm.TaskDao;
      var dimJobIds = dimJobDao.GetAll().Select(x => x.JobId);
      var newJobs = jobDao.GetAll()
        .Where(x => !dimJobIds.Contains(x.JobId))
        .Select(x => new {
          JobId = x.JobId,
          UserId = x.OwnerUserId,
          JobName = x.Name ?? string.Empty,
          DateCreated = x.DateCreated,
          ProjectId = dimProjectDao.GetLastValidIdByProjectId(x.ProjectId),
          TotalTasks = taskDao.GetAll().Count(y => y.JobId == x.JobId)
        })
        .ToList();
      dimJobDao.Save(newJobs.Select(x => new DimJob {
        JobId = x.JobId,
        JobName = x.JobName,
        UserId = x.UserId,
        UserName = GetUserName(x.UserId),
        DateCreated = x.DateCreated,
        ProjectId = x.ProjectId,
        TotalTasks = x.TotalTasks,
        CompletedTasks = 0,
        DateCompleted = null
      }));
    }

    private void UpdateExistingDimJobs(PersistenceManager pm) {
      var dimProjectDao = pm.DimProjectDao;
      var jobDao = pm.JobDao;
      var dimJobDao = pm.DimJobDao;
      var factTaskDao = pm.FactTaskDao;
      foreach (var dimJob in dimJobDao.GetNotCompletedJobs()) {
        var taskStates = factTaskDao.GetByJobId(dimJob.JobId)
            .GroupBy(x => x.TaskState)
            .Select(x => new {
              State = x.Key,
              Count = x.Count()
            }).ToList();
        int totalTasks = 0, completedTasks = 0;
        foreach (var state in taskStates) {
          totalTasks += state.Count;
          if (CompletedStates.Contains(state.State)) {
            completedTasks += state.Count;
          }
        }
        var job = jobDao.GetById(dimJob.JobId);
        if (totalTasks == completedTasks) {
          var completeDate = factTaskDao.GetLastCompletedTaskFromJob(dimJob.JobId);
          if (completeDate == null) {
            if (job == null) {
              completeDate = DateTime.Now;
            }
          }
          dimJob.DateCompleted = completeDate;
        }
        if(job != null) {
          dimJob.JobName = job.Name;
          dimJob.ProjectId = dimProjectDao.GetLastValidIdByProjectId(job.ProjectId);
        }

        dimJob.TotalTasks = totalTasks;
        dimJob.CompletedTasks = completedTasks;
      }
    }

    private void FlagJobsForDeletion(PersistenceManager pm) {
      var jobDao = pm.JobDao;
      var jobs = jobDao.GetJobsReadyForDeletion();
      foreach(var job in jobs) {
        job.State = JobState.DeletionPending;
      }
    }

    private void UpdateDimClientsTable(PersistenceManager pm) {
      var dimClientDao = pm.DimClientDao;
      var resourceDao = pm.ResourceDao;

      var resources = resourceDao.GetAll().ToList();
      var dimClients = dimClientDao.GetAllOnlineClients().ToList();

      var onlineClients = dimClients.Where(x => resources.Select(y => y.ResourceId).Contains(x.ResourceId));
      var addedResources = resources.Where(x => !dimClients.Select(y => y.ResourceId).Contains(x.ResourceId));
      var removedResources = dimClients.Where(x => !resources.Select(y => y.ResourceId).Contains(x.ResourceId));

      // set expiration time of removed resources
      foreach(var r in removedResources) {
        r.DateExpired = DateTime.Now;
      }

      // add new resources
      dimClientDao.Save(addedResources.Select(x => new DimClient {
        ResourceId = x.ResourceId,
        ParentResourceId = x.ParentResourceId,
        Name = x.Name,
        ResourceType = x.ResourceType,
        DateCreated = DateTime.Now,
        DateExpired = null
      }));

      // expire client if its parent has changed and create a new entry
      // otherwise perform "normal" update
      foreach(var dimc in onlineClients) {
        var r = resources.Where(x => x.ResourceId == dimc.ResourceId).SingleOrDefault();
        if(r != null) {
          if(dimc.ParentResourceId == null ? r.ParentResourceId != null : dimc.ParentResourceId != r.ParentResourceId) {
            var now = DateTime.Now;
            dimc.DateExpired = now;
            dimClientDao.Save(new DimClient {
              ResourceId = r.ResourceId,
              ParentResourceId = r.ParentResourceId,
              Name = r.Name,
              ResourceType = r.ResourceType,
              DateCreated = now,
              DateExpired = null
            });
          } else {
            dimc.Name = r.Name;
          }
        }
      }
    }

    //// (1) for new slaves (not yet reported in Table DimClients) ...
    //// and modified slaves (name or parent resource changed) a new DimClient-entry is created
    //// (2) for already reported removed and modifid clients the expiration date is set
    //private void UpdateDimClientsTableOld(PersistenceManager pm) {
    //  var dimClientDao = pm.DimClientDao;
    //  var slaveDao = pm.SlaveDao;
    //  var slaves = slaveDao.GetAll();
    //  var recentlyAddedClients = dimClientDao.GetAllOnlineClients();
    //  var slaveIds = slaves.Select(x => x.ResourceId);

    //  var removedClientIds = recentlyAddedClients
    //    .Where(x => !slaveIds.Contains(x.ResourceId))
    //    .Select(x => x.Id);
    //  var modifiedClients =
    //    from slave in slaves
    //    join client in recentlyAddedClients on slave.ResourceId equals client.ResourceId
    //    where (slave.Name != client.Name
    //           || slave.ParentResourceId == null && client.ResourceGroupId != null // because both can be null and null comparison
    //           || slave.ParentResourceId != null && client.ResourceGroupId == null // does return no entry on the sql server
    //           || slave.ParentResourceId != client.ResourceGroupId
    //           || ((slave.ParentResource != null) && slave.ParentResource.ParentResourceId != client.ResourceGroup2Id))
    //    select new {
    //      SlaveId = slave.ResourceId,
    //      ClientId = client.Id
    //    };
    //  var clientIds = dimClientDao.GetAllOnlineClients().Select(x => x.ResourceId);
    //  var modifiedClientIds = modifiedClients.Select(x => x.SlaveId);
    //  var newClients = slaves
    //    .Where(x => !clientIds.Contains(x.ResourceId)
    //                || modifiedClientIds.Contains(x.ResourceId))
    //    .Select(x => new {
    //      x.ResourceId,
    //      x.Name,
    //      ResourceGroupId = x.ParentResourceId,
    //      GroupName = x.ParentResource != null ? x.ParentResource.Name : null,
    //      ResourceGroup2Id = x.ParentResource != null ? x.ParentResource.ParentResourceId : null,
    //      GroupName2 = x.ParentResource != null ? x.ParentResource.ParentResource != null ? x.ParentResource.ParentResource.Name : null : null
    //    })
    //    .ToList();

    //  var clientsToUpdate = removedClientIds.Union(modifiedClients.Select(x => x.ClientId));
    //  dimClientDao.UpdateExpirationTime(clientsToUpdate, DateTime.Now);
    //  dimClientDao.Save(newClients.Select(x => new DimClient {
    //    ResourceId = x.ResourceId,
    //    Name = x.Name,
    //    ExpirationTime = null,
    //    ResourceGroupId = x.ResourceGroupId,
    //    GroupName = x.GroupName,
    //    ResourceGroup2Id = x.ResourceGroup2Id,
    //    GroupName2 = x.GroupName2
    //  }));
    //}


    private void UpdateFactClientInfoTable(DimTime newTime, PersistenceManager pm) {
      var factClientInfoDao = pm.FactClientInfoDao;
      var slaveDao = pm.SlaveDao;
      var dimClientDao = pm.DimClientDao;

      var newRawFactInfos =
        from s in slaveDao.GetAll()
        join c in dimClientDao.GetAllOnlineSlaves() on s.ResourceId equals c.ResourceId
        join lcf in factClientInfoDao.GetLastUpdateTimestamps() on c.ResourceId equals lcf.ResourceId into joinCf
        from cf in joinCf.DefaultIfEmpty()
        select new {
          ClientId = c.Id,
          UserId = s.OwnerUserId ?? Guid.Empty,
          TotalCores = s.Cores ?? 0,
          FreeCores = s.FreeCores ?? 0,
          TotalMemory = s.Memory ?? 0,
          FreeMemory = s.FreeMemory ?? 0,
          CpuUtilization = s.CpuUtilization,
          SlaveState = s.SlaveState,
          IsAllowedToCalculate = s.IsAllowedToCalculate,
          LastFactTimestamp = cf.Timestamp
        };

      factClientInfoDao.Save(
        from x in newRawFactInfos.ToList()
        let duration = x.LastFactTimestamp != null
                       ? (int)(newTime.Time - (DateTime)x.LastFactTimestamp).TotalSeconds
                       : (int)SmallestTimeSpan.TotalSeconds
        select new FactClientInfo {
          ClientId = x.ClientId,
          DimTime = newTime,
          UserId = x.UserId,
          NumUsedCores = x.TotalCores - x.FreeCores,
          NumTotalCores = x.TotalCores,
          UsedMemory = x.TotalMemory - x.FreeMemory,
          TotalMemory = x.TotalMemory,
          CpuUtilization = Math.Round(x.CpuUtilization, 2),
          SlaveState = x.SlaveState,
          IdleTime = x.SlaveState == SlaveState.Idle && x.IsAllowedToCalculate ? duration : 0,
          UnavailableTime = x.SlaveState == SlaveState.Idle && !x.IsAllowedToCalculate ? duration : 0,
          OfflineTime = x.SlaveState == SlaveState.Offline ? duration : 0,
          IsAllowedToCalculate = x.IsAllowedToCalculate
        }
      );
    }

    private void UpdateFactProjectInfoTable(DimTime newTime, PersistenceManager pm) {
      var factProjectInfoDao = pm.FactProjectInfoDao;
      var dimProjectDao = pm.DimProjectDao;
      var projectDao = pm.ProjectDao;

      var projectAvailabilityStats = projectDao.GetAvailabilityStatsPerProject();
      var projectUsageStats = projectDao.GetUsageStatsPerProject();
      var dimProjects = dimProjectDao.GetAllOnlineProjects().ToList();

      factProjectInfoDao.Save(
        from dimp in dimProjects
        let aStats = projectAvailabilityStats.Where(x => x.ProjectId == dimp.ProjectId).SingleOrDefault()
        let uStats = projectUsageStats.Where(x => x.ProjectId == dimp.ProjectId).SingleOrDefault()
        select new FactProjectInfo {
            ProjectId = dimp.Id,
            DimTime = newTime,
            NumTotalCores = aStats != null ? aStats.Cores : 0,
            TotalMemory = aStats != null ? aStats.Memory : 0,
            NumUsedCores = uStats != null ? uStats.Cores : 0,
            UsedMemory = uStats != null ? uStats.Memory : 0
          }
        );
    }

    private void UpdateFactTaskTable(PersistenceManager pm) {
      var factTaskDao = pm.FactTaskDao;
      var taskDao = pm.TaskDao;
      var dimClientDao = pm.DimClientDao;

      var factTaskIds = factTaskDao.GetAll().Select(x => x.TaskId);
      var notFinishedFactTasks = factTaskDao.GetNotFinishedTasks();
      //var notFinishedFactTasks = factTaskDao.GetNotFinishedTasks().Select(x => new {
      //  x.TaskId,
      //  x.LastClientId
      //});

      // query several properties for all new and not finished tasks
      // in order to use them later either...
      // (1) to update the fact task entry of not finished tasks
      // (2) to insert a new fact task entry for new tasks
      var newAndNotFinishedTasks =
        (from task in taskDao.GetAllChildTasks()
         let stateLogs = task.StateLogs.OrderByDescending(x => x.DateTime)
         let lastSlaveId = stateLogs.First(x => x.SlaveId != null).SlaveId
         where (!factTaskIds.Contains(task.TaskId)
                || notFinishedFactTasks.Select(x => x.TaskId).Contains(task.TaskId))
         join lastFactTask in notFinishedFactTasks on task.TaskId equals lastFactTask.TaskId into lastFactPerTask
         from lastFact in lastFactPerTask.DefaultIfEmpty()
         join client in dimClientDao.GetAllOnlineClients() on lastSlaveId equals client.ResourceId into clientsPerSlaveId
         from client in clientsPerSlaveId.DefaultIfEmpty()
         select new {
           TaskId = task.TaskId,
           JobId = task.JobId,
           Priority = task.Priority,
           CoresRequired = task.CoresNeeded,
           MemoryRequired = task.MemoryNeeded,
           State = task.State,
           StateLogs = stateLogs.OrderBy(x => x.DateTime),
           LastClientId = client != null
                          ? client.Id : lastFact != null
                          ? lastFact.LastClientId : (Guid?)null,
           NotFinishedTask = notFinishedFactTasks.Any(y => y.TaskId == task.TaskId)
         }).ToList();

      // (1) update data of already existing facts
      // i.e. for all in newAndNotFinishedTasks where NotFinishedTask = true
      foreach (var notFinishedFactTask in notFinishedFactTasks) {
        var nfftUpdate = newAndNotFinishedTasks.Where(x => x.TaskId == notFinishedFactTask.TaskId).SingleOrDefault();
        if(nfftUpdate != null) {
          var taskData = CalculateFactTaskData(nfftUpdate.StateLogs);

          notFinishedFactTask.StartTime = taskData.StartTime;
          notFinishedFactTask.EndTime = taskData.EndTime;
          notFinishedFactTask.LastClientId = nfftUpdate.LastClientId;
          notFinishedFactTask.Priority = nfftUpdate.Priority;
          notFinishedFactTask.CoresRequired = nfftUpdate.CoresRequired;
          notFinishedFactTask.MemoryRequired = nfftUpdate.MemoryRequired;
          notFinishedFactTask.NumCalculationRuns = taskData.CalculationRuns;
          notFinishedFactTask.NumRetries = taskData.Retries;
          notFinishedFactTask.WaitingTime = taskData.WaitingTime;
          notFinishedFactTask.CalculatingTime = taskData.CalculatingTime;
          notFinishedFactTask.TransferTime = taskData.TransferTime;
          notFinishedFactTask.TaskState = nfftUpdate.State;
          notFinishedFactTask.Exception = taskData.Exception;
          notFinishedFactTask.InitialWaitingTime = taskData.InitialWaitingTime;
        }
      }

      // (2) insert facts for new tasks
      // i.e. for all in newAndNotFinishedTasks where NotFinishedTask = false
      factTaskDao.Save(
        from x in newAndNotFinishedTasks
        where !x.NotFinishedTask
        let taskData = CalculateFactTaskData(x.StateLogs)
        select new FactTask {
          TaskId = x.TaskId,
          JobId = x.JobId,
          StartTime = taskData.StartTime,
          EndTime = taskData.EndTime,
          LastClientId = x.LastClientId,
          Priority = x.Priority,
          CoresRequired = x.CoresRequired,
          MemoryRequired = x.MemoryRequired,
          NumCalculationRuns = taskData.CalculationRuns,
          NumRetries = taskData.Retries,
          WaitingTime = taskData.WaitingTime,
          CalculatingTime = taskData.CalculatingTime,
          TransferTime = taskData.TransferTime,
          TaskState = x.State,
          Exception = taskData.Exception,
          InitialWaitingTime = taskData.InitialWaitingTime
        });


      ////update data of already existing facts
      //foreach (var notFinishedTask in factTaskDao.GetNotFinishedTasks()) {
      //  var ntc = newTasks.Where(x => x.TaskId == notFinishedTask.TaskId);
      //  if (ntc.Any()) {
      //    var x = ntc.Single();
      //    var taskData = CalculateFactTaskData(x.StateLogs);

      //    notFinishedTask.StartTime = taskData.StartTime;
      //    notFinishedTask.EndTime = taskData.EndTime;
      //    notFinishedTask.LastClientId = x.LastClientId;
      //    notFinishedTask.Priority = x.Priority;
      //    notFinishedTask.CoresRequired = x.CoresRequired;
      //    notFinishedTask.MemoryRequired = x.MemoryRequired;
      //    notFinishedTask.NumCalculationRuns = taskData.CalculationRuns;
      //    notFinishedTask.NumRetries = taskData.Retries;
      //    notFinishedTask.WaitingTime = taskData.WaitingTime;
      //    notFinishedTask.CalculatingTime = taskData.CalculatingTime;
      //    notFinishedTask.TransferTime = taskData.TransferTime;
      //    notFinishedTask.TaskState = x.State;
      //    notFinishedTask.Exception = taskData.Exception;
      //    notFinishedTask.InitialWaitingTime = taskData.InitialWaitingTime;
      //  }
      //}
    }

    private string GetUserName(Guid userId) {
      try {
        // we cannot use the ServiceLocator.Instance.UserManager since the janitor service
        // is not hosted in the iis the MemberShip.GetUser method causes exceptions
        // needs to be further investigated current workaround: use the authenticationcontext
        // we could also connect to the access service to get the user name
        using (ASPNETAuthenticationDataContext dc = new ASPNETAuthenticationDataContext()) {
          var user = dc.aspnet_Users.SingleOrDefault(x => x.UserId == userId);
          return user != null ? user.UserName : UnknownUserName;
        }
      }
      catch (Exception) {
        return UnknownUserName;
      }
    }

    private class FactTaskData {
      public int CalculationRuns { get; set; }
      public int Retries { get; set; }
      public long CalculatingTime { get; set; }
      public long WaitingTime { get; set; }
      public long TransferTime { get; set; }
      public long InitialWaitingTime { get; set; }
      public string Exception { get; set; }
      public DateTime? StartTime { get; set; }
      public DateTime? EndTime { get; set; }
    }

    private FactTaskData CalculateFactTaskData(IEnumerable<StateLog> stateLogs) {
      var factTaskData = new FactTaskData();
      var enumerator = stateLogs.GetEnumerator();
      if (enumerator.MoveNext()) {
        StateLog current = enumerator.Current, first = current, prev = null;
        while (current != null) {
          var next = enumerator.MoveNext() ? enumerator.Current : null;
          int timeSpanInSeconds;
          if (next != null) {
            timeSpanInSeconds = (int)(next.DateTime - current.DateTime).TotalSeconds;
          } else {
            timeSpanInSeconds = (int)(DateTime.Now - current.DateTime).TotalSeconds;
            factTaskData.Exception = current.Exception;
          }
          switch (current.State) {
            case TaskState.Calculating:
              factTaskData.CalculatingTime += timeSpanInSeconds;
              factTaskData.CalculationRuns++;
              if (factTaskData.CalculationRuns == 1) {
                factTaskData.StartTime = current.DateTime;
                factTaskData.InitialWaitingTime = (int)(current.DateTime - first.DateTime).TotalSeconds;
              }
              if (prev != null && prev.State != TaskState.Transferring) {
                factTaskData.Retries++;
              }
              break;

            case TaskState.Waiting:
              factTaskData.WaitingTime += timeSpanInSeconds;
              break;

            case TaskState.Transferring:
              factTaskData.TransferTime += timeSpanInSeconds;
              break;

            case TaskState.Finished:
            case TaskState.Failed:
            case TaskState.Aborted:
              factTaskData.EndTime = current.DateTime;
              break;
          }
          prev = current;
          current = next;
        }
      }
      return factTaskData;
    }
  }
}