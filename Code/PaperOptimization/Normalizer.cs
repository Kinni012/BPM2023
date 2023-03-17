using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperOptimization
{
    public static class Normalizer
    {
        /// <summary>
        /// Static constructor that initializes available types and data sets
        /// </summary>
        static Normalizer()
        {
            Types = new List<string>() { "Real", "Int" };
            DataSets = new List<string>() { "Full.xml", "Half1.xml", "Half2.xml", "Quarter1.xml", "Quarter2.xml", "Quarter3.xml", "Quarter4.xml" };
            for (int i = 1; i <= 50; i++)
            {
                DataSets.Add(i.ToString().PadLeft(2, '0') + ".xml");
            }

        }
        /// <summary>
        /// Available encoding types
        /// </summary>
        public static List<string> Types = null;
        /// <summary>
        /// Available data sets
        /// </summary>
        public static List<string> DataSets = null;

        /// <summary>
        /// Map user input to optimization types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FindCorrectType(string type)
        {
            string result = "";
            if (type.ToUpper() == "REAL" || type.ToUpper() == "R")
                result = "Real";
            if (type.ToUpper() == "INT" || type.ToUpper() == "I")
                result = "Int";
            if (type.ToUpper() == "ALL" || type.ToUpper() == "A")
                result = "All";
            return result;
        }

        /// <summary>
        /// This method maps user input to specific data sets
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static string FindCorrectDataSet(string dataSet)
        {
            string result = "";
            if (dataSet.ToUpper() == "ALL" || dataSet.ToUpper() == "A")
            {
                result = "All";
                return result;
            }

            if (dataSet.ToUpper().Contains("CP"))
            {
                dataSet = dataSet.Remove(0, 2);
                result = Convert.ToInt32(dataSet).ToString().PadLeft(2, '0') + ".xml";
               
                return result;
            }


            if (dataSet.ToUpper() == "FULL" || dataSet.ToUpper() == "F")
            {
                result = "Full.xml";
                return result;
            }

            if (dataSet.ToUpper().Contains("H") || dataSet.ToUpper().Contains("HALF"))
            {
                result += "Half";
                if (dataSet.Contains("1"))
                    result += "1";
                if (dataSet.Contains("2"))
                    result += "2";
                result += ".xml";
            }

            if (dataSet.ToUpper().Contains("Q") || dataSet.ToUpper().Contains("QUARTER"))
            {
                result += "Quarter";
                if (dataSet.Contains("1"))
                    result += "1";
                if (dataSet.Contains("2"))
                    result += "2";
                if (dataSet.Contains("3"))
                    result += "3";
                if (dataSet.Contains("4"))
                    result += "4";
                result += ".xml";
            }


            return result;
        }

        /// <summary>
        /// Make sure that only available data sets get passed to the optimization
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<string> GetValidDataSets(string dataSet)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < DataSets.Count; i++)
            {
                if (dataSet == DataSets[i] || dataSet == "All")
                    result.Add(DataSets[i]);
            }
            return result;
        }

        public static List<string> GetValidTypes(string type)
        {
            List<string> result = new List<string>();
            if (type == Types[0] || type == "All")
                result.Add(Types[0]);
            if (type == Types[1] || type == "All")
                result.Add(Types[1]);
            return result;
        }
    }
}
