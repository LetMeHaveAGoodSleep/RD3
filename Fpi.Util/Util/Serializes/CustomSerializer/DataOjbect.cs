namespace Fpi.Util.Serializes.CustomSerializer
{
    using System;
    using System.IO;

    /// <summary>
    /// 数据对象
    /// </summary>
    public abstract class DataOjbect
    {
        /// <summary>
        /// 最后一次错误
        /// </summary>
        private string error;

        /// <summary>
        /// 获取最后一次IO错误
        /// </summary>
        public string Error
        {
            get { return this.error; }
            protected set { this.error = value; }
        }

        /// <summary>
        /// 加载对象信息
        /// </summary>
        /// <param name="input">输入字节流</param>
        /// <returns>是否成功加载</returns>
        public abstract bool Load(Stream input);

        /// <summary>
        /// 将对象信息保存到指定的字节流
        /// </summary>
        /// <param name="output">指定的字节流</param>
        public abstract void Save(Stream output);

        /// <summary>
        /// 从指定路径的文件加载对象信息
        /// </summary>
        /// <param name="path">指定的路径</param>
        /// <returns>是否成功加载</returns>
        public virtual bool Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                this.error = "Path can not be null or empty";
                return false;
            }

            if (!File.Exists(path))
            {
                this.error = "File not exist";
                return false;
            }

            bool result = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                result = this.Load(fs);
            }
            catch (System.Exception ex)
            {
                this.error = ex.Message;
                result = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 将对象信息保存到指定路径的文件
        /// </summary>
        /// <param name="path">指定的路径</param>
        public virtual void Save(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                this.error = "Path can not be null or empty";
                return;
            }

            FileStream fs = null;
            string temp = string.Empty;
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (File.Exists(path))
                {
                    temp = Path.Combine(dir, Guid.NewGuid().ToString() + ".tmp");
                    File.Copy(path, temp, true);
                }

                fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                fs.SetLength(0);
                this.Save(fs);
                fs.Flush();
                if ((!string.IsNullOrEmpty(temp)) && File.Exists(temp))
                {
                    File.Delete(temp);
                }
            }
            catch (System.Exception ex)
            {
                this.error = ex.Message;
                if (!string.IsNullOrEmpty(temp))
                {
                    if (File.Exists(temp))
                    {
                        try
                        {
                            if (fs != null)
                            {
                                fs.Close();
                                fs = null;
                            }

                            File.Copy(temp, path, true);
                        }
                        finally
                        {
                        }

                        try
                        {
                            File.Delete(temp);
                        }
                        finally
                        {
                        }
                    }
                }

                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
            }
        }
    }
}
