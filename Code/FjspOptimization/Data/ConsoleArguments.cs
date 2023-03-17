namespace FjspOptimization.Data
{
    /// <summary>
    /// Class that stores information about one configuration to run the flexible job shop scheduling problem optimization
    /// </summary>
    public class ConsoleArguments
    {
        /// <summary>
        /// Data set that is optimized
        /// </summary>
        public string DataSet { get; internal set; }
        /// <summary>
        /// Stop after generations or time
        /// </summary>
        public string StoppingCriteria { get; internal set; }
        /// <summary>
        /// Numeric value of the stopping criteria
        /// </summary>
        public int Criteria { get; internal set; }
        /// <summary>
        /// How many repetitions should be evaluated
        /// </summary>
        public int Repetitions { get; internal set; }
        /// <summary>
        /// Impact of the makespan on the objective function
        /// </summary>
        public double TimeFactor { get; internal set; }
        /// <summary>
        /// Impact of the costs on the objective function
        /// </summary>
        public double CostFactor { get; internal set; }
        /// <summary>
        /// Should a variable neighborhood search be applied?
        /// </summary>
        public bool ApplyVns { get; internal set; }
        /// <summary>
        /// Should the metaheuristic minimize or maximize the objective function?
        /// </summary>
        public int MinimizeOrMaximize { get; internal set; }
        /// <summary>
        /// Normalize the results?
        /// </summary>
        public bool UseNormalizedValue { get; internal set; }
        /// <summary>
        /// Amount of cobots that can be used in the optimization
        /// </summary>
        public double AmountOfCobots { get; internal set; }

        /// <summary>
        /// Convert the integer that is read in for the maximization to a bool value
        /// </summary>
        public bool Maximization => MinimizeOrMaximize == 1;

        /// <summary>
        /// How close should a solution be to the best to apply a variable neighborhood search?
        /// </summary>
        public double VnsPercentage { get; internal set; }
        /// <summary>
        /// Which neighborhood operator should be used?
        /// </summary>
        public int VnsNeighborhood { get; set; }
    }
}
