using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class RestartCommand : CustomCommand
	{
		public RestartCommand(MqttPublisher publisher, string name = "Shutdown", Guid id = default) : base(publisher,
			"shutdown /r", name ?? "Restart", id) {
			State = "OFF";
		}
	}
}