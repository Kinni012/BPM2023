using System;
using System.Collections.Generic;
using System.Linq;
using Easy4SimFramework;
using FjspEasy4SimLibrary;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.HelperClasses
{
    public static class SimulationPropertySetter
    {

        public static void SetParameterOptimizationInSimulation(Solver solver, ParameterArrayOptimization<int> parameterOptimization, IEnumerable<int> values)
        {
            CSimBase baseObject =
                solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                    x.Name == parameterOptimization.BaseObject.Name);

            if(baseObject is FjspEvaluation eval)
            {
                if(parameterOptimization.PropertyInfo.Name == "WorkstationAssignment")
                {
                    for (int i = 0; i < values.Count(); i++)
                    {
                        int bound = eval.WorkstationAssignment.UpperBounds[i];
                        int value = values.ElementAt(i);
                        if(value < bound)
                        {
                            eval.WorkstationAssignment.Value[i] = value;
                        }
                        else
                        {
                            Console.WriteLine("Error in SimulationPropertySetter");
                        }

                    }
                } else if (parameterOptimization.PropertyInfo.Name == "Priority")
                {
                    for (int i = 0; i < values.Count(); i++)
                    {
                        int bound = eval.Priority.UpperBounds[i];
                        int value = values.ElementAt(i);
                        if (value < bound)
                        {
                            eval.Priority.Value[i] = value;
                        }
                        else
                        {
                            Console.WriteLine("Error in SimulationPropertySetter");
                        }
                    }
                } else if (parameterOptimization.PropertyInfo.Name == "CobotLocations")
                {
                    for (int i = 0; i < values.Count(); i++)
                    {
                        int bound = eval.CobotLocations.UpperBounds[i];
                        int value = values.ElementAt(i);
                        if (value < bound)
                        {
                            eval.CobotLocations.Value[i] = value;
                        }
                        else
                        {
                            Console.WriteLine("Error in SimulationPropertySetter");
                        }
                    }
                } else
                {

                }
            }
            else
            {
                var propertyInfo = baseObject.GetType().GetProperties().FirstOrDefault(x => x.Name == parameterOptimization.PropertyInfo.Name);
                var Property = propertyInfo.GetValue(baseObject);
                Property = new ParameterArrayOptimization<int>(values.Count());
                if (Property is ParameterArrayOptimization<int> parameterArray)
                {
                    parameterArray.Value = values.ToArray();
                }
                propertyInfo.SetValue(baseObject, Property);
            }
        }

        public static void SetParameterOptimizationInSimulation(Solver solver, ParameterArrayOptimization<double> parameterOptimization, IEnumerable<double> values)
        {
            CSimBase baseObject =
                solver.SimulationObjects.SimulationList.Values.FirstOrDefault(x =>
                    x.Name == parameterOptimization.BaseObject.Name);

            var propertyInfo = baseObject.GetType().GetProperties().FirstOrDefault(x => x.Name == parameterOptimization.PropertyInfo.Name);
            var Property = propertyInfo.GetValue(baseObject);
            Property = new ParameterArrayOptimization<double>(values.Count());
            if (Property is ParameterArrayOptimization<double> parameterArray)
            {
                parameterArray.Value = values.ToArray();
            }
            propertyInfo.SetValue(baseObject, Property);
        }
    }
}
