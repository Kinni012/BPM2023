using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.RealVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HEAL.Attic;

namespace HeuristicLab.Easy4Sim.Plugin
{  /// <summary>
    /// An operator which creates a new random real vector with each element uniformly distributed in a specified range.
    /// With this solution creator it is also possible to create int(example: from 5 to 10) and bool(0, 1) values.
    /// </summary>
    [Item("Easy4SimSolutionCreator", "An operator which creates a new random real vector with each element uniformly distributed in a specified range. Types (real, int, bool).")]
    [StorableType]
    public class Easy4SimSolutionCreator : RealVectorCreator, IStrategyParameterCreator
    {
        [StorableConstructor]
        protected Easy4SimSolutionCreator(StorableConstructorFlag deserializing) : base(deserializing) { }
        protected Easy4SimSolutionCreator(Easy4SimSolutionCreator original, Cloner cloner) : base(original, cloner) { }
        public Easy4SimSolutionCreator() : base() { }
        public override IDeepCloneable Clone(Cloner cloner)
        {
            return new Easy4SimSolutionCreator(this, cloner);
        }

        /// <summary>
        /// Generates a new random real vector with the given <paramref name="length"/> and in the interval [min,max) and the Type.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="length"/> is smaller or equal to 0.<br />
        /// Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.
        /// </exception>
        /// <remarks>
        /// Note that if <paramref name="min"/> is equal to <paramref name="max"/>, all elements of the vector will be equal to <paramref name="min"/>.
        /// </remarks>
        /// <param name="random">The random number generator.</param>
        /// <param name="length">The length of the real vector.</param>
        /// <param name="bounds">The lower and upper bound (1st and 2nd column) of the positions in the vector. Third column is used for type identification(real, int). If there are less rows than dimensions, the rows are cycled.</param>
        /// <returns>The newly created real vector.</returns>
        public static RealVector Apply(IRandom random, int length, DoubleMatrix bounds)
        {
            RealVector result = new RealVector(length);
            result.Randomize(random, length, bounds);
            return result;
        }


        /// <summary>
        /// Forwards the call to <see cref="Apply(IRandom, int, DoubleMatrix)"/>.
        /// </summary>
        /// <param name="random">The pseudo random number generator to use.</param>
        /// <param name="length">The length of the real vector.</param>
        /// <param name="bounds">The lower and upper bound (1st and 2nd column) of the positions in the vector. Third column is used for type identification(real, int). If there are less rows than dimensions, the rows are cycled.</param>
        /// <returns>The newly created real vector.</returns>
        protected override RealVector Create(IRandom random, IntValue length, DoubleMatrix bounds)
        {
            return Apply(random, bounds.Rows, bounds);
        }
    }
    public static class MyExtensions
    {
        public static RealVector Randomize(this RealVector array, IRandom random, int length, DoubleMatrix bounds)
        {
            if (length > 0)
            {
                for (int i = 0; i < length; i++)
                {
                    double type = 0;
                    if (bounds.Columns >= 3)
                    {
                       type = bounds[i % bounds.Rows, 2];
                    }

                    switch (type)
                    {
                        case 1:
                            //Type int 
                            if (int.TryParse(bounds[i % bounds.Rows, 0].ToString(), out int minInt) &&
                                int.TryParse(bounds[i % bounds.Rows, 1].ToString(), out int maxInt))
                            {
                                array[i] = random.Next(minInt, maxInt);
                            }
                            else
                            {
                                array[i] = 0;
                            }
                            break;

                        default:
                            // Default Type of the double matrix is double
                            double min = bounds[i % bounds.Rows, 0];
                            double max = bounds[i % bounds.Rows, 1];
                            array[i] = min + (max - min) * random.NextDouble();
                            break;
                    }

                }
            }
            return array;
        }
    }

    public enum SolutionCreatorTypes { DoubleValue, IntValue, BoolValue }
}
