using System.Collections.Generic;

namespace nobnak.Config {

	public class ConfigFile {
		public const char COMMENT = '#';
		public static readonly char[] SEPERATOR = new char[]{ '=' };

		private Dictionary<string, string> _map = new Dictionary<string, string>();

		public string this[string key]{
			get {
				string data;
				if (_map.TryGetValue(key, out data))
					return data;
				return null;			
			}
		}
		public bool TryGet(string key, out string data) {
			return _map.TryGetValue(key, out data);
		}

		public string Get(string key, string defaultValue) {
			string value;
			return TryGet(key, out value) ? value : defaultValue;
		}
		public int Get(string key, int defaultValue) {
			string value;
			return TryGet(key, out value) ? int.Parse(value) : defaultValue;
		}
		public float Get(string key, float defaultValue) {
			string value;
			return TryGet(key, out value) ? float.Parse(value) : defaultValue;
		}

		#if !NETFX_CORE
		public static ConfigFile Load(string path) {
			using (var reader = System.IO.File.OpenText(path)) {
				var config = new ConfigFile();

				string line;
				while ((line = reader.ReadLine()) != null) {
					if (string.IsNullOrEmpty(line))
						continue;

					if (line[0] == COMMENT)
						continue;

					var cols = line.Split(SEPERATOR);
					if (cols != null && cols.Length == 2)
						config._map.Add(cols[0], cols[1]);
				}

				return config;
			}
		}
		#endif
	}
}