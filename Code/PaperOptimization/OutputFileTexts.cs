using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperOptimization
{
    public class OutputFileTexts
    {
        public string MiningInformation { get; set; }
        public string MiningInformationResource { get; set; }
        public string BestCost { get; set; }
        public string BestMakespan{ get; set; }


        public List<string> ResourceInformations { get; set; } = new List<string>();


        public OutputFileTexts(string miningInformation, string miningInformationResource, string bestCost, string bestMakespan)
        {
            MiningInformation = miningInformation;
            MiningInformationResource = miningInformationResource;
            BestCost = bestCost;
            BestMakespan = bestMakespan;


            if (MiningInformationResource.Contains("======================================"))
            {
                string[] parts = MiningInformationResource.Split(new[] {"======================================" + Environment.NewLine},
                    StringSplitOptions.RemoveEmptyEntries);

                ResourceInformations = parts.ToList();
            }
        }
    }
}
