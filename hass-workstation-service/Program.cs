using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using hass_workstation_service.Communication;
using hass_workstation_service.Communication.InterProcesCommunication;
using hass_workstation_service.Communication.NamedPipe;
using hass_workstation_service.Data;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace hass_workstation_service
{
	public class Program
	{
		public static async Task Main(string[] args) {
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Hass Workstation Service", "logs");
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.WriteTo.File(new RenderedCompactJsonFormatter(), Path.Combine(path, "log.txt"),
					rollingInterval: RollingInterval.Day)
				.CreateLogger();
			// We do it this way because there is currently no way to pass an argument to a dotnet core app when using clickonce
			if (Process.GetProcessesByName("hass-workstation-service").Count() > 1) //bg service running
			{
#if !DEBUG
                StartUI();
#endif
			}
			else {
				try {
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						await CreateHostBuilder(args).RunConsoleAsync();
					else
						// we only support MS Windows for now
						throw new NotImplementedException("Your platform is not yet supported");
				}
				catch (Exception ex) {
					Log.Fatal(ex, "Application start-up failed");
				}
				finally {
					Log.CloseAndFlush();
				}
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) {
			return Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostContext, loggingBuilder) =>
					loggingBuilder.AddSerilog(dispose: true))
				.ConfigureServices((hostContext, services) => {
					var deviceConfig = new DeviceConfigModel {
						Name = Environment.MachineName,
						Identifiers = "hass-workstation-service" + Environment.MachineName,
						Manufacturer = Environment.UserName,
						Model = Environment.OSVersion.ToString(),
						Sw_version = GetVersion()
					};
					services.AddSingleton(deviceConfig);
					services.AddSingleton<IServiceContractInterfaces, InterProcessApi>();
					services.AddSingleton<IConfigurationService, ConfigurationService>();
					services.AddSingleton<MqttPublisher>();
					services.AddHostedService<Worker>();
				}).ConfigureIpcHost(builder => {
					// configure IPC endpoints
					builder.AddNamedPipeEndpoint<IServiceContractInterfaces>("pipeinternal");
				});
		}

		internal static string GetVersion() {
			if (!Debugger.IsAttached) return Assembly.GetExecutingAssembly().GetName().Version.ToString();

			return "Debug";
		}

		public static void StartUI() {
			Log.Logger.Information(Environment.CurrentDirectory + "\\UserInterface.exe");
			Process.Start(Environment.CurrentDirectory + "\\UserInterface.exe");
		}
	}
}