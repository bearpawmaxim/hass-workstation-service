using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using hass_workstation_service.Communication.NamedPipe;
using hass_workstation_service.Data;
using JKang.IpcServiceFramework.Client;
using Microsoft.Extensions.DependencyInjection;
using UserInterface.ViewModels;

namespace UserInterface.Views
{
	public class GeneralSettingsView : UserControl
	{
		private readonly IIpcClient<IServiceContractInterfaces> client;

		public GeneralSettingsView() {
			InitializeComponent();
			// register IPC clients
			var serviceProvider = new ServiceCollection()
				.AddNamedPipeIpcClient<IServiceContractInterfaces>("general", "pipeinternal")
				.BuildServiceProvider();

			// resolve IPC client factory
			var clientFactory = serviceProvider
				.GetRequiredService<IIpcClientFactory<IServiceContractInterfaces>>();

			// create client
			client = clientFactory.CreateClient("general");

			DataContext = new GeneralSettingsViewModel();
			GetSettings();
		}

		public void Configure(object sender, RoutedEventArgs args) {
			var model = (GeneralSettingsViewModel) DataContext;
			ICollection<ValidationResult> results;
			if (model.IsValid(model, out results)) {
				var result = client.InvokeAsync(x =>
					x.WriteGeneralSettings(new GeneralSettings { NamePrefix = model.NamePrefix }));
			}
		}

		public async void GetSettings() {
			var settings = await client.InvokeAsync(x => x.GetGeneralSettings());
			((GeneralSettingsViewModel) DataContext).Update(settings);
		}


		private void InitializeComponent() {
			AvaloniaXamlLoader.Load(this);
		}
	}
}