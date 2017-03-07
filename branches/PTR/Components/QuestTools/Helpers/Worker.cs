using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QuestTools;
using Zeta.Bot;

namespace Arcanum
{
    /// <summary>
    /// A worker thread. Use Start() method with code to be executed.
    /// </summary>
    public class Worker
    {
        public Worker()
        {
            //BotMain.OnStop += bot => Stop();
            BotMain.OnStart += bot => Stop();            
        }

        private Thread _thread;
        private WorkerDelegate _worker;
        private bool _working;
        public delegate bool WorkerDelegate();
        public int WaitTime;

        public delegate void WorkerEvent();
        public event WorkerEvent OnStopped = () => { };
        public event WorkerEvent OnStarted = () => { };

        public bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

        /// <summary>
        /// Run code in a new worker thread. WorkerDelegate should return true to end, false to repeat.
        /// </summary>
        /// <param name="worker">Delegate to be run</param>
        public void Start(WorkerDelegate worker)
        {
            if (IsRunning)
                return;

            var frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var ns = type != null ? type.Namespace : string.Empty;

            _worker = worker;
            _thread = new Thread(SafeWorkerDelegate)
            {
                Name = string.Format("Worker: {0}.{1}", ns, type),
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };

            Logger.Debug("Starting {0} Thread Id={1}", _thread.Name, _thread.ManagedThreadId);

            _working = true;
            _thread.Start();

            OnStarted.Invoke();
        }

        public void Stop()
        {
            try
            {               
                if (!IsRunning)
                    return;

                Logger.Debug("Shutting down thread");
               
                _thread.Abort(new { RequestingThreadId = Thread.CurrentThread.ManagedThreadId});
                //_thread.Join();
            }
            catch (Exception)
            {
                _working = false;
            }
        }

        public void SafeWorkerDelegate()
        {
            if (_thread == null)
                return;            

            Logger.Debug("Thread {0}: {1} Started", _thread.ManagedThreadId, _thread.Name);

            while (_working)
            {
                try
                {
                    Thread.Sleep(Math.Max(50, WaitTime));

                    if (_worker == null)
                        continue;

                    if (_worker.Invoke())
                        _working = false;
                }
                catch (ThreadAbortException ex)
                {
                    _working = false;
                    Logger.Debug("Aborting Thread: {0}, StateInfo={1}", _thread.ManagedThreadId, ex.ExceptionState);
                    Thread.ResetAbort();                    
                }
                catch (Exception ex)
                {
                    Logger.Log("Error in Thread {0}: {1} {2}", _thread.ManagedThreadId, _thread.Name, ex);
                }
            }

            Logger.Debug("Thread {0}: {1} Finished", _thread.ManagedThreadId, _thread.Name);

            OnStopped.Invoke();
        }

    }
}
