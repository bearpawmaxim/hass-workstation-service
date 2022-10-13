using System;
using System.Management;
using System.Runtime.Versioning;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	[SupportedOSPlatform("windows")]
	public class WMIQuerySensor : AbstractSensor
	{
		protected readonly ObjectQuery _objectQuery;
		protected readonly ManagementObjectSearcher _searcher;

		public WMIQuerySensor(MqttPublisher publisher, string query, int? updateInterval = null,
			string name = "WMIQuerySensor", Guid id = default, string scope = "") : base(publisher,
			name ?? "WMIQuerySensor", updateInterval ?? 10, id) {
			Query = query;
			Scope = scope;
			_objectQuery = new ObjectQuery(Query);
			ManagementScope managementscope;
			// if we have a custom scope, use that
			if (!string.IsNullOrWhiteSpace(scope))
				managementscope = new ManagementScope(scope);
			// otherwise, use the default
			else
				managementscope = new ManagementScope(@"\\localhost\");
			_searcher = new ManagementObjectSearcher(managementscope, _objectQuery);
		}

		public string Query { get; }
		public string Scope { get; }

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
			using (var collection = _searcher.Get()) {
				foreach (ManagementObject mo in collection)
				foreach (var property in mo.Properties)
					return property.Value.ToString();
				return "";
			}
		}
	}
}