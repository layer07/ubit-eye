using MessagePack;
using SQLite;

namespace MinerPulse
{
	[MessagePackObject(true)]
	public class Auth
	{
		[IgnoreMember]
		[PrimaryKey]
		[AutoIncrement]
		public int ID { get; set; }
		[Indexed]
		public Guid UUID { get; set; }
		[Indexed]
		public string Username { get; set; }
		[Indexed]
		public string Password { get; set; }
		[Indexed]
		public Guid PrivateKey { get; set; }
		public bool root { get; set; }
		public long CreationDate { get; set; }
		public string ConfigString { get; set; }
		public string MicroBTUser { get; set; }
		public string MicroBTPW { get; set; }
		public string IPRangeStart { get; set; }
		public string IPRangeEnd { get; set; }
		public int RefreshRate { get; set; }	
	}
}
