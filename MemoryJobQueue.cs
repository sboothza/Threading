using System.Collections.Generic;

namespace Threading
{
	/// <summary>
	/// Simple job queue implemented with a generic in-memory queue
	/// </summary>
	public class MemoryJobQueue : IJobQueue
	{
		/// <summary>
		/// jobqueue member
		/// </summary>
		protected Queue<IPoolableJob> _jobQueue;
		/// <summary>
		/// lockobject member
		/// </summary>
		protected object _lockObject = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryJobQueue"/> class.
		/// </summary>
		public MemoryJobQueue()
		{
			_jobQueue = new Queue<IPoolableJob>();
		}

		/// <summary>
		/// Des the queue.
		/// </summary>
		/// <returns></returns>
		public IPoolableJob GetJob(object state)
		{
			lock (_lockObject)
			{
				if (_jobQueue.Count > 0)
					return _jobQueue.Dequeue();
				return null;
			}
		}

		/// <summary>
		/// Ens the queue.
		/// </summary>
		/// <param name="job">The job.</param>
		public void QueueJob(IPoolableJob job)
		{
			lock (_lockObject)
			{
				_jobQueue.Enqueue(job);
			}
		}

		/// <summary>
		/// Flags the job as complete in the queue.
		/// Only really applicable for transactional queues
		/// </summary>
		/// <param name="job">The job.</param>
		/// <param name="state"></param>
		public void Complete(IPoolableJob job, object state)
		{
			//do nothing
		}

		/// <summary>
		/// Flags the job as failed in the queue.
		/// Only really applicable for transactional queues
		/// </summary>
		/// <param name="job">The job.</param>
		/// <param name="state"></param>
		public void Fail(IPoolableJob job, object state)
		{
			//do nothing
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				lock (_lockObject)
				{
					return _jobQueue.Count;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether [jobs waiting].
		/// </summary>
		/// <value><c>true</c> if [jobs waiting]; otherwise, <c>false</c>.</value>
		public bool JobsWaiting
		{
			get
			{
				lock (_lockObject)
				{
					return _jobQueue.Count > 0;
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{

		}
	}
}
