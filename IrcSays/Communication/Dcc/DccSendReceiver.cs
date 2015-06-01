using System;
using System.IO;
using System.Net;

namespace IrcSays.Communication.Dcc
{
	/// <summary>
	///     A class responsible for receiving files from another source using the DCC XMIT protocol.
	/// </summary>
	public sealed class DccSendReceiver : DccOperation
	{
		private FileInfo _fileInfo;
		private FileStream _fileStream;

		/// <summary>
		///     Gets or sets a value indicating whether the specified file will be overwritten if it already exists and a resume is
		///     not possible. If this is set to false,
		///     the file will be renamed.
		/// </summary>
		public bool ForceOverwrite { get; set; }

		/// <summary>
		///     Gets the path to the file that was created. This may differ from the initial file path if it has been automatically
		///     renamed to avoid overwriting a file.
		/// </summary>
		public string FileSavedAs { get; private set; }

		/// <summary>
		///     Construct a new DccSendReceiver.
		/// </summary>
		/// <param name="fileInfo">
		///     A reference to the file to save. If the file exists, a resume will be attempted. If the resume
		///     fails, the file will be renamed.
		/// </param>
		public DccSendReceiver(FileInfo fileInfo)
		{
			_fileInfo = fileInfo;
			FileSavedAs = fileInfo.FullName;
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			var i = 1;
			var fileName = Path.GetFileNameWithoutExtension(_fileInfo.Name);
			while (_fileInfo.Exists &&
					!ForceOverwrite)
			{
				_fileInfo = new FileInfo(
					Path.Combine(_fileInfo.DirectoryName,
						string.Format("{0} ({1}).{2}", fileName, i++, _fileInfo.Extension)));
			}
			FileSavedAs = _fileInfo.FullName;
			_fileStream = new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			base.OnReceived(buffer, count);

			_fileStream.Write(buffer, 0, count);
			BytesTransferred += count;
			var writeBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int) BytesTransferred));
			QueueWrite(writeBuffer, 0, 4);
		}

		protected override void OnDisconnected()
		{
			base.OnDisconnected();
			CloseFile();
		}

		protected override void OnError(Exception ex)
		{
			base.OnError(ex);
			CloseFile();
		}

		private void CloseFile()
		{
			if (_fileStream != null)
			{
				_fileStream.Dispose();
			}
		}
	}
}