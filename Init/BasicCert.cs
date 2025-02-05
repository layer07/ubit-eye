using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerPulse
{
	public static class BasicCert
	{
		public static string CertString = "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";
	}

	public static class CertHelper
	{
		public static void EnsureCertificate()
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;
			string certPath = Path.Combine(baseDir, "cert.pfx");

			if (!File.Exists(certPath))
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("✖ cert.pfx not found. Writing certificate from embedded Base64 data...");
				Console.ResetColor();

				try
				{
					byte[] certBytes = Convert.FromBase64String(BasicCert.CertString);
					File.WriteAllBytes(certPath, certBytes);

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("✔ Certificate successfully written to: " + certPath);
					Console.ResetColor();
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("✖ Failed to write certificate: " + ex.Message);
					Console.ResetColor();
					Environment.Exit(1);
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("✔ cert.pfx already exists in: " + certPath);
				Console.ResetColor();
			}
		}
	}
}
