using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class FileConst
    {
        public static readonly string ConfigDirectory = AppDomain.CurrentDomain.BaseDirectory + "Config";
        public static readonly string UserPath = ConfigDirectory + "\\User.json";
        public static readonly string ConstPath = ConfigDirectory + "\\Const.json";
        public static readonly string VarPath = ConfigDirectory + "\\Var.json";
    }
}
