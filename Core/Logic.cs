using Fleck;
using MessagePack;
using System;

namespace MinerPulse
{
	public class Logic
	{
		public Logic(IWebSocketConnection socket, byte[] byteMsg)
		{
			try
			{
				WillGetter inc = MessagePackSerializer.Deserialize<WillGetter>(byteMsg);

				string username = inc.PrivateKey != Guid.Empty ? MemDB.PrivateKeyToUsername(inc.PrivateKey) : null;

				bool isInvalidLogin = inc.PrivateKey != Guid.Empty && username == null;

				WiredStream w = new WiredStream { Will = inc.Will, RAW_MESSAGE = byteMsg, SOCKET = socket, StringVal = inc.StringVal, IntVal = inc.IntVal, GuidVal = inc.GuidVal, IsLoggedIn = username != null };

				if (w.IsLoggedIn)
				{
					w.FullUser = MemDB.GetFullUser(inc.PrivateKey);
				}

				if (isInvalidLogin)
				{
					w.RUSH("BAD_PKEY");
					return;
				}
				else
				{
					if (CoreFunc.FUNC_HASH.TryGetValue(w.Will, out Action<WiredStream> action))
					{
						action(w);
					}
					else
					{
						Console.WriteLine($"Unknown Will value: {w.Will}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in processing message: {ex.Message}");
			}
		}
	}

	[MessagePackObject(true)]
	public record struct WillGetter
	{
		public string Will { get; set; }
		public Guid PrivateKey { get; set; }
		public string StringVal { get; set; }
		public int IntVal { get; set; }
		public Guid GuidVal { get; set; }
	}
}
