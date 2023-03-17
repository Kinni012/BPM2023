using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.PetriNet
{
    public class PetriNet : ICloneable
    {
        /// <summary>
        /// List of places in the workflow net
        /// </summary>
        public List<Place> Places { get; set; }

        /// <summary>
        /// List of transitions in the new
        /// </summary>
        public List<Transition> Transitions { get; set; }


        public int FireableTransitions
        {
            get
            {
                int result = 0;
                foreach (Transition transition in Transitions.Where(x => x.CanFire))
                {
                    result += transition.To.Count;
                }
                return result;
            }
        }


        /// <summary>
        /// If the net contains a start node, return it
        /// </summary>
        public Place StartNode => Places.FirstOrDefault(x => x.Name == "Start");

        /// <summary>
        /// If the net contains a end node, return it
        /// </summary>
        public Place EndNode => Places.FirstOrDefault(x => x.Name == "End");

        /// <summary>
        /// Returns all non start and end nodes
        /// </summary>
        public List<Place> NormalNodes => Places.Where(x => x.Name != "Start" && x.Name != "End").ToList();

        public PetriNet()
        {
            Places = new List<Place>();
            Transitions = new List<Transition>();
        }

        /// <summary>
        /// Initialize the petri net based on a given process tree
        /// Recursive build of the petri net
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static PetriNet GetPetriNetFromTree(ProcessTree.ProcessTree tree, ref int parallelCounter, ref int xorCounter)
        {
            PetriNet result = new PetriNet();

            if (tree.Children.Count == 0)
            {
                Place start = new Place("Start", 1);
                Place end = new Place("End", 0);
                Place p = new Place(tree.Label, 0);

                Transition t1 = new Transition(start, p);
                Transition t2 = new Transition(p, end);

                result.Places.Add(start);
                result.Places.Add(p);
                result.Places.Add(end);

                result.Transitions.Add(t1);
                result.Transitions.Add(t2);
                return result;
            }

            if (tree.Children.Count > 0)
            {
                List<PetriNet> nets = new List<PetriNet>();
                foreach (ProcessTree.ProcessTree processTree in tree.Children)
                {
                    PetriNet net = GetPetriNetFromTree(processTree, ref parallelCounter, ref xorCounter);
                    nets.Add(net);
                }

                if (tree.Label == "Sequence")
                {
                    Place prevPlace = null;
                    //Find first start node and add it to the result
                    Place start = nets.First().Places.FirstOrDefault(x => x.Name == "Start");
                    //Find last end node and add it to the result
                    Place end = nets.Last().Places.FirstOrDefault(x => x.Name == "End");

                    for (int i = 0; i < nets.Count; i++)
                    {
                        //We have to add all non start or end places, otherwise we lose information
                        foreach (Place currentPlace in nets[i].Places.Where(x => x.Name != "Start" && x.Name != "End"))
                            result.Places.Add(currentPlace);


                        List<Transition> startTransitions =
                            nets[i].Transitions.Where(x => x.From.Select(z => z.Name).Contains("Start")).ToList();
                        List<Transition> otherTransitions =
                            nets[i].Transitions.Where(x => !x.From.Select(z => z.Name).Contains("Start")).ToList();


                        foreach (Transition transition in startTransitions)
                        {
                            if (transition.From.Select(x => x.Name).Contains("Start") && i > 0)
                            {
                                if (prevPlace != null)
                                {
                                    for (int j = 0; j < transition.From.Count; j++)
                                    {
                                        if (transition.From[j].Name == "Start")
                                        {
                                            transition.From[j] = prevPlace;
                                        }
                                    }
                                    transition.Name = "Sequence";
                                    result.Transitions.Add(transition);
                                }
                            }
                            else
                            {
                                result.Transitions.Add(transition);
                            }
                        }


                        foreach (Transition transition in otherTransitions)
                        {
                            //Store the last place of each sequence part as prev place
                            //Later sequence parts will refernce to this place
                            if (transition.To.Select(x => x.Name).Contains("End") && i < nets.Count - 1)
                            {
                                prevPlace = transition.From.FirstOrDefault();
                            }
                            else //Add all non special transitions
                            {
                                result.Transitions.Add(transition);
                            }
                        }
                    }

                    result.Places.Add(start);
                    result.Places.Add(end);
                }

                //TODO: Loop not working correctly yet
                /*
                if (tree.Label == "Loop")
                {
                    Place firstStartingPlace = null;
                    Place firstEndPlace = null;
                    Place prevStartingPlace = null;
                    Place prevEndingPlace = null;

                    for (int i = 0; i < nets.Count; i++)
                    {
                        List<Place> currentPlaces = nets[i].Places;
                        List<Transition> currentTransitions = nets[i].Transitions;


                        if (i == 0)
                        {
                            result.Places.Add(currentPlaces.FirstOrDefault(x => x.Name == "Start"));
                            result.Places.Add(currentPlaces.FirstOrDefault(x => x.Name == "End"));
                            foreach (Transition transition in currentTransitions)
                            {
                                if (transition.From.Name == "Start")
                                {
                                    firstStartingPlace = transition.To;
                                    prevStartingPlace = transition.To;
                                }

                                if (transition.To.Name == "End")
                                {
                                    firstEndPlace = transition.From;
                                    prevEndingPlace = transition.From;
                                }


                                result.Transitions.Add(transition);
                            }
                        }

                        foreach (Place currentPlace in currentPlaces.Where(x => x.Name != "Start" && x.Name != "End"))
                        {
                            result.Places.Add(currentPlace);
                        }


                        if (i == nets.Count - 1)
                        {
                            foreach (Transition transition in currentTransitions.Where(x => x.From.Name == "Start"))
                            {
                                transition.From = prevEndingPlace;
                                result.Transitions.Add(transition);
                            }

                            currentTransitions = currentTransitions.Where(x => x.From.Name != "Start").ToList();

                            foreach (Transition transition in currentTransitions.Where(x => x.To.Name == "End"))
                            {
                                transition.To = firstStartingPlace;

                                result.Transitions.Add(transition);
                            }
                            currentTransitions = currentTransitions.Where(x => x.To.Name != "End").ToList();

                            foreach (Transition transition in currentTransitions)
                            {
                                result.Transitions.Add(transition);
                            }

                        }

                    }
                }*/


                if (tree.Label == "Parallel")
                {
                    //Find first start node and add it to the result
                    Place start = nets.First().Places.FirstOrDefault(x => x.Name == "Start");
                    //Find last end node and add it to the result
                    Place end = nets.Last().Places.FirstOrDefault(x => x.Name == "End");

                    Place parallelStart = new Place($"ParallelSplit{parallelCounter}", 0);
                    Place parallelJoin = new Place($"ParallelJoin{parallelCounter}", 0);
                    parallelCounter++;

                    Transition splitTransition = new Transition();
                    splitTransition.Name = "ParallelSplit";
                    splitTransition.From = new List<Place>() { parallelStart };

                    Transition joinTransition = new Transition();
                    joinTransition.Name = "ParallelJoin";
                    joinTransition.To = new List<Place>() { parallelJoin };

                    Transition t1 = new Transition(start, parallelStart);
                    Transition t2 = new Transition(parallelJoin, end);


                    for (int i = 0; i < nets.Count; i++)
                    {
                        List<Place> currentPlaces = nets[i].Places;
                        List<Transition> currentTransitions = nets[i].Transitions;


                        //Add all non start/end places to the result
                        foreach (Place currentPlace in currentPlaces.Where(x => x.Name != "Start" && x.Name != "End"))
                            result.Places.Add(currentPlace);

                        foreach (Transition transition in currentTransitions)
                        {
                            bool startOrEndTransition = false;
                            //If the start of the transition is a start node we add all To nodes to the parallel split
                            if (transition.From.Select(x => x.Name).Contains("Start"))
                            {
                                foreach (Place place in transition.To)
                                    splitTransition.To.Add(place);
                                startOrEndTransition = true;
                            }

                            //If the end of the transition is a end node, we add all From nodes to the parallel join
                            if (transition.To.Select(x => x.Name).Contains("End"))
                            {
                                foreach (Place place in transition.From)
                                    joinTransition.From.Add(place);
                                startOrEndTransition = true;
                            }
                            if (!startOrEndTransition)
                                result.Transitions.Add(transition);

                        }
                    }
                    result.Transitions.Add(splitTransition);
                    result.Transitions.Add(joinTransition);
                    result.Transitions.Add(t1);
                    result.Transitions.Add(t2);
                    result.Places.Add(start);
                    result.Places.Add(end);
                    result.Places.Add(parallelStart);
                    result.Places.Add(parallelJoin);
                }

                if (tree.Label == "Xor")
                {
                    //Find first start node and add it to the result
                    Place start = nets.First().Places.FirstOrDefault(x => x.Name == "Start");
                    //Add a XorSplit
                    Place xorStart = new Place($"XorSplit{xorCounter}", 0);
                    //Find last end node and add it to the result
                    Place end = nets.Last().Places.FirstOrDefault(x => x.Name == "End");
                    //Add a XorJoin
                    Place xorJoin = new Place($"XorJoin{xorCounter}", 0);
                    xorCounter++;

                    Transition t1 = new Transition(start, xorStart);
                    Transition t2 = new Transition(xorJoin, end);


                    for (int i = 0; i < nets.Count; i++)
                    {
                        List<Place> currentPlaces = nets[i].Places;
                        List<Transition> currentTransitions = nets[i].Transitions;


                        //Add all normal places to the result
                        foreach (Place currentPlace in currentPlaces.Where(x => x.Name != "Start" && x.Name != "End"))
                        {
                            result.Places.Add(currentPlace);
                        }

                        //Change all transitions so that they start from the same start or end node
                        foreach (Transition transition in currentTransitions)
                        {
                            if (transition.From.Select(x => x.Name).Contains("Start"))
                            {
                                transition.From = new List<Place>() { xorStart };
                            }
                            else if (transition.To.Select(x => x.Name).Contains("End"))
                            {
                                transition.To = new List<Place>() { xorJoin };
                            }
                            result.Transitions.Add(transition);

                        }
                    }

                    result.Places.Add(start);
                    result.Places.Add(end);
                    result.Places.Add(xorJoin);
                    result.Places.Add(xorStart);
                    result.Transitions.Add(t1);
                    result.Transitions.Add(t2);
                }
            }

            return result;
        }


        /// <summary>
        /// Add a hidden transition from end to start
        /// </summary>
        public void AddHiddenTransition()
        {
            Place startPlace = Places.FirstOrDefault(x => x.Name == "Start");
            Place endPlace = Places.FirstOrDefault(x => x.Name == "End");
            if (startPlace != null && endPlace != null)
                Transitions.Add(new Transition(endPlace, startPlace));
        }


        



        private void CheckTransitions(string placeName)
        {

            List<Transition> toSplit = new List<Transition>();
            foreach (Transition t in Transitions)
            {
                bool isSplit = false;
                foreach (string name in t.To.Select(x => x.Name))
                {
                    if (name.Contains("XorSplit") || name.Contains("ParallelSplit"))
                    {
                        isSplit = true;
                        break;
                    }
                }
                if (isSplit)
                    toSplit.Add(t);
            }

            bool transitionFired = true;
            while (transitionFired)
            {
                transitionFired = false;
                //Always fire transitions that lead to a split (Xor or Parallel)
                foreach (Transition transition in toSplit)
                    if (transition.CanFire)
                    {
                        transition.Fire();
                        transitionFired = true;
                    }
            }


            //Check normal transitions
            //A previous place has a token and can fire
            //We fire the transition and are done with this event
            foreach (Transition transition in Transitions)
            {
                if (transition.CanFire &&
                    transition.To.Select(x => x.Name).Contains(placeName))
                {
                    transition.Fire();
                    break;
                }
            }


            List<Transition> toJoin = new List<Transition>();
            foreach (Transition t in Transitions)
            {
                bool isSplit = false;
                foreach (string name in t.To.Select(x => x.Name))
                {
                    if (name.Contains("XorJoin") || name.Contains("ParallelJoin"))
                    {
                        isSplit = true;
                        break;
                    }
                }
                if (isSplit)
                    toJoin.Add(t);
            }


            transitionFired = true;
            while (transitionFired)
            {
                transitionFired = false;
                //Always fire transitions that lead to a split (Xor or Parallel)
                foreach (Transition transition in toJoin)
                    if (transition.CanFire)
                    {
                        transition.Fire();
                        transitionFired = true;
                    }
            }

            //If a transition can be fired where the end result is the end we activate it
            foreach (Transition transition in Transitions.Where(x => x.To.Select(z => z.Name).Contains("End")))
                if (transition.CanFire)
                    transition.Fire();
        }



        /// <summary>
        /// Check if a specific trace of events can be replayed on a petri net and the end transition is reached
        /// The support describes the amount of traces that can be replayed on a net
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public bool TraceInNet(List<string> events)
        {
            //First step in the replay is, that we set enough tokens in the starting place
            foreach (Place place in Places)
            {
                if (place.Name == "Start")
                    place.Tokens = 1;
                else
                    place.Tokens = 0;
            }


            foreach (string location in events)
            {
                CheckTransitions(location);

                //Activate hidden transition
                foreach (Transition transition in Transitions.Where(x => x.From.Select(y => y.Name).Contains("End") &&
                                                                                 x.To.Select(z => z.Name).Contains("Start")))
                    if (transition.CanFire)
                    {
                        transition.Fire();
                        //Once the hidden transition fires, we know our petri net is in the log
                        return true;
                    }
            }
            return false;
        }

        public List<Alignment> NetAlignment(List<string> events, List<string> traceIdentifiers)
        {
            //First step in the replay is, that we set enough tokens in the starting place
            foreach (Place place in Places)
            {
                if (place.Name == "Start")
                    place.Tokens = 1;
                else
                    place.Tokens = 0;
            }

            List<Alignment> result = new List<Alignment>();

            for (int i = 0; i < events.Count; i++)
            {
                string location = events[i];
                string trace = traceIdentifiers[i];
                bool normalCaseFound = false;


                List<Transition> parallelSplits = new List<Transition>();
                List<Transition> xorSplits = new List<Transition>();
                List<Transition> parallelJoins = new List<Transition>();
                List<Transition> xorJoins = new List<Transition>();

                foreach (Transition transition in Transitions)
                {
                    List<string> toNames = transition.To.Select(x => x.Name).ToList();
                    foreach (string name in toNames)
                    {
                        if (name.Contains("ParallelSplit"))
                            parallelSplits.Add(transition);
                        if (name.Contains("ParallelJoin"))
                            parallelJoins.Add(transition);
                        if (name.Contains("XorSplit"))
                            xorSplits.Add(transition);
                        if (name.Contains("XorJoin"))
                            xorJoins.Add(transition);
                    }
                }


                bool splitFound = true;
                while (splitFound)
                {
                    splitFound = false;

                    //Special case parallel split
                    foreach (Transition transition in parallelSplits)
                    {
                        if (transition.CanFire)
                        {
                            
                            result.Add(new Alignment(">>", "ParallelSplit", FireableTransitions));
                            transition.Fire();
                            splitFound = true;
                            break;
                        }
                    }


                    //Special case xor split
                    foreach (Transition transition in xorSplits)
                    {
                        if (transition.CanFire && transition.ToNonSplit(Transitions).Contains(location))
                        {
                            result.Add(new Alignment(">>", "XorSplit", FireableTransitions));
                            transition.Fire();
                            splitFound = true;
                            break;
                        }
                    }
                }


                //Normal case
                foreach (Transition transition in Transitions)
                {
                    if (transition.CanFire && transition.To.Select(x => x.Name).Contains(location))
                    {
                        Alignment alignment = new Alignment(location, location, FireableTransitions);
                        alignment.Trace = trace;
                        result.Add(alignment);
                        normalCaseFound = true;
                        transition.Fire();
                        break;
                    }
                }

                //No normal case found
                if (!normalCaseFound)
                {
                    result.Add(new Alignment(location, ">>", 0));
                }




                bool joinFound = true;
                while (joinFound)
                {
                    joinFound = false;

                    //Special case parallel join
                    foreach (Transition transition in parallelJoins)
                    {
                        if (transition.CanFire)
                        {
                            result.Add(new Alignment(">>", "ParallelJoin", FireableTransitions));
                            transition.Fire();
                            joinFound = true;
                            break;
                        }
                    }


                    //Special case xor join
                    foreach (Transition transition in xorJoins)
                    {
                        if (transition.CanFire)
                        {
                            result.Add(new Alignment(">>", "XorJoin", FireableTransitions));
                            transition.Fire();
                            joinFound = true;
                            break;
                        }
                    }
                }



                //Special case to end
                foreach (Transition transition in Transitions.Where(x => x.To.Select(z => z.Name).Contains("End")))
                {
                    if (transition.CanFire)
                    {
                        result.Add(new Alignment(">>", "End", FireableTransitions));
                        transition.Fire();
                    }
                }

                //Special case hidden activity
                foreach (Transition transition in Transitions.Where(x => x.From.Select(y => y.Name).Contains("End")
                                                                         && x.To.Select(z => z.Name).Contains("Start")))
                {
                    if (transition.CanFire)
                    {
                        result.Add(new Alignment(">>", "Hidden", FireableTransitions));
                        transition.Fire();
                    }
                }
            }

            return result;

        }

        /// <summary>
        /// Support describes the number of traces that can be replayed
        /// ==> number of fitting traces / number of traces
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public double CalculateSupport(List<ProcessMiningEvent> events)
        {

            IEnumerable<string> distinctKeys = events.Select(x => x.TraceIdentifier).Distinct();
            int tracesContainingKey = 0;
            //Calculate Support
            foreach (string key in distinctKeys)
            {
                List<ProcessMiningEvent> traceEvents =
                    events.Where(x => x.TraceIdentifier == key).OrderBy(x => x.StartTime).ToList();
                List<string> tracesLocations = traceEvents.Select(x => x.ActivityKey).ToList();

                if (TraceInNet(tracesLocations))
                    tracesContainingKey++;
            }

            double result = tracesContainingKey / ((double)distinctKeys.Count());

            if (result > 1 || result < 0)
            {
                Console.WriteLine("Error in support");
                throw new ArgumentException("Support can not be larger than 1 or lower than 0");
            }
            return result;
        }

        /// <summary>
        /// The confidence describe the number of places that are part of a given net
        /// </summary>
        /// <param name="alignments"></param>
        /// <returns></returns>
        private double CalculateConfidence(List<Alignment> alignments)
        {
            List<double> confidenceValues = new List<double>();
            foreach (Place place in NormalNodes)
            {
                int occurances = 0;
                int explainable = 0;

                foreach (Alignment tuple in alignments.Where(x => x.MoveOnModel == place.Name))
                {
                    occurances += 1;
                    if (tuple.MoveOnLog == place.Name)
                        explainable += 1;

                }

                if (explainable != 0)
                    confidenceValues.Add((double)explainable / (double)occurances);
            }

            double result = HarmonicMean(confidenceValues);

            if (result > 1 || result < 0)
            {
                Console.WriteLine("Error in confidence");
                throw new ArgumentException("Confidence can not be larger than 1 or lower than 0");
            }
            return result;
        }


        private double CalculateCoverage(List<ProcessMiningEvent> minedEvents)
        {

            //Calculate coverage
            int coverageCounter = 0;
            foreach (ProcessMiningEvent minedEvent in minedEvents)
                if (NormalNodes.Select(x => x.Name).Contains(minedEvent.ActivityKey))
                    coverageCounter++;
            double result = coverageCounter / Convert.ToDouble(minedEvents.Count);

            if (result > 1 || result < 0)
            {
                Console.WriteLine("Error in Coverage");
                throw new ArgumentException("Coverage can not be larger than 1 or lower than 0");
            }
            return result;
        }

        /// <summary>
        /// Determinism is the number of enabled transitions in each state divided by 
        /// </summary>
        /// <param name="alignments"></param>
        /// <returns></returns>
        private double CalculateDeterminism(List<Alignment> alignments)
        {
            //Calculate determinism

            List<string> ignoreList = new List<string>(){"XorSplit", "XorJoin", "ParallelSplit", "ParallelJoin"};
            int determinism = 0;
            int totalAlignment = 0;
            foreach (Alignment tuple in alignments)
            {
                if (!ignoreList.Contains(tuple.MoveOnModel) && tuple.NumberOfEnabledTransitions > 0)
                {
                    determinism += tuple.NumberOfEnabledTransitions;
                    totalAlignment += 1;
                }
            }

            double result = (double)totalAlignment / (double)determinism;
            if (result > 1 || result < 0)
            {
                Console.WriteLine("Error in Determinism");
                throw new ArgumentException("Determinism can not be larger than 1 or lower than 0");
            }
            return result;
        }

        private double CalculateLanguageFit(List<Alignment> alignments)
        {
            //Calculate language fit
            double possiblePaths = CalculatePossiblePaths();
            List<string> paths = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (Alignment alignment in alignments.Where(x => x.MoveOnLog != ">>"))
            {
                //Ignore start and hidden
                if (alignment.MoveOnLog == "Start" ||
                    alignment.MoveOnLog == "Hidden" ||
                    alignment.MoveOnLog == "XorSplit" ||
                    alignment.MoveOnLog == "ParallelSplit" ||
                    alignment.MoveOnLog == "XorJoin" ||
                    alignment.MoveOnLog == "ParallelJoin")
                    continue;

                if (alignment.MoveOnLog != "End")
                {
                    sb.Append(alignment.MoveOnLog);
                }
                else
                {
                    if (!paths.Contains(sb.ToString()))
                        paths.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (possiblePaths == 0)
            {
                throw new ArgumentException("Zero possible paths");
            }

            double result = paths.Count / possiblePaths;
            if (result > 1 || result < 0)
            {
                Console.WriteLine("Error in Language fit");
                throw new ArgumentException("Language fit can not be larger than 1 or lower than 0");
            }
            return result;
        }


        /// <summary>
        /// Get Quality metrics of a given list of event logs on the current petri net
        /// </summary>
        /// <param name="minedEvents"></param>
        /// <returns></returns>
        public QualityMetrics ReplayLogOnProcessModel(List<ProcessMiningEvent> minedEvents)
        {
            //double support = CalculateSupport(minedEvents);
            

            List<ProcessMiningEvent> orderedEvents = minedEvents.OrderBy(x => x.StartTime).ToList();
            List<Alignment> alignments = NetAlignment(orderedEvents.Select(x => x.ActivityKey).ToList(), orderedEvents.Select(x => x.TraceIdentifier).ToList());

            double support = CalculateSupport2(alignments, minedEvents);
         
            double confidence = CalculateConfidence(alignments);

            double coverage = CalculateCoverage(orderedEvents);
            double determinism = CalculateDeterminism(alignments);
            double languageFit = CalculateLanguageFit(alignments);




            QualityMetrics result = new QualityMetrics();
            result.Support = support;
            result.Confidence = confidence;
            result.Coverage = coverage;
            result.Determinism = determinism;
            result.LanguageFit = languageFit;
            return result;
        }

        //Alignment based support calculation
        private double CalculateSupport2(List<Alignment> alignments, List<ProcessMiningEvent> minedEvents)
        {
            List<Alignment> alignmentsWithTraces = alignments.Where(x => x.Trace != null).ToList();
            IEnumerable<IGrouping<string, Alignment>> groupedAlignments = alignmentsWithTraces.GroupBy(x => x.Trace);

            double allTraces = 0;
            double explainAble = 0;

            foreach (string s in minedEvents.Select(x => x.TraceIdentifier).Distinct())
            {
                allTraces += 1;
                IGrouping<string, Alignment> alignmentsForTrace = groupedAlignments.FirstOrDefault(x => x.Key == s);
                if(alignmentsForTrace == null)
                    continue;
                IEnumerable<string> placeNamesForTrace =  alignmentsForTrace.Select(x => x.MoveOnLog).Distinct();
                bool newTransitions = true;
                List<Transition> transitions = new List<Transition>();
                while (newTransitions)
                {
                    if (transitions.Count == 0)
                    {
                        foreach (Transition transition in Transitions)
                        {
                            List<string> fromNames = transition.From.Select(x => x.Name).ToList();
                            if (fromNames.Intersect(placeNamesForTrace).Any())
                            {
                                transitions.Add(transition);
                            }
                        }
                    }

                    newTransitions = false;

                    List<Transition> toAdd = new List<Transition>();
                    foreach (Transition transition in Transitions.Except(transitions))
                    {
                        foreach (Transition transition1 in transitions)
                        {
                            List<string> toNames = transition1.To.Select(x => x.Name).ToList();
                            if (transition.From.Select(x => x.Name).Intersect(toNames).Any())
                            {
                                toAdd.Add(transition);
                                newTransitions = true;
                            }
                        }
                    }

                    foreach (Transition transition in toAdd)
                    {
                        transitions.Add(transition);
                    }
                }

                bool c = false;
                foreach (Transition transition in transitions)
                {
                    foreach (Place place in transition.To)
                    {
                        if (place.Name == "End")
                        {
                            explainAble += 1;
                            c = true;
                            break;
                        }
                    }

                    if (c)
                        break;
                }


            }


            return explainAble / allTraces;
        }


        private double CalculatePossiblePaths()
        {
            double result = 1;
            foreach (Place place in Places)
            {
                int transitionsFromPlace = 0;
                foreach (Transition transition in Transitions)
                {
                    bool foundTransition = false;
                    foreach (Place place1 in transition.From)
                    {
                        if (place1.Name == place.Name)
                        {
                            foundTransition = true;
                        }
                    }

                    if (foundTransition)
                    {
                        transitionsFromPlace += transition.To.Count;
                    }
                }

                if (transitionsFromPlace > 1)
                    result *= transitionsFromPlace;
            }
            return result;
        }


        //https://www.omnicalculator.com/math/harmonic-mean#what-is-the-harmonic-mean
        static double HarmonicMean(List<double> values)
        {
            //Count the numbers - let's say there are n of them.
            int n = values.Count;
            double s = 0;
            for (int i = 0; i < n; i++)
            {
                //Compute the reciprocal of each number - recall the reciprocal of x is just 1/x.
                //Add those reciprocals and denote the sum by s.
                s += 1 / values[i];
            }
            //Calculate the harmonic mean by dividing n by s.
            return n / s;
        }


        /// <summary>
        /// Create a deep clone of the petri net
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            PetriNet net = new PetriNet();
            foreach (Place place in Places)
                net.Places.Add((Place)place.Clone());

            foreach (Transition transition in Transitions)
            {
                Transition t = (Transition)transition.Clone();
                foreach (Place place in t.From)
                    t.From.Add(net.Places.FirstOrDefault(x => x.Id == place.Id));
                foreach (Place place in t.To)
                    t.To.Add(net.Places.FirstOrDefault(x => x.Id == place.Id));

                net.Transitions.Add(t);
            }
            return net;
        }
    }
}
