using Easy4SimFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FjspEasy4SimLibrary
{
    /// <summary>
    /// Loads a existing FJSSP to pass it to the FjspEvaluation
    /// </summary>
    public class FjspLoader : CSimBase
    {
        /// <summary>
        /// Content of the FJSSP file
        /// </summary>
        public ParameterString FileContent { get; set; }
        /// <summary>
        /// Output to the FjspEvaluation
        /// </summary>
        public OutEventObject ReadData { get; set; }

        #region constructor
        /// <summary>
        /// Do not use the default constructor
        /// </summary>
        public FjspLoader() => InitializeParameters();

        /// <summary>
        /// Constructor for the simulation framework
        /// </summary>
        /// <param name="id">Id in the simulation</param>
        /// <param name="name">Unique name in the simulation</param>
        /// <param name="settings">Settings where the object is inserted to</param>
        public FjspLoader(long id, string name, SolverSettings settings) : base(id, name, settings) => InitializeParameters();
        #endregion constructor

        /// <summary>
        /// Initialize the Framework parameters and connections.
        /// </summary>
        private void InitializeParameters()
        {
            FileContent = new ParameterString("");
            ReadData = new OutEventObject(this);
        }

        /// <summary>
        /// The FJSSP data is read in the initialize method.
        /// By doing it this way, the problem data only needs to be read once.
        /// After reading it the solver with the initialized problem can be cloned.
        /// </summary>
        public override void Initialize()
        {
            FlexibleJobShopSchedulingData readData = new FlexibleJobShopSchedulingData();
            //Support linux and windows file ending
            List<string> lines = FileContent.Value.Split(new [] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            
            if (lines.Count > 0) //Metadata from first line
            {
                string[] parts = lines[0].Split().Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (parts.Length > 0)
                {
                    if (int.TryParse(parts[0], out int numberOfJobs))
                        readData.NumberOfJobs = numberOfJobs;
                    else 
                        Console.WriteLine("FjspLoader: Error reading metadata position 0 " + parts[0]);
                }
                if (parts.Length > 1)
                {
                    if (int.TryParse(parts[1], out int numberOfMachines))
                        readData.NumberOfMachines = numberOfMachines;
                    else
                        Console.WriteLine("FjspLoader: Error reading metadata position 1 " + parts[1]);
                }
                if (parts.Length > 2)
                {
                    if (int.TryParse(parts[2], out int averageNumberOfMachinesPerJob))
                        readData.AverageNumberOfMachinesPerJob = averageNumberOfMachinesPerJob;
                    else
                        Console.WriteLine("FjspLoader: Error reading metadata position 2 " + parts[2]);
                }

            }
            int operationId = 0;
            if (lines.Count > 1) //At least one job
            {
                int lineNumber = 1;
                bool lineEmpty = true;
                List<string> parts = lines[lineNumber].Split(new [] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (parts.Count > 0)
                    lineEmpty = false;

                int jobId = 0;
                while (!lineEmpty) //We got a line to parse
                {
                    Job job = new Job();
                    job.Id = jobId;
                    jobId++;
                    if (int.TryParse(parts[0], out int numberOfOperations))
                        job.NumberOfOperations = numberOfOperations; //Operations in one job
                    else
                        Console.WriteLine("FjspLoader: Error reading number of operations: " + parts[0]);

                    parts.RemoveAt(0);
                    while (parts.Count > 0)
                    {
                        if (string.IsNullOrWhiteSpace(parts[0]))
                        {
                            parts.RemoveAt(0);
                            Console.WriteLine("Removed empty entry");
                        }

                        if (int.TryParse(parts[0], out int operationsPossibilities))
                        {
                            Operation operation = new Operation();
                            operation.Id = operationId;
                            operation.JobId = job.Id;

                            operationId++;

                            MachineProcessingTimePair intermediate = null;
                            for (int i = 1; i <= operationsPossibilities * 2; i++) //Parse machines and times 
                            {
                                if (i % 2 == 0)
                                {
                                    if (intermediate != null)
                                    {
                                        if (int.TryParse(parts[i], out int processingTime))
                                        {
                                            intermediate.ProcessingTime = processingTime;
                                            operation.MachineProcessingTimePairs.Add((MachineProcessingTimePair)intermediate.Clone());
                                            intermediate = null;
                                        }
                                        else
                                        {
                                            Console.WriteLine("FjspLoader: Can not parse processing time " + parts[i]);
                                            parts.RemoveAt(i);
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.TryParse(parts[i], out int machine))
                                    {
                                        intermediate = new MachineProcessingTimePair();
                                        intermediate.Machine = machine;
                                    }
                                    else
                                    {
                                        Console.WriteLine("FjspLoader: Can not parse machine " + parts[i]);
                                        parts.RemoveAt(i);
                                    }
                                }
                            }
                            parts.RemoveRange(0, operationsPossibilities * 2 + 1);
                            operation.MachineProcessingTimePairs =
                                operation.MachineProcessingTimePairs.OrderBy(x => x.Machine).ToList();
                            job.Operations.Add(operation);
                        }
                        else
                        {
                            Console.WriteLine("FjspLoader: Can not parse " + parts[0]);
                            parts.RemoveAt(0);
                        }
                    }
                    
                    readData.Jobs.Add(job);
                    lineNumber++;
                    parts = lines[lineNumber].Split(new [] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    lineEmpty = true;
                    if (parts.Count > 0)
                    {
                        lineEmpty = false;
                        if (parts[0] == "Arrivaltimes")
                            lineEmpty = true;
                    }

                }
            }
            ReadData.Set(readData);
        }



        public override object Clone()
        {
            FjspLoader result = new FjspLoader(Index, Name, Settings);
            result.FileContent = FileContent;
            if (ReadData.Value != null)
                result.ReadData.Set(ReadData.Value);
            return result;
        }
    }
}
