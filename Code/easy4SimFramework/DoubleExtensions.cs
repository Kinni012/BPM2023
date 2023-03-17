using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easy4SimFramework
{
    public static class DoubleExtensions
    {
        public static bool IsAlmost(this double x, double y)
        {
            if (double.IsInfinity(x))
                return x > 0 ? double.IsPositiveInfinity(y) : double.IsNegativeInfinity(y);
            return Math.Abs(x - y) < 1.0E-12;
        }
    }
}
