using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HeuristicLab.Clients.Hive;
using HeuristicLab.Core;
using HeuristicLab.Optimization;

namespace HeuristicLab.SimGenOpt
{
    public static class HiveApi
    {
        private static readonly object locker = new object();

        public class Options
        {
            public Guid ProjectId { get; set; }
            public IEnumerable<Guid> ResourceIds { get; set; }
            public string JobName { get; set; }
            public bool Distribute { get; set; }
        }

        public static IEnumerable<T> ExecuteInHive<T>(IEnumerable<T> executables, CancellationToken cancellationToken) where T : IExecutable
        {
            return ExecuteInHive(executables, null, cancellationToken);
        }

        public static IEnumerable<T> ExecuteInHive<T>(IEnumerable<T> executables, Options options, CancellationToken cancellationToken) where T : IExecutable
        {
            if (options == null)
            {
                options = new Options
                {
                    ProjectId = Guid.Empty,
                    ResourceIds = new Guid[0],
                    JobName = string.Empty,
                    Distribute = true
                };
            }

            if (!executables.Any())
                throw new ArgumentException("At least one executable must be specified.");

            Project[] projects;
            Resource[] resources;

            lock (locker)
            {
                HiveClient.Instance.Refresh();
                projects = HiveClient.Instance.Projects.ToArray();
                resources = HiveClient.Instance.Resources.ToArray();
            }

            var projectId = GuardProjectId(options.ProjectId, projects);
            var resourceIds = GuardResourceIds(options.ResourceIds, projectId, resources);
            var jobName = GuardJobName(executables, options.JobName);
            var distribute = options.Distribute;

            var refreshableJob = PackJob(executables, projectId, resourceIds, jobName, distribute);
            HiveClient.Store(refreshableJob, cancellationToken);

            var taskIds = refreshableJob.HiveTasks.Select(x => x.Task.Id).ToArray();

            Exception exception = null;
            var signal = SetupWaitHandle(refreshableJob, e => exception = e);
            refreshableJob.StartResultPolling();

            try
            {
                signal.Wait(cancellationToken);
                if (exception != null) throw exception;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                HiveClient.LoadJob(refreshableJob);
                executables = UnpackJob<T>(refreshableJob, taskIds);
                HiveClient.Delete(refreshableJob);
            }

            return executables;
        }

        private static Guid GuardProjectId(Guid projectId, IEnumerable<Project> projects)
        {
            Project selectedProject;

            if (projectId == Guid.Empty)
            {
                selectedProject = projects.FirstOrDefault();
                if (selectedProject == null) throw new ArgumentException("A default project is not available.");
            }
            else
            {
                selectedProject = projects.SingleOrDefault(x => x.Id == projectId);
                if (selectedProject == null) throw new ArgumentException("The specified project is not available.");
            }

            return selectedProject.Id;
        }

        private static IEnumerable<Guid> GuardResourceIds(IEnumerable<Guid> resourceIds, Guid projectId, IEnumerable<Resource> resources)
        {
            Resource[] availableResources;
            lock (locker) availableResources = HiveClient.Instance.GetAvailableResourcesForProject(projectId).ToArray();

            var availableResourceIds = availableResources.Select(x => x.Id);
            var unavailableResources = resourceIds.Except(availableResourceIds);
            if (unavailableResources.Any()) throw new ArgumentException("Some of the specified resources are not available for the specified project.");

            if (!resourceIds.Any())
            {
                resourceIds = availableResourceIds;
            }

            return resourceIds;
        }

        private static string GuardJobName<T>(IEnumerable<T> executables, string jobName) where T : IExecutable
        {
            if (string.IsNullOrEmpty(jobName))
            {
                jobName = string.Join(" + ", executables);
            }

            return jobName;
        }

        private static RefreshableJob PackJob<T>(IEnumerable<T> executables, Guid projectId, IEnumerable<Guid> resourceIds, string jobName, bool distribute) where T : IExecutable
        {
            var refreshableJob = new RefreshableJob()
            {
                Job = {
          Name = jobName,
          ProjectId = projectId,
          ResourceIds = resourceIds.ToList()
        }
            };

            foreach (var executable in executables)
            {
                var itemTask = ItemTask.GetItemTaskForItem(executable);
                itemTask.ComputeInParallel = distribute && (executable is Experiment || executable is BatchRun);

                var hiveTask = itemTask.CreateHiveTask();
                refreshableJob.HiveTasks.Add(hiveTask);
            }

            return refreshableJob;
        }

        private static IEnumerable<T> UnpackJob<T>(RefreshableJob refreshableJob, IList<Guid> taskIds) where T : IExecutable
        {
            var hiveTasks = refreshableJob.HiveTasks.OrderBy(x => taskIds.IndexOf(x.Task.Id));

            foreach (var hiveTask in hiveTasks)
            {
                var executable = (T)hiveTask.ItemTask.Item;
                yield return executable;
            }
        }

        private static ManualResetEventSlim SetupWaitHandle(RefreshableJob refreshableJob, Action<Exception> exceptionCallback)
        {
            var signal = new ManualResetEventSlim(false);

            refreshableJob.StateLogListChanged += (sender, args) =>
            {
                if (refreshableJob.IsFinished())
                    signal.Set();
            };
            refreshableJob.ExceptionOccured += (sender, args) =>
            {
                exceptionCallback(args.Value);
                signal.Set();
            };

            return signal;
        }
    }
}
