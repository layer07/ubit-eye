using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinerPulse.Watcher;
using MinerPulse.Models;
using System.Diagnostics;


namespace MinerPulse
{
	public static class Globals
	{
		public static ConcurrentDictionary<int, Miner> MinerList { get; } = new ConcurrentDictionary<int, Miner>();

		public static ConcurrentDictionary<int, MegaObject> MinerData { get; } = new ConcurrentDictionary<int, MegaObject>();
		public static ConcurrentDictionary<string, double> AggregatedTotals { get; } = new ConcurrentDictionary<string, double>();


		public static string Owner = null;

		public static Stopwatch RUNTIME_SW = Stopwatch.StartNew();
		public static bool IsLinux = false;
		public static long Penalty = 100;
		public static long SecPenalty = 1000000;

	}
}
