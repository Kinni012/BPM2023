using System;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.ProcessMining
{
    public class ProcessMiningEvent
    {
        public string ActivityKey { get; set; }
        public string TraceIdentifier { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ProcessMiningEvent()
        {

        }


        public ProcessMiningEvent(string activityKey, DateTime startTime, DateTime endTime, string traceIdentifier)
        {
            ActivityKey = activityKey;
            StartTime = startTime;
            EndTime = endTime;
            TraceIdentifier = traceIdentifier;
        }

        public override string ToString()
        {
            return $"Trace: {TraceIdentifier} ActivityKey: {ActivityKey}";
        }
    }
}
