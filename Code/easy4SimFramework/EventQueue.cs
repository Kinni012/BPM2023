using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;

namespace Easy4SimFramework
{
    /// <summary>
    /// Is used for event handling in the simulation.
    /// </summary>
    [StorableType("A10D4D5E-1ABF-4D07-B1B0-77215BB41040")]
    public class EventQueue : ICloneable<EventQueue>
    {
        /// <summary>
        /// Sorted list with all queued events.
        /// </summary>
        [Storable]
        public SortedList<long, List<long>> EventList { get; set; } = new SortedList<long, List<long>>();
        public EventQueue() { }

        [StorableConstructor]
        public EventQueue(StorableConstructorFlag flag){}
        /// <summary>
        /// Adds an event at a specific time to the environment.
        /// </summary>
        /// <param name="iSimBase"></param>
        /// <param name="simulationTime"></param>
        public void Add(ISimBase iSimBase, long simulationTime)
        {
            if (!EventList.ContainsKey(simulationTime))
                EventList.Add(simulationTime, new List<long>());
            if (!EventList[simulationTime].Contains(iSimBase.Index))
                EventList[simulationTime].Add(iSimBase.Index);
        }
        /// <summary>
        /// Gets the next event in the event queue. Removes all events that are illegally queued in the past.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public ISimBase GetNextEvent(long time, SimulationObjects simulationObjects)
        {
            RemoveInvalidEvents(time);
            if (!EventList.Any(x => x.Key == time))
                return null;

            long MinKey = EventList.Keys.Min();

            ISimBase returnObject = simulationObjects.SimulationList[EventList[MinKey][0]];
            EventList[MinKey].RemoveAt(0);
            if (EventList[MinKey].Count == 0)
                EventList.Remove(MinKey);
            return returnObject;
        }

        private void RemoveInvalidEvents(long time, bool removeSameTime = false)
        {
            List<long> keys = new List<long>();
            foreach (long l in EventList.Keys.Where(x => x <= time))
            {
                keys.Add(l);
            }
            foreach (long l in keys)
            {
                if (l < time || removeSameTime)
                    EventList.Remove(l);
            }
        }

        public List<ISimBase> GetAllNextEvents(long time, SimulationObjects simulationObjects, out long EventTime)
        {
            RemoveInvalidEvents(time, true);
            EventTime = -1;
            if (EventList.Count == 0)
                return null;
            List<ISimBase> result = new List<ISimBase>();
            long minKey = EventList.Keys.Min();
            foreach (long key in EventList[minKey])
                result.Add(simulationObjects.SimulationList[key]);

            EventTime = minKey;
            return result;
        }

        /// <summary>
        /// Resets the event list
        /// </summary>
        public void Reset() => EventList.Clear();
        public EventQueue Clone()
        {
            EventQueue result = new EventQueue();
            foreach (KeyValuePair<long, List<long>> item in EventList)
            {
                result.EventList.Add(item.Key, new List<long>());
                foreach (long value in item.Value)
                    result.EventList[item.Key].Add(value);
            }
            return result;
        }
    }
}
