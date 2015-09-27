namespace Std.Ui.Logging
{
	public interface ILogEntryFormatter
	{
		void FormatEntry(LogEntry entry, string line);
	}
}