using System;
using System.Collections.Generic;
using System.Text;
using FjspOptimization.Data;

namespace FjspOptimization
{
    /// <summary>
    /// Simple class that reads in parameters given as console arguments or as user input.
    /// </summary>
    public static class HelperFunctions
    {
        public static ConsoleArguments GetArgumentsFromUserOrConsole(List<string> arguments)
        {//=========================== User input ==============================================
            foreach (string argument in arguments)
            {
                if (argument.ToUpper().Contains("HELP"))
                {
                    Console.WriteLine("Provide the following parameters to the command line:" + System.Environment.NewLine);
                    Console.WriteLine("DataSet: B1, B2...");
                    Console.WriteLine("Runtime in minutes");
                    Console.WriteLine("Number of repetitions");
                    Console.WriteLine("Time factor in objective function, e.g. 1");
                    Console.WriteLine("Cost factor in objective function, e.g. 0");
                    Console.WriteLine("Use variable neighborhood search? 0/1(no/yes)");
                    Console.WriteLine("Minimize or maximize: 0/1(0 == minimize, 1 == maximize)");
                    Console.WriteLine("Use normalized objective function: 0 == false, 1 == true");
                    Environment.Exit(0);
                }
            }
            ConsoleArguments result = new ConsoleArguments();

            //========================= Data set ===============================
            int parameterCounter = 0;
            result.DataSet = ReadDataSet(arguments, ref parameterCounter);


            //========================= Stopping criterium ===============================
            result.StoppingCriteria = ReadCriterium(arguments, ref parameterCounter);
            result.Criteria = ReadCriterium2(arguments, result.StoppingCriteria, ref parameterCounter);

            //========================= VNS ===============================

            result.ApplyVns = ReadApplyVns(arguments, ref parameterCounter);

            result.AmountOfCobots = ReadCobotPercentage(arguments, ref parameterCounter);

            //========================= Minimize or maximize ===============================
            Console.WriteLine("Minimize or maximize (0 => minimize, 1 maximize):");
            result.MinimizeOrMaximize = 0;
            Console.WriteLine("Minimize or maximize: " + result.Maximization);

            //========================= Normalize ===============================
            Console.WriteLine("Use normalized value (0 == false, 1== true)?");
            result.UseNormalizedValue = false;
            Console.WriteLine("Normalization: " + result.UseNormalizedValue);
     

            result.VnsPercentage = ReadVnsPercentage(arguments, ref parameterCounter);

            result.VnsNeighborhood = ReadNeighborhood(arguments, ref parameterCounter);

           
            return result;
        }

        private static int ReadNeighborhood(List<string> arguments, ref int parameterCounter)
        {
            Console.WriteLine("Insert VNS neighborhood: 0 - x");
            while (true)
            {
                if (arguments.Count > parameterCounter)
                    if (int.TryParse(arguments[parameterCounter], out int vnsNeighborhood))
                    {
                        Console.WriteLine("Neighborhood:" + vnsNeighborhood);
                        parameterCounter++;
                        return vnsNeighborhood;
                    }

                string input = Console.ReadLine();
                if (int.TryParse(input, out int vnsNeighborhood2))
                {
                    Console.WriteLine("Neighborhood:" + vnsNeighborhood2);
                    return vnsNeighborhood2;
                }
            }
        }

        private static double ReadVnsPercentage(List<string> arguments, ref int parameterCounter)
        {
            Console.WriteLine("Insert vns percentage, how close a solution must be to the best to apply vns to it, e.g.: 0.1 => 10%:");
            while (true)
            {
                if (arguments.Count > parameterCounter)
                    if (double.TryParse(arguments[parameterCounter], out double vnsPercentage))
                    {
                        Console.WriteLine(vnsPercentage * 100 + "%");
                        parameterCounter++;
                        return vnsPercentage;
                    }

                string input = Console.ReadLine();
                if (double.TryParse(input, out double vnsPercentage2))
                {
                    Console.WriteLine(vnsPercentage2 * 100 + "%");
                    return vnsPercentage2;
                }
            }
        }

        private static double ReadCobotPercentage(List<string> arguments, ref int parameterCounter)
        {
            Console.WriteLine("Cobot percentage e.g.: 0.2 = 20%:");
            while (true)
            {
                if (arguments.Count > parameterCounter)
                    if (double.TryParse(arguments[parameterCounter], out double parsedCobotsArgs))
                    {
                        parameterCounter++;
                        Console.WriteLine(parsedCobotsArgs * 100 + "%");
                        return parsedCobotsArgs;

                    }

                string inpu = Console.ReadLine();
                if (double.TryParse(inpu, out double parsedCobots))
                {
                    Console.WriteLine(parsedCobots);
                    return parsedCobots;
                }
            }
        }

