using System;
using System.Collections.Generic;
using Easy4SimFramework;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin
{
    public static class GenericProblemMethods
    {

        /// <summary>
        /// Simple method that returns the amount of changes
        /// Loop 0: 1
        /// Loop 1: 3
        /// Loop 2: 5
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int SetAmountOfChanges(int i) => (i * 2) + 1;

        /// <summary>
        /// Is the current run close enough to the best run to apply the vns?
        /// </summary>
        /// <param name="Maximization"></param>
        /// <param name="currentRunFitness"></param>
        /// <param name="BestSolutionFoundsoFar"></param>
        /// <param name="VnsPercentage"></param>
        /// <returns></returns>
        public static bool InRangeForVns(bool Maximization, double currentRunFitness, double BestSolutionFoundsoFar, double VnsPercentage)
        {
            bool vns;
            if (Maximization)
                vns = currentRunFitness >= BestSolutionFoundsoFar - VnsPercentage * Math.Abs(BestSolutionFoundsoFar);
            else
                vns = currentRunFitness <= BestSolutionFoundsoFar + VnsPercentage * Math.Abs(BestSolutionFoundsoFar);
            return vns;
        }


        public static List<Solver> CloneSolvers(Solver solver, int amountOfStochasticSolvers, bool deterministicOrStochastic)
        {
            List < Solver > result = new List<Solver>();
            if (!deterministicOrStochastic)
                result.Add((Solver)solver.Clone());
            else
                for (int j = 0; j < amountOfStochasticSolvers; j++)
                    result.Add((Solver)solver.Clone());
            return result;
        }


        public static double GetMakespanOfSolvers(List<Solver> solvers)
        {
            double result = 0;
            foreach (Solver solver in solvers)
                result += solver.Environment.SimulationTime;

            result /= solvers.Count;

            return result;
        }


        public static double GetCostsOfSolvers(List<Solver> solvers)
        {
            double result = 0;
            foreach (Solver solver in solvers)
                result += solver.SimulationStatistics.Fitness;

            result /= solvers.Count;
            return result;
        }
    }
}
