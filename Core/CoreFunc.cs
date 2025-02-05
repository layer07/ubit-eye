using MessagePack;
using MinerPulse;
using System;
using System.Collections.Generic;

namespace MinerPulse
{
	public static class CoreFunc
	{
		public static readonly Dictionary<string, Action<WiredStream>> FUNC_HASH =
			new()
			{
								{ "CreateAcc", w => Actions.CreateAcc(w) },
								{ "Login", w => Actions.Login(w) },
								{ "LOGIN_PRIVATE_KEY", w => Actions.LoginPrivateKey(w) },
								{ "PING", w => HandlePing(w) },
								{ "FIRST_REQ", w => Actions.FirstReq(w) },
								{ "GET_ALL_MINERS", w => ReadMiners.GET_ALL_MINERS(w) },
								{ "GET_ALL_MINERS_CUSTOM", w => ReadMiners.GET_ALL_MINERS_CUSTOM(w) },

								{ "GET_THIS_MINER", w => ReadMiners.GET_THIS_MINER(w) },
								{ "MINER_UPDATE", w => ReadMiners.GET_THIS_MINER(w) },
								{ "CHANGE_PASSWORD", w => Actions.ChangePassword(w) },
								{ "REQ_HOME_DATA", w => Actions.GetAggMinerData(w) },
								{ "UPDATE_MINER_PASS", w => Actions.UpdateMinerPass(w) },
								{ "UPDATE_MINER_IPS", w => Actions.UpdateScanIP(w) },
								{ "REFRESH_SCAN", w => Actions.MinerScanRequest(w) },

			};


		public static void HandlePing(WiredStream w)
		{
			PingBack TPing = MessagePackSerializer.Deserialize<PingBack>(w.RAW_MESSAGE);
			w.RUSH("PING", "RESPONSE", TPing.Timestamp);
		}
	}

	[MessagePackObject(true)]
	public struct PingBack
	{
		public string Will { get; set; }
		public long Timestamp { get; set; }
	}
}
