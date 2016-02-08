using System;

namespace ProgramMain.Framework
{
    public static class WorkerEventPriorityTypeConverter
    {
        public static Array EventPriorityValues = null;
        public static Array EventPriorityEnum
        {
            get 
            {
                return EventPriorityValues ?? (EventPriorityValues = Enum.GetValues(typeof (WorkerMessageThread.EventPriorityType)));
            }
        }
        
        public static int Length
        {
            get
            {
                return EventPriorityEnum.Length;
            }
        }

        public static WorkerMessageThread.EventPriorityType ToEventPriorityType(this Int32 i)
        {
            if (i >= 0 && i < EventPriorityEnum.Length)
                return (WorkerMessageThread.EventPriorityType)Convert.ChangeType(EventPriorityEnum.GetValue(i), typeof(WorkerMessageThread.EventPriorityType));
            
            return WorkerMessageThread.EventPriorityType.Idle;
        }
    }
}