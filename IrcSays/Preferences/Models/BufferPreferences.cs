using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace IrcSays.Preferences.Models
{
	public class BufferPreferences : PreferenceBase
	{
		private bool _isLoggingEnabled;
		private int _maximumPasteLines;
		private int _minimumCopyLength;
		private int _inputHistory;
		private int _bufferLines;

		public BufferPreferences()
		{
			_isLoggingEnabled = true;
			_minimumCopyLength = 3;
			_inputHistory = 50;
			_maximumPasteLines = 10;
			_bufferLines = 500;
		}

		public int BufferLines
		{
			get { return _bufferLines; }
			set
			{
				ValidateRange(value, 100);
				_bufferLines = value;

				OnPropertyChanged();
			}
		}

		public int InputHistory
		{
			get { return _inputHistory; }
			set
			{
				ValidateRange(value, 1, 1000);
				_inputHistory = value;
				OnPropertyChanged();
			}
		}

		public int MinimumCopyLength
		{
			get { return _minimumCopyLength; }
			set
			{
				ValidateRange(value, 1, 100);
				_minimumCopyLength = value;
				OnPropertyChanged();
			}
		}

		public int MaximumPasteLines
		{
			get { return _maximumPasteLines; }
			set
			{
				ValidateRange(value, 1, 100);
				_maximumPasteLines = value;
				OnPropertyChanged();
			}
		}

		public bool IsLoggingEnabled
		{
			get { return _isLoggingEnabled; }
			set
			{
				_isLoggingEnabled = value;
				OnPropertyChanged();
			}
		}
	}
}