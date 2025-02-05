using Fleck;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPulse
{
	[MessagePackObject(true)]
	public record struct BENCHMARK
	{
		public string MilliSeconds { get; set; }
		public long NanoSeconds { get; set; }
		public long Ticks { get; set; }
		public long MS { get; set; }
	}
	public record struct WiredStream
	{
		public string Will { get; set; }
		public byte[] RAW_MESSAGE { get; set; }
		public IWebSocketConnection SOCKET { get; set; }
		public long RECV_TIME { get; set; }
		public string Username { get; set; }
		public Auth FullUser { get; set; }
		public bool root { get; set; } = false;
		public string StringVal { get; set; } = string.Empty;
		public int IntVal { get; set; }
		public Guid GuidVal { get; set; }
		public bool IsLoggedIn { get; set; } = false;
		public bool OKAY { get; set; } = false;

		public WiredStream()
		{
			RECV_TIME = Globals.RUNTIME_SW.ElapsedTicks;
		}

		private void SendMessage(Dictionary<string, object> userStream)
		{
			SOCKET.Send(MessagePackSerializer.Serialize(userStream));
		}

		private Dictionary<string, object> CreateUserStream(string will, dynamic streamInfo, dynamic obj = null, string message = "", long? timestamp = null, string msg1 = "", string msg2 = "", string line = "", bool? error = null, bool SUCC = false)
		{
			var userStream = new Dictionary<string, object> { { "Will", will }, { "StreamInfo", streamInfo } };

			if (obj != null)
				userStream["Obj"] = obj;
			if (!string.IsNullOrEmpty(message))
				userStream["Message"] = message;
			if (timestamp.HasValue)
				userStream["Timestamp"] = timestamp.Value;
			if (!string.IsNullOrEmpty(msg1))
				userStream["Msg1"] = msg1;
			if (!string.IsNullOrEmpty(msg2))
				userStream["Msg2"] = msg2;
			if (!string.IsNullOrEmpty(line))
				userStream["LINE"] = line;

			if (error.HasValue)
				userStream["ERROR"] = error.Value;

			return userStream;
		}

		private Dictionary<string, object> CreateUserStreamMini(string will, dynamic streamInfo, bool OKAY = false)
		{
			var userStream = new Dictionary<string, object> { { "Will", will }, { "StreamInfo", streamInfo } };

			userStream["OKAY"] = OKAY;

			return userStream;
		}

		private Dictionary<string, object> CreateUserStreamSucc(string will, dynamic streamInfo, dynamic obj = null, string message = "", long? timestamp = null, string msg1 = "", string msg2 = "", string line = "", bool? error = null)
		{
			var userStream = new Dictionary<string, object> { { "Will", will }, { "StreamInfo", streamInfo } };

			if (obj != null)
				userStream["Obj"] = obj;
			if (!string.IsNullOrEmpty(message))
				userStream["Message"] = message;
			if (timestamp.HasValue)
				userStream["Timestamp"] = timestamp.Value;
			if (!string.IsNullOrEmpty(msg1))
				userStream["Msg1"] = msg1;
			if (!string.IsNullOrEmpty(msg2))
				userStream["Msg2"] = msg2;
			if (!string.IsNullOrEmpty(line))
				userStream["LINE"] = line;

			if (error.HasValue)
				userStream["ERROR"] = error.Value;

			return userStream;
		}

		public void OPENFIRE(dynamic OBJ, string NEW_WILL = "DEFAULT_WILL")
		{
			BENCHMARK streamInfo = Helpers.CREATE_BENCH(RECV_TIME);

			var userStream = CreateUserStream(NEW_WILL != "DEFAULT_WILL" ? NEW_WILL : Will, streamInfo, OBJ);
			SendMessage(userStream);
			// UNSLogs.LogSent(this, streamInfo);
		}

		public void RUSH(string NEW_WILL = "ACK", string MSG = "", long? TimeStamp = null)
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			var userStream = CreateUserStream(NEW_WILL, streamInfo, message: MSG, timestamp: TimeStamp);
			SendMessage(userStream);
		}

		public void RUSHWILLSUCC(string NEW_WILL = "ACK", bool SUCC = false)
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			var userStream = CreateUserStreamMini(NEW_WILL, streamInfo, SUCC);
			SendMessage(userStream);
		}

		public void BASH(string LINE = "", bool? ERROR = null)
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			var userStream = CreateUserStream("BASH", streamInfo, line: LINE, error: ERROR);
			SendMessage(userStream);
		}

		public void REALBASH(string LINE = "", bool? ERROR = null)
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			var userStream = CreateUserStream("REALBASH", streamInfo, line: LINE, error: ERROR);
			SendMessage(userStream);
		}

		public void SIDE_NOTI(string MSG_1 = "", string MSG_2 = "")
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			var userStream = CreateUserStream("SIDE_NOTI", streamInfo, msg1: MSG_1, msg2: MSG_2);
			SendMessage(userStream);
		}

		public void POPUP(string MSG = "Nothing.", string MSG_2 = "[...]")
		{
			var streamInfo = Helpers.CREATE_BENCH(RECV_TIME);
			dynamic K = new ExpandoObject();
			K.Message = MSG;
			K.Message2 = MSG_2;
			var userStream = CreateUserStream("POPUP", streamInfo, obj: K);
			SendMessage(userStream);
		}
	}

}
