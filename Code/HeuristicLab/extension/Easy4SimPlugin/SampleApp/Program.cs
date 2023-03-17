using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Clients.Hive;
using HeuristicLab.Data;
using HeuristicLab.Easy4SimMultiEncoding.Plugin;
using HeuristicLab.HiveProblem;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Easy4SimRealEncodingProblem problem = new Easy4SimRealEncodingProblem();
            Easy4SimFileValue val = new Easy4SimFileValue();
            val.Value = "C:\\develop\\easy4sim\\src\\DemoExcel\\QueueingSystem\\Presentation2\\Hl_optimization.xlsx";
            problem.Easy4SimEvaluationScript = val;

            try
            {

                IEnumerable<Type> usedTypes;
                byte[] jobByteArray = PersistenceUtil.Serialize(problem, out usedTypes);
                Console.WriteLine("Serializaton worked");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
