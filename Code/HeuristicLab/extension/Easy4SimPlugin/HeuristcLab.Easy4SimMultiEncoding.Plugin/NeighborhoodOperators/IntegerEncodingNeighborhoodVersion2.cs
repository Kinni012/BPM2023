using System.Collections.Generic;
using Easy4SimFramework;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators
{
    public static class IntegerEncodingNeighborhoodVersion2
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
                double d = twister.NextDouble() * 100;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 90;
                }


                if (d < 30)
                    IntegerEncodingWorkstationAssignmentNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else if (d < 90)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
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
                double d = twister.NextDouble() * 100;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 90;
                }
                if (d < 30)
                    IntegerEncodingWorkstationAssignmentNeighborhood.GreedyChange(twister, integerEncodedSolution, integerEncoding, solver, integerInformation);
                else if (d < 90)
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
                double d = twister.NextDouble() * 100;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 90;
                }

                if (d < 30)
                {
                    IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningChange(twister, miningResult,
                        integerEncodedSolution, solver, integerInformation);
                }
                else if (d < 90)
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
            for (int i = 0; i < amountOfChanges; i++)
            {
                double d = twister.NextDouble() * 100;
                if (integerInformation[2].Amount == 0)
                {
                    d = twister.NextDouble() * 90;
                }
                if (d < 30)
                    IntegerEncodingWorkstationAssignmentNeighborhood.ProcessMiningDictionaryChange(twister, minedWorkstations, integerEncodedSolution, solver, integerInformation);
                else if (d < 90)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.ProcessMiningChangeV2(twister, minedWorkstations, integerEncodedSolution, integerInformation);
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
                double d = integerInformation[2].Amount == 0 ? twister.NextDouble() * 90 : twister.NextDouble() * 100;
                if(d < 30)
                    IntegerEncodingWorkstationAssignmentNeighborhood.Experiment1(twister, minedWorkstations, integerEncodedSolution, integerEncoding, solver, integerInformation);
                else if (d < 90)
                    IntegerEncodingPriorityNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);
                else
                    IntegerEncodingCobotNeighborhood.BasicChange(twister, integerInformation, integerEncoding, integerEncodedSolution);


            }
        }
    }
}
