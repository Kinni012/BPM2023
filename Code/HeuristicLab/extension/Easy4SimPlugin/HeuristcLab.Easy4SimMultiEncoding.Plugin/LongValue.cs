using System;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HEAL.Attic;
// ReSharper disable UnusedMember.Global

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    [Item("LongValue", "Represents an long value.")]
    [StorableType("EA403E48-6398-4359-87AB-FA2999437662")]
    public class LongValue : ValueTypeValue<long>, IComparable, IStringConvertibleValue
    {
        [Storable]
        public override long Value { get; set; }

        [StorableConstructor]
        public LongValue(StorableConstructorFlag deserializing) : base(deserializing) { }

        public LongValue(LongValue original, Cloner cloner) : base(original, cloner)
        {
            Value = original.Value;
        }
        public LongValue() { }
        public LongValue(long value) : base(value) { }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            LongValue result = new LongValue(this, cloner);
            result.Value = Value;
            return result;
        }


        public virtual int CompareTo(object obj)
        {
            if (obj is LongValue other)
                return Value.CompareTo(other.Value);
            return Value.CompareTo(obj);
        }

        protected virtual bool Validate(string value, out string errorMessage)
        {
            bool valid = long.TryParse(value, out long val);
            errorMessage = string.Empty;
            if (!valid)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Invalid Value (Valid Value Format: \"");
                sb.Append(FormatPatterns.GetIntFormatPattern());
                sb.Append("\")");
                errorMessage = sb.ToString();
            }
            return valid;
        }
        protected virtual string GetValue()
        {
            return Value.ToString();
        }
        protected virtual bool SetValue(string value)
        {
            if (!long.TryParse(value, out long val)) return false;
            Value = val;
            return true;
        }

        #region IStringConvertibleValue Members
        bool IStringConvertibleValue.Validate(string value, out string errorMessage)
        {
            return Validate(value, out errorMessage);
        }
        string IStringConvertibleValue.GetValue()
        {
            return GetValue();
        }
        bool IStringConvertibleValue.SetValue(string value)
        {
            return SetValue(value);
        }
        #endregion
    }
}
