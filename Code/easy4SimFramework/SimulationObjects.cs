using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HEAL.Attic;

namespace Easy4SimFramework
{
    /// <summary>
    /// Class that holds Lists of all simulation and visualization objects.
    /// </summary>
    [StorableType("AF037292-2887-4A16-BCA4-45AA93F1A247")]
    public class SimulationObjects : ICloneable<SimulationObjects>
    {
        #region events
        [StorableType("CF1A1BB7-66CD-45A0-9A5B-D23998139AF7")]
        public class VisualizationEventArgs : EventArgs
        {
            [Storable]
            public string Message { get; set; }
            [Storable]
            public IBaseVisualizationModel VisualizationModel { get; set; }
            [StorableConstructor]
            public VisualizationEventArgs(StorableConstructorFlag flag) { }
            public VisualizationEventArgs(string message, IBaseVisualizationModel visualizationModel)
            {
                Message = message;
                VisualizationModel = visualizationModel;
            }
        }
        [StorableType("35593CD0-FE98-40C1-9C04-8A0075E91F4B")]
        public class WindowParameterEventArgs : EventArgs
        {
            [Storable]
            public string WindowType { get; set; }
            [Storable]
            public string WindowParameter { get; set; }
            [Storable]
            public string Value { get; set; }

            public WindowParameterEventArgs(StorableConstructorFlag flag) { }
            public WindowParameterEventArgs(string windowType, string windowParameter, string value)
            {
                WindowType = windowType;
                WindowParameter = windowParameter;
                Value = value;
            }

        }


        public delegate void AddedVisualComponentDuringRuntimeDelegate(object sender, VisualizationEventArgs e);
        public delegate void ChangeWindowParameterDuringRuntimeDelegate(object sender, WindowParameterEventArgs e);

        public event ChangeWindowParameterDuringRuntimeDelegate WindowParameterChangedDuringRuntime;
        public event AddedVisualComponentDuringRuntimeDelegate VisualComponentAddedDuringRuntime;

        public void AddVisualComponentDuringRuntimeEvent(string componentName,
            IBaseVisualizationModel baseVisualizationModel)
        {
            // Make sure someone is listening to event
            VisualComponentAddedDuringRuntime?.Invoke(null, new VisualizationEventArgs(componentName, baseVisualizationModel));
        }

        public void ChangeVisualizationWindowDuringRuntime(string windowType, string windowParameter, string value)
        {
            // Make sure someone is listening to event
            WindowParameterChangedDuringRuntime?.Invoke(null, new WindowParameterEventArgs(windowType, windowParameter, value));
        }


        #endregion

        #region Ctor
        [StorableConstructor]
        public SimulationObjects(StorableConstructorFlag flag) { }
        public SimulationObjects() => SimulationObjectId = Guid.NewGuid().ToString();
        #endregion
        /// <summary>
        /// Dictionary of objects with the simulation number
        /// </summary>
        [Storable]
        public Dictionary<long, CSimBase> SimulationList { get; set; } = new Dictionary<long, CSimBase>();

        /// <summary>
        /// Dictionary of non simulation objects that are created in the simulation.
        /// Each object that should be saved in the environment should at least be cloneable.
        /// </summary>
        [Storable]
        public Dictionary<Guid, INonSimulationObject> NonSimulationObjectList { get; set; } = new Dictionary<Guid, INonSimulationObject>();

        /// <summary>
        /// List of visualizations objects of the type IBaseVisualizationModel.
        /// Virtual method UpdateVisualization is used to update the visual object.
        /// </summary>
        [Storable]
        public Dictionary<Guid, IBaseVisualizationModel> VisualizationsList { get; set; } = new Dictionary<Guid, IBaseVisualizationModel>();
        /// <summary>
        /// List of visualizations objects of the type IBaseVisualizationModel.
        /// Virtual method UpdateVisualization is used to update the visual object.
        /// </summary>
        [Storable]
        public List<IBaseWindowVm> WindowList { get; set; } = new List<IBaseWindowVm>();

