using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using hass_workstation_service.Communication.NamedPipe;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using UserInterface.Util;
using UserInterface.ViewModels;

namespace UserInterface.Views
{
	public class AppInfo : UserControl
	{
		private readonly string _basePath;
		private readonly IIpcClient<IServiceContractInterfaces> client;

		public AppInfo() {
			InitializeComponent();
			// register IPC clients
			var serviceProvider = new ServiceCollection()
				.AddNamedPipeIpcClient<IServiceContractInterfaces>("info", "pipeinternal")
				.BuildServiceProvider();

			// resolve IPC client factory
			var clientFactory = serviceProvider
				.GetRequiredService<IIpcClientFactory<IServiceContractInterfaces>>();

			// create client
			client = clientFactory.CreateClient("info");
			_basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Hass Workstation Service");


			DataContext = new InfoViewModel();
			UpdateVersion();
		}

		public async void UpdateVersion() {
			try {
				var result = await client.InvokeAsync(x => x.GetCurrentVersion());
				((InfoViewModel) DataContext).UpdateServiceVersion(result);
			}
			catch (Exception) {
			}
		}

		public void GitHub(object sender, RoutedEventArgs args) {
			BrowserUtil.OpenBrowser("https://github.com/sleevezipper/hass-workstation-service");
		}

		public void Discord(object sender, RoutedEventArgs args) {
			BrowserUtil.OpenBrowser("https://discord.gg/VraYT2N3wd");
		}

		public void OpenLogDirectory(object sender, RoutedEventArgs args) {
			var path = Path.Combine(_basePath, "logs");
			Process.Start("explorer.exe", path);
		}

		public void OpenConfigDirectory(object sender, RoutedEventArgs args) {
			Process.Start("explorer.exe", _basePath);
		}

		private void InitializeComponent() {
			AvaloniaXamlLoader.Load(this);
		}
	}
}