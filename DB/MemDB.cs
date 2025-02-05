using MinerPulse;
using System;
using System.Collections.Generic;
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
			// Try to retrieve the Auth object from the FullUser cache
			if (FullUser.TryGetValue(userId, out Auth auth))
			{
				return auth;
			}

			// If not found in cache, fallback to the database
			auth = DB.COREDB.Table<Auth>().FirstOrDefault(a => a.PrivateKey == userId);

			// If found in the database, add it to the cache for future lookups
			if (auth != null)
			{
				FullUser[userId] = auth;
			}

			// Throw an exception if the user is not found
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

			// Fallback to database if not found in cache
			username = DB.PrivateKeyToUsername(privateKey);
			if (!string.IsNullOrEmpty(username))
			{
				// Add to the in-memory cache
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
