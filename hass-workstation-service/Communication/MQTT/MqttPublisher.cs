using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using hass_workstation_service.Communication.InterProcesCommunication.Models;
using hass_workstation_service.Communication.Util;
using hass_workstation_service.Data;
using hass_workstation_service.Domain;
using hass_workstation_service.Domain.Commands;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Extensions.ManagedClient;
using Serilog;

namespace hass_workstation_service.Communication
{
	public class MqttPublisher
	{
		private readonly IConfigurationService _configurationService;
		private readonly ILogger<MqttPublisher> _logger;
		private readonly IManagedMqttClient _mqttClient;

		public MqttPublisher(
			ILogger<MqttPublisher> logger,
			DeviceConfigModel deviceConfigModel,
			IConfigurationService configurationService) {
			Subscribers = new List<AbstractCommand>();
			_logger = logger;
			DeviceConfigModel = deviceConfigModel;
			_configurationService = configurationService;
			NamePrefix = configurationService.GeneralSettings?.NamePrefix;

			var options = _configurationService.GetMqttClientOptionsAsync().Result;
			_configurationService.MqqtConfigChangedHandler = ReplaceMqttClient;
			_configurationService.NamePrefixChangedHandler = UpdateNamePrefix;

			var factory = new MqttFactory();
			_mqttClient = factory.CreateManagedMqttClient();

			if (options != null) {
				_mqttClient.StartAsync(options);
				_mqttClientMessage = "Connecting...";
			}
			else {
				_mqttClientMessage = "Not configured";
			}

			_mqttClient.UseConnectedHandler(e => { _mqttClientMessage = "All good"; });
			_mqttClient.UseApplicationMessageReceivedHandler(e => HandleMessageReceived(e.ApplicationMessage));

			// configure what happens on disconnect
			_mqttClient.UseDisconnectedHandler(e => { _mqttClientMessage = e.ReasonCode.ToString(); });
		}

		private string _mqttClientMessage { get; set; }
		public DateTime LastConfigAnnounce { get; private set; }
		public DateTime LastAvailabilityAnnounce { get; private set; }
		public DeviceConfigModel DeviceConfigModel { get; }
		public ICollection<AbstractCommand> Subscribers { get; }
		public string NamePrefix { get; private set; }

		public bool IsConnected {
			get {
				if (_mqttClient == null)
					return false;
				return _mqttClient.IsConnected;
			}
		}

		public async Task Publish(MqttApplicationMessage message) {
			if (_mqttClient.IsConnected)
				await _mqttClient.PublishAsync(message);
			else
				_logger.LogInformation($"Message dropped because mqtt not connected: {message}");
		}


		public async Task AnnounceAutoDiscoveryConfig(AbstractDiscoverable discoverable, bool clearConfig = false) {
			if (_mqttClient.IsConnected) {
				var options = new JsonSerializerOptions {
					PropertyNamingPolicy = new CamelCaseJsonNamingpolicy(),
					IgnoreNullValues = true,
					PropertyNameCaseInsensitive = true
				};

				var message = new MqttApplicationMessageBuilder()
					.WithTopic(
						$"homeassistant/{discoverable.Domain}/{DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(discoverable.GetAutoDiscoveryConfig().NamePrefix, discoverable.ObjectId)}/config")
					.WithPayload(clearConfig
						? ""
						: JsonSerializer.Serialize(discoverable.GetAutoDiscoveryConfig(),
							discoverable.GetAutoDiscoveryConfig().GetType(), options))
					//.WithRetainFlag()
					.Build();
				await Publish(message);
				// if clearconfig is true, also remove previous state messages
				if (clearConfig) {
					var stateMessage = new MqttApplicationMessageBuilder()
						.WithTopic(
							$"homeassistant/{discoverable.Domain}/{DeviceConfigModel.Name}/{DiscoveryConfigModel.GetNameWithPrefix(discoverable.GetAutoDiscoveryConfig().NamePrefix, discoverable.ObjectId)}/state")
						.WithPayload("")
						// .WithRetainFlag()
						.Build();
					await Publish(stateMessage);
				}

				LastConfigAnnounce = DateTime.UtcNow;
			}
		}

		public async void ReplaceMqttClient(IManagedMqttClientOptions options) {
			_logger.LogInformation("Replacing Mqtt client with new config");
			await _mqttClient.StopAsync();
			try {
				await _mqttClient.StartAsync(options);
			}
			catch (MqttConnectingFailedException ex) {
				_mqttClientMessage = ex.ResultCode.ToString();
				Log.Logger.Error("Could not connect to broker: " + ex.ResultCode);
			}
			catch (MqttCommunicationException ex) {
				_mqttClientMessage = ex.ToString();
				Log.Logger.Error("Could not connect to broker: " + ex.Message);
			}
		}

		public MqqtClientStatus GetStatus() {
			return new MqqtClientStatus {IsConnected = _mqttClient.IsConnected, Message = _mqttClientMessage};
		}

		public async void AnnounceAvailability(string domain, bool offline = false) {
			if (_mqttClient.IsConnected) {
				await _mqttClient.PublishAsync(
					new MqttApplicationMessageBuilder()
						.WithTopic($"homeassistant/{domain}/{DeviceConfigModel.Name}/availability")
						.WithPayload(offline ? "offline" : "online")
						.Build()
				);
				LastAvailabilityAnnounce = DateTime.UtcNow;
			}
			else {
				_logger.LogInformation("Availability announce dropped because mqtt not connected");
			}
		}

		public async Task DisconnectAsync() {
			if (_mqttClient.IsConnected)
				await _mqttClient.InternalClient.DisconnectAsync();
			else
				_logger.LogInformation("Disconnected");
		}

		public async void Subscribe(AbstractCommand command) {
			if (IsConnected) {
				await _mqttClient.SubscribeAsync(((CommandDiscoveryConfigModel) command.GetAutoDiscoveryConfig())
					.Command_topic);
			}
			else {
				while (IsConnected == false) await Task.Delay(5500);

				await _mqttClient.SubscribeAsync(((CommandDiscoveryConfigModel) command.GetAutoDiscoveryConfig())
					.Command_topic);
			}

			Subscribers.Add(command);
		}

		public void UpdateNamePrefix(string prefix) {
			NamePrefix = prefix;
		}

		private void HandleMessageReceived(MqttApplicationMessage applicationMessage) {
			foreach (var command in Subscribers)
				if (((CommandDiscoveryConfigModel) command.GetAutoDiscoveryConfig()).Command_topic ==
				    applicationMessage.Topic) {
					if (Encoding.UTF8.GetString(applicationMessage?.Payload) == "ON")
						command.TurnOn();
					else if (Encoding.UTF8.GetString(applicationMessage?.Payload) == "OFF") command.TurnOff();
				}
		}
	}
}