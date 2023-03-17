using System;
using System.Collections.Generic;
using Easy4SimFramework;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators
{
    public static class IntegerEncodingNeighborhood
    {

        /// <summary>
        /// Basic change that changes x positions of a given vector to a value within its bounds
        /// 33% Chance for each part of the encoding to be changed if cobots are allowed to place
        /// 50% Chance without cobots
        /// </summary>
        public static void BasicChange(int amountOfChanges,
                                        MersenneTwister twister,
                                        List<ParameterArrayOptimization<int>> integerInformation,
                                        IntegerVectorEncoding integerEncoding,
                                        IntegerVector integerEncodedSolution)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }


                if (d < 1)
                    IntegerEncodingWorkstationAssignmentNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
            }
        }

        /// <summary>
        /// Same as the basic change, however, it is made sure, that the same position is not changed multiple times
        /// </summary>
        public static void BasicChangeNoRepeat(int amountOfChanges,
                                               MersenneTwister twister,
                                               List<ParameterArrayOptimization<int>> integerInformation,
                                               IntegerVectorEncoding integerEncoding,
                                               IntegerVector integerEncodedSolution)
        {

            List<int> changedPositions = new List<int>();

            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }
                if (d < 1)
                    IntegerEncodingWorkstationAssignmentNeighborhood.BasicChangeNoRepeat(twister, integerInformation, integerEncoding, integerEncodedSolution, ref changedPositions);
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChangeNoRepeat(twister, integerInformation, integerEncoding, integerEncodedSolution, ref changedPositions);
                else
                    IntegerEncodingCobotNeighborhood.BasicChangeNoRepeat(twister, integerInformation, integerEncoding, integerEncodedSolution, ref changedPositions);
            }
        }

        /// <summary>
        /// Greedy assignment of the task to workstation assignment
        /// Rest is similar to the basic change
        /// </summary>
        public static void GreedyChange(int amountOfChanges,
            MersenneTwister twister,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }
                if (d < 1)
                    IntegerEncodingWorkstationAssignmentNeighborhood.GreedyChange(twister, integerEncodedSolution, integerEncoding, solver, integerInformation);
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
            }
        }

        /// <summary>
        /// Utilize a mined local process model for the changes
        /// 33% chance for each part in case of cobots
        /// 50% chance in case no cobots are assignable
        /// the task to workstation assignment is split in basic change and process mining change to equal parts
        /// </summary>
        public static void ProcessMiningChange(int amountOfChanges,
            MersenneTwister twister,
            IntermediateLocalProcessModelResult miningResult,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }

                if (d < 1)
                {
                    IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningChange(twister, miningResult,
                        integerEncodedSolution, solver, integerInformation);
                }
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
            }
        }
        public static void ProcessMiningChangeV2(int amountOfChanges,
            MersenneTwister twister,
            IntermediateLocalProcessModelResult miningResult,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }

                if (d < 1)
                {
                    IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningChange(twister, miningResult,
                        integerEncodedSolution, solver, integerInformation);

                }
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
            }
        }
        public static void ProcessMiningChangeDictionaryV1(int amountOfChanges,
            MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            try
            {
                for (int i = 0; i < amountOfChanges; i++)
                {
                    double d = twister.NextDouble() * 2.5;
                    if (integerInformation[2].Amount == 0)
                    {
                        d = twister.NextDouble() * 2;
                    }

                    if (d < 1)
                        IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningDictionaryChange(twister,
                            minedWorkstations, integerEncodedSolution, solver, integerInformation);
                    else if (d < 2)
                        IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding,
                            integerEncodedSolution);
                    else
                        IntegerEncodingCobotNeighborhood.ProcessMiningChangeV2(twister, minedWorkstations,
                            integerEncodedSolution, integerInformation);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error in ProcessMiningChangeDictionaryV1");
                throw e;
            }
        }
        public static void ProcessMiningChangeDictionaryV2(int amountOfChanges,
            MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 3;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 2;
                }
                if (d < 1)
                    IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningDictionaryChange(twister, minedWorkstations, integerEncodedSolution, solver, integerInformation);
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
            }
        }

        public static void ProcessMiningExperiment1(int amountOfChanges,
            MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            List<ParameterArrayOptimization<int>> integerInformation,
            IntegerVectorEncoding integerEncoding,
            IntegerVector integerEncodedSolution,
            Solver solver)
        {
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = integerInformation[2].Amount == 0 ? twister.NextDouble() * 2 : twister.NextDouble() * 3;
                if (d < 1)
                    IntegerEncodingWorkstationAssignmentNeighborhood.Experiment1(twister, minedWorkstations, integerEncodedSolution, integerEncoding, solver, integerInformation);
                else if (d < 2)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);


            }
        }
    }
}
