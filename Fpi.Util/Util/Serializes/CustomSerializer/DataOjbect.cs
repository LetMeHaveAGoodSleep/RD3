namespace Fpi.Util.Serializes.CustomSerializer
{
    using System;
    using System.IO;

    /// <summary>
    /// ���ݶ���
    /// </summary>
    public abstract class DataOjbect
    {
        /// <summary>
        /// ���һ�δ���
        /// </summary>
        private string error;

        /// <summary>
        /// ��ȡ���һ��IO����
        /// </summary>
        public string Error
        {
            get { return this.error; }
            protected set { this.error = value; }
        }

        /// <summary>
        /// ���ض�����Ϣ
        /// </summary>
        /// <param name="input">�����ֽ���</param>
        /// <returns>�Ƿ�ɹ�����</returns>
        public abstract bool Load(Stream input);

        /// <summary>
        /// ��������Ϣ���浽ָ�����ֽ���
        /// </summary>
        /// <param name="output">ָ�����ֽ���</param>
        public abstract void Save(Stream output);

        /// <summary>
        /// ��ָ��·�����ļ����ض�����Ϣ
        /// </summary>
        /// <param name="path">ָ����·��</param>
        /// <returns>�Ƿ�ɹ�����</returns>
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
        /// ��������Ϣ���浽ָ��·�����ļ�
        /// </summary>
        /// <param name="path">ָ����·��</param>
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
