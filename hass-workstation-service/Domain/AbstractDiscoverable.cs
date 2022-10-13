using System;
using System.Text.RegularExpressions;
using hass_workstation_service.Communication;

namespace hass_workstation_service.Domain
{
	public abstract class AbstractDiscoverable
	{
		public abstract string Domain { get; }
		public string Name { get; protected set; }
		public string ObjectId => Regex.Replace(Name, "[^a-zA-Z0-9_-]", "_");
		public Guid Id { get; protected set; }
		public abstract DiscoveryConfigModel GetAutoDiscoveryConfig();
	}
}