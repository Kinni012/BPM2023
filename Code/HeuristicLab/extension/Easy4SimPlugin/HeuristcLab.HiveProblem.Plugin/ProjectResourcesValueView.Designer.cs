namespace HeuristicLab.HiveProblem
{
    partial class ProjectResourcesValueView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.hiveProjectSelector = new HeuristicLab.Clients.Hive.JobManager.Views.HiveProjectSelector();
            this.refreshButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hiveProjectSelector
            // 
            this.hiveProjectSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.hiveProjectSelector.Caption = "Hive Project Selector View";
            this.hiveProjectSelector.Content = null;
            this.hiveProjectSelector.JobId = new System.Guid("00000000-0000-0000-0000-000000000000");
            this.hiveProjectSelector.Location = new System.Drawing.Point(0, 32);
            this.hiveProjectSelector.Name = "hiveProjectSelector";
            this.hiveProjectSelector.ProjectId = null;
            this.hiveProjectSelector.ReadOnly = false;
            this.hiveProjectSelector.SelectedProject = null;
            this.hiveProjectSelector.SelectedProjectId = null;
            this.hiveProjectSelector.SelectedResourceIds = null;
            this.hiveProjectSelector.Size = new System.Drawing.Size(432, 512);
            this.hiveProjectSelector.TabIndex = 0;
            this.hiveProjectSelector.SelectedProjectChanged += new System.EventHandler(this.hiveProjectSelector_SelectedProjectChanged);
            this.hiveProjectSelector.AssignedResourcesChanged += new System.EventHandler(this.hiveProjectSelector_AssignedResourcesChanged);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshButton.Image = HeuristicLab.Common.Resources.VSImageLibrary.Refresh;
            this.refreshButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.Location = new System.Drawing.Point(3, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(426, 23);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.refreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // ProjectResourcesValueView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.hiveProjectSelector);
            this.Name = "ProjectResourcesValueView";
            this.Size = new System.Drawing.Size(432, 544);
            this.ResumeLayout(false);

        }

        #endregion

        private Clients.Hive.JobManager.Views.HiveProjectSelector hiveProjectSelector;
        private System.Windows.Forms.Button refreshButton;
    }
}
