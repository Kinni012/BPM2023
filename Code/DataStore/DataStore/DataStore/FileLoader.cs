using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DataStore
{
    public class FileLoader
    {
        /// <summary>
        /// Method that finds all files containing a specific text 
        /// </summary>
        /// <returns></returns>
        public List<string> FindAvailableFiles(string text)
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<string> result = new List<string>();
            try
            {
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.Contains(text) && !name.EndsWith(".dat"))
                    {
                        int index = name.IndexOf(text);
                        result.Add(name.Substring(index));
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return result;
        }


        /// <summary>
        /// Loads the file content of a file specified as parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string LoadFile(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "";
            try
            {
                //First try with name ending
                resourceName = assembly.GetManifestResourceNames()
                    .Single(str => str.EndsWith(name));
            }
            catch (Exception)
            {
                //Ignore error
            }
            //Second try without name ending, not the best style
            try
            {
                if (resourceName == "")
                    resourceName = assembly.GetManifestResourceNames().Single(x => x.Contains(name));

            }
            catch (Exception)
            {
                //Ignore error
            }
            //If file not found ==> return empty string
            if (resourceName == "")
                return "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }

        }

        public Stream GetResourceStream(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));
            return assembly.GetManifestResourceStream(resourceName);
        }

        public List<NamedStream> GetAllModelStreams()
        {
            List<NamedStream> result = new List<NamedStream>();
            var assembly = Assembly.GetExecutingAssembly();
            List<string> strings = assembly.GetManifestResourceNames().Where(x => x.StartsWith("DataStore.Data.easy4simModels.")).ToList();
            foreach (string s in strings)
            {
                string name = s.Replace("DataStore.Data.easy4simModels.", "");
                name = name.Replace(".dll", "");
                result.Add(new NamedStream(name, assembly.GetManifestResourceStream(s)));

            }

            return result;


        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public FileLoader() { }


        public static List<SALBP_WeckenborgData> AvailableInstancesForSize(string size)
        {
            List<SALBP_WeckenborgData> resultList = new List<SALBP_WeckenborgData>();
            var assembly = Assembly.GetExecutingAssembly();
            XSSFWorkbook workbook;
            XSSFWorkbook results;
            String allSolutions;
            #region constants
            //=============== Constant strings to avoid typing mistakes =====================
            const string smallString = "small";
            const string mediumString = "medium";
            const string largeString = "large";
            #endregion
            #region getWorkbooks
            //================ Set workbook ==========================
            string instanceResults = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("DataStore.Data.Weckenborg."))
                .FirstOrDefault(x => x.Contains("Gesamtauswertung"));
            using (Stream stream = assembly.GetManifestResourceStream(instanceResults))
            {
                results = new XSSFWorkbook(stream);
            }

            int amountOfLines = 0;

            switch (size)
            {
                case smallString:
                    {
                        amountOfLines = 20;
                        string instance = assembly.GetManifestResourceNames()
                            .Where(x => x.StartsWith("DataStore.Data.Weckenborg."))
                            .FirstOrDefault(x => x.Contains("20er"));
                        using (Stream stream = assembly.GetManifestResourceStream(instance))
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                    }
                    break;
                case mediumString:
                    {
                        amountOfLines = 50;
                        string instance = assembly.GetManifestResourceNames()
                            .Where(x => x.StartsWith("DataStore.Data.Weckenborg."))
                            .FirstOrDefault(x => x.Contains("50er"));
                        using (Stream stream = assembly.GetManifestResourceStream(instance))
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                    }
                    break;
                case largeString:
                    {
                        amountOfLines = 100;
                        string instance = assembly.GetManifestResourceNames()
                            .Where(x => x.StartsWith("DataStore.Data.Weckenborg."))
                            .FirstOrDefault(x => x.Contains("100er"));
                        using (Stream stream = assembly.GetManifestResourceStream(instance))
                        {
                            workbook = new XSSFWorkbook(stream);
                        }
                    }
                    break;
                default:
                    return null;
            }
            //=========================================================
            #endregion


            ISheet resultSheet = results.GetSheetAt(0);


            for (int i = 2; i < 1502; i++)
            {
                IRow row = resultSheet.GetRow(i);

                int instanceNumber = Convert.ToInt32(row.GetCell(0).ToString());
                int taskNumber = Convert.ToInt32(row.GetCell(1).ToString());
                if (taskNumber != amountOfLines)
                    continue;

                int numberOfStations = Convert.ToInt32(row.GetCell(2).ToString());
                int numberOfCobots = Convert.ToInt32(row.GetCell(3).ToString());
                int robotFlexibility = Convert.ToInt32(row.GetCell(4).ToString());
                int cooperativeFlexibility = Convert.ToInt32(row.GetCell(5).ToString());
                int bestResultWeckenborgGa = Convert.ToInt32(row.GetCell(6).ToString());
                int bestResultWeckenborgMip = Convert.ToInt32(row.GetCell(12).ToString());

                //We are in the correct sheet now 
                SALBP_WeckenborgData result = new SALBP_WeckenborgData(instanceNumber, taskNumber, numberOfStations, numberOfCobots, robotFlexibility, cooperativeFlexibility, bestResultWeckenborgGa, bestResultWeckenborgMip);


                for (int j = 0; j < workbook.NumberOfSheets; j++)
                {
                    ISheet currentSheet = workbook.GetSheetAt(j);
                    if (instanceNumber != Convert.ToInt32(currentSheet.GetRow(0).GetCell(1).ToString()))
                        continue;

                    if (taskNumber != Convert.ToInt32(currentSheet.GetRow(1).GetCell(1).ToString()))
                        continue;

                    if (numberOfStations != Convert.ToInt32(currentSheet.GetRow(2).GetCell(1).ToString()))
                        continue;

                    if (numberOfCobots != Convert.ToInt32(currentSheet.GetRow(3).GetCell(1).ToString()))
                        continue;

                    if (robotFlexibility != Convert.ToInt32(currentSheet.GetRow(7).GetCell(1).ToString()))
                        continue;

                    if (cooperativeFlexibility != Convert.ToInt32(currentSheet.GetRow(8).GetCell(1).ToString()))
                        continue;

                    result.CycleTime = Convert.ToInt32(currentSheet.GetRow(5).GetCell(1).ToString());
                    result.CycleTimeUpperLimit = Convert.ToInt32(currentSheet.GetRow(4).GetCell(1).ToString());
                    //While we are in the correct Sheet, load task times and precedence relations
                    for (int k = 0; k < amountOfLines; k++)
                    {
                        IRow taskRow = currentSheet.GetRow(11 + k);
                        SALBP_Task task = new SALBP_Task();
                        int taskId = Convert.ToInt32(taskRow.GetCell(0).ToString());
                        int taskDurationHuman = Convert.ToInt32(taskRow.GetCell(1).ToString());
                        int taskDurationRobot = Convert.ToInt32(taskRow.GetCell(2).ToString());
                        int taskDurationCollaborative = Convert.ToInt32(taskRow.GetCell(3).ToString());
                        task.TaskNumber = taskId;
                        task.HumanProductionTime = taskDurationHuman;
                        if (taskDurationRobot != 99999)
                            task.RobotProductionTime = taskDurationRobot;

                        if (taskDurationCollaborative != 99999)
                            task.CollaborativeProductionTime = taskDurationCollaborative;

                        IRow precedenceRow = currentSheet.GetRow(1 + k);
                        for (int l = 1; l <= amountOfLines; l++)
                        {
                            if (precedenceRow.GetCell(4 + l).ToString() == "1")
                            {
                                task.NextTasks.Add(l);
                            }
                        }
                        result.Tasks.Add(task);
                    }

                    foreach (SALBP_Task task in result.Tasks)
                        foreach (int nextTask in task.NextTasks)
                            result.Tasks.FirstOrDefault(x => x.TaskNumber == nextTask).PreviousTasks.Add(task.TaskNumber);


                    break;

                }

                resultList.Add((SALBP_WeckenborgData)result.Clone());
            }


            //===================== Add result 
            string instanceAllSolutions = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("DataStore.Data."))
                .FirstOrDefault(x => x.Contains("000_AllSolutions"));
            using (Stream stream = assembly.GetManifestResourceStream(instanceAllSolutions))
            {
                StreamReader reader = new StreamReader(stream);
                StringBuilder sb = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] lineParts = line.Split(new[] { ',' });
                    if (lineParts.Length >= 6 && lineParts[1].StartsWith("instance"))
                    {
                        try
                        {

                            //Get instance number 
                            string instanceIdentifier = lineParts[1].Replace("instance_n=", "");
                            int instanceSize = Convert.ToInt32(instanceIdentifier.Split('_')[0]);
                            int instanceId = Convert.ToInt32(instanceIdentifier.Split('_')[1]);
                            List<SALBP_WeckenborgData> dataList = resultList.Where(x =>
                                x.InstanceNumber == instanceId && x.TaskNumbers == instanceSize).ToList();
                            foreach (SALBP_WeckenborgData data in dataList)
                            {
                                if (int.TryParse(lineParts[6], out int bestAmountOfWorkstations))
                                {
                                    data.BestAmountOfWorkstations = bestAmountOfWorkstations;
                                }
                                data.Trickiness = lineParts[5];
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }


                    if (lineParts.Length >= 7 &&
                        lineParts[2].StartsWith("instance"))
                    {

                        try
                        {

                            //Get instance number 
                            string instanceIdentifier = lineParts[2].Replace("instance_n=", "");
                            int instanceSize = Convert.ToInt32(instanceIdentifier.Split('_')[0]);
                            if (!int.TryParse(instanceIdentifier.Split('_')[1], out int rightFormat))
                            {
                                continue;
                            }
                            int instanceId = Convert.ToInt32(instanceIdentifier.Split('_')[1]);
                            List<SALBP_WeckenborgData> dataList = resultList.Where(x =>
                                x.InstanceNumber == instanceId && x.TaskNumbers == instanceSize).ToList();
                            foreach (SALBP_WeckenborgData data in dataList)
                            {
                                if (int.TryParse(lineParts[7], out int bestAmountOfWorkstations))
                                {
                                    data.BestAmountOfWorkstations = bestAmountOfWorkstations;
                                }
                                data.Trickiness = lineParts[6];
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                allSolutions = sb.ToString();
            }

            List<SALBP_WeckenborgData> filteredResults = new List<SALBP_WeckenborgData>();
            if (size.ToLower() == "small")
            {
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 442));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 441));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 165));
            }


            if (size.ToLower() == "medium")
            {
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 455));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 454));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 53));

            }

            if (size.ToLower() == "large")
            {
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 451));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 328));
                filteredResults.AddRange(resultList.Where(x => x.InstanceNumber == 19));

            }


            return filteredResults;
        }


        public List<string> GetTestInstances(int amountOfEachSize)
        {
            List<string> result = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            List<string> strings = assembly.GetManifestResourceNames().Where(x => x.StartsWith("DataStore.Data.SALBP_Otto_2013.")).ToList();

            result.AddRange(strings.Where(x => x.Contains("small")) //Instance size
                .OrderBy(x => Guid.NewGuid()) //Order Randomly
                .Take(amountOfEachSize) //Take amount specified
                .Select(x => $"{x.Split(new[] { '.' }).Reverse().Skip(1).First()}.{x.Split(new[] { '.' }).Reverse().First()}") //Return file name 
                .ToArray());

            //result.AddRange(strings.Where(x => x.Contains("medium") && !x.Contains("permuted")) //Instance size
            //    .OrderBy(x => Guid.NewGuid()) //Order Randomly
            //    .Take(amountOfEachSize) //Take amount specified
            //    .Select(x => $"{x.Split(new[] { '.' }).Reverse().Skip(1).First()}.{x.Split(new[] { '.' }).Reverse().First()}") //Return file name 
            //    .ToArray());

            //result.AddRange(strings.Where(x => x.Contains("large") && !x.Contains("very")) //Instance size
            //    .OrderBy(x => Guid.NewGuid()) //Order Randomly
            //    .Take(amountOfEachSize) //Take amount specified
            //    .Select(x => $"{x.Split(new[] { '.' }).Reverse().Skip(1).First()}.{x.Split(new[] { '.' }).Reverse().First()}") //Return file name 
            //    .ToArray());

            //result.AddRange(strings.Where(x => x.Contains("very_large")) //Instance size
            //    .OrderBy(x => Guid.NewGuid()) //Order Randomly
            //    .Take(amountOfEachSize) //Take amount specified
            //    .Select(x => $"{x.Split(new[] { '.' }).Reverse().Skip(1).First()}.{x.Split(new[] { '.' }).Reverse().First()}") //Return file name 
            //    .ToArray());

            return result;
        }
    }
}
