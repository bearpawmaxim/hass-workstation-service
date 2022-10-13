using System;
using System.Runtime.InteropServices;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class KeyCommand : AbstractCommand
	{
		public const int KEYEVENTF_EXTENTEDKEY = 1;
		public const int KEYEVENTF_KEYUP = 0x0002;
		public const int VK_MEDIA_NEXT_TRACK = 0xB0;
		public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
		public const int VK_MEDIA_PREV_TRACK = 0xB1;
		public const int VK_VOLUME_MUTE = 0xAD;
		public const int VK_VOLUME_UP = 0xAF;
		public const int VK_VOLUME_DOWN = 0xAE;

		public KeyCommand(MqttPublisher publisher, byte keyCode, string name = "Key", Guid id = default) : base(
			publisher, name ?? "Key", id) {
			KeyCode = keyCode;
		}

		public byte KeyCode { get; protected set; }

		public override CommandDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return new CommandDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Availability_topic = $"homeassistant/sensor/{Publisher.DeviceConfigModel.Name}/availability",
				Command_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{Publisher.NamePrefix}{ObjectId}/set",
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Device = Publisher.DeviceConfigModel
			};
		}

		[DllImport("user32.dll")]
		public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);


		public override string GetState() {
			return "OFF";
		}

		public override void TurnOff() {
		}

		public override void TurnOn() {
			keybd_event(KeyCode, 0, 0, IntPtr.Zero);
			keybd_event(KeyCode, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
		}
	}
}