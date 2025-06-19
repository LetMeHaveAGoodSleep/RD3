using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
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

        private List<Factor> _selectedFactors = [];
        public List<Factor> SelectedFactors
        {
            get => _selectedFactors;
            set => SetProperty(ref _selectedFactors, value);
        }

        private ObservableCollection<Factor> _factors = [];
        public ObservableCollection<Factor> Factors
        {
            get=> _factors;
            set => SetProperty(ref _factors, value);
        }

        private string _selectedDoeColunm;
        public string SelectedDoeColunm
        {
            get => _selectedDoeColunm;
            set 
            {
                if (value == "Response")
                {
                    BindEnable = true;
                }
                SetProperty(ref _selectedDoeColunm, value);
            } 
        }

        private ObservableCollection<string> _doeColunms = [];
        public ObservableCollection<string> DoeColunms
        {
            get => _doeColunms;
            set => SetProperty(ref _doeColunms, value);
        }

        private string _selectedResponse;
        public string SelectedResponse
        {
            get => _selectedResponse;
            set => SetProperty(ref _selectedResponse, value);
        }

        private ObservableCollection<string> _responses = [];
        public ObservableCollection<string> Responses
        {
            get => _responses;
            set => SetProperty(ref _responses, value);
        }

        private ObservableCollection<KeyValuePair<string,string>> _parameterPairs = [];
        public ObservableCollection<KeyValuePair<string, string>> ParameterPairs
        {
            get => _parameterPairs;
            set => SetProperty(ref _parameterPairs, value);
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

            DialogHostService.ShowDialog(nameof(DOEDesignView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    DoeColunms = [];
                    ParameterPairs = [];
                    return;
                }

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
                var temp = SelectedFactors.ConvertAll(t => t.ToString());
                DoeColunms.AddRange(temp);
                DoeColunms.Add("Response");

                ParameterPairs.Clear();
                foreach (var item in temp)
                {
                    ParameterPairs.Add(KeyValuePair.Create(item, item));
                }
            });
        });

        public DelegateCommand BindCommand => new(async () => 
        {
            var keyValuePair = KeyValuePair.Create(SelectedResponse, SelectedDoeColunm);
            if (!ParameterPairs.Contains(keyValuePair))
            {
                ParameterPairs.Add(keyValuePair);
                SelectedDoeColunm = null;
                SelectedResponse = null;
                await DialogExtensions.Info("温馨提示", "绑定成功");
            }
        });

        public DelegateCommand UnBindCommand => new(async () =>
        {
            var keyValuePair = KeyValuePair.Create(SelectedResponse, SelectedDoeColunm);
            if (ParameterPairs.Contains(keyValuePair))
            {
                ParameterPairs.Remove(keyValuePair);
                SelectedDoeColunm = null;
                SelectedResponse = null;
                await DialogExtensions.Info("温馨提示", "解绑成功");
            }
        });

        public DelegateCommand SaveCommand => new(async () =>
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
                await DialogExtensions.Info("温馨提示", "保存成功");
            }
        });
        public DelegateCommand ImportCommand => new(async () =>
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
                        ParameterPairs = JsonConvert.DeserializeObject<ObservableCollection<KeyValuePair<string, string>>>(mergedArray[1].ToString());
                    }
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
                    SelectedFactors.Clear();
                    foreach (KeyValuePair<string, string> item in ParameterPairs)
                    {
                        DoeColunms.Add(item.Value);
                        SelectedFactors.Add((Factor)Enum.Parse(typeof(Factor), item.Value));
                    }
                }
                catch (Exception ex)
                {
                    await DialogExtensions.Info("异常", $"读取文件时出现错误：{ex.Message}");
                }
            }
        });

        public DelegateCommand ChooseReactorCommand => new(() => 
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                { "SelectedDevices", SelectedDevices }
            };
            DialogHostService.ShowDialog(nameof(ChooseReactorView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
                SelectedDevices = callback.Parameters.GetValue<List<Device>>("Reactors");
                foreach (DataColumn column in DataResult.Columns) 
                {
                    column.ReadOnly= false;
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

        });
        public DelegateCommand AnalyseCommand => new(async () =>
        {

        });
        public DOEMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            var collection = EnumUtil.GetEnumValues<Factor>();
            foreach (var item in collection) 
            {
                Factors.Add(item);
            }

            Responses.Clear();
            var collection1 = EnumUtil.GetEnumValues<DOEResponse>();
            foreach (var item in collection1)
            {
                Responses.Add(EnumUtil.GetEnumDescription(item));
            }
        }

        public string Title => AppSession.CompanyName;

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
}
