using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class CurrentClockSpeedSensor : WMIQuerySensor
	{
		public CurrentClockSpeedSensor(MqttPublisher publisher, int? updateInterval = null,
			string name = "CurrentClockSpeed", Guid id = default) : base(publisher,
			"SELECT CurrentClockSpeed FROM Win32_Processor", updateInterval ?? 10, name ?? "CurrentClockSpeed", id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:speedometer",
				Unit_of_measurement = "MHz",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}
	}
}