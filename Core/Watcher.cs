// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * Watcher.cs
 * 
 * Version: @(#)Watcher.cs 0.0.07 16/01/2025
 *
 * Description: Basic TCP-Client for grabbing WhatsMiner telemetry.
 *
 * Author: D. Leatti (Forbannet)
 * URL: https://kernelriot.com
 * Github: /layer07
 *
 *        ██▓    ▄▄▄     ▓██   ██▓▓█████  ██▀███  
 *       ▓██▒   ▒████▄    ▒██  ██▒▓█   ▀ ▓██ ▒ ██▒
 *       ▒██░   ▒██  ▀█▄   ▒██ ██░▒███   ▓██ ░▄█ ▒
 *       ▒██░   ░██▄▄▄▄██  ░ ▐██▓░▒▓█ ▄ ▒██▀▀█▄  
 *       ░██████▒▓█   ▓██▒ ░ ██▒▓░░▒████▒░██▓ ▒██▒
 *       ░ ▒░▓  ░▒▒   ▓▒█░  ██▒▒▒ ░░ ▒░ ░░ ▒▓ ░▒▓░
 *       ░ ░ ▒  ░ ▒   ▒▒ ░▓██ ░▒░  ░ ░  ░  ░▒ ░ ▒░
 *         ░ ░    ░   ▒   ▒ ▒ ░░     ░     ░░   ░ 
 *           ░  ░     ░  ░░ ░        ░  ░   ░     
 */
using MinerPulse.Models;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinerPulse
{
	public static class Watcher
	{
		private static readonly ConcurrentDictionary<int, bool> Blacklist = new ConcurrentDictionary<int, bool>();

		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter(), new StringOrNumberJsonConverter() }
		};

		public static async Task Watch()
		{
			while (true)
			{
				List<Miner> miners = Globals.MinerList.Values.ToList();

				await Task.WhenAll(miners.Select(ProcessMinerAsync));

				await Task.Delay(1000);
			}
		}

		private static async Task ProcessMinerAsync(Miner miner)
		{
			try
			{
				string summaryJson = await SendCommandAsync(miner.IP, "summary");
				SummaryResponse summary = Deserialize<SummaryResponse>(summaryJson);

				string devicesJson = await SendCommandAsync(miner.IP, "edevs");
				DevicesResponse devices = Deserialize<DevicesResponse>(devicesJson);

				string psuJson = await SendCommandAsync(miner.IP, "get_psu");
				PsuResponse psu = Deserialize<PsuResponse>(psuJson);

				ErrorCodeResponse errorCode = null;

				if (!Blacklist.ContainsKey(miner.ID))
				{
					string errorCodeJson = await SendCommandAsync(miner.IP, "get_error_code");

					if (!string.IsNullOrEmpty(errorCodeJson))
					{
						bool isInvalidCmd = false;

						try
						{
							using (JsonDocument doc = JsonDocument.Parse(errorCodeJson))
							{
								if (doc.RootElement.TryGetProperty("Msg", out JsonElement msgElement))
								{
									if (msgElement.ValueKind == JsonValueKind.String && msgElement.GetString() == "invalid cmd")
									{
										isInvalidCmd = true;
									}
								}
							}
						}
						catch (JsonException)
						{
							isInvalidCmd = false;
						}

						if (isInvalidCmd)
						{
							Blacklist.TryAdd(miner.ID, true);
							Console.ForegroundColor = ConsoleColor.DarkYellow;
							Console.WriteLine($"⚠️ Miner {miner.ID} added to blacklist due to invalid cmd.");
							Console.ResetColor();
						}
						else
						{
							errorCode = Deserialize<ErrorCodeResponse>(errorCodeJson);
						}
					}
				}

				MegaObject megaObject = Globals.MinerData.GetOrAdd(miner.ID,
																   _ => new MegaObject());
				megaObject.Summary = summary;
				megaObject.Devices = devices;
				megaObject.PSU = psu;
				megaObject.ErrorCode = errorCode;

				string logMessage = (summary == null || devices == null || psu == null || (!Blacklist.ContainsKey(miner.ID) && errorCode == null)) ? $"Incomplete data received for Miner ID {miner.ID:D2}." : null;
				logMessage?.LogWarning();
			}
			catch (Exception ex)
			{
				LogError($"Unexpected error processing miner {miner.IP} (ID: {miner.ID:D2}): {ex.Message}");
			}
		}

		private static async Task<string> SendCommandAsync(string minerIp, string command)
		{
			try
			{
				using TcpClient tcpClient = new TcpClient();
				await tcpClient.ConnectAsync(minerIp, 4028);
				using NetworkStream networkStream = tcpClient.GetStream();

				string commandJson = JsonSerializer.Serialize(new { cmd = command });
				byte[] commandBytes = Encoding.UTF8.GetBytes(commandJson);
				await networkStream.WriteAsync(commandBytes, 0, commandBytes.Length);

				byte[] responseBytes = new byte[65536];
				int bytesRead = await networkStream.ReadAsync(responseBytes, 0, responseBytes.Length);

				string response = (bytesRead == 0) ? null : Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

				return (response != null && IsValidJson(response)) ? response : null;
			}
			catch (Exception ex)
			{
				LogError($"Error communicating with miner {minerIp} for command '{command}': {ex.Message}");
				return null;
			}
		}

		private static T Deserialize<T>(string json)
			where T : class
		{
			if (string.IsNullOrEmpty(json))
				return null;

			try
			{
				return JsonSerializer.Deserialize<T>(json, JsonOptions);
			}
			catch (JsonException)
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.WriteLine($"[DEBUG] Failed to deserialize JSON to {typeof(T).Name}. JSON: {json}");
				Console.ResetColor();
				return null;
			}
		}

		private static bool IsValidJson(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				return false;

			str = str.Trim();
			if ((!str.StartsWith("{") || !str.EndsWith("}")) && (!str.StartsWith("[") || !str.EndsWith("]")))
				return false;

			try
			{
				using (JsonDocument doc = JsonDocument.Parse(str, new JsonDocumentOptions { AllowTrailingCommas = true }))
				{
					return true;
				}
			}
			catch (JsonException)
			{
				return false;
			}
		}

		private static void LogWarning(this string message)
		{
			if (message != null)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"⚠️ {message}");
				Console.ResetColor();
			}
		}

		private static void LogError(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"❌ {message}");
			Console.ResetColor();
		}
	}
}