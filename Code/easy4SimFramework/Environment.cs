using System;
using HEAL.Attic;

namespace Easy4SimFramework
{
    /// <inheritdoc />
    /// <summary>
    /// Has general settings for the Simulation.
    /// </summary>
    [StorableType("F4AF3B3E-FE72-4244-BB62-92A6C65AF6C2")]
    public class Environment : ICloneable<Environment>
    {
        #region Additional Types

        [StorableType("2DF9EB20-1F4C-4BFF-89F5-43C0C0B81E1E")]
        public class StringEventArgs : EventArgs
        {
            [Storable]
            public string Message { get; set; }
            [StorableConstructor]
            public StringEventArgs(StorableConstructorFlag flag) { }
            public StringEventArgs(string message) => Message = message;
        }

        [StorableType("3F687790-D7F1-4684-8875-53C299FCC91C")]
        public class LogEventArgs : EventArgs
        {
            [Storable]
            public string Message { get; set; }
            [Storable]
            public string Sender { get; set; }
            [Storable]
            public long SimulationTime { get; set; }
            [Storable]
            public LoggingCategory Category { get; set; }

            [StorableConstructor]
            public LogEventArgs(StorableConstructorFlag flag) { }
            public LogEventArgs(string sender, string message, long simulationTime, LoggingCategory category = LoggingCategory.Info)
            {
                Sender = sender;
                Message = message;
                SimulationTime = simulationTime;
                Category = category;
            }

        }

        [StorableType("290431B7-AEF2-4174-AF05-AC25CFC2694C")]
        public enum LoggingCategory { Error, Warning, Info, Statistics };

        #endregion

        #region Constants
        //Naming convention for Excel Sheets that are should be loaded with this Framework
        //********************* Excel Sheet names for loading *********************
        public const string ObjectSheetName = "Objects";
        public const string EnvironmentSheetName = "Environment";
        public const string ParameterSheetName = "Params";
        public const string ConnectionSheetName = "Connections";
        public const string VisualizationSheetName = "Visualization";
        //*************************************************************************
        #endregion

        #region Ctor
        [StorableConstructor]
        private Environment(StorableConstructorFlag flag) { }

        public Environment()
        {
            Initialize();
        }
        #endregion

        #region Properties
        [Storable]
        public string SimulationFile { get; set; }

        /// <summary>
        /// Current simulation time of this environment object
        /// </summary>
        [Storable]
        public long SimulationTime { get; set; }

        public DateTime SimulationDateTime
        {
            get
            {
                return TimeStart.Add(new TimeSpan(0, 0, 0, 0, Convert.ToInt32(SimulationTime) * TimeScale));
            }
        }
        /// <summary>
        /// Simulation stops when this time is reached
        /// </summary>
        [Storable]
        public long FinishSimulationTime { get; set; }

        /// <summary>
        /// Time scale of this simulation, only used for ui, does not affect the internal time increase
        /// </summary>
        [Storable]
        public int TimeScale { get; set; }
        
        /// <summary>
        /// Start date of the simulation
        /// </summary>
        [Storable]
        public DateTime TimeStart { get; set; }

        /// <summary>
        /// End date of the simulation
        /// </summary>
        [Storable]
        public DateTime TimeEnd { get; set; }

        /// <summary>
        /// Time increase per simulation Step.
        /// </summary>
        [Storable]
        public int IncreaseTime { get; set; }

        /// <summary>
        /// Summed up delay time.
        /// </summary>
        [Storable]
        public long TotalDelayTime { get; set; }

        /// <summary>
        /// Time increase before a delay happens.
        /// </summary>
        [Storable]
        public long IncreaseDelayTime { get; set; }

        /// <summary>
        /// Delay length.
        /// </summary>
        [Storable]
        public int Delay { get; set; }

        /// <summary>
        /// Flag if the simulation reached its final state.
        /// </summary>
        [Storable]
        public bool SimulationFinished { get; set; }

        /// <summary>
        /// Summed up update time.
        /// </summary>
        [Storable]
        public long TotalUpdateTime { get; set; }

        /// <summary>
        /// Update time increase.
        /// </summary>
        [Storable]
        public long UpdateTimeIncrease { get; set; }

        /// <summary>
        /// Summed up Logging time.
        /// </summary>
        [Storable]
        public long TotalLoggingTime { get; set; }

        /// <summary>
        /// Time increase before a logging happens.
        /// </summary>
        [Storable]
        public long LoggingTimeIncrease { get; set; }

        /// <summary>
        /// EnvironmentId
        /// </summary>
        [Storable]
        public string EnvironmentGuid { get; set; }

        #endregion Properties

        public delegate void ErrorDelegate(object sender, StringEventArgs e);
        public delegate void LogDelegate(object sender, LogEventArgs e);

        /// <summary>
        /// Access to the last Errors that occured during the run of the last simulation
        /// </summary>
        public event ErrorDelegate ErrorOccured;
        public event LogDelegate EventLogging;

        #region Methods
        public void NewEventLog(string sender, string message)
        {
            EventLogging?.Invoke(sender, new LogEventArgs(sender, message, SimulationTime));
        }
        public void NewEventLog(string sender, string message, LoggingCategory category)
        {
            EventLogging?.Invoke(sender, new LogEventArgs(sender, message, SimulationTime, category));
        }
        public void UpdateStatus(string sender, string status)
        {
            // Make sure someone is listening to event
            ErrorOccured?.Invoke(sender, new StringEventArgs(status));
        }
        public Environment Clone()
        {
            Environment result = new Environment
            {
                SimulationTime = SimulationTime,
                FinishSimulationTime = FinishSimulationTime,
                IncreaseTime = IncreaseTime,
                Delay = Delay,
                IncreaseDelayTime = IncreaseDelayTime,
                LoggingTimeIncrease = LoggingTimeIncrease,
                SimulationFile = SimulationFile,
                SimulationFinished = SimulationFinished,
                TimeEnd = TimeEnd,
                TimeScale = TimeScale,
                TimeStart = TimeStart,
                TotalDelayTime = TotalDelayTime,
                TotalLoggingTime = TotalLoggingTime,
                TotalUpdateTime = TotalUpdateTime,
                UpdateTimeIncrease = UpdateTimeIncrease,
                EnvironmentGuid = EnvironmentGuid
            };
            result.ErrorOccured += ErrorOccured;

            return result;
        }
        private void Initialize()
        {
            SimulationTime = 0;
            FinishSimulationTime = 10000;
            IncreaseTime = 1;
            IncreaseDelayTime = 1000;
            UpdateTimeIncrease = 1;
            LoggingTimeIncrease = 1;
            TotalDelayTime = 0;
            TotalLoggingTime = 0;
            TotalUpdateTime = 0;
            Delay = 0;
            SimulationFinished = false;
            SimulationFile = "";
            EnvironmentGuid = Guid.NewGuid().ToString();
        }
        public void Reset()
        {
            Initialize();
        }
        #endregion
    }
}
