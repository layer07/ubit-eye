using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeLibChecker
{
	public static void CheckNativeLibInteractive()
	{
		Console.OutputEncoding = Encoding.UTF8;

		// If Windows, we're skip
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✔ Windows detected. Native lib check skipped.");
			Console.ResetColor();
			return;
		}
		// Unknown OS? Warn and carry on.
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("ℹ Unknown OS detected. Proceed with caution...");
			Console.ResetColor();
			return;
		}

		string baseDir = AppDomain.CurrentDomain.BaseDirectory;
		string soPath = Path.Combine(baseDir, "libe_sqlite3.so");

		if (File.Exists(soPath))
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("✔ 'libe_sqlite3.so' found. Booting up...");
			Console.ResetColor();
			return;
		}

		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("✖ 'libe_sqlite3.so' missing in:");
		Console.WriteLine("  " + baseDir);
		Console.ResetColor();
		Console.WriteLine();
		Console.WriteLine("Select an option:");
		Console.WriteLine("A) Download from official repo");
		Console.WriteLine("B) Exit and wait for file");
		Console.WriteLine("C) Run anyway (may not work)");
		Console.Write("Your choice: ");
		string choice = Console.ReadLine().Trim().ToUpperInvariant();
		
		var actions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
		{
			["A"] = () =>
			{
				string url = "https://raw.githubusercontent.com/ericsink/cb/master/bld/bin/e_sqlite3/linux/musl-x64/libe_sqlite3.so";
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("\nDownloading from: " + url);
				Console.ResetColor();
				try
				{
					var psi = new ProcessStartInfo
					{
						FileName = "wget",
						Arguments = $"-q \"{url}\" -O \"{soPath}\"",
						UseShellExecute = false,
						CreateNoWindow = true
					};
					using (var proc = Process.Start(psi))
					{
						proc.WaitForExit();
					}
					if (File.Exists(soPath))
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("✔ Download successful. Booting up...");
						Console.ResetColor();
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("✖ Download failed.");
						Console.ResetColor();
						Environment.Exit(1);
					}
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("✖ Error: " + ex.Message);
					Console.ResetColor();
					Environment.Exit(1);
				}
			},
			["B"] = () =>
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("\nExiting. Please supply 'libe_sqlite3.so' and try again.");
				Console.ResetColor();
				Environment.Exit(1);
			},
			["C"] = () =>
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("\nRunning without 'libe_sqlite3.so'. Expect instability...");
				Console.ResetColor();
			}
		};

		if (actions.TryGetValue(choice, out Action act))
			act();
		else
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("\nInvalid option. Exiting.");
			Console.ResetColor();
			Environment.Exit(1);
		}
	}
}
