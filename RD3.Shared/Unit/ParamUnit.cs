using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    [Table(nameof(ParamUnit))]
    public class ParamUnit : BindableBase
    {
        private int _id;
        /// <summary>
        /// 主键（自增）
        /// </summary>
        [Key] // 标记为主键
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自增约束
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
    }
}
