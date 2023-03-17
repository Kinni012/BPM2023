using HeuristicLab.Common;
using HEAL.Attic;
using HeuristicLab.Core;
using HeuristicLab.Data;

namespace HeuristicLab.Easy4Sim.Plugin
{
    [Item("Easy4SimFile", "Represents the path to a Easy4Sim file.")]
    [StorableType]
    public class Easy4SimFileValue : FileValue
    {
        protected Easy4SimFileValue(StorableConstructorFlag deserializing) : base(deserializing) { }
        protected Easy4SimFileValue(Easy4SimFileValue original, Cloner cloner)
            : base(original, cloner)
        {
        }
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimFileValue(this, cloner);
        }
        public Easy4SimFileValue()
            : base()
        {
        }
    }
}
