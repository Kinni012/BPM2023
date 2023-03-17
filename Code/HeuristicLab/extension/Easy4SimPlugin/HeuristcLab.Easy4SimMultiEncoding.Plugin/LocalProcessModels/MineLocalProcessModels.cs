using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels
{
    public class MineLocalProcessModel
    {
        private XmlDocument document;
        private string v;

        /// <summary>
        /// Attribute name of the activity key in the xml document
        /// </summary>
        public string ActivityKey { get; set; } = "WorkstationsSpecific"; //WorkGroupSpecific
        /// <summary>
        /// Attribute name of the start time in the xml document
        /// </summary>
        public string StartTimeKey { get; set; } = "start_timestamp";
        /// <summary>
        /// Attribute name of the end time in the xml document
        /// </summary>
        public string EndTimeKey { get; set; } = "time:timestamp";
        /// <summary>
        /// Attribute name of the Order/job
        /// </summary>
        public string OrderOrJob { get; set; } = "Order";
        public FileHandler FileHandler { get; set; }
        /// <summary>
        /// The current xml document that is analyzed
        /// </summary>
        public XmlDocument CurrentDocument { get; set; }
        /// <summary>
        /// List of unique activity keys extracted from the current xml document
        /// </summary>
        public List<string> ActivityKeys { get; set; }
        /// <summary>
        /// List of all extracted process mining events from the current xml document
        /// </summary>
        public List<ProcessMiningEvent> ProcessMiningEvents { get; set; }
        /// <summary>
        /// List of basic process trees from the current xml document
        /// </summary>
        public List<ProcessTree.ProcessTree> Trees { get; set; }
        public Random.MersenneTwister Twister { get; private set; }

        public void Initialize()
        {
            ActivityKeys = new List<string>();
            ProcessMiningEvents = new List<ProcessMiningEvent>();
            Trees = new List<ProcessTree.ProcessTree>();
            Twister = new Random.MersenneTwister();
            if (CurrentDocument != null)
            {

                ProcessMiningEvents = GetProcessMiningEventsFromDocument(CurrentDocument);
                Trees = GenerateBaseTreesForMinedEvents(ProcessMiningEvents);
                ActivityKeys = ProcessMiningEvents.Select(x => x.ActivityKey).Distinct().ToList().OrderBy(x => x).ToList();
            }

        }

        public MineLocalProcessModel()
        {
            Initialize();
        }
        public MineLocalProcessModel(XmlDocument document)
        {
            CurrentDocument = document;
            Initialize();
        }
        public MineLocalProcessModel(XmlDocument document, string activityKey)
        {
            CurrentDocument = document;
            ActivityKey = activityKey;
            Initialize();
        }
        public MineLocalProcessModel(FileHandler fileHandler)
        {
            Initialize();
            FileHandler = fileHandler;
        }

        public MineLocalProcessModel(XmlDocument document, string activityKey, string orderOrJob)
        {
            ActivityKey = activityKey;
            OrderOrJob = orderOrJob;
            CurrentDocument = document;
            Initialize();
        }

        public List<ProcessMiningEvent> GetProcessMiningEventsFromDocument(XmlDocument document)
        {
            List<ProcessMiningEvent> result = new List<ProcessMiningEvent>();


            XmlNodeList traces = document.GetElementsByTagName("trace");  //Get all traces in document
            foreach (XmlNode trace in traces)
            {
                foreach (XmlNode node in trace.ChildNodes)
                {

                    if (node.Name == "event") //Get all events in traces
                    {
                        string attributeKeyValue = "";
                        DateTime startTime = DateTime.MinValue;
                        DateTime endTime = DateTime.MinValue;
                        string traceIdentifier = "";


                        foreach (XmlNode eventChild in node.ChildNodes) //Iterate all event children
                        {
                            XmlAttributeCollection attrColl = eventChild.Attributes; //Get attributes of children

                            if (attrColl["key"].Value == ActivityKey)
                                attributeKeyValue = attrColl["value"].Value;

                            if (attrColl["key"].Value == StartTimeKey)
                                startTime = DateTime.ParseExact(attrColl["value"].Value, "yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz", null);

                            if (attrColl["key"].Value == EndTimeKey)
                                endTime = DateTime.ParseExact(attrColl["value"].Value, "yyyy'-'MM'-'dd'T'HH:mm:ss.fffzzz", null);

                            if (attrColl["key"].Value == OrderOrJob)
                                traceIdentifier = attrColl["value"].Value;

                        }

                        if (startTime != DateTime.MinValue && endTime != DateTime.MinValue)
                            result.Add(new ProcessMiningEvent(attributeKeyValue, startTime, endTime, traceIdentifier));
                    }
                }
            }

            return result;
        }




        public List<ProcessTree.ProcessTree> GenerateBaseTreesForMinedEvents(List<ProcessMiningEvent> events)
        {
            List<string> activityKeys = events.Select(x => x.ActivityKey).Distinct().ToList().OrderBy(x => x).ToList();
            List<ProcessTree.ProcessTree> result = new List<ProcessTree.ProcessTree>();
            foreach (string key in activityKeys)
            {
                ProcessTree.ProcessTree tree = new ProcessTree.ProcessTree(key);
                result.Add(tree);
            }
            return result;
        }

        public IntermediateLocalProcessModelResult MineLocalProcessModels(bool showPerformance = false)
        {
            // ================== Performance metrics =====================================
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            int generatedIndividuals = 0;
            int duplicateCounter = 0;
            if (showPerformance)
            {
                sw.Start();
                //List of base trees with quality metrics
            }
            LocalProcessMiningResult result = new LocalProcessMiningResult();


            // ================== Init =====================================
            foreach (ProcessTree.ProcessTree tree in Trees)
            {
                PetriNet.PetriNet net = new PetriNet.PetriNet();
                int parallelCounter = 0;
                int xorCounter = 0;
                net = PetriNet.PetriNet.GetPetriNetFromTree(tree, ref parallelCounter, ref xorCounter);
                net.AddHiddenTransition();
                QualityMetrics metric = net.ReplayLogOnProcessModel(ProcessMiningEvents);

                result.InitialIntermediates.Add(new IntermediateLocalProcessModelResult(tree, metric));
                if (showPerformance)
                    generatedIndividuals++;

            }

            if (showPerformance)
                Console.WriteLine("Best quality after initialization: " + result.BestInitialMetric);


            IntermediateLocalProcessModelResult bestResult = null;



            // ================== Mining part =====================================
            for (int i = 0; i < 2; i++)
            {
                Dictionary<string, List<string>> representations = new Dictionary<string, List<string>>();
                int count = 100 < result.InitialIntermediates.Count ? 100 : result.InitialIntermediates.Count;
                result.InitialIntermediates = result.InitialIntermediates.OrderByDescending(x => x.MetricSum).Take(count).ToList();
                foreach (IntermediateLocalProcessModelResult intermediateLocalProcessModelResult in result.InitialIntermediates.OrderByDescending(x => x.MetricSum))
                {
                    if (bestResult == null || intermediateLocalProcessModelResult.MetricSum > bestResult.MetricSum)
                        bestResult = intermediateLocalProcessModelResult;

                    foreach (string treeOperator in ProcessTree.Operators.AllOperators())
                    {
                        foreach (ProcessTree.ProcessTree tree in intermediateLocalProcessModelResult.CurrentProcessTree.GetLeaves())
                        {
                            foreach (string secondChild in intermediateLocalProcessModelResult.CurrentProcessTree.ActivityKeysNotInTree(ActivityKeys))
                            {
                                ProcessTree.ProcessTree clonedTree = (ProcessTree.ProcessTree)intermediateLocalProcessModelResult.CurrentProcessTree.Clone();
                                clonedTree.ReplaceSpecificChildWithOperator(tree.Label, treeOperator, secondChild);

                                List<string> clonedTreeRepresentations = clonedTree.GetRepresentations();
                                StringBuilder sb = new StringBuilder();
                                foreach (string key in clonedTree.GetLeaves().Select(x => x.Label).OrderBy(x => x))
                                {
                                    sb.Append(key);
                                }

                                string dicKey = sb.ToString();

                                if (representations.ContainsKey(dicKey))
                                {
                                    //Duplicate detected
                                    if (representations[dicKey].Intersect(clonedTreeRepresentations).Any())
                                    {
                                        duplicateCounter++;
                                        continue;
                                    }
                                    else
                                    {
                                        //Key in dictionary but no duplicate detected
                                        representations[dicKey].AddRange(clonedTreeRepresentations);
                                    }
                                }
                                else
                                {
                                    //Key not in dictionary
                                    representations.Add(dicKey, clonedTreeRepresentations);
                                }



                                PetriNet.PetriNet net = new PetriNet.PetriNet();
                                int parallelCounter = 0;
                                int xorCounter = 0;
                                net = PetriNet.PetriNet.GetPetriNetFromTree(clonedTree, ref parallelCounter, ref xorCounter);
                                net.AddHiddenTransition();

                                QualityMetrics metric = net.ReplayLogOnProcessModel(ProcessMiningEvents);
                                result.CurrentIntermediates.Add(new IntermediateLocalProcessModelResult(clonedTree, metric));
                                if (showPerformance)
                                    generatedIndividuals++;
                            }
                        }
                    }
                }
                result.InitialIntermediates = result.CurrentIntermediates;
                if (showPerformance)
                    Console.WriteLine($"Best quality after {i + 1} generations: " + result.BestInitialMetric);
            }

            foreach (IntermediateLocalProcessModelResult intermediateLocalProcessModelResult in result.InitialIntermediates)
            {
                if (bestResult == null || intermediateLocalProcessModelResult.MetricSum > bestResult.MetricSum)
                    bestResult = intermediateLocalProcessModelResult;
            }

            Console.WriteLine($"Best quality after the last generations: " + result.BestInitialMetric);

            // ================== Show performance metrics =====================================
            if (showPerformance)
            {
                sw.Stop();
                Console.WriteLine($"Elapsed seconds: " + sw.ElapsedMilliseconds / 1000);
                Console.WriteLine("Number of generated individuals: " + generatedIndividuals);
                Console.WriteLine("Duplicates prevented: " + duplicateCounter);
                Console.WriteLine();
            }
            return bestResult;

        }


        public IntermediateLocalProcessModelResult MineRandomLocalProcessModel(bool showPerformance = true, int generationSize = 200, int lpmSize = 4)
        {
            Stopwatch sw = new Stopwatch();
            int generatedIndividuals = 0;
            int duplicateCounter = 0;
            if (showPerformance)
            {
                sw.Start();
                //List of base trees with quality metrics
            }
            LocalProcessMiningResult result = new LocalProcessMiningResult();
            // ================== Init =====================================
            foreach (ProcessTree.ProcessTree tree in Trees)
            {
                PetriNet.PetriNet net = new PetriNet.PetriNet();
                int parallelCounter = 0;
                int xorCounter = 0;
                net = PetriNet.PetriNet.GetPetriNetFromTree(tree, ref parallelCounter, ref xorCounter);
                net.AddHiddenTransition();
                QualityMetrics metric = net.ReplayLogOnProcessModel(ProcessMiningEvents);

                result.InitialIntermediates.Add(new IntermediateLocalProcessModelResult(tree, metric));
                if (showPerformance)
                    generatedIndividuals++;

            }

            if (showPerformance)
                Console.WriteLine("Best quality after initialization: " + result.BestInitialMetric);


            IntermediateLocalProcessModelResult bestResult = null;
            // ================== Mining part =====================================
            for (int i = 0; i < lpmSize; i++)
            {
                Dictionary<string, List<string>> representations = new Dictionary<string, List<string>>();
                List<IntermediateLocalProcessModelResult> descendingIntermediates = result.InitialIntermediates.OrderByDescending(x => x.MetricSum).ToList();


                double totalMetricSum = result.InitialIntermediates.Select(x => x.MetricSum).Sum();


                while (result.CurrentIntermediates.Count < generationSize)
                {
                    double randomIndividual = Twister.NextDouble() * totalMetricSum;
                    double upperBound = 0;
                    IntermediateLocalProcessModelResult toManipulate = null;
                    for (int j = 0; j < descendingIntermediates.Count; j++)
                    {
                        upperBound += descendingIntermediates[j].MetricSum;
                        if (randomIndividual <= upperBound)
                        {
                            toManipulate = descendingIntermediates[j];
                            break;
                        }
                    }

                    string treeOperator = ProcessTree.Operators.RandomOperator();
                    string firstLeave = toManipulate.CurrentProcessTree.GetRandomLeave().Label;
                    string secondChild = toManipulate.CurrentProcessTree.RandomNodeNotInTree(ActivityKeys);

                    ProcessTree.ProcessTree clonedTree =
                        (ProcessTree.ProcessTree)toManipulate.CurrentProcessTree.Clone();
                    clonedTree.ReplaceSpecificChildWithOperator(firstLeave, treeOperator, secondChild);

                    PetriNet.PetriNet net = new PetriNet.PetriNet();
                    int parallelCounter = 0;
                    int xorCounter = 0;
                    net = PetriNet.PetriNet.GetPetriNetFromTree(clonedTree, ref parallelCounter, ref xorCounter);
                    net.AddHiddenTransition();

                    QualityMetrics metric = net.ReplayLogOnProcessModel(ProcessMiningEvents);
                    IntermediateLocalProcessModelResult res =
                        new IntermediateLocalProcessModelResult(clonedTree, metric);
                    result.CurrentIntermediates.Add(res);

                    if (bestResult == null || metric.MetricSum > bestResult.MetricSum || res.Order > bestResult.Order)
                        bestResult = res;
                    if (showPerformance)
                        generatedIndividuals++;

                }
                result.InitialIntermediates = result.CurrentIntermediates;
                result.CurrentIntermediates = new List<IntermediateLocalProcessModelResult>();
                if (showPerformance)
                    Console.WriteLine($"Best quality after generation {i}: " + result.BestInitialMetric);
            }


            // ================== Show performance metrics =====================================
            if (showPerformance)
            {
                sw.Stop();
                Console.WriteLine($"Elapsed seconds: " + sw.ElapsedMilliseconds / 1000);
                Console.WriteLine("Number of generated individuals: " + generatedIndividuals);
                Console.WriteLine("Duplicates prevented: " + duplicateCounter);
                Console.WriteLine();
            }
            return bestResult;
        }
    }
}
