using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CoreAudio;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class CurrentVolumeSensor : AbstractSensor
	{
		private readonly MMDeviceEnumerator deviceEnumerator;
		private readonly MMDeviceCollection devices;

		public CurrentVolumeSensor(MqttPublisher publisher, int? updateInterval = null, string name = "CurrentVolume",
			Guid id = default) : base(publisher, name ?? "CurrentVolume", updateInterval ?? 10, id) {
			deviceEnumerator = new MMDeviceEnumerator();
			devices = deviceEnumerator.EnumerateAudioEndPoints(EDataFlow.eRender, DEVICE_STATE.DEVICE_STATE_ACTIVE);
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:volume-medium",
				Unit_of_measurement = "%",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			var peaks = new List<float>();

			foreach (var device in devices) peaks.Add(device.AudioMeterInformation.PeakValues[0]);

			return Math.Round(peaks.Max() * 100, 0).ToString(CultureInfo.InvariantCulture);
		}
	}
}