using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class ShutdownCommand : CustomCommand
	{
		public ShutdownCommand(MqttPublisher publisher, string name = "Shutdown", Guid id = default) : base(publisher,
			"shutdown /s", name ?? "Shutdown", id) {
			State = "OFF";
		}
	}
}