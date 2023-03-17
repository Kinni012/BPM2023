using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HEAL.Attic;

namespace Easy4SimFramework
{
    static class ConstantValues
    {
        public const double CompareTolerance = 0.00001;
    }
    #region types
    #region ParameterValues
    /*====================== Struct serialization =============================
    https://github.com/HeuristicLab/HEAL.Attic/blob/master/docs/Manual.md
    Structs that should be included in serialization must be marked with the StorableType attribute. 
    HEAL.Attic automatically (de-)serializes all fields within structs. It is therefore not necessary to mark fields with the Storable attribute. 
    HEAL.Attic even throws an exception on serialization when it finds fields with Storable attributes within structs.
    =========================================================================== */

    /// <summary>
    /// Parameter type for real values(double)
    /// </summary>
    [StorableType("112DB78A-7F04-4DEE-9138-E30D48887CA1")]
    public struct ParameterReal : IValueable<double>
    {
        public double Value { get; set; }
        public ParameterReal(double s) { Value = s; }
        public static implicit operator double(ParameterReal d) => d.Value;
    }

    /// <summary>
    /// Parameter type for lists that should be optimized
    /// </summary>
    [StorableType("0C9C52A0-5640-4CB1-ADBC-A1468ED027FD")]
    public class ParameterArrayOptimization<T> : ParameterArrayOptimization, IValueable<T[]>, ICloneable<ParameterArrayOptimization<T>>
    {
        public int Amount => Value.Length;
        [Storable]
        public T[] Value { get; set; }
        [Storable]
        public T[] LowerBounds { get; set; }
        [Storable]
        public T[] UpperBounds { get; set; }

        [StorableConstructor]
        public ParameterArrayOptimization(StorableConstructorFlag flag) : base(flag) { }
        public ParameterArrayOptimization(int dimension)
        {
            Value = new T[dimension];
            LowerBounds = new T[dimension];
            UpperBounds = new T[dimension];
        }

        public ParameterArrayOptimization<T> Clone()
        {
            ParameterArrayOptimization<T> result = new ParameterArrayOptimization<T>(Value.Length);
            for (int i = 0; i < Value.Length; i++)
            {
                result.Value[i] = Value[i];
                result.LowerBounds[i] = LowerBounds[i];
                result.UpperBounds[i] = UpperBounds[i];

            }
            return result;
        }
    }

    /// <summary>
    /// Parameter type for int values
    /// </summary>
    [StorableType("F5B302C6-5BA1-4737-8940-68C601A11899")]
    public struct ParameterInt : IValueable<int>
    {
        public int Value { get; set; }
        public ParameterInt(int s) { Value = s; }
        public static implicit operator int(ParameterInt d) => d.Value;
    }

