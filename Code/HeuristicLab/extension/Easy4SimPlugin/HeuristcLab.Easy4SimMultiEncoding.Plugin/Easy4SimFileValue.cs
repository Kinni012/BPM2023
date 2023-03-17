using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HEAL.Attic;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [Item("Easy4SimFile", "Represents the path to a Easy4Sim file.")]
    [StorableType("268BD35B-CEC4-4AA4-B76B-C8DF7042C485")]
    public class Easy4SimFileValue : FileValue
    {
        [StorableConstructor]
        public Easy4SimFileValue(StorableConstructorFlag deserializing) : base(deserializing) { }
        public Easy4SimFileValue(Easy4SimFileValue original, Cloner c) : base(original, c) => Value = original.Value;

        public override IDeepCloneable Clone(Cloner c)
        {
            Easy4SimFileValue result = new Easy4SimFileValue(this, c);
            result.Value = Value;
            return result;
        } 
        public Easy4SimFileValue() { }

        public Easy4SimFileValue(string value)
        {
            Value = value;
        }
    }
}