        /// <summary>
        /// Dictionary of derived objects with the simulation number
        /// </summary>
        [Storable]
        public Dictionary<long, ISimBase> DerivedSimulationList { get; set; } = new Dictionary<long, ISimBase>();
        /// <summary>
        /// List that contains all information about all Parameters that can be optimized.
        /// </summary>
        [Storable]
        public List<ParameterOptimization> ParameterOptimizations { get; set; } = new List<ParameterOptimization>();
        /// <summary>
        /// Queue of all events for this simulation objects
        /// </summary>
        [Storable]
        public EventQueue EventQueue { get; set; } = new EventQueue();

        /// <summary>
        /// List of all Links that should be connected
        /// </summary>
        [Storable]
        public List<Link> LinkList { get; set; } = new List<Link>();


        /// <summary>
        /// List that contains all Information about Visualization classes
        /// </summary>
        [Storable]
        public List<VisualizationTypes> VisualizationTypeList { get; set; } = new List<VisualizationTypes>();

        /// <summary>
        /// SimulationObjectId
        /// </summary>
        [Storable]
        public string SimulationObjectId { get; set; }

        public void AddSimulationsObject(CSimBase obj)
        {
            if (SimulationList.ContainsKey(obj.Index)) return;
            SimulationList.Add(obj.Index, obj);
        }

        public void AddNonSimulationObject(INonSimulationObject obj)
        {
            NonSimulationObjectList.Add(obj.CurrentGuid, obj);
        }

        public void RemoveNonSimulationObject(INonSimulationObject obj)
        {
            NonSimulationObjectList.Remove(obj.CurrentGuid);
        }



        public void AddVisualizationObject(IBaseVisualizationModel baseVisualizationModel)
        {
            if (VisualizationsList.ContainsKey(baseVisualizationModel.CurrentGuid)) return;
            VisualizationsList.Add(baseVisualizationModel.CurrentGuid, baseVisualizationModel);
        }
        public void AddVisualizationObjectDuringRuntime(IBaseVisualizationModel baseVisualizationModel)
        {
            if (VisualizationsList.ContainsKey(baseVisualizationModel.CurrentGuid)) return;
            VisualizationsList.Add(baseVisualizationModel.CurrentGuid, baseVisualizationModel);
            AddVisualComponentDuringRuntimeEvent(baseVisualizationModel.UniqueName, baseVisualizationModel);
        }

        public void RemoveVisualizationObject(IBaseVisualizationModel baseVisualizationModel)
        {

            if (!VisualizationsList.ContainsKey(baseVisualizationModel.CurrentGuid)) return;
            VisualizationsList.Remove(baseVisualizationModel.CurrentGuid);
        }

        public bool ContainsVisualizationObject(IBaseVisualizationModel baseVisualizationModel)
        {
            return VisualizationsList.ContainsKey(baseVisualizationModel.CurrentGuid);
        }

        public void AddParameterOptimization(string nodeName, string nodeParameter, string nodeType, int amount)
        {
            ParameterOptimizations.Add(new ParameterOptimization<bool>(nodeName, nodeParameter, nodeType, amount, false, true));
        }

        public void AddParameterOptimization(string nodeName, string nodeParameter, string nodeType, int amount, int minimum, int maximum)
        {
            ParameterOptimizations.Add(new ParameterOptimization<int>(nodeName, nodeParameter, nodeType, amount, minimum, maximum));
        }
        public void AddParameterOptimization(string nodeName, string nodeParameter, string nodeType, int amount, double minimum, double maximum)
        {
            ParameterOptimizations.Add(new ParameterOptimization<double>(nodeName, nodeParameter, nodeType, amount, minimum, maximum));
        }


        public ISimBase GetSimulationObjectByName(string name)
        {
            return SimulationList.Values.FirstOrDefault(x => x.Name == name) ??
                                SimulationList.Values.FirstOrDefault(x => x.Name == name.Replace("/", "_")); //component support
        }


        public long GetNextSimulationListIndex() => SimulationList.Keys.Max() + 1;
        

        public void Reset()
        {
            SimulationList.Clear();
            NonSimulationObjectList.Clear();
            VisualizationsList.Clear();
            WindowList.Clear();
            DerivedSimulationList.Clear();
            LinkList.Clear();
            ParameterOptimizations.Clear();
            EventQueue.Reset();
            VisualizationTypeList.Clear();
        }

