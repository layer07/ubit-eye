using MessagePack;
using MinerPulse.Models;

namespace MinerPulse
{
	public static class ReadMiners
	{
		public static void GET_ALL_MINERS(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.POPUP("You must be logged in.", "Nope");
				return;
			}
			List<Miner> miners = Globals.MinerList.Values.ToList();
			W.OPENFIRE(miners);
		}

		public static void GET_ALL_MINERS_CUSTOM(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.POPUP("You must be logged in.", "Nope");
				return;
			}

			MinerScanReqV2 CHG_PW_REQ = MessagePackSerializer.Deserialize<MinerScanReqV2>(W.RAW_MESSAGE);
			Console.WriteLine("Scanning from {0} - {1}", CHG_PW_REQ.IPStart, CHG_PW_REQ.IPEnd);
			Console.WriteLine(CHG_PW_REQ.IPStart);
			FindMiners.ScanAsync(true, CHG_PW_REQ.IPStart, CHG_PW_REQ.IPEnd).Wait();
			Console.WriteLine("Finished Scan");

			List<Miner> miners = Globals.MinerList.Values.ToList();
			if (miners.Count > 0)
			{
				W.OPENFIRE(miners);
			}
			else
			{
				W.POPUP("No miners found", string.Format("{0} - {1}", CHG_PW_REQ.IPStart, CHG_PW_REQ.IPEnd));
			}
		}

		public static void GET_THIS_MINER(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.POPUP("You must be logged in.", "Authentication Required");
				return;
			}

			int minerID = W.IntVal;
			if (Globals.MinerData.TryGetValue(minerID, out MegaObject miner))
			{
				W.OPENFIRE(miner);
			}
			else
			{
				W.POPUP($"Miner with ID {minerID} not found.", "Error");
			}
		}
	}
}
