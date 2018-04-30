using System;
using System.Threading;
using Helpers.DataStructure;

namespace isxaker
{

    /// <summary>
    ///     Simple fake of Thread Pool class
    /// </summary>
    public sealed class SimpleThreadPool : IDisposable
    {
        #region constants

        private const int DefaultMaxThreadsPerCore = 2;
        private const int MaxCallStackDeep = 1000;

        #endregion

        #region  fields

        private readonly AutoResetEvent _isRunnuing;
        private readonly BlockingQueue<Action> _tasks;
        private readonly BlockingQueue<Thread> _workers;
        private bool _disallowAdd;
        private bool _disposed;

        #endregion

        #region properties

        /// <summary>
        ///     return maximum threads count
        /// </summary>
        private int MaxThreads { get { return SimpleThreadPool.DefaultMaxThreadsPerCore*Environment.ProcessorCount; } }

        #endregion

        #region  constructors

        public SimpleThreadPool()
        {
            this._disposed = false;
            this._disallowAdd = false;
            this._isRunnuing = new AutoResetEvent(false);

            this._workers = new BlockingQueue<Thread>(this.MaxThreads);
            this._tasks = new BlockingQueue<Action>(SimpleThreadPool.MaxCallStackDeep);

            for (var i = 0; i < this.MaxThreads; ++i)
            {
                this.CreateAndStartWorker();
            }
        }

        #endregion

        #region IDizposable

        public void Dispose()
        {
            var waitForThreads = false;
            if (!this._disposed)
            {
                GC.SuppressFinalize(this);

                this._disallowAdd = true;
                while (this._tasks.Count > 0)
                {
                    this._isRunnuing.WaitOne();
                }
                this._tasks.Close();
                this._disposed = true;
                waitForThreads = true;
            }

            if (waitForThreads)
            {
                for (int i = 0; i < this._workers.Count; i++)
                {
                    Thread worker = this._workers.Dequeue();
                    worker.Join();
                }
                this._workers.Close();
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///     EnQueue task for execution
        /// </summary>
        /// <param name="task">Action for executing</param>
        public void QueueTask(Action task)
        {
            if (this._disallowAdd)
            {
                throw new InvalidOperationException("This Pool instance is in the process of being disposed, can't add anymore");
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException("This Pool instance has already been disposed");
            }

            this._tasks.Enqueue(task);
        }

        #endregion

        #region hepler mathods

        private void Worker()
        {
            while (true)
            {
                Action task;
                if (!this._tasks.TryDequeue(out task))
                {
                    return;
                }

                if (this._disposed)
                {
                    return;
                }

                Thread curWorker;
                if (!this._workers.TryDequeue(out curWorker))
                {
                    return;
                }
                task();

                this._isRunnuing.Set();
                this._workers.Enqueue(Thread.CurrentThread);
            }
        }

        private void CreateAndStartWorker()
        {
            Thread worker = new Thread(this.Worker);
            worker.Start();
            this._workers.Enqueue(worker);
        }

        #endregion
    }

}