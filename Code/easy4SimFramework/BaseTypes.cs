using System;
using System.Collections.Generic;
using System.Reflection;
using HEAL.Attic;

namespace Easy4SimFramework
{
    [StorableType("F9B502AD-F2EC-49E8-BF18-AF9DF6C597D0")]
    public class ParameterArrayOptimization : ICloneable<ParameterArrayOptimization>
    {
        /// <summary>
        /// Object for which parameters should be optimized
        /// </summary>
        [Storable]
        public CSimBase BaseObject { get; set; }

        /// <summary>
        /// PropertyInfo of the Property
        /// </summary>
        [Storable]
        public PropertyInfo PropertyInfo { get; set; }

        public ParameterArrayOptimization() { }
        [StorableConstructor]
        protected ParameterArrayOptimization(StorableConstructorFlag flag) { }

        protected ParameterArrayOptimization(CSimBase cSimBase, PropertyInfo propertyInfo)
        {
            BaseObject = cSimBase;
            PropertyInfo = propertyInfo;
        }

        public ParameterArrayOptimization Clone()
        {
            ParameterArrayOptimization result = new ParameterArrayOptimization();
            if (BaseObject != null)
                result.BaseObject = BaseObject;
            result.PropertyInfo = PropertyInfo;
            return result;
        }
    }

    /// <summary>
    /// Abstract base class for all input and output types.
    /// Every input and output type needs a parent
    /// </summary>
    [StorableType("499CAAEC-6DB0-49E7-AE13-F5B6E5C2AA08")]
    public abstract class InputOutputBase
    {
        [Storable]
        public ISimBase Parent { get; set; }

        [Storable] public abstract bool Ready { get; set; } 
        [StorableConstructor]
        protected InputOutputBase(StorableConstructorFlag flag) { }
        protected InputOutputBase() => Parent = null;
        protected InputOutputBase(CSimBase simBase) => Parent = simBase;
    }
    /// <summary>
    /// Abstract base class for all input and output types.
    /// Every input and output type needs a value
    /// </summary>
    /// <typeparam name="T">Type of the input or output</typeparam>
    [StorableType("530DE729-C527-402B-8468-AB52CFFE8552")]
    public abstract class InputOutputBase<T> : InputOutputBase
    {
        protected InputOutputBase(CSimBase p) : base(p) { }
        protected InputOutputBase() { }
        [StorableConstructor]
        protected InputOutputBase(StorableConstructorFlag flag) : base(flag) { }
        [Storable]
        public T Value { get; set; }
        public virtual void SetStartValue(T value) => Value = value;
    }
    /// <inheritdoc />
    /// <summary>
    /// Abstract base class for output types.
    /// An output type needs to derive from this base class, for the counter input to connect to this output. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("4DDBB180-7583-4D0E-ABAC-09182A4C06AB")]
    public abstract class OutputBase<T> : InputOutputBase<T>
    {
        [Storable]
        public InputOutputBase ConnectedInput { get; set; }
        public override bool Ready
        {
            get => ConnectedInput.Ready;
            set => ConnectedInput.Ready = value;
        }
        protected OutputBase(CSimBase p) : base(p) { }
        protected OutputBase() { }
        [StorableConstructor]
        protected OutputBase(StorableConstructorFlag flag) : base(flag) { }
        public abstract void Set(T value);
        public static implicit operator T(OutputBase<T> s) => s.Value;
        public static implicit operator string(OutputBase<T> s) => s.Value.ToString();

    }
    /// <inheritdoc />
    /// <summary>
    /// Abstract base class for all Input Types -&gt; connect Method needs to be overwritten in the derived class. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("68AC3CE0-EF41-4C9C-9B94-582E85901EDF")]
    public abstract class InputBase<T> : InputOutputBase<T>
    {
        [Storable]
        public InputOutputBase ConnectedOutput { get; set; }
        protected InputBase(CSimBase p) : base(p) { }
        public override bool Ready { get; set; } = true;
        protected InputBase() { }
        [StorableConstructor]
        protected InputBase(StorableConstructorFlag flag) : base(flag) { }
        public abstract void Connect(InputOutputBase output);
        public static implicit operator T(InputBase<T> s) => s.Value;
        public static implicit operator string(InputBase<T> s) => s.Value.ToString();
    }

