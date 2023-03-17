using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Easy4SimFramework;
using FjspEasy4SimLibrary;
using HeuristicLab.Easy4SimMultiEncoding.Plugin;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;
using Environment = Easy4SimFramework.Environment;

namespace PriorityRuleGenerator
{
    class Program
    {
        public static string DataSet = "TestL_BBDHi_0.10.txt";
        public static double CobotAmount = 0.2;
        private static bool _singleRun = false;
        static void Main(string[] args)
        {
            if (_singleRun)
                RunOnOneDataSet(DataSet, CobotAmount, true);
            else
                RunForMultipleDataSets();
            Console.ReadLine();
        }


        private static void RunForMultipleDataSets()
        {
            List<string> dataSets = new List<string>();

            List<int> problemCategories = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            List<int> problemInstances = new List<int>() { 0, 10, 20, 30 };
            List<double> cobotSettings = new List<double>() { 0, 0.2, 0.4 };
            StringBuilder result = new StringBuilder();
            foreach (int category in problemCategories)
            {
                foreach (int instance in problemInstances)
                {
                    string dataSet = $"TestL_BBDHi_{category}.{instance}.txt";
                    Console.Write(dataSet + "\t");
                    foreach (double cobotSetting in cobotSettings)
                    {
                        double fitness = RunOnOneDataSet(dataSet, cobotSetting, false);
                        Console.Write(Math.Round(fitness, 2) + "\t");
                    }
                    Console.WriteLine();
                    
                }
            }
        }

        private static double RunOnOneDataSet(string dataSet, double cobotAmount, bool debugLog)
        {
            SimulationObjects simulationObjects = new SimulationObjects();
            Environment environment = new Environment();

            DataStore.FileLoader loader = new DataStore.FileLoader();
            //string fileContent = loader.LoadFile("Mk01.fjs");
            string fileContent = loader.LoadFile(dataSet);


            double fitness = int.MaxValue;
            int bestPriorityRule = 0;
            int bestCobotRule = 0;
            for (int i = 0; i < FjspPriorityRuleSolutionGenerator.PriorityRules.Count; i++)
            {
                for (int j = 0; j < FjspPriorityRuleSolutionGenerator.CobotRules.Count; j++)
                {
                    SolverSettings settings = new SolverSettings(environment, simulationObjects, null);
                    FjspLoader fJsspLoader = new FjspLoader(0, "FjspLoader1", settings);
                    fJsspLoader.FileContent = new ParameterString(fileContent);
                    settings.SimulationObjects.UpdateElement(fJsspLoader);

                    FjspPriorityRuleSolutionGenerator ruleSolutionGenerator = new FjspPriorityRuleSolutionGenerator(1, "FjspPriorityRuleSolutionGenerator1", settings);
                    ruleSolutionGenerator.ReadData.Connect(fJsspLoader.ReadData);
                    ruleSolutionGenerator.AmountOfCobots = new ParameterReal(cobotAmount);
                    ruleSolutionGenerator.Rule = i;
                    ruleSolutionGenerator.CobotRule = j;
                    ruleSolutionGenerator.Random = new MersenneTwister();
                    settings.SimulationObjects.UpdateElement(ruleSolutionGenerator);

                    Link link = new Link();
                    link.OutputComponentName = "FjspLoader1";
                    link.InputComponentName = "FjspPriorityRuleSolutionGenerator1";
                    link.OutputConnectorName = "ReadData";
                    link.InputConnectorName = "ReadData";

                    simulationObjects.AddLink(link);

                    Solver solver = new Solver(settings);
                    solver.SimulationStatistics.AmountOfCobotsPercent = cobotAmount;
                    solver.Init();

                    solver.Start();
                    solver.RunFinishEventOnly();

                    int encodingCounter = 0;
                    int length = ruleSolutionGenerator.WorkstationAssignment.Amount +
                                 ruleSolutionGenerator.Priority.Amount + ruleSolutionGenerator.CobotLocations.Amount;
                    IntegerVector result = new IntegerVector(length);
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

                    ValidateResult(result, solver.SimulationStatistics.Fitness, dataSet, cobotAmount, debugLog);
                    if (debugLog)
                    {
                        Console.WriteLine("Run " + (i + j));
                        Console.WriteLine($"  Rule => {FjspPriorityRuleSolutionGenerator.PriorityRules[i].Method.Name}");
                        Console.WriteLine($"  Cobot rule => {FjspPriorityRuleSolutionGenerator.CobotRules[j].Method.Name}");
                        Console.WriteLine($"  Fitness: {solver.SimulationStatistics.Fitness}");
                    }
                    if (solver.SimulationStatistics.Fitness < fitness)
                    {
                        bestPriorityRule = i;
                        bestCobotRule = j;
                        fitness = solver.SimulationStatistics.Fitness;
                    }
                }
            }

            if (debugLog)
            {
                Console.WriteLine($"Best result found with priority rule {FjspPriorityRuleSolutionGenerator.PriorityRules[bestPriorityRule].Method.Name} " +
                                  $"and cobot rule {FjspPriorityRuleSolutionGenerator.CobotRules[bestCobotRule].Method.Name}\n Solution quality: {fitness}");
            }
            return fitness;
        }

        private static void ValidateResult(IntegerVector result, double simulationStatisticsFitness, string dataSet, double cobotAmount, bool debugLog)
        {
            PriorityRuleSolutionGenerator.ProblemFile = dataSet;
            PriorityRuleSolutionGenerator.AmountOfCobots = cobotAmount;

            SimulationObjects simulationObjects = new SimulationObjects();
            Easy4SimFramework.Environment environment = new Easy4SimFramework.Environment();

            Logger logger = null; //set log path

            SolverSettings settings = new SolverSettings(environment, simulationObjects, logger);

            DataStore.FileLoader loader = new DataStore.FileLoader();
            string FileContent = loader.LoadFile(dataSet);

            FjspLoader fJSSP_Loader = new FjspLoader(0, "FJSSP_Loader1", settings);
            fJSSP_Loader.FileContent = new ParameterString(FileContent);

            FjspEvaluation evaluation = new FjspEvaluation(1, "FJSSP_Evaluation1", settings);
            evaluation.ReadData.Connect(fJSSP_Loader.ReadData);

            Link link = new Link();
            link.OutputComponentName = "FJSSP_Loader1";
            link.InputComponentName = "FJSSP_Evaluation1";
            link.OutputConnectorName = "ReadData";
            link.InputConnectorName = "ReadData";

            simulationObjects.AddLink(link);



            Solver s = new Solver(settings);
            s.SimulationStatistics.AmountOfCobotsPercent = cobotAmount;
            s.Init();


            int counter = 0;
            for (int i = 0; i < evaluation.WorkstationAssignment.Amount; i++)
            {
                evaluation.WorkstationAssignment.Value[i] = result[counter];
                counter++;
            }
            for (int i = 0; i < evaluation.Priority.Amount; i++)
            {
                evaluation.Priority.Value[i] = result[counter];
                counter++;
            }
            for (int i = 0; i < evaluation.CobotLocations.Amount; i++)
            {
                evaluation.CobotLocations.Value[i] = result[counter];
                counter++;
            }
            s.RunFinishEventOnly();
            if (s.SimulationStatistics.Fitness == simulationStatisticsFitness)
            {
                if (debugLog)
                    Console.WriteLine("Solution validation correct");
            }
        }
    }
}
