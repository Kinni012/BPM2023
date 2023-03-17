using System;
using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;

namespace Easy4SimFramework
{
    [StorableType("289B896F-F362-44E0-835E-15CDAE62D595")]
    public class SimulationStatistics : ICloneable<SimulationStatistics>
    {

        [Storable]
        public double Fitness { get; set; }
        [Storable]
        public Dictionary<int, double> FitnessPerComponent { get; set; }
        [Storable]
        public long ExecutionTime { get; set; }
        [Storable]
        public Dictionary<int, long> ExecutionTimePerComponent { get; set; }

        public List<FitnessElement> FitnessElements
        {
            get
            {
                List<FitnessElement> result = new List<FitnessElement>();
                foreach (KeyValuePair<int, double> keyValuePair in FitnessPerComponent)
                {
                    result.Add(new FitnessElement(){Id=keyValuePair.Key, Fitness = keyValuePair.Value});
                }

                foreach (KeyValuePair<int, long> keyValuePair in ExecutionTimePerComponent)
                {
                    bool found = false;
                    foreach (FitnessElement element in result)
                    {
                        if (element.Id == keyValuePair.Key)
                        {
                            element.Time = keyValuePair.Value;
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                        result.Add(new FitnessElement() { Id = keyValuePair.Key, Time = keyValuePair.Value});
                }

                int id = 1;
                foreach (FitnessElement element in result.OrderBy(x => x.Fitness))
                {
                    element.FitnessRank = id;
                    id++;
                }
                id = 1;
                foreach (FitnessElement element in result.OrderBy(x => x.Time))
                {
                    element.TimeRank = id;
                    id++;
                }

                return result;
            }
        }

        [Storable]
        public int BestSolutionFoundSoFarGa { get; set; }
        [Storable]
        public string ProcessMiningInformation { get; set; }
        [Storable]
        public string DataSet { get; set; }
        [Storable]
        public int AmountOfCobots { get; set; }
        [Storable]
        public int AmountOfWorkstations { get; set; }
        [Storable]
        public List<string> Workstations { get; set; }
        [Storable]
        public double AmountOfCobotsPercent { get; set; }

        public string VnsNeighborhood { get; set; }
        public static bool ApplyVNS { get; set; }
        public static int ReadCobots { get; set; }
        public static string ReadDataSet { get; set; }
        public static int LowerLimitTime { get; set; } = -1;
        public static int UpperLimitTime { get; set; } = -1;
        public static double UpperLimitCost { get; set; } = -1;
        public static double LowerLimitCost { get; set; } = -1;
        public static DateTime LastFileDate { get; set; }
        public static List<string> FixedCobots { get; set; }
        public static long EvaluatedSolutions { get; set; }
        public static long EvaluatedVnsSolutions { get; set; }
        [StorableConstructor]
        public SimulationStatistics(StorableConstructorFlag flag) { }

        public SimulationStatistics()
        {
            Fitness = 0;
            ExecutionTime = 0;
            BestSolutionFoundSoFarGa = int.MaxValue;
            ExecutionTimePerComponent = new Dictionary<int, long>();
            FitnessPerComponent = new Dictionary<int, double>();
            DataSet = "";
            AmountOfCobots = 0;
            Workstations = new List<string>();
        }

        public SimulationStatistics Clone()
        {
            SimulationStatistics result = new SimulationStatistics { 
                Fitness = Fitness, 
                ExecutionTime = ExecutionTime, 
                BestSolutionFoundSoFarGa = BestSolutionFoundSoFarGa,
                ProcessMiningInformation = ProcessMiningInformation,
                DataSet = DataSet,
                AmountOfCobots = AmountOfCobots,
                AmountOfWorkstations = AmountOfWorkstations,
                AmountOfCobotsPercent = AmountOfCobotsPercent,
                VnsNeighborhood = VnsNeighborhood
            };
            result.Workstations = new List<string>();
            foreach (string workstation in Workstations)
            {
                result.Workstations.Add(workstation);
            }
            foreach (KeyValuePair<int, long> pair in ExecutionTimePerComponent)
                result.ExecutionTimePerComponent.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<int, double> pair in FitnessPerComponent)
                result.FitnessPerComponent.Add(pair.Key, pair.Value);
            return result;
        }
    }

    public class FitnessElement
    {
        public double Fitness { get; set; }
        public long Time { get; set; }
        public int Id { get; set; }
        public int FitnessRank { get; set; }
        public int TimeRank { get; set; }
        public FitnessElement()
        {
                
        }
    }
}
