using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class InfoViewModel : ViewModelBase
	{
		private string serviceVersion;


		public string ServiceVersion {
			get => "Service version: " + serviceVersion;
			private set => this.RaiseAndSetIfChanged(ref serviceVersion, value);
		}

		public void UpdateServiceVersion(string version) {
			ServiceVersion = version;
		}
	}
}