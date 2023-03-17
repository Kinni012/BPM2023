using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.PetriNet
{
    public class Transition : ICloneable
    {
        /// <summary>
        /// Starting place of the transition
        /// </summary>
        public List<Place> From { get; set; }

        /// <summary>
        /// Destination of the transition
        /// </summary>
        public List<Place> To { get; set; }


        public List<string> ToNonSplit(List<Transition> transitions)
        {

            List<Place> result = new List<Place>();
            List<Place> spliters = new List<Place>();
            foreach (Place place in To)
            {
                if (place.Name.Contains("Split"))
                    spliters.Add(place);
                else
                    result.Add(place);
            }

            while (spliters.Count > 0)
            {
                List<Place> nextPlaces = spliters;
                spliters = new List<Place>();
                foreach (Place p in nextPlaces)
                {
                    List<Transition> transitionsFromNextPlace =
                        transitions.Where(x => x.From.Select(y => y.Name).Contains(p.Name)).ToList();

                    foreach (Transition transition in transitionsFromNextPlace)
                    {
                        foreach (Place place in transition.To)
                        {
                            if (place.Name.Contains("Split"))
                                spliters.Add(place);
                            else
                                result.Add(place);
                        }
                    }
                }
            }

            return result.Select(x => x.Name).ToList();
        }


        /// <summary>
        /// Name of the transition, for special transitions, e.g. parallel split
        /// </summary>
        public string Name { get; set; }

        public Transition()
        {
            From = new List<Place>();
            To = new List<Place>();
        }
        public Transition(Place from, Place to)
        {
            From = new List<Place>();
            From.Add(from);
            To = new List<Place>();
            To.Add(to);
        }

        public Transition(List<Place> from = null, List<Place> to = null)
        {
            From = from;
            To = to;
            if (From == null)
                From = new List<Place>();
            if (To == null)
                To = new List<Place>();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Place p in From)
            {
                sb.Append(p.Name + " ");
            }

            sb.Append("=> ");

            foreach (Place p in To)
            {
                sb.Append(p.Name + " ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// A transition can fire, if each from place has a token
        /// </summary>
        public bool CanFire
        {
            get
            {
                foreach (Place p in From)
                    if (p.Tokens == 0)
                        return false;

                return true;
            }
        }
        /// <summary>
        /// Remove a token from each input place
        /// Add a token to each to place
        /// </summary>
        public void Fire()
        {
            foreach (Place fromPlace in From)
                fromPlace.Tokens -= 1;

            foreach (Place toPlace in To)
                toPlace.Tokens += 1;
        }
        public object Clone()
        {
            Transition result = new Transition();
            foreach (Place p in From)
                result.From.Add((Place)p.Clone());

            foreach (Place p in To)
                result.To.Add((Place)p.Clone());

            result.Name = Name;
            return result;
        }

    }

}
