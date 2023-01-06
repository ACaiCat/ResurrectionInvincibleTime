using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace GodTime
{
	public class Config
	{
		[JsonProperty("无敌时间")]
		public int time;

        public Config Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static Config Read(string file)
		{
			if (!File.Exists(file))
			{
				WriteExample(file);
			}
			return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
		}

		public static void WriteExample(string file)
		{
			var Ex = new _Config
			{
				time=10
            };
			var Conf = new Config()
			{
				time = Ex.time
			};
			Conf.Write(file);
		}
	}

	public class _Config
	{
        [JsonProperty("无敌时间")]
        public int time;

    }
}

