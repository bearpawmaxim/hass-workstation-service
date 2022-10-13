using System;
using System.Threading.Tasks;
using hass_workstation_service.Communication;
using MQTTnet;

namespace hass_workstation_service.Domain.Sensors
{
	public abstract class AbstractSensor : AbstractDiscoverable
	{
		protected SensorDiscoveryConfigModel _autoDiscoveryConfigModel;

		public AbstractSensor(MqttPublisher publisher, string name, int updateInterval = 10, Guid id = default) {
			Id = id == Guid.Empty ? Guid.NewGuid() : id;
			Name = name;
			Publisher = publisher;
			UpdateInterval = updateInterval;
		}

        /// <summary>
        ///     The update interval in seconds. It checks state only if the interval has passed.
        /// </summary>
        public int UpdateInterval { get; protected set; }

		public DateTime? LastUpdated { get; protected set; }
		public string PreviousPublishedState { get; protected set; }
		public MqttPublisher Publisher { get; protected set; }
		public override string Domain => "sensor";

		public abstract string GetState();

		public async Task PublishStateAsync() {
			// dont't even check the state if the update interval hasn't passed
			if (LastUpdated.HasValue && LastUpdated.Value.AddSeconds(UpdateInterval) > DateTime.UtcNow)
				return;

			var state = GetState();
			// don't publish the state if it hasn't changed
			if (PreviousPublishedState == state)
				return;

			var message = new MqttApplicationMessageBuilder()
				.WithTopic(GetAutoDiscoveryConfig().State_topic)
				.WithPayload(state)
				//.WithExactlyOnceQoS()
				//.WithRetainFlag()
				.Build();
			await Publisher.Publish(message);
			PreviousPublishedState = state;
			LastUpdated = DateTime.UtcNow;
		}

		public async void PublishAutoDiscoveryConfigAsync() {
			await Publisher.AnnounceAutoDiscoveryConfig(this);
		}

		public async Task UnPublishAutoDiscoveryConfigAsync() {
			await Publisher.AnnounceAutoDiscoveryConfig(this, true);
			_autoDiscoveryConfigModel = null;
		}

		protected SensorDiscoveryConfigModel SetAutoDiscoveryConfigModel(SensorDiscoveryConfigModel config) {
			_autoDiscoveryConfigModel = config;
			return config;
		}
	}
}