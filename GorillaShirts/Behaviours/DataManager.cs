using GorillaShirts.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private string dataLocation;

        private bool canWrite = true;

        private Dictionary<string, object> data = [];

        private JsonSerializerSettings serializeSettings, deserializeSettings;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            var vector3Converter = new Vector3Converter();
            var versionConverter = new VersionConverter();

            serializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };
            serializeSettings.Converters.Add(vector3Converter);
            serializeSettings.Converters.Add(versionConverter);

            deserializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            deserializeSettings.Converters.Add(vector3Converter);
            deserializeSettings.Converters.Add(versionConverter);

            dataLocation = Path.Combine(Application.persistentDataPath, $"{Constants.Name}.json");

            ReadData();
        }

        public void ReadData()
        {
            if (File.Exists(dataLocation))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(dataLocation), deserializeSettings) ?? [];
                }
                catch (JsonException ex)
                {
                    data = [];
                    Logging.Warning($"Could not read shirt settings: {ex.Message}");
                    try
                    {
                        // Keep the damaged file before saving defaults.
                        File.Copy(dataLocation, dataLocation + ".invalid-" + Guid.NewGuid().ToString("N"));
                    }
                    catch (Exception backupError) when (backupError is IOException || backupError is UnauthorizedAccessException)
                    {
                        canWrite = false;
                        Logging.Error($"Could not back up shirt settings; saving is disabled: {backupError.Message}");
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    canWrite = false;
                    Logging.Error($"Could not read shirt settings; saving is disabled: {ex.Message}");
                }
                return;
            }

            WriteData();
        }

        public void WriteData()
        {
            if (!canWrite) return;

            string temporaryPath = dataLocation + ".tmp";
            try
            {
                // Settings are small. Finish each save before accepting the next one.
                string output = JsonConvert.SerializeObject(data, serializeSettings);
                Directory.CreateDirectory(Path.GetDirectoryName(dataLocation));
                File.WriteAllText(temporaryPath, output);
                if (File.Exists(dataLocation))
                    File.Replace(temporaryPath, dataLocation, dataLocation + ".bak");
                else
                    File.Move(temporaryPath, dataLocation);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                Logging.Error($"Could not save shirt settings: {ex.Message}");
            }
        }

        public void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool HasItem(string key) => data.ContainsKey(key);

        public T GetItem<T>(string key, T defaultValue = default, bool setDefaultValue = true)
        {
            if (data.TryGetValue(key, out object value))
            {
                if (value is T item) return item;

                TypeCode typeCode = Type.GetTypeCode(typeof(T));

                if (typeCode != TypeCode.Int64 && value is long deserializedLong)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Int16 when deserializedLong >= short.MinValue && deserializedLong <= short.MaxValue:
                            short int16 = Convert.ToInt16(deserializedLong);
                            data[key] = int16;
                            return (T)(object)int16;
                        case TypeCode.Int32 when deserializedLong >= int.MinValue && deserializedLong <= int.MaxValue:
                            int int32 = Convert.ToInt32(deserializedLong);
                            data[key] = int32;
                            return (T)(object)int32;
                    }
                }
            }

            if (setDefaultValue) SetItem(key, defaultValue);
            return defaultValue;
        }

        public void SetItem(string key, object value)
        {
            if (data.ContainsKey(key)) data[key] = value;
            else data.Add(key, value);

            WriteData();
        }

        public void RemoveItem(string key)
        {
            if (HasItem(key))
            {
                data.Remove(key);
                WriteData();
            }
        }
    }
}
