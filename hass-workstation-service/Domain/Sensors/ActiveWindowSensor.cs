using System;
using System.Runtime.InteropServices;
using System.Text;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class ActiveWindowSensor : AbstractSensor
	{
		public ActiveWindowSensor(MqttPublisher publisher, int? updateInterval = null,
			string name = "ActiveWindow", Guid id = default)
			: base(publisher, name ?? "ActiveWindow", updateInterval ?? 10, id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/" +
				              $"{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:window-maximize",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			return GetActiveWindowTitle();
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		private string GetActiveWindowTitle() {
			var strTitle = string.Empty;
			var handle = GetForegroundWindow();
			var intLength = GetWindowTextLength(handle) + 1;
			var stringBuilder = new StringBuilder(intLength);
			if (GetWindowText(handle, stringBuilder, intLength) > 0) strTitle = stringBuilder.ToString();

			return strTitle;
		}
	}
}