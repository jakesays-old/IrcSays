using System;
using System.IO;
using System.Net;

namespace IrcSays.Communication.Dcc
{
	/// <summary>
	///     This class is responsible for sending files via the DCC XMIT protocol.
	/// </summary>
	public sealed class DccXmitSender : DccOperation
	{
		private const int SendChunkSize = 4096;

		private bool _isTransferring = false;
		private readonly FileInfo _fileInfo;
		private FileStream _fileStream;
		private readonly byte[] _resumeBytes = new byte[4];
		private readonly byte[] _buffer = new byte[SendChunkSize];
		private int _handshakeBytesReceived;

		/// <summary>
		///     Construct a new DccXmitSender.
		/// </summary>
		/// <param name="fileInfo">A reference to the file to send.</param>
		public DccXmitSender(FileInfo fileInfo)
		{
			_fileInfo = fileInfo;
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			var timeStampBytes =
				BitConverter.GetBytes(
					IPAddress.HostToNetworkOrder((int) (_fileInfo.LastWriteTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds));
			QueueWrite(timeStampBytes, 0, 4);
		}

		protected override void OnReceived(byte[] buffer, int count)
		{
			base.OnReceived(buffer, count);

			if (!_isTransferring)
			{
				Array.Copy(buffer, 0, _resumeBytes, _handshakeBytesReceived, Math.Min(4, count));
				_handshakeBytesReceived += count;

				if (_handshakeBytesReceived >= 4)
				{
					var resume = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_resumeBytes, 0));
					_fileStream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);
					if (resume > 0)
					{
						_fileStream.Seek(resume, SeekOrigin.Begin);
						BytesTransferred = resume;
					}
					_isTransferring = true;
					WriteDataBlock();
				}
			}
		}

		protected override void OnSent(byte[] buffer, int offset, int count)
		{
			if (_isTransferring)
			{
				BytesTransferred += count;
				WriteDataBlock();
			}
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

		private void WriteDataBlock()
		{
			if (_fileStream.Position < _fileStream.Length)
			{
				var count = _fileStream.Read(_buffer, 0, SendChunkSize);
				QueueWrite(_buffer, 0, count);
			}
			else
			{
				Close();
			}
		}
	}
}