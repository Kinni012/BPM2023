using System;
using HEAL.Attic;

namespace Easy4SimFramework
{
    /// <summary>
    /// Contains all information, that the solver needs to calculate the simulation.
    /// This includes environmental settings, simulation objects and a log path.
    /// </summary>
    [StorableType("430858C2-139C-42F7-A798-75092903DB8B")]
    public class SolverSettings : ICloneable<SolverSettings>
    {
        [Storable]
        public Guid Guid { get; set; }

        [StorableConstructor]
        public SolverSettings(StorableConstructorFlag flag){}

        public SolverSettings(){}

        public SolverSettings(SolverSettings settings)
        {
            SolverSettings s = settings.Clone();
            Statistics = s.Statistics;
            Environment = s.Environment;
            SimulationObjects = s.SimulationObjects;
            Logger = s.Logger;

        }
        public SolverSettings(Environment environment, SimulationObjects simulationsObjects, Logger logger)
        {
            Guid = System.Guid.NewGuid();
            Environment = environment;
            SimulationObjects = simulationsObjects;
            Logger = logger;
            Statistics = new SimulationStatistics();
        }

        public SolverSettings(Environment environment, SimulationObjects simulationsObjects, Logger logger, SimulationStatistics statistics)
        {
            Guid = System.Guid.NewGuid();
            Environment = environment;
            SimulationObjects = simulationsObjects;
            Logger = logger;
            Statistics = statistics ?? new SimulationStatistics();
            
        }

        [Storable]
        public SimulationStatistics Statistics { get; set; }
        /// <summary>
        /// Environmental Settings used in the Simulation.
        /// </summary>
        [Storable]
        public Environment Environment { get; set; }
        /// <summary>
        /// Contains all simulations objects and events.
        /// </summary>
        [Storable]
        public SimulationObjects SimulationObjects { get; set; }
        /// <summary>
        /// Path to the log file of this simulation.
        /// </summary>
        [Storable]
        public Logger Logger{ get; set; }

        public SolverSettings Clone()
        {
            SolverSettings settings = new SolverSettings(Environment?.Clone(), SimulationObjects?.Clone(), Logger?.Clone(), Statistics?.Clone());
            settings.Guid = Guid;
            settings.SimulationObjects.UpdateSolverSettingsOfAllItems(settings);
            settings.SimulationObjects.UpdateAllConnections(settings);
            settings.SimulationObjects.ConnectAllLinks();
            foreach (var visualizationModel in settings.SimulationObjects.VisualizationsList)
            {
                if(visualizationModel.Value is VisualizationInitializedWithSolver vi)
                 vi.UpdateReference();
            }


            return settings;
        }
    }
}
