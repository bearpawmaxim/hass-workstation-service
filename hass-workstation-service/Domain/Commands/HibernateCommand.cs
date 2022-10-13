using System;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain.Commands
{
	public class HibernateCommand : CustomCommand
	{
		public HibernateCommand(MqttPublisher publisher, string name = "Hibernate", Guid id = default) : base(publisher,
			"shutdown /h", name ?? "Hibernate", id) {
			State = "OFF";
		}
	}
}