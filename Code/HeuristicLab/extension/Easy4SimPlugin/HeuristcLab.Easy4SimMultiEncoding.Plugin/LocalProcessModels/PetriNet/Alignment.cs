using System;

namespace HeuristicLab.Easy4SimMultiEncoding.Plugin.LocalProcessModels.PetriNet
{
    public class Alignment : ICloneable
    {
        /// <summary>
        /// Move on model.
        /// A move on model can for example be a hidden transition that is fired from end to start.
        /// </summary>
        public string MoveOnModel { get; set; }
        /// <summary>
        /// Move on Log.
        /// A move on log is a transition that can not be mapped to the model.
        /// </summary>
        public string MoveOnLog { get; set; }
        /// <summary>
        /// The number of transitions that can be fired in each state is an important measure.
        /// </summary>
        public int NumberOfEnabledTransitions { get; set; }

        public string Trace { get; set; }

        public Alignment(string moveOnModel, string moveOnLog, int enabledTransitions)
        {
            MoveOnModel = moveOnModel;
            MoveOnLog = moveOnLog;
            NumberOfEnabledTransitions = enabledTransitions;
        }

        public object Clone()
        {

            Alignment result = new Alignment(MoveOnModel, MoveOnLog, NumberOfEnabledTransitions);
            return result;
        }
    }
}
