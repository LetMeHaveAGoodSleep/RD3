using System;
using System.Collections;
using System.IO;


namespace Fpi.Util.Sundry
{
    /// <summary>
    /// FileUtil 的摘要说明。
    /// </summary>
    public class FileUtil
    {
        private FileUtil()
        {
        }

        /// <summary>
        /// 注意：请勿在委托中使用lock(Fpi.Devices.FileSystem.fileLockObj)
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="handler"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object AccessFile(string fileName, AccessFileHandler handler, params object[] args)
        {
#if WINCE
			lock(FileSystem.fileLockObj)
#else
            lock (lockObj)
#endif
            {
                if (handler != null)
                    return handler(fileName, args);
                else
                    return null;
            }
        }

        public static bool Save(string filename, string text)
        {
            return Save(filename, text, true);
        }

        public static byte[] GetBytes(string filename)
        {
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] data = new byte[fs.Length];
                byte[] buffer = new byte[1024];
                int index = 0;
                int offset = 0;
                while (true)
                {
                    index = fs.Read(buffer, 0, 1024);
                    if (index == 0)
                        break;
                    Buffer.BlockCopy(buffer, 0, data, offset, index);
                    offset += index;
                }
                return data;
            }
        }

        public static bool Save(string filename, string text, bool commit)
        {
#if !WINCE
            return TempSave(filename, text);
#else
			bool ret = TempSave(filename, text);
#if !ONSIMULATE
			if (commit)
			{
#if GPDP
			    FileSystem.GetInstance().WriteDataAreaFromRAMToFlash();
#endif
			}
#endif
			return ret;
#endif
        }

        public static bool Save(string filename, byte[] data)
        {
            return Save(filename, data, true);
        }

        public static bool Save(string filename, byte[] data, bool commit)
        {
#if !WINCE
            return TempSave(filename, data);
#else
			bool ret = TempSave(filename, data);
#if !ONSIMULATE
			if (commit)
			{
#if GPDP
			    FileSystem.GetInstance().WriteDataAreaFromRAMToFlash();
#endif
			}
#endif
			return ret;
#endif
        }

        private static object lockObj = new object();

        private static bool TempSave(string fileName, string text)
        {
#if WINCE
			lock(FileSystem.fileLockObj)
#else
            lock (lockObj)
#endif
            {
                TextWriter textWriter = null;
                try
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    textWriter = File.CreateText(fileName);

                    textWriter.Write(text);
                    textWriter.Flush();
                }
                finally
                {
                    if (textWriter != null)
                        textWriter.Close();
                }
                return true;
            }
        }

        public static string[] LoadFile(string fileName)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fileName);
                string str = "";
                ArrayList lst = new ArrayList();
                while (null != (str = sr.ReadLine()))
                {
                    str = str.Trim();
                    if (str.Length > 0)
                    {
                        lst.Add(str);
                    }
                }
                return (string[]) lst.ToArray(typeof (string));
            }
            finally
            {
                if (null != sr)
                {
                    sr.Close();
                }
            }
        }

        private static bool TempSave(string fileName, byte[] data)
        {
#if WINCE
			lock(FileSystem.fileLockObj)
#else
            lock (lockObj)
#endif
            {
                FileStream fs = null;
                BinaryWriter w = null;

                try
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    fs = new FileStream(fileName, FileMode.CreateNew);
                    w = new BinaryWriter(fs);
                    w.Write(data);
                }
                finally
                {
                    if (w != null)
                    {
                        w.Close();
                    }
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
                return true;
            }
        }
    }

    public delegate object AccessFileHandler(string fileName, params object[] args);
}