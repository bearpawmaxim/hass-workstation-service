using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class NextCommand : KeyCommand
	{
		public NextCommand(MqttPublisher publisher, string name = "Next", Guid id = default) : base(publisher,
			VK_MEDIA_NEXT_TRACK, name ?? "Next", id) {
		}
	}
}