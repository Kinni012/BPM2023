using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HEAL.Attic;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;

namespace Easy4SimFramework
{
    [StorableType("D5007656-35A3-4FFE-A27A-0888E60F22DA")]
    public class Logger : ICloneable<Logger>
    {
        [Storable]
        public int ThreadNumber { get; set; }
        [Storable]
        public string LogPath { get; set; }
        [StorableConstructor]
        public Logger(StorableConstructorFlag flag){}
        public Logger(string logPath)
        {
            LogPath = logPath;
        }
        public ILog CreateLoggerForName(string name)
        {
            try
            {
                FileInfo configFileInfo = new FileInfo(".\\..\\..\\..\\Easy4SimFramework\\log4net.config");
                //Trace.WriteLine("Create logger for name:" + name);
                ILoggerRepository repository;
                if (LogManager.GetAllRepositories().Any(x => x.Name == (ThreadNumber + name)))
                    repository = LogManager.GetAllRepositories().FirstOrDefault(x => x.Name == ThreadNumber + name);
                else
                    repository = LogManager.CreateRepository(ThreadNumber + name);

                //Trace.WriteLine("repository name:" + repository?.Name);

                XmlConfigurator.Configure(repository, configFileInfo);
                if (repository != null &&
                    repository.GetAppenders().Length > 0 &&
                    repository.GetAppenders()[0] is FileAppender appender)
                {
                    appender.File = $"{LogPath}\\out\\{ThreadNumber}\\{name}.csv";
                    ((PatternLayout)appender.Layout).Header = "Date;Level;Simulation Time;Message;" + System.Environment.NewLine;
                }


                ILog l = LogManager.GetLogger(repository?.Name, ThreadNumber.ToString());
                l?.Info("0\";\"Created LogFile");
                //Trace.WriteLine("created logger:" + l.Logger.Name);
                return l;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public void ChangeLogPathBasedOnThreadNumber(int threadNumber)
        {
            ThreadNumber = threadNumber;
        }
        public void InitializeLogging(){}
        public void StopLogging() { }
        public void DynamicLogging(){}
        public static void ShutdownLoggers()
        {
        //    ILog[] loggers = LogManager.GetCurrentLoggers();
        //    LogManager.Shutdown();
        }
        public Logger Clone()
        {
            Logger logger = new Logger(LogPath) {LogPath = LogPath, ThreadNumber = ThreadNumber};
            return logger;
        }
        public void LogInfo(ISimBase simBase, string s)
        {
            if (simBase is CSimBase b)
                b.Logger?.Info($"{simBase.Settings.Environment.SimulationTime}\";\"" + s);
        }
        public void LogWarning(ISimBase simBase, string s)
        {
            if (simBase is CSimBase b)
                b.Logger?.Warn($"{simBase.Settings.Environment.SimulationTime}\";\"" + s);
        }
        public void LogError(ISimBase simBase, string s)
        {
            if (simBase is CSimBase b)
                b.Logger?.Error($"{simBase.Settings.Environment.SimulationTime}\";\"" + s);
        }
    }
}