    /// <summary>
    /// Base class for matrix classes in the Easy4Sim Framework.
    /// </summary>
    /// <typeparam name="T"> Type of the matrix, typically in the Easy4Sim Framework int, and double is used</typeparam>
    [StorableType("D4133DCA-29FF-422D-A739-89A425349BAF")]
    public abstract class MatrixBase<T>
    {
        [Storable]
        public T[,] Matrix { get; set; }
        #region Ctor
        [StorableConstructor]
        protected MatrixBase(StorableConstructorFlag flag) { }
        protected MatrixBase() => Matrix = new T[0, 0];
        protected MatrixBase(int rows, int columns) => Matrix = new T[rows, columns];
        #endregion
        public int Rows
        {
            get => Matrix.GetLength(0);
            set
            {
                if (value == Rows) return; //Only copy to new matrix if row number changed
                T[,] newArray = new T[value, Columns];
                Array.Copy(Matrix, newArray, Math.Min(value * Columns, Matrix.Length));
                Matrix = newArray;
            }
        }
        public int Columns
        {
            get => Matrix.GetLength(1);
            set
            {
                if (value == Columns) return; //Only copy to new matrix if column number changed
                T[,] newArray = new T[Rows, value];
                for (int i = 0; i < Rows; i++)
                    Array.Copy(Matrix, i * Columns, newArray, i * value, Math.Min(value, Columns));
                Matrix = newArray;
            }
        }
        public virtual T this[int rowIndex, int columnIndex]
        {
            get => Matrix[rowIndex, columnIndex];
            set
            {
                if (!value.Equals(Matrix[rowIndex, columnIndex]))
                    Matrix[rowIndex, columnIndex] = value;
            }
        }
        public T[,] CloneAsMatrix()
        {
            return (T[,])Matrix.Clone();
        }
    }

    /// <summary>
    /// Base class for array classes used in the Easy4SimFramework
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("CA5FC108-E70E-49E9-8A4F-3FE8F1834A17")]
    public abstract class ArrayBase<T>
    {
        [Storable]
        public T[] ValueArray { get; set; } = new T[0];

        [Storable]
        public int Size
        {
            get => ValueArray.Length;
            set
            {
                if (value == ValueArray.Length) return; //only resize if size changes
                T[] newArray = new T[value];
                Array.Copy(ValueArray, newArray, Math.Min(value, ValueArray.Length)); //Copy as many values from old array as possible, for example if an Array is resized from length 5 to 3, the new Array is filled with the first three values from the old array
                ValueArray = newArray;
            }
        }
        [StorableConstructor]
        protected ArrayBase(StorableConstructorFlag flag) { }

        protected ArrayBase() { }

        protected ArrayBase(int size) => ValueArray = new T[size];
        public virtual T this[int index]
        {
            get => ValueArray[index];
            set
            {
                if (!value.Equals(ValueArray[index]))
                    ValueArray[index] = value;
            }
        }
        public T[] CloneAsArray()
        {
            return (T[])ValueArray.Clone();
        }

    }

    /// <summary>
    /// Base class for list classes used in the Easy4SimFramework
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [StorableType("E22BD386-FCDB-4706-9E1D-9CB1060C966A")]
    public abstract class ListBase<T>
    {
        [Storable] public List<T> Value { get; set; } = new List<T>();

        [Storable]
        public int Count => Value.Count;

        [StorableConstructor]
        protected ListBase(StorableConstructorFlag flag) { }

        protected ListBase() { }

        public virtual T this[int index]
        {
            get => Value[index];
            set
            {
                if (!value.Equals(Value[index]))
                    Value[index] = value;
            }
        }
        public List<T> CloneAsList()
        {
            List<T> resultList = new List<T>();
            resultList.AddRange(Value);
            return resultList;
        }
    }




    [StorableType("9CE1BFA0-E22C-4DA6-924D-3A6C06EC13F7")]
    public abstract class VisualizationInitializedWithSolver
    {
        [Storable]
        public SolverSettings SolverSettings { get; set; }
        protected VisualizationInitializedWithSolver(SolverSettings settings) => SolverSettings = settings;
        /// <summary>
        /// Typically this Constructor is not used -> we have to use this constructor to set a DataContext in the view
        /// </summary>
        protected VisualizationInitializedWithSolver() { }
        [StorableConstructor]
        protected VisualizationInitializedWithSolver(StorableConstructorFlag flag) { }
        public virtual void UpdateReference() { }
    }

    //Abstract Base Class is needed to Create a List<ItemProperty>
    //https://stackoverflow.com/a/353134
    [StorableType("98E17CE1-E43E-4BF6-ABC3-0A90BF1ED42E")]
    public abstract class ItemProperty : ICloneable
    {
        [Storable]
        public string Name { get; set; }
        [Storable]
        public abstract Type CurrentType { get; }
        [StorableConstructor]
        protected ItemProperty(StorableConstructorFlag flag) { }

        protected ItemProperty(string name)
        {
            Name = name;
        }
        protected ItemProperty() { }
        public abstract object Clone();
    }

}