        public SimulationObjects Clone()
        {
            SimulationObjects simulationObjects = new SimulationObjects
            {
                EventQueue = EventQueue.Clone(),
                SimulationObjectId = SimulationObjectId
            };

            foreach (long key in SimulationList.Keys)
                simulationObjects.SimulationList.Add(key, (CSimBase)SimulationList[key].Clone());
            foreach (Guid key in NonSimulationObjectList.Keys)
                simulationObjects.NonSimulationObjectList.Add(key, (INonSimulationObject)NonSimulationObjectList[key].Clone());

            foreach (long key in DerivedSimulationList.Keys)
                simulationObjects.DerivedSimulationList.Add(key, (ISimBase)DerivedSimulationList[key].Clone());

            foreach (ParameterOptimization optimization in ParameterOptimizations)
            {
                if (optimization is ParameterOptimization<int> intOptimization)
                    simulationObjects.ParameterOptimizations.Add(intOptimization.Clone());
                else if (optimization is ParameterOptimization<bool> boolOptimization)
                    simulationObjects.ParameterOptimizations.Add(boolOptimization.Clone());
                else if (optimization is ParameterOptimization<double> doubleOptimization)
                    simulationObjects.ParameterOptimizations.Add(doubleOptimization.Clone());
                else
                    simulationObjects.ParameterOptimizations.Add(optimization.Clone());
            }

            foreach (KeyValuePair<Guid, IBaseVisualizationModel> model in VisualizationsList)
            {
                simulationObjects.VisualizationsList.Add(model.Key, (IBaseVisualizationModel)model.Value.Clone());
            }

            foreach (VisualizationTypes types in VisualizationTypeList)
            {
                simulationObjects.VisualizationTypeList.Add(types.Clone());
            }

            foreach (Link link in LinkList)
                simulationObjects.LinkList.Add(link.Clone());

            simulationObjects.ConnectAllLinks();
            return simulationObjects;
        }

        public void UpdateSolverSettingsOfAllItems(SolverSettings settings)
        {
            foreach (KeyValuePair<long, CSimBase> pair in SimulationList)
                UpdateSettingsOfISimBase(pair.Value, settings);
            foreach (KeyValuePair<long, ISimBase> pair in DerivedSimulationList)
                UpdateSettingsOfISimBase(pair.Value, settings);
            foreach (KeyValuePair<Guid, IBaseVisualizationModel> pair in VisualizationsList)
            {
                if (pair.Value is VisualizationInitializedWithSolver item)
                    item.SolverSettings = settings;
            }

        }

        private void UpdateSettingsOfISimBase(ISimBase simBase, SolverSettings settings)
        {
            simBase.Settings = settings;
        }

