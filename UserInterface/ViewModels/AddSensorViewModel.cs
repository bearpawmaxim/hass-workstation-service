using System;
using hass_workstation_service.Communication.InterProcesCommunication.Models;
using MangaReader.Avalonia.Platform.Win.Interop;
using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class AddSensorViewModel : ViewModelBase
	{
		private readonly int _screenWidth;
		private readonly int _screenHeight;
		private string _description;
		private string _moreInfoLink;
		private string _name;
		private string _query;
		private string _scope;
		private AvailableSensors _selectedType;
		private bool _showQueryInput;
		private bool _showWindowNameInput;
		private int _updateInterval;
		private string _windowName;
		private bool _showScaleFactorInput;
		private decimal _scaleFactor;

		public AddSensorViewModel() {
			_screenWidth = WinApi.GetSystemMetrics(0);
			_screenHeight = WinApi.GetSystemMetrics(1);
		}

		public AvailableSensors SelectedType {
			get => _selectedType;
			set => this.RaiseAndSetIfChanged(ref _selectedType, value);
		}

		public string Name {
			get => _name;
			set => this.RaiseAndSetIfChanged(ref _name, value);
		}

		public int UpdateInterval {
			get => _updateInterval;
			set => this.RaiseAndSetIfChanged(ref _updateInterval, value);
		}

		public string Description {
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value);
		}

		public bool ShowQueryInput {
			get => _showQueryInput;
			set => this.RaiseAndSetIfChanged(ref _showQueryInput, value);
		}

		public bool ShowWindowNameInput {
			get => _showWindowNameInput;
			set => this.RaiseAndSetIfChanged(ref _showWindowNameInput, value);
		}

		public bool ShowScaleFactorInput {
			get => _showScaleFactorInput;
			set => this.RaiseAndSetIfChanged(ref _showScaleFactorInput, value);
		}

		public string MoreInfoLink {
			get => _moreInfoLink;
			set => this.RaiseAndSetIfChanged(ref _moreInfoLink, value);
		}

		public string Query {
			get => _query;
			set => this.RaiseAndSetIfChanged(ref _query, value);
		}

		public string Scope {
			get => _scope;
			set => this.RaiseAndSetIfChanged(ref _scope, value);
		}

		public string WindowName {
			get => _windowName;
			set => this.RaiseAndSetIfChanged(ref _windowName, value);
		}

		public decimal ScaleFactor {
			get => _scaleFactor;
			set {
				this.RaiseAndSetIfChanged(ref _scaleFactor, value);
				this.RaisePropertyChanged(nameof(ScaleFactorHelp));
			}
		}

		public string ScaleFactorHelp {
			get {
				int scaledWidth = (int)(_screenWidth * ScaleFactor);
				int scaledHeight = (int)(_screenHeight * ScaleFactor);
				return
					$"Original: {_screenWidth}x{_screenHeight}{Environment.NewLine}Scaled: {scaledWidth}x{scaledHeight}";
			}
		}
	}
}