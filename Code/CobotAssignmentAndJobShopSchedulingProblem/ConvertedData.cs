using System;
using System.Collections.Generic;
using System.Linq;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    public class ConvertedData : ICloneable
    {
        public List<ProducedProductLog> ProductsProduced { get; set; } = new List<ProducedProductLog>();
        public List<WorkstepLog> Logs { get; set; } = new List<WorkstepLog>();


        public List<ConvertedOrder> Orders { get; set; } = new List<ConvertedOrder>();
        public List<ConvertedOrder> FinishedOrders { get; set; } = new List<ConvertedOrder>();


        public List<ConvertedWorkstation> Workstations { get; set; } = new List<ConvertedWorkstation>();

        public List<ConvertedWorkstation> WorkstationsBasedOnAssignment(double[] cobotLocations)
        {
            List<ConvertedWorkstation> initialWorkstations = Workstations.Where(x => !x.IsCobotAssigned).ToList();
            List<ConvertedWorkstation> result = new List<ConvertedWorkstation>();

            foreach (double d in cobotLocations)
            {

                if (initialWorkstations.Count == 0)
                    break;
                int index = (int)(d * initialWorkstations.Count);
                result.Add(initialWorkstations[index]);
                initialWorkstations.RemoveAt(index);
            }

            return result;
        }
        public List<ConvertedWorkstation> WorkstationsBasedOnAssignmentInt(int[] cobotLocations)
        {
            List<ConvertedWorkstation> initialWorkstations = Workstations.Where(x => !x.IsCobotAssigned).ToList();
            List<ConvertedWorkstation> result = new List<ConvertedWorkstation>();

            foreach (int i in cobotLocations)
            {

                if (initialWorkstations.Count == 0)
                    break;
                int index = i % initialWorkstations.Count;
                result.Add(initialWorkstations[index]);
                initialWorkstations.RemoveAt(index);
            }

            return result;
        }

        public List<ConvertedWorkstep> WorkSteps
        {
            get
            {
                List<ConvertedWorkstep> result = new List<ConvertedWorkstep>();

                foreach (ConvertedOrder order in Orders)
                {
                    foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder)
                    {
                        result.Add(workstep);
                    }
                    foreach (ConvertedWorkstep workstep in order.FinishedWorksteps)
                    {
                        result.Add(workstep);
                    }
                }
                foreach (ConvertedOrder order in FinishedOrders)
                {
                    foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder)
                    {
                        result.Add(workstep);
                    }
                    foreach (ConvertedWorkstep workstep in order.FinishedWorksteps)
                    {
                        result.Add(workstep);
                    }
                }
                if (result.Count > 0)
                    result = result.OrderBy(x => x.RelativeId).ToList();

                return result;
            }
        }


        public List<ConvertedWorkstep> FinishedWorkSteps
        {
            get
            {
                List<ConvertedWorkstep> result = new List<ConvertedWorkstep>();

                foreach (ConvertedOrder order in Orders)
                {
                    foreach (ConvertedWorkstep workstep in order.FinishedWorksteps)
                    {
                        result.Add(workstep);
                    }
                }

                foreach (ConvertedOrder order in FinishedOrders)
                {
                    foreach (ConvertedWorkstep workstep in order.FinishedWorksteps)
                    {
                        result.Add(workstep);
                    }
                }

                result = result.OrderBy(x => x.RelativeId).ToList();

                return result;
            }
        }

        public Dictionary<string, int> RelativeIds { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RelativeGroupIds { get; set; } = new Dictionary<string, int>();
        public int AmountOfWorkSteps => WorkSteps.Count;
        public int AmountOfNonAssignedWorkSteps => WorkSteps.Count(x => x.WorkstationShould == "");
        public int AmountOfWorkstations => Workstations.Count;

        public ConvertedWorkstep[] NonAssignedWorkSteps => WorkSteps.Where(x => string.IsNullOrEmpty(x.WorkstationShould)).ToArray();

        public ConvertedWorkstep[] NonAssignedWorkStepsAtTheStart
        {
            get
            {
                List<ConvertedWorkstep> result = new List<ConvertedWorkstep>();

                foreach (ConvertedWorkstep workStep in WorkSteps.OrderBy(x => x.RelativeId))
                {
                    string group = workStep.WorkstationGroup;
                    var workstations = Workstations.Where(x => x.WorkstationGroupNumber == group).ToList();
                    if (workstations.Count > 1)
                    {
                        result.Add(workStep);
                    }
                }
                return result.ToArray();

            }
        }

        #region ctor
        public ConvertedData() { }
        #endregion

        #region Methods

        public void AssignRelativeWorkstationGroupNumber()
        {
            int number = 0;
            List<ConvertedWorkstation> workstations = Workstations.OrderBy(x => x.RelativeId).ToList();
            foreach (ConvertedWorkstation workstation in workstations)
            {
                if (!RelativeGroupIds.ContainsKey(workstation.WorkstationGroupNumber))
                {
                    RelativeGroupIds.Add(workstation.WorkstationGroupNumber, number);
                    number++;
                }
                workstation.RelativeWorkgroupId = RelativeGroupIds[workstation.WorkstationGroupNumber];
            }
        }

        public List<ConvertedWorkstep> WorkstepsWhereWorkstationHasToBeDecided
        {
            get
            {
                List<ConvertedWorkstep> result = new List<ConvertedWorkstep>();
                foreach (ConvertedWorkstep step in WorkSteps)
                {
                    if (Workstations.Where(x => x.WorkstationGroupNumber == step.WorkstationGroup).Count() > 1)
                    {
                        result.Add(step);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Return all operations with more than one possible machine
        /// </summary>
        public List<ConvertedWorkstep> OperationsWhereWorkstationHasToBeDecided
        {
            get
            {
                List<ConvertedWorkstep> result = new List<ConvertedWorkstep>();
                foreach (ConvertedOrder o in Orders)
                    foreach (ConvertedWorkstep step in o.WorkstepsInOrder)
                    {
                        int numberOfWorkstations =
                            Workstations.Where(x => x.WorkstationGroupNumber == step.WorkstationGroup).Count();

                        if (numberOfWorkstations > 1) //More than one machine possible to execute one operation
                            result.Add(step);
                    }
                return result;
            }
        }


        public int NumberOfWorkstationsInGroup(string workstationGroup)
        {
            return Workstations.Count(x => x.WorkstationGroupNumber == workstationGroup);
        }
        public ConvertedWorkstation[] WorkstationsInGroup(string workstationGroup)
        {
            return Workstations.Where(x => x.WorkstationGroupNumber == workstationGroup).ToArray();
        }


        public object Clone()
        {
            ConvertedData result = new ConvertedData();
            foreach (KeyValuePair<string, int> pair in RelativeGroupIds)
                result.RelativeGroupIds.Add(pair.Key, pair.Value);
            foreach (KeyValuePair<string, int> pair in RelativeIds)
                result.RelativeIds.Add(pair.Key, pair.Value);


            result.Workstations = new List<ConvertedWorkstation>();
            for (int i = 0; i < Workstations.Count; i++)
                result.Workstations.Add((ConvertedWorkstation)Workstations.ElementAt(i).Clone());


            foreach (ConvertedOrder order in Orders)
            {
                ConvertedOrder cOrder = (ConvertedOrder)order.Clone();
                //foreach (ConvertedWorkstep step in order.WorkstepsInOrder)
                //    cOrder.WorkstepsInOrder.Add(step);

                result.Orders.Add(cOrder);
            }


            foreach (ProducedProductLog producedProductLog in ProductsProduced)
            {
                result.ProductsProduced.Add((ProducedProductLog)producedProductLog.Clone());

            }

            foreach (WorkstepLog log in Logs)
            {
                result.Logs.Add((WorkstepLog)log.Clone());

            }



            foreach (ConvertedOrder order in FinishedOrders)
            {
                ConvertedOrder cOrder = (ConvertedOrder)order.Clone();
                foreach (ConvertedWorkstep step in order.FinishedWorksteps)
                {
                    cOrder.FinishedWorksteps.Add(step);
                }
                result.FinishedOrders.Add(cOrder);
            }

            result.SetConvertedData();
            return result;
        }

        #endregion


        public void SetConvertedData()
        {
            foreach (ConvertedOrder order in Orders)
                order.ConvertedData = this;
            foreach (ConvertedOrder order in FinishedOrders)
                order.ConvertedData = this;
            foreach (ConvertedWorkstation workstation in Workstations)
                workstation.ConvertedData = this;
            foreach (ConvertedWorkstep workStep in WorkSteps)
                workStep.ConvertedData = this;
        }

        public Dictionary<string, List<ConvertedWorkstation>> GetWorkstationGroups()
        {
            Dictionary<string, List<ConvertedWorkstation>> result = new Dictionary<string, List<ConvertedWorkstation>>();
            foreach (ConvertedWorkstation workstation in Workstations)
            {
                if (!result.Keys.Contains(workstation.WorkstationGroupNumber))
                    result.Add(workstation.WorkstationGroupNumber, new List<ConvertedWorkstation>());
                result[workstation.WorkstationGroupNumber].Add(workstation);
            }

            return result;
        }

        /// <summary>
        /// If no previous worksteps have been read in, we assign all worksteps that have a lower relative id as previous worksteps 
        /// </summary>
        public void AssignTaskPreviousWorksteps()
        {
            foreach (ConvertedOrder order in Orders)
            {
                List<int> distinctIds = order.WorkstepsInOrder.Select(x => x.RelativeId).Distinct().ToList();
                foreach (ConvertedWorkstep workstep in order.WorkstepsInOrder.OrderByDescending(x => x.RelativeId))
                {
                    if (workstep.PreviousWorkSteps.Count == 0)
                    {
                        workstep.PreviousWorkSteps = distinctIds.Where(x => x < workstep.RelativeId).ToList();
                    }
                }
            }
        }
    }
}
