using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using hass_workstation_service.Communication;
using Microsoft.Win32;

namespace hass_workstation_service.Domain.Sensors
{
	public class WebcamProcessSensor : AbstractSensor
	{
		private readonly HashSet<string> processes = new();

		public WebcamProcessSensor(MqttPublisher publisher, int? updateInterval = null, string name = "WebcamProcess",
			Guid id = default) : base(publisher, name ?? "WebcamProcess", updateInterval ?? 10, id) {
		}

		public override string GetState() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return IsWebCamInUseRegistry();
			return "unsupported";
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Availability_topic = $"homeassistant/sensor/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		[SupportedOSPlatform("windows")]
		private void CheckLastUsed(RegistryKey key) {
			foreach (var subKeyName in key.GetSubKeyNames())
				// NonPackaged has multiple subkeys
				if (subKeyName == "NonPackaged")
					using (var nonpackagedkey = key.OpenSubKey(subKeyName)) {
						CheckLastUsed(nonpackagedkey);
					}
				else
					using (var subKey = key.OpenSubKey(subKeyName)) {
						if (subKey.GetValueNames().Contains("LastUsedTimeStop")) {
							var endTime = subKey.GetValue("LastUsedTimeStop") is long
								? (long) subKey.GetValue("LastUsedTimeStop")
								: -1;
							if (endTime <= 0) processes.Add(subKeyName);
						}
					}
		}

		[SupportedOSPlatform("windows")]
		private string IsWebCamInUseRegistry() {
			// Clear old values
			processes.Clear();

			using (var key = Registry.LocalMachine.OpenSubKey(
				       @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam")) {
				CheckLastUsed(key);
			}

			using (var key = Registry.CurrentUser.OpenSubKey(
				       @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam")) {
				CheckLastUsed(key);
			}

			if (processes.Count() > 0) return string.Join(",", processes.ToArray());
			return "off";
		}
	}
}