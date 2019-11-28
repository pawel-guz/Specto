using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Specto.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specto.Relay
{
    public class Reply
    {
        public static Reply ParseFrom(string json)
        {
            Reply reply = JsonConvert.DeserializeObject<Reply>(json);
            if (reply != null)
            {

                JObject json_reply = JObject.Parse(json);
                JToken json_result = json_reply["result"];

                switch (reply.Type)
                {
                    case "networkData": reply = json_result.ToObject<NetworkData>(); break;
                    case "deviceInfo": reply = json_result.ToObject<DeviceInfo>(); break;
                    case "setWiFiFeedback": reply = json_result.ToObject<SetWiFiFeedback>(); break;
                    case "setColorFeedback": reply = json_result.ToObject<SetColorFeedback>(); break;
                    default: reply = null; break;
                }
            }
            return reply;
        }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public class NetworkData : Reply
        {
            [JsonProperty(PropertyName = "ssid")]
            public string SSID { get; set; }
            [JsonProperty(PropertyName = "signalStrenght")]
            public int SignalStrenght { get; set; }
            [JsonProperty(PropertyName = "isProtected")]
            public bool IsProtected { get; set; }
        }
        public class DeviceInfo : Reply
        {
            [JsonProperty(PropertyName = "serialNumber")]
            public long SerialNumber { get; set; }
        }

        public class SetWiFiFeedback : Reply
        {
            [JsonProperty(PropertyName = "connected")]
            public bool Connected { get; set; }
        }

        public class SetColorFeedback : Reply
        {
            [JsonProperty(PropertyName = "color")]
            public string Color { get; set; }
        }
    }

    public class Command
    {
        public string ParseToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
        [JsonProperty(PropertyName = "cmd")]
        private string Name { get; set; }
        [JsonProperty(PropertyName = "params")]
        private object Params { get; set; }

        public class SetColor : Command
        {
            public SetColor(string color, bool expectReply = false)
            {
                Name = "setColor";

                if (expectReply)
                    Params = new ColorReplyParams() { Color = color, ExpectReply = expectReply };
                else
                    Params = new ColorParams() { Color = color };
            }

            public class ColorParams
            {
                [JsonProperty(PropertyName = "color")]
                public string Color { get; set; }
            }
            public class ColorReplyParams
            {
                [JsonProperty(PropertyName = "color")]
                public string Color { get; set; }

                [JsonProperty(PropertyName = "expectReply")]
                public bool ExpectReply;
            }
        }
        public class SetWiFi : Command
        {
            public SetWiFi(string ssid)
            {
                Name = "setWiFi";
                Params = new WiFiParams()
                {
                    SSID = ssid,
                    Password = ""
                };
            }

            public SetWiFi(string ssid, string password)
            {
                Name = "setWiFi";
                Params = new WiFiParams()
                {
                    SSID = ssid,
                    Password = password
                };
            }

            public SetWiFi(Network.WiFiData data)
            {
                Name = "setWiFi";
                Params = new WiFiParams()
                {
                    SSID = data.SSID,
                    Password = data.Password
                };
            }

            public class WiFiParams
            {
                [JsonProperty(PropertyName = "ssid")]
                public string SSID { get; set; }
                [JsonProperty(PropertyName = "password")]
                public string Password { get; set; }
            }
        }
        public class FetchNetworks : Command
        {
            public FetchNetworks()
            {
                Name = "fetchNetworks";
                Params = null;
            }
        }

        public class GetInfo : Command
        {
            public GetInfo()
            {
                Name = "getDeviceInfo";
                Params = null;
            }
        }

        public class Reboot : Command
        {
            public Reboot()
            {
                Name = "doReboot";
                Params = null;
            }
        }
    }
}
