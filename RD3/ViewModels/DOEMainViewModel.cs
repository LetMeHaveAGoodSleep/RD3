using Fpi.Communication.Commands.Config;
using ImTools;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class DOEMainViewModel : BaseViewModel, IDialogAware
    {
        private bool _designEnable = false;
        public bool DesignEnable
        {
            get => _designEnable;
            set { SetProperty(ref _designEnable, value); }
        }

        private bool _bindEnable = false;
        public bool BindEnable
        {
            get => _bindEnable;
            set { SetProperty(ref _bindEnable, value); }
        }

        private ObservableCollection<Factor> _selectedFactors = [];
        public ObservableCollection<Factor> SelectedFactors
        {
            get => _selectedFactors;
            set => SetProperty(ref _selectedFactors, value);
        }

        private string _selectedDoeColunm;
        public string SelectedDoeColunm
        {
            get => _selectedDoeColunm;
            set 
            {
                BindEnable = false;
                SetProperty(ref _selectedDoeColunm, value);
                if (value == "Response")
                {
                    BindEnable = true;
                }
            } 
        }

        private ObservableCollection<string> _doeColunms = [];
        public ObservableCollection<string> DoeColunms
        {
            get => _doeColunms;
            set => SetProperty(ref _doeColunms, value);
        }

        private DOEResponse _selectedResponse;
        public DOEResponse SelectedResponse
        {
            get => _selectedResponse;
            set
            {
                BindEnable = false;
                SetProperty(ref _selectedResponse, value);
                if (SelectedDoeColunm == "Response")
                {
                    BindEnable = true;
                }
            }
        }

        private ObservableCollection<DOEParameterPair> _parameterPairs = [];
        public ObservableCollection<DOEParameterPair> ParameterPairs
        {
            get => _parameterPairs;
            set => SetProperty(ref _parameterPairs, value);
        }

        private ObservableCollection<OrthogonalParam> _designCol = [];
        public ObservableCollection<OrthogonalParam> DesignCol
        {
            get => _designCol;
            set => SetProperty(ref _designCol, value);
        }

        private DataTable DataDesign = new();

        private DataTable _dataResult = new DataTable();
        public DataTable DataResult
        {
            get { return _dataResult; }
            private set { SetProperty(ref _dataResult, value); }
        }

        public List<Device> SelectedDevices = [];

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand DesignCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                { "Factors", SelectedFactors }
            };

            DialogHostService.ShowOnce(nameof(DOEDesignView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    DoeColunms = [];
                    ParameterPairs = [];
                    return;
                }
                DesignCol = callback.Parameters.GetValue<ObservableCollection<OrthogonalParam>>(nameof(DesignCol));
                DataDesign = callback.Parameters.GetValue<DataTable>("DesignResult");

                var copyData = DataDesign.Clone();
                copyData.Columns.Add("Response");
                copyData.Columns.Add("Reactor");
                copyData.Rows.Clear();
                foreach (DataRow item in DataDesign.Rows)
                {
                    DataRow dataRow = copyData.NewRow();
                    foreach (DataColumn item1 in copyData.Columns)
                    {
                        foreach (DataColumn column in DataDesign.Columns)
                        {
                            if (column.ColumnName == item1.ColumnName)
                            {
                                dataRow[item1] = item[column];
                                break;
                            }
                        }
                        item1.ReadOnly = true;
                    }
                    copyData.Rows.Add(dataRow);
                }
                DataResult = copyData;

                DoeColunms.Clear();
                DoeColunms.AddRange(SelectedFactors.Select(f => f.ToString()).ToList());
                DoeColunms.Add("Response");

                ParameterPairs.Clear();
                foreach (var item in SelectedFactors)
                {
                    ParameterPairs.Add(new DOEParameterPair() { Param1 = item.ToString(), Param2 = item.ToString(), IsResponse = false });
                }
            });
        });

        public DelegateCommand BindCommand => new(() =>
        {
            var parameterPair = ParameterPairs.FindFirst(t => t.Param1 == "Response");
            if (parameterPair != null)
            {
                if (parameterPair.Param2 == SelectedResponse.ToString())
                {
                    return;
                }
                DOEResponse res = (DOEResponse)Enum.Parse(typeof(DOEResponse), parameterPair.Param2); // 转换
                if (HandyControl.Controls.MessageBox.Show($"响应面已与'{EnumUtil.GetEnumDescription(res)}'绑定，是否切换至与'{EnumUtil.GetEnumDescription(SelectedResponse)}'绑定?", "温馨提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                ParameterPairs.Remove(parameterPair);
            }
            parameterPair = new DOEParameterPair() { Param1 = "Response", Param2 = SelectedResponse.ToString(), IsResponse = true };
            ParameterPairs.Add(parameterPair);
            HandyControl.Controls.MessageBox.Info($"绑定成功", "温馨提示");
        });

        public DelegateCommand UnBindCommand => new(() =>
        {
            var parameterPair = ParameterPairs.FindFirst(t => t.Param1 == "Response" && t.Param2 == SelectedResponse.ToString());
            if (parameterPair != null)
            {
                ParameterPairs.Remove(parameterPair);
                HandyControl.Controls.MessageBox.Info("解绑成功", "温馨提示");
            }
        });

        public DelegateCommand SaveCommand => new(() =>
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "保存DOE文件";
            // 设置.doe文件过滤器
            saveFileDialog.Filter = "DOE文件(*.doe)|*.doe";
            saveFileDialog.DefaultExt = ".doe";

            if ((bool)saveFileDialog.ShowDialog())
            {
                string filePath = saveFileDialog.FileName;
                string json = DataDesign.ToJson();
                var json1 = JsonConvert.SerializeObject(ParameterPairs);
                json = JsonConvertUtil.MergeJsons(json, json1);
                //json = AESEncryption.Encrypt(json);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, json);
                HandyControl.Controls.MessageBox.Info("保存成功", "温馨提示");
            }
        });

        public DelegateCommand ImportCommand => new(() =>
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择.doe文件";
            openFileDialog.Filter = "DOE文件(*.doe)|*.doe";
            openFileDialog.DefaultExt = ".doe";

            if ((bool)openFileDialog.ShowDialog())
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    //fileContent = AESEncryption.Decrypt(fileContent);
                    JToken mergedToken = JToken.Parse(fileContent);
                    if (mergedToken.Type == JTokenType.Array)
                    {
                        JArray mergedArray = (JArray)mergedToken;
                        DataDesign = JsonConvert.DeserializeObject<DataTable>(mergedArray[0].ToString());
                        ParameterPairs = JsonConvert.DeserializeObject<ObservableCollection<DOEParameterPair>>(mergedArray[1].ToString());
                    }
                    var copyData = DataDesign.Clone();
                    copyData.Columns.Add("Response",typeof(double));
                    copyData.Columns.Add("Reactor");
                    copyData.Rows.Clear();
                    foreach (DataRow item in DataDesign.Rows)
                    {
                        DataRow dataRow = copyData.NewRow();
                        foreach (DataColumn item1 in copyData.Columns)
                        {
                            foreach (DataColumn column in DataDesign.Columns)
                            {
                                if (column.ColumnName == item1.ColumnName)
                                {
                                    dataRow[item1] = item[column];
                                    break;
                                }
                            }
                            item1.ReadOnly = true;
                        }
                        copyData.Rows.Add(dataRow);
                    }
                    DataResult = copyData;
                    DoeColunms.Clear();
                    SelectedFactors.Clear();
                    foreach (DOEParameterPair item in ParameterPairs)
                    {
                        DoeColunms.Add(item.Param1);
                        SelectedFactors.Add((Factor)Enum.Parse(typeof(Factor), item.Param2));
                    }
                }
                catch (Exception ex)
                {
                    HandyControl.Controls.MessageBox.Info($"读取文件时出现错误：{ex.Message}", "温馨提示");
                }
            }
        });

        public DelegateCommand ChooseReactorCommand => new(() => 
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                { "SelectedDevices", SelectedDevices }
            };
            DialogHostService.ShowOnce(nameof(ChooseReactorView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
                SelectedDevices = callback.Parameters.GetValue<List<Device>>("Reactors");
                foreach (DataColumn column in DataResult.Columns) 
                {
                    column.ReadOnly = false;
                }
                for (int i = 0; i < DataResult.Rows.Count; i++)
                {
                    DataRow row = DataResult.Rows[i];
                    if (i >= SelectedDevices.Count)
                    {
                        row["Reactor"] = SelectedDevices[i % SelectedDevices.Count].Name;
                    }
                    else
                    {
                        row["Reactor"] = SelectedDevices[i].Name;
                    }
                }
                foreach (DataColumn column in DataResult.Columns)
                {
                    column.ReadOnly = true;
                }
            });
        });

        public DelegateCommand GetResultCommand => new(async () =>
        {
            var resPair = ParameterPairs.FindFirst(t => t.IsResponse);
            if (resPair == null)
            {
                HandyControl.Controls.MessageBox.Info("未绑定响应因子", "温馨提示");
                return;
            }
            await Task.Run(() =>
              {
                  List<string> sqlList = [];
                  foreach (DataRow item in DataResult.Rows)
                  {
                      List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>();
                      string deviceId = item["Reactor"].ToString();
                      for (int i = 1; i < DataResult.Columns.Count - 2; i++)
                      {
                          list.Add(System.Collections.Generic.KeyValuePair.Create(DataResult.Columns[i].ColumnName, Convert.ToDouble(item[DataResult.Columns[i]])));
                      }
                      var sql = DOEResultUtil.GetDOEResultSql(list, deviceId, resPair.Param2);
                      sqlList.Add(sql);
                  }
                  DataTable dataTable = SQLiteHelper.GetDatatableSync(sqlList);
                  DataResult.Columns["Response"].ReadOnly = false;
                  for (int i = 0; i < DataResult.Rows.Count; i++)
                  {
                      if (i > dataTable.Rows.Count - 1)
                      {
                          DataResult.Rows[i]["Response"] = RandomNumberUtil.GetRandomDouble(10, 100);
                      }
                      else
                      {
                          DataResult.Rows[i]["Response"] = dataTable.Rows[i][resPair.Param2];
                      }
                  }
                  DataResult.Columns["Response"].ReadOnly = true;
              });

        });

        public DelegateCommand AnalyseCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(ChooseDOEAnalyseMethodView), callback => 
            {
                if (callback.Result != ButtonResult.OK) return;
                bool is3DView = callback.Parameters.GetValue<bool>("Is3DView");
                if (is3DView)
                {

                }
                else
                {
                    DialogParameters keyValuePairs = new DialogParameters()
                    {
                        {"Result",DataResult },
                        {nameof(OrthogonalParam),DesignCol }
                    };
                    DialogHostService.ShowOnce(nameof(DOEAnalyse2DView), keyValuePairs, callback => 
                    {

                    });
                }
            });
        });

        public DOEMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "实验设计";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }
    }

    public class DOEParameterPair : BindableBase
    {
        private string _param1;
        public string Param1
        {
            get => _param1;
            set => SetProperty(ref _param1, value);
        }

        private string _param2;
        public string Param2
        {
            get => _param2;
            set => SetProperty(ref _param2, value);
        }

        private bool _isResponse;
        public bool IsResponse
        {
            get => _isResponse;
            set => SetProperty(ref _isResponse, value);
        }
    }
}
