using System;

namespace Threading
{
	/// <summary>
	/// Generic job interface - all jobs will implement it
	/// </summary>
	public interface IPoolableJob
	{
		object State { get; set; }
		/// <summary>
		/// The name of the job (used for debug output)
		/// </summary>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates that the entire context has completed
		/// </summary>
		bool IsComplete
		{
			get;
		}

		/// <summary>
		/// The thread's work, i.e. its entry point method in the class that implements the job
		/// </summary>
		void Work();

		/// <summary>
		/// Result of the operation
		/// </summary>
		JobCompletionStatus ResultCode
		{
			get;
		}

		/// <summary>
		/// Occurs when [on started].
		/// </summary>
		event WorkerEvent OnStarted;
		/// <summary>
		/// Occurs when [on complete].
		/// </summary>
		event WorkerEvent OnComplete;
		/// <summary>
		/// Occurs when [on failed].
		/// </summary>
		event WorkerEvent OnFailed;

		/// <summary>
		/// Fires the on started.
		/// </summary>
		void FireOnStarted();
		/// <summary>
		/// Fires the on complete.
		/// </summary>
		void FireOnComplete();
		/// <summary>
		/// Fires the on failed.
		/// </summary>
		void FireOnFailed();
	}

	/// <summary>
	/// Job Queue interface
	/// </summary>
	public interface IJobQueue : IDisposable
	{
		/// <summary>
		/// Gets the job.
		/// </summary>
		/// <returns></returns>
		IPoolableJob GetJob(object state);

		/// <summary>
		/// Flags the job as complete in the queue.
		/// Only really applicable for transactional queues
		/// </summary>
		/// <param name="job">The job.</param>
		void Complete(IPoolableJob job, object state);

		/// <summary>
		/// Flags the job as failed in the queue.
		/// Only really applicable for transactional queues
		/// </summary>
		/// <param name="job">The job.</param>
		void Fail(IPoolableJob job, object state);

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		int Count
		{
			get;
		}
		/// <summary>
		/// Gets a value indicating whether [jobs waiting].
		/// </summary>
		/// <value><c>true</c> if [jobs waiting]; otherwise, <c>false</c>.</value>
		bool JobsWaiting
		{
			get;
		}

		/// <summary>
		/// Adds a job to the work queue
		/// It will be picked up as soon as a worker is available
		/// There is no way to delete a job
		/// </summary>
		/// <param name="job">The job.</param>
		void QueueJob(IPoolableJob job);
	}

	/// <summary>
	/// Manages pool of workers (which manage threads)
	/// Each worker retrieves next job from queue, executes it, then returns.
	/// </summary>
	public interface IThreadPool : IDisposable
	{
		/// <summary>
		/// Gets or sets a value indicating whether debug information should be output
		/// </summary>
		/// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
		bool IsDebug
		{
			get;
			set;
		}
		/// <summary>
		/// Retrieves the number of running workers
		/// </summary>
		/// <value>The active workers.</value>
		int ActiveThreads
		{
			get;
		}
		/// <summary>
		/// Returns the maximum allowed number of workers
		/// </summary>
		/// <value>The max workers.</value>
		int MaxThreads
		{
			get;
		}

		/// <summary>
		/// retrieves the total workers in the list
		/// </summary>
		/// <value>The current workers.</value>
		int CurrentThreads
		{
			get;
		}

		/// <summary>
		/// number of jobs waiting in the queue
		/// </summary>
		/// <value>The waiting jobs.</value>
		int WaitingJobs
		{
			get;
		}

		/// <summary>
		/// Gets the job queue.
		/// </summary>
		/// <value>The job queue.</value>
		IJobQueue JobQueue
		{
			get;
		}

		bool Busy { get; }
	}
}
