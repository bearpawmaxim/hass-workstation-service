using hass_workstation_service.Data;
using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class GeneralSettingsViewModel : ViewModelBase
	{
		private string namePrefix;

		public string NamePrefix {
			get => namePrefix;
			set => this.RaiseAndSetIfChanged(ref namePrefix, value);
		}

		public void Update(GeneralSettings settings) {
			NamePrefix = settings.NamePrefix;
		}
	}
}