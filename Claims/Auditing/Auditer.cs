using System.Threading.Channels;

namespace Claims.Auditing
{
	/// <summary>
	/// Records audit entries without blocking the caller.
	/// Entries are queued in memory and saved to the database by <see cref="AuditBackgroundService"/>.
	/// </summary>
	public class Auditer
	{
		private readonly Channel<object> _queue = Channel.CreateUnbounded<object>(
			new UnboundedChannelOptions { SingleReader = true });

		/// <summary>
		/// Queued audit entries (<see cref="ClaimAudit"/> or <see cref="CoverAudit"/>) waiting to be saved.
		/// </summary>
		public ChannelReader<object> Reader => _queue.Reader;

		/// <summary>
		/// Queues an audit entry for an operation on a claim.
		/// </summary>
		public void AuditClaim(string id, string httpRequestType)
		{
			_queue.Writer.TryWrite(new ClaimAudit
			{
				Created = DateTime.Now,
				HttpRequestType = httpRequestType,
				ClaimId = id
			});
		}

		/// <summary>
		/// Queues an audit entry for an operation on a cover.
		/// </summary>
		public void AuditCover(string id, string httpRequestType)
		{
			_queue.Writer.TryWrite(new CoverAudit
			{
				Created = DateTime.Now,
				HttpRequestType = httpRequestType,
				CoverId = id
			});
		}
	}
}
