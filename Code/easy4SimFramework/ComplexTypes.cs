using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HEAL.Attic;

// File that contains non abstract class implementation, that should be part of the Framework.
namespace Easy4SimFramework
{
    [StorableType("D6F822F6-2760-468E-A708-88D85CBD0FF4")]
    public class ItemProperty<T> : ItemProperty, IValueable<T>
    {
        [Storable]
        public T Value { get; set; }
        public override Type CurrentType => typeof(T);
        [StorableConstructor]
        public ItemProperty(StorableConstructorFlag flag): base(flag){}

        public ItemProperty(string name, T value) : base(name)
        {
            Value = value;
        }
        public override string ToString()
        {
            return "Name: " + Name + " Value: " + Value + " Type: " + CurrentType.Name;
        }

        public override object Clone()
        {
            return new ItemProperty<T>(Name, Value);
        }
    }

    [StorableType("92E3C6AC-B34F-4F2E-BA0C-624916305C0B")]
    public class Item : IValueable<List<ItemProperty>>, INonSimulationObject
    {
        [Storable]
        public Guid CurrentGuid { get; set; }
        [Storable]
        public List<ItemProperty> Value { get; set; }
        [StorableConstructor]
        public Item(StorableConstructorFlag flag) {}
        public Item() => InitializeParameters();
        private void InitializeParameters()
        {
            Value = new List<ItemProperty>();
            CurrentGuid = Guid.NewGuid();
        }
        public bool ContainsKey(string s) => Value.Any(x => x.Name == s);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"Item[{CurrentGuid}]");
            if (Value?.Count == 0) return sb.ToString();
            sb.Append(" Values: ( ");
            if (Value != null)
                foreach (ItemProperty prop in Value)
                    sb.Append("( " + prop + " ) ");
            sb.Append(")");
            return sb.ToString();
        }
        public object Clone()
        {
            Item item = new Item();
            foreach (ItemProperty property in Value)
                item.Value.Add((ItemProperty)property.Clone());
            return item;
        }
    }
}
