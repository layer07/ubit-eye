// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * FindMiners.cs
 * 
 * Version: @(#)FindMiners.cs 0.0.03 29/01/2025
 *
 * Description: Utility class for scanning and discovering WhatsMiners within a specified IP range.
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
using System.Threading;
using System.Threading.Tasks;
using MinerPulse.Models;

namespace MinerPulse
{
	public static class FindMiners
	{
		private static string Rgb(int code) => $"\x1b[38;5;{code}m";
		private static string BoldRgb(int code) => $"\x1b[1;38;5;{code}m";
		private static readonly Random _rng = new Random();

		public static async Task ScanAsync(bool DBIPS_SCAN = false, string IPS = null, string IPE = null)
		{
			int apiPort = 4028;
			int timeoutMs = 500;
			int concurrencyLimit = 100;

			// Retrieve IP range
			string ipAddrStart = ConfigParser.Config.Miners.IP_ADDR_START?.ToString() ?? "192.168.15.1";
			string ipAddrEnd = ConfigParser.Config.Miners.IP_ADDR_END?.ToString() ?? "192.168.15.254";

			if (DBIPS_SCAN)
			{
				ipAddrStart = DB.COREDB.Table<Auth>().First().IPRangeStart;
				ipAddrEnd = DB.COREDB.Table<Auth>().First().IPRangeEnd;
			}

			if (!string.IsNullOrEmpty(IPS) && !string.IsNullOrEmpty(IPE))
			{
				ipAddrStart = IPS;
				ipAddrEnd = IPE;
			}

			// Header
			Console.WriteLine($"{Rgb(82)}▛▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▜");
			Console.WriteLine($"{Rgb(46)}▌ {Rgb(226)}SCANNING NETWORK: {Rgb(39)}{ipAddrStart} {Rgb(196)}»»» {Rgb(39)}{ipAddrEnd} {Rgb(46)}▐");
			Console.WriteLine($"{Rgb(82)}▙▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▟\x1b[0m\n");

			// Validate IP format
			string[] startSegments = ipAddrStart.Split('.');
			string[] endSegments = ipAddrEnd.Split('.');

			if (startSegments.Length != 4 || endSegments.Length != 4)
			{
				Console.WriteLine($"{Rgb(196)}[!] {Rgb(214)}INVALID IP FORMAT IN CONFIGURATION\x1b[0m");
				return;
			}

			// Subnet validation
			bool sameSubnet = startSegments[0] == endSegments[0] &&
							startSegments[1] == endSegments[1] &&
							startSegments[2] == endSegments[2];

			if (!sameSubnet)
			{
				Console.WriteLine($"{BoldRgb(196)}[!] {Rgb(214)}IP RANGE MUST BE IN SAME SUBNET\x1b[0m");
				return;
			}

			string baseIp = $"{startSegments[0]}.{startSegments[1]}.{startSegments[2]}.";
			int startRange, endRange;

			// IP parsing validation
			if (!int.TryParse(startSegments[3], out startRange) || !int.TryParse(endSegments[3], out endRange))
			{
				Console.WriteLine($"{Rgb(196)}[!] {Rgb(214)}INVALID IP RANGE SEGMENTS\x1b[0m");
				return;
			}

			if (startRange > endRange)
			{
				Console.WriteLine($"{BoldRgb(196)}[!] {Rgb(214)}INVERSE RANGE DETECTED (START > END)\x1b[0m");
				return;
			}

			// Scanning parameters display
			Console.WriteLine($"{Rgb(45)}══════════════ {Rgb(46)}SCAN PARAMETERS {Rgb(45)}══════════════");
			Console.WriteLine($"{Rgb(75)}» HOST RANGE:\t{Rgb(228)}{baseIp}{startRange} {Rgb(196)}➔ {Rgb(228)}{baseIp}{endRange}");
			Console.WriteLine($"{Rgb(75)}» TOTAL HOSTS:\t{Rgb(228)}{endRange - startRange + 1}");
			Console.WriteLine($"{Rgb(75)}» PORT:\t\t{Rgb(228)}{apiPort}/TCP");
			Console.WriteLine($"{Rgb(75)}» TIMEOUT:\t{Rgb(228)}{timeoutMs}ms");
			Console.WriteLine($"{Rgb(75)}» THREADS:\t{Rgb(228)}{concurrencyLimit}");
			Console.WriteLine($"{Rgb(45)}═══════════════════════════════════════════════\x1b[0m\n");

			int discovered = 0;
			var semaphore = new SemaphoreSlim(concurrencyLimit);

			List<Task> tasks = Enumerable.Range(startRange, endRange - startRange + 1)
				.Select(async i =>
				{
					await semaphore.WaitAsync();
					string ip = $"{baseIp}{i}";
					try
					{
						using var client = new TcpClient();
						var connectTask = client.ConnectAsync(ip, apiPort);
						var timeoutTask = Task.Delay(timeoutMs);
						var completedTask = await Task.WhenAny(connectTask, timeoutTask);

						if (completedTask == connectTask && client.Connected)
						{
							Interlocked.Increment(ref discovered);
							Console.WriteLine($"{Rgb(46)}[+] {Rgb(255)}LIVE HOST {Rgb(39)}{ip}:{apiPort} {Rgb(8)}«« {Rgb(93)}WHATSMINER_{i}\x1b[0m");

							string lastSegment = ip.Split('.')[3];
							if (int.TryParse(lastSegment, out int id))
							{
								var miner = new Miner
								{
									ID = id,
									IP = ip,
									Hostname = $"whatsminer{id}",
									Location = "DC-A",
									LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
								};

								Globals.MinerList.AddOrUpdate(
									miner.ID,
									miner,
									(key, existing) => {
										existing.IP = miner.IP;
										existing.Hostname = miner.Hostname;
										existing.Location = miner.Location;
										existing.LastUpdate = miner.LastUpdate;
										return existing;
									});

								Console.WriteLine($"{Rgb(226)}[~] {Rgb(255)}UPDATED REPO {Rgb(39)}ID_{id} {Rgb(8)}« {Rgb(214)}{DateTime.Now:HH:mm:ss}\x1b[0m");
							}
							else
							{
								Console.WriteLine($"{Rgb(196)}[!] {Rgb(214)}FAILED PARSE ID {Rgb(240)}FROM {ip}\x1b[0m");
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"{Rgb(124)}[!] {Rgb(241)}SCAN ERROR {Rgb(131)}{ip} {Rgb(8)}» {Rgb(240)}{ex.Message}\x1b[0m");
					}
					finally
					{
						semaphore.Release();
					}
				})
				.ToList();

			await Task.WhenAll(tasks);

			// Final report
			Console.WriteLine($"\n{Rgb(45)}════════════════ {Rgb(46)}SCAN SUMMARY {Rgb(45)}════════════════");
			Console.WriteLine($"{Rgb(75)}» TOTAL SCANNED:\t{Rgb(228)}{endRange - startRange + 1}");
			Console.WriteLine($"{Rgb(75)}» LIVE HOSTS:\t\t{Rgb(46)}{discovered}");
			Console.WriteLine($"{Rgb(75)}» REPO ENTRIES:\t{Rgb(228)}{Globals.MinerList.Count}");
			Console.WriteLine($"{Rgb(45)}═══════════════════════════════════════════════\x1b[0m");

			if (Globals.MinerList.Count > 0)
			{
				Console.WriteLine($"\n{Rgb(46)}ACTIVE MINERS FOUND:{Rgb(40)}");
				foreach (var miner in Globals.MinerList.Values.OrderBy(m => m.IP))
				{
					Console.WriteLine($" {Rgb(93)}➔ {Rgb(39)}{miner.IP} {Rgb(8)}« {Rgb(214)}ID_{miner.ID} {Rgb(240)}({miner.Hostname})\x1b[0m");
				}
			}
			else
			{
				Console.WriteLine($"\n{Rgb(196)}✖ NO MINERS DISCOVERED IN TARGET RANGE\x1b[0m");
			}
		}
	}

}
