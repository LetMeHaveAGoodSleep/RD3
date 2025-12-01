using ClipperLib;
using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class RealTimeDataViewModel : BaseViewModel,IDialogAware
    {
        private Fermentor _currentFermentor = AnalysisSolution.GetInstance().CurrentFermentor;
        public Fermentor CurrentFermentor
        {
            get => _currentFermentor;
            set => SetProperty(ref _currentFermentor, value);
        }

        public ObservableCollection<Fermentor> Fermentors
        {
            get => AnalysisSolution.GetInstance().FermentorCol;
        }

        public ObservableCollection<ReportNode> ReportNodes
        {
            get => AnalysisSolution.GetInstance().ReportNodeCol;
        }

        private DateTime? _startTime = DateTime.Today.AddDays(-3);
        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private DateTime? _endTime = DateTime.Today;
        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private CancellationTokenSource _exportCancellationTokenSource;

        private DateTime _lastSearchTime;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(300);
        private CancellationTokenSource _cancellationTokenSource;
        private string _condition = string.Empty;

        private ObservableCollection<RealTimeParam> _realTimeParamCol = [];
        public ObservableCollection<RealTimeParam> RealTimeParamCol { get { return _realTimeParamCol; } set { SetProperty(ref _realTimeParamCol, value); } }

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }

        private int _pageIndex = 1;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        private int _dataCountPerPage = 10;
        public int DataCountPerPage
        {
            get { return _dataCountPerPage; }
            set { SetProperty(ref _dataCountPerPage, value); }
        }

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public DelegateCommand QueryCommand => new(async () =>
        {
            _condition = $"where SampleTime > '{_startTime?.ToString(Const.DateTimeFormat)}' and SampleTime < '{_endTime?.ToString(Const.DateTimeFormat)}' and ReactorName = '{CurrentFermentor.Device.Name}' ";
            await QueryAsync();
        });

        public DelegateCommand ExportCommand => new DelegateCommand(ExecuteExportAsync);

        public DelegateCommand ChooseColumnCommand => new DelegateCommand(() => 
        {
            DialogHostService.ShowDialog(nameof(ChooseRTParamView),  callback => { });
        });

        public DelegateCommand ReloadDataCommand => new(async () =>
        {
            await QueryAsync();
        });

        public DelegateCommand CancelLoadCommand => new(() =>
        {
            _cancellationTokenSource?.Cancel();
        });

        public RealTimeDataViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        async Task QueryAsync()
        {
            // 取消之前的搜索请求（如果有）
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var now = DateTime.Now;
            _lastSearchTime = now;

            await Task.Delay(_debounceInterval);

            // 如果在等待期间有新的搜索请求，则跳过此次执行
            if ((DateTime.Now - _lastSearchTime) < _debounceInterval)
                return;

            try
            {
                var list = await Task.Run(() =>
                {
                    DataTable dataTable = RD3SQLHelper.GetPaginationCount(RD3SQLHelper.RTParamTable, _condition);
                    int dataCount = Convert.ToInt32(dataTable.Rows[0][0]);
                    PageCount = dataCount / DataCountPerPage + (dataCount % DataCountPerPage != 0 ? 1 : 0);
                    int startIndex = (PageIndex - 1) * DataCountPerPage + 1;
                    DataTable data = RD3SQLHelper.GetPaginationData(RD3SQLHelper.RTParamTable, _condition, startIndex, DataCountPerPage);
                    var list = DataTableConverter.ConvertTo<RealTimeParam>(data);
                    return list;
                }, _cancellationTokenSource.Token);
                RealTimeParamCol.Clear();
                foreach (RealTimeParam row in list)
                {
                    RealTimeParamCol.Add(row);
                }
            }
            catch (OperationCanceledException)
            {
                // 搜索被取消，无需处理
            }
            catch (Exception ex)
            {
                // 处理其他异常
                HandyControl.Controls.MessageBox.Show($"搜索出错: {ex.Message}", "温馨提示");
            }



        }

        private async void ExecuteExportAsync()
        {
            // 1. 取消之前的导出操作（如果存在）
            _exportCancellationTokenSource?.Cancel();
            // 释放之前的取消令牌源资源
            _exportCancellationTokenSource?.Dispose();

            // 2. 创建新的取消令牌源，设置60秒后自动取消
            using (_exportCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
            {
                try
                {
                    // 3. 调用异步导出方法，传入取消令牌
                    await ExportDataToCsvAsync(_exportCancellationTokenSource.Token);
                    HandyControl.Controls.MessageBox.Info("数据已经导出至桌面\\Export文件夹下");
                }
                catch (OperationCanceledException)
                {
                }
                catch (DirectoryNotFoundException ex)
                {
                }
                catch (IOException ex)
                {
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    // 9. 最终释放取消令牌源（using也会自动释放，此处双重保障）
                    _exportCancellationTokenSource?.Dispose();
                    _exportCancellationTokenSource = null;
                }
            }
        }

        /// <summary>
        /// 真正的异步导出逻辑方法
        /// </summary>
        /// <param name="cancellationToken">取消令牌（用于终止导出操作）</param>
        /// <returns>异步任务</returns>
        private async Task ExportDataToCsvAsync(CancellationToken cancellationToken)
        {
            // 检查取消令牌是否已触发
            cancellationToken.ThrowIfCancellationRequested();

            #region 步骤1：构建CSV表头
            const string split = ","; // CSV列分隔符
            var colHeadsBuilder = new StringBuilder();
            // 筛选启用的列，拼接表头
            foreach (var item in ReportNodes.Where(t => t.Used))
            {
                // 每次拼接前检查取消状态
                cancellationToken.ThrowIfCancellationRequested();
                colHeadsBuilder.Append($"{item.ShowName}{split}");
            }
            // 移除末尾的分隔符，添加换行
            string colHeads = colHeadsBuilder.ToString().TrimEnd(',') + "\r\n";
            #endregion

            #region 步骤2：构建CSV内容体
            var csvContentBuilder = new StringBuilder();
            csvContentBuilder.Append(colHeads); // 追加表头

            // 检查取消令牌
            cancellationToken.ThrowIfCancellationRequested();

            // 异步查询SQLite数据（关键：将同步SQL查询改为异步，避免阻塞UI）
            string querySql = $"select * from {RD3SQLHelper.RTParamTable} {_condition}";
            DataTable table = await Task.Run(() =>
            {
                // 子任务中也需检查取消状态
                cancellationToken.ThrowIfCancellationRequested();
                return SQLiteHelper.GetDatasToDataTable(querySql);
            }, cancellationToken);

            // 遍历数据行，拼接CSV内容
            foreach (DataRow row in table.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var lineBuilder = new StringBuilder();
                foreach (var node in ReportNodes.Where(t => t.Used))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // 处理空值：避免row[node.FieldName]为DBNull导致的空引用
                    string cellValue = row[node.FieldName] is DBNull ? string.Empty : row[node.FieldName].ToString();
                    lineBuilder.Append($"{cellValue}{split}");
                }
                // 移除末尾分隔符，添加换行
                string line = lineBuilder.ToString().TrimEnd(',') + "\r\n";
                csvContentBuilder.Append(line);
            }
            #endregion

            #region 步骤3：生成文件路径并写入
            cancellationToken.ThrowIfCancellationRequested();

            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // 拼接output文件夹路径
            string outputDir = Path.Combine(desktopPath, "Export");
            // 自动创建不存在的文件夹
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // 生成文件名（处理StartTime/EndTime为null的情况）
            string startTimeStr = StartTime?.ToString("yyyyMMddHHmmss") ?? "StartTime";
            string endTimeStr = EndTime?.ToString("yyyyMMddHHmmss") ?? "EndTime";
            string fileName = $"{startTimeStr}~{endTimeStr}.csv";
            // 拼接最终文件路径
            string fullFilePath = Path.Combine(outputDir, fileName);

            // 异步写入文件（使用File的异步方法，避免阻塞）
            await File.WriteAllTextAsync(fullFilePath, csvContentBuilder.ToString(), Encoding.UTF8, cancellationToken);
            #endregion
        }

        private async void PageUpdated(FunctionEventArgs<int> info)
        {
            PageIndex = info.Info;
            await QueryAsync();
        }

        public string Title => "实时信息";

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
