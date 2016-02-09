using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using ProgramMain.Framework.WorkerThread.Types;

namespace ProgramMain.Framework.WorkerThread
{
    public class WorkerMessageThread
    {
        private readonly AutoResetEvent _autoEvent = new AutoResetEvent(false);

        private readonly Control _delegateControl;

        public WorkerMessageThread(Control delegateCOntrol)
        {
            //для делегейта в родительский поток
            _delegateControl = delegateCOntrol;

            CreateWorkerThread();
        }

        ~WorkerMessageThread()
        {
            Terminating = true;
        }

        private readonly ManualResetEvent _terminateEvent = new ManualResetEvent(false);
        private bool _terminating;
        public bool Terminating
        {
            set
            {
                if (_terminating != value && value)
                {
                    _terminating = true;
                    Terminate();
                }
            }
            get
            {
                return _terminating;
            }
        }

        private const int WorkerEventSize = 1000;
        private readonly List<WorkerEvent> _workerEventList = new List<WorkerEvent>();
        private readonly SemaphoreSlim _lockWe = new SemaphoreSlim(1, 1);

        public int WorkerQueueCount
        {
            get { return _workerEventList.Count; }
        }

        public class OwnerEventArgs
        {
            // Summary:
            //     Represents an event with no event data.
            public static readonly OwnerEventArgs Empty = new OwnerEventArgs();
        }

        public delegate void OwnerEventHandler<in T>(object sender, T e) where T : OwnerEventArgs;

        protected delegate void OwnerEventDelegate<in T>(T eventParams) where T : OwnerEventArgs;

        protected void FireOwnerEvent<T>(OwnerEventDelegate<T> ownerEvent, T eventParams) where T : OwnerEventArgs
        {
            if (Terminating) return;
            //синхронный вызов из рабочего потока в поток приложения
            _delegateControl.Invoke(ownerEvent, new Object[] { eventParams });
        }

        private void Terminate()
        {
            while (!_terminateEvent.WaitOne(100))
            {
                WakeupWorkerThread();
                Application.DoEvents();
            }
        }
        
        virtual protected void OnControlClosing()
        {
        }

        protected virtual bool DispatchThreadEvents(WorkerEvent workerEvent)
        {
            return false;
        }

        protected void PutWorkerThreadEvent(WorkerEvent workerEvent)
        {
            try
            {
                _lockWe.Wait();
                
                if (!workerEvent.IsCollapsible
                    || _workerEventList.Find(we => we.CompareTo(workerEvent) == 0) == null)
                {
                    if (_workerEventList.Count > WorkerEventSize)
                    {
                        _workerEventList.RemoveAt(0);
                    }
                    _workerEventList.Add(workerEvent);
                }
            }
            finally
            {
                _lockWe.Release();
            }
            WakeupWorkerThread();
        }

        protected void PutWorkerThreadEvent(WorkerEventType workerEventType, bool isCollapsible, EventPriorityType priorityType)
        {
            PutWorkerThreadEvent(new WorkerEvent(workerEventType, isCollapsible, priorityType));
        }

        protected void PutWorkerThreadEvent(WorkerEventType workerEventType)
        {
            PutWorkerThreadEvent(new WorkerEvent(workerEventType, false, EventPriorityType.Normal));
        }

        protected WorkerEvent PopupWorkerThreadEvent()
        {
            return PopupWorkerThreadEvent(WorkerEventType.None);
        }

        protected void DropWorkerThreadEvents(WorkerEventType workerEventType)
        {
            try
            {
                _lockWe.Wait();

                if (workerEventType != WorkerEventType.None)
                {
                    _workerEventList.RemoveAll(we => we.EventType == workerEventType);
                }
            }
            finally
            {
                _lockWe.Release();
            }
        }

        private void CreateWorkerThread()
        {
            var threadDeligate = new ThreadStart(WorkerThread);
            var thread = new Thread(threadDeligate) {Priority = ThreadPriority.BelowNormal};

            thread.Start();
        }

        private void WorkerThread()
        {
            do
            {
                _autoEvent.WaitOne();
                if (Terminating)
                    break;
                DoThreadWork();
            } while (true);
            
            OnControlClosing();
            
            _terminateEvent.Set();
        }

        private void DoThreadWork()
        {
            WorkerEvent workerEvent;
            do
            {
                workerEvent = PopupWorkerThreadEvent();
                try
                {
                    if (workerEvent != null)
                    {
                        DispatchThreadEvents(workerEvent);
                    }
                }
                catch (Exception ex)
                {
                    //do nothing
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            } while (workerEvent != WorkerEvent.Empty && !Terminating);
        }

        private void WakeupWorkerThread()
        {
            _autoEvent.Set();
        }

        private WorkerEvent PopupWorkerThreadEvent(WorkerEventType workerEventType)
        {
            //выбираем задания из очереди, RedrawLayer имеет низший приоритет
            var res = WorkerEvent.Empty;
            try
            {
                _lockWe.Wait();

                //если не задан тип задание, то ищем любой в соответствии с приоритетом)
                for (var i = EventPriorityTypeConverter.Length - 1; i >= 0; i--)
                {
                    var item = i.ToEventPriorityType();

                    var tmp = _workerEventList.Find(we =>
                        we.EventPriority == item
                        && (workerEventType == WorkerEventType.None || workerEventType == we.EventType));
                    if (tmp != null)
                    {
                        res = tmp;
                        break;
                    }
                }

                if (res != WorkerEvent.Empty)
                {
                    //выбираем все/или одно задание данного типа(зависит от типа задания)
                    if (res.IsCollapsible)
                    {
                        _workerEventList.RemoveAll(we => we.CompareTo(res) == 0);
                    }
                    else
                    {
                        _workerEventList.Remove(res);
                    }
                }
            }
            finally
            {
                _lockWe.Release();
            }
            return res;
        }
    }
}
