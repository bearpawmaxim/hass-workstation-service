﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using hass_workstation_service.Communication;
using Serilog;

namespace hass_workstation_service.Domain.Sensors
{
	internal enum PCUserStatuses
	{
        /// <summary>
        ///     all users are locked
        /// </summary>
        Locked,

        /// <summary>
        ///     No users are logged in
        /// </summary>
        LoggedOff,

        /// <summary>
        ///     A user is using this computer
        /// </summary>
        InUse,

        /// <summary>
        ///     unable to connect to computer / other error
        /// </summary>
        Unknown
	}

	public class SessionStateSensor : AbstractSensor
	{
		public SessionStateSensor(MqttPublisher publisher, int? updateInterval = null, string name = "SessionState",
			Guid id = default) : base(publisher, name ?? "SessionState", updateInterval ?? 10, id) {
		}

		public override SensorDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return _autoDiscoveryConfigModel ?? SetAutoDiscoveryConfigModel(new SensorDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Device = Publisher.DeviceConfigModel,
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Icon = "mdi:lock",
				Availability_topic = $"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/availability"
			});
		}

		public override string GetState() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return GetPCUserStatus().ToString();
			return "unsupported";
		}


		[SupportedOSPlatform("windows")]
		private PCUserStatuses GetPCUserStatus() {
			try {
				var scope = new ManagementScope();
				scope.Connect();

				var explorerProcesses = Process.GetProcessesByName("explorer")
					.Select(p => p.Id.ToString())
					.ToHashSet();
				var REprocessid = new Regex(@"(?<=Handle="").*?(?="")", RegexOptions.Compiled);
				var numberOfLogonSessionsWithExplorer = 1;
				using (var managemntObjectSearcher =
				       new ManagementObjectSearcher(scope, new SelectQuery("SELECT * FROM Win32_SessionProcess"))) {
					numberOfLogonSessionsWithExplorer = managemntObjectSearcher.Get()
						.Cast<ManagementObject>()
						.Where(mo => explorerProcesses.Contains(REprocessid.Match(mo["Dependent"].ToString()).Value))
						.Select(mo => mo["Antecedent"].ToString())
						.Distinct()
						.Count();
				}

				var numberOfUserDesktops = 1;

				// this can fail sometimes, that's why we set numberOfUserDesktops to 1
				try {
					using (var managementObjectSearcher = new ManagementObjectSearcher(scope,
						       new SelectQuery(
							       "select * from win32_Perfrawdata_TermService_TerminalServicesSession"))) {
						numberOfUserDesktops = managementObjectSearcher.Get().Count - 1; // don't count Service desktop
					}
				}
				catch {
				}


				var numberOflogonUIProcesses = Process.GetProcessesByName("LogonUI").Length;

				if (numberOflogonUIProcesses >= numberOfUserDesktops) {
					if (numberOfLogonSessionsWithExplorer > 0)
						return PCUserStatuses.Locked;
					return PCUserStatuses.LoggedOff;
				}

				return PCUserStatuses.InUse;
			}
			catch (Exception e) {
				Log.Logger.Error(e, "Exception in SessionStateSensor");
				return PCUserStatuses.Unknown;
			}
		}
	}
}