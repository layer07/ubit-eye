// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * Helpers.cs
 * 
 * Version: @(#)Helpers.cs 0.0.02 15/01/2025
 *
 * Description: Utility functions for MinerStack, including a header printer and asynchronous scanner.
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static MinerPulse.Watcher;
using MinerPulse.Models;
using System.Runtime.InteropServices;
using System.Reflection;

namespace MinerPulse
{
	public static partial class Helpers
	{
		/// <summary>
		/// Prints the stylized header to the console.
		/// </summary>
		public static void PrintHeader()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(" ███▄ ▄███▓ ██▓ ▄████▄   ██▀███   ▒█████   ▄▄▄▄   ▄▄▄█████▓");
			Console.WriteLine("▓██▒▀█▀ ██▒▓██▒▒██▀ ▀█  ▓██ ▒ ██▒▒██▒  ██▒▓█████▄ ▓  ██▒ ▓▒");
			Console.WriteLine("▓██    ▓██░▒██▒▒▓█    ▄ ▓██ ░▄█ ▒▒██░  ██▒▒██▒ ▄██▒ ▓██░ ▒░");
			Console.WriteLine("▒██    ▒██ ░██░▒▓▓▄ ▄██▒▒██▀▀█▄  ▒██   ██░▒██░█▀  ░ ▓██▓ ░ ");
			Console.WriteLine("▒██▒   ░██▒░██░▒ ▓███▀ ░░██▓ ▒██▒░ ████▓▒░░▓█  ▀█▓  ▒██▒ ░ ");
			Console.WriteLine("░ ▒░   ░  ░░▓  ░ ░▒ ▒  ░░ ▒▓ ░▒▓░░ ▒░▒░▒░ ░▒▓███▀▒  ▒ ░░   ");
			Console.WriteLine("░  ░      ░ ▒ ░  ░  ▒     ░▒ ░ ▒░  ░ ▒ ▒░ ▒░▒   ░     ░    ");
			Console.WriteLine("░      ░    ▒ ░░          ░░   ░ ░ ░ ░ ▒   ░    ░   ░      ");
			Console.WriteLine("       ░    ░  ░ ░         ░         ░ ░   ░               ");
			Console.WriteLine("               ░                                ░           ");
			Console.ResetColor();
		}

		public static BENCHMARK CREATE_BENCH(long startTime)
		{
			long elapsedTicks = Globals.RUNTIME_SW.ElapsedTicks - startTime;
			return new BENCHMARK
			{
				NanoSeconds = elapsedTicks * Globals.Penalty,
				MilliSeconds = $"{(double)elapsedTicks / Globals.SecPenalty:0.000} ms",
				Ticks = elapsedTicks,
				MS = elapsedTicks / Globals.SecPenalty
			};
		}

		public static bool OS_Check()
		{
			bool LinuxCheck = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			Globals.IsLinux = LinuxCheck;
			Globals.Penalty = Globals.IsLinux ? 1 : 100;
			Globals.SecPenalty = Globals.IsLinux ? 1000000 : 10000;
			return LinuxCheck;
		}

		public static AggregatedMinerData GetAggregatedMinerData()
		{
			// Sum of all HS RT
			double totalHsRt = Globals.MinerData.Values
				.Where(m => m.Summary?.Summaries != null)
				.Sum(m => m.Summary.Summaries.Sum(s => s.HsRt));

			// Sum of all Power
			double totalPower = Globals.MinerData.Values
				.Where(m => m.Summary?.Summaries != null)
				.Sum(m => m.Summary.Summaries.Sum(s => s.Power));

			// Sum of all Accepted and Rejected
			int totalAccepted = Globals.MinerData.Values
				.Where(m => m.Summary?.Summaries != null)
				.Sum(m => m.Summary.Summaries.Sum(s => s.Accepted));

			int totalRejected = Globals.MinerData.Values
				.Where(m => m.Summary?.Summaries != null)
				.Sum(m => m.Summary.Summaries.Sum(s => s.Rejected));

			// List of VINs under PSU
			var vinList = Globals.MinerData.Values
				.Where(m => m.PSU?.Msg?.Vin != null)
				.Select(m => m.PSU.Msg.Vin)
				.ToList();
			
			return new AggregatedMinerData(totalHsRt, totalPower, totalAccepted, totalRejected, vinList);
		}


		public static void DumpAllMiners()
		{
			if (Globals.MinerData.IsEmpty)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("[INFO] No miner data available to dump.");
				Console.ResetColor();
				return;
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("\n📜 Dumping all miners' data:\n");
			Console.ResetColor();

			foreach (var kvp in Globals.MinerData)
			{
				int minerId = kvp.Key;
				MegaObject data = kvp.Value;

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"--- Miner ID: {minerId} ---");
				Console.ResetColor();

				string summary = JsonSerializer.Serialize(data.Summary, new JsonSerializerOptions { WriteIndented = true });
				string devices = JsonSerializer.Serialize(data.Devices, new JsonSerializerOptions { WriteIndented = true });
				string psu = JsonSerializer.Serialize(data.PSU, new JsonSerializerOptions { WriteIndented = true });
				string errorCode = JsonSerializer.Serialize(data.ErrorCode, new JsonSerializerOptions { WriteIndented = true });

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Summary:");
				Console.ResetColor();
				Console.WriteLine(summary);
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Devices:");
				Console.ResetColor();
				Console.WriteLine(devices);
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("PSU:");
				Console.ResetColor();
				Console.WriteLine(psu);
				Console.WriteLine();

				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Error Code:");
				Console.ResetColor();
				Console.WriteLine(errorCode);
				Console.WriteLine();
			}
		}

		


	}
}
