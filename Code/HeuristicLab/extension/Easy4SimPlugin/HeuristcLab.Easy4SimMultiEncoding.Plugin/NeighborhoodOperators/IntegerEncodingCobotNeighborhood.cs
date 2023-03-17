using System;
using System.Collections.Generic;
using System.Linq;
using Easy4SimFramework;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators
{
    public static class IntegerEncodingCobotNeighborhood
    {
        public static void BasicChange(MersenneTwister twister, //Random number generator
            List<ParameterArrayOptimization<int>>
                encodingPartInformation, //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation, //Stored bounds for the positions in the encoding
            IntegerVector currentSolution) //The current encoded solution
        {
            int index = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                        twister.Next(encodingPartInformation[2].Amount);
            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
        }

        public static void BasicChangeNoRepeat(MersenneTwister twister, //Random number generator
            List<ParameterArrayOptimization<int>>
                encodingPartInformation, //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation, //Stored bounds for the positions in the encoding
            IntegerVector currentSolution, //The current encoded solution
            ref List<int> changedPositions)
        {

            int index = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                        twister.Next(encodingPartInformation[2].Amount);
            int counter = 0;
            while (changedPositions.Contains(index) && counter < 10)
            {
                counter++;
                index = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                        twister.Next(encodingPartInformation[2].Amount);
            }

            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
            changedPositions.Add(index);
        }

        public static void ProcessMiningChangeV2(MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            IntegerVector integerEncodedSolution,
            List<ParameterArrayOptimization<int>> encodingPartInformation)
        {
            try
            {


                int index = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                            twister.Next(encodingPartInformation[2].Amount);
                int cobotsStart = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount;
                int cobotsEnd = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                                encodingPartInformation[2].Amount;
                Dictionary<int, bool> normalWorkstations = new Dictionary<int, bool>();
                Dictionary<int, bool> cobotLocations = new Dictionary<int, bool>();
                for (int i = 1; i <= minedWorkstations.Count; i++)
                {
                    normalWorkstations.Add(i, false);
                }

                //Find out current located cobots
                for (int i = cobotsStart; i < index; i++)
                {
                    int index2 = integerEncodedSolution[i];
                    int key = normalWorkstations.ElementAt(index2).Key;
                    cobotLocations.Add(key, true);
                    normalWorkstations.Remove(key);
                }

                Dictionary<int, int> generateNewCobotLocation = new Dictionary<int, int>();
                foreach (KeyValuePair<int, int> pair in minedWorkstations)
                {
                    if (cobotLocations.ContainsKey(pair.Key))
                        continue;
                    generateNewCobotLocation.Add(pair.Key, pair.Value);
                }

                int sum = generateNewCobotLocation.Values.Sum();
                int randomNewValue = twister.Next(sum) + 1;
                int sum2 = 0;
                for (int i = 0; i < generateNewCobotLocation.Count; i++)
                {
                    sum2 += generateNewCobotLocation.ElementAt(i).Value;
                    if (sum2 >= randomNewValue)
                    {
                        integerEncodedSolution[index] = i;
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error in ProcessMiningChangeV2");
                Console.WriteLine("integerEncodedSolution length: " + integerEncodedSolution.Length);
                throw;
            }
        }
    }
}