        #region link methods
        public void ConnectAllLinks()
        {
            foreach (Link link in LinkList)
                ConnectLinkItem(link);
        }
        public bool ConnectLinkItem(Link link)
        {
            string variableName1 = link.OutputConnectorName;
            string variableName2 = link.InputConnectorName;
            string objectName1 = link.OutputComponentName;
            string objectName2 = link.InputComponentName;
            int variableIndex1 = -1;
            int variableIndex2 = -2;

            if (variableName1.Contains("["))
            {
                int i1 = variableName1.IndexOf('[');
                int i2 = variableName1.IndexOf(']');
                string s = variableName1.Substring(i1 + 1, i2 - i1 - 1);
                variableIndex1 = Convert.ToInt32(s);
                variableName1 = variableName1.Substring(0, i1);
            }
            if (variableName2.Contains("["))
            {
                int i1 = variableName2.IndexOf('[');
                int i2 = variableName2.IndexOf(']');
                string s = variableName2.Substring(i1 + 1, i2 - i1 - 1);
                variableIndex2 = Convert.ToInt32(s);
                variableName2 = variableName2.Substring(0, i1);
            }

            ISimBase baseObject1 = GetSimulationObjectByName(objectName1);
            ISimBase baseObject2 = GetSimulationObjectByName(objectName2);
            if (baseObject1 != null && baseObject2 != null)
            {
                object object1 = null;
                object object2 = null;
                FieldInfo fieldInfo1 = baseObject1.GetType().GetField(variableName1);
                if (fieldInfo1 == null)
                {
                    string[] temp = variableName1.Split('_');
                    fieldInfo1 = baseObject1.GetType().GetField(temp[0]);


                    if (fieldInfo1 != null)
                        object1 = ((object[])fieldInfo1.GetValue(baseObject1))[Convert.ToInt32(temp[1])];
                }

                if(fieldInfo1 != null && object1 == null)
                    object1 = variableIndex1 < 0 ? fieldInfo1.GetValue(baseObject1) : ((object[])fieldInfo1.GetValue(baseObject1))[variableIndex1];
                if (object1 == null)
                {
                    PropertyInfo pInfo1= baseObject1.GetType().GetProperty(variableName1);
                    if (pInfo1 != null)
                        object1 = variableIndex1 < 0 ? pInfo1.GetValue(baseObject1) : ((object[])pInfo1.GetValue(baseObject1))[variableIndex1];
                }
                


                FieldInfo fieldInfo2 = baseObject2.GetType().GetField(variableName2);
                if (fieldInfo2 == null)
                {
                    string[] temp = variableName2.Split('_');
                    fieldInfo2 = baseObject2.GetType().GetField(temp[0]);


                    if (fieldInfo2 != null)
                        object2 =  ((object[])fieldInfo2.GetValue(baseObject2))[Convert.ToInt32(temp[1])];
                }
                if (fieldInfo2 != null && object2 == null)
                    object2 = variableIndex2 < 0 ? fieldInfo2.GetValue(baseObject2) : ((object[])fieldInfo2.GetValue(baseObject2))[variableIndex2];
                if (object2 == null)
                {
                    PropertyInfo pInfo2 = baseObject2.GetType().GetProperty(variableName2);
                    if (pInfo2 != null)
                        object2 = variableIndex2 < 0 ? pInfo2.GetValue(baseObject2) : ((object[])pInfo2.GetValue(baseObject2))[variableIndex2];
                }

                if (object1 != null && object2 != null)
                {
                    MethodInfo connectMethod = object2?.GetType().GetMethod("Connect");
                    if (connectMethod != null)
                    {
                        connectMethod.Invoke(object2, new[] { object1 });
                        return true;
                    }
                }
            }

            return false;
        }

        public bool AddLink(Link link)
        {
            if (LinkList.Any(x => x.Equals(link))) return false; //check if link is added already

            LinkList.Add(link);
            return ConnectLinkItem(link);
        }

        #endregion

        public void UpdateAllConnections(SolverSettings settings)
        {
            foreach (KeyValuePair<long, CSimBase> pair in SimulationList)
                UpdateConnectionPoint(pair.Value, settings);
            foreach (KeyValuePair<long, ISimBase> pair in DerivedSimulationList)
                UpdateConnectionPoint(pair.Value, settings);
        }

        private void UpdateConnectionPoint(ISimBase simBase, SolverSettings settings)
        {
            FieldInfo[] fInfos = simBase.GetType().GetFields();
            foreach (FieldInfo fInfo in fInfos)
            {


                Type t = fInfo.FieldType;
                if (t.IsSubclassOf(typeof(InputOutputBase)))
                {
                    var val = fInfo.GetValue(simBase);
                    if (val is InputOutputBase inputOutputBase)
                    {
                        inputOutputBase.Parent = settings.SimulationObjects.GetSimulationObjectByName(inputOutputBase.Parent.Name);
                    }
                }


                t.IsSubclassOf(typeof(OutputBase<object>));
            }

        }

        public void UpdateElement(CSimBase element)
        {
            long key = 0;
            foreach (KeyValuePair<long, CSimBase> pair in SimulationList)
            {
                if (pair.Value.Index == element.Index)
                {
                    key = pair.Key;
                    break;
                }
            }
            SimulationList[key] = element;
        }
    }
    [StorableType("324C0EB4-6456-4816-890E-984C4E9D1928")]
    public class Link : ICloneable<Link>
    {
        [Storable]
        public string OutputComponentName { get; set; }
        [Storable]
        public string OutputConnectorName { get; set; }
        [Storable]
        public string InputComponentName { get; set; }
        [Storable]
        public string InputConnectorName { get; set; }

        [StorableConstructor]
        public Link(StorableConstructorFlag flag){}
        public Link(){}
        public Link(string outputName, string outputConnector, string inputName, string inputConnector)
        {
            OutputComponentName = outputName;
            OutputConnectorName = outputConnector;
            InputComponentName = inputName;
            InputConnectorName = inputConnector;
        }

        public bool Equals(Link link)
        {
            return OutputComponentName == link.OutputComponentName &&
                   OutputConnectorName == link.OutputConnectorName &&
                   InputComponentName == link.InputComponentName &&
                   InputConnectorName == link.InputConnectorName;
        }

        public Link Clone()
        {
            return new Link(OutputComponentName, OutputConnectorName, InputComponentName, InputConnectorName);
        }
    }

    /// <summary>
    /// Base class for every Parameter optimization.
    /// This base class is needed to allow the creation of Lists of specific base types. 
    /// </summary>
    [StorableType("6A8E4559-FB14-4FE6-A32E-0C1AAB6D13E6")]
    public class ParameterOptimization : ICloneable<ParameterOptimization>
    {
        #region ctor
        [StorableConstructor]
        public ParameterOptimization(StorableConstructorFlag flag){}
        protected ParameterOptimization() { }
        protected ParameterOptimization(string nodeName, string nodeParameter, string nodeType, int amount)
        {
            NodeName = nodeName;
            NodeParameter = nodeParameter;
            NodeType = nodeType;
            Amount = amount;
        }
        [Storable]
        #endregion
        public string NodeName { get; set; }
        [Storable]
        public string NodeParameter { get; set; }
        [Storable]
        public string NodeType { get; set; }
        [Storable]
        public int Amount { get; set; }
        public ParameterOptimization Clone() => new ParameterOptimization(NodeName, NodeParameter, NodeType, Amount);

    }

    /// <summary>
    /// Generic typed Parameter optimization class.
    /// In this class a node parameter can be defined with bounds and step width.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("E0485095-C279-456B-9087-149EB36D3FE8")]
    public class ParameterOptimization<T> : ParameterOptimization, ICloneable<ParameterOptimization<T>>
        where T : IComparable<T>
    {
        [Storable]
        public T Minimum { get; set; }

        [Storable]
        public T Maximum { get; set; }

        #region Ctor
        public ParameterOptimization() { }
        [StorableConstructor]
        public ParameterOptimization(StorableConstructorFlag flag) { }
        public ParameterOptimization(string nodeName, string nodeParameter, string nodeType, int amount, T minimum, T maximum) : base(nodeName,
            nodeParameter, nodeType, amount)
        {
            Minimum = minimum;
            Maximum = maximum;
        }
        #endregion

        public new ParameterOptimization<T> Clone()
        {
            return new ParameterOptimization<T>(NodeName, NodeParameter, NodeType, Amount, Minimum, Maximum)
            {
                Maximum = Maximum,
                Minimum = Minimum
            };
        }

    }

    [StorableType("894093E0-6C47-49EA-AA6B-504E740C5624")]
    public class VisualizationTypes : ICloneable<VisualizationTypes>
    {
        #region Ctor
        [StorableConstructor]
        public VisualizationTypes(StorableConstructorFlag flag) { }

        public VisualizationTypes() {}
        public VisualizationTypes(string visualizationWindow, Type simulationType, Type visualizationModel, Type visualizationType)
        {
            VisualizationWindow = visualizationWindow;
            SimulationType = simulationType;
            VisualizationModel = visualizationModel;
            VisualizationType = visualizationType;
            GuidString = Guid.NewGuid().ToString();
        }
        #endregion
        [Storable]
        public string GuidString { get; set; }
        [Storable]
        public string VisualizationWindow { get; set; }

        [Storable]
        public Type SimulationType { get; set; }

        [Storable]
        public Type VisualizationModel { get; set; }

        [Storable]
        public Type VisualizationType { get; set; }
        public override bool Equals(object obj)
        {
            if (!(obj is VisualizationTypes visualizationTypes)) return base.Equals(obj);
            return VisualizationWindow == visualizationTypes.VisualizationWindow &&
                   SimulationType == visualizationTypes.SimulationType &&
                   VisualizationModel == visualizationTypes.VisualizationModel &&
                   VisualizationType == visualizationTypes.VisualizationType;
        }

        public override int GetHashCode()
        {
            return new Guid(GuidString).GetHashCode();
        }


        public VisualizationTypes Clone()
        {
            return new VisualizationTypes(VisualizationWindow, SimulationType, VisualizationModel, VisualizationType);
        }
    }
}
