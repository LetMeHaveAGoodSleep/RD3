using System;
using System.Collections;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Fpi.Util.Compress
{
    /// <summary>
    /// CompressUtil 的摘要说明。
    /// </summary>
    public class CompressUtil
    {
        private CompressUtil()
        {
        }

        public static byte[] CompressBytes(byte[] data)
        {
            Deflater f = new Deflater(Deflater.BEST_COMPRESSION);
            f.SetInput(data);
            f.Finish();

            MemoryStream o = new MemoryStream(data.Length);

            try
            {
                byte[] buf = new byte[1024];
                while (!f.IsFinished)
                {
                    int got = f.Deflate(buf);
                    o.Write(buf, 0, got);
                }
            }
            finally
            {
                o.Close();
            }
            return o.ToArray();
        }

        public static byte[] DecompressBytes(byte[] data)
        {
            Inflater f = new Inflater();
            f.SetInput(data);

            MemoryStream o = new MemoryStream(data.Length);
            try
            {
                byte[] buf = new byte[1024];
                while (!f.IsFinished)
                {
                    int got = f.Inflate(buf);
                    o.Write(buf, 0, got);
                }
            }
            finally
            {
                o.Close();
            }
            return o.ToArray();
        }

        public static void CompressBytesToFile(string zipFile, string unzipFile, byte[] data)
        {
            FileStream fs = new FileStream(zipFile, FileMode.Create);
            using (ZipOutputStream zip = new ZipOutputStream(fs))
            {
                ZipEntry ze = new ZipEntry(ZipEntry.CleanName(Path.GetFileName(unzipFile)));
                zip.PutNextEntry(ze);
                zip.Write(data, 0, data.Length);
                zip.Finish();
                zip.Close();
            }
            fs.Close();
        }

        /// <summary>
        /// 将多个文件流压为一个zip文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="zipedFile"></param>
        public static void ZipFiles(FileStream[] files, string zipedFile)
        {
            using (FileStream w = new FileStream(zipedFile, FileMode.Create))
            {
                using (ZipOutputStream zip = new ZipOutputStream(w))
                {
                    foreach (FileStream fs in files)
                    {
                        ZipEntry ze = new ZipEntry(ZipEntry.CleanName(Path.GetFileName(fs.Name)));
                        zip.PutNextEntry(ze);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        zip.Write(buffer, 0, buffer.Length);
                        fs.Close();
                    }
                    zip.Finish();
                    zip.Close();
                }
            }
        }

        /// <summary>
        /// 将该目录内所有子目录和文件全压成一个zip文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="zipedFile"></param>
        /// <param name="zipFinishRemoved"></param>
        public static void ZipFiles(string dir, string zipedFile, bool zipFinishRemoved)
        {
            string newFile = zipedFile;

            using (FileStream w = new FileStream(newFile, FileMode.Create))
            {
                using (ZipOutputStream zip = new ZipOutputStream(w))
                {
                    WalkZip(dir, dir, zip, newFile, zipFinishRemoved);
                    zip.Finish();
                    zip.Close();
                }
            }
            if (zipFinishRemoved)
            {
                Directory.Delete(dir, false);
            }
        }

        private static void WalkZip(string rootDir, string dir,
                                    ZipOutputStream output, string outFile, bool zipFinishRemoved)
        {
            foreach (string f in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(f) == ".zip" || f.ToLower() == outFile.ToLower())
                {
                    continue;
                }
                ZipEntry fileZip = new ZipEntry(ZipEntry.CleanName(GetShortFileName(rootDir, f)));
                output.PutNextEntry(fileZip);

                FileStream fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int) fs.Length);
                output.Write(buffer, 0, buffer.Length);
                fs.Close();
                if (zipFinishRemoved)
                {
                    File.Delete(f);
                }
            }
            foreach (string d in Directory.GetDirectories(dir))
            {
                ZipEntry dirZip =
                    new ZipEntry((ZipEntry.CleanName(GetShortFileName(rootDir, d)) + "\\").Replace("//", "/"));
                output.PutNextEntry(dirZip);

                WalkZip(rootDir, d, output, outFile, zipFinishRemoved);

                if (zipFinishRemoved)
                {
                    Directory.Delete(d, false);
                }
            }
        }

        private static string GetShortFileName(string dir, string p)
        {
            return p.Substring(dir.Length);
        }

        public static string DecompressBytes(string filePath)
        {
            byte[] data = DecompressToBytes(filePath);
            return GetString(data, 0, data.Length);
        }

        private static string GetString(byte[] value, int startIndex, int length)
        {
            for (int i = startIndex + length - 1; i >= startIndex; i--)
            {
                if (value[i] != (byte) 0)
                {
                    length = i + 1 - startIndex;
                    break;
                }
            }
            return UTF8Encoding.UTF8.GetString(value, startIndex, length);
        }


        public static Stream DecompressToStream(string filePath)
        {
            byte[] data = DecompressToBytes(filePath);
            Stream s = new MemoryStream(data);
            return s;
        }

        /// <summary>
        /// 将压缩文件解压到文件
        /// add by maohb.2010.9.6
        /// </summary>
        /// <param name="zipfile">压缩文件</param>
        /// <param name="destDir">解压后存放的目标目录</param>
        public static void DecompressToFiles(string zipfile, string destDir)
        {
            if (!destDir.EndsWith(@"\"))
            {
                destDir = destDir + @"\";
            }
            using (ZipInputStream inputStream = new ZipInputStream(File.OpenRead(zipfile)))
            {
                ZipEntry entry;

                while ((entry = inputStream.GetNextEntry()) != null)
                {
                    byte[] buffer = new byte[inputStream.Length];
                    inputStream.Read(buffer, 0, buffer.Length);

                    using (FileStream w = new FileStream(destDir + entry.Name, FileMode.Create))
                    {
                        w.Write(buffer, 0, buffer.Length);
                    }
                }

            }
        }

        private static byte[] DecompressToBytes(string filePath)
        {
            using (ZipInputStream inputStream = new ZipInputStream(File.OpenRead(filePath)))
            {
                ZipEntry entry;
                ArrayList entryList = new ArrayList();
                int count = 0;
                while ((entry = inputStream.GetNextEntry()) != null)
                {
                    byte[] buffer = new byte[inputStream.Length];
                    inputStream.Read(buffer, 0, buffer.Length);
                    entryList.Add(buffer);
                    count += buffer.Length;
                }


                byte[] data = new byte[count];
                int offset = 0;
                foreach (byte[] bs in entryList)
                {
                    Buffer.BlockCopy(bs, 0, data, offset, bs.Length);
                    offset += bs.Length;
                }
                return data;
            }
        }
    }
}