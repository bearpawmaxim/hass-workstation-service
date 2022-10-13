using System;
using System.Globalization;
using System.Management;
using System.Runtime.Versioning;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	[SupportedOSPlatform("windows")]
	public class MemoryUsageSensor : WMIQuerySensor
	{
		public MemoryUsageSensor(MqttPublisher publisher, int? updateInterval = null, string name = "MemoryUsage",
			Guid id = default) : base(publisher,
			"SELECT FreePhysicalMemory,TotalVisibleMemorySize FROM Win32_OperatingSystem", updateInterval ?? 10,
			name ?? "MemoryUsage", id) {
		}

		public override string GetState() {
			using (var collection = _searcher.Get()) {
				ulong? totalMemory = null;
				ulong? freeMemory = null;
				foreach (ManagementObject mo in collection) {
					totalMemory = (ulong) mo.Properties["TotalVisibleMemorySize"]?.Value;
					freeMemory = (ulong) mo.Properties["FreePhysicalMemory"]?.Value;
				}

				if (totalMemory != null && freeMemory != null) {
					decimal totalMemoryDec = totalMemory.Value;
					decimal freeMemoryDec = freeMemory.Value;
					var precentageUsed = 100 - freeMemoryDec / totalMemoryDec * 100;
					return precentageUsed.ToString("#.##", CultureInfo.InvariantCulture);
				}

				return "";
			}
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:memory",
				Unit_of_measurement = "%",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}
	}
}