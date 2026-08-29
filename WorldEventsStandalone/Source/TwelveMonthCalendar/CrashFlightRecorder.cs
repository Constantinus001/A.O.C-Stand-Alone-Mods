using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using TaleWorlds.CampaignSystem;

namespace TwelveMonthCalendar
{

internal static class CrashFlightRecorder
{
	private const int Capacity = 256;

	private static readonly ConcurrentQueue<string> Events = new ConcurrentQueue<string>();

	private static int _eventCount;

	private static int _flushInProgress;

	internal static void Record(string category, string detail)
	{
		try
		{
			string entry = string.Format("{0:O} [{1}] {2}: {3}", DateTime.UtcNow, Thread.CurrentThread.ManagedThreadId, category ?? "Event", detail ?? string.Empty);
			Events.Enqueue(entry);
			Interlocked.Increment(ref _eventCount);
			string result;
			while (Volatile.Read(ref _eventCount) > 256 && Events.TryDequeue(out result))
			{
				Interlocked.Decrement(ref _eventCount);
			}
		}
		catch
		{
		}
	}

	internal static void RecordCampaignCheckpoint(string trigger)
	{
		try
		{
			CampaignTime now = CampaignTime.Now;
			Record("Campaign", $"{trigger}; NowDays={now.ToDays:F3}; Year={now.GetYear}; DayOfYear={now.GetDayOfYear}; Season={now.GetSeasonOfYear}; DayOfSeason={now.GetDayOfSeason}");
		}
		catch (Exception exception)
		{
			Record("Campaign", trigger + " checkpoint failed: " + exception.GetType().FullName);
		}
	}

	internal static void Flush(string reason, Exception exception)
	{
		if (Interlocked.CompareExchange(ref _flushInProgress, 1, 0) != 0)
		{
			return;
		}
		try
		{
			StringBuilder report = new StringBuilder();
			report.AppendLine("Ages of Calradia crash flight recorder");
			report.AppendLine("CapturedUtc=" + DateTime.UtcNow.ToString("O"));
			report.AppendLine("Reason=" + (reason ?? "Unknown"));
			if (exception != null)
			{
				report.AppendLine("Exception:");
				report.AppendLine(exception.ToString());
			}
			report.AppendLine("Recent events (oldest to newest):");
			foreach (string entry in Events)
			{
				report.AppendLine(entry);
			}
			Diagnostics.WriteCrashSnapshot(report.ToString());
		}
		catch
		{
		}
		finally
		{
			Volatile.Write(ref _flushInProgress, 0);
		}
	}
}
}
