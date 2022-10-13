using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class PlayPauseCommand : KeyCommand
	{
		public PlayPauseCommand(MqttPublisher publisher, string name = "PlayPause", Guid id = default) : base(publisher,
			VK_MEDIA_PLAY_PAUSE, name ?? "PlayPause", id) {
		}
	}
}