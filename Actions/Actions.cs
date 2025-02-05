using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MinerPulse
{
	public static class Actions
	{
		public static void LoginPrivateKey(WiredStream W)
		{
			Auth K = DB.LoginPrivateKey(W.GuidVal);
			if (K != null)
			{
				W.OPENFIRE(K, "LOGIN_OK");
			}
			else
			{
				W.POPUP("Invalid Private Key", "Nope");
			}
		}

		public static void GetAggMinerData(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.SIDE_NOTI("NOPE", "Auth needed");
				return;
			}

			MergedDataBTC MERGED_DATA = new MergedDataBTC();
			MERGED_DATA.NETWORK_DATA = GlobalData.NetworkOverviewGlobal;
			MERGED_DATA.MINER_DATA = Helpers.GetAggregatedMinerData();

			W.OPENFIRE(MERGED_DATA);
		}

		public static async void MinerScanRequest(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.SIDE_NOTI("NOPE", "Auth needed");
				return;
			}
			await FindMiners.ScanAsync();
			Thread.Sleep(500);
			W.RUSHWILLSUCC("FINISHED_SCAN");

			List<Miner> miners = Globals.MinerList.Values.ToList();
			W.OPENFIRE(miners, "GET_ALL_MINERS");
		}

		public static void UpdateMinerPass(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.SIDE_NOTI("NOPE", "Auth needed");
				return;
			}
			MinerPWReq CHG_PW_REQ = MessagePackSerializer.Deserialize<MinerPWReq>(W.RAW_MESSAGE);
			Auth ThisUser = W.FullUser;
			ThisUser.MicroBTUser = CHG_PW_REQ.MinerUsername;
			ThisUser.MicroBTPW = CHG_PW_REQ.MinerPassword;
			DB.COREDB.Update(ThisUser);

			W.RUSHWILLSUCC("UPDATE_SETTINGS_OK", true);
		}

		public static void UpdateScanIP(WiredStream W)
		{
			if (!W.IsLoggedIn)
			{
				W.SIDE_NOTI("NOPE", "Auth needed");
				return;
			}
			IPScanReq CHG_IP_REQ = MessagePackSerializer.Deserialize<IPScanReq>(W.RAW_MESSAGE);
			Auth ThisUser = W.FullUser;
			ThisUser.IPRangeStart = CHG_IP_REQ.IPStart;
			ThisUser.IPRangeEnd = CHG_IP_REQ.IPEnd;
			ThisUser.RefreshRate = CHG_IP_REQ.RefreshRate;

			DB.COREDB.Update(ThisUser);

			W.RUSHWILLSUCC("UPDATE_IP_RANGE_OK", true);

			Auth N_U = DB.COREDB.Table<Auth>().Where((Auth x) => x.PrivateKey == ThisUser.PrivateKey).First();

			W.OPENFIRE(N_U, "UPDATE_USER");
		}

		public static void Login(WiredStream W)
		{
			try
			{
				MiniAcc loginRequest = MessagePackSerializer.Deserialize<MiniAcc>(W.RAW_MESSAGE);
				Console.WriteLine("Login Request for Username: {0}", loginRequest.Username);

				Auth user = DB.COREDB.Table<Auth>().FirstOrDefault((Auth x) => x.Username == loginRequest.Username);

				if (user == null)
				{
					W.POPUP("Failure", $"Username {loginRequest.Username} does not exist.");
					return;
				}

				bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);

				if (!isPasswordValid)
				{
					W.POPUP("Failure", "Incorrect password.");
					return;
				}

				string sessionToken = Guid.NewGuid().ToString();

				W.BASH($"<gre>❯ User [{user.Username}] logged in successfully</gre>");
				W.SIDE_NOTI("Success!", $"Welcome back, {user.Username}!");
				W.OPENFIRE(user, "LOGIN_OK");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during login: {0}", ex.Message);
				W.POPUP("Error", "An unexpected error occurred during login. Please try again.");
			}
		}

		public static void FirstReq(WiredStream W)
		{
			GenericRequest GenRequest = MessagePackSerializer.Deserialize<GenericRequest>(W.RAW_MESSAGE);

			if (DB.COREDB.Table<Auth>().Count() > 0)
			{
				W.RUSH("BUILD_LOGIN");
			}
			else
			{
				W.RUSH("BUILD_CREATE_ACC");
			}
		}

		public static void CreateAcc(WiredStream W)
		{
			MiniAcc acc = MessagePackSerializer.Deserialize<MiniAcc>(W.RAW_MESSAGE);
			Console.WriteLine("AccCreation Request [{0}]", acc.Username);

			if (DB.COREDB.Table<Auth>().Count() > 0)
			{
				W.RUSH("BUILD_LOGIN");
				W.POPUP("ACCOUNT CREATION DISABLED", "ADMIN ACC ALREADY CREATED");
				return;
			}

			if (acc.Password.Length < 6)
			{
				W.POPUP("Failure", "Please ensure your password is at least 6 characters long");
				return;
			}

			if (DB.COREDB.Table<Auth>().Any((Auth x) => x.Username == acc.Username))
			{
				W.POPUP("Failure", $"Username {acc.Username} already exists");
				return;
			}

			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(acc.Password);
			bool isRoot = !DB.COREDB.Table<Auth>().Any();

			Auth newAuth = new Auth
			{
				UUID = Guid.NewGuid(),
				PrivateKey = Guid.NewGuid(),
				Username = acc.Username,
				Password = hashedPassword,
				root = isRoot,
				CreationDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};

			DB.COREDB.Insert(newAuth);

			W.BASH($"<gre>❯ Account [{acc.Username}] created</gre>");
			W.SIDE_NOTI("Success!", "Account created");
			W.OPENFIRE(newAuth, "LOGIN_OK");
		}

		public static void ChangePassword(WiredStream W)
		{
			try
			{
				ChangePasswordRequest request = MessagePackSerializer.Deserialize<ChangePasswordRequest>(W.RAW_MESSAGE);
				Console.WriteLine("Change Password Request for PrivateKey: {0}", request.PrivateKey);

				Auth user = DB.LoginPrivateKey(request.PrivateKey);
				if (user == null)
				{
					W.POPUP("Failure", "Invalid Private Key.");
					return;
				}

				bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password);
				if (!isPasswordValid)
				{
					W.POPUP("Failure", "Current password is incorrect.");
					return;
				}

				if (request.NewPassword.Length < 6)
				{
					W.POPUP("Failure", "New password must be at least 6 characters long.");
					return;
				}

				string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
				user.Password = hashedPassword;

				DB.COREDB.Update(user);

				W.BASH($"<gre>❯ Password for user [{user.Username}] changed successfully</gre>");
				W.SIDE_NOTI("Success!", "Your password has been updated.");
				W.RUSHWILLSUCC("CHANGE_PASSWORD", true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during password change: {0}", ex.Message);
				W.POPUP("Error", "An unexpected error occurred while changing the password.");
			}
		}

		public static void SaveMinerPassword(WiredStream W)
		{
			try
			{
				SaveMinerPasswordRequest request = MessagePackSerializer.Deserialize<SaveMinerPasswordRequest>(W.RAW_MESSAGE);
				Console.WriteLine("Save Miner Password Request for Miner ID: {0}", request.MinerId);

				Auth user = DB.LoginPrivateKey(request.PrivateKey);
				if (user == null)
				{
					W.POPUP("Failure", "Invalid Private Key.");
					return;
				}

				if (!Globals.MinerList.Values.Any((Miner m) => m.ID == request.MinerId))
				{
					W.POPUP("Failure", $"Miner with ID {request.MinerId} does not exist.");
					return;
				}

				string encryptedPassword = EncryptionHelper.EncryptString(request.MinerPassword);

				MinerPassword existingPassword = DB.COREDB
					.Table<MinerPassword>()
					.FirstOrDefault(mp => mp.UserUUID == user.UUID && mp.MinerId == request.MinerId);
				if (existingPassword != null)
				{
					existingPassword.Password = encryptedPassword;
					existingPassword.MinerUser = request.MinerUser;
					DB.COREDB.Update(existingPassword);
				}
				else
				{
					MinerPassword newPassword = new MinerPassword
					{
						ID = 0,
						UserUUID = user.UUID,
						MinerId = request.MinerId,
						MinerUser = request.MinerUser,
						EncryptedMinerPassword = encryptedPassword
					};
					DB.COREDB.Insert(newPassword);
				}

				W.BASH($"<gre>❯ Miner credentials saved successfully for Miner ID [{request.MinerId}]</gre>");
				W.SIDE_NOTI("Success!", "Miner credentials have been saved.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during saving miner credentials: {0}", ex.Message);
				W.POPUP("Error", "An unexpected error occurred while saving the miner credentials.");
			}
		}
	}
}
