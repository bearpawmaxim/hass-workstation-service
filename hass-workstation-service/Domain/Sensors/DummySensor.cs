using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class DummySensor : AbstractSensor
	{
		private readonly Random _random;

		public DummySensor(MqttPublisher publisher, int? updateInterval = null, string name = "Dummy",
			Guid id = default) : base(publisher, name ?? "Dummy", updateInterval ?? 1, id) {
			_random = new Random();
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			return _random.Next(0, 100).ToString();
		}
	}
}