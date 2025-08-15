using Newtonsoft.Json;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class CameraAlarmConfig
    {
        static Dictionary<string, string> dictionary;
        public static string GetValue(string key)
        {
            if (dictionary == null)
            {
                string jsonContent = File.ReadAllText(FileConst.CameraAlarmPath);
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            }
            dictionary.TryGetValue(key, out var value);
            return value?.ToString();
        }
        public static void SetValue(string key, string value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
            }
            string json = JsonConvert.SerializeObject(dictionary);

            string originalFile = FileConst.CameraAlarmPath;
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
