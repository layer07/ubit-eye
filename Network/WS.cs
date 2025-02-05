using Fleck;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace MinerPulse
{
	internal class WS
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MainServer()
		{
			FleckLog.Level = Fleck.LogLevel.Error;

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

			string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert.pfx");

			X509Certificate2 certificate = new X509Certificate2(certPath, "102030");

			var server = new WebSocketServer($"wss://0.0.0.0:{ConfigParser.Config.Web.WSS_PORT}") { Certificate = certificate };

			server.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls;
			PreJitCriticalMethods();

			server.ListenerSocket.NoDelay = true;
			server.Start(socket => {
				socket.OnOpen = () => WS_OPEN(socket);
				socket.OnClose = () => WS_CLOSE(socket);
				socket.OnMessage = async message => await WS_RECV_STR(socket, message);
				socket.OnBinary = async binarymsg => WS_RECV(socket, binarymsg);
			});
		}

		private static void PreJitCriticalMethods()
		{
			RuntimeHelpers.PrepareMethod(typeof(WS).GetMethod(nameof(WS_RECV), new[] { typeof(IWebSocketConnection), typeof(byte[]) }).MethodHandle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WS_OPEN(IWebSocketConnection SOCKET)
		{
			try
			{
			}
			catch (Exception ex)
			{
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WS_CLOSE(IWebSocketConnection SOCKET) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WS_RECV(IWebSocketConnection SOCKET, byte[] BYTE_MSG)
		{
			_ = new Logic(SOCKET, BYTE_MSG);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static async Task WS_RECV_STR(IWebSocketConnection SOCKET, string MESSAGE) { }
	}
}