using System;
using System.Globalization;
using System.Runtime.InteropServices;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Sensors
{
	public class LastActiveSensor : AbstractSensor
	{
		private DateTime _lastActive = DateTime.MinValue;

		public LastActiveSensor(MqttPublisher publisher, int? updateInterval = 10, string name = "LastActive",
			Guid id = default) : base(publisher, name ?? "LastActive", updateInterval ?? 10, id) {
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
			var lastInput = GetLastInputTime();
			if ((_lastActive - lastInput).Duration().TotalSeconds > 1) _lastActive = lastInput;
			return _lastActive.ToString("o", CultureInfo.InvariantCulture);
		}


		private static DateTime GetLastInputTime() {
			var idleTime = 0;
			var lastInputInfo = new LASTINPUTINFO();
			lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
			lastInputInfo.dwTime = 0;

			var envTicks = Environment.TickCount;

			if (GetLastInputInfo(ref lastInputInfo)) {
				var lastInputTick = Convert.ToInt32(lastInputInfo.dwTime);

				idleTime = envTicks - lastInputTick;
			}


			return idleTime > 0 ? DateTime.Now - TimeSpan.FromMilliseconds(idleTime) : DateTime.Now;
		}


		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		[StructLayout(LayoutKind.Sequential)]
		private struct LASTINPUTINFO
		{
			public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

			[MarshalAs(UnmanagedType.U4)] public int cbSize;
			[MarshalAs(UnmanagedType.U4)] public uint dwTime;
		}
	}
}