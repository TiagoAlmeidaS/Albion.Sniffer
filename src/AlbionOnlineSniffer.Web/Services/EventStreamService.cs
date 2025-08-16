using System.Collections.Concurrent;
using AlbionOnlineSniffer.Capture.Models;

namespace AlbionOnlineSniffer.Web.Services
{
	public class EventStreamService
	{
		private readonly ConcurrentQueue<object> _recentEvents = new ConcurrentQueue<object>();
		private readonly ConcurrentQueue<object> _recentPackets = new ConcurrentQueue<object>();
		private const int MaxEvents = 200;
		private const int MaxPackets = 100;
		private PacketCaptureMetrics? _lastMetrics;

		public void AddEvent(object message)
		{
			_recentEvents.Enqueue(message);
			TrimQueue(_recentEvents, MaxEvents);
		}

		public void AddRawPacket(byte[] payload)
		{
			_recentPackets.Enqueue(new { Length = payload?.Length ?? 0, Hex = payload != null ? Convert.ToHexString(payload) : "" });
			TrimQueue(_recentPackets, MaxPackets);
		}

		public void UpdateMetrics(PacketCaptureMetrics metrics)
		{
			_lastMetrics = metrics;
		}

		public object[] GetRecentEvents() => _recentEvents.ToArray();
		public object[] GetRecentPackets() => _recentPackets.ToArray();
		public PacketCaptureMetrics? GetLastMetrics() => _lastMetrics;

		private static void TrimQueue(ConcurrentQueue<object> queue, int max)
		{
			while (queue.Count > max && queue.TryDequeue(out _)) { }
		}
	}
}