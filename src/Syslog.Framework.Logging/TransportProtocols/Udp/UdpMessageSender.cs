using System;
using System.Net;
using System.Net.Sockets;

namespace Syslog.Framework.Logging.TransportProtocols.Udp
{
	/// <summary>
	/// Sends message to a syslog server using UDP datagrams.
	/// </summary>
	public class UdpMessageSender : IMessageSender, IDisposable
	{
		private UdpClient _udpClient;

		public UdpMessageSender(string serverHost, int serverPort)
		{
			_udpClient = new UdpClient(serverHost, serverPort);
		}

		public void SendMessageToServer(byte[] messageData)
		{
			_udpClient?.SendAsync(messageData, messageData.Length);
		}

		public void Dispose()
		{
			_udpClient?.Dispose();
			_udpClient = null;
		}
	}
}