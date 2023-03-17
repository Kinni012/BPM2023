using System.Collections.Generic;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses
{
    public class IntelligentChangeWorkstation
    {
        public string Machine { get; set; }

        public int TotalProcessingTime { get; set; }
        public List<int> OperationsIds { get; set; }
        public int Weight { get; set; }
        public IntelligentChangeWorkstation()
        {

        }
    }
}
