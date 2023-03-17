using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Easy4SimFramework;

namespace CobotAssignmentAndJobShopSchedulingProblem
{
    /// <summary>
    /// Easy4Sim class to read in from xml problem definition file and pass it to the next component
    /// </summary>
    public class ProjectLoader : CSimBase
    {
        /// <summary>
        /// File content of the xml problem definition
        /// </summary>
        public ParameterString FileContent { get; set; }
        /// <summary>
        /// Connection to the Project processor
        /// In this connection, all read data is set
        /// </summary>
        public OutEventObject ReadData { get; set; }
        public ConvertedData ConvertedData { get; set; }


        #region constructor
        public ProjectLoader() => InitializeParameters();
        public ProjectLoader(long i, string n, SolverSettings settings) : base(i, n, settings) => InitializeParameters();
        #endregion constructor

        public override void Initialize()
        {
            try
            {
                ConvertedData result = new ConvertedData();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(FileContent.Value);

                XmlNodeList workstations = doc.GetElementsByTagName("Workstation");
                XmlNodeList orders = doc.GetElementsByTagName("Order");
                List<ConvertedWorkstation> workstationList = new List<ConvertedWorkstation>();
                foreach (XmlNode workstation in workstations)
                {
                    ConvertedWorkstation ws = new ConvertedWorkstation();
                    foreach (XmlNode child in workstation.ChildNodes)
                    {
                        if (child.Name.ToUpper() == "ID")
                        {
                            int id = Convert.ToInt32(child.InnerXml);
                            ws.RelativeId = id;

                            continue;
                        }

                        if (child.Name.ToUpper() == "RELATIVEWORKGROUPID")
                        {
                            int relativeId = Convert.ToInt32(child.InnerXml);
                            ws.RelativeWorkgroupId = relativeId;
                            continue;
                        }
                        if (child.Name.ToUpper() == "GUID")
                        {
                            Guid id = Guid.Parse(child.InnerXml);
                            ws.Id = id;
                            continue;
                        }

                        if (child.Name.ToUpper() == "NAME")
                        {
                            ws.WorkstationNumber = child.InnerXml;
                            continue;
                        }

                        if (child.Name.ToUpper() == "WORKSTATIONGROUP")
                        {
                            ws.WorkstationGroupNumber = child.InnerXml;
                            continue;
                        }

                        if (child.Name.ToUpper() == "COSTFACTOR")
                        {
                            ws.CostProcessingPerSecond = Convert.ToDouble(child.InnerXml, CultureInfo.InvariantCulture);
                            ws.CostSetupToolPerSecond = Convert.ToDouble(child.InnerXml, CultureInfo.InvariantCulture);
                            continue;
                        }

                        if (child.Name.ToUpper() == "TIMEFACTOR")
                        {
                            ws.SpeedFactorWorking = Convert.ToDouble(child.InnerXml, CultureInfo.InvariantCulture);
                            ws.SpeedFactorSetup = Convert.ToDouble(child.InnerXml, CultureInfo.InvariantCulture);
                            continue;
                        }

                        if (child.Name.ToUpper() == "CAPACITYTYP" || child.Name.ToUpper() == "CapacityType".ToUpper())
                        {
                            ws.CapacityTyp = Convert.ToInt32(child.InnerXml);
                            continue;
                        }
                        if (child.Name.ToUpper() == "CAPACITY")
                        {
                            ws.Capacity = Convert.ToInt32(child.InnerXml);
                            continue;
                        }
                        if (child.Name.ToUpper() == "DESCRIPTION")
                        {
                            ws.Description = child.InnerXml;
                            continue;
                        }

                        if (child.Name.ToUpper() == ("BreakingChance").ToUpper())
                        {
                            ws.BreakingChance = Convert.ToDouble(child.InnerXml);
                        }
                    }
                    workstationList.Add(ws);
                }

                result.Workstations = workstationList.ToList();

                List<ConvertedOrder> orderList = new List<ConvertedOrder>();
                foreach (XmlNode orderXml in orders)
                {
                    ConvertedOrder order = new ConvertedOrder();
                    //int orderRelationNumber = 0;
                    foreach (XmlNode childXml in orderXml.ChildNodes)
                    {
                        if (childXml.Name.ToUpper() == "ID")
                        {
                            order.OrderNumber = childXml.InnerXml;
                            continue;
                        }

                        if (childXml.Name.ToUpper() == "NAME")
                        {
                            order.OrderNumber = childXml.InnerXml;
                            continue;
                        }

                        if (childXml.Name.ToUpper() == "OrderGroupNumber")
                        {
                            order.OrderGroupNumber = childXml.InnerXml;
                            continue;
                        }

                        if (childXml.Name.ToUpper() == "TASK")
                        {
                            ConvertedWorkstep workstep = new ConvertedWorkstep();
                            //workstep.OrderRelationNumber = orderRelationNumber;
                            workstep.OrderNumber = order.OrderNumber;
                            //orderRelationNumber++;
                            foreach (XmlNode taskProperty in childXml.ChildNodes)
                            {
                                if (taskProperty.Name.ToUpper() == "Id".ToUpper())
                                {
                                    workstep.RelativeId = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "Amount".ToUpper())
                                {
                                    workstep.Amount = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "SetupTime".ToUpper())
                                {
                                    workstep.SetupTime = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "DeSetupTime".ToUpper())
                                {
                                    workstep.DeSetupTime = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "ProductionTime".ToUpper())
                                {
                                    workstep.ProductionTime = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "OrderName".ToUpper())
                                {
                                    workstep.OrderNumber = taskProperty.InnerXml;
                                    continue;
                                }
                                if (taskProperty.Name.ToUpper() == "RelationNumber".ToUpper())
                                {
                                    workstep.OrderRelationNumber = Convert.ToInt32(taskProperty.InnerXml);
                                    continue;
                                }
                                if (taskProperty.Name.ToUpper() == "PreviousWorksteps".ToUpper())
                                {

                                    string[] parts = taskProperty.InnerXml.Split(',');
                                    foreach (string part in parts.Where(x => !string.IsNullOrWhiteSpace(x)))
                                        workstep.PreviousWorkSteps.Add(Convert.ToInt32(part));
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "WorkgroupShould".ToUpper())
                                {
                                    workstep.WorkstationGroup = taskProperty.InnerXml;
                                    workstep.WorkstationShould = taskProperty.InnerXml;
                                    continue;
                                }

                                if (taskProperty.Name.ToUpper() == "AlternativeWorkStationGroup".ToUpper())
                                    workstep.AlternativeWorkgroup = taskProperty.InnerXml;


                            }

                            if (!workstep.WorkstationShouldRelative.HasValue && workstep.WorkstationGroup != "")
                            {
                                foreach (ConvertedWorkstation workstation in workstationList)
                                {
                                    if (workstation.WorkstationGroupNumber == workstep.WorkstationGroup)
                                    {
                                        workstep.WorkstationShouldRelative = workstation.RelativeWorkgroupId;
                                    }
                                }
                            }
                            order.WorkstepsInOrder.Add(workstep);

                        }
                    }
                    orderList.Add(order);
                }

                result.Orders = orderList;

                result.AssignRelativeWorkstationGroupNumber();
                result.AssignTaskPreviousWorksteps();

                ReadData.Set(result);

            }
            catch (Exception e)
            {
                Console.WriteLine(@"Error in ProjectLoader");
                Console.WriteLine(e);
                throw;
            }
        }

        private void InitializeParameters()
        {
            FileContent = new ParameterString("");
            ReadData = new OutEventObject(this);
            //Set optimization to false by default, set to true in hl plugin
            //Initialize input and output connections here
        }


        public override object Clone()
        {
            ProjectLoader result = new ProjectLoader(Index, Name, Settings);
            result.FileContent = FileContent;
            result.CurrentGuid = CurrentGuid;
            result.OptimizeForSimulation = OptimizeForSimulation;
            if (ConvertedData != null)
                result.ConvertedData = (ConvertedData)ConvertedData.Clone();
            if (ReadData.Value != null)
                result.ReadData.Set(result.ConvertedData);
            return result;
        }
    }
}
