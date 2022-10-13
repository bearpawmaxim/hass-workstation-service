using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using hass_workstation_service.Communication.InterProcesCommunication.Models;
using hass_workstation_service.Communication.NamedPipe;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using UserInterface.ViewModels;

namespace UserInterface.Views
{
	public class BrokerSettings : UserControl
	{
		private readonly IIpcClient<IServiceContractInterfaces> client;

		public BrokerSettings() {
			InitializeComponent();
			// register IPC clients
			var serviceProvider = new ServiceCollection()
				.AddNamedPipeIpcClient<IServiceContractInterfaces>("broker", "pipeinternal")
				.BuildServiceProvider();

			// resolve IPC client factory
			var clientFactory = serviceProvider
				.GetRequiredService<IIpcClientFactory<IServiceContractInterfaces>>();

			// create client
			client = clientFactory.CreateClient("broker");


			DataContext = new BrokerSettingsViewModel();
			GetSettings();
			GetStatus();
		}

		public void Ping(object sender, RoutedEventArgs args) {
			var result = client.InvokeAsync(x => x.Ping("ping")).Result;
		}

		public void Configure(object sender, RoutedEventArgs args) {
			var model = (BrokerSettingsViewModel) DataContext;
			ICollection<ValidationResult> results;
			if (model.IsValid(model, out results)) {
				var result = client.InvokeAsync(x => x.WriteMqttBrokerSettingsAsync(new MqttSettings {
					Host = model.Host, Username = model.Username, Password = model.Password ?? "", Port = model.Port,
					UseTLS = model.UseTLS, RootCAPath = model.RootCAPath, ClientCertPath = model.ClientCertPath,
					RetainLWT = model.RetainLWT
				}));
			}
		}

		public async void GetSettings() {
			var settings = await client.InvokeAsync(x => x.GetMqttBrokerSettings());
			((BrokerSettingsViewModel) DataContext).Update(settings);
		}

		public async void GetStatus() {
			while (true)
				try {
					var status = await client.InvokeAsync(x => x.GetMqqtClientStatus());

					((BrokerSettingsViewModel) DataContext).UpdateStatus(status);
					await Task.Delay(1000);
				}
				catch (Exception) {
					((BrokerSettingsViewModel) DataContext).UpdateStatus(new MqqtClientStatus
						{IsConnected = false, Message = "Unable to get connectionstatus"});
				}
		}


		private void InitializeComponent() {
			AvaloniaXamlLoader.Load(this);
		}
	}
}