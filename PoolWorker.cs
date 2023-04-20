using System;
using System.Threading;

namespace Threading
{
	/// <summary>
	/// PoolWorker states - N.B. for internal use only
	/// Controls the state machine for the thread workers
	/// Does *NOT* (in general) control the actual thread!!!!
	/// </summary>
	internal enum PoolWorkerState
	{
		/// <summary>
		/// The worker is instructed to stop processing as soon as the current job is done
		/// </summary>
		Stopping = 1,
		/// <summary>
		/// The worker is starting up again after being idle
		/// </summary>
		Starting = 2,
		/// <summary>
		/// The worker is idle
		/// </summary>
		Idle = 0,
		/// <summary>
		/// The worker is running a job
		/// </summary>
		Running = 3,
		/// <summary>
		/// The worker will terminate after the job is complete - this actually kills the thread
		/// </summary>
		Kill = 4
	}

	/// <summary>
	/// The thread worker class
	/// It manages the actual thread, as well as processing the tasks
	/// It automatically retrieves the next available task from the queue when the current task is completed
	/// </summary>
	internal class PoolWorker
	{
		/// <summary>
		/// Flag to indicate this worker must be terminated - ie. the pool size has been shrunk
		/// </summary>
		internal bool _mustKill;
		/// <summary>
		/// Indicates whether to output debug information
		/// </summary>
		internal static bool _isDebug;

		/// <summary>
		/// The actual thread object
		/// </summary>
		internal Thread _thread;
		/// <summary>
		/// The owning thread pool (owns the job queue)
		/// </summary>
		protected ThreadPool _parent;

		/// <summary>
		/// The currently executing job
		/// </summary>
		protected IPoolableJob _job;

		/// <summary>
		/// This property is used for retrieving the current state of the worker
		/// </summary>
		public PoolWorkerState State
		{
			get;
			set;
		}

		public DateTime IdleStamp
		{
			get;
			protected set;
		}

		private readonly object _internalState;

		internal PoolWorker(ThreadPool parent, string name, object internalState)
		{
			State = PoolWorkerState.Idle;
			_internalState = internalState;

			_thread = new Thread(Run)
			{
				Name = name
			}; //create the thread
			_parent = parent;
			_thread.Start(); //Start the thread
		}

		/// <summary>
		/// This method is used to start the worker when it's ready
		/// It can be called multiple times - it doesn't actually start the thread,
		/// it merely sets the status
		/// </summary>
		internal void Start()
		{
			State = PoolWorkerState.Starting;
		}

		/// <summary>
		/// Instructs the worker to stop once it's ready\
		/// It can be called multiple times - it doesn't actually stop the thread,
		/// it merely sets the status
		/// </summary>
		internal void Stop()
		{
			State = PoolWorkerState.Stopping;
		}

		/// <summary>
		/// Instructs the worker to terminate when it's ready
		/// </summary>
		internal void Kill()
		{
			_mustKill = true;
		}

		/// <summary>
		/// Retrieve the next job from the thread pool's queue
		/// </summary>
		protected internal bool GetNextJob()
		{
			try
			{
				IPoolableJob job = _parent.JobQueue.GetJob(_internalState);

				if (job == null)
					return false;

				if (_isDebug)
					Console.WriteLine($"{_thread.Name} picked up job {job.Name}");
				_job = job;
				Start();
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return false;
			}
		}

		/// <summary>
		/// The 'threadmain' - just a state machine controlling the thread
		/// This method is run by the actual thread.
		/// When this method terminates, the thread dies
		/// </summary>
		public void Run()
		{
			while (true)
			{
				try
				{
					if (_mustKill)
						return;

					switch (State)
					{
						case PoolWorkerState.Starting:
							if (_isDebug)
								Console.WriteLine($"{_thread.Name} starting");
							State = PoolWorkerState.Running;
							break;

						case PoolWorkerState.Stopping:
							if (_isDebug)
								Console.WriteLine($"{_thread.Name} stopping");
							State = PoolWorkerState.Idle;
							break;

						case PoolWorkerState.Idle:
							if (!GetNextJob())
							{
								if (IdleStamp.CompareTo(DateTime.MinValue) == 0)
									IdleStamp = DateTime.Now;
								Thread.Sleep(10);
							}
							break;

						case PoolWorkerState.Running:
							IdleStamp = DateTime.MinValue;
							if (_isDebug && _job != null)
								Console.WriteLine($"{_thread.Name} running job {_job.Name}");
							if (_job != null)
							{
								try
								{
									_job.Work();
									if (_job.ResultCode == JobCompletionStatus.Success)
										_parent.JobQueue.Complete(_job, _internalState);
									else
										_parent.JobQueue.Fail(_job, _internalState);
								}
								catch
								{
									_parent.JobQueue.Fail(_job, _internalState);
								}
							}
							if (_isDebug && _job != null)
								Console.WriteLine($"{_thread.Name} done job {_job.Name}");
							State = PoolWorkerState.Idle;
							break;

						case PoolWorkerState.Kill:
							if (_isDebug)
								Console.WriteLine($"{_thread.Name} killed");
							return;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}
	}
}
