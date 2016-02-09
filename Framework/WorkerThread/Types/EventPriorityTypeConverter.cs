using System;

namespace ProgramMain.Framework.WorkerThread.Types
{
    public static class EventPriorityTypeConverter
    {
        public static Array EventPriorityValues = null;
        public static Array EventPriorityEnum
        {
            get 
            {
                return EventPriorityValues ?? (EventPriorityValues = Enum.GetValues(typeof (EventPriorityType)));
            }
        }
        
        public static int Length
        {
            get
            {
                return EventPriorityEnum.Length;
            }
        }

        public static EventPriorityType ToEventPriorityType(this Int32 i)
        {
            if (i >= 0 && i < EventPriorityEnum.Length)
                return (EventPriorityType)Convert.ChangeType(EventPriorityEnum.GetValue(i), typeof(EventPriorityType));
            
            return EventPriorityType.Idle;
        }
    }
}