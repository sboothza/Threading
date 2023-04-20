using System;

namespace Threading
{
	public enum JobCompletionStatus : byte
	{
		Success = 0,
		FailNoRetry = 1,
		FailRetry = 2
	}
	/// <summary>
	/// Status of the worker
	/// </summary>
	public enum WorkerStatus : byte
	{
		/// <summary>
		/// Worker waiting
		/// </summary>
		Waiting,
		/// <summary>
		/// Worker started
		/// </summary>
		Started,
		/// <summary>
		/// Worker complete
		/// </summary>
		Complete,
		/// <summary>
		/// Worker failed
		/// </summary>
		Failed
	}

	/// <summary>
	/// Worker event arguments
	/// </summary>
	public class WorkerEventArgs : EventArgs
	{
		/// <summary>
		/// Actual job
		/// </summary>
		public IPoolableJob Job;
		/// <summary>
		/// Status of worker
		/// </summary>
		public WorkerStatus Status;

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerEventArgs"/> class.
		/// </summary>
		/// <param name="job">The job.</param>
		/// <param name="status">The status.</param>
		public WorkerEventArgs(IPoolableJob job, WorkerStatus status)
		{
			Job = job;
			Status = status;
		}
	}

	/// <summary>
	/// Delegate for event that is thrown by worker
	/// </summary>
	public delegate void WorkerEvent(object sender, WorkerEventArgs args);
}
