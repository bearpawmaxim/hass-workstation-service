using System;
using System.Globalization;
using System.Runtime.InteropServices;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class LastBootSensor : AbstractSensor
	{
		public LastBootSensor(MqttPublisher publisher, int? updateInterval = 10, string name = "LastBoot",
			Guid id = default) : base(publisher, name ?? "LastBoot", updateInterval ?? 10, id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:clock-time-three-outline",
				Device_class = "timestamp"
			});
		}

		public override string GetState() {
			return (DateTime.Now - TimeSpan.FromMilliseconds(GetTickCount64())).ToString("o",
				CultureInfo.InvariantCulture);
		}

		[DllImport("kernel32")]
		private static extern ulong GetTickCount64();
	}
}