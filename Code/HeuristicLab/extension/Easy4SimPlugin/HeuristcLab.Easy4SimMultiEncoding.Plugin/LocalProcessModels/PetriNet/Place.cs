using System;
using System.Text;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.PetriNet
{
    public class Place : ICloneable
    {
        /// <summary>
        /// Unique id that is set when place is constructed
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Name of the place
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Amount of tokens currently in the place
        /// </summary>
        public int Tokens { get; set; }


        public Place()
        {
            Id = Guid.NewGuid();
        }
        public Place(string name, int tokens)
        {
            Name = name;
            Tokens = tokens;
            Id = Guid.NewGuid();
        }
        public Place(string name, int tokens, Guid id)
        {
            Name = name;
            Tokens = tokens;
            Id = id;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            if (Tokens > 0)
            {
                sb.Append($" ({Tokens} tokens)");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a cloned version of the place
        /// </summary>
        /// <returns></returns>
        public object Clone() => new Place(Name, Tokens, Id);
    }
}
