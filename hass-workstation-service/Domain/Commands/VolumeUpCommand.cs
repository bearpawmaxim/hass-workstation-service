using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class VolumeUpCommand : KeyCommand
	{
		public VolumeUpCommand(MqttPublisher publisher, string name = "VolumeUp", Guid id = default) : base(publisher,
			VK_VOLUME_UP, name ?? "VolumeUp", id) {
		}
	}
}