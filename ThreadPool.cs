using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Threading
{
	/// <summary>
	/// Thread pool class
	/// Manages pool of workers (which manage threads)
	/// Each worker retrieves next job from queue, executes it, then returns.
	/// </summary>
	public class ThreadPool : IThreadPool
	{
		/// <summary>
		/// The maximum workers to use
		/// </summary>
		protected int _maxThreads;
		/// <summary>
		/// The list to store the workers
		/// </summary>
		internal List<PoolWorker> _workerList;

		/// <summary>
		/// The  object name (for debug)
		/// </summary>
		protected string _name;

		/// <summary>
		/// Gets or sets a value indicating whether debug information should be output
		/// </summary>
		public bool IsDebug
		{
			get => PoolWorker._isDebug;
			set => PoolWorker._isDebug = value;
		}

		/// <summary>
		/// Retrieves the number of running workers
		/// </summary>
		public int ActiveThreads
		{
			get
			{
				return _workerList.Count(worker => worker.State == PoolWorkerState.Running || worker.State == PoolWorkerState.Starting);
			}
		}

		/// <summary>
		/// Returns the maximum allowed number of workers
		/// </summary>
		public virtual int MaxThreads => _maxThreads;

		/// <summary>
		/// retrieves the total workers in the list
		/// </summary>
		public int CurrentThreads => _workerList.Count;

		/// <summary>
		/// number of jobs waiting in the queue
		/// </summary>
		public int WaitingJobs => JobQueue.Count;

		/// <summary>
		/// Gets the job queue.
		/// </summary>
		/// <value>The job queue.</value>
		public IJobQueue JobQueue
		{
			get;
			protected set;
		}

		public bool Busy => ActiveThreads > 0;

		/// <summary>
		/// Initializes a new thread pool
		/// </summary>
		/// <param name="queue">The queue.</param>
		/// <param name="workerThreads">The number of worker threads to create in the pool</param>
		/// <param name="name">The name to use for the name property of each thread in the pool (for debugging)</param>
		/// <param name="getState"></param>
		public ThreadPool(IJobQueue queue, int workerThreads, string name, Func<object> getState)
		{
			_maxThreads = workerThreads;
			_name = name;
			_workerList = new List<PoolWorker>(workerThreads);
			JobQueue = queue;

			//instantiate the workers
			for (var i = 0; i < _maxThreads; i++)
				_workerList.Add(new PoolWorker(this, $"{_name}_{i}", getState()));

		}

		/// <summary>
		/// Initializes a new thread pool
		/// </summary>
		/// <param name="queue">The queue.</param>
		/// <param name="workerThreads">The number of worker threads to create in the pool</param>
		/// <param name="name">The name to use for the name property of each thread in the pool (for debugging)</param>
		/// <param name="getState"></param>
		public ThreadPool(IJobQueue queue, int workerThreads, string name)
		{
			_maxThreads = workerThreads;
			_name = name;
			_workerList = new List<PoolWorker>(workerThreads);
			JobQueue = queue;

			//instantiate the workers
			for (var i = 0; i < _maxThreads; i++)
				_workerList.Add(new PoolWorker(this, $"{_name}_{i}", null));

		}

		/// <summary>
		/// Destroys the workers and their threads
		/// </summary>
		public void Dispose()
		{
			while (_workerList.Count > 0)
			{
				PoolWorker poolWorker = _workerList[0];
				poolWorker.Kill();
				var waitcount = 5;
				while ((poolWorker._thread.ThreadState == ThreadState.Running || poolWorker._thread.ThreadState == ThreadState.WaitSleepJoin) && (waitcount-- > 0))
					Thread.Sleep(100);

				if (poolWorker._thread.ThreadState == ThreadState.Running || poolWorker._thread.ThreadState == ThreadState.WaitSleepJoin)
					poolWorker._thread.Abort();

				poolWorker._thread.Join(1000);
				_workerList.RemoveAt(0);
			}
		}
	}
}
