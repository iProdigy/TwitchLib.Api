﻿using Newtonsoft.Json;

namespace TwitchLib.Api.Helix.Models.Channels.GetChannelInformation
{
    public class ChannelInformation
    {
        [JsonProperty(PropertyName = "broadcaster_id")]
        public string BroadcasterId { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_login")]
        public string BroadcasterLogin { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_name")]
        public string BroadcasterName { get; protected set; }
        [JsonProperty(PropertyName = "broadcaster_language")]
        public string BroadcasterLanguage { get; protected set; }
        [JsonProperty(PropertyName = "game_id")]
        public string GameId { get; protected set; }
        [JsonProperty(PropertyName = "game_name")]
        public string GameName { get; protected set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; protected set; }
        [JsonProperty(PropertyName = "delay")]
        public int Delay { get; protected set; }
    }
}
