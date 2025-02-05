// StaticServer.cs
// SPDX-License-Identifier: GPL-2.0-or-later
/*
 * StaticServer.cs
 * 
 * Version: @(#)StaticServer.cs 0.0.02 17/01/2025
 *
 * Description: Configures and runs a secure static file server using Kestrel with HTTPS and Prometheus metrics.
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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MinerPulse
{
	public static class StaticServer
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
				var webConfig = ConfigParser.Config.Web;
				string webPath = webConfig.WEB_PATH;
				int webPortSsl = webConfig.WEB_PORT_SSL;
				string uploadFolder = webConfig.UPLOAD_FOLDER;

				// Configuration display with ASCII colors
				Console.WriteLine($"\u001b[36m[CONFIG]\u001b[0m HTTPS Port: {webPortSsl}");
				Console.WriteLine($"\u001b[36m[CONFIG]\u001b[0m Web Path: {webPath}");
				Console.WriteLine($"\u001b[36m[CONFIG]\u001b[0m Upload Folder: {uploadFolder}");

				webBuilder.UseKestrel((context, options) =>
				{
					options.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024;

					if (File.Exists(CertFilePath))
					{
						var certificate = new X509Certificate2(CertFilePath, CertPassword);
						options.Listen(IPAddress.Any, webPortSsl, listenOptions =>
						{
							listenOptions.UseHttps(httpsOptions =>
							{
								httpsOptions.ServerCertificate = certificate;
								httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
							});
						});
						Console.WriteLine("\u001b[32;1m[CRYPTO]\u001b[0m TLS v1.2/1.3 handler initialized");
					}
					else
					{
						throw new FileNotFoundException("Certificate file not found.", CertFilePath);
					}
				});

				webBuilder.UseContentRoot(webPath);
				Console.WriteLine($"\u001b[36m[PATH]\u001b[0m Content root: \u001b[33m{webPath}\u001b[0m");

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
					services.AddControllers();
					services.Configure<FormOptions>(options =>
					{
						options.MultipartBodyLengthLimit = 1000L * 1024 * 1024;
					});
				});

				webBuilder.Configure((context, app) =>
				{
					var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
					var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

					if (env.IsDevelopment())
					{
						app.UseDeveloperExceptionPage();
					}

					// ASCII-art header for static files section
					Console.WriteLine("\u001b[35m[■]\u001b[0m Initializing static content delivery system");
					app.UseStaticFiles();
					Console.WriteLine($"\u001b[32;1m[STATIC]\u001b[0m Serving: \u001b[33m{webPath}\u001b[0m");

					if (Directory.Exists(uploadFolder))
					{
						app.UseStaticFiles(new StaticFileOptions
						{
							FileProvider = new PhysicalFileProvider(uploadFolder),
							RequestPath = "/uploads",
							ServeUnknownFileTypes = true,
							DefaultContentType = "application/octet-stream"
						});
						Console.WriteLine($"\u001b[32;1m[UPLOAD]\u001b[0m Mounted: \u001b[33m{uploadFolder}\u001b[0m → \u001b[36m/uploads\u001b[0m");
					}
					else
					{
						logger.LogWarning($"Upload folder not found: {uploadFolder}. Uploads will not be served.");
					}

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

					app.UseRouting();
					app.UseEndpoints(endpoints => endpoints.MapControllers());

					app.Run(async (context) =>
					{
						string filePath = context.Request.Path == "/" ? "index.html" : context.Request.Path.Value.TrimStart('/');
						string fileFullPath = Path.Combine(webPath, filePath);

						if (File.Exists(fileFullPath))
						{
							var provider = new FileExtensionContentTypeProvider();
							if (!provider.TryGetContentType(fileFullPath, out var contentType))
							{
								contentType = "application/octet-stream";
							}

							context.Response.ContentType = contentType;
							context.Response.Headers.ContentLength = new FileInfo(fileFullPath).Length;

							using var fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
							await fileStream.CopyToAsync(context.Response.Body);
						}
						else
						{
							context.Response.StatusCode = 404;
							await context.Response.WriteAsync("404 Not Found");
						}
					});
				});

				Console.WriteLine("\u001b[32;1m[READY]\u001b[0m Web host configuration complete");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\u001b[31;1m[FAIL]\u001b[0m Critical configuration error: \u001b[33m{ex.Message}\u001b[0m");
				throw;
			}
		}

		public static void StartServer()
		{
			try
			{
				Console.WriteLine("\u001b[36m");
				Console.WriteLine(@"
███████ ███████ ██████  ██    ██ ███████ ██████      ██████   █████  ███████ ███████ 
██      ██      ██   ██ ██    ██ ██      ██   ██     ██   ██ ██   ██ ██      ██      
███████ █████   ██████  ██    ██ █████   ██████      ██████  ███████ ███████ ███████ 
     ██ ██      ██   ██  ██  ██  ██      ██   ██     ██      ██   ██      ██      ██ 
███████ ███████ ██   ██   ████   ███████ ██   ██     ██      ██   ██ ███████ ███████ 				


				");
				Console.WriteLine("\u001b[0m");
				Console.WriteLine("\u001b[35;1m[■]\u001b[0m Initializing secure server environment...");

				_host = CreateHostBuilder().Build();
				Console.WriteLine("\u001b[32;1m[BOOT]\u001b[0m Starting kestrel core...");
				_host.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\u001b[31;1m[PANIC]\u001b[0m Failed to initialize: \u001b[33m{ex.Message}\u001b[0m");
			}
		}

		public static void StopServer()
		{
			try
			{
				_host?.StopAsync().GetAwaiter().GetResult();
				_host?.Dispose();
				_host = null;
				Console.WriteLine("\u001b[32;1m[SHUTDOWN]\u001b[0m Server termination sequence complete");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\u001b[31;1m[ERROR]\u001b[0m Termination failure: \u001b[33m{ex.Message}\u001b[0m");
			}
		}

		public static void KestrelRestart()
		{
			Console.WriteLine("\u001b[33;1m[RELOAD]\u001b[0m Initiating hot restart sequence...");
			StopServer();
			StartServer();
		}
	}
}


		/*
         *	Make sure that cert.pfx is in the same folder as the binary.
         *	Run the WebServer:
         * 	new Thread(StaticServer.StartServer) { IsBackground = true, Priority = ThreadPriority.Highest }.Start();
         * 	Works!
         */
