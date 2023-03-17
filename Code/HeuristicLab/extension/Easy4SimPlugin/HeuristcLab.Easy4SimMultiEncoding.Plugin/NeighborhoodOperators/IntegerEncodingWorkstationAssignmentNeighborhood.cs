using System;
using System.Collections.Generic;
using System.Linq;
using CobotAssignmentAndJobShopSchedulingProblem;
using Easy4SimFramework;
using FjspEasy4SimLibrary;
using HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining;
using HeuristicLab.Encodings.IntegerVectorEncoding;
using HeuristicLab.Random;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.NeighborhoodOperators
{
    public static class IntegerEncodingWorkstationAssignmentNeighborhood
    {
        public static void BasicChange(MersenneTwister twister, //Random number generator
            List<ParameterArrayOptimization<int>>
                encodingPartInformation, //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation, //Stored bounds for the positions in the encoding
            IntegerVector currentSolution) //The current encoded solution
        {
            int index = twister.Next(encodingPartInformation[0].Amount);
            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
        }

        public static void BasicChangeNoRepeat(MersenneTwister twister, //Random number generator
            List<ParameterArrayOptimization<int>>
                encodingPartInformation, //Which part of the encoding belongs to the workstation assignment, Priority or cobot assignment
            IntegerVectorEncoding boundInformation, //Stored bounds for the positions in the encoding
            IntegerVector currentSolution, //The current encoded solution
            ref List<int> changedPositions)
        {

            int index = twister.Next(encodingPartInformation[0].Amount);
            int counter = 0;
            while (changedPositions.Contains(index) && counter < 10)
            {
                counter++;
                index = twister.Next(encodingPartInformation[0].Amount);
            }

            currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
            changedPositions.Add(index);
        }

        public static void GreedyChange(MersenneTwister twister,
            IntegerVector currentSolution,
            IntegerVectorEncoding boundInformation,
            Solver solver,
            List<ParameterArrayOptimization<int>> encodingPartInformation)
        {

            //Generate a random change index
            int index = twister.Next(encodingPartInformation[0].Amount);


            //Changes to workstation should
            var baseObject = solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                x.Name == encodingPartInformation[0].BaseObject.Name);


            if (baseObject is FjspEvaluation evaluation)
            {
                //Find start/end of cobot encoding
                int cobotEncodingStart = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount;
                int cobotEncodingEnd = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                                       encodingPartInformation[2].Amount;

                //Create a dictionary with the current workstations that have a cobot assigned
                Dictionary<int, bool> cobotLocations = new Dictionary<int, bool>();
                if (evaluation.ReadData.Value is FlexibleJobShopSchedulingData fjssd)
                    for (int i = 0; i < fjssd.NumberOfMachines; i++)
                        cobotLocations.Add(i, false);

                //Set cobot locations based on encoding
                for (int i = cobotEncodingStart; i < cobotEncodingEnd; i++)
                {
                    int j = currentSolution[i];
                    while (cobotLocations[j] == true)
                    {
                        j++;
                    }

                    cobotLocations[j] = true;
                }

                //Operation to change
                Operation operation = evaluation.Data.OperationsWhereWorkstationHasToBeDecided[index];
                // Bound of operations task to workstation encoding
                int bound = boundInformation.Bounds[index, 1];
                //List of workstations with cobots
                List<int> cobotWorkstations = cobotLocations.Where(x => x.Value).Select(y => y.Key).ToList();

                List<int> operationsMachines = operation.MachineProcessingTimePairs.Select(x => x.Machine).ToList();

                if (cobotWorkstations.Intersect(operationsMachines)
                    .Any()) //We found workstations with cobots where the operation can be completed
                {
                    double sum = 0;
                    for (int i = 0; i < operation.MachineProcessingTimePairs.Count; i++)
                    {
                        MachineProcessingTimePair pair = operation.MachineProcessingTimePairs[i];
                        if (cobotWorkstations.Contains(pair.Machine))
                        {
                            sum += 3; //Three times the chance of a cobot workstation in comparison to a normal workstation
                        }
                        else
                        {
                            sum += 1;
                        }
                    }

                    double randomlySelectedMachine = twister.NextDouble() * sum;

                    sum = 0;
                    for (int i = 0; i < operation.MachineProcessingTimePairs.Count; i++)
                    {
                        MachineProcessingTimePair pair = operation.MachineProcessingTimePairs[i];
                        if (cobotWorkstations.Contains(pair.Machine))
                            sum += 3; //Three times the chance of a cobot workstation in comparison to a normal workstation
                        else
                            sum += 1;
                        if (sum > randomlySelectedMachine)
                        {
                            if (i > bound)
                                Console.WriteLine("Error in Greedy change");
                            currentSolution[index] = i;
                            return;
                        }
                    }

                }
                else
                {
                    //No feasable cobot workstation, apply basic change
                    currentSolution[index] = twister.Next(boundInformation.Bounds[index, 1]);
                }



            }

            if (baseObject is ProjectProcessor processor)
            {
                if (processor.ConvertedData is ConvertedData data)
                {
                    int cobotEncodingStart = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount;
                    int cobotEncodingEnd = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                                           encodingPartInformation[2].Amount;

                    //Create a dictionary for the cobot locations
                    Dictionary<int, bool> cobotLocations = new Dictionary<int, bool>();
                    for (int i = 0; i < processor.Settings.Statistics.AmountOfWorkstations; i++)
                        cobotLocations.Add(i, false);

                    for (int i = cobotEncodingStart; i < cobotEncodingEnd; i++)
                    {
                        int j = currentSolution[i];
                        while (cobotLocations[j] == true)
                        {
                            j++;
                        }

                        cobotLocations[j] = true;
                    }

                    ConvertedWorkstep operation = data.OperationsWhereWorkstationHasToBeDecided[index];
                    List<ConvertedWorkstation> workstations = data.WorkstationsInGroup(operation.WorkstationGroup).ToList();

                    int sum = 0;
                    foreach (ConvertedWorkstation workstation in workstations)
                    {
                        int indexOfWorkstation =
                            processor.Settings.Statistics.Workstations.IndexOf(workstation.WorkstationNumber);
                        if (cobotLocations[indexOfWorkstation])
                            sum += 3;
                        else
                            sum += 1;
                    }
                    int randomlySelectedMachine = twister.Next(sum + 1);
                    sum = 0;
                    foreach (ConvertedWorkstation workstation in workstations)
                    {
                        int indexOfWorkstation =
                            processor.Settings.Statistics.Workstations.IndexOf(workstation.WorkstationNumber);
                        if (cobotLocations[indexOfWorkstation])
                            sum += 3;
                        else
                            sum += 1;
                        if (sum > randomlySelectedMachine)
                        {
                            currentSolution[index] = workstations.IndexOf(workstation);
                            return;
                        }
                    }


                }
            }
        }


        public static void ProcessMiningChange(MersenneTwister twister,
            IntermediateLocalProcessModelResult MinedResult,
            IntegerVector currentSolution,
            Solver solver,
            List<ParameterArrayOptimization<int>> encodingPartInformation)
        {
            try
            {


                //Get the FjspEvaluation
                var baseObject = solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                    x.Name == encodingPartInformation[0].BaseObject.Name);
                if (baseObject is FjspEvaluation eval)
                {
                    if (eval.ReadData.Value is FlexibleJobShopSchedulingData fjssd)
                    {
                        //Get the workstations from the local process mining 
                        //We assume that these workstations are hotspots
                        List<int> hotspots = MinedResult.CurrentProcessTree.GetLeaves().Select(x => Convert.ToInt32(x.Label))
                            .ToList();


                        Dictionary<int, int> minedWorkstations = new Dictionary<int, int>();
                        for (int i = 1; i <= eval.Settings.Statistics.AmountOfWorkstations; i++)
                        {
                            if (hotspots.Contains(i))
                                minedWorkstations.Add(i, 3);
                            else
                                minedWorkstations.Add(i, 1);
                        }

                        List<Operation> operations = fjssd.OperationsWhereWorkstationHasToBeDecided.ToList();

                        int index = twister.Next(encodingPartInformation[0].Amount);

                        Operation operation = fjssd.OperationsWhereWorkstationHasToBeDecided[index];
                        List<MachineProcessingTimePair> pairs = operation.MachineProcessingTimePairs;
                        int sum = 0;

                        //Find out machines where operation can be produced
                        List<int> possibleMachines = pairs.Select(x => x.Machine).ToList();
                        foreach (int i in possibleMachines)
                        {
                            sum += minedWorkstations[i];
                        }

                        int newVal = twister.Next(sum) + 1;

                        int sum2 = 0;
                        for (int i = 0; i < possibleMachines.Count; i++)
                        {
                            sum2 += minedWorkstations[possibleMachines[i]];
                            if (sum2 >= newVal)
                            {
                                currentSolution[index] = i;
                                return;
                            }
                        }

                    }
                }

                if (baseObject is ProjectProcessor processor)
                {
                    if (processor.ConvertedData is ConvertedData convertedData)
                    {
                        //Get the workstations from the local process mining 
                        //We assume that these workstations are hotspots
                        List<string> hotspots = MinedResult.CurrentProcessTree.GetLeaves().Select(x => x.Label)
                            .ToList();


                        Dictionary<int, int> minedWorkstations = new Dictionary<int, int>();
                        for (int i = 1; i <= processor.Settings.Statistics.AmountOfWorkstations; i++)
                        {
                            if (hotspots.Contains(processor.Settings.Statistics.Workstations[i - 1]))
                                minedWorkstations.Add(i, 3);
                            else
                                minedWorkstations.Add(i, 1);
                        }
                        List<ConvertedWorkstep> operations = convertedData.OperationsWhereWorkstationHasToBeDecided.ToList();

                        int index = twister.Next(encodingPartInformation[0].Amount);
                        ConvertedWorkstep operation = convertedData.OperationsWhereWorkstationHasToBeDecided[index];

                        List<ConvertedWorkstation> workstations =
                            convertedData.WorkstationsInGroup(operation.WorkstationGroup).ToList();

                        int sum = 0;
                        foreach (ConvertedWorkstation workstation in workstations)
                        {
                            int i = processor.Settings.Statistics.Workstations.IndexOf(workstation.WorkstationNumber);
                            sum += minedWorkstations.ElementAt(i).Value;
                        }

                        int newVal = twister.Next(sum + 1);

                        int sum2 = 0;
                        for (int i = 0; i < workstations.Count; i++)
                        {
                            int j = processor.Settings.Statistics.Workstations.IndexOf(workstations[i].WorkstationNumber);
                            sum2 += minedWorkstations.ElementAt(j).Value;
                            if (sum2 >= newVal)
                            {
                                currentSolution[index] = i;
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in Process mining change");
                throw e;
            }

        }


        public static void ProcessMiningDictionaryChange(MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            IntegerVector currentSolution,
            Solver solver,
            List<ParameterArrayOptimization<int>> encodingPartInformation)
        {
            try
            {

                //Get position to change
                int index = twister.Next(encodingPartInformation[0].Amount);

                //Get the FjspEvaluation
                var baseObject = solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                    x.Name == encodingPartInformation[0].BaseObject.Name);


                if (baseObject is FjspEvaluation eval)
                {
                    if (eval.ReadData.Value is FlexibleJobShopSchedulingData fjssd)
                    {
                        Operation operation = fjssd.OperationsWhereWorkstationHasToBeDecided[index];
                        List<MachineProcessingTimePair> pairs = operation.MachineProcessingTimePairs;
                        int sum = 0;

                        //Find out machines where operation can be produced
                        List<int> possibleMachines = pairs.Select(x => x.Machine).ToList();
                        foreach (int i in possibleMachines)
                        {
                            sum += minedWorkstations[i];
                        }

                        int newVal = twister.Next(sum) + 1;

                        int sum2 = 0;
                        for (int i = 0; i < possibleMachines.Count; i++)
                        {
                            sum2 += minedWorkstations[possibleMachines[i]];
                            if (sum2 >= newVal)
                            {
                                currentSolution[index] = i;
                                return;
                            }
                        }

                    }

                    return;
                }

                if (baseObject is ProjectProcessor projectProcessor)
                {
                    if (projectProcessor.ConvertedData is ConvertedData convertedData)
                    {
                        ConvertedWorkstep workstep = convertedData.OperationsWhereWorkstationHasToBeDecided[index];
                        string group = workstep.WorkstationGroup;
                        List<ConvertedWorkstation> workstations = convertedData.WorkstationsInGroup(group).ToList();
                        int sum = 0;

                        foreach (ConvertedWorkstation ws in workstations)
                        {
                            sum += minedWorkstations[solver.SimulationStatistics.Workstations.IndexOf(ws.WorkstationNumber) + 1];
                        }

                        int newVal = twister.Next(sum) + 1;

                        int sum2 = 0;
                        for (int i = 0; i < workstations.Count; i++)
                        {
                            string workstationNumber = workstations[i].WorkstationNumber;
                            int indexOfWorkstation = solver.SimulationStatistics.Workstations.IndexOf(workstationNumber);

                            sum2 += minedWorkstations[indexOfWorkstation + 1];
                            if (sum2 >= newVal)
                            {
                                currentSolution[index] = i;
                                return;
                            }
                        }

                    }
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error in ProcessMiningDictionaryChange - Workstation assignment");
                throw;
            }

            return;
        }


        public static void Experiment1(MersenneTwister twister,
            Dictionary<int, int> minedWorkstations,
            IntegerVector currentSolution,
            IntegerVectorEncoding boundInformation,
            Solver solver,
            List<ParameterArrayOptimization<int>> encodingPartInformation)
        {
            //Generate a random change index
            int index = twister.Next(encodingPartInformation[0].Amount);


            //Changes to workstation should
            var baseObject = solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                x.Name == encodingPartInformation[0].BaseObject.Name);


            if (baseObject is FjspEvaluation evaluation)
            {
                //Create a copy of the dictionary
                Dictionary<int, int> workstations = minedWorkstations.ToDictionary(x => x.Key, x => x.Value);



                //Find start/end of cobot encoding
                int cobotEncodingStart = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount;
                int cobotEncodingEnd = encodingPartInformation[0].Amount + encodingPartInformation[1].Amount +
                                       encodingPartInformation[2].Amount;

                //Create a dictionary with the current workstations that have a cobot assigned
                Dictionary<int, bool> cobotLocations = new Dictionary<int, bool>();
                if (evaluation.ReadData.Value is FlexibleJobShopSchedulingData fjssd)
                    for (int i = 0; i < fjssd.NumberOfMachines; i++)
                        cobotLocations.Add(i, false);

                //Set cobot locations based on encoding
                for (int i = cobotEncodingStart; i < cobotEncodingEnd; i++)
                {
                    int j = currentSolution[i];
                    while (cobotLocations[j] == true)
                    {
                        j++;
                    }
                    cobotLocations[j] = true;
                }

                for (int i = 0; i < cobotLocations.Count; i++)
                {
                    if (cobotLocations[i])
                        workstations[i + 1] += 5;
                }

                //Operation to change
                Operation operation = evaluation.Data.OperationsWhereWorkstationHasToBeDecided[index];
                // Bound of operations task to workstation encoding
                int bound = boundInformation.Bounds[index, 1];

                List<int> operationsMachines = operation.MachineProcessingTimePairs.Select(x => x.Machine).ToList();

                int sum = 0;
                foreach (int machine in operationsMachines)
                {
                    sum += workstations[machine];
                }
                double randomlySelectedMachine = twister.NextDouble() * sum;
                sum = 0;

                for (int i = 0; i < operationsMachines.Count; i++)
                {
                    int machine = operationsMachines[i];

                    sum += workstations[machine];
                    if (sum > randomlySelectedMachine)
                    {
                        if (i > bound)
                            Console.WriteLine("Error in experiment 1 change");
                        currentSolution[index] = i;
                        return;
                    }
                }
            }
        }
    }
}
