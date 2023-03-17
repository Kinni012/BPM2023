using Easy4SimFramework;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Easy4SimMultiEncoding.Plugin;
using HeuristicLab.Optimization;
using HeuristicLab.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PaperOptimization
{
    public static class HelperFunctions
    {
        /// <summary>
        /// List of crossover operators available for the real encoding
        /// </summary>
        public static string[] RealCrossovers = { "UniformSomePositionsArithmeticCrossover", "SinglePointCrossover", "AverageCrossover", "HeuristicCrossover", "MultiRealVectorCrossover" };
        public static string[] RealMutator = { "PolynomialAllPositionManipulator", "PolynomialOnePositionManipulator", "UniformOnePositionManipulator", "MultiRealVectorManipulator" };
        public static string[] RealSelectors = { "ProportionalSelector", "TournamentSelector", "RandomSelector", "BestSelector", "WorstSelector" };

        public static string[] IntCrossovers = { "RoundedUniformSomePositionsArithmeticCrossover", "SinglePointCrossover", "RoundedAverageCrossover", "RoundedHeuristicCrossover", "MultiIntegerVectorCrossover" };
        public static string[] IntMutator = { "RoundedNormalAllPositionsManipulator", "UniformOnePositionManipulator", "SelfAdaptiveRoundedNormalAllPositionsManipulator", "UniformSomePositionsManipulator" };
        public static string[] IntSelectors = { "ProportionalSelector", "TournamentSelector", "RandomSelector", "BestSelector", "WorstSelector" };


        public static UserArguments GetArgumentsFromUserOrConsole(List<string> arguments)
        {
            UserArguments userArguments = new UserArguments();
            int inputCounter = 0;

            userArguments.ValidTypes = ReadInType(arguments, ref inputCounter);
            userArguments.ValidDataSets = ReadInDataSet(arguments, ref inputCounter);
            userArguments.StoppingCriteria = ReadInStoppingCriterium(arguments, ref inputCounter);
            userArguments.TimeFactor = ReadInTimeFactor(arguments, ref inputCounter);
            userArguments.CostFactor = ReadInCostFactor(arguments, ref inputCounter);

            userArguments.ApplyVns = ReadInVns(arguments, ref inputCounter);
            if (userArguments.ApplyVns)
                userArguments.VnsNeighborhoodOperator = ReadInNeighborhoodOperator(arguments, ref inputCounter);
            if (userArguments.ApplyVns)
                userArguments.VnsPercentage = ReadInVnsPercentage(arguments, ref inputCounter);
            if (userArguments.ApplyVns)
                userArguments.VnsSecondaryOperator = ReadInVnsSecondary(arguments, ref inputCounter);


            userArguments.AmountOfCobots = ReadInAmountOfCobots(arguments, ref inputCounter);
            userArguments.MinimizeOrMaximize = ReadInMinimizeOrMaximize(arguments, ref inputCounter);
            userArguments.UseNormalizedValue = ReadInNormalization(arguments, ref inputCounter);


            //========================= Fix at the moment =====================================
            #region genetic operators
            userArguments.ValidSelectors.Add(0);
            userArguments.ValidCrossovers.Add(0);
            userArguments.ValidMutators.Add(0);
            #endregion
            userArguments.Repetitions = ReadInRepetitions(arguments, ref inputCounter);

            userArguments.DeterministicOrStochastic = ReadInDeterminism(arguments, ref inputCounter);
           
            return userArguments;
        }

        private static bool ReadInDeterminism(List<string> arguments, ref int inputCounter)
        {
            return false;
            Console.WriteLine("Deterministic or stochastic (0 == Deterministic, 1== Stochastic)?");
            bool determinism = false;
            while (true)
            {
                if (arguments.Count > 10)
                    if (int.TryParse(arguments[10], out int normalizedDeterministicOrStochastic))
                    {
                        if (normalizedDeterministicOrStochastic == 1)
                        {
                            Console.WriteLine("Stochastic simulation");
                            determinism = true;
                            break;
                        }
                        else if (normalizedDeterministicOrStochastic == 0)
                        {
                            Console.WriteLine("Deterministic simulation");
                            determinism = false;
                            break;
                        }
                    }

                string inputNormalizedDeterminism = Console.ReadLine();
                if (int.TryParse(inputNormalizedDeterminism, out int normalizedDeterministicOrStochastic2))
                {
                    if (normalizedDeterministicOrStochastic2 == 1)
                    {
                        Console.WriteLine("Stochastic simulation");
                        determinism = true;
                        break;
                    }
                    else if (normalizedDeterministicOrStochastic2 == 0)
                    {
                        Console.WriteLine("Deterministic simulation");
                        determinism = false;
                        break;
                    }
                }
            }
            inputCounter++;
            return determinism;
        }

        private static bool ReadInNormalization(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Use normalized value (0 == false, 1== true)?");
            bool useNormalization;
            while (true)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int normalizedValueArgs))
                    {
                        if (normalizedValueArgs == 1)
                        {
                            Console.WriteLine("Normalize");
                            useNormalization = true;
                            break;
                        }
                        else if (normalizedValueArgs == 0)
                        {
                            Console.WriteLine("No normalization");
                            useNormalization = false;
                            break;
                        }
                    }

                string inputNormalizedValue = Console.ReadLine();
                if (int.TryParse(inputNormalizedValue, out int parsedMinimizeOrMaximize))
                {
                    if (parsedMinimizeOrMaximize == 1)
                    {
                        Console.WriteLine("Normalize");
                        useNormalization = true;
                        break;
                    }
                    else if (parsedMinimizeOrMaximize == 0)
                    {
                        Console.WriteLine("No normalization");
                        useNormalization = false;
                        break;
                    }
                }
            }
            inputCounter++;
            return useNormalization;
        }

        private static int ReadInMinimizeOrMaximize(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Minimize or maximize (0 => minimize, 1 maximize):");
            int minimizeOrMaximize = 0;
            while (true)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int minimizeOrMaximizeArgs))
                    {
                        if (minimizeOrMaximizeArgs == 1)
                        {
                            Console.WriteLine("maximize");
                            minimizeOrMaximize = minimizeOrMaximizeArgs;
                            break;
                        }
                        else if (minimizeOrMaximizeArgs == 0)
                        {
                            Console.WriteLine("minimize");
                            minimizeOrMaximize = minimizeOrMaximizeArgs;
                            break;
                        }
                    }

                string inputOptimizationType = Console.ReadLine();
                if (int.TryParse(inputOptimizationType, out int parsedMinimizeOrMaximize))
                {
                    if (parsedMinimizeOrMaximize == 1)
                    {
                        Console.WriteLine("maximize");
                        minimizeOrMaximize = parsedMinimizeOrMaximize;
                        break;
                    }
                    else if (parsedMinimizeOrMaximize == 0)
                    {
                        Console.WriteLine("minimize");
                        minimizeOrMaximize = parsedMinimizeOrMaximize;
                        break;
                    }
                }
            }
            inputCounter++;
            return minimizeOrMaximize;
        }

        private static int ReadInAmountOfCobots(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("AmountOfCobots:");
            int amountOfCobots = 0;
            while (true)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedCobotsArgs))
                    {
                        Console.WriteLine(parsedCobotsArgs);
                        amountOfCobots = parsedCobotsArgs;
                        break;
                    }

                string inpu = Console.ReadLine();
                if (int.TryParse(inpu, out int parsedCobots))
                {
                    Console.WriteLine(parsedCobots);
                    amountOfCobots = parsedCobots;
                    break;
                }
            }
            inputCounter++;
            return amountOfCobots;
        }

        private static double ReadInVnsSecondary(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Insert vns secondary operator:");
            double vnsSecondary = -1;
            while (vnsSecondary == -1)
            {
                if (arguments.Count > inputCounter)
                    if (double.TryParse(arguments[inputCounter], out double parsedVnsPercentage))
                        vnsSecondary = parsedVnsPercentage;
                if (vnsSecondary != -1)
                {
                    Console.WriteLine(vnsSecondary);
                    break;
                }

                string consoleInput = Console.ReadLine();
                if (double.TryParse(consoleInput, out double parsedVnsPercentage2))
                {
                    vnsSecondary = parsedVnsPercentage2;
                }
            }
            inputCounter++;
            return vnsSecondary;
        }

        private static double ReadInVnsPercentage(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Insert vns percentage:");
            double vnsPercentage = -1;
            while (vnsPercentage == -1)
            {
                if (arguments.Count > inputCounter)
                    if (double.TryParse(arguments[inputCounter], out double parsedVnsPercentage))
                        vnsPercentage = parsedVnsPercentage;
                if (vnsPercentage != -1)
                {
                    Console.WriteLine(vnsPercentage);
                    break;
                }

                string consoleInput = Console.ReadLine();
                if (double.TryParse(consoleInput, out double parsedVnsPercentage2))
                {
                    vnsPercentage = parsedVnsPercentage2;
                }
            }
            inputCounter++;
            return vnsPercentage;
        }

        private static bool ReadInVns(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Apply vns:");
            bool applyVns = true;
            while (true)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedVns))
                    {
                        if (parsedVns == 1)
                        {
                            applyVns = true;
                            Console.WriteLine("True");
                            break;
                        }
                        else if (parsedVns == 0)
                        {
                            Console.WriteLine("False");
                            applyVns = false;
                            break;
                        }
                    }

                //Console.WriteLine("Automatically selected for faster starting: 1");
                string inp = Console.ReadLine();
                if (int.TryParse(inp, out int parsedApplyVns))
                {
                    if (parsedApplyVns == 1)
                    {
                        Console.WriteLine("True");
                        applyVns = true;
                        break;
                    }
                    else if (parsedApplyVns == 0)
                    {
                        Console.WriteLine("False");
                        applyVns = false;
                        break;
                    }
                }
            }
            inputCounter++;
            return applyVns;
        }

        private static double ReadInCostFactor(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Insert cost factor for optimization:");
            double CostFactor = -1;
            while (CostFactor == -1)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedCostFactorArgs))
                        CostFactor = parsedCostFactorArgs;
                if (CostFactor != -1)
                {
                    Console.WriteLine(CostFactor);
                    break;
                }

                //userArguments.CostFactor = 1;
                //Console.WriteLine("1");
                string i2 = Console.ReadLine();
                if (double.TryParse(i2, out double parsedCostFactor))
                {
                    CostFactor = parsedCostFactor;
                }
            }
            inputCounter++;
            return CostFactor;
        }

        private static double ReadInTimeFactor(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Insert time factor for optimization:");
            double TimeFactor = -1;
            while (TimeFactor == -1)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedTimeFactorArgs))
                        TimeFactor = parsedTimeFactorArgs;
                if (TimeFactor != -1)
                {
                    Console.WriteLine(TimeFactor);
                    break;
                }

                string i1 = Console.ReadLine();
                //userArguments.TimeFactor = 1;
                //Console.WriteLine("1");
                if (double.TryParse(i1, out double parsedTimeFactor))
                {
                    TimeFactor = parsedTimeFactor;
                }
            }
            inputCounter++;
            return TimeFactor;
        }

        private static int ReadInRepetitions(List<string> arguments, ref int inputCounter)
        {
            return 1;
            int Repetitions = 0;
            while (Repetitions == 0)
            {
                Console.WriteLine("Repetitions:");
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedReptitons))
                        Repetitions = parsedReptitons;
                if (Repetitions != 0)
                {
                    Console.WriteLine(Repetitions);
                    break;
                }

                string input = Console.ReadLine();
                //userArguments.Repetitions = 10;
                //Console.WriteLine("For faster starting 10 is selected");
                if (int.TryParse(input, out int parsedRepetitions))
                {
                    if (parsedRepetitions > 0)
                        Repetitions = parsedRepetitions;
                }
            }
            inputCounter++;
            return Repetitions;
        }

        /// <summary>
        /// Currently we only use time as stopping criterium
        /// Read in the runtime of the algorithm in minutes
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="inputCounter"></param>
        /// <returns></returns>
        private static (string, int) ReadInStoppingCriterium(List<string> arguments, ref int inputCounter)
        {
            string generationsOrTime = "time";
            int criterium = 0;

            while (criterium == 0)
            {
                Console.WriteLine("Insert time in minutes:");
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int convertedArgument))
                        criterium = convertedArgument * 60000;
                if (criterium != 0)
                {
                    Console.WriteLine(criterium / 60000);
                    break;
                }

                string input = Console.ReadLine();

                if (int.TryParse(input, out int value))
                    criterium = value * 60000;
            }
            inputCounter++;
            return (generationsOrTime, criterium);
        }

        /// <summary>
        /// Which data should be solved
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="inputCounter"></param>
        /// <returns></returns>
        private static List<string> ReadInDataSet(List<string> arguments, ref int inputCounter)
        {
            string dataSet = "";

            while (dataSet == "")
            {
                Console.WriteLine("Insert data set (Full, Half1, Half2, Quarter1, ... Quarter4,  cp1-cp50):");

                if (arguments.Count > inputCounter)
                    dataSet = arguments[inputCounter];
                dataSet = Normalizer.FindCorrectDataSet(dataSet);
                if (dataSet != "")
                {
                    Console.WriteLine(dataSet);
                    break;
                }

                dataSet = Console.ReadLine();
                dataSet = Normalizer.FindCorrectDataSet(dataSet);
                Console.WriteLine("Selected:" + dataSet);
            }
            inputCounter++;
            return Normalizer.GetValidDataSets(dataSet);
        }
        /// <summary>
        /// Get encoding type from arguments or user (integer or biased random key)
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="inputCounter"></param>
        /// <returns></returns>
        private static List<string> ReadInType(List<string> arguments, ref int inputCounter)
        {
            string type = "";
            while (type != "Real" && type != "Int" && type != "All")
            {
                Console.WriteLine("Insert type (int, real, all):");

                if (arguments.Count > inputCounter)
                    type = arguments[inputCounter];
                type = Normalizer.FindCorrectType(type);
                if (type == "Real" || type == "Int")
                {
                    Console.WriteLine(type);
                    break;
                }

                type = Console.ReadLine();
                Console.WriteLine("Selected: " + type);
            }
            inputCounter++;
            return Normalizer.GetValidTypes(type);
        }

        private static int ReadInNeighborhoodOperator(List<string> arguments, ref int inputCounter)
        {
            Console.WriteLine("Insert vns neighborhood operator 0 - x:");
            int vnsNeighborhoodOperator = -1;
            while (vnsNeighborhoodOperator == -1)
            {
                if (arguments.Count > inputCounter)
                    if (int.TryParse(arguments[inputCounter], out int parsedVnsNeighborhoodOperator1))
                        vnsNeighborhoodOperator = parsedVnsNeighborhoodOperator1;
                if (vnsNeighborhoodOperator != -1)
                {
                    Console.WriteLine(vnsNeighborhoodOperator);
                    break;
                }

                //userArguments.CostFactor = 1;
                //Console.WriteLine("1");
                string i2 = Console.ReadLine();
                if (int.TryParse(i2, out int parsedVnsNeighborhoodOperator2))
                    vnsNeighborhoodOperator = parsedVnsNeighborhoodOperator2;

            }
            inputCounter++;
            return vnsNeighborhoodOperator;
        }

        internal static void WriteXesFiles(Dictionary<UserArgument, (double, OutputFileTexts)> resultDictionary, string outputpath)
        {
            foreach (KeyValuePair<UserArgument, (double, OutputFileTexts)> pair in resultDictionary)
            {
                Console.WriteLine("Wrote process mining information to file");

                for (int i = 0; i < pair.Value.Item2.ResourceInformations.Count; i++)
                {
                    string path = Path.Combine(new[]
                    {
                        outputpath, $"{pair.Key.DataSet}_{pair.Key.Type}_C{pair.Value.Item2.BestCost}_T{pair.Value.Item2.BestMakespan}_{pair.Value.Item1}_{i}.xes"
                    });
                    File.WriteAllText(path, pair.Value.Item2.ResourceInformations[i]);
                }
            }
        }


        /// <summary>
        /// Define a real encoded problem for the genetic algorithm and set it as problem for the algorithm
        /// </summary>
        /// <param name="combination"></param>
        /// <param name="algorithm"></param>
        /// <param name="userArgument"></param>
        /// <param name="endTime"></param>
        public static void SetRealProblem(Combination combination, GeneticAlgorithm algorithm, UserArguments userArgument, DateTime endTime, bool DebugLog)
        {
            Easy4SimRealEncodingProblem.EndTime = endTime;
            if (DebugLog)
                Console.WriteLine($"03 - Create {combination.ValidType} problem");

            bool minMax = false;
            if (userArgument.MinimizeOrMaximize == 1)
                minMax = true;

            Easy4SimRealEncodingProblem problem = new Easy4SimRealEncodingProblem(maximization: minMax, userArgument.AmountOfCobots);
            problem.TimeFactor = userArgument.TimeFactor;
            problem.CostFactor = userArgument.CostFactor;

            Easy4SimRealEncodingProblem.UseNormalizedValue = userArgument.UseNormalizedValue;
            Easy4SimRealEncodingProblem.BestSolutionSoFarGa = double.MaxValue;
            Easy4SimRealEncodingProblem.ApplyVNS = userArgument.ApplyVns;
            Easy4SimRealEncodingProblem.VnsNeighborhoodOperator = userArgument.VnsNeighborhoodOperator;
            Easy4SimRealEncodingProblem.VnsPercentage = userArgument.VnsPercentage;
            Easy4SimRealEncodingProblem.VnsIntelligent = userArgument.VnsSecondaryOperator;
            Easy4SimRealEncodingProblem.DeterministicOrStochastic = userArgument.DeterministicOrStochastic;
            problem.Easy4SimEvaluationScript.Value = combination.ValidDataSet;
            if (DebugLog)
                Console.WriteLine("  Number of simulation objects " + problem.SolverSettings.SimulationObjects.SimulationList.Count);
            algorithm.Problem = problem;
            problem.SolverSettings.Statistics.DataSet = combination.ValidDataSet;
            SimulationStatistics.LowerLimitCost = -1;
            SimulationStatistics.UpperLimitCost = -1;
            SimulationStatistics.LowerLimitTime = -1;
            SimulationStatistics.UpperLimitTime = -1;

            algorithm.CrossoverParameter.Value = problem.Encoding.Operators.OfType<ICrossover>().FirstOrDefault(x => x.Name.Contains(RealCrossovers[combination.Crossover]));
            algorithm.MutatorParameter.Value = problem.Encoding.Operators.OfType<IManipulator>().FirstOrDefault(x => x.Name.Contains(RealMutator[combination.Mutator]));
        }
        /// <summary>
        /// Define a integer encoded problem for the genetic algorithm and set it
        /// </summary>
        /// <param name="combination"></param>
        /// <param name="algorithm"></param>
        /// <param name="userArgument"></param>
        /// <param name="endTime"></param>
        public static void SetIntProblem(Combination combination, GeneticAlgorithm algorithm, UserArguments userArgument, DateTime endTime, bool DebugLog)
        {
            Easy4SimIntegerEncodingProblem.EndTime = endTime;
            if (DebugLog)
                Console.WriteLine($"03 - Create {combination.ValidType} problem");

            bool minMax = false;
            if (userArgument.MinimizeOrMaximize == 1)
                minMax = true;

            Easy4SimIntegerEncodingProblem problem = new Easy4SimIntegerEncodingProblem(maximization: minMax, userArgument.AmountOfCobots);
            problem.TimeFactor = userArgument.TimeFactor;
            problem.CostFactor = userArgument.CostFactor;
            Easy4SimIntegerEncodingProblem.UseNormalizedValue = userArgument.UseNormalizedValue;
            problem.Easy4SimEvaluationScript.Value = combination.ValidDataSet;
            Easy4SimIntegerEncodingProblem.BestSolutionSoFarGa = double.MaxValue;
            Easy4SimIntegerEncodingProblem.ApplyVNS = userArgument.ApplyVns;
            Easy4SimIntegerEncodingProblem.VnsNeighborhoodOperator = userArgument.VnsNeighborhoodOperator;
            Easy4SimIntegerEncodingProblem.VnsIntelligent = userArgument.VnsSecondaryOperator;
            Easy4SimIntegerEncodingProblem.VnsPercentage = userArgument.VnsPercentage;
            if (DebugLog)
                Console.WriteLine("Number of simulation objects " + problem.SolverSettings.SimulationObjects.SimulationList.Count);
            algorithm.Problem = problem;
            problem.SolverSettings.Statistics.DataSet = combination.ValidDataSet;
            SimulationStatistics.LowerLimitCost = -1;
            SimulationStatistics.UpperLimitCost = -1;
            SimulationStatistics.LowerLimitTime = -1;
            SimulationStatistics.UpperLimitTime = -1;
            algorithm.CrossoverParameter.Value = problem.Encoding.Operators.OfType<ICrossover>().FirstOrDefault(x => x.Name.Contains(IntCrossovers[combination.Crossover]));
            algorithm.MutatorParameter.Value = problem.Encoding.Operators.OfType<IManipulator>().FirstOrDefault(x => x.Name.Contains(IntMutator[combination.Mutator]));
        }


        public static void GetSelector(int selector, GeneticAlgorithm algorithm)
        {
            switch (selector)
            {
                case 0:
                    algorithm.SelectorParameter.Value = algorithm.SelectorParameter.ValidValues.ElementAt(5);
                    break;
                case 1:
                    TournamentSelector sel = new TournamentSelector();
                    algorithm.SelectorParameter.Value = algorithm.SelectorParameter.ValidValues.ElementAt(7);
                    break;
                case 2:
                    algorithm.Selector = new RandomSelector();
                    algorithm.SelectorParameter.Value = algorithm.SelectorParameter.ValidValues.ElementAt(6);
                    break;
                case 3:
                    algorithm.Selector = new BestSelector();
                    algorithm.SelectorParameter.Value = algorithm.SelectorParameter.ValidValues.ElementAt(0);
                    break;
                case 4:
                    algorithm.Selector = new WorstSelector();
                    algorithm.SelectorParameter.Value = algorithm.SelectorParameter.ValidValues.ElementAt(8);
                    break;
            }
        }



    }

}
