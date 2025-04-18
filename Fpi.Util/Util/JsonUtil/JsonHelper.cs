using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Fpi.Util.JsonUtil
{
    /// <summary>
    /// JSON帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 将Model转换成JOSN字符串
        /// </summary>
        /// <param name="value">Model对象</param>
        /// <returns></returns>
        public static string ModelToJson(object value)
        {
            string strRet = "";
            try
            {
                strRet = JsonConvert.SerializeObject(value);
            }
            catch
            {
                strRet = "";
            }
            return strRet;
        }

        /// <summary>
        /// 将String字符串转换成MODEL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T JsonToModel<T>(string value)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(value, setting);
        }
    }
}
