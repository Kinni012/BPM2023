using System.Collections.Generic;
using System.Linq;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessTree
{
    public sealed class Operators
    {
        public static Random.MersenneTwister Twister { get; set; }
        private static Operators instance = null;
        private static readonly object padlock = new object();

        public Dictionary<string, string> OperatorList = new Dictionary<string, string>();

        private Operators() { }

        public static Operators Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Operators();
                        instance.OperatorList.Add("Sequence", "\\>");
                        instance.OperatorList.Add("Xor", "x");
                        //instance.OperatorList.Add("Parallel", "+");
                        //instance.OperatorList.Add("Loop", "l");
                        instance.OperatorList.Add("None", " ");
                        if (Operators.Twister == null)
                            Operators.Twister = new Random.MersenneTwister();
                    }
                    return instance;
                }
            }
        }

        public static string RandomOperator()
        {
            //List<string> result = new List<string>() { "Sequence", "Xor", "Parallel" };
            List<string> result = new List<string>() { "Sequence", "Xor" };
            if (Operators.Twister == null)
                Operators.Twister = new Random.MersenneTwister();

            return result.OrderBy(x => Twister.Next()).FirstOrDefault();
        }

        public static List<string> AllOperators()
        {
            //List<string> result = new List<string>() { "Sequence", "Xor", "Parallel" };
            List<string> result = new List<string>() { "Sequence", "Xor"};
            return result;
        }

    }
}