    /// <summary>
    /// Parameter type for dimension values
    /// </summary>
    [StorableType("411E2DA9-3437-4A6D-AD5C-AC6B3DC8BEE4")]
    public struct ParameterDimension : IValueable<int>
    {
        public int Value { get; set; }
        public ParameterDimension(int s) { Value = s; }
        public static implicit operator int(ParameterDimension d) => d.Value;
        public T[] ConnectIndexReturn<T>(ref T[] input, int arrayLength, CSimBase p) where T : InputOutputBase
        {
            input = new T[arrayLength];
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(CSimBase) });
            if (constructorInfo != null)
            {
                object[] parameters = { p };
                for (int i = 0; i < arrayLength; i++)
                    input[i] = (T)constructorInfo.Invoke(parameters);
            }
            return input;
        }
    }

    /// <summary>
    /// Parameter type for bool values
    /// </summary>
    [StorableType("17056C64-B75B-41BC-A4D3-1B9E72BDBDD3")]
    public struct ParameterBool : IValueable<bool>
    {
        public bool Value { get; set; }
        public ParameterBool(bool s) { Value = s; }
        public static implicit operator bool(ParameterBool d) => d.Value;
    }
    /// <summary>
    /// Parameter type for string values.
    /// </summary>
    [StorableType("4F05E609-D913-4231-956F-2C6D4C032CB6")]
    public struct ParameterString : IValueable<string>
    {
        public string Value { get; set; }
        public ParameterString(string s) { Value = s; }
        public static implicit operator string(ParameterString d) => d.Value;
    }
    #endregion
    #region matrix types
    [StorableType("D31B80F4-3C41-4E5B-BB0F-B2522AC940C7")]
    public class BooleanMatrix : MatrixBase<bool>
    {
        public BooleanMatrix() { }
        [StorableConstructor]
        public BooleanMatrix(StorableConstructorFlag flag) : base(flag) { }
        public BooleanMatrix(int rows, int columns) : base(rows, columns) { }
    }
    [StorableType("86D89FF3-EA6A-4AE7-A35A-626ADFF3879F")]
    public class DoubleMatrix : MatrixBase<double>
    {
        public DoubleMatrix() { }
        [StorableConstructor]
        public DoubleMatrix(StorableConstructorFlag flag) : base(flag) { }
        public DoubleMatrix(int rows, int columns) : base(rows, columns) { }
    }
    [StorableType("1591A60F-CB93-40B8-915F-149A0AE609D7")]
    public class IntegerMatrix : MatrixBase<int>
    {
        public IntegerMatrix() { }
        [StorableConstructor]
        public IntegerMatrix(StorableConstructorFlag flag) : base(flag) { }
        public IntegerMatrix(int rows, int columns) : base(rows, columns) { }
    }
    [StorableType("6D495515-BBF6-4D62-B728-D6DEAD8DCE12")]
    public class StringMatrix : MatrixBase<string>
    {
        public StringMatrix() { }
        [StorableConstructor]
        public StringMatrix(StorableConstructorFlag flag) : base(flag) { }
        public StringMatrix(int rows, int columns) : base(rows, columns) { }
    }
    #endregion

    #region ArrayTypes
    [StorableType("E16DEE1C-CD4E-4B18-933C-31CE75349548")]
    public class BoolArray : ArrayBase<bool>
    {
        public BoolArray() { }
        [StorableConstructor]
        public BoolArray(StorableConstructorFlag flag) : base(flag) { }
        public BoolArray(int size) : base(size) { }
    }
    [StorableType("C218753C-7B4D-4DE0-8D79-3EE82B7AA442")]
    public class IntArray : ArrayBase<int>
    {
        public IntArray() { }
        [StorableConstructor]
        public IntArray(StorableConstructorFlag flag) : base(flag) { }
        public IntArray(int size) : base(size) { }
    }
    [StorableType("86BF8B48-E76C-4823-B9C9-F52086A441C0")]
    public class RealArray : ArrayBase<double>
    {
        public RealArray() { }
        [StorableConstructor]
        public RealArray(StorableConstructorFlag flag) : base(flag) { }
        public RealArray(int size) : base(size) { }
    }
    [StorableType("CE3CC990-6B23-4EB3-8D25-C81A768A8FDA")]
    public class StringArray : ArrayBase<string>
    {
        public StringArray() { }
        [StorableConstructor]
        public StringArray(StorableConstructorFlag flag) : base(flag) { }
        public StringArray(int size) : base(size) { }
    }
    /// <summary>
    /// Generic Array to exchange data between Nodes
    /// </summary>
    [StorableType("FF33229F-BA25-462C-846F-8E8F5D13C846")]
    public class ObjectArray : ArrayBase<object>
    {
        public ObjectArray() { }
        [StorableConstructor]
        public ObjectArray(StorableConstructorFlag flag) : base(flag) { }
        public ObjectArray(int size) : base(size) { }
    }


    /// <summary>
    /// Generic Array to exchange data between Nodes
    /// </summary>
    [StorableType("3843703A-359F-4661-A7D8-A9A110B38D11")]
    public class ObjectList : ListBase<object>
    {
        public ObjectList() { }
        [StorableConstructor]
        public ObjectList(StorableConstructorFlag flag) : base(flag) { }

    }


    #endregion
    #endregion types
    #region Input and output types

    #region FunctionPointerConnection
    ///<summary>
    /// Use a function pointer to pass any object and get a return value as object
    /// </summary>
    public delegate object DelegateFunction(object parameter);

    /// <summary>
    /// Typically this is used to offer a method to the Component that is connected.
    /// This can be used to offer additional functionality during the simulation runtime.
    ///
    /// An example would be, that a component A offers an optimization function to component B.
    /// Component B can now call this optimization function on a problem and gets a good solution.
    /// This is easier than component B passes the problem to component A and gets a optimized result.
    /// </summary>
    [StorableType("F7DADC3F-BDFB-46D3-8D5A-8124C7478A78")]
    public class OutFunction : OutputBase<DelegateFunction>, IValueable<DelegateFunction>
    {
        [StorableConstructor]
        public OutFunction(StorableConstructorFlag flag) : base(flag) { }
        public OutFunction(CSimBase p) : base(p) { }
        public OutFunction(CSimBase p, DelegateFunction v) : base(p) => Value = v;
        public override void Set(DelegateFunction value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InFunction inFunction:
                    inFunction.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Typically this is used to receive a method from a Component that is connected.
    /// This can be used to receive additional functionality during the simulation runtime.
    ///
    /// An example would be, that a component A offers an optimization function to component B.
    /// Component B can now call this optimization function on a problem and gets a good solution.
    /// This is easier than component B passes the problem to component A and gets a optimized result.
    /// </summary>
    [StorableType("D7937E08-BD07-4688-B5CD-E6F9A51E2C5A")]
    public class InFunction : InputBase<DelegateFunction>, IValueable<DelegateFunction>
    {
        [StorableConstructor]
        public InFunction(StorableConstructorFlag flag) : base(flag) { }
        public InFunction(CSimBase p) : base(p) { }
        public InFunction(CSimBase p, DelegateFunction value) : base(p) => Value = value;
        public override void Connect(InputOutputBase outFunction)
        {
            switch (outFunction)
            {
                case OutFunction of:
                    of.ConnectedInput = this;
                    ConnectedOutput = of;
                    Value = of.Value;
                    break;
            }
        }
    }



    #endregion

    #region object
    /// <summary>
    /// Can be used to pass Parameters if there are no suitable types in the framework.
    /// If this is used to often, it could be better to implement custom Input and Output types.
    /// </summary>
    [StorableType("EF7B35FF-D1D2-46BE-B3CD-322A8156BA76")]
    public class OutObject : OutputBase<object>, IValueable<object>
    {
        [StorableConstructor]
        public OutObject(StorableConstructorFlag flag) : base(flag) { }
        public OutObject(CSimBase p) : base(p) { }
        public OutObject(CSimBase p, object v) : base(p) => Value = v;
        public override void Set(object value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InObject inObject:
                    inObject.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Can be used to receive Parameters if there are no suitable types in the framework.
    /// If this is used to often, it could be better to implement custom Input and Output types.
    /// </summary>
    [StorableType("561AFD0B-46F6-4ADD-972A-915293CC5500")]
    public class InObject : InputBase<object>, IValueable<object>
    {
        public InObject(StorableConstructorFlag flag) : base(flag) { }
        public InObject(CSimBase p) : base(p) { }
        public InObject(CSimBase p, object value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutObject outObject:
                    outObject.ConnectedInput = this;
                    ConnectedOutput = outObject;
                    Value = outObject.Value;
                    break;
            }
        }
    }
    #endregion
    #region Item
    /// <summary>
    /// Can be used to pass Parameters if there are no suitable types in the framework.
    /// If this is used to often, it could be better to implement custom Input and Output types.
    /// </summary>
    [StorableType("035839EC-124D-409F-93AA-8144E1B6AED5")]
    public class OutItem : OutputBase<Item>, IValueable<Item>
    {
        [StorableConstructor]
        public OutItem(StorableConstructorFlag flag) : base(flag) { }
        public OutItem(CSimBase p) : base(p) { }
        public OutItem(CSimBase p, Item v) : base(p) => Value = v;
        public override void Set(Item value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InputBase<Item> inItem when inItem is IValueable<Item>:
                    inItem.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Can be used to receive Parameters if there are no suitable types in the framework.
    /// If this is used to often, it could be better to implement custom Input and Output types.
    /// </summary>
    [StorableType("D8068115-7410-402E-B590-6B0D19937355")]
    public class InItem : InputBase<Item>, IValueable<Item>
    {
        public InItem(StorableConstructorFlag flag) : base(flag) { }
        public InItem(CSimBase p) : base(p) { }
        public InItem(CSimBase p, Item value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<Item> outItem when outItem is IValueable<Item>:
                    outItem.ConnectedInput = this;
                    ConnectedOutput = outItem;
                    Value = outItem.Value;
                    break;
            }
        }
    }
    #endregion


    #region bool
    /// <summary>
    /// Output type for bool values
    /// </summary>
    [StorableType("E746E5A4-5B83-44B3-9F26-455E6FEFB80D")]
    public class OutBool : OutputBase<bool>, IValueable<bool>
    {
        [StorableConstructor]
        public OutBool(StorableConstructorFlag flag) : base(flag) { }
        public OutBool(CSimBase p) : base(p) { }
        public OutBool(CSimBase p, bool v) : base(p) => Value = v;
        public override void Set(bool value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    inBool.Value = value;
                    break;
                case IValueable<int> inInt when inInt is InputBase<int>:
                    inInt.Value = Convert.ToInt32(value);
                    break;
                case IValueable<double> inReal when inReal is InputBase<double>:
                    inReal.Value = Convert.ToDouble(value);
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for bool values
    /// </summary>
    [StorableType("7E19E689-4527-4730-8EDC-AFA3C68856B3")]
    public class InBool : InputBase<bool>, IValueable<bool>
    {
        [StorableConstructor]
        public InBool(StorableConstructorFlag flag) : base(flag) { }
        public InBool(CSimBase p) : base(p) { }
        public InBool(CSimBase p, bool value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = outBool.Value;
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value != 0;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = Math.Abs(outReal.Value) > ConstantValues.CompareTolerance;
                    break;
            }
        }
    }
    #endregion
    #region int
    /// <summary>
    /// Output type for int values
    /// </summary>
    [StorableType("A828BE90-5DAC-4D5D-B3A8-05FBB583FD59")]
    public class OutInt : OutputBase<int>, IValueable<int>
    {
        [StorableConstructor]
        public OutInt(StorableConstructorFlag flag) : base(flag) { }
        public OutInt(CSimBase p) : base(p) { }
        public OutInt(CSimBase p, int v) : base(p) => Value = v;
        public override void Set(int value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    inBool.Value = value != 0;
                    break;
                case IValueable<int> inInt when inInt is InputBase<int>:
                    inInt.Value = value;
                    break;
                case IValueable<double> inReal when inReal is InputBase<double>:
                    inReal.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for int values
    /// </summary>
    [StorableType("18740A85-6911-4067-BF17-03E43848A65F")]
    public class InInt : InputBase<int>, IValueable<int>
    {
        [StorableConstructor]
        public InInt(StorableConstructorFlag flag) : base(flag) { }
        public InInt(CSimBase p) : base(p) { }
        public InInt(CSimBase p, int value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = Convert.ToInt32(outBool.Value);
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = Convert.ToInt32(outReal.Value);
                    break;
            }
        }
    }
    #endregion
    #region real
    /// <summary>
    /// Output type for double values
    /// </summary>
    [StorableType("820A5465-5BF2-4EE8-A791-C73F124DE28C")]
    public class OutReal : OutputBase<double>, IValueable<double>
    {
        [StorableConstructor]
        public OutReal(StorableConstructorFlag flag) : base(flag) { }
        public OutReal(CSimBase p) : base(p) { }
        public OutReal(CSimBase p, double v) : base(p) => Value = v;
        public override void Set(double value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    inBool.Value = Math.Abs(value) > ConstantValues.CompareTolerance;
                    break;
                case IValueable<int> inInt when inInt is InputBase<int>:
                    inInt.Value = Convert.ToInt32(value);
                    break;
                case IValueable<double> inReal when inReal is InputBase<double>:
                    inReal.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for double values
    /// </summary>
    [StorableType("3B7B27DA-1422-4254-A6BB-1C362FE7C936")]
    public class InReal : InputBase<double>, IValueable<double>
    {
        [StorableConstructor]
        public InReal(StorableConstructorFlag flag) : base(flag) { }
        public InReal(CSimBase p) : base(p) { }
        public InReal(CSimBase p, double value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = Convert.ToDouble(outBool.Value);
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = outReal.Value;
                    break;
            }
        }
    }

    #endregion
    #region string
    /// <summary>
    /// Output type for string values
    /// </summary>
    [StorableType("AA1F5E5B-C04C-44DA-810B-C43FB4468F1C")]
    public class OutString : OutputBase<string>, IValueable<string>
    {
        [StorableConstructor]
        public OutString(StorableConstructorFlag flag) : base(flag) { }
        public OutString(CSimBase p) : base(p) { }
        public OutString(CSimBase p, string v) : base(p) => Value = v;
        public override void Set(string value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<string> inString when inString is InputBase<string>:
                    inString.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for string values
    /// </summary>
    [StorableType("9E4010F1-16EF-4B7F-B5C3-7BBED28B5D8C")]
    public class InString : InputBase<string>, IValueable<string>
    {
        [StorableConstructor]
        public InString(StorableConstructorFlag flag) : base(flag) { }
        public InString(CSimBase p) : base(p) { }
        public InString(CSimBase p, string value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<string> outString when outString is IValueable<string>:
                    outString.ConnectedInput = this;
                    ConnectedOutput = outString;
                    Value = outString.Value;
                    break;
            }
        }
    }
    #endregion


    #region boolMatrix
    /// <summary>
    /// Output type for bool matrix values
    /// </summary>
    [StorableType("3DD4D9DC-BADE-4561-8E53-644D02341295")]
    public class OutBoolMatrix : OutputBase<MatrixBase<bool>>, IValueable<MatrixBase<bool>>
    {
        [StorableConstructor]
        public OutBoolMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutBoolMatrix(CSimBase p) : base(p) { }
        public OutBoolMatrix(CSimBase p, MatrixBase<bool> v) : base(p) => Value = v;
        public override void Set(MatrixBase<bool> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InBoolMatrix inBool:
                    inBool.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for bool matrix values
    /// </summary>
    [StorableType("7D1E202F-BB1D-42D6-B0BC-A519F487B7A6")]
    public class InBoolMatrix : InputBase<MatrixBase<bool>>, IValueable<MatrixBase<bool>>
    {
        [StorableConstructor]
        public InBoolMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InBoolMatrix(CSimBase p) : base(p) { }
        public InBoolMatrix(CSimBase p, MatrixBase<bool> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutBoolMatrix outBoolMatrix:
                    outBoolMatrix.ConnectedInput = this;
                    ConnectedOutput = outBoolMatrix;
                    Value = outBoolMatrix.Value;
                    break;
            }
        }
    }
    #endregion boolMatrix
    #region intMatrix
    /// <summary>
    /// Output type for int matrix values
    /// </summary>
    [StorableType("F4A7D851-89EE-4B99-92A3-BB376D00EBDA")]
    public class OutIntMatrix : OutputBase<MatrixBase<int>>, IValueable<MatrixBase<int>>
    {
        [StorableConstructor]
        public OutIntMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutIntMatrix(CSimBase p) : base(p) { }
        public OutIntMatrix(CSimBase p, MatrixBase<int> v) : base(p) => Value = v;
        public override void Set(MatrixBase<int> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InIntMatrix inInt:
                    inInt.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for int matrix values
    /// </summary>
    [StorableType("5A2FA84B-63B2-4DCD-BCCF-896446DB66B8")]
    public class InIntMatrix : InputBase<MatrixBase<int>>, IValueable<MatrixBase<int>>
    {
        [StorableConstructor]
        public InIntMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InIntMatrix(CSimBase p) : base(p) { }
        public InIntMatrix(CSimBase p, MatrixBase<int> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutIntMatrix outIntMatrix:
                    outIntMatrix.ConnectedInput = this;
                    ConnectedOutput = outIntMatrix;
                    Value = outIntMatrix.Value;
                    break;
            }
        }
    }
    #endregion intMatrix
    #region doubleMatrix
    /// <summary>
    /// Output type for double matrix values
    /// </summary>
    [StorableType("E875B07F-9C8A-4890-94BD-05BA92D965DA")]
    public class OutRealMatrix : OutputBase<MatrixBase<double>>, IValueable<MatrixBase<double>>
    {
        [StorableConstructor]
        public OutRealMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutRealMatrix(CSimBase p) : base(p) { }
        public OutRealMatrix(CSimBase p, MatrixBase<double> v) : base(p) => Value = v;
        public override void Set(MatrixBase<double> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InRealMatrix inInt:
                    inInt.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for double matrix values
    /// </summary>
    [StorableType("B24EE8AB-B1F6-4857-8400-8F729A504606")]
    public class InRealMatrix : InputBase<MatrixBase<double>>, IValueable<MatrixBase<double>>
    {
        [StorableConstructor]
        public InRealMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InRealMatrix(CSimBase p) : base(p) { }
        public InRealMatrix(CSimBase p, MatrixBase<double> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutRealMatrix outRealMatrix:
                    outRealMatrix.ConnectedInput = this;
                    ConnectedOutput = outRealMatrix;
                    Value = outRealMatrix.Value;
                    break;
            }
        }
    }
    #endregion
    #region stringMatrix
    /// <summary>
    /// Output type for string matrix values
    /// </summary>
    [StorableType("3E3CABA2-2B0A-48F4-A0C9-D0A20CC553AA")]
    public class OutStringMatrix : OutputBase<MatrixBase<string>>, IValueable<MatrixBase<string>>
    {
        [StorableConstructor]
        public OutStringMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutStringMatrix(CSimBase p) : base(p) { }
        public OutStringMatrix(CSimBase p, MatrixBase<string> v) : base(p) => Value = v;
        public override void Set(MatrixBase<string> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InStringMatrix inString:
                    inString.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for string matrix values
    /// </summary>
    [StorableType("7A006763-BA5B-43FB-BA3B-0E06E06BF564")]
    public class InStringMatrix : InputBase<MatrixBase<string>>, IValueable<MatrixBase<string>>
    {
        [StorableConstructor]
        public InStringMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InStringMatrix(CSimBase p) : base(p) { }
        public InStringMatrix(CSimBase p, MatrixBase<string> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutStringMatrix outStringMatrix:
                    outStringMatrix.ConnectedInput = this;
                    ConnectedOutput = outStringMatrix;
                    Value = outStringMatrix.Value;
                    break;
            }
        }
    }
    #endregion

    #region boolArray
    /// <summary>
    /// Output type for bool array values
    /// </summary>
    [StorableType("96A64ABC-7550-4A47-BF5B-C35946718AEF")]
    public class OutBoolArray : OutputBase<ArrayBase<bool>>, IValueable<ArrayBase<bool>>
    {
        [StorableConstructor]
        public OutBoolArray(StorableConstructorFlag flag) : base(flag) { }
        public OutBoolArray(CSimBase p) : base(p) { }
        public OutBoolArray(CSimBase p, ArrayBase<bool> v) : base(p) => Value = v;
        public override void Set(ArrayBase<bool> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InBoolArray inBool:
                    inBool.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for bool array values
    /// </summary>
    [StorableType("8D30F21A-6281-4B9C-8ECB-3F9C30516E19")]
    public class InBoolArray : InputBase<ArrayBase<bool>>, IValueable<ArrayBase<bool>>
    {
        [StorableConstructor]
        public InBoolArray(StorableConstructorFlag flag) : base(flag) { }
        public InBoolArray(CSimBase p) : base(p) { }
        public InBoolArray(CSimBase p, ArrayBase<bool> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutBoolArray outBoolArray:
                    outBoolArray.ConnectedInput = this;
                    ConnectedOutput = outBoolArray;
                    Value = outBoolArray.Value;
                    break;
            }
        }
    }
    #endregion boolArray
    #region intArray

    /// <summary>
    /// Output type for int array values
    /// </summary>
    [StorableType("83D6E14C-454C-492E-8C84-64461D61ECAA")]
    public class OutIntArray : OutputBase<ArrayBase<int>>, IValueable<ArrayBase<int>>
    {
        [StorableConstructor]
        public OutIntArray(StorableConstructorFlag flag) : base(flag) { }
        public OutIntArray(CSimBase p) : base(p) { }
        public OutIntArray(CSimBase p, ArrayBase<int> v) : base(p) => Value = v;
        public override void Set(ArrayBase<int> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InIntArray inInt:
                    inInt.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for int array values
    /// </summary>
    [StorableType("94178431-EAA8-48BA-87EA-21F50CBE370C")]
    public class InIntArray : InputBase<ArrayBase<int>>, IValueable<ArrayBase<int>>
    {
        [StorableConstructor]
        public InIntArray(StorableConstructorFlag flag) : base(flag) { }
        public InIntArray(CSimBase p) : base(p) { }
        public InIntArray(CSimBase p, ArrayBase<int> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutIntArray outIntArray:
                    outIntArray.ConnectedInput = this;
                    ConnectedOutput = outIntArray;
                    Value = outIntArray.Value;
                    break;
            }
        }
    }

    #endregion intArray
    #region doubleArray
    /// <summary>
    /// Output type for double array values
    /// </summary>
    [StorableType("61A8EF44-DC47-425D-85C9-842FBAFBB57B")]
    public class OutRealArray : OutputBase<ArrayBase<double>>, IValueable<ArrayBase<double>>
    {
        [StorableConstructor]
        public OutRealArray(StorableConstructorFlag flag) : base(flag) { }
        public OutRealArray(CSimBase p) : base(p) { }
        public OutRealArray(CSimBase p, ArrayBase<double> v) : base(p) => Value = v;
        public override void Set(ArrayBase<double> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InRealArray inInt:
                    inInt.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for double array values
    /// </summary>
    [StorableType("39B85ACC-0574-468F-A514-D73C5AEDDD39")]
    public class InRealArray : InputBase<ArrayBase<double>>, IValueable<ArrayBase<double>>
    {
        [StorableConstructor]
        public InRealArray(StorableConstructorFlag flag) : base(flag) { }
        public InRealArray(CSimBase p) : base(p) { }
        public InRealArray(CSimBase p, ArrayBase<double> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutRealArray outRealArray:
                    outRealArray.ConnectedInput = this;
                    ConnectedOutput = outRealArray;
                    Value = outRealArray.Value;
                    break;
            }
        }
    }
    #endregion doubleArray
    #region stringArray
    /// <summary>
    /// Output type for string array values
    /// </summary>
    [StorableType("0C1769A9-2ABE-4EE5-889B-DF5AC8AFEA8D")]
    public class OutStringArray : OutputBase<ArrayBase<string>>, IValueable<ArrayBase<string>>
    {
        [StorableConstructor]
        public OutStringArray(StorableConstructorFlag flag) : base(flag) { }
        public OutStringArray(CSimBase p) : base(p) { }
        public OutStringArray(CSimBase p, ArrayBase<string> v) : base(p) => Value = v;
        public override void Set(ArrayBase<string> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InStringArray inString:
                    inString.Value = value;
                    break;
            }
        }
    }
    /// <summary>
    /// Input type for string array values
    /// </summary>
    [StorableType("E6767BA9-6000-4D86-B520-0F03AECB0471")]
    public class InStringArray : InputBase<ArrayBase<string>>, IValueable<ArrayBase<string>>
    {
        [StorableConstructor]
        public InStringArray(StorableConstructorFlag flag) : base(flag) { }
        public InStringArray(CSimBase p) : base(p) { }
        public InStringArray(CSimBase p, ArrayBase<string> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutStringArray outStringArray:
                    outStringArray.ConnectedInput = this;
                    ConnectedOutput = outStringArray;
                    Value = outStringArray.Value;
                    break;
            }
        }
    }
    #endregion stringArray

    #region objectArray
    /// <summary>
    /// Output type for object array values
    /// </summary>
    [StorableType("A51F668C-E9A4-475C-A125-D0E02A4826A4")]
    public class OutObjectArray : OutputBase<ArrayBase<object>>, IValueable<ArrayBase<object>>
    {
        [StorableConstructor]
        public OutObjectArray(StorableConstructorFlag flag) : base(flag) { }
        public OutObjectArray(CSimBase p) : base(p) { }
        public OutObjectArray(CSimBase p, ArrayBase<object> v) : base(p) => Value = v;
        public override void Set(ArrayBase<object> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InObjectArray inString:
                    inString.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for object array values
    /// </summary>
    [StorableType("1E61D934-7A13-4BE5-96C6-10AD1C248E22")]
    public class InObjectArray : InputBase<ArrayBase<object>>, IValueable<ArrayBase<object>>
    {
        [StorableConstructor]
        public InObjectArray(StorableConstructorFlag flag) : base(flag) { }
        public InObjectArray(CSimBase p) : base(p) { }
        public InObjectArray(CSimBase p, ArrayBase<object> value) : base(p) => Value = value;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutObjectArray outObjectArray:
                    outObjectArray.ConnectedInput = this;
                    ConnectedOutput = outObjectArray;
                    Value = outObjectArray.Value;
                    break;
            }
        }
    }
    #endregion

    #region objectList
    /// <summary>
    /// Output type for object lists
    /// </summary>
    [StorableType("E27EE1A6-012E-4F58-A400-A0C65D2168A7")]
    public class OutObjectList : OutputBase<ListBase<object>>, IValueable<ListBase<object>>
    {
        [StorableConstructor]
        public OutObjectList(StorableConstructorFlag flag) : base(flag) { }

        public OutObjectList(CSimBase p) : base(p)
        {
            if (Value == null)
                Value = new ObjectList();
        }
        public OutObjectList(CSimBase p, ListBase<object> v) : base(p)
        {

            Value = v;
        }
        public OutObjectList(CSimBase p, List<object> v) : base(p)
        {
            if (Value == null)
                Value = new ObjectList();
            Value.Value = v;
        }

        public OutObjectList(CSimBase p, IEnumerable<object> v) : base(p)
        {
            if (Value == null)
                Value = new ObjectList();
            Value.Value = v.ToList();
        }
        public override void Set(ListBase<object> value)
        {
            Value = value;
            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InObjectList inList:
                    inList.Value = value;
                    break;
            }
        }
    }

    /// <summary>
    /// Input type for object lists
    /// </summary>
    [StorableType("A91EA1C8-F06D-4889-B79A-6AB5D627C7D0")]
    public class InObjectList : InputBase<ListBase<object>>, IValueable<ListBase<object>>
    {
        [StorableConstructor]
        public InObjectList(StorableConstructorFlag flag) : base(flag) { }

        public InObjectList(CSimBase p) : base(p)
        {

            if (Value == null)
                Value = new ObjectList();
        }
        public InObjectList(CSimBase p, ListBase<object> value) : base(p) => Value = value;

        public InObjectList(CSimBase p, List<object> v) : base(p)
        {

            if (Value == null)
                Value = new ObjectList();
            Value.Value = v;
        }
        public InObjectList(CSimBase p, IEnumerable<object> v) : base(p)
        {

            if (Value == null)
                Value = new ObjectList();
            Value.Value = v.ToList();
        }
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutObjectList outList:
                    outList.ConnectedInput = this;
                    ConnectedOutput = outList;
                    Value = outList.Value;
                    break;
            }
        }
    }
    #endregion

    #endregion Input and output types

    #region Input and ouput event types
    #region object
    /// <summary>
    /// Default output type for event based object Parameters
    /// </summary>
    [StorableType("DE79F250-5522-44D5-AA58-6F8B7C81196C")]
    public class OutEventObject : OutputBase<object>, IValueable<object>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventObject(StorableConstructorFlag flag) : base(flag) { }
        public OutEventObject(CSimBase p) : base(p) { }

        public OutEventObject(CSimBase p, object v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(object value)
        {
            if (Value == value)
                return;

            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<object> valueable when valueable is InputBase<object>:
                    if (valueable.Value == value) return;
                    valueable.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (valueable is InEventObject inEventObject)
                        inEventObject.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

            }
        }
    }
    /// <summary>
    /// Default input type for event based object Parameters
    /// </summary>
    [StorableType("51F24963-995B-4D0A-8D64-D63512104B78")]
    public class InEventObject : InputBase<object>, IValueable<object>
    {
        [Storable]
        public long LastT { get; set; } = -1;

        [StorableConstructor]
        public InEventObject(StorableConstructorFlag flag) : base(flag) { }
        public InEventObject(CSimBase p) : base(p) { }
        public InEventObject(CSimBase p, bool v) : base(p) => Value = v;
        public override void SetStartValue(object v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<object> outputBase when outputBase is IValueable<object>:
                    outputBase.ConnectedInput = this;
                    ConnectedOutput = outputBase;
                    Value = outputBase.Value;
                    break;
            }
        }
    }
    #endregion
    #region bool
    /// <summary>
    /// Default output type for event based bool Parameters
    /// </summary>
    [StorableType("A3DF24D3-EA4A-4F51-8A75-B06479E1ECA6")]
    public class OutEventBool : OutputBase<bool>, IValueable<bool>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventBool(StorableConstructorFlag flag) : base(flag) { }
        public OutEventBool(CSimBase p) : base(p) { }

        public OutEventBool(CSimBase p, bool v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(bool value)
        {
            if (Value == value)
                return;

            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    if (inBool.Value == value) return;
                    inBool.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inBool is InEventBool inEventBool)
                        inEventBool.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

                case IValueable<int> inInt when inInt is InputBase<int>:
                    int intValue = Convert.ToInt32(value);
                    if (inInt.Value == intValue) return;
                    inInt.Value = intValue;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inInt is InEventInt inEventInt)
                        inEventInt.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
                case IValueable<double> inReal when inReal is InputBase<double>:
                    double doubleValue = Convert.ToInt32(value);
                    if (Math.Abs(inReal.Value - doubleValue) < ConstantValues.CompareTolerance) return;
                    inReal.Value = doubleValue;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inReal is InEventReal inEventReal)
                        inEventReal.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based bool Parameters
    /// </summary>
    [StorableType("742B8170-62F4-4DDF-AAE8-B97D5B447F90")]
    public class InEventBool : InputBase<bool>, IValueable<bool>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventBool(StorableConstructorFlag flag) : base(flag) { }
        public InEventBool(CSimBase p) : base(p) { }
        public InEventBool(CSimBase p, bool v) : base(p) => Value = v;
        public static implicit operator bool(InEventBool s) => s.Value;
        public static implicit operator string(InEventBool s) => s.Value.ToString();
        public override void SetStartValue(bool v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = outBool.Value;
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value != 0;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = Math.Abs(outReal.Value) > ConstantValues.CompareTolerance;
                    break;
            }
        }
    }
    #endregion
    #region int

    /// <summary>
    /// Default output type for event based int Parameters
    /// </summary>
    [StorableType("1A9D6751-176B-4B37-BE01-E855F9FCA4AD")]
    public class OutEventInt : OutputBase<int>, IValueable<int>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventInt(StorableConstructorFlag flag) : base(flag) { }
        public OutEventInt(CSimBase p) : base(p) { }
        public OutEventInt(CSimBase p, int v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(int value)
        {
            if (Value == value)
                return;

            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    bool boolValue = Math.Abs(value) > ConstantValues.CompareTolerance;
                    if (inBool.Value == boolValue) return;
                    inBool.Value = boolValue;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inBool is InEventBool inEventBool)
                        inEventBool.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

                case IValueable<int> inInt when inInt is InputBase<int>:
                    if (inInt.Value == value) return;
                    inInt.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inInt is InEventInt inEventInt)
                        inEventInt.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

                case IValueable<double> inReal when inReal is InputBase<double>:
                    if (Math.Abs(inReal.Value - value) < ConstantValues.CompareTolerance) return;
                    inReal.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inReal is InEventReal inEventReal)
                        inEventReal.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }

    /// <summary>
    /// Default input type for event based int Parameters
    /// </summary>
    [StorableType("FCCA4FD1-0EF9-4D28-8DFD-423D6A25893A")]
    public class InEventInt : InputBase<int>, IValueable<int>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventInt(StorableConstructorFlag flag) : base(flag) { }
        public InEventInt(CSimBase p) : base(p) { }
        public InEventInt(CSimBase p, int v) : base(p) => Value = v;
        public static implicit operator int(InEventInt s) => s.Value;
        public static implicit operator string(InEventInt s) => s.Value.ToString();
        public override void SetStartValue(int v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = Convert.ToInt32(outBool.Value);
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = Convert.ToInt32(outReal.Value);
                    break;
            }
        }
    }
    #endregion
    #region real

    /// <summary>
    /// Default output type for event based real Parameters
    /// </summary>
    [StorableType("8D000E0F-2AAB-4AFA-8D88-BB2459114859")]
    public class OutEventReal : OutputBase<double>, IValueable<double>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventReal(StorableConstructorFlag flag) : base(flag) { }
        public OutEventReal(CSimBase p) : base(p) { }

        public OutEventReal(CSimBase p, double v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(double value)
        {
            if (Math.Abs(Value - value) < ConstantValues.CompareTolerance)
                return;

            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<bool> inBool when inBool is InputBase<bool>:
                    bool boolValue = Math.Abs(value) > ConstantValues.CompareTolerance;
                    if (inBool.Value == boolValue) return;
                    inBool.Value = boolValue;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inBool is InEventBool inEventBool)
                        inEventBool.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

                case IValueable<int> inInt when inInt is InputBase<int>:
                    int intValue = Convert.ToInt32(value);
                    if (inInt.Value == intValue) return;
                    inInt.Value = intValue;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inInt is InEventInt inEventInt)
                        inEventInt.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

                case IValueable<double> inReal when inReal is InputBase<double>:
                    if (Math.Abs(inReal.Value - value) < ConstantValues.CompareTolerance) return;
                    inReal.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inReal is InEventReal inEventReal)
                        inEventReal.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }

    /// <summary>
    /// Default input type for event based real Parameters
    /// </summary>
    [StorableType("F8884388-E87B-459F-8B6D-6115824D5FFA")]
    public class InEventReal : InputBase<double>, IValueable<double>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventReal(StorableConstructorFlag flag) : base(flag) { }
        public InEventReal(CSimBase p) : base(p) { }
        public InEventReal(CSimBase p, int v) : base(p) => Value = v;
        public static implicit operator double(InEventReal s) => s.Value;
        public static implicit operator string(InEventReal s) => s.Value.ToString(CultureInfo.CurrentCulture);
        public override void SetStartValue(double v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<bool> outBool when outBool is IValueable<bool>:
                    outBool.ConnectedInput = this;
                    ConnectedOutput = outBool;
                    Value = Convert.ToDouble(outBool.Value);
                    break;
                case OutputBase<int> outInt when outInt is IValueable<int>:
                    outInt.ConnectedInput = this;
                    ConnectedOutput = outInt;
                    Value = outInt.Value;
                    break;
                case OutputBase<double> outReal when outReal is IValueable<double>:
                    outReal.ConnectedInput = this;
                    ConnectedOutput = outReal;
                    Value = outReal.Value;
                    break;
            }
        }
    }

    #endregion
    #region string

    /// <summary>
    /// Default output type for event based string Parameters
    /// </summary>
    [StorableType("AEB8042A-E074-42FA-AE24-F423DAE775AE")]
    public class OutEventString : OutputBase<string>, IValueable<string>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventString(StorableConstructorFlag flag) : base(flag) { }
        public OutEventString(CSimBase p) : base(p) { }

        public OutEventString(CSimBase p, string v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(string value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<string> inString when inString is InputBase<string>:
                    inString.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inString is InEventString inEventString)
                        inEventString.LastT = Parent.Settings.Environment.SimulationTime;
                    break;

            }
        }
    }

    /// <summary>
    /// Default input type for event based string Parameters
    /// </summary>
    [StorableType("C7A1DF27-59BF-4D94-B1D5-5EF2BF5ABFCB")]
    public class InEventString : InputBase<string>, IValueable<string>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventString(StorableConstructorFlag flag) : base(flag) { }
        public InEventString(CSimBase p) : base(p) { }
        public InEventString(CSimBase p, string v) : base(p) => Value = v;
        public static implicit operator string(InEventString s) => s.Value;
        public override void SetStartValue(string v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<string> outString when outString is IValueable<string>:
                    outString.ConnectedInput = this;
                    ConnectedOutput = outString;
                    Value = outString.Value;
                    break;
            }
        }
    }

    #endregion



    #region eventObjectMatrix

    /// <summary>
    /// Default output type for event based object matrix
    /// </summary>
    [StorableType("FA3A6F1C-7DBA-4494-A3E2-D9D23F06CC9E")]
    public class OutEventObjectMatrix : OutputBase<MatrixBase<object>>, IValueable<MatrixBase<object>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventObjectMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutEventObjectMatrix(CSimBase p) : base(p) { }

        public OutEventObjectMatrix(CSimBase p, MatrixBase<object> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(MatrixBase<object> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventObjectMatrix inEventObjectMatrix:
                    inEventObjectMatrix.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventObjectMatrix.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based object matrix
    /// </summary>
    [StorableType("A1B02DBC-F7AA-4E01-8AC3-7ED8221E3518")]
    public class InEventObjectMatrix : InputBase<MatrixBase<object>>, IValueable<MatrixBase<object>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventObjectMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InEventObjectMatrix(CSimBase p) : base(p) { }
        public InEventObjectMatrix(CSimBase p, MatrixBase<object> v) : base(p) => Value = v;
        public override void SetStartValue(MatrixBase<object> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventObjectMatrix outEventObjectMatrix:
                    outEventObjectMatrix.ConnectedInput = this;
                    ConnectedOutput = outEventObjectMatrix;
                    Value = outEventObjectMatrix.Value;
                    break;
            }
        }
    }
    #endregion eventIntMatrix
    #region eventIntMatrix

    /// <summary>
    /// Default output type for event based int matrix
    /// </summary>
    [StorableType("A480F8A8-BD24-452E-9E10-10F7FC5B134B")]
    public class OutEventIntMatrix : OutputBase<MatrixBase<int>>, IValueable<MatrixBase<int>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventIntMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutEventIntMatrix(CSimBase p) : base(p) { }

        public OutEventIntMatrix(CSimBase p, MatrixBase<int> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(MatrixBase<int> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventIntMatrix inEventRealMatrix:
                    inEventRealMatrix.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventRealMatrix.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based int matrix
    /// </summary>
    [StorableType("2A86CC4F-2BD5-4B1E-A2AA-8CB2489D4C0C")]
    public class InEventIntMatrix : InputBase<MatrixBase<int>>, IValueable<MatrixBase<int>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventIntMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InEventIntMatrix(CSimBase p) : base(p) { }
        public InEventIntMatrix(CSimBase p, MatrixBase<int> v) : base(p) => Value = v;
        public override void SetStartValue(MatrixBase<int> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventIntMatrix ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventIntMatrix
    #region eventBoolMatrix

    /// <summary>
    /// Default output type for event based bool matrix
    /// </summary>
    [StorableType("3AA6EEAF-407F-41E4-A3F1-3B6F84E448B6")]
    public class OutEventBoolMatrix : OutputBase<MatrixBase<bool>>, IValueable<MatrixBase<bool>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventBoolMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutEventBoolMatrix(CSimBase p) : base(p) { }

        public OutEventBoolMatrix(CSimBase p, MatrixBase<bool> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(MatrixBase<bool> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventBoolMatrix inEventBoolMatrix:
                    inEventBoolMatrix.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventBoolMatrix.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based bool matrix
    /// </summary>
    [StorableType("0DBFDF80-0316-469C-952F-86E7106CA0A5")]
    public class InEventBoolMatrix : InputBase<MatrixBase<bool>>, IValueable<MatrixBase<bool>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventBoolMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InEventBoolMatrix(CSimBase p) : base(p) { }
        public InEventBoolMatrix(CSimBase p, MatrixBase<bool> v) : base(p) => Value = v;
        public override void SetStartValue(MatrixBase<bool> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventBoolMatrix ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventIntMatrix
    #region eventDoubleMatix
    /// <summary>
    /// Default output type for event based double matrix
    /// </summary>
    [StorableType("A74321BD-C7E1-4CF2-98A0-D2929FA4C8F9")]
    public class OutEventRealMatrix : OutputBase<MatrixBase<double>>, IValueable<MatrixBase<double>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventRealMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutEventRealMatrix(CSimBase p) : base(p) { }

        public OutEventRealMatrix(CSimBase p, MatrixBase<double> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(MatrixBase<double> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventRealMatrix inEventRealMatrix:
                    inEventRealMatrix.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventRealMatrix.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based double matrix
    /// </summary>
    [StorableType("34B19E2E-2B74-4C7A-A9A5-9BEFAF0AE906")]
    public class InEventRealMatrix : InputBase<MatrixBase<double>>, IValueable<MatrixBase<double>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventRealMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InEventRealMatrix(CSimBase p) : base(p) { }
        public InEventRealMatrix(CSimBase p, MatrixBase<double> v) : base(p) => Value = v;
        public override void SetStartValue(MatrixBase<double> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventRealMatrix ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventDoubleMatix
    #region eventStringMatrix

    /// <summary>
    /// Default output type for event based string matrix
    /// </summary>
    [StorableType("BC25E3C8-D895-447A-8B91-EB9003640A3A")]
    public class OutEventStringMatrix : OutputBase<MatrixBase<string>>, IValueable<MatrixBase<string>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventStringMatrix(StorableConstructorFlag flag) : base(flag) { }
        public OutEventStringMatrix(CSimBase p) : base(p) { }

        public OutEventStringMatrix(CSimBase p, MatrixBase<string> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(MatrixBase<string> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventStringMatrix inEventBoolMatrix:
                    inEventBoolMatrix.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventBoolMatrix.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based string matrix
    /// </summary>
    [StorableType("20BF3A83-42C8-4893-8779-8368B32E941E")]
    public class InEventStringMatrix : InputBase<MatrixBase<string>>, IValueable<MatrixBase<string>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventStringMatrix(StorableConstructorFlag flag) : base(flag) { }
        public InEventStringMatrix(CSimBase p) : base(p) { }
        public InEventStringMatrix(CSimBase p, MatrixBase<string> v) : base(p) => Value = v;
        public override void SetStartValue(MatrixBase<string> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventStringMatrix outEventStringMatrix:
                    outEventStringMatrix.ConnectedInput = this;
                    ConnectedOutput = outEventStringMatrix;
                    Value = outEventStringMatrix.Value;
                    break;
            }
        }
    }
    #endregion eventIntMatrix



    #region eventObjectArray

    /// <summary>
    /// Default output type for event based object array
    /// </summary>
    [StorableType("AFF8E49E-236D-4A83-9E91-200E210D8F33")]
    public class OutEventObjectArray : OutputBase<ArrayBase<object>>, IValueable<ArrayBase<object>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventObjectArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventObjectArray(CSimBase p) : base(p) { }
        public OutEventObjectArray(CSimBase p, ArrayBase<object> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<object> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventObjectArray inEventRealArray:
                    inEventRealArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventRealArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based object array
    /// </summary>
    [StorableType("7CD7E470-08D4-432C-8D93-7E38C37DEE96")]
    public class InEventObjectArray : InputBase<ArrayBase<object>>, IValueable<ArrayBase<object>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventObjectArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventObjectArray(CSimBase p) : base(p) { }
        public InEventObjectArray(CSimBase p, ArrayBase<object> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<object> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventObjectArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventObjectArray
    #region eventDoubleArray
    /// <summary>
    /// Default output type for event based double array
    /// </summary>
    [StorableType("4304C694-C30D-418A-A396-447E28D9BF53")]
    public class OutEventRealArray : OutputBase<ArrayBase<double>>, IValueable<ArrayBase<double>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventRealArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventRealArray(CSimBase p) : base(p) { }

        public OutEventRealArray(CSimBase p, ArrayBase<double> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<double> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventRealArray inEventRealArray:
                    inEventRealArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventRealArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based double array
    /// </summary>
    [StorableType("B2E8458F-1C28-4BC0-B176-F267AB4E4CAD")]
    public class InEventRealArray : InputBase<ArrayBase<double>>, IValueable<ArrayBase<double>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventRealArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventRealArray(CSimBase p) : base(p) { }
        public InEventRealArray(CSimBase p, ArrayBase<double> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<double> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventRealArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventDoubleArray
    #region eventIntArray
    /// <summary>
    /// Default output type for event based int array
    /// </summary>
    [StorableType("74E01411-6D53-4AF1-A7C6-2420897EB78B")]
    public class OutEventIntArray : OutputBase<ArrayBase<int>>, IValueable<ArrayBase<int>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventIntArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventIntArray(CSimBase p) : base(p) { }
        public OutEventIntArray(CSimBase p, ArrayBase<int> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<int> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventIntArray inEventIntArray:
                    inEventIntArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventIntArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based int array
    /// </summary>
    [StorableType("AD0BC74E-4994-4578-ABE0-7A6A31D5AFAF")]
    public class InEventIntArray : InputBase<ArrayBase<int>>, IValueable<ArrayBase<int>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventIntArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventIntArray(CSimBase p) : base(p) { }
        public InEventIntArray(CSimBase p, ArrayBase<int> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<int> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventIntArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventIntArray
    #region eventBoolArray
    /// <summary>
    /// Default output type for event based bool array
    /// </summary>
    [StorableType("43088279-C63B-4105-BA1D-0D41D3320296")]
    public class OutEventBoolArray : OutputBase<ArrayBase<bool>>, IValueable<ArrayBase<bool>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventBoolArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventBoolArray(CSimBase p) : base(p) { }

        public OutEventBoolArray(CSimBase p, ArrayBase<bool> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<bool> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventBoolArray inEventBoolArray:
                    inEventBoolArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventBoolArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based bool array
    /// </summary>
    [StorableType("746DA467-C3A6-4357-BCF4-F653864A3D74")]
    public class InEventBoolArray : InputBase<ArrayBase<bool>>, IValueable<ArrayBase<bool>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventBoolArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventBoolArray(CSimBase p) : base(p) { }
        public InEventBoolArray(CSimBase p, ArrayBase<bool> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<bool> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventBoolArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventBoolArray
    #region eventStringArray
    /// <summary>
    /// Default output type for event based string array
    /// </summary>
    [StorableType("A0BD16F0-8793-4922-9824-3F172BF6960B")]
    public class OutEventStringArray : OutputBase<ArrayBase<string>>, IValueable<ArrayBase<string>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventStringArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventStringArray(CSimBase p) : base(p) { }

        public OutEventStringArray(CSimBase p, ArrayBase<string> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<string> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventStringArray inEventStringArray:
                    inEventStringArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventStringArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    /// <summary>
    /// Default input type for event based string array
    /// </summary>
    [StorableType("4B4CF753-9D36-41A4-A8A1-B5532D485250")]
    public class InEventStringArray : InputBase<ArrayBase<string>>, IValueable<ArrayBase<string>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventStringArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventStringArray(CSimBase p) : base(p) { }
        public InEventStringArray(CSimBase p, ArrayBase<string> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<string> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventStringArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion eventStringArray

    #region eventItem
    [StorableType("3D4EA52A-AF3A-4BD2-8B33-749B5ADFCCE5")]
    public class OutEventItem : OutputBase<Item>, IValueable<Item>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventItem(StorableConstructorFlag flag) : base(flag) { }
        public OutEventItem(CSimBase p) : base(p) { }
        public OutEventItem(CSimBase p, Item v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(Item value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case IValueable<Item> inItem when inItem is InputBase<Item>:
                    inItem.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    if (inItem is InEventItem inEventItem)
                        inEventItem.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }
    [StorableType("2917CC02-8500-440E-9CBB-6070CC89B87A")]
    public class InEventItem : InputBase<Item>, IValueable<Item>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventItem(StorableConstructorFlag flag) : base(flag) { }
        public InEventItem(CSimBase p) : base(p) { }
        public InEventItem(CSimBase p, Item v) : base(p) => Value = v;
        public override void SetStartValue(Item v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutputBase<Item> outItem when outItem is IValueable<Item>:
                    outItem.ConnectedInput = this;
                    ConnectedOutput = outItem;
                    Value = outItem.Value;
                    break;
            }
        }
    }

    /// <summary>
    /// Default output type for event based object array
    /// </summary>
    [StorableType("AFF8E49E-236D-4A83-9E91-200E210D8F35")]
    public class OutEventItemArray : OutputBase<ArrayBase<Item>>, IValueable<ArrayBase<Item>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public OutEventItemArray(StorableConstructorFlag flag) : base(flag) { }
        public OutEventItemArray(CSimBase p) : base(p) { }
        public OutEventItemArray(CSimBase p, ArrayBase<Item> v) : base(p) => Value = v;
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Set(ArrayBase<Item> value)
        {
            Value = value;
            LastT = Parent.Settings.Environment.SimulationTime;

            if (ConnectedInput == null) return;
            switch (ConnectedInput)
            {
                case InEventItemArray inEventItemArray:
                    inEventItemArray.Value = value;
                    Parent.Settings.SimulationObjects.EventQueue.Add(ConnectedInput.Parent, Parent.Settings.Environment.SimulationTime);
                    inEventItemArray.LastT = Parent.Settings.Environment.SimulationTime;
                    break;
            }
        }
    }

    /// <summary>
    /// Default input type for event based object array
    /// </summary>
    [StorableType("7CD7E470-08D4-432C-8D93-7E38C37DEF96")]
    public class InEventItemArray : InputBase<ArrayBase<Item>>, IValueable<ArrayBase<Item>>
    {
        [Storable]
        public long LastT { get; set; } = -1;
        [StorableConstructor]
        public InEventItemArray(StorableConstructorFlag flag) : base(flag) { }
        public InEventItemArray(CSimBase p) : base(p) { }
        public InEventItemArray(CSimBase p, ArrayBase<Item> v) : base(p) => Value = v;
        public override void SetStartValue(ArrayBase<Item> v)
        {
            Value = v;
            LastT = -1;
        }
        public bool Change() => Parent.Settings.Environment.SimulationTime == LastT;
        public override void Connect(InputOutputBase output)
        {
            switch (output)
            {
                case OutEventItemArray ob:
                    ob.ConnectedInput = this;
                    ConnectedOutput = ob;
                    Value = ob.Value;
                    break;
            }
        }
    }
    #endregion
    #endregion Input and ouput event types
}
