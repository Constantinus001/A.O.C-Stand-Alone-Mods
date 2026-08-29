using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.CharacterCreationContent;

namespace TwelveMonthCalendar
{

internal static class CharacterOriginStoryCapture
{
	private sealed class OriginChoice
	{
		internal readonly string MenuTitle;

		internal readonly string MenuDescription;

		internal readonly string OptionText;

		internal readonly string OptionDescription;

		internal OriginChoice(string menuTitle, string menuDescription, string optionText, string optionDescription)
		{
			MenuTitle = menuTitle;
			MenuDescription = menuDescription;
			OptionText = optionText;
			OptionDescription = optionDescription;
		}
	}

	private static readonly Dictionary<string, OriginChoice> ChoicesByMenuId = new Dictionary<string, OriginChoice>(StringComparer.Ordinal);

	private static readonly List<string> MenuOrder = new List<string>();

	private static CharacterCreationManager _activeManager;

	internal static void Record(CharacterCreationManager manager, NarrativeMenuOption option)
	{
		if (manager == null || manager.CurrentMenu == null || option == null)
		{
			return;
		}
		if (_activeManager != manager)
		{
			Clear();
			_activeManager = manager;
		}
		string text = manager.CurrentMenu.StringId ?? string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			if (!ChoicesByMenuId.ContainsKey(text))
			{
				MenuOrder.Add(text);
			}
			ChoicesByMenuId[text] = new OriginChoice(ToPlainText(manager.CurrentMenu.Title), ToPlainText(manager.CurrentMenu.Description), ToPlainText(option.Text), ToPlainText(option.DescriptionText));
		}
	}

	internal static string ConsumeSnapshot()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string item in MenuOrder)
			{
				if (ChoicesByMenuId.TryGetValue(item, out var value))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine().AppendLine();
					}
					if (!string.IsNullOrEmpty(value.MenuTitle))
					{
						stringBuilder.Append(value.MenuTitle).AppendLine();
					}
					if (!string.IsNullOrEmpty(value.OptionText))
					{
						stringBuilder.Append("• ").Append(value.OptionText).AppendLine();
					}
					if (!string.IsNullOrEmpty(value.OptionDescription))
					{
						stringBuilder.Append(value.OptionDescription);
					}
					else if (!string.IsNullOrEmpty(value.MenuDescription))
					{
						stringBuilder.Append(value.MenuDescription);
					}
				}
			}
			return stringBuilder.ToString().Trim();
		}
		finally
		{
			Clear();
		}
	}

	private static void Clear()
	{
		ChoicesByMenuId.Clear();
		MenuOrder.Clear();
		_activeManager = null;
	}

	private static string ToPlainText(object text)
	{
		if (text != null)
		{
			return text.ToString().Replace('\t', ' ').Trim();
		}
		return string.Empty;
	}
}
}