        private static bool ReadApplyVns(List<string> arguments, ref int parameterCounter)
        {
            Console.WriteLine("Apply vns (0 = false, 1 = true):");
            while (true)
            {
                if (arguments.Count > parameterCounter)
                    if (int.TryParse(arguments[parameterCounter], out int parsedVns))
                    {
                        if (parsedVns == 1)
                        {
                            parameterCounter++;
                            Console.WriteLine("True");
                            return true;
                        }
                        else if (parsedVns == 0)
                        {
                            parameterCounter++;
                            Console.WriteLine("False");
                            return false;
                        }
                    }

                string input = Console.ReadLine();
                if (int.TryParse(input, out int parsedApplyVns))
                {
                    if (parsedApplyVns == 1)
                    {
                        Console.WriteLine("True");
                        return true;
                    }
                    else if (parsedApplyVns == 0)
                    {
                        Console.WriteLine("False");
                        return false;
                    }
                }
            }
        }

        private static int ReadCriterium2(List<string> arguments, string resultStoppingCriteria, ref int parameterCounter)
        {
            int criterium = 0;
            while (criterium == 0)
            {
                if (resultStoppingCriteria == "generations")
                    Console.WriteLine("Insert generations:");
                //if (generationsOrTime == "time")
                //    Console.WriteLine("Insert time in milliseconds:");
                if (resultStoppingCriteria == "time")
                    Console.WriteLine("Insert run time in minutes:");


                if (arguments.Count > parameterCounter)
                    if (int.TryParse(arguments[parameterCounter], out int convertedArgument))
                        criterium = convertedArgument * 60000;
                if (criterium != 0)
                {
                    Console.WriteLine(criterium / 60000);
                    break;
                }

                string input = Console.ReadLine();
                //criterium = Convert.ToInt32(input);
                if (int.TryParse(input, out int value))
                    criterium = value * 60000;
            }

            parameterCounter++;

            return criterium;
        }

        private static string ReadCriterium(List<string> arguments, ref int parameterCounter)
        {
            string generationsOrTime = "";
            while (generationsOrTime != "generations" && generationsOrTime != "time")
            {
                //Console.WriteLine("Run for amount of generations or time (generations, time):");
                //generationsOrTime = Console.ReadLine();
                //Console.WriteLine("For faster starting time is selected");
                generationsOrTime = "time";
            }

            return generationsOrTime;
        }

        private static string ReadDataSet(List<string> arguments, ref int parameterCounter)
        {
            string dataSet = "";
            while (dataSet == "")
            {
                Console.WriteLine("Insert data set (B1-B10 => Brandimarte L1-L40 =>Lawrence, BB => BBDHi_, BBD => TestL_BBDHi_):");

                if (arguments.Count > parameterCounter)
                    dataSet = arguments[parameterCounter];
                dataSet = FindCorrectDataSet(dataSet);
                if (dataSet != "")
                {
                    Console.WriteLine(dataSet);
                    parameterCounter++;
                    return dataSet;
                }

                dataSet = Console.ReadLine();
                dataSet = FindCorrectDataSet(dataSet);
                Console.WriteLine("Selected:" + dataSet);
            }

            return dataSet;
        }

        private static string FindCorrectDataSet(string dataSet)
        {
            StringBuilder result = new StringBuilder();
            if (dataSet.ToUpper().Contains("B") && !dataSet.ToUpper().Contains("BB")) //=> Brandimarte data set
            {
                result.Append("Mk");
                string number = dataSet.Substring(1);
                string numberString = number.ToString().PadLeft(2, '0');
                result.Append(numberString);
                result.Append(".fjs");
                return result.ToString();
            }
            if (dataSet.ToUpper().Contains("L"))
            {
                string number = dataSet.Substring(1);
                result.Append("la");
                string numberString = number.ToString().PadLeft(2, '0');
                result.Append(numberString);
                result.Append(".fjs");
                return result.ToString();
            }

            if (dataSet.ToUpper().Contains("BB") && !dataSet.ToUpper().Contains("BBD"))
            {
                result.Append("BBDHi_");
                string number = dataSet.Substring(2);
                string numberString = number.ToString().PadLeft(2, '0');
                result.Append(numberString);
                result.Append(".txt");
                return result.ToString();
            }
            if (dataSet.ToUpper().Contains("BBD"))
            {
                result.Append("TestL_BBDHi_");
                result.Append(dataSet.Substring(3));
                result.Append(".txt");
                return result.ToString();
            }
            return result.ToString();

        }
    }
}
