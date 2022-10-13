using System.ComponentModel.DataAnnotations;
using hass_workstation_service.Communication.InterProcesCommunication.Models;
using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class BrokerSettingsViewModel : ViewModelBase
	{
		private string clientCertPath;
		private string host;
		private bool isConnected;
		private string message;
		private string password;
		private int? port;
		private bool retainLWT = true;
		private string rootCaPath;
		private string username;
		private bool useTLS;

		public bool IsConnected {
			get => isConnected;
			set => this.RaiseAndSetIfChanged(ref isConnected, value);
		}

		public string Message {
			get => message;
			set => this.RaiseAndSetIfChanged(ref message, value);
		}

		[Required(AllowEmptyStrings = false)]
		public string Host {
			get => host;
			set => this.RaiseAndSetIfChanged(ref host, value);
		}

		public string Username {
			get => username;
			set => this.RaiseAndSetIfChanged(ref username, value);
		}

		public string Password {
			get => password;
			set => this.RaiseAndSetIfChanged(ref password, value);
		}

		[Required]
		[Range(1, 65535)]
		public int? Port {
			get => port;
			set => this.RaiseAndSetIfChanged(ref port, value);
		}

		public bool UseTLS {
			get => useTLS;
			set => this.RaiseAndSetIfChanged(ref useTLS, value);
		}


		public bool RetainLWT {
			get => retainLWT;
			set => this.RaiseAndSetIfChanged(ref retainLWT, value);
		}

		public string RootCAPath {
			get => rootCaPath;
			set => this.RaiseAndSetIfChanged(ref rootCaPath, value);
		}

		public string ClientCertPath {
			get => clientCertPath;
			set => this.RaiseAndSetIfChanged(ref clientCertPath, value);
		}


		public void Update(MqttSettings settings) {
			Host = settings.Host;
			Username = settings.Username;
			Password = settings.Password;
			Port = settings.Port;
			UseTLS = settings.UseTLS;
			RetainLWT = settings.RetainLWT;
			RootCAPath = settings.RootCAPath;
			ClientCertPath = settings.ClientCertPath;
		}

		public void UpdateStatus(MqqtClientStatus status) {
			IsConnected = status.IsConnected;
			Message = status.Message;
		}
	}
}