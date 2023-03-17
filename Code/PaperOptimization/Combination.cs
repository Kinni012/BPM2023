using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperOptimization
{
    public class Combination
    {
        public string ValidType { get; set; }
        public string ValidDataSet { get; set; }
        public int Selector { get; set; }
        public int Crossover { get; set; }
        public int Mutator { get; set; }


        public Combination(string validType, string validDataSet, int selector, int crossover, int mutator)
        {
            ValidType = validType;
            ValidDataSet = validDataSet;
            Selector = selector;
            Mutator = mutator;
            Crossover = crossover;
        }
    }
}
