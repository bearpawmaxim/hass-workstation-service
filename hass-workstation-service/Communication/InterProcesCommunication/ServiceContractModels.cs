using System;
using hass_workstation_service.Domain.Commands;
using hass_workstation_service.Domain.Sensors;

namespace hass_workstation_service.Communication.InterProcesCommunication.Models
{
	public class MqttSettings
	{
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int? Port { get; set; }
		public bool UseTLS { get; set; }

		public bool RetainLWT { get; set; }
		public string RootCAPath { get; set; }
		public string ClientCertPath { get; set; }
	}

	public class MqqtClientStatus
	{
		public bool IsConnected { get; set; }
		public string Message { get; set; }
	}

	public class ConfiguredSensorModel
	{
		public ConfiguredSensorModel(AbstractSensor sensor) {
			Id = sensor.Id;
			Name = sensor.Name;
			Enum.TryParse(sensor.GetType().Name, out AvailableSensors type);
			Type = type;
			Value = sensor.PreviousPublishedState;
			if (sensor is WMIQuerySensor wMIQuerySensor) {
				Query = wMIQuerySensor.Query;
				Scope = wMIQuerySensor.Scope;
			}

			if (sensor is NamedWindowSensor namedWindowSensor) WindowName = namedWindowSensor.WindowName;
			UpdateInterval = sensor.UpdateInterval;
			UnitOfMeasurement = ((SensorDiscoveryConfigModel) sensor.GetAutoDiscoveryConfig()).Unit_of_measurement;
		}

		public ConfiguredSensorModel() {
		}

		public Guid Id { get; set; }
		public AvailableSensors Type { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
		public string Query { get; set; }
		public string Scope { get; set; }
		public string WindowName { get; set; }
		public int UpdateInterval { get; set; }
		public string UnitOfMeasurement { get; set; }
	}

	public class ConfiguredCommandModel
	{
		public ConfiguredCommandModel(AbstractCommand command) {
			Id = command.Id;
			Enum.TryParse(command.GetType().Name, out AvailableCommands type);
			Type = type;
			Name = command.Name;
			if (command is CustomCommand customCommand) Command = customCommand.Command;
			if (command is KeyCommand keyCommand) Key = "0x" + Convert.ToString(keyCommand.KeyCode, 16);
		}

		public ConfiguredCommandModel() {
		}

		public Guid Id { get; set; }
		public AvailableCommands Type { get; set; }
		public string Name { get; set; }
		public string Command { get; set; }
		public string Key { get; set; }
	}

	public enum AvailableSensors
	{
		UserNotificationStateSensor,
		DummySensor,
		CurrentClockSpeedSensor,
		CPULoadSensor,
		WMIQuerySensor,
		MemoryUsageSensor,
		WebcamActiveSensor,
		WebcamProcessSensor,
		MicrophoneActiveSensor,
		MicrophoneProcessSensor,
		ActiveWindowSensor,
		NamedWindowSensor,
		LastActiveSensor,
		LastBootSensor,
		SessionStateSensor,
		CurrentVolumeSensor,
		MasterVolumeSensor,
		GPUTemperatureSensor,
		GPULoadSensor,
		ScreenshotSensor
	}

	public enum AvailableCommands
	{
		CustomCommand,
		ShutdownCommand,
		LogOffCommand,
		RestartCommand,
		HibernateCommand,
		KeyCommand,
		PlayPauseCommand,
		NextCommand,
		PreviousCommand,
		VolumeUpCommand,
		VolumeDownCommand,
		MuteCommand
	}
}