using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class PreviousCommand : KeyCommand
	{
		public PreviousCommand(MqttPublisher publisher, string name = "Previous", Guid id = default) : base(publisher,
			VK_MEDIA_PREV_TRACK, name ?? "Previous", id) {
		}
	}
}