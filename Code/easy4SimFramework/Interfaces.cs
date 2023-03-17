using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using HEAL.Attic;
using log4net;

//Multiple Interfaces that are used in the Framework
namespace Easy4SimFramework
{
    /// <summary>
    /// Used define structs and classes of different base types.
    /// Guarantees that the access to the values of structs and classes in the Framework is similar.
    /// Used for nearly all structs in the Framework.
    /// </summary>
    /// <typeparam name="T"> Type of the Value </typeparam>
    [StorableType("E77C3F63-722A-4E26-9AF5-FEBBB6762255")]
    public interface IValueable<T>
    {
        [Storable]
        T Value { get; set; }
    }
    /// <summary>
    /// Base Cloneable Interface used in the Framework
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("ADE71730-02BC-4F49-80B6-E5F2FE14022A")]
    public interface ICloneable<T> 
    {
        T Clone();
    }

    /// <summary>
    /// Base Interface for non simulation objects that should be stored in the environment
    /// </summary>
    [StorableType("1CF4853A-4E46-4F27-8055-19F81677EB35")]
    public interface INonSimulationObject : ICloneable, IGuid
    {

    }
    /// <summary>
    /// Used to define that this class is created with a Guid.
    /// Does not allow to set a Guid, because this Guid should be set in the constructor of the class. <para/>
    /// Example: <para/>
    /// public Guid CurrentGuid { get; internal set; }
    /// </summary>
    [StorableType("3F6985DA-EE2D-43C2-B72D-D0B9AC05D180")]
    public interface IGuid
    {
        [Storable]
        Guid CurrentGuid { get; set; }
    }

    /// <summary>
    /// The Framework offers the ability to define Advanced Information about Elements
    /// </summary>
    [StorableType("A718818D-FDE1-494A-B71A-D0D088EA52EE")]
    public interface IAdvancedItemInformation
    {
        /// <summary>
        /// Since we want to define the Tooltip of Parameters of Simulation objects in the Library.
        /// An Example would be a class Wizard that has a Parameter DoMagic of the Type PARAM_STRING and its really not intuitive what it does.
        /// This means we can now implement the AdvancedItemInformation Interface. 
        /// In the ToolTipInformation we can now add("DoMagic", "If DoMagic is x output is 1000, otherwise its pi").
        /// The Editor can now check if Wizard implements AdvancedItemInformation and set a specific ToolTip for each Parameter
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> ToolTipInformation();
    }

    /// <summary>
    /// Base Interface for all objects that should be simulated in the Framework.
    /// Defines standard methods and properties that are necessary for the simulation.
    /// </summary>
    [StorableType("C0A14DE2-F889-4198-B4C7-2D5C30DB350F")]
    public interface ISimBase : ICloneable
    {
        /// <summary>
        /// Logger of the specific Node
        /// </summary>
        [Storable]
        ILog Logger { get; set; }
        /// <summary>
        /// Index of the Nodes.
        /// Nodes with a lower Index are calculated first in each simulation step.
        /// </summary>
        [Storable]
        long Index { get; set; }
        /// <summary>
        /// Name of the Node.
        /// </summary>
        [Storable]
        string Name { get; set; }
        /// <summary>
        /// Name that is used for Logging of the Node.
        /// </summary>
        [Storable]
        string LogName { get; set; }
        /// <summary>
        /// LogLevel that is used for Logging of the Node
        /// </summary>
        [Storable]
        ParameterInt LogLevel { get; set; }

        /// <summary>
        /// Settings that the solver needs for calculation.
        /// </summary>
        [Storable]
        SolverSettings Settings { get; set; }

        #region Methods
        /// <summary>
        /// Init the Node with Parameter Values. Called once before the first simulation starts.
        /// </summary>
        void Initialize();
        /// <summary>
        /// Called to reset node to the state at simulation time zero.
        /// If the simulation is called multiple times, all parameters have to be reset to their original value.
        /// </summary>
        void Start();
        /// <summary>
        /// Called when the simulation finishes.
        /// Used for additional features, like logging of statistics.
        /// </summary>
        void End();
        /// <summary>
        /// Used to calculate Values that change in every simulation step.
        /// An example would be a Node that has a sinus output that is based on the current simulation time.
        /// </summary>
        void DiscreteCalculation();
        /// <summary>
        /// Event based Simulation.
        /// If an Event for this Node is added at a specific simulation time, this method is called.
        /// Used for Nodes that do not have to calculate the output in every simulation step.
        /// An Example would be a Node that  between zero and one every after 100 simulation steps have passed. 
        /// </summary>
        void DynamicCalculation();
        /// <summary>
        /// Used for the calculation of integrals.
        /// </summary>
        void DerivedCalculation();
        #endregion
    }


    #region Visualization interfaces
    /// <summary>
    /// Base Interface for all objects that should be updated in the visualization.
    /// </summary>
    [StorableType("348B4827-1B7B-4242-AA4A-40406E6DE837")]
    public interface IBaseVisualizationModel : ICloneable, IPositionable
    {
        /// <summary>
        /// Unique identifier in the visualization.
        /// </summary>
        [Storable]
        string UniqueName { get; set; }

        [Storable]
        IBaseVisualComponent VisualizationObject { get; set; }
        CSimBase InitializeObject();
        /// <summary>
        /// Update the current visualization state.
        /// </summary>
        void UpdateVisualization();
    }


    /// <summary>
    /// Base interface for all objects that should be added to any window.
    /// </summary>
    [StorableType("76649DD2-CC4C-4C5F-89A4-C7FAD951CB49")]
    public interface IBaseVisualComponent : INotifyPropertyChanged, ICloneable, IGuid
    {
        [Storable]
        string Name { get; set; }
        void InitializeVisualization();
        void UpdateVisualization();
        
    }

    /// <summary>
    /// Base interface for all window view models.
    /// Each window should have a name and the ability to display a visual component. 
    /// </summary>
    [StorableType("5F63FDE5-CA65-47CE-97FE-B26E40D54B60")]
    public interface IBaseWindowVm
    {
        [Storable]
        IBaseWindow Window { get; set; }
        [Storable]
        string WindowTitle { get; set; }
        void InitializeVisualization();
        bool AddVisualComponent(IBaseVisualComponent vis);
    }

    /// <summary>
    /// Base interface for all Windows.
    /// </summary>
    [StorableType("11737490-7293-419A-9535-14B7E33CA198")]
    public interface IBaseWindow
    {
        [Storable]
        bool CloseAppOnWindowClose { get; set; }

        Window Window { get; }

        void Initialize();
    }

    [StorableType("32475702-B844-4E2D-90BA-7C7FA6B01005")]
    public interface IPositionable : IGuid
    {
        [Storable]
        CSimBase CurrentObject { get; set; }
        [Storable]
        double PositionX { get; set; }
        [Storable]
        double PositionY { get; set; }
        [Storable]
        double Width { get; set; }
        [Storable]
        double Height { get; set; }
    }

    [StorableType("D9CD1510-8744-4630-B7FF-10AE9AF4CFC5")]
    public interface IImportableUserControlViewModel : IGuid
    {
        void SimulationLoaded();
        void SimulationLoadedWithProperties(Dictionary<string, string> dictionary);
        void SimulationStarted();
        void SolverChanged(Solver solver);
        void SimulationStopped();
        void SimulationPaused();
        void SimulationResumed();
        void SimulationFinished();
    }


    [StorableType("0575B83E-B217-422F-A352-B5DAF54D6603")]
    public interface INamedUserControl : IGuid
    {
        [Storable]
        string PageName { get; set; }
        void SetCurrentSolver(Solver solver);
    }

    #endregion
}
