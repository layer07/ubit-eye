using System.Runtime.CompilerServices;

namespace MinerPulse
{
	public static class MemDB
	{
		private static Dictionary<Guid, string> privateKeyToUsernameCache = new Dictionary<Guid, string>();
		private static Dictionary<Guid, Auth> FullUser = new Dictionary<Guid, Auth>();

		public static void LoadEntriesIntoMemDB()
		{
			privateKeyToUsernameCache = DB.GetAllPrivateKeysAndUsernames();
			FullUser = DB.COREDB.Table<Auth>().ToDictionary(auth => auth.PrivateKey);
		}

		public static Auth GetFullUser(Guid userId)
		{
			if (FullUser.TryGetValue(userId, out Auth auth))
			{
				return auth;
			}

			auth = DB.COREDB.Table<Auth>().FirstOrDefault(a => a.PrivateKey == userId);

			if (auth != null)
			{
				FullUser[userId] = auth;
			}

			if (auth == null)
			{
				throw new Exception($"User with ID {userId} not found in both cache and database.");
			}

			return auth;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static string PrivateKeyToUsername(Guid privateKey)
		{
			if (privateKeyToUsernameCache.TryGetValue(privateKey, out string username))
			{
				return username;
			}

			username = DB.PrivateKeyToUsername(privateKey);
			if (!string.IsNullOrEmpty(username))
			{
				privateKeyToUsernameCache[privateKey] = username;
			}

			return username;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static void RefreshCache()
		{
			LoadEntriesIntoMemDB();
		}
	}
}