using System;
using System.Runtime.InteropServices;
using System.Text;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class ActiveWindowSensor : AbstractSensor
	{
		public ActiveWindowSensor(MqttPublisher publisher, int? updateInterval = null, string name = "ActiveWindow",
			Guid id = default) : base(publisher, name ?? "ActiveWindow", updateInterval ?? 10, id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:window-maximize",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			return GetActiveWindowTitle();
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		private string GetActiveWindowTitle() {
			const int nChars = 256;
			var Buff = new StringBuilder(nChars);
			var handle = GetForegroundWindow();

			if (GetWindowText(handle, Buff, nChars) > 0) return Buff.ToString();
			return null;
		}
	}
}