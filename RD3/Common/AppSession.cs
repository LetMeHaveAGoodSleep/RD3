using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Common
{
    public static class AppSession
    {
        public static User CurrentUser { get; set; }

        public static ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public static string CompanyName { get{ return VarConfig.GetValue("Company")?.ToString(); }}
    }
}
