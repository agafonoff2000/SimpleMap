using System;
using ProgramMain.Framework.WorkerThread.Types;

namespace ProgramMain.Framework.WorkerThread
{
    public class WorkerEvent : IComparable
    {
        // Summary:
        //     Represents an event with no event data.
        public static readonly WorkerEvent Empty = new WorkerEvent();

        private readonly WorkerEventType _eventType = WorkerEventType.None;
        public WorkerEventType EventType
        {
            get
            {
                return _eventType;
            }
        }

        private readonly EventPriorityType _eventPriority = EventPriorityType.Normal;
        public EventPriorityType EventPriority
        {
            get
            {
                return _eventPriority;
            }
        }

        private readonly bool _collapsible;
        public bool IsCollapsible
        {
            get
            {
                return _collapsible;
            }
        }

        private WorkerEvent()
        {
            _collapsible = false;
        }

        public WorkerEvent(WorkerEventType pEventType)
        {
            _eventType = pEventType;
            _collapsible = false;
            _eventPriority = EventPriorityType.Normal;
        }

        public WorkerEvent(WorkerEventType pEventType, bool pIsCollapsible)
        {
            _eventType = pEventType;
            _collapsible = pIsCollapsible;
            _eventPriority = EventPriorityType.Normal;
        }

        public WorkerEvent(WorkerEventType pEventType, bool pIsCollapsible, EventPriorityType pPriorityType)
        {
            _eventType = pEventType;
            _collapsible = pIsCollapsible;
            _eventPriority = pPriorityType;
        }

        virtual public int CompareTo(object obj)
        {
            if (obj != null)
            {
                if (((WorkerEvent)obj)._eventType > EventType)
                    return 1;
                if (((WorkerEvent)obj)._eventType < EventType)
                    return -1;
                    
                return 0;
            }
            return -1;
        }
    }
}