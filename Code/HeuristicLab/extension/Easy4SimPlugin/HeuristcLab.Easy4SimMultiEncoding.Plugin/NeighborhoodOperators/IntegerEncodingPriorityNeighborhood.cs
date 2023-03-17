using System.Collections.Generic;
using System.Linq;
using Easy4SimFramework;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators
{
    public static class IntegerEncodingPriorityNeighborhood
    {
        public static void BasicChange(MersenneTwister twister,                                    //Random number generator
            List<ParameterArrayOptimization<int>> encodingPartInformation,   //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation,                      //Stored bounds for the positions in the encoding
            IntegerVector currentSolution)                       //The current encoded solution
        {
            int index = encodingPartInformation[0].Amount + twister.Next(encodingPartInformation[1].Amount);
            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
        }

        public static void BasicChangeNoRepeat(MersenneTwister twister,      //Random number generator
            List<ParameterArrayOptimization<int>> encodingPartInformation,   //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation,                          //Stored bounds for the positions in the encoding
            IntegerVector currentSolution,                                   //The current encoded solution
            ref List<int> changedPositions)
        {

            int index = encodingPartInformation[0].Amount + twister.Next(encodingPartInformation[1].Amount);
            int counter = 0;
            while (changedPositions.Contains(index) && counter < 10)
            {
                counter++;
                index = encodingPartInformation[0].Amount + twister.Next(encodingPartInformation[1].Amount);
            }
            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
            changedPositions.Add(index);
        }

        /// <summary>
        /// Swap the priority of two operations.
        /// </summary>
        /// <param name="integerVector2"></param>
        /// <param name="amountOfChanges"></param>
        /// <param name="rnd"></param>
        public static void PrioritySwap(MersenneTwister twister, IntegerVector integerVector2, int amountOfChanges, IntegerVectorEncoding boundInformation)
        {
            int startPriority = 0;
            int endPriority = 0;
            //Find start of the priority encoding part
            for (int i = 0; i < boundInformation.Bounds.GetColumn(1).Count(); i++)
                if (boundInformation.Bounds[i, 1] == int.MaxValue)
                {
                    startPriority = i;
                    break;
                }

            //Find the end of the priority encoding part
            for (int i = boundInformation.Bounds.GetColumn(1).Count() - 1; i >= 0; i--)
                if (boundInformation.Bounds[i, 1] == int.MaxValue)
                {
                    endPriority = i;
                    break;
                }

            //Generate two different indexes in the range
            int range = endPriority - startPriority;
            int index1 = twister.Next(range);
            int index2 = twister.Next(range);
            if (index2 == index1)
            {
                for (int i = 0; i < 10; i++)
                {
                    index2 = twister.Next(range);
                    if (index2 != index1)
                        break;
                }
            }
            index1 = index1 + startPriority;
            index2 = index2 + startPriority;

            //Swap the values of the indexes
            int exchangeValue = integerVector2[index1];
            integerVector2[index1] = integerVector2[index2];
            integerVector2[index2] = exchangeValue;
        }

        /// <summary>
        /// Takes 2 positions in the priority part of the encoding and reverts all values between them
        /// </summary>
        /// <param name="integerVector2"></param>
        /// <param name="amountOfChanges"></param>
        /// <param name="rnd"></param>
        public static void PriorityExchange(MersenneTwister twister, IntegerVector integerVector2, int amountOfChanges, IntegerVectorEncoding boundInformation)
        {
            int startPriority = 0;
            int endPriority = 0;

            for (int i = 0; i < boundInformation.Bounds.GetColumn(1).Count(); i++)
                if (boundInformation.Bounds[i, 1] == int.MaxValue)
                {
                    startPriority = i;
                    break;
                }

            for (int i = boundInformation.Bounds.GetColumn(1).Count() - 1; i >= 0; i--)
                if (boundInformation.Bounds[i, 1] == int.MaxValue)
                {
                    endPriority = i;
                    break;
                }

            int range = endPriority - startPriority;

            int index1 = twister.Next(range - 1);
            int index2 = twister.Next(range - index1);
            index1 += startPriority;
            index2 += index1;

            List<int> values = new List<int>();
            //Get all values in the range between the random generated values
            for (int i = index1; i <= index2; i++)
                values.Add(integerVector2[i]);

            //Reverse all values between the two generated index variables
            int counter = 0;
            for (int i = index2; i >= index1; i--)
            {
                integerVector2[i] = values[counter];
                counter++;
            }
        }
    }
}
