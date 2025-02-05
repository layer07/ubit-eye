using MinerPulse;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinerPulse
{
	public static class Init
	{
		// === CONSTANTS ===
		private const string CONFIG_FILE = "app.conf";
		private const string DEFAULT_IP_START = "192.168.1.1";
		private const string DEFAULT_IP_END = "192.168.1.199";
		private const string DEFAULT_WEB_PORT_SSL = "44405"; 

		private const ConsoleColor INFO = ConsoleColor.Green;
		private const ConsoleColor WARN = ConsoleColor.Yellow;
		private const ConsoleColor ERR = ConsoleColor.Red;

		// === HELPER LAMBDAS & EXTENSIONS ===
		// Inline logging lambda.
		private static readonly Action<string, ConsoleColor> Log = (msg, col) =>
		{
			Console.ForegroundColor = col;
			Console.WriteLine(msg);
			Console.ResetColor();
		};

		// Inline prompt lambda.
		private static readonly Func<string, string, string> Prompt = (q, def) =>
		{
			Console.Write($"{q} (default: {def}): ");
			var inp = Console.ReadLine()?.Trim();
			return string.IsNullOrEmpty(inp) ? def : inp;
		};

		// === MAIN INITIALIZATION ===
		public static void Chk()
		{
			// Setup console encoding and culture.
			Console.OutputEncoding = Encoding.UTF8;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

			NativeLibChecker.CheckNativeLibInteractive();
			CertHelper.EnsureCertificate();
			Helpers.PrintHeader();

			// Ensure config file exists.
			EnsureConfigFile();

			ConfigParser.load_config(CONFIG_FILE, true);
			Helpers.OS_Check();

			if (ConfigParser.Config == null)
			{
				Log("[ERROR] Configuration not loaded. Exiting application.", ERR);
				return;
			}

			// Launch server based on config.
			LaunchStaticServer();

			try
			{
				FindMiners.ScanAsync().GetAwaiter().GetResult();
				Log("[INFO] Miner scanning completed successfully.", INFO);
			}
			catch (Exception ex)
			{
				Log($"[ERROR] Exception during scanning: {ex.Message}", ERR);
				return;
			}

			Task.Run(() => Watcher.Watch()).ConfigureAwait(false);
			Log("[INFO] Watcher started successfully.", INFO);

			Console.WriteLine("\n🔍 Monitoring in progress...");

			new Thread(WS.MainServer) { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
			new Thread(NetworkDataFetcher.FetchNetworkData) { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
			new Thread(FindLoop) { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
		}

		public static void ConfigChk()
		{
			if (ConfigParser.Config == null)
			{
				Log("[ERROR] Configuration not loaded. Exiting application.", ERR);
				return;
			}
		}

		// === CONFIG FILE GENERATION ===
		public static void EnsureConfigFile()
		{
			if (File.Exists(CONFIG_FILE))
			{
				Log("✔ Config file found. Proceeding...", INFO);
				return;
			}

			Log("ℹ Config file 'app.conf' not found.", WARN);

			var prompts = new Dictionary<string, string>
			{
				{ "IP_ADDR_START", DEFAULT_IP_START },
				{ "IP_ADDR_END", DEFAULT_IP_END },
				{ "WEB_PORT_SSL", DEFAULT_WEB_PORT_SSL }
			};

			var responses = prompts.ToDictionary(kvp => kvp.Key, kvp => Prompt($"Enter Miner {kvp.Key}", kvp.Value));

			// Compute default for WSS_PORT as (WEB_PORT_SSL + 1).
			int webPort;
			if (!int.TryParse(responses["WEB_PORT_SSL"], out webPort))
			{
				webPort = int.Parse(DEFAULT_WEB_PORT_SSL);
			}
			string defaultWssPort = (webPort + 1).ToString();

			// Prompt for WSS_PORT.
			string wssPort = Prompt("Enter WSS_PORT", defaultWssPort);

			// Add WSS_PORT to responses.
			responses.Add("WSS_PORT", wssPort);

			// Build the config file content.
			string configContent =
$@"[General]
API_KEY = 3f29b9f4-4c3a-4a52-9f68-1a0f3c74eabc
FIRST_RUN = true
debug = true

[Miners]
IP_ADDR_START = {responses["IP_ADDR_START"]}
IP_ADDR_END = {responses["IP_ADDR_END"]}

[Web]
WEB_PATH = ${{currfolder}}/web
WEB_PORT_SSL = {responses["WEB_PORT_SSL"]}
WSS_PORT = {responses["WSS_PORT"]}
UPLOAD_FOLDER = ${{currfolder}}/web/upload
";

			try
			{
				File.WriteAllText(CONFIG_FILE, configContent, Encoding.UTF8);
				Log($"✔ Config file generated successfully at: {Path.GetFullPath(CONFIG_FILE)}", INFO);
			}
			catch (Exception ex)
			{
				Log($"✖ Failed to generate config file: {ex.Message}", ERR);
				Environment.Exit(1);
			}
		}

		// === SERVER LAUNCHING ===
		public static void LaunchStaticServer()
		{
			var webConfig = ConfigParser.Config?.Web;
			Action startServer = (webConfig != null && Directory.Exists(webConfig.WEB_PATH))
				? new Action(() =>
				{
					new Thread(StaticServer.StartServer)
					{
						IsBackground = true,
						Priority = ThreadPriority.Highest
					}.Start();
					Log("[INFO] StaticServer started successfully.", INFO);
				})
				: new Action(() =>
				{
					new Thread(StaticServerMinimal.StartServer)
					{
						IsBackground = true,
						Priority = ThreadPriority.Highest
					}.Start();
					Log("[INFO] Minimal StaticServer started successfully.", INFO);
				});
			startServer();
		}

		// === FIND LOOP ===
		public static void FindLoop()
		{
			Log("FindMiners Loop - Started", INFO);
			Thread.Sleep(5000);
			while (true)
			{
				FindMiners.ScanAsync().GetAwaiter().GetResult();
				Thread.Sleep(180000);
			}
		}
	}
}
