namespace Fpi.Util.Serializes.CustomSerializer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Drawing;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Reflection;

    /// <summary>
    /// Xml设置类
    /// </summary>
    public class XmlSettings : XmlDataObject
    {
        /// <summary>
        /// 数据存储路径
        /// </summary>
        private string pth;

        /// <summary>
        /// 字符字典
        /// </summary>
        private Dictionary<string, string> stringDic = new Dictionary<string, string>();

        /// <summary>
        /// Int32数据字典
        /// </summary>
        private Dictionary<string, int> int32Dic = new Dictionary<string, int>();

        /// <summary>
        /// Int64数据字典
        /// </summary>
        private Dictionary<string, long> int64Dic = new Dictionary<string, long>();

        /// <summary>
        /// Byte数据字典
        /// </summary>
        private Dictionary<string, byte> byteDic = new Dictionary<string, byte>();

        /// <summary>
        /// Single数据字典
        /// </summary>
        private Dictionary<string, float> floatDic = new Dictionary<string, float>();

        /// <summary>
        /// Double数据字典
        /// </summary>
        private Dictionary<string, double> doubleDic = new Dictionary<string, double>();

        private Dictionary<string, Guid> guidDic = new Dictionary<string, Guid>();

        /// <summary>
        /// Boolean型数据字典
        /// </summary>
        private Dictionary<string, bool> boolDic = new Dictionary<string, bool>();

        private Dictionary<string, Point> pointDic = new Dictionary<string, Point>();

        private Dictionary<string, PointF> pointFDic = new Dictionary<string, PointF>();

        private Dictionary<string, Color> colorDic = new Dictionary<string, Color>();

        private Dictionary<string, RectangleF> rectFDic = new Dictionary<string, RectangleF>();

        /// <summary>
        /// 矩形字典
        /// </summary>
        private Dictionary<string, Rectangle> rectDic = new Dictionary<string, Rectangle>();

        private Dictionary<string, object> otherDic = new Dictionary<string, object>();

        private Dictionary<string, Size> sizeDic = new Dictionary<string, Size>();

        private Dictionary<string, SizeF> sizeFDic = new Dictionary<string, SizeF>();

        private Dictionary<string, DateTime> timeDic = new Dictionary<string, DateTime>();      

        private Dictionary<Type, ISerializationSurrogate> serializers = new Dictionary<Type, ISerializationSurrogate>();
        /// <summary>
        /// 列表字典
        /// </summary>
        private Dictionary<string, IList> listDic = new Dictionary<string, IList>();     
 
        public XmlSettings()
            : base()
        {
            this.RootName = "Items";
            this.AddSerializer(typeof(object[]), new ArraySerializer());
            this.AddSerializer(typeof(int[]), new Int32ArraySerializer());
            this.AddSerializer(typeof(Bitmap), new ImageSerializer());
            this.AddSerializer(typeof(byte[]), new ByteArraySerializer());
            this.AddSerializer(typeof(long[]), new ArraySerializer<long>());
            this.AddSerializer(typeof(double[]), new ArraySerializer<double>());
            this.AddSerializer(typeof(string[]), new ArraySerializer<string>());
            this.AddSerializer(typeof(float[]), new ArraySerializer<float>());
            this.AddSerializer(typeof(DateTime[]), new ArraySerializer<DateTime>());
            this.AddSerializer(typeof(bool[]), new ArraySerializer<bool>());
            this.AddSerializer(typeof(Color), new ColorSerializer());
            this.AddSerializer(typeof(Font), new FontSerializer());
            this.AddSerializer(typeof(Enum), new EnumSerializer());
            this.AddSerializer(typeof(StringFormat), new StringFormatSerializer());

        }

        /// <summary>
        /// 获取或设置存储路径
        /// </summary>
        public string Path
        {
            get { return this.pth; }

            set { this.pth = value; }
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>指定键对应的字符串</returns>
        public string GetString(string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            string lKey = key.ToLower();
            if (stringDic.ContainsKey(lKey))
            {
                return stringDic[lKey];
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的字符串</returns>
        public string GetString(string key)
        {
            return this.GetString(key, string.Empty);
        }

        /// <summary>
        /// 获取Int32数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Int32数据</returns>
        public int? GetInt32(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (int32Dic.ContainsKey(lKey))
            {
                return int32Dic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Int32数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Int32数据</returns>
        public Guid? GetGuid(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.guidDic.ContainsKey(lKey))
            {
                return this.guidDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Int64数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Int64数据</returns>
        public long? GetInt64(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (int64Dic.ContainsKey(lKey))
            {
                return int64Dic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Byte数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Byte数据</returns>
        public byte? GetByte(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (byteDic.ContainsKey(lKey))
            {
                return byteDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Single数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Single数据</returns>
        public float? GetSingle(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (floatDic.ContainsKey(lKey))
            {
                return floatDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Double数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Double数据</returns>
        public double? GetDouble(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (doubleDic.ContainsKey(lKey))
            {
                return doubleDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Boolean数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Boolean数据</returns>
        public bool? GetBoolean(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.boolDic.ContainsKey(lKey))
            {
                return this.boolDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取PointF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的PointF数据</returns>
        public PointF? GetPointF(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.pointFDic.ContainsKey(lKey))
            {
                return this.pointFDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Point数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Point数据</returns>
        public Point? GetPoint(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.pointDic.ContainsKey(lKey))
            {
                return this.pointDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取RectangleF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的RectangleF数据</returns>
        public RectangleF? GetRectangleF(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.rectFDic.ContainsKey(lKey))
            {
                return this.rectFDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Color数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Color数据</returns>
        public Color? GetColor(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.colorDic.ContainsKey(lKey))
            {
                return this.colorDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Rectangle数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Rectangle数据</returns>
        public Rectangle? GetRectangle(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.rectDic.ContainsKey(lKey))
            {
                return this.rectDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取Size数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Size数据</returns>
        public Size? GetSize(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.sizeDic.ContainsKey(lKey))
            {
                return this.sizeDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取SizeF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的SizeF数据</returns>
        public SizeF? GetSizeF(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.sizeFDic.ContainsKey(lKey))
            {
                return this.sizeFDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 获取DateTime数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的DateTime数据</returns>
        public DateTime? GetTime(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.timeDic.ContainsKey(lKey))
            {
                return this.timeDic[lKey];
            }

            return null;
        }


        /// <summary>
        /// 获取Ilist数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>指定键对应的Ilist数据</returns>
        public IList GetList(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string lKey = key.ToLower();
            if (this.listDic.ContainsKey(lKey))
            {
                return this.listDic[lKey];
            }

            return null;
        }

        /// <summary>
        /// 设置字符值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (stringDic.ContainsKey(lKey))
            {
                stringDic[lKey] = value;
            }
            else
            {
                stringDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Int32数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (int32Dic.ContainsKey(lKey))
            {
                int32Dic[lKey] = value;
            }
            else
            {
                int32Dic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Int64数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, long value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (int64Dic.ContainsKey(lKey))
            {
                int64Dic[lKey] = value;
            }
            else
            {
                int64Dic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Byte数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, byte value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (byteDic.ContainsKey(lKey))
            {
                byteDic[lKey] = value;
            }
            else
            {
                byteDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Single数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (floatDic.ContainsKey(lKey))
            {
                floatDic[lKey] = value;
            }
            else
            {
                floatDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Double数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, double value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (doubleDic.ContainsKey(lKey))
            {
                doubleDic[lKey] = value;
            }
            else
            {
                doubleDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Int32数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.boolDic.ContainsKey(lKey))
            {
                this.boolDic[lKey] = value;
            }
            else
            {
                this.boolDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Rectangle数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, Rectangle value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.rectDic.ContainsKey(lKey))
            {
                this.rectDic[lKey] = value;
            }
            else
            {
                this.rectDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置RectangleF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, RectangleF value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.rectFDic.ContainsKey(lKey))
            {
                this.rectFDic[lKey] = value;
            }
            else
            {
                this.rectFDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置PointF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, PointF value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.pointFDic.ContainsKey(lKey))
            {
                this.pointFDic[lKey] = value;
            }
            else
            {
                this.pointFDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Guid数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, Guid value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.guidDic.ContainsKey(lKey))
            {
                this.guidDic[lKey] = value;
            }
            else
            {
                this.guidDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Point数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, Point value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.pointDic.ContainsKey(lKey))
            {
                this.pointDic[lKey] = value;
            }
            else
            {
                this.pointDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Color数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, Color value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.colorDic.ContainsKey(lKey))
            {
                this.colorDic[lKey] = value;
            }
            else
            {
                this.colorDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置SizeF数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, SizeF value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.sizeFDic.ContainsKey(lKey))
            {
                this.sizeFDic[lKey] = value;
            }
            else
            {
                this.sizeFDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置Size数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, Size value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.sizeDic.ContainsKey(lKey))
            {
                this.sizeDic[lKey] = value;
            }
            else
            {
                this.sizeDic.Add(lKey, value);
            }
        }

        /// <summary>
        /// 设置DateTime数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, DateTime value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.timeDic.ContainsKey(lKey))
            {
                this.timeDic[lKey] = value;
            }
            else
            {
                this.timeDic.Add(lKey, value);
            }
        }
        /// <summary>
        /// 设置IList数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetValue(string key, IList value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string lKey = key.ToLower();
            if (this.listDic.ContainsKey(lKey))
            {
                this.listDic[lKey] = value;
            }
            else
            {
                this.listDic.Add(lKey, value);
            }
        }
        public void SetValue(string key, object value)
        {
            if (this.otherDic.ContainsKey(key))
            {
                this.otherDic[key] = value;
            }
            else
            {
                this.otherDic.Add(key, value);
            }            
        }

        public T[] GetValuesOfType<T>()
        {
            List<T> result = new List<T>();
            foreach (object item in this.otherDic.Values)
            {
                if (item is T)
                {
                    result.Add((T)item);
                }
            }

            return result.ToArray();
        }

        public void AddSerializer(Type type, ISerializationSurrogate serializer)
        {
            if (this.serializers.ContainsKey(type))
            {
                this.serializers[type] = serializer;
            }
            else
            {
                this.serializers.Add(type, serializer);
            }
        }

        public ISerializationSurrogate GetSerilizer(Type type)
        {
            if (this.serializers.ContainsKey(type))
            {
                return this.serializers[type];
            }

            foreach (KeyValuePair<Type,ISerializationSurrogate> kv in this.serializers)
            {
                if (type.IsSubclassOf(kv.Key))
                {
                    return kv.Value;
                }
            }

            return null;
        }

        public override bool Load(string path)
        {
            this.pth = path;
            return base.Load(path);
        }

        public override bool Load(System.IO.Stream input)
        {
            this.Clear();
            return base.Load(input);
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public virtual void Clear()
        {
            this.stringDic.Clear();
            this.int32Dic.Clear();
            this.boolDic.Clear();
            this.int64Dic.Clear();
            this.rectDic.Clear();
            this.otherDic.Clear();
            this.byteDic.Clear();
            this.floatDic.Clear();
            this.doubleDic.Clear();
            this.pointFDic.Clear();
            this.pointDic.Clear();
            this.colorDic.Clear();
            this.rectFDic.Clear();
            this.timeDic.Clear();
            this.sizeDic.Clear();
            this.sizeFDic.Clear();
            this.guidDic.Clear();
        }

        public object GetValue(string name)
        {
            if (this.otherDic.ContainsKey(name))
            {
                return this.otherDic[name];
            }

            return null;
        }

        public virtual void FillSerializeInfo(SerializationInfo info)
        {
            foreach (KeyValuePair<string, int> kv in this.int32Dic)
            {
                info.AddValue(kv.Key, (int)kv.Value);
            }

            foreach (KeyValuePair<string, long> kv in this.int64Dic)
            {
                info.AddValue(kv.Key, (long)kv.Value);
            }

            foreach (KeyValuePair<string, byte> kv in this.byteDic)
            {
                info.AddValue(kv.Key, (byte)kv.Value);
            }

            foreach (KeyValuePair<string, float> kv in this.floatDic)
            {
                info.AddValue(kv.Key, (float)kv.Value);
            }

            foreach (KeyValuePair<string, double> kv in this.doubleDic)
            {
                info.AddValue(kv.Key, (double)kv.Value);
            }

            foreach (KeyValuePair<string, bool> kv in this.boolDic)
            {
                info.AddValue(kv.Key, (bool)kv.Value);
            }

            foreach (KeyValuePair<string, Guid> kv in this.guidDic)
            {
                info.AddValue(kv.Key, (Guid)kv.Value);
            }

            foreach (KeyValuePair<string, string> kv in this.stringDic)
            {
                info.AddValue(kv.Key, (string)kv.Value);
            }

            foreach (KeyValuePair<string, Rectangle> kv in this.rectDic)
            {
                info.AddValue(kv.Key, (Rectangle)kv.Value);
            }

            foreach (KeyValuePair<string, RectangleF> kv in this.rectFDic)
            {
                info.AddValue(kv.Key, (RectangleF)kv.Value);
            }

            foreach (KeyValuePair<string, Point> kv in this.pointDic)
            {
                info.AddValue(kv.Key, (Point)kv.Value);
            }

            foreach (KeyValuePair<string, PointF> kv in this.pointFDic)
            {
                info.AddValue(kv.Key, (PointF)kv.Value);
            }

            foreach (KeyValuePair<string, Size> kv in this.sizeDic)
            {
                info.AddValue(kv.Key, (Size)kv.Value);
            }

            foreach (KeyValuePair<string, SizeF> kv in this.sizeFDic)
            {
                info.AddValue(kv.Key, (SizeF)kv.Value);
            }

            foreach (KeyValuePair<string, DateTime> kv in this.timeDic)
            {
                info.AddValue(kv.Key, (DateTime)kv.Value);
            }

            foreach (KeyValuePair<string, Color> kv in this.colorDic)
            {
                info.AddValue(kv.Key, (Color)kv.Value);
            }
            foreach (KeyValuePair<string, IList> kv in this.listDic)
            {
                info.AddValue(kv.Key, (IList)kv.Value);
            }

            foreach (KeyValuePair<string, object> kv in this.otherDic)
            {
                info.AddValue(kv.Key, kv.Value);
            }
        }


        public override bool ReadMember(System.Xml.XPath.XPathNavigator subNode)
        {
            if (!base.ReadMember(subNode))
            {
                string nodeName = subNode.Name;
                if (string.Equals(nodeName, "Item", StringComparison.OrdinalIgnoreCase) || string.Equals(nodeName, "Items", StringComparison.OrdinalIgnoreCase))
                {
                    string type = subNode.GetAttribute("type", string.Empty);
                    string key = subNode.GetAttribute("key", string.Empty);
                    if ((!string.IsNullOrEmpty(key)) && (!string.IsNullOrEmpty(type)))
                    {
                        string ltype = type.ToLower();

                       
                            switch (ltype)
                            {
                                case "string":
                                    {
                                        this.SetValue(key, subNode.Value);
                                        break;
                                    }

                                case "int32":
                                    {
                                        this.SetValue(key, subNode.ValueAsInt);
                                        break;
                                    }

                                case "int64":
                                    {
                                        this.SetValue(key, subNode.ValueAsLong);
                                        break;
                                    }

                                case "byte":
                                    {
                                        this.SetValue(key, (byte)subNode.ValueAsInt);
                                        break;
                                    }

                                case "single":
                                    {
                                        this.SetValue(key, (float)subNode.ValueAsDouble);
                                        break;
                                    }

                                case "guid":
                                    {
                                        try
                                        {
                                            Guid gid = new Guid(subNode.Value);
                                            this.SetValue(key, gid);
                                        }
                                        finally
                                        {
                                        }

                                        break;
                                    }

                                case "double":
                                    {
                                        this.SetValue(key, subNode.ValueAsDouble);
                                        break;
                                    }

                                case "boolean":
                                    {
                                        this.SetValue(key, subNode.ValueAsBoolean);
                                        break;
                                    }

                                case "point":
                                    {
                                        string xStr = subNode.GetAttribute("x", string.Empty);
                                        string yStr = subNode.GetAttribute("y", string.Empty);
                                        int x, y;
                                        if (int.TryParse(xStr, out x) && int.TryParse(yStr, out y))
                                        {
                                            this.SetValue(key, new Point(x, y));
                                            return true;
                                        }

                                        break;
                                    }

                                case "size":
                                    {
                                        string wStr = subNode.GetAttribute("width", string.Empty);
                                        string hStr = subNode.GetAttribute("height", string.Empty);
                                        int width, height;
                                        if (int.TryParse(wStr, out width) && int.TryParse(hStr, out height))
                                        {
                                            this.SetValue(key, new Size(width, height));
                                            return true;
                                        }

                                        break;
                                    }

                                case "sizef":
                                    {
                                        string wStr = subNode.GetAttribute("width", string.Empty);
                                        string hStr = subNode.GetAttribute("height", string.Empty);
                                        float width, height;
                                        if (float.TryParse(wStr, out width) && float.TryParse(hStr, out height))
                                        {
                                            this.SetValue(key, new SizeF(width, height));
                                            return true;
                                        }

                                        break;
                                    }

                                case "pointf":
                                    {
                                        string xStr = subNode.GetAttribute("x", string.Empty);
                                        string yStr = subNode.GetAttribute("y", string.Empty);
                                        float x, y;
                                        if (float.TryParse(xStr, out x) && float.TryParse(yStr, out y))
                                        {
                                            this.SetValue(key, new PointF(x, y));
                                            return true;
                                        }

                                        break;
                                    }

                                case "datetime":
                                    {
                                        string aStr = subNode.GetAttribute("year", string.Empty);
                                        string rStr = subNode.GetAttribute("month", string.Empty);
                                        string gStr = subNode.GetAttribute("day", string.Empty);
                                        string bStr = subNode.GetAttribute("hour", string.Empty);
                                        string mStr = subNode.GetAttribute("minute", string.Empty);
                                        string sStr = subNode.GetAttribute("second", string.Empty);
                                        string msStr = subNode.GetAttribute("millisecond", string.Empty);
                                        int year, month, day, hour, minute, second, millisecond;
                                        if (int.TryParse(aStr, out year) && int.TryParse(rStr, out month) && int.TryParse(gStr, out day) && int.TryParse(bStr, out hour))
                                        {
                                            if (int.TryParse(mStr, out minute) && int.TryParse(sStr, out second) && int.TryParse(msStr, out millisecond))
                                            {
                                                this.SetValue(key, new DateTime(year, month, day, hour, minute, second, millisecond));
                                                return true;
                                            }
                                        }

                                        break;
                                    }

                                case "color":
                                    {
                                        string aStr = subNode.GetAttribute("a", string.Empty);
                                        string rStr = subNode.GetAttribute("r", string.Empty);
                                        string gStr = subNode.GetAttribute("g", string.Empty);
                                        string bStr = subNode.GetAttribute("b", string.Empty);
                                        byte a, r, g, b;
                                        if (byte.TryParse(aStr, out a) && byte.TryParse(rStr, out r) && byte.TryParse(gStr, out g) && byte.TryParse(bStr, out b))
                                        {
                                            this.SetValue(key, Color.FromArgb(a, r, g, b));
                                            return true;
                                        }

                                        break;
                                    }


                                case "rectangle":
                                    {
                                        string xStr = subNode.GetAttribute("x", string.Empty);
                                        string yStr = subNode.GetAttribute("y", string.Empty);
                                        string wStr = subNode.GetAttribute("width", string.Empty);
                                        string hStr = subNode.GetAttribute("height", string.Empty);
                                        int x, y, w, h;
                                        if (int.TryParse(xStr, out x) && int.TryParse(yStr, out y) && int.TryParse(wStr, out w) && int.TryParse(hStr, out h))
                                        {
                                            if (w >= 0 && h >= 0)
                                            {
                                                this.SetValue(key, new Rectangle(x, y, w, h));
                                                return true;
                                            }
                                        }

                                        break;
                                    }

                                case "rectanglef":
                                    {
                                        string xStr = subNode.GetAttribute("x", string.Empty);
                                        string yStr = subNode.GetAttribute("y", string.Empty);
                                        string wStr = subNode.GetAttribute("width", string.Empty);
                                        string hStr = subNode.GetAttribute("height", string.Empty);
                                        float x, y, w, h;
                                        if (float.TryParse(xStr, out x) && float.TryParse(yStr, out y) && float.TryParse(wStr, out w) && float.TryParse(hStr, out h))
                                        {
                                            if (w >= 0 && h >= 0)
                                            {
                                                this.SetValue(key, new RectangleF(x, y, w, h));
                                                return true;
                                            }
                                        }

                                        break;
                                    }
                                case "ilist":
                                    {

                                        try
                                        {
                                            IList objList = new List<object>();
                                            XmlSettings xs = new XmlSettings();
                                            Dictionary<string, string> dics = new Dictionary<string, string>();
                                            xs.LoadFrom(subNode, dics);
                                            SerializationInfo sinfo = new SerializationInfo(objList.GetType(), new FormatterConverter());
                                            xs.FillSerializeInfo(sinfo);
                                            SerializationInfoEnumerator itor = sinfo.GetEnumerator();
                                            while (itor.MoveNext())
                                            {
                                                objList.Add(itor.Value);
                                            }
                                            this.SetValue(key, objList);
                                        }
                                        catch
                                        {
                                            throw;
                                        }
                                        break;

                                    }
                                default:
                                    {
                                        try
                                        {
                                            Type tp = GetTypeFrom(type);
                                            ISerializationSurrogate serilizer = null;
                                            if (tp != null)
                                            {
                                                object obj = null;
                                                try
                                                {
                                                    if (!type.EndsWith("Font"))
                                                    {
                                                        obj = Activator.CreateInstance(tp);
                                                    }
                                                }
                                                catch
                                                {

                                                }
                                               
                                                serilizer = obj as ISerializationSurrogate;
                                                if (serilizer == null)
                                                {
                                                    serilizer = this.GetSerilizer(tp);
                                                }

                                                if (serilizer != null)
                                                {

                                                    XmlSettings xs = new XmlSettings();
                                                    Dictionary<string, string> dics = new Dictionary<string, string>();
                                                    xs.LoadFrom(subNode, dics);
                                                    SerializationInfo sinfo = new SerializationInfo(tp, new FormatterConverter());
                                                    xs.FillSerializeInfo(sinfo);
                                                    try
                                                    {
                                                        obj = serilizer.SetObjectData(obj, sinfo, new StreamingContext(), null);
                                                    }
                                                    catch
                                                    {
                                                        //throw;
                                                    }

                                                    this.SetValue(key, obj);
                                                    return true;
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            throw;
                                        }
                                        break;
                                    }
                            }
                        
                    }

                    return true;
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsNestedBaseType(Type type, Type baseType)
        {
            if (type.BaseType != null)
            {
                if (baseType.IsInterface)
                {
                    Type t = null;

                    t = type.GetInterface(baseType.Name);

                    if (t != null)
                    {
                        return true;
                    }
                    else
                    {
                        return IsNestedBaseType(type.BaseType, baseType);
                    }
                }
                else
                {
                    if (type.BaseType.Equals(baseType))
                    {
                        return true;
                    }
                    else
                    {
                        return IsNestedBaseType(type.BaseType, baseType);
                    }
                }
            }

            return false;
        }

        public override void WriteMember(System.Xml.XmlTextWriter writer)
        {
            foreach (KeyValuePair<string, string> s in this.stringDic)
            {
                if (string.IsNullOrEmpty(s.Value))
                {
                    continue;
                }

                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "string");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, int> s in this.int32Dic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "int32");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, long> s in this.int64Dic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "int64");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, Guid> s in this.guidDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "guid");
                writer.WriteValue(s.Value.ToString());
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, byte> s in this.byteDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "byte");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, float> s in this.floatDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "single");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, double> s in this.doubleDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "double");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, bool> s in this.boolDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "boolean");
                writer.WriteValue(s.Value);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, Point> s in this.pointDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "point");
                this.WriteAttribute(writer, "x", s.Value.X);
                this.WriteAttribute(writer, "y", s.Value.Y);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, PointF> s in this.pointFDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "pointf");
                this.WriteAttribute(writer, "x", s.Value.X);
                this.WriteAttribute(writer, "y", s.Value.Y);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, Size> s in this.sizeDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "size");
                this.WriteAttribute(writer, "width", s.Value.Width);
                this.WriteAttribute(writer, "height", s.Value.Height);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, SizeF> s in this.sizeFDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "sizef");
                this.WriteAttribute(writer, "width", s.Value.Width);
                this.WriteAttribute(writer, "height", s.Value.Height);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, DateTime> s in this.timeDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "datetime");
                this.WriteAttribute(writer, "year", s.Value.Year);
                this.WriteAttribute(writer, "month", s.Value.Month);
                this.WriteAttribute(writer, "day", s.Value.Day);
                this.WriteAttribute(writer, "hour", s.Value.Hour);
                this.WriteAttribute(writer, "minute", s.Value.Minute);
                this.WriteAttribute(writer, "second", s.Value.Second);
                this.WriteAttribute(writer, "millisecond", s.Value.Millisecond);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, Color> s in this.colorDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "color");
                this.WriteAttribute(writer, "a", s.Value.A);
                this.WriteAttribute(writer, "r", s.Value.R);
                this.WriteAttribute(writer, "g", s.Value.G);
                this.WriteAttribute(writer, "b", s.Value.B);
                writer.WriteEndElement();
            }


            foreach (KeyValuePair<string, Rectangle> s in this.rectDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "rectangle");
                this.WriteAttribute(writer, "x", s.Value.X);
                this.WriteAttribute(writer, "y", s.Value.Y);
                this.WriteAttribute(writer, "width", s.Value.Width);
                this.WriteAttribute(writer, "height", s.Value.Height);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, RectangleF> s in this.rectFDic)
            {
                writer.WriteStartElement("Item");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "rectanglef");
                this.WriteAttribute(writer, "x", s.Value.X);
                this.WriteAttribute(writer, "y", s.Value.Y);
                this.WriteAttribute(writer, "width", s.Value.Width);
                this.WriteAttribute(writer, "height", s.Value.Height);
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, IList> s in this.listDic)
            {
                if (s.Value == null || s.Value.Count < 1) continue;
                writer.WriteStartElement("Items");
                this.WriteAttribute(writer, "key", s.Key);
                this.WriteAttribute(writer, "type", "IList");
                for (int i = 0; i < s.Value.Count; i++)
                {
                    object obj = s.Value[i];


                    ISerializationSurrogate serializer = obj as ISerializationSurrogate;
                    Type tp = obj.GetType();
                    if (serializer == null)
                    {
                        serializer = GetSerilizer(tp);
                    }

                    if (serializer != null)
                    {
                        SerializationInfo sinfo = new SerializationInfo(tp, new FormatterConverter());
                        StreamingContext sc = new StreamingContext();
                        serializer.GetObjectData(obj, sinfo, sc);
                        SaveSerializerInfoTo(writer, sinfo, obj.GetHashCode().ToString(), tp.FullName);
                    }
                    else
                    {
                        ISerializable iserial = obj as ISerializable;
                        if (iserial != null)
                        {
                            SerializationInfo sinfo = new SerializationInfo(tp, new FormatterConverter());
                            StreamingContext sc = new StreamingContext();
                            iserial.GetObjectData(sinfo, sc);
                            SaveSerializerInfoTo(writer, sinfo, obj.GetHashCode().ToString(), tp.FullName);
                        }
                    }
                }
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, object> other in this.otherDic)
            {
                if (other.Value == null)
                {
                    continue;
                }

                ISerializationSurrogate serializer = other.Value as ISerializationSurrogate;
                Type tp = other.Value.GetType();
                if (serializer == null)
                {
                    serializer = GetSerilizer(tp);
                }

                if (serializer != null)
                {
                    SerializationInfo sinfo = new SerializationInfo(tp, new FormatterConverter());
                    StreamingContext sc = new StreamingContext();
                    serializer.GetObjectData(other.Value, sinfo, sc);
                    SaveSerializerInfoTo(writer, sinfo, other.Key, tp.FullName);
                }
                else
                {
                    ISerializable iserial = other.Value as ISerializable;
                    if (iserial != null)
                    {
                        SerializationInfo sinfo = new SerializationInfo(tp, new FormatterConverter());
                        StreamingContext sc = new StreamingContext();
                        iserial.GetObjectData(sinfo, sc);
                        SaveSerializerInfoTo(writer, sinfo, other.Key, tp.FullName);
                    }
                }
            }
        }        

        /// <summary>
        /// 保存配置数据
        /// </summary>
        public virtual void Save()
        {
            this.Save(this.Path);
        }

        /// <summary>
        /// 加载配置数据
        /// </summary>
        /// <returns>是否加载成功</returns>
        public virtual bool Load()
        {
            return this.Load(this.Path);
        }

        public static void SaveSerializerInfoTo(XmlTextWriter writer, SerializationInfo info, string rootName, string typeName)
        {
            XmlSettings setting = new XmlSettings();
            SerializationInfoEnumerator itor = info.GetEnumerator();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("key", rootName);
            dic.Add("type", typeName);           
            while (itor.MoveNext())
            {
                if (itor.Current.Value is int)
                {
                    setting.SetValue(itor.Name, (int)itor.Value);
                }
                else if (itor.Current.Value is string)
                {
                    setting.SetValue(itor.Name, (string)itor.Value);
                }
                else if (itor.Current.Value is bool)
                {
                    setting.SetValue(itor.Name, (bool)itor.Value);
                }
                else if (itor.Current.Value is float)
                {
                    setting.SetValue(itor.Name, (float)itor.Value);
                }
                else if (itor.Current.Value is byte)
                {
                    setting.SetValue(itor.Name, (byte)itor.Value);
                }
                else if (itor.Current.Value is double)
                {
                    setting.SetValue(itor.Name, (double)itor.Value);
                }
                else if (itor.Current.Value is long)
                {
                    setting.SetValue(itor.Name, (long)itor.Value);
                }
                else if (itor.Current.Value is Guid)
                {
                    setting.SetValue(itor.Name, (Guid)itor.Value);
                }
                else if (itor.Current.Value is Color)
                {
                    setting.SetValue(itor.Name, (Color)itor.Value);
                }
                else if (itor.Current.Value is Size)
                {
                    setting.SetValue(itor.Name, (Size)itor.Value);
                }
                else if (itor.Current.Value is SizeF)
                {
                    setting.SetValue(itor.Name, (SizeF)itor.Value);
                }
                else if (itor.Current.Value is Rectangle)
                {
                    setting.SetValue(itor.Name, (Rectangle)itor.Value);
                }
                else if (itor.Current.Value is RectangleF)
                {
                    setting.SetValue(itor.Name, (RectangleF)itor.Value);
                }
                else if (itor.Current.Value is Point)
                {
                    setting.SetValue(itor.Name, (Point)itor.Value);
                }
                else if (itor.Current.Value is PointF)
                {
                    setting.SetValue(itor.Name, (PointF)itor.Value);
                }
                else if (itor.Current.Value is IList && !(itor.Current.Value is Array))
                {
                    setting.SetValue(itor.Name, (IList)itor.Value);
                }
                else
                {
                    setting.SetValue(itor.Name, itor.Value);
                }
            }

            setting.SaveTo(writer, "Item", dic);
        }

        private static Dictionary<string, Type> typeCached = new Dictionary<string, Type>();

        private static List<string> assems = new List<string>();

        public static Type GetTypeFrom(string typeName)
        {
            Type type = null;
            try
            {
                if (typeCached.ContainsKey(typeName))
                {
                    return typeCached[typeName];
                }

                type = Type.GetType(typeName, false, true);
                if (type != null)
                {
                    typeCached.Add(typeName, type);
                    return type;
                }
               

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in assemblies)
                {
                    if (assems.Contains(asm.CodeBase.ToLower()))
                    {
                        type = asm.GetType(typeName);
                        if (type != null)
                        {
                            typeCached.Add(typeName, type);
                            return type;
                        }

                        continue;
                    }

                    assems.Add(asm.CodeBase.ToLower());
                    type = asm.GetType(typeName);
                    if (type != null)
                    {
                        typeCached.Add(typeName, type);
                        return type;
                    }
                }                
            }
            catch
            {
            }
            finally
            {
            }

            return type;
        }
    }
}
