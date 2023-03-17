using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Easy4SimFramework;
using FjspOptimization.Data;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Data;
using HeuristicLab.Easy4SimMultiEncoding.Plugin;
using HeuristicLab.Optimization;
using HeuristicLab.Random;
using HeuristicLab.Selection;
using HeuristicLab.SequentialEngine;

namespace FjspOptimization
{
    /// <summary>
    /// Start program for the FJSSP evaluation.
    /// Based on user input a genetic algorithm in HL is created.
    /// This algorithm is filled with a custom Easy4SimFjsspEncoding.
    /// </summary>
    class Program
    {

        public static string[] IntCrossovers = { "RoundedUniformSomePositionsArithmeticCrossover", "SinglePointCrossover", "RoundedAverageCrossover", "RoundedHeuristicCrossover", "MultiIntegerVectorCrossover" };
        public static string[] IntMutator = { "RoundedNormalAllPositionsManipulator", "UniformOnePositionManipulator", "SelfAdaptiveRoundedNormalAllPositionsManipulator", "UniformSomePositionsManipulator" };
        public static string[] IntSelectors = { "ProportionalSelector", "TournamentSelector", "RandomSelector", "BestSelector", "WorstSelector" };
        

        static void Main(string[] args)
        {
            try
            {
                List<string> arguments = args.ToList();
                // ================ Get user inputs ===============================================================
                ConsoleArguments consoleArguments = HelperFunctions.GetArgumentsFromUserOrConsole(arguments);

                // ================= Create genetic algorithm based on user input ==================================
                GeneticAlgorithm algorithm = InitializeAlgorithm(consoleArguments);

                Easy4SimFjsspEncoding.Algorithm = algorithm;

                

                // ================= Set end time ==================================================================
                DateTime endTime = DateTime.Now;
                Console.WriteLine("Start time: " + endTime.ToLocalTime());
                endTime = endTime.AddMilliseconds(consoleArguments.Criteria);
                Console.WriteLine("End time: " + endTime.ToLocalTime());
                Easy4SimFjsspEncoding.EndTime = endTime;
                // =============== Start algorithm =================================================================

                algorithm.Prepare(true);
                if (consoleArguments.StoppingCriteria == "generations")
                    algorithm.Start();
                if (consoleArguments.StoppingCriteria == "time")
                {
                    try
                    {
                        Console.WriteLine($"Stopping criterium time: {consoleArguments.Criteria / 60000} minutes");
                        var ct = new CancellationTokenSource(consoleArguments.Criteria).Token;
                        algorithm.Start(ct);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                        throw;
                    }
                }
                if(algorithm.Problem is Easy4SimFjsspEncoding problem)
                {
                    Console.WriteLine($"  Best quality: {consoleArguments.DataSet} => {problem.BestQuality}");
                    // ============== Write results ===================================================================
                    OutputResults(consoleArguments, algorithm, problem);
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                Console.Read();
                throw;
            }
        }
        /// <summary>
        /// Print all results from the genetic algorithm run to a output file with a generated name
        /// </summary>
        /// <param name="consoleArguments"></param>
        /// <param name="algorithm"></param>
        /// <param name="problem"></param>
        private static void OutputResults(ConsoleArguments consoleArguments, 
            GeneticAlgorithm algorithm,
            Easy4SimFjsspEncoding problem)
        {
            string outputPath = Path.Combine(new[] { Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..", "Results", "ColectedRuns" });

            bool exists = Directory.Exists(outputPath);
            if (!exists)
                Directory.CreateDirectory(outputPath);

            string fileName = $"{Guid.NewGuid()}.csv";

            StringBuilder sb = new StringBuilder();
            string separator = ",";
            sb.Append($"Data set: {separator}{consoleArguments.DataSet}{System.Environment.NewLine}");
            sb.Append($"Time in minutes: {separator}{consoleArguments.Criteria / 60000}{System.Environment.NewLine}");
            sb.Append($"Vns: {separator}{consoleArguments.ApplyVns}{System.Environment.NewLine}");
            sb.Append($"Vns percentage: {separator}{consoleArguments.VnsPercentage}{System.Environment.NewLine}");
            sb.Append($"Amount of cobots: {separator}{consoleArguments.AmountOfCobots}{System.Environment.NewLine}");
            sb.Append($"Vns neighborhood: {separator}{problem.Solver.SimulationStatistics.VnsNeighborhood}{System.Environment.NewLine}");
            sb.Append($"Makespan: {separator}{problem.BestQuality.Value}{System.Environment.NewLine}");
            sb.Append($"Minimum amount of evaluated neighbors: {separator}{Easy4SimFjsspEncoding.MinimumVnsNeighborhoodSize}{System.Environment.NewLine}");
            sb.Append($"Average evaluated vns solutions per vns application:{separator}{Math.Round(System.Convert.ToDouble(Easy4SimFjsspEncoding.EvaluatedVnsSolutions) / Convert.ToDouble(Easy4SimFjsspEncoding.VnsRuns), 2)}{System.Environment.NewLine}");
            sb.Append($"Average improvements per vns application:{separator}{Math.Round(System.Convert.ToDouble(Easy4SimFjsspEncoding.VnsImprovementsFound) / Convert.ToDouble(Easy4SimFjsspEncoding.VnsRuns), 2)}{System.Environment.NewLine}");
            sb.Append($"Vns applications:{separator}{Easy4SimFjsspEncoding.VnsRuns}{System.Environment.NewLine}");
            sb.Append($"GA generation size:{separator}{algorithm.PopulationSize.Value}{System.Environment.NewLine}");
            sb.Append($"GA evaluated solutions:{separator}{algorithm.Results["EvaluatedSolutions"]}{System.Environment.NewLine}");
            sb.Append($"Best solution:{System.Environment.NewLine}{problem.ReplayInformation}{System.Environment.NewLine}");

            if (!File.Exists(Path.Combine(new[] { outputPath, fileName })))
                File.WriteAllText(Path.Combine(new[] { outputPath, fileName }), sb.ToString());


            Console.WriteLine($"Finished, wrote results to: {fileName}");
            Console.ReadLine();
        }

        /// <summary>
        /// Initialize the problem based on all user input
        /// </summary>
        /// <param name="consoleArguments"></param>
        /// <returns></returns>
        private static Easy4SimFjsspEncoding InitializeProblem(ConsoleArguments consoleArguments)
        {

            MersenneTwister twister = new MersenneTwister();
            Easy4SimFjsspEncoding problem = new Easy4SimFjsspEncoding(maximization: consoleArguments.Maximization, amountOfCobots: consoleArguments.AmountOfCobots, twister: twister);

            problem.TimeFactor = consoleArguments.TimeFactor;
            problem.CostFactor = consoleArguments.CostFactor;
            problem.Easy4SimEvaluationScript.Value = consoleArguments.DataSet;


            problem.SolverSettings.Statistics.DataSet = consoleArguments.DataSet;


            Easy4SimFjsspEncoding.UseNormalizedValue = consoleArguments.UseNormalizedValue;
            Easy4SimFjsspEncoding.BestSolutionSoFarGa = double.MaxValue;
            Easy4SimFjsspEncoding.ApplyVNS = consoleArguments.ApplyVns;
            Easy4SimFjsspEncoding.VnsPercentage = consoleArguments.VnsPercentage;
            Easy4SimFjsspEncoding.VnsNeighborhood = consoleArguments.VnsNeighborhood;

            return problem;

        }

        /// <summary>
        /// Initialize the genetic algorithm based on user input
        /// </summary>
        /// <param name="consoleArguments"></param>
        /// <returns></returns>
        private static GeneticAlgorithm InitializeAlgorithm(ConsoleArguments consoleArguments)
        {
            GeneticAlgorithm algorithm = new GeneticAlgorithm();
            if (consoleArguments.StoppingCriteria == "generations")
                algorithm.MaximumGenerations.Value = consoleArguments.Criteria;
            else
                algorithm.MaximumGenerations.Value = 10000000; //high amount of generations, stop after time
            
            // ================= Create FJSP based on user input ===============================================
            Easy4SimFjsspEncoding problem = InitializeProblem(consoleArguments);
            algorithm.Problem = problem;
            algorithm.PopulationSize.Value = 100;
            algorithm.ReevaluteElites = true;
            algorithm.Elites.Value = 3;
            
            //if (consoleArguments.VnsNeighborhood >= 6)
            //{
            //    consoleArguments.VnsNeighborhood = consoleArguments.VnsNeighborhood % 6;
            //    algorithm.PopulationSize = new IntValue(1000);
            //}

            Console.WriteLine("============================================================================================");
            string selector = IntSelectors[0];
            string crossover = IntCrossovers[0];
            string mutator = IntMutator[0];

            ISelector s = algorithm.SelectorParameter.ValidValues.FirstOrDefault(x => x.Name.Contains(selector));
            if (s is ProportionalSelector proportionalSelector)
            {
                proportionalSelector.MaximizationParameter.Value = new BoolValue(consoleArguments.Maximization);
            }

            ICrossover c = algorithm.CrossoverParameter.ValidValues.FirstOrDefault(x => x.Name.Contains(crossover));
            IManipulator m = algorithm.MutatorParameter.ValidValues.FirstOrDefault(x => x.Name.Contains(mutator));

            algorithm.CrossoverParameter.Value = c;
            algorithm.MutatorParameter.Value = m;
            algorithm.SelectorParameter.Value = s;
            //algorithm.SelectorParameter.Value = problem.Encoding.Operators.OfType<ISelector>().FirstOrDefault(x => x.Name.Contains(selector));

            Console.WriteLine("Selector:" + selector);
            Console.WriteLine("Crossover:" + crossover);
            Console.WriteLine("Mutator:" + mutator);

            Console.WriteLine("============================================================================================");

            SequentialEngine engine = new SequentialEngine();
            algorithm.Engine = engine;
            return algorithm;
        }
    }
}
