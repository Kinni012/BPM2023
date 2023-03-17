using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperOptimization
{
    public class UserArgument
    {

        public string DataSet { get; set; }
        public string Type { get; set; }
        public int Selector { get; set; }
        public int Crossover { get; set; }
        public int Mutator { get; set; }

        protected bool Equals(UserArgument other)
        {
            return DataSet == other.DataSet && Type == other.Type && Selector == other.Selector && Crossover == other.Crossover && Mutator == other.Mutator;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserArgument)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DataSet.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                hashCode = (hashCode * 397) ^ Selector;
                hashCode = (hashCode * 397) ^ Crossover;
                hashCode = (hashCode * 397) ^ Mutator;
                return hashCode;
            }
        }

        public static bool operator ==(UserArgument left, UserArgument right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserArgument left, UserArgument right)
        {
            return !Equals(left, right);
        }

        public UserArgument() { }
        public UserArgument(string dataSet, string type, int selector, int crossover, int mutator)
        {
            DataSet = dataSet;
            Type = type;
            Selector = selector;
            Crossover = crossover;
            Mutator = mutator;
        }

        public UserArgument(Combination combination)
        {
            DataSet = combination.ValidDataSet;
            Type = combination.ValidType;
            Selector = combination.Selector;
            Crossover = combination.Crossover;
            Mutator = combination.Mutator;
        }
    }
}
