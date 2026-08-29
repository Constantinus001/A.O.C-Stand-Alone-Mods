using System.Collections.Generic;

namespace TwelveMonthCalendar
{

internal sealed class CalendarWarStatisticsSnapshot
{
	internal int WarDeclarations { get; set; }

	internal int PeaceAgreements { get; set; }

	internal int TotalTroopsFielded { get; set; }

	internal int TotalLosses { get; set; }

	internal string PlayerStatus { get; set; }

	internal List<CalendarWarStatisticsRecord> Wars { get; private set; }

	internal CalendarWarStatisticsSnapshot()
	{
		Wars = new List<CalendarWarStatisticsRecord>();
		PlayerStatus = "Your faction is currently at peace.";
	}
}
}
