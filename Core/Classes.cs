using MessagePack;
using SQLite;

[MessagePackObject(true)]
public class Miner
{
	public int ID { get; set; }
	public string IP { get; set; }
	public string Hostname { get; set; }
	public string Location { get; set; }
	public long LastUpdate { get; set; }
}


namespace MinerPulse
{
	[MessagePackObject(true)]
	public record struct ChangePasswordRequest
	{
		public Guid PrivateKey { get; set; }
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
	}
}




namespace MinerPulse
{
	[MessagePackObject(true)]
	public record struct ChangeAvatarRequest
	{
		public Guid PrivateKey { get; set; }
		public string AvatarUrl { get; set; }
	}
}


namespace MinerPulse
{
	[MessagePackObject(true)]
	public record struct SaveMinerPasswordRequest
	{
		public Guid PrivateKey { get; set; }
		public int MinerId { get; set; }
		public string MinerUser { get; set; }
		public string MinerPassword { get; set; }
	}
}

namespace MinerPulse
{
	[MessagePackObject(true)]
	public class MinerPassword
	{
		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }

		[Indexed]
		public Guid UserUUID { get; set; }

		[Indexed]
		public int MinerId { get; set; }

		public string MinerUser { get; set; }
		public string Password { get; set; }


		public string EncryptedMinerPassword { get; set; }

	}
}

[MessagePackObject(true)]
public record struct MinerPWReq
{
	public string Will { get; set; }
	public string MinerUsername { get; set; }
	public string MinerPassword { get; set; }
	public Guid PrivateKey { get; set; }
	public int RefreshRate { get; set; }
}

[MessagePackObject(true)]
public record struct MinerScanReqV2
{
	public string Will { get; set; }
	public string IPStart { get; set; }
	public string IPEnd { get; set; }
	public Guid PrivateKey { get; set; }
}


[MessagePackObject(true)]
public record struct IPScanReq
{
	public string Will { get; set; }
	
	public Guid PrivateKey { get; set; }
	public string IPStart { get; set; }
	public string IPEnd { get; set; }
	public int RefreshRate { get; set; }
	}



	[MessagePackObject(true)]
public class AggregatedMinerData
{
	public double TotalHsRt { get; set; }
	public double TotalPower { get; set; }
	public int TotalAccepted { get; set; }
	public int TotalRejected { get; set; }
	public List<string> VinList { get; set; }

	public AggregatedMinerData(double totalHsRt, double totalPower, int totalAccepted, int totalRejected, List<string> vinList)
	{
		TotalHsRt = totalHsRt;
		TotalPower = totalPower;
		TotalAccepted = totalAccepted;
		TotalRejected = totalRejected;
		VinList = vinList;
	}
}
