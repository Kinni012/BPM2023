using System;
using HEAL.Attic;

// Attributes that are used in the Easy4SimFramework
namespace Easy4SimFramework
{
    [StorableType("6086F398-D332-4837-8C78-DA4B3013FFA3")]
    public class Optimization : Attribute
    {
        [StorableConstructor]
        public Optimization(StorableConstructorFlag flag) { }
        public Optimization() { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Attribute to define Information of a Node. Can be used to Define additional Information that should be displayed in the Editor/Frontend.
    /// </summary>
    [StorableType("1E3C7C25-2F33-4104-89C5-DD2F832776F1")]
    public class Information : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructor of the Attribute.
        /// </summary>
        /// <param name="info"> Information of the simulation node. </param>
        public Information(string info) => Info = info;
        [StorableConstructor]
        public Information(StorableConstructorFlag flag){}
        [Storable]
        public string Info { get; set; }
        public override string ToString() => Info;
    }
    /// <inheritdoc />
    /// <summary>
    /// Attribute that defines the dependencies between visualization objects, data context and the window.
    /// </summary>
    [StorableType("449848A6-4C96-4B8B-8B56-3A1BBED836ED")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class VisualizationType : Attribute
    {
        [StorableConstructor]
        public VisualizationType(StorableConstructorFlag flag){}

        public VisualizationType(string windowType, Type classType, Type layoutType)
        {
            WindowType = windowType;
            ClassType = classType;
            LayoutType = layoutType;
        }
        /// <summary>
        /// Type of the window that is used for the visualization.
        /// </summary>
        [Storable]
        public string WindowType { get; set; }
        /// <summary>
        /// Data context of the visualization Type.
        /// </summary>
        [Storable]
        public Type ClassType { get; set; }
        /// <summary>
        /// Visualization type of the Node.
        /// </summary>
        [Storable]
        public Type LayoutType { get; set; }
        public override string ToString()
        {
            return WindowType + " " + ClassType.Name + " LayoutType: " + LayoutType.Name;
        }
    }
}
