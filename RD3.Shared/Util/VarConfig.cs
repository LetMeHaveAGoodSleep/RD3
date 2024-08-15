using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public  class VarConfig
    {
        static Dictionary<string, object> dictionary;

        public static object GetValue(string key)
        {
            if (dictionary == null)
            {
                string jsonContent = AESEncryption.DecryptFile(FileConst.VarPath);
                //string jsonContent = File.ReadAllText(FileConst.VarPath);
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonContent);
            }
            dictionary.TryGetValue(key, out var value);
            return value;
        }
        public void SetValue(string key, object value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else
            {
                dictionary[key] = value;
            }
            string json = JsonConvert.SerializeObject(dictionary);
            json = AESEncryption.Encrypt(json);
            File.WriteAllText(FileConst.VarPath, json);
        }    
    }
}
