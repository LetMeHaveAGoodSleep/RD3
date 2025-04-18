using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class NamedXmlSettings : XmlSettings
    {
        public NamedXmlSettings(string filename)
        {
            if (File.Exists(filename))
            {
                this.Path = filename;
            }
            else
            {
                string dir = System.IO.Path.Combine(Application.StartupPath, @"Config\UIInfo\");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                this.Path = System.IO.Path.Combine(Application.StartupPath, @"Config\UIInfo\" + filename + ".xml");
            }
        }  
    }
}
