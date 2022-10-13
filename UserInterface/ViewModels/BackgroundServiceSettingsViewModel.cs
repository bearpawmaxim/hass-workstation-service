using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class BackgroundServiceSettingsViewModel : ViewModelBase
	{
		private bool isAutostartEnabled;
		private bool isRunning;
		private string message;

		public bool IsAutoStartEnabled {
			get => isAutostartEnabled;
			private set => this.RaiseAndSetIfChanged(ref isAutostartEnabled, value);
		}

		public bool IsRunning {
			get => isRunning;
			private set => this.RaiseAndSetIfChanged(ref isRunning, value);
		}

		public string Message {
			get => message;
			private set => this.RaiseAndSetIfChanged(ref message, value);
		}

		public void UpdateStatus(bool isRunning, string message) {
			IsRunning = isRunning;
			Message = message;
		}

		public void UpdateAutostartStatus(bool isEnabled) {
			IsAutoStartEnabled = isEnabled;
		}
	}
}