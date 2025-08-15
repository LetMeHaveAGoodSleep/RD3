using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class GraphUnitConfig
    {
        public static Dictionary<string, string> Dictionary
        {
            get;
            private set;
        }

        public static object GetValue(string key)
        {
            if (Dictionary == null)
            {
                string jsonContent = AESEncryption.DecryptFile(FileConst.GraphUnitPath);
                Dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            }
            Dictionary.TryGetValueIgnoreCase(key, out var value);
            return value;
        }
        public static void SetValue(string key, string value)
        {
            if (!Dictionary.TryAdd(key, value))
            {
                Dictionary[key] = value;
            }
            string json = JsonConvert.SerializeObject(Dictionary);

            string originalFile = FileConst.GraphUnitPath;
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
