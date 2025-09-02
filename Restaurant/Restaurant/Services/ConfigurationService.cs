using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant.Services
{
    public class ConfigurationService
    {
        private Dictionary<string, string> _configCache;

        public ConfigurationService()
        {
            _configCache = new Dictionary<string, string>();
        }

        public async Task LoadConfigurationsAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync("appsettings.json");
                var root = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (!root.TryGetValue("OrderSettings", out var settings))
                    throw new Exception("Secțiunea 'OrderSettings' nu a fost găsită în appsetting.json");

                var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(settings.GetRawText());

                _configCache.Clear();
                foreach (var entry in raw)
                    _configCache[entry.Key] = entry.Value.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Eroare la încărcarea fișierului appsetting.json: " + ex.Message);
            }
        }


        public string GetValue(string key)
        {
            return _configCache.TryGetValue(key, out var value) ? value : null;
        }

        public decimal GetDecimal(string key, decimal defaultValue = 0m)
        {
            if (_configCache.TryGetValue(key, out var val))
                if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    return result;
            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_configCache.TryGetValue(key, out var val))
                if (int.TryParse(val, out var result))
                    return result;
            return defaultValue;
        }
    }
}
