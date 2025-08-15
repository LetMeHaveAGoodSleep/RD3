using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public  class PameterMapperConfig
    {
        static Dictionary<string, object> dictionary;

        public static object GetValue(string key)
        {
            if (dictionary == null)
            {
                string jsonContent = AESEncryption.DecryptFile(FileConst.ParameterMapperConfigPath);
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
            }
            dictionary.TryGetValueIgnoreCase(key, out var value);
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                value = key;
            }
            return value;
        }
        public static void SetValue(string key, object value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
            }
            string json = JsonConvert.SerializeObject(dictionary);

            string originalFile = FileConst.ParameterMapperConfigPath;
            string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
            // 检查文件是否存在并重命名
            if (File.Exists(originalFile))
            {
                File.WriteAllText(newFile, json);
                File.Move(newFile, originalFile, true);
                File.Delete(newFile);
            }
            else
            {
                File.WriteAllText(originalFile, json);
            }
        }    
    }
}
