using System;
using System.Globalization;
using System.Linq;
using hass_workstation_service.Communication;
using LibreHardwareMonitor.Hardware;

namespace hass_workstation_service.Domain.Sensors
{
	public class GpuTemperatureSensor : AbstractSensor
	{
		private readonly Computer _computer;
		private readonly IHardware _gpu;

		public GpuTemperatureSensor(MqttPublisher publisher, int? updateInterval = null, string name = "GPUTemperature",
			Guid id = default) : base(publisher, name ?? "GPUTemperature", updateInterval ?? 10, id) {
			_computer = new Computer {
				IsCpuEnabled = false,
				IsGpuEnabled = true,
				IsMemoryEnabled = false,
				IsMotherboardEnabled = false,
				IsControllerEnabled = false,
				IsNetworkEnabled = false,
				IsStorageEnabled = false
			};

			_computer.Open();
			_gpu = _computer.Hardware.FirstOrDefault(h =>
				h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia);
		}

		public override DiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Device_class = "temperature",
				Unit_of_measurement = "°C",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			if (_gpu == null) return "NotSupported";
			_gpu.Update();
			var sensor = _gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
			if (sensor == null) return "NotSupported";

			return sensor.Value.HasValue
				? sensor.Value.Value.ToString("#.##", CultureInfo.InvariantCulture)
				: "Unknown";
		}
	}
}