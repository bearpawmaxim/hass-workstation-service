using System;
using System.Globalization;
using CoreAudio;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class MasterVolumeSensor : AbstractSensor
	{
		private readonly MMDeviceEnumerator deviceEnumerator;

		public MasterVolumeSensor(MqttPublisher publisher, int? updateInterval = null, string name = "MasterVolume",
			Guid id = default) : base(publisher, name ?? "CurrentVolume", updateInterval ?? 10, id) {
			deviceEnumerator = new MMDeviceEnumerator();
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{ObjectId}/state",
				Icon = "mdi:volume-medium",
				Unit_of_measurement = "%",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			var defaultAudioDevice = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

			// check if the volume is muted
			if (defaultAudioDevice.AudioEndpointVolume.Mute) return "0";

			return Math
				.Round(defaultAudioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100,
					0) // round volume and convert to percent
				.ToString(CultureInfo.InvariantCulture); // convert to string
		}
	}
}