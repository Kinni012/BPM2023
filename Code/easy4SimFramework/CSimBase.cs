using System;
using HEAL.Attic;
using log4net;

namespace Easy4SimFramework
{
    [StorableType("4F66D84C-8D8C-4CD5-ADAF-87C2D394C758")]
    public abstract class CSimBase : ISimBase, IGuid
    {
        
        #region Properties
        [Storable]
        public Guid CurrentGuid { get; set; }
        [Storable]
        public ILog Logger { get; set; }
        [Storable]
        public long Index { get; set; }
        [Storable]
        public string Name { get; set; }
        [Storable]
        public ParameterInt LogLevel { get; set; }
        [Storable]
        public SolverSettings Settings { get; set; }
        [Storable]
        public virtual string LogName
        {
            get => Name;
            set => Name = value;
        }
        [Storable]
        public bool OptimizeForSimulation{ get; set; }

        #endregion Properties

        #region Ctor
        [StorableConstructor]
        protected CSimBase(StorableConstructorFlag flag){}

        protected CSimBase()
        {
            InitGuid();
            Index = 0;
            Name = "";
            LogLevel = new ParameterInt();
        }
        /// <summary>
        /// Register the CSimBase in the passed Environment.
        /// Set simulation index and name.
        /// </summary>
        /// <param name="index"> simulations index of the CSimBase </param>
        /// <param name="name"> simulations name of the CSimBase</param>
        /// <param name="settings"> solver settings, contain the SimulationsObjects where this CSimBase is added </param>
        protected CSimBase(long index, string name, SolverSettings settings)
        {
            InitGuid();
            Index = index;
            Name = name;
            Settings = settings;
            settings?.SimulationObjects?.AddSimulationsObject(this);
        }


        #endregion

        #region Methods

        private void InitGuid()
        {
            CurrentGuid = Guid.NewGuid();
        }
        /// <summary>
        /// Called on solver.Init(), can be used to dynamically add components during the runtime of the simulation
        /// </summary>
        public virtual void Initialize(){}
        /// <summary>
        /// Called before the simulation is started the first time
        /// </summary>
        public virtual void Start(){}

        /// <summary>
        /// Called at the end of the simulation.
        /// </summary>
        public virtual void End(){}
        /// <summary>
        /// Code called on component event.
        /// </summary>
        public virtual void DiscreteCalculation(){}
        /// <summary>
        /// Code called in ever simulation step.
        /// </summary>
        public virtual void DynamicCalculation(){}
        /// <summary>
        /// Used for derived simulation, not used and tested at the moment!
        /// </summary>
        public virtual void DerivedCalculation(){}
        /// <summary>
        /// Override and return a clone of the current simulation component.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        /// <summary>
        /// Add event for the component in the environment at current simulation time + passed value
        /// </summary>
        public void AddSelfEventAtSimulationTimePlusValue(long value)
        {
            Settings.SimulationObjects.EventQueue.Add(this, Settings.Environment.SimulationTime + value);
        }
        
        /// <summary>
        /// Add event for the component in the environment at passed time step
        /// </summary>
        public void AddSelfEventAtSimulationTime(long simulationTime = -1)
        {
            if (simulationTime == -1)
                simulationTime = Settings.Environment.SimulationTime;
            Settings.SimulationObjects.EventQueue.Add(this, simulationTime);
        }

        /// <summary>
        /// Legacy support for some libraries.
        /// Can be used to check if an action should happen in this simulation step.
        /// If the compareTime is in the future, we add an event in the compareTime simulation step.
        /// </summary>
        /// <param name="compareTime"></param>
        /// <returns></returns>
        public bool SimulationTimeGreaterOrEqualThan(long compareTime)
        {
            if (Settings.Environment.SimulationTime >= compareTime)
                return true;  //action can happen in this simulation step
            AddSelfEventAtSimulationTime(compareTime); //add event for simulation step where action should happen
            return false;
        }
        
        #endregion
    }
}
