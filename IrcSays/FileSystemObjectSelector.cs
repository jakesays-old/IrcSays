using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using IrcSays.Application;
using IrcSays.Utility;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;

namespace IrcSays
{
	public static class FileSystemObjectSelector
	{
		public static string SaveFileAs(IntPtr parentWindow, string title, 
			string initialDirectory,
			List<FileSystemFilter> filters,
			string defaultExtension = null,
			bool addToMruList = false)
		{
			var dlg = new CommonSaveFileDialog
			{
				AlwaysAppendDefaultExtension = true,
				AddToMostRecentlyUsedList = addToMruList,
				CreatePrompt = false,
			};

			if (defaultExtension.NotNullOrEmpty())
			{
				dlg.DefaultExtension = defaultExtension;
			}

			if (!filters.IsNullOrEmpty())
			{
				foreach (var filter in filters)
				{
					if (filter.Description.IsNullOrEmpty() ||
						filter.Filter.IsNullOrEmpty())
					{
						throw new InvalidOperationException("Both description and filter required");
					}

					dlg.Filters.Add(new CommonFileDialogFilter(filter.Description, filter.Filter));
				}
			}
			else
			{
				dlg.Filters.Add(new CommonFileDialogFilter("All files", "*.*"));
			}

			if (initialDirectory.NotNullOrEmpty())
			{
				dlg.InitialDirectory = initialDirectory;
			}

			if (dlg.ShowDialog(parentWindow) == CommonFileDialogResult.Ok)
			{
				try
				{
					return dlg.FileName;
				}
				catch
				{
					Xceed.Wpf.Toolkit.MessageBox.Show("Could not create a ShellObject from the selected item",
						App.ChatWindow.Title,
						MessageBoxButton.OK,
						MessageBoxImage.Error);
				}
			}

			return null;
		}

		public static List<string> SelectFiles(IntPtr parentWindow, string title, string initialFolder,
			List<FileSystemFilter> filters, 
			bool addToMruList = false,			
			bool multiSelect = false
			)
		{
			var dlg = new CommonOpenFileDialog
			{
				EnsureReadOnly = true,
				IsFolderPicker = false,
				AllowNonFileSystemItems = false,
				Title = title,
				AddToMostRecentlyUsedList = addToMruList,
				EnsurePathExists = false,
				Multiselect = multiSelect
			};

			if (!filters.IsNullOrEmpty())
			{
				foreach (var filter in filters)
				{
					if (filter.Description.IsNullOrEmpty() ||
						filter.Filter.IsNullOrEmpty())
					{
						throw new InvalidOperationException("Both description and filter required");
					}

					dlg.Filters.Add(new CommonFileDialogFilter(filter.Description, filter.Filter));
				}
			}
			else
			{
				dlg.Filters.Add(new CommonFileDialogFilter("All files", "*.*"));
			}

			if (initialFolder.NotNullOrEmpty())
			{
				dlg.InitialDirectory = initialFolder;
			}

			if (dlg.ShowDialog(parentWindow) != CommonFileDialogResult.Ok)
			{
				return null;
			}

			try
			{
				// Try to get a valid selected item
				var selectedFiles = dlg.FilesAsShellObject.Select(s => s.ParsingName).ToList();
				return selectedFiles;
			}
			catch
			{
				Xceed.Wpf.Toolkit.MessageBox.Show("Could not create a ShellObject from the selected item",
					App.ChatWindow.Title,
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}

			return null;
		}

		public static string SelectFolder(IntPtr parentWindow, string title, string initialFolder)
		{
			var dlg = new CommonOpenFileDialog
			{
				EnsureReadOnly = true,
				IsFolderPicker = true,
				AllowNonFileSystemItems = false,
				Title = title,
				AddToMostRecentlyUsedList = true,
				EnsurePathExists = false
			};

			if (initialFolder.NotNullOrEmpty())
			{
				dlg.InitialDirectory = initialFolder;
			}

			if (dlg.ShowDialog(parentWindow) != CommonFileDialogResult.Ok)
			{
				return null;
			}

			try
			{
				// Try to get a valid selected item
				var shellObject = dlg.FileAsShellObject as ShellContainer;
				if (shellObject != null)
				{
					return shellObject.ParsingName;
				}
			}
			catch
			{
				Xceed.Wpf.Toolkit.MessageBox.Show("Could not create a ShellObject from the selected item",
					App.ChatWindow.Title,
					MessageBoxButton.OK,
					MessageBoxImage.Error);
			}

			return null;
		}
	}
}