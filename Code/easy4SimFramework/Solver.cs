using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HEAL.Attic;
using TypeInfo = System.Reflection.TypeInfo;

namespace Easy4SimFramework
{
    using pair = KeyValuePair<long, CSimBase>;
    /// <summary>
    /// Used to run the simulation until a specific time or until the simulation has finished.
    /// </summary>
    [StorableType("C76289EA-5BFC-420C-819C-011B340E69EF")]
    public class Solver : ICloneable
    {
        #region Properties
        [Storable]
        public Environment Environment { get; set; }
        [Storable]
        public Logger Logger { get; set; }
        [Storable]
        public SolverSettings SolverSettings { get; set; }
        [Storable]
        public SimulationStatistics SimulationStatistics { get; set; }
        [Storable]
        public SimulationObjects SimulationObjects { get; set; }
        #endregion Properties
        #region Events
        public event Action UpdateVisualizationEvent;
        public void RaiseUpdateVisualizationEvent() => UpdateVisualizationEvent?.Invoke();
        #endregion Events
        #region Constructor
        /// <summary>
        /// Constructor for HEAL.Attic Serialization
        /// </summary>
        /// <param name="flag"></param>
        [StorableConstructor]
        public Solver(StorableConstructorFlag flag) { }
        public Solver(SolverSettings settings)
        {
            Environment = settings.Environment;
            SimulationObjects = settings.SimulationObjects;
            Logger = settings.Logger;
            SolverSettings = settings;
            SimulationStatistics = settings.Statistics;
        }

        /// <summary>
        /// Constructor that is used for the clone method
        /// </summary>
        private Solver()
        {
                
        }

        #endregion Constructor
        #region Methods
        /// <summary>
        /// Initialize all simulation objects.
        /// Should be called before the first run of the simulation.
        /// </summary>
        public void Init()
        {
            List<long> keys = new List<long>(SimulationObjects.SimulationList.Keys);
            keys.Sort();
            foreach (long k in keys)
            {
                CSimBase cSimBase = SimulationObjects.SimulationList[k];
                cSimBase.Initialize();
            }

            SimulationObjects.VisualizationsList.Values.ToList().ForEach(y => y.UpdateVisualization());
        }
        private void SaveExecute(Action method, string name)
        {
            try
            {
                method();
            }
            catch (Exception e)
            {
                Environment.UpdateStatus(name, e.Message);
                throw;
            }
        }

        /// <summary>
        /// Reset the simulation time in the environment.
        /// Resets the simulation objects to the start state. This means Start() is called for every ISimBase.
        /// Furthermore discrete and dynamic calculations are called once (time = 0).
        /// </summary>
        public void Start()
        {
            Environment.SimulationTime = 0;
            if (Logger != null)
                foreach (pair pair in SimulationObjects.SimulationList)
                    pair.Value.Logger = Logger.CreateLoggerForName(pair.Value.Name);
            //Call start
            List<CSimBase> componentList = SimulationObjects.SimulationList.Values.ToList();

            foreach (CSimBase iSimBase in componentList)
                SaveExecute(() => iSimBase.Start(), iSimBase.Name);
            foreach (ISimBase iSimBase in SimulationObjects.SimulationList.Values)
                SaveExecute(() => iSimBase.DynamicCalculation(), iSimBase.Name);
            foreach (ISimBase iSimBase in SimulationObjects.SimulationList.Values)
                SaveExecute(() => iSimBase.DiscreteCalculation(), iSimBase.Name);
            Visualization();
            Logger?.InitializeLogging();
            Environment.SimulationFinished = false;
        }
        /// <summary>
        /// Call the End() method of all simulations objects.
        /// Close all logging streams
        /// </summary>
        public void End()
        {
            foreach (pair p in SimulationObjects.SimulationList)
                p.Value.End();
            Logger?.StopLogging();
        }
        /// <summary>
        /// Calculate until a given simulation time is reached.
        /// </summary>
        /// <param name="time"></param>
        public void CalculateTo(long time)
        {
            if (Environment.SimulationFinished)
            {
                End();
                return;
            }
            while (Environment.SimulationTime < time)
            {
                DerivedCalculations();
                Environment.SimulationTime += Environment.IncreaseTime;
                DynamicCalculation();
                Visualization();
                DiscreteCalculation();
                Delay();
                Logging();
                if (!Environment.SimulationFinished) continue;
                End();
                return;
            }
        }
        /// <summary>
        /// Calculate until the simulation has finished.
        /// </summary>
        public void CalculateFinish()
        {
            while (!Environment.SimulationFinished && Environment.SimulationTime < Environment.FinishSimulationTime)
            {
                DerivedCalculations();
                Environment.SimulationTime += Environment.IncreaseTime;
                DynamicCalculation();
                Visualization();
                DiscreteCalculation();
                Delay();
                Logging();
            }
            End();
        }
        private void DynamicCalculation()
        {
            foreach (ISimBase iSimBase in SimulationObjects.SimulationList.Values)
                SaveExecute(() => iSimBase.DynamicCalculation(), iSimBase.Name);
        }
        private void DiscreteCalculation()
        {
            ISimBase iSimBase = SimulationObjects.EventQueue.GetNextEvent(Environment.SimulationTime, SimulationObjects);
            while (iSimBase != null)
            {
                SaveExecute(() => iSimBase.DiscreteCalculation(), iSimBase.Name);
                iSimBase = SimulationObjects.EventQueue.GetNextEvent(Environment.SimulationTime, SimulationObjects);
            }
        }
        private void Delay()
        {
            if (Environment.SimulationTime >= Environment.TotalDelayTime)
            {
                Environment.TotalDelayTime += Environment.IncreaseDelayTime;
                Thread.Sleep(Environment.Delay);
            }
        }
        private void Logging()
        {
            if (Environment.SimulationTime >= Environment.TotalLoggingTime)
            {
                Environment.TotalLoggingTime += Environment.LoggingTimeIncrease;
                Logger?.DynamicLogging();
            }
        }
        private void Visualization()
        {
            if (Environment.SimulationTime < Environment.TotalUpdateTime) return;
            Environment.TotalUpdateTime += Environment.UpdateTimeIncrease;
            foreach (IBaseVisualizationModel baseVisualizationModel in SimulationObjects.VisualizationsList.Values)
            {
                SaveExecute(() => baseVisualizationModel.UpdateVisualization(), baseVisualizationModel.UniqueName);
            }
            RaiseUpdateVisualizationEvent();
        }
        private void DerivedCalculations()
        {
            //Integration -> always in the smallest possible step
            for (long i = Environment.SimulationTime + 1; i <= Environment.SimulationTime + Environment.IncreaseTime; i++)
            {
                foreach (ISimBase iSimBase in SimulationObjects.DerivedSimulationList.Values)
                {
                    SaveExecute(() => iSimBase.DerivedCalculation(), iSimBase.Name);
                }
                //TODO: DER_REAL
                //foreach (valpair p in Env.derval)
                //{
                //    p.Value.calc_dev(1);
                //}
            }
        }

        public static void RunSimulationParallel(SolverSettings settings, int numberOfThreads, int numberOfParallelRuns)
        {
            if (settings == null) return;

            Task.Factory.StartNew(() =>
            {
                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads };
                Parallel.For(0, numberOfParallelRuns, options, index =>
                {
                    SolverSettings currentThreadSettings = settings.Clone();
                    currentThreadSettings.Logger.ChangeLogPathBasedOnThreadNumber(index);
                    Task t = Task.Factory.StartNew(() => RunSimulation(currentThreadSettings));
                    Task.WaitAll(t);
                });
            });

        }
        public static void RunSimulation(SolverSettings settings)
        {
            if (settings == null) return;
            Solver solver = new Solver(settings);
            solver.Init();
            solver.Start();
            for (long currentSimulationTime = settings.Environment.SimulationTime;
                currentSimulationTime <= settings.Environment.FinishSimulationTime;
                currentSimulationTime += settings.Environment.IncreaseTime)
            {
                solver.CalculateTo(currentSimulationTime);
                settings.Environment.SimulationTime = currentSimulationTime;
            }
        }

        public static void RunFinish(SolverSettings settings)
        {
            if (settings == null) return;
            Solver solver = new Solver(settings);
            solver.Init();
            solver.Start();
            long currentSimulationTime = settings.Environment.SimulationTime;
            while (!settings.Environment.SimulationFinished)
            {
                solver.CalculateTo(currentSimulationTime);
                settings.Environment.SimulationTime = currentSimulationTime;
                currentSimulationTime += settings.Environment.IncreaseTime;
            }
        }


        /// <summary>
        /// Used for the optimization in HeuristicLab
        /// No Init is called here, needs to be called in the heursiticLab plugin
        /// </summary>
        public void RunFinishEventOnly()
        {
            Start();

            while (!Environment.SimulationFinished)
            {
                List<ISimBase> iSimBase = SimulationObjects.EventQueue.GetAllNextEvents(Environment.SimulationTime, SimulationObjects, out long NextEventTime);
                if (NextEventTime == -1)
                    break;
                SolverSettings.Environment.SimulationTime = NextEventTime;
                foreach (ISimBase simBase in iSimBase)
                    simBase.DiscreteCalculation();
            }
            End();
        }


        public void LoadVisualizationTypes(string path)
        {
            //Get all Assemblies
            string[] files = Directory.GetFiles(path, "*.dll");
            List<Assembly> allAssemblies = new List<Assembly>();
            foreach (string s in files)
            {
                try
                {
                    var x = AppDomain.CurrentDomain.GetAssemblies();

                    string fileName = Path.GetFileNameWithoutExtension(Path.GetFullPath(s));
                    bool ignore = false;
                    foreach (Assembly assembly in x)
                    {
                        if (assembly.FullName.Contains(fileName))
                        {
                            ignore = true;
                            break;
                        }
                    }

                    if (ignore)
                    {
                        continue;
                    }

                    Assembly a = Assembly.LoadFile(Path.GetFullPath(s));
                    allAssemblies.Add(a);
                    //Console.WriteLine(s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach (Assembly a in allAssemblies)
            {
                try
                {
                    foreach (TypeInfo t in a.DefinedTypes)
                    {
                        if (t.GetCustomAttributes().Count() == 0) continue;
                        foreach (Attribute attr in t.GetCustomAttributes())
                        {

                            if (!(attr is VisualizationType temp)) continue;
                            bool contains = false;
                            VisualizationTypes tempType = new VisualizationTypes(temp.WindowType,
                                temp.ClassType,
                                t,
                                temp.LayoutType);

                            foreach (VisualizationTypes types in this.SimulationObjects.VisualizationTypeList)
                            {
                                if (!tempType.Equals(types)) continue;
                                contains = true;
                                break;
                            }

                            if (!contains)
                            {
                                this.SimulationObjects.VisualizationTypeList
                                    .Add(tempType);
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is ReflectionTypeLoadException le)
                    {
                        var loaderExceptions = le.LoaderExceptions;
                        foreach (Exception exception in loaderExceptions)
                        {
                            string s = exception.Message;
                        }
                    }
                    else
                    {

                        Console.WriteLine(e);
                    }
                }
            }

        }
        public void OpenVisualizationForCurrentSimulation() { }
        #endregion Methods

        public object Clone()
        {
            return new Solver(SolverSettings.Clone());
        }
    }
}
