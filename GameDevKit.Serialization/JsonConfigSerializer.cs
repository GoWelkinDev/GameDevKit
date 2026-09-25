// SPDX-License-Identifier: MPL-2.0
using GameDevKit.Serialization.Interfaces;
using System.Text.Json;

namespace GameDevKit.Serialization
{
    public sealed class JsonConfigSerializer<T> where T : class, IConfig<T>, new()
    {
        private readonly string filePath;
        private readonly T defaultValues;
        private T data;

        /// <summary>
        /// 初始化配置加载器
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <param name="defaults">默认配置实例</param>
        public JsonConfigSerializer(string filePath, T defaults = null)
        {
            this.filePath = filePath;
            defaultValues = defaults ?? new T();

            Load();
        }

        public T Config => data;

        public void Load()
        {
            if (!File.Exists(filePath))
            {
                data = CloneDefaults();
                Save();
                return;
            }

            string json = File.ReadAllText(filePath);
            T loaded;
            try
            {
                loaded = JsonSerializer.Deserialize<T>(json, T.JsonTypeInfo);
            }
            catch (JsonException ex)
            {
                // 备份损坏文件
                string backup = filePath + ".bak";
                File.Copy(filePath, backup, overwrite: true);
                throw new InvalidOperationException($"Config file '{filePath}' is malformed. A backup has been created at '{backup}'.", ex);
            }

            loaded ??= new T();

            // 补全字段
            loaded.ApplyDefaults(defaultValues);

            data = loaded;
        }

        public void Save()
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string tmp = $"{filePath}.tmp";
            string json = JsonSerializer.Serialize(data, T.JsonTypeInfo);
            File.WriteAllText(tmp, json);
            File.Move(tmp, filePath, overwrite: true);
        }

        private T CloneDefaults()
        {
            string json = JsonSerializer.Serialize(defaultValues, T.JsonTypeInfo);
            return JsonSerializer.Deserialize<T>(json, T.JsonTypeInfo)!;
        }
    }
}
