using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperOptimization
{
    /// <summary>
    /// This class stores information about all combinations which should be run
    /// E.g. 2 Selectors, 2 Mutators and 2 Crossovers will lead to 8 combinations
    /// </summary>
    public class UserArguments
    {
        public List<string> Arguments { get; set; }
        public List<string> ValidTypes { get; set; }

        /// <summary>
        /// Stopping criteria can be time with milliseconds or generations with a specified amount of generations
        /// </summary>
        public (string, int) StoppingCriteria { get; set; }
        /// <summary>
        /// Which data sets should be solved
        /// </summary>
        public List<string> ValidDataSets { get; set; }
        /// <summary>
        /// Which selectors should be tested
        /// </summary>
        public List<int> ValidSelectors { get; set; }

        /// <summary>
        /// Which crossovers should be tested
        /// </summary>
        public List<int> ValidCrossovers { get; set; }

        /// <summary>
        /// Which mutators should be tested
        /// </summary>
        public List<int> ValidMutators { get; set; }
        /// <summary>
        /// Amount of repetitions that are performed with one specific setting
        /// </summary>
        public int Repetitions { get; set; }

        /// <summary>
        /// Influence of makespan on the objective function
        /// </summary>
        public double TimeFactor { get; set; }
        /// <summary>
        /// Influence of costs on the objective function
        /// </summary>
        public double CostFactor { get; set; }
        /// <summary>
        /// Use a variable neighborhood search during the optimization
        /// </summary>
        public bool ApplyVns { get; set; }
        /// <summary>
        /// Which neighborhood operator should be used 
        /// </summary>
        public int VnsNeighborhoodOperator{ get; set; }
        /// <summary>
        /// Normalized objective function with bound from the limits file
        /// </summary>
        public bool UseNormalizedValue { get; set; }
        /// <summary>
        /// Amount of cobots that can be used by the optimization
        /// </summary>
        public int AmountOfCobots { get; set; }

        /// <summary>
        /// Minimize or maximize the objective value
        /// </summary>
        public int MinimizeOrMaximize { get; set; }
        /// <summary>
        /// Apply the vns if a solution is within a given range to the best solution found so far
        /// </summary>
        public double VnsPercentage { get; set; }
        /// <summary>
        /// Amount of intelligent changes 
        /// </summary>
        public double VnsSecondaryOperator { get; set; }
        /// <summary>
        /// Do deterministic or stochastic simulation
        /// </summary>
        public bool DeterministicOrStochastic { get; set; } //true == stochastic, false == deterministic

        public UserArguments()
        {
            Arguments = new List<string>();
            ValidTypes = new List<string>();
            StoppingCriteria = ("generations", 1);
            ValidDataSets = new List<string>();
            ValidSelectors = new List<int>();
            ValidCrossovers = new List<int>();
            ValidMutators = new List<int>();
            Repetitions = 0;
            TimeFactor = 0;
            CostFactor = 0;
            ApplyVns = false;
            AmountOfCobots = 0;
            MinimizeOrMaximize = 0;
            VnsPercentage = 0;
            VnsSecondaryOperator = 0;
            UseNormalizedValue = false;
            DeterministicOrStochastic = false;
        }


        /// <summary>
        /// Get all combinations based on the current user input
        /// </summary>
        /// <returns></returns>
        public List<Combination> GetCombinations()
        {
            List<Combination> result = new List<Combination>();

            for (int i = 0; i < Repetitions; i++)
                foreach (string validType in ValidTypes)
                    foreach (string validDataSet in ValidDataSets)
                        foreach (int selector in ValidSelectors)
                            foreach (int crossover in ValidCrossovers)
                                foreach (int mutator in ValidMutators)
                                    result.Add(new Combination(validType, validDataSet, selector, crossover, mutator));

            return result;
        }

        public override string ToString()
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append($"Type of the simulation: ");
            foreach (string type in ValidTypes)
                sb.Append(type + " ");
            sb.Append(Environment.NewLine);



            sb.Append($"Datasets: ");
            foreach (string dataSet in ValidDataSets)
                sb.Append(dataSet + " ");
            sb.Append(Environment.NewLine);

            sb.Append($"Stopping criteria: {StoppingCriteria.Item1} => {StoppingCriteria.Item2 / 60000} minutes{Environment.NewLine}");
            sb.Append($"Time factor in the optimization: {TimeFactor}{Environment.NewLine}");
            sb.Append($"Cost factor in the optimization: {CostFactor}{Environment.NewLine}");
            sb.Append($"Variable neighborhood search: {ApplyVns}{Environment.NewLine}");
            sb.Append($"Vns neighborhood: {VnsNeighborhoodOperator}{Environment.NewLine}");
            sb.Append($"Vns percentage: {VnsPercentage}{Environment.NewLine}");
            sb.Append($"Vns secondary operator percentage: {VnsSecondaryOperator}{Environment.NewLine}");
            sb.Append($"Amount of Cobots: {AmountOfCobots}{Environment.NewLine}");
            sb.Append($"Minimize or maximize: {MinimizeOrMaximize}{Environment.NewLine}");
            sb.Append($"Use normalized value: {UseNormalizedValue}{Environment.NewLine}");
            sb.Append($"Deterministic or stochastic: {DeterministicOrStochastic}{Environment.NewLine}");

            return sb.ToString();
        }
    }
}
