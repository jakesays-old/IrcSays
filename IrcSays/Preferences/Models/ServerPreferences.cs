using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IrcSays.Utility;

namespace IrcSays.Preferences.Models
{
	public class ServerPreferences : PreferenceBase
	{
		private ObservableCollection<ServerPreference> _servers;

		public ServerPreferences()
		{
			_servers = new ObservableCollection<ServerPreference>();
			AddNewCommand = new RelayCommand((o) =>
			{
				Servers.Add(new ServerPreference()
					{
						Name = "New Server",
						Port = 6667,
					});
			});

			RemoveServerCommand = new RelayCommand(o =>
			{
				Servers.Remove(SelectedServer);
			},  o => SelectedServer != null);
		}

		public ICommand AddNewCommand { get; set; }
		public ICommand RemoveServerCommand { get; set; }
		public ObservableCollection<ServerPreference> Servers { get; set; }
		public ServerPreference SelectedServer { get; set; }

	}
}
