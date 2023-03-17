using System;
using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;

namespace HeuristicLab.SimGenOpt
{
    [Item("ProjectResourcesValue", "Represents a project and selected project resources.")]
    [StorableType("897D2065-5C7D-448B-B9DB-4D612BF1844E")]
    public class ProjectResourcesValue : Item
    {
        [Storable]
        private Guid projectId;
        public Guid ProjectId
        {
            get { return projectId; }
            set
            {
                if (projectId == value) return;
                projectId = value;
                OnToStringChanged();
            }
        }

        [Storable]
        private List<Guid> resourceIds;
        public List<Guid> ResourceIds
        {
            get { return resourceIds; }
            set
            {
                if (resourceIds == value) return;
                resourceIds = value;
                OnToStringChanged();
            }
        }

        [StorableConstructor]
        protected ProjectResourcesValue(StorableConstructorFlag _) : base(_) { }
        protected ProjectResourcesValue(ProjectResourcesValue original, Cloner cloner) : base(original, cloner)
        {
            ProjectId = original.ProjectId;
            ResourceIds = original.ResourceIds.Select(x => x).ToList();
        }
        public ProjectResourcesValue()
        {
            ProjectId = Guid.Empty;
            ResourceIds = new List<Guid>();
        }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new ProjectResourcesValue(this, cloner);
        }

        public override string ToString()
        {
            return string.Format("ProjectId: {0}, NrOfResources: {1}", ProjectId, ResourceIds.Count);
        }
    }
}
