using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using System;
using System.Reflection;
using Easy4SimFramework;
using FjspEasy4SimLibrary;
using HeuristicLab.Random;
namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    /// <summary>
    /// Generates a new random integer vector with each element uniformly distributed in a specified range.
    /// </summary>
    [Item("PriorityRuleSolutionGenerator", "An operator which creates a new random int vector with each element uniformly distributed in a specified range.")]
    [StorableType("BC307354-23AE-4A18-895B-D682E897B45D")]
    public class PriorityRuleSolutionGenerator : IntegerVectorCreator
    {
        [StorableConstructor]
        protected PriorityRuleSolutionGenerator(StorableConstructorFlag _) : base(_) { }
        protected PriorityRuleSolutionGenerator(PriorityRuleSolutionGenerator original, Cloner cloner) : base(original, cloner) { }
        public PriorityRuleSolutionGenerator() { }
        public static string ProblemFile { get; set; }
        public static double AmountOfCobots { get; set; }

        private static int _solutionCounter;
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new PriorityRuleSolutionGenerator(this, cloner);
        }

        /// <summary>
        /// Generates a new random integer vector with the given <paramref name="length"/>.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="length">The length of the int vector.</param>
        /// <param name="bounds">A matrix containing the inclusive lower and inclusive upper bound in the first and second column and a step size in the third column.
        /// Each line represents the bounds for a certain dimension. If fewer lines are given, the lines are cycled.</param>
        /// <returns>The newly created integer vector.</returns>
        public static IntegerVector Apply(IRandom random, int length, IntMatrix bounds)
        {
            int numberOfPossibilities = FjspPriorityRuleSolutionGenerator.PriorityRules.Count *
                                        FjspPriorityRuleSolutionGenerator.CobotRules.Count;

            if (_solutionCounter < numberOfPossibilities)
            {
                IntegerVector result = GeneratedPriorityRuleSolution(length);
                _solutionCounter++;
                return result;
            }
            else
            {
                try
                {
                    var result = new IntegerVector(length);
                    result.Randomize(random, bounds);
                    _solutionCounter++;
                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error in {MethodBase.GetCurrentMethod().DeclaringType} - {MethodBase.GetCurrentMethod()?.Name}: " + e);
                    throw;
                }
            }
        }

        private static IntegerVector GeneratedPriorityRuleSolution(int length)
        {
            SimulationObjects simulationObjects = new SimulationObjects();
            Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

            DataStore.FileLoader loader = new DataStore.FileLoader();
            string fileContent = loader.LoadFile(ProblemFile);

            
            IntegerVector result = new IntegerVector(length);

            int priorityRuleIndex = _solutionCounter / FjspPriorityRuleSolutionGenerator.CobotRules.Count;
            int cobotRules = _solutionCounter - (FjspPriorityRuleSolutionGenerator.CobotRules.Count * priorityRuleIndex);


            SolverSettings settings = new SolverSettings(environment, simulationObjects, null);
            FjspLoader fJsspLoader = new FjspLoader(0, "FjspLoader1", settings);
            fJsspLoader.FileContent = new ParameterString(fileContent);
            settings.SimulationObjects.UpdateElement(fJsspLoader);

            FjspPriorityRuleSolutionGenerator ruleSolutionGenerator = new FjspPriorityRuleSolutionGenerator(1, "FjspPriorityRuleSolutionGenerator1", settings);
            ruleSolutionGenerator.ReadData.Connect(fJsspLoader.ReadData);
            ruleSolutionGenerator.AmountOfCobots = new ParameterReal(AmountOfCobots);
            ruleSolutionGenerator.Rule = priorityRuleIndex;
            ruleSolutionGenerator.CobotRule = cobotRules;
            ruleSolutionGenerator.Random = new MersenneTwister();
            settings.SimulationObjects.UpdateElement(ruleSolutionGenerator);

            Link link = new Link();
            link.OutputComponentName = "FjspLoader1";
            link.InputComponentName = "FjspPriorityRuleSolutionGenerator1";
            link.OutputConnectorName = "ReadData";
            link.InputConnectorName = "ReadData";

            simulationObjects.AddLink(link);

            Solver solver = new Solver(settings);
            solver.SimulationStatistics.AmountOfCobotsPercent = AmountOfCobots;
            solver.Init();

            solver.Start();
            solver.RunFinishEventOnly();
            Console.WriteLine($"Generated initial solution({priorityRuleIndex * FjspPriorityRuleSolutionGenerator.CobotRules.Count + cobotRules}): {solver.SolverSettings.Statistics.Fitness}");
            int encodingCounter = 0;
            foreach (int assignment in ruleSolutionGenerator.WorkstationAssignment.Value)
            {
                result[encodingCounter] = assignment;
                encodingCounter++;
            }

            foreach (int priority in ruleSolutionGenerator.Priority.Value)
            {
                result[encodingCounter] = priority;
                encodingCounter++;
            }

            foreach (int cobotLocation in ruleSolutionGenerator.CobotLocations.Value)
            {
                result[encodingCounter] = cobotLocation;
                encodingCounter++;
            }

            return result;
        }

        protected override IntegerVector Create(IRandom random, IntValue length, IntMatrix bounds)
        {
            return Apply(random, length.Value, bounds);
        }
    }
}
