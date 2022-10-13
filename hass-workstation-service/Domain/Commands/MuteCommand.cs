using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class MuteCommand : KeyCommand
	{
		public MuteCommand(MqttPublisher publisher, string name = "Mute", Guid id = default) : base(publisher,
			VK_VOLUME_MUTE, name ?? "Mute", id) {
		}
	}
}