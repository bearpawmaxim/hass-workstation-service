using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using hass_workstation_service.Communication.NamedPipe;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using UserInterface.ViewModels;

namespace UserInterface.Views
{
	public class BackgroundServiceSettings : UserControl
	{
		private readonly IIpcClient<IServiceContractInterfaces> _client;

		public BackgroundServiceSettings() {
			InitializeComponent();
			// register IPC clients
			var serviceProvider = new ServiceCollection()
				.AddNamedPipeIpcClient<IServiceContractInterfaces>("broker", "pipeinternal")
				.BuildServiceProvider();

			// resolve IPC client factory
			var clientFactory = serviceProvider
				.GetRequiredService<IIpcClientFactory<IServiceContractInterfaces>>();

			// create client
			_client = clientFactory.CreateClient("broker");

			DataContext = new BackgroundServiceSettingsViewModel();
			Ping();
		}

		public async void Ping() {
			while (true) {
				if (DataContext is not BackgroundServiceSettingsViewModel viewModel)
					throw new Exception("Wrong viewmodel class!");

				try {
					var result = await _client.InvokeAsync(x => x.Ping("ping"));

					if (result == "pong")
						viewModel.UpdateStatus(true, "All good");
					else
						viewModel.UpdateStatus(false, "Not running");
				}
				catch (Exception) {
					viewModel.UpdateStatus(false, "Not running");
				}

				var autostartresult = await _client.InvokeAsync(x => x.IsAutoStartEnabled());
				viewModel.UpdateAutostartStatus(autostartresult);

				await Task.Delay(1000);
			}
		}

		public void Start(object sender, RoutedEventArgs args) {
			//TODO: fix the path. This will depend on the deployment structure.
			Process.Start("hass-worstation-service.exe");
		}

		public void EnableAutostart(object sender, RoutedEventArgs args) {
			_client.InvokeAsync(x => x.EnableAutostart(true));
		}

		public void DisableAutostart(object sender, RoutedEventArgs args) {
			_client.InvokeAsync(x => x.EnableAutostart(false));
		}

		private void InitializeComponent() {
			AvaloniaXamlLoader.Load(this);
		}
	}
}