using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IrcSays.Utility;
using JetBrains.Annotations;

namespace IrcSays.Preferences
{
	public class PreferenceBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void ValidateRange(int value, int minValue, int? maxValue = null, [CallerMemberName] string propertyName = null)
		{
			if (value < minValue ||
				(maxValue.HasValue && value > maxValue))
			{
				var upperLimit = "(none)";
				if (maxValue.HasValue)
				{
					upperLimit = maxValue.ToString();
				}
				
				throw new InvalidOperationException("Property {0} value {1} outside range {2} - {3}".FormatWith(
					propertyName, value, minValue, upperLimit));
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}