using System;
using System.IO;
using System.Linq;
using System.Text;

namespace TwelveMonthCalendar
{

internal static class Diagnostics
{
	private static readonly object SyncRoot = new object();

	private const long MaximumLogBytes = 5242880L;

	private const int MaximumCrashReports = 20;

	private static string _logPath;

	internal static string LogPath => _logPath;

	internal static void Initialize()
	{
		try
		{
			string assemblyDirectory = Path.GetDirectoryName(typeof(Diagnostics).Assembly.Location);
			string moduleDirectory = assemblyDirectory;
			if (!string.IsNullOrWhiteSpace(assemblyDirectory))
			{
				DirectoryInfo candidateModuleDirectory = Directory.GetParent(assemblyDirectory)?.Parent;
				if (candidateModuleDirectory != null)
				{
					moduleDirectory = candidateModuleDirectory.FullName;
				}
			}
			if (!TrySetLogPath(Path.Combine(moduleDirectory, "Logs")))
			{
				if (!TrySetLogPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord", "Configs", "ModLogs")))
				{
					return;
				}
				Write("INFO  Module log directory was not writable; using the Documents fallback.");
			}
			Write("=== Ages of Calradia diagnostics started " + DateTime.Now.ToString("O") + " ===");
		}
		catch
		{
		}
	}

	private static bool TrySetLogPath(string directory)
	{
		if (string.IsNullOrWhiteSpace(directory))
		{
			return false;
		}
		try
		{
			Directory.CreateDirectory(directory);
			string text = Path.Combine(directory, "AgesOfCalradia.log");
			RotateLogIfNeeded(text);
			File.AppendAllText(text, string.Empty, Encoding.UTF8);
			_logPath = text;
			PruneCrashReports(directory);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static void Info(string message)
	{
		Write("INFO  " + message);
	}

	internal static void Error(string message, Exception exception)
	{
		Write("ERROR " + message + Environment.NewLine + exception);
	}

	internal static void WriteCrashSnapshot(string contents)
	{
		if (string.IsNullOrWhiteSpace(_logPath) || string.IsNullOrWhiteSpace(contents))
		{
			return;
		}
		try
		{
			string text = Path.Combine(Path.GetDirectoryName(_logPath), "CrashReports");
			Directory.CreateDirectory(text);
			File.WriteAllText(Path.Combine(text, "AgesOfCalradia-crash-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".log"), contents, Encoding.UTF8);
			PruneCrashReports(Path.GetDirectoryName(_logPath));
		}
		catch
		{
		}
	}

	private static void Write(string message)
	{
		if (string.IsNullOrWhiteSpace(_logPath))
		{
			return;
		}
		try
		{
			lock (SyncRoot)
			{
				File.AppendAllText(_logPath, DateTime.Now.ToString("O") + " " + message + Environment.NewLine, Encoding.UTF8);
			}
		}
		catch
		{
		}
	}

	private static void RotateLogIfNeeded(string path)
	{
		try
		{
			FileInfo file = new FileInfo(path);
			if (file.Exists && file.Length >= 5242880)
			{
				string previous = path + ".previous";
				if (File.Exists(previous))
				{
					File.Delete(previous);
				}
				File.Move(path, previous);
			}
		}
		catch
		{
		}
	}

	private static void PruneCrashReports(string logDirectory)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(logDirectory))
			{
				return;
			}
			string crashDirectory = Path.Combine(logDirectory, "CrashReports");
			if (Directory.Exists(crashDirectory))
			{
				FileInfo[] array = (from file in new DirectoryInfo(crashDirectory).GetFiles("AgesOfCalradia-crash-*.log")
					orderby file.CreationTimeUtc descending
					select file).Skip(20).ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Delete();
				}
			}
		}
		catch
		{
		}
	}
}
}
