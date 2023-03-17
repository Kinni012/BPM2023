using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HeuristicLab.Clients.Hive;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;
using HeuristicLab.SimGenOpt;

namespace HeuristicLab.HiveProblem
{
  [View("ProjectResourcesValueView")]
  [Content(typeof(ProjectResourcesValue), true)]
  public partial class ProjectResourcesValueView : ItemView {
    private readonly object locker = new object();
    private bool updatingProjects;
    private bool progressRegistered;

    public new ProjectResourcesValue Content {
      get { return (ProjectResourcesValue)base.Content; }
      set { base.Content = value; }
    }

    public ProjectResourcesValueView() {
      InitializeComponent();
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();

      hiveProjectSelector.Enabled = Content != null && !ReadOnly && !Locked;
    }

    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      HiveClient.Instance.Refreshing += HiveClient_Instance_Refreshing;
      HiveClient.Instance.Refreshed += HiveClient_Instance_Refreshed;
    }

    protected override void DeregisterContentEvents() {
      base.DeregisterContentEvents();
      HiveClient.Instance.Refreshed -= HiveClient_Instance_Refreshed;
      HiveClient.Instance.Refreshing -= HiveClient_Instance_Refreshing;
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();

      if (Content == null) {
        hiveProjectSelector.SelectedProjectId = Guid.Empty;
        hiveProjectSelector.SelectedResourceIds = new List<Guid>();
      } else {
        hiveProjectSelector.SelectedProjectId = Content.ProjectId;
        hiveProjectSelector.SelectedResourceIds = Content.ResourceIds.ToList();
      }
    }

    private void HiveClient_Instance_Refreshing(object sender, EventArgs e) {
      if (InvokeRequired) Invoke((Action<object, EventArgs>)HiveClient_Instance_Refreshing, sender, e);
      else {
        if (!progressRegistered) {
          Progress.Show(this, "Refreshing ...");
          progressRegistered = true;
        }
        refreshButton.Enabled = false;
      }
    }

    private void HiveClient_Instance_Refreshed(object sender, EventArgs e) {
      if (InvokeRequired) Invoke((Action<object, EventArgs>)HiveClient_Instance_Refreshed, sender, e);
      else {
        if (progressRegistered) {
          Progress.Hide(this);
          progressRegistered = false;
        }
        refreshButton.Enabled = true;
      }
    }

    private void hiveProjectSelector_SelectedProjectChanged(object sender, EventArgs e) {
      Content.ProjectId = hiveProjectSelector.SelectedProject.Id;
    }

    private void hiveProjectSelector_AssignedResourcesChanged(object sender, EventArgs e) {
      Content.ResourceIds = hiveProjectSelector.AssignedResources.Select(x => x.Id).ToList();
    }

    private async void refreshButton_Click(object sender, EventArgs e) {
      lock (locker) {
        if (updatingProjects) return;
        updatingProjects = true;
      }

      await SecurityExceptionUtil.TryAsyncAndReportSecurityExceptions(
        action: UpdateProjects,
        finallyCallback: () => updatingProjects = false);
    }

    private void UpdateProjects() {
      HiveClient.Instance.RefreshProjectsAndResources();
      hiveProjectSelector.ProjectId = Content != null ? Content.ProjectId : Guid.Empty;
      hiveProjectSelector.SelectedProjectId = Content != null ? Content.ProjectId : Guid.Empty;
      hiveProjectSelector.SelectedResourceIds = Content != null ? Content.ResourceIds.ToList() : new List<Guid>();
      hiveProjectSelector.Content = HiveClient.Instance.Projects;
    }
  }
}
