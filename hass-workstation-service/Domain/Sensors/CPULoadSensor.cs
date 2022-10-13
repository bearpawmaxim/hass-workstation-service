using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	[SupportedOSPlatform("windows")]
	public class CPULoadSensor : WMIQuerySensor
	{
		public CPULoadSensor(MqttPublisher publisher, int? updateInterval = null, string name = "CPULoadSensor",
			Guid id = default) : base(publisher,
			"SELECT PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor", updateInterval ?? 10,
			name ?? "CPULoadSensor", id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:chart-areaspline",
				Unit_of_measurement = "%",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		[SupportedOSPlatform("windows")]
		public override string GetState() {
			using (var collection = _searcher.Get()) {
				var processorLoadPercentages = new List<int>();
				foreach (ManagementObject mo in collection)
				foreach (var property in mo.Properties)
					processorLoadPercentages.Add(int.Parse(property.Value.ToString()));
				var average = processorLoadPercentages.Count > 0 ? processorLoadPercentages.Average() : 0.0;
				return average.ToString("#.##", CultureInfo.InvariantCulture);
			}
		}
	}
}