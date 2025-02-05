using SQLite;

namespace MinerPulse
{
	public static class DB
	{
		private static string GetRunningFolder()
		{
			return AppDomain.CurrentDomain.BaseDirectory;
		}

		public static string CONN_STRING_HDD = Path.Combine(GetRunningFolder(), "DB", "main.db");
		public static SQLiteConnection COREDB;

		static DB()
		{
			InitializeDatabase();
		}

		public static Dictionary<Guid, string> GetAllPrivateKeysAndUsernames()
		{
			return COREDB.Table<Auth>().ToDictionary(user => user.PrivateKey, user => user.Username);
		}

		public static string PrivateKeyToUsername(Guid PrivateKey)
		{
			return COREDB.Table<Auth>().FirstOrDefault(x => x.PrivateKey == PrivateKey)?.Username ?? Globals.Owner;
		}

		public static Auth LoginPrivateKey(Guid PrivateKey)
		{
			return DB.COREDB.Table<Auth>().Where(x => x.PrivateKey == PrivateKey).FirstOrDefault();
		}

		private static void InitializeDatabase()
		{
			try
			{
				string dbFolder = Path.Combine(GetRunningFolder(), "DB");

				if (!Directory.Exists(dbFolder))
				{
					Directory.CreateDirectory(dbFolder);
				}

				if (!File.Exists(CONN_STRING_HDD))
				{
					Console.WriteLine($"Database not found. Creating new database at: {CONN_STRING_HDD}");
					COREDB = new SQLiteConnection(CONN_STRING_HDD);
					CreateTables();
				}
				else
				{
					COREDB = new SQLiteConnection(CONN_STRING_HDD);
					Console.WriteLine($"Database loaded from: {CONN_STRING_HDD}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during DB initialization: " + ex.ToString());
				throw;
			}
		}

		private static void CreateTables()
		{
			COREDB.CreateTable<Auth>();
			Console.WriteLine("Database tables created.");
		}
	}
}
