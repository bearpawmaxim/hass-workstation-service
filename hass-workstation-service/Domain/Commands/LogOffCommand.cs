using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class LogOffCommand : CustomCommand
	{
		public LogOffCommand(MqttPublisher publisher, string name = "Shutdown", Guid id = default) : base(publisher,
			"shutdown /l", name ?? "LogOff", id) {
			State = "OFF";
		}
	}
}