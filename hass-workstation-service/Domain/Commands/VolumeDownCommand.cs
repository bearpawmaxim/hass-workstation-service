using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class VolumeDownCommand : KeyCommand
	{
		public VolumeDownCommand(MqttPublisher publisher, string name = "VolumeDown", Guid id = default) : base(
			publisher, VK_VOLUME_DOWN, name ?? "VolumeDown", id) {
		}
	}
}