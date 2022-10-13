using System;
using System.Diagnostics;
using hass_workstation_service.Communication;
using Serilog;

namespace hass_workstation_service.Domain.Commands
{
	public class CustomCommand : AbstractCommand
	{
		public CustomCommand(MqttPublisher publisher, string command, string name = "Custom", Guid id = default) : base(
			publisher, name ?? "Custom", id) {
			Command = command;
			State = "OFF";
		}

		public string Command { get; protected set; }
		public string State { get; protected set; }
		public Process Process { get; private set; }

		public override async void TurnOn() {
			State = "ON";
			Process = new Process();
			var startInfo = new ProcessStartInfo();
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.CreateNoWindow = true;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = $"/C {Command}";
			Process.StartInfo = startInfo;

			// turn off the sensor to guarantee disable the switch
			// useful if command changes power state of device
			State = "OFF";

			try {
				Process.Start();
			}
			catch (Exception e) {
				Log.Logger.Error($"Sensor {Name} failed", e);
				State = "FAILED";
			}
		}


		public override CommandDiscoveryConfigModel GetAutoDiscoveryConfig() {
			return new CommandDiscoveryConfigModel {
				Name = Name,
				NamePrefix = Publisher.NamePrefix,
				Unique_id = Id.ToString(),
				Availability_topic = $"homeassistant/sensor/{Publisher.DeviceConfigModel.Name}/availability",
				Command_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{Publisher.NamePrefix}{ObjectId}/set",
				State_topic =
					$"homeassistant/{Domain}/{Publisher.DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(Publisher.NamePrefix, ObjectId)}/state",
				Device = Publisher.DeviceConfigModel
			};
		}

		public override string GetState() {
			return State;
		}

		public override void TurnOff() {
			Process.Kill();
		}
	}
}