using System;

namespace Threading
{
	/// <summary>
	/// Base Job class - barely wraps the IPoolableJob interface
	/// </summary>
	[Serializable]
	public abstract class Job : IPoolableJob
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Job"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		protected Job(string name)
		{
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Job"/> class.
		/// </summary>
		protected Job()
		{
		}

		public object State { get; set; }

		/// <summary>
		/// The name of the job (used for debug output)
		/// </summary>
		/// <value></value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates that the entire context has completed
		/// </summary>
		/// <value></value>
		public bool IsComplete
		{
			get;
			protected set;
		}

		/// <summary>
		/// The thread's work, i.e. its entry point method in the class that implements the job
		/// </summary>
		public virtual void Work()
		{
			try
			{
				FireOnStarted();
				ResultCode = Execute();
				IsComplete = true;
				FireOnComplete();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception: {0}", ex);
				IsComplete = true;
				ResultCode = 0;
				FireOnFailed();
				throw;
			}
		}

		/// <summary>
		/// Override this method to implement the work.
		/// Throw any errors directly out - they will be caught and handled by the thread management.
		/// However, the task will probably be re-run, so ensure that any errors are not due to some 
		/// programming or configuration error - in this case return a failure error code.
		/// </summary>
		/// <returns></returns>
		protected abstract JobCompletionStatus Execute();

		/// <summary>
		/// Result of the operation
		/// </summary>
		/// <value></value>
		public JobCompletionStatus ResultCode
		{
			get;
			protected set;
		}

		/// <summary>
		/// Occurs when [on started].
		/// </summary>
		public event WorkerEvent OnStarted;
		/// <summary>
		/// Occurs when [on complete].
		/// </summary>
		public event WorkerEvent OnComplete;
		/// <summary>
		/// Occurs when [on failed].
		/// </summary>
		public event WorkerEvent OnFailed;

		/// <summary>
		/// Fires the on started.
		/// </summary>
		public void FireOnStarted()
		{
			OnStarted?.Invoke(this, new WorkerEventArgs(this, WorkerStatus.Started));
		}

		/// <summary>
		/// Fires the on complete.
		/// </summary>
		public void FireOnComplete()
		{
			OnComplete?.Invoke(this, new WorkerEventArgs(this, WorkerStatus.Complete));
		}

		/// <summary>
		/// Fires the on failed.
		/// </summary>
		public void FireOnFailed()
		{
			OnFailed?.Invoke(this, new WorkerEventArgs(this, WorkerStatus.Failed));
		}
	}
}
