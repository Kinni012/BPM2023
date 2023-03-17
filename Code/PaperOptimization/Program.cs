using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Easy4SimMultiEncoding.Plugin;
using HeuristicLab.Optimization;
using HeuristicLab.SequentialEngine;
using System.Globalization;
using Easy4SimFramework;
using Environment = System.Environment;

namespace PaperOptimization
{
    class Program
    {
        /// <summary>
        /// Additional log information to run on vsc
        /// </summary>
        public static bool DebugLog = false;

        public static bool ProcessMiningLog = true;

        private static string OutputPath = "";
        private static string OutputXesPath = "";
        /// <summary>
        /// Set the output path for the created excel files
        /// </summary>
        private static string SetOutputPath()
        {
            string outputPath = Path.Combine(new[] { Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..", "Results" });
            string outputXesPath = Path.Combine(new[] { Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "..", "..", "Results", "XesFiles" });
            try
            {
                bool exists = Directory.Exists(outputPath);
                if (!exists)
                    Directory.CreateDirectory(outputPath);


                if (!Directory.Exists(outputXesPath))
                    Directory.CreateDirectory(outputXesPath);

                OutputPath = outputPath;
                OutputXesPath = outputXesPath;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in SetOutputPath: " + e);
                throw;
            }
            return outputPath;
        }

        static void Main(string[] args)
        {
            foreach (string argument in args)
            {
                if (argument.ToUpper().Contains("HELP"))
                {
                    Console.WriteLine("Provide the following parameters to the command line:" + Environment.NewLine);
                    Console.WriteLine("Encoding: real/int");
                    Console.WriteLine("DataSet: full/half1/half2/quarter1/.../quarter4/cp1/.../cp10");
                    Console.WriteLine("Runtime in minutes");
                    Console.WriteLine("Time factor in objective function, e.g. 1");
                    Console.WriteLine("Cost factor in objective function, e.g. 0");
                    Console.WriteLine("Use variable neighborhood search? 0/1(no/yes)");
                    Console.WriteLine("Neighborhood operator 0 - x:");
                    Console.WriteLine("Vns percentage => how close a solution has to be to the best solution to be used in vns (e.g. 0.1):");
                    Console.WriteLine("Vns secondary operator (e.g. 0.1):");
                    Console.WriteLine("Amount of cobots, e.g. 0/5");
                    Console.WriteLine("Minimize or maximize: 0/1(0 == minimize, 1 == maximize)");
                    Console.WriteLine("Use normalized objective function: 0 == false, 1 == true");
                    return;
                }
            }


            try
            {

                List<string> arguments = args.ToList();
                UserArguments userArguments = HelperFunctions.GetArgumentsFromUserOrConsole(arguments);
                Console.Clear();
                Console.WriteLine(userArguments.ToString());
                Console.WriteLine("==============================================================");



                List<Combination> dataStore = userArguments.GetCombinations();


                DateTime now = DateTime.Now;
                now = now.AddMilliseconds(dataStore.Count * userArguments.StoppingCriteria.Item2);
                Console.WriteLine("Estimated end time: " + now);

                Console.WriteLine($"{dataStore.Count} possibilities selected");


                int counter = 0;

                Dictionary<UserArgument, (double, OutputFileTexts)> resultDictionary = new Dictionary<UserArgument, (double, OutputFileTexts)>();


                foreach (Combination data in dataStore)
                {
                    Console.WriteLine($"Evaluation of solution {counter + 1}/{dataStore.Count}");
                    counter++;

                    if (DebugLog)
                        Console.WriteLine("02 - Initialize genetic algorithm");
                    //========================== Create and set problem ===============================================
                    GeneticAlgorithm algorithm = new GeneticAlgorithm();
                    HelperFunctions.GetSelector(data.Selector, algorithm);

                    if (userArguments.StoppingCriteria.Item1 == "generations")
                        algorithm.MaximumGenerations.Value = userArguments.StoppingCriteria.Item2;
                    else
                        algorithm.MaximumGenerations.Value = 10000000; //high amount of generations, stop after time

                    if (data.ValidType == "Int")
                    {
                        HelperFunctions.SetIntProblem(data,
                                   algorithm,
                                   userArguments,
                                   now,
                                   DebugLog);
                    }
                    else
                    {
                        HelperFunctions.SetRealProblem(data,
                                   algorithm,
                                   userArguments,
                                   now,
                                   DebugLog);
                    }


                    //========================== Create and set engine ===============================================
                    SequentialEngine engine = new SequentialEngine();
                    algorithm.Engine = engine;

                    // ===================== Run algorithm ====================================
                    if (DebugLog)
                        Console.WriteLine("04 - set engine");
                    Stopwatch sw = new Stopwatch();
                    if (DebugLog)
                        Console.WriteLine("05 - Start algorithm");
                    sw.Start();
                    algorithm.Prepare(true);
                    if (userArguments.StoppingCriteria.Item1 == "generations")
                        algorithm.Start();
                    if (userArguments.StoppingCriteria.Item1 == "time")
                    {
                        Console.WriteLine($"Stopping criterium time: {userArguments.StoppingCriteria.Item2}");
                        var ct = new CancellationTokenSource(userArguments.StoppingCriteria.Item2).Token;
                        algorithm.Start(ct);
                    }
                    sw.Stop();
                    if (DebugLog)
                        Console.WriteLine("06 - Print results");
                    Console.WriteLine($"  Algorithm duration: {sw.ElapsedMilliseconds}");
                    // ======================== Write the algorithm results to files ==================================================
                    string outputPath = SetOutputPath();
                    string fileName = $"{Guid.NewGuid()}.csv";
                    List<string> lines = new List<string>();
                    string separator = ",";




                    if (algorithm.Problem is Easy4SimRealEncodingProblem realProblem)
                    {
                        Console.WriteLine($"{data.ValidType} {data.ValidDataSet} {userArguments.StoppingCriteria}: {realProblem.BestQuality.Value.ToString()}(Best cost: {realProblem.bestCost}, Best time: {realProblem.bestTime})  ({algorithm.CrossoverParameter.Value?.Name}, {algorithm.MutatorParameter.Value?.Name}, {algorithm.SelectorParameter.Value?.Name})");
                        lines.Add($"Encoding:{separator}{data.ValidType}");
                        lines.Add($"Dataset:{separator}{data.ValidDataSet}");
                        lines.Add($"Runtime:{separator}{userArguments.StoppingCriteria.Item2 / 60000}{separator}minutes");
                        lines.Add($"Crossover:{separator}{algorithm.CrossoverParameter.Value?.Name}");
                        lines.Add($"Mutator:{separator}{algorithm.MutatorParameter.Value?.Name}");
                        lines.Add($"Selector:{separator}{algorithm.SelectorParameter.Value?.Name}");
                        lines.Add($"ApplyVns:{separator}{userArguments.ApplyVns}");
                        lines.Add($"VNS%:{separator}{userArguments.VnsPercentage}");
                        lines.Add($"VNSI%:{separator}{userArguments.VnsSecondaryOperator}");
                        lines.Add($"Minimize or maximize:{separator}{userArguments.MinimizeOrMaximize}");
                        lines.Add($"Evaluated solutions:{separator}{SimulationStatistics.EvaluatedSolutions}");
                        lines.Add($"Time factor:{separator}{userArguments.TimeFactor}");
                        lines.Add($"Cost factor:{separator}{userArguments.CostFactor}");
                        lines.Add($"Best time:{separator}{realProblem.bestTime}");
                        lines.Add($"Best Cost:{separator}{realProblem.bestCost}");
                        lines.Add($"Normalize :{separator}{userArguments.UseNormalizedValue}");
                        lines.Add($"VNS neighborhood:{separator}{realProblem.Solver.SimulationStatistics.VnsNeighborhood}");
                        lines.Add($"Amount of Cobots:{separator}{userArguments.AmountOfCobots}");
                        lines.Add($"Best quality:{separator}{realProblem.BestQuality.Value.ToString()}");

                        lines.Add("");
                        lines.Add(realProblem.GanttInformation.Value.ToString());


                        if (ProcessMiningLog)
                        {
                            UserArgument argument = new UserArgument(data);
                            int currentRunFitness = 0;
                            int currentRunMakespan = 0;
                            int currentRunCost = 0;
                            try
                            {
                                currentRunFitness = Convert.ToInt32(realProblem.BestQuality.Value.ToString(),
                                    CultureInfo.InvariantCulture);
                                currentRunMakespan =
                                    Convert.ToInt32(realProblem.SolverSettings.Environment.SimulationTime);
                                currentRunCost = Convert.ToInt32(realProblem.SolverSettings.Statistics.Fitness);
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                if (currentRunFitness == 0)
                                {
                                    double test = Convert.ToDouble(realProblem.BestQuality.Value.ToString(), CultureInfo.InvariantCulture);
                                    currentRunFitness = (int)test;
                                }
                            }
                            catch (Exception) { }
                            if (resultDictionary.ContainsKey(argument))
                            {
                                if (resultDictionary[argument].Item1 > currentRunFitness)
                                    resultDictionary[argument] = (currentRunFitness, new OutputFileTexts(realProblem.MiningInformation.Value.ToString(), realProblem.MiningInformationResource.Value.ToString(), realProblem.BestCost.Value.ToString(), realProblem.BestMakespan.Value.ToString()));
                            }
                            else
                                resultDictionary.Add(argument, (currentRunFitness, new OutputFileTexts(realProblem.MiningInformation.Value.ToString(), realProblem.MiningInformationResource.Value.ToString(), realProblem.BestCost.Value.ToString(), realProblem.BestMakespan.Value.ToString())));

                        }
                    }

                    if (algorithm.Problem is Easy4SimIntegerEncodingProblem intProblem)
                    {
                        Console.WriteLine($"{data.ValidType} {data.ValidDataSet} {userArguments.StoppingCriteria}: {intProblem.BestQuality.Value.ToString()}(Best cost: {intProblem.bestCost}, Best time: {intProblem.bestTime})  ({algorithm.CrossoverParameter.Value?.Name}, {algorithm.MutatorParameter.Value?.Name}, {algorithm.SelectorParameter.Value?.Name})");
                        lines.Add($"Encoding:{separator}{data.ValidType}");
                        lines.Add($"Dataset:{separator}{data.ValidDataSet}");
                        lines.Add($"Runtime:{separator}{userArguments.StoppingCriteria.Item2 / 60000}{separator}minutes");
                        lines.Add($"Crossover:{separator}{algorithm.CrossoverParameter.Value?.Name}");
                        lines.Add($"Mutator:{separator}{algorithm.MutatorParameter.Value?.Name}");
                        lines.Add($"Selector:{separator}{algorithm.SelectorParameter.Value?.Name}");
                        lines.Add($"ApplyVns:{separator}{userArguments.ApplyVns}");
                        lines.Add($"VNS%:{separator}{userArguments.VnsPercentage}");
                        lines.Add($"VNSI%:{separator}{userArguments.VnsSecondaryOperator}");
                        lines.Add($"Minimize or maximize:{separator}{userArguments.MinimizeOrMaximize}");
                        lines.Add($"Evaluated solutions:{separator}{SimulationStatistics.EvaluatedSolutions}");
                        lines.Add($"Time factor:{separator}{userArguments.TimeFactor}");
                        lines.Add($"Cost factor:{separator}{userArguments.CostFactor}");
                        lines.Add($"Best time:{separator}{intProblem.bestTime}");
                        lines.Add($"Best Cost:{separator}{intProblem.bestCost}");
                        lines.Add($"Normalize :{separator}{userArguments.UseNormalizedValue}");
                        lines.Add($"VNS neighborhood:{separator}{intProblem.Solver.SimulationStatistics.VnsNeighborhood}");
                        lines.Add($"Amount of Cobots:{separator}{userArguments.AmountOfCobots}");
                        lines.Add($"Best quality:{separator}{intProblem.BestQuality.Value.ToString()}");

                        lines.Add("");
                        lines.Add(intProblem.GanttInformation.Value.ToString());
                    }


                    using (FileStream stream = new FileStream(Path.Combine(outputPath, fileName), FileMode.Append))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            foreach (string line in lines)
                                writer.WriteLine(line);
                        }
                    }
                    Console.WriteLine();
                    if (DebugLog)
                    {
                        Console.WriteLine($"  Results:");
                        foreach (IResult result in algorithm.Results)
                        {
                            Console.WriteLine($"  {result.Name}: {result.Value} ");
                        }

                        Console.WriteLine();
                        Console.WriteLine($"  Algorithm properties:");
                        foreach (PropertyInfo info in algorithm.GetType().GetProperties())
                        {
                            try
                            {
                                Console.WriteLine($"  {info.Name}: {info.GetValue(algorithm)} ");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error{info.Name}: " + e);
                            }
                        }
                        Console.WriteLine("Finished");
                    }

                }

                if (ProcessMiningLog)
                {
                    HelperFunctions.WriteXesFiles(resultDictionary, OutputXesPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception happened:");
                Console.WriteLine("  " + e.Message);
                Console.WriteLine("  " + e.InnerException);
            }


            Console.Write("Finished");
        }



    }
}
