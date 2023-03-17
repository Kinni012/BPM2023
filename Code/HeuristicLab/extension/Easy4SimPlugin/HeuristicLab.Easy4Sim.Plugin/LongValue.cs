using System;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HEAL.Attic;

namespace HeuristicLab.Easy4Sim.Plugin
{
    [Item("LongValue", "Represents an long value.")]
    [StorableType]
    public class LongValue : ValueTypeValue<long>, IComparable, IStringConvertibleValue
    {
        [StorableConstructor]
        protected LongValue(StorableConstructorFlag deserializing) : base(deserializing) { }
        protected LongValue(LongValue original, Cloner cloner)
          : base(original, cloner)
        {
        }
        public LongValue() : base() { }
        public LongValue(long value) : base(value) { }

        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new LongValue(this, cloner);
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is LongValue other)
                return Value.CompareTo(other.Value);
            else
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
            if (long.TryParse(value, out long val))
            {
                Value = val;
                return true;
            }
            else
            {
                return false;
            }
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
