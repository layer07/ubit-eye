// StaticServerMinimal.cs
// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * StaticServerMinimal.cs
 * 
 * Version: @(#)StaticServerMinimal.cs 0.0.02 17/01/2025
 *
 * Description: Configures and runs a minimal secure HTTPS server using Kestrel solely to expose
 *              the /metrics endpoint for Prometheus metrics.
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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MinerPulse
{
	/// <summary>
	/// Minimal static server that exposes only the /metrics endpoint using Kestrel with HTTPS.
	/// </summary>
	public static class StaticServerMinimal
	{
		private const string CertFilePath = "cert.pfx";
		private const string CertPassword = "102030";
		private static IHost _host;

		public static IHostBuilder CreateHostBuilder() =>
			Host.CreateDefaultBuilder()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					ConfigureWebHost(webBuilder);
				});

		public static void ConfigureWebHost(IWebHostBuilder webBuilder)
		{
			try
			{				
				int webPortSsl = 44405;

				webBuilder.UseKestrel((context, options) =>
				{
					options.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024; // 5 GB

					if (File.Exists(CertFilePath))
					{
						var certificate = new X509Certificate2(CertFilePath, CertPassword);
						options.Listen(IPAddress.Any, webPortSsl, listenOptions =>
						{
							listenOptions.UseHttps(httpsOptions =>
							{
								httpsOptions.ServerCertificate = certificate;
								httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
							});
						});
						Console.WriteLine("[INFO] HTTPS listener configured on port {0}.", webPortSsl);
					}
					else
					{
						throw new FileNotFoundException("Certificate file not found.", CertFilePath);
					}
				});

				webBuilder.ConfigureLogging((context, logging) =>
				{
					logging.ClearProviders();
					logging.AddConsole();
					logging.AddFilter("Microsoft", LogLevel.Error);
					logging.AddFilter("System", LogLevel.Error);
				});

				webBuilder.ConfigureServices(services =>
				{
					services.AddRouting();
				});

				webBuilder.Configure((context, app) =>
				{
					app.UseRouting();

					// Prometheus export here
					app.Map("/metrics", builder =>
					{
						builder.Run(async context =>
						{
							if (context.Request.Method != HttpMethods.Get)
							{
								context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
								await context.Response.WriteAsync("Method Not Allowed");
								return;
							}

							string metrics = PrometheusMetrics.GetMetrics();
							context.Response.ContentType = "text/plain; version=0.0.4";
							await context.Response.WriteAsync(metrics);
						});
					});

					app.Map("/getwsport", builder =>
					{
						builder.Run(async context =>
						{

							int wsPort = ConfigParser.Config.Web.WSS_PORT;
							context.Response.ContentType = "text/plain";
							await context.Response.WriteAsync(wsPort.ToString());
						});
					});

					// Fallback for any unmatched routes.
					app.Run(async context =>
					{
						context.Response.StatusCode = 404;
						await context.Response.WriteAsync("404 Not Found");
					});
				});

				Console.WriteLine("[INFO] Minimal web host configured successfully.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[ERROR] Failed to configure web host: {0}", ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Starts the minimal Kestrel server.
		/// </summary>
		public static void StartServer()
		{
			try
			{
				_host = CreateHostBuilder().Build();
				Console.WriteLine("[INFO] Starting minimal Kestrel server for /metrics...");
				_host.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine("[ERROR] Failed to start server: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Stops the minimal Kestrel server gracefully.
		/// </summary>
		public static void StopServer()
		{
			try
			{
				_host?.StopAsync().GetAwaiter().GetResult();
				_host?.Dispose();
				_host = null;
				Console.WriteLine("[INFO] Minimal Kestrel server stopped successfully.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("[ERROR] Failed to stop server: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Restarts the minimal Kestrel server.
		/// </summary>
		public static void RestartServer()
		{
			Console.WriteLine("[INFO] Restarting minimal Kestrel server...");
			StopServer();
			StartServer();
		}
	}
}
