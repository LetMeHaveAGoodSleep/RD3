using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
namespace XZ.SQLite
{
public class EnhancedSqliteBackupService : IDisposable
    {
        private readonly string _sourceDbPath;
        private readonly string _backupRootPath;
        private Timer _backupTimer;
        private DateTime _lastBackupDate;
        private readonly object _backupLock = new object();
        private readonly int _maxRetryAttempts = 3;
        private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(30);

        public EnhancedSqliteBackupService(string sourceDbPath, string backupRootPath)
        {
            _sourceDbPath = sourceDbPath ?? throw new ArgumentNullException(nameof(sourceDbPath));
            _backupRootPath = backupRootPath ?? throw new ArgumentNullException(nameof(backupRootPath));

            // 确保根目录存在
            Directory.CreateDirectory(_backupRootPath);
        }

        public void Start()
        {
            // 立即执行第一次备份
            Task.Run(() => PerformBackupWithRetry());

            // 设置每小时执行一次的定时器
            _backupTimer = new Timer(_ => Task.Run(() => PerformBackupWithRetry()),
                                  null,
                                  TimeSpan.FromMinutes(60),
                                  TimeSpan.FromMinutes(60));
        }

        private async Task PerformBackupWithRetry()
        {
            int attempt = 0;
            bool success = false;

            while (attempt < _maxRetryAttempts && !success)
            {
                attempt++;

                try
                {
                    PerformBackup();
                    success = true;
                    Log("备份成功完成");
                }
                catch (IOException ioEx) when (ioEx.Message.Contains("used by another process"))
                {
                    Log($"文件被锁定，尝试 {attempt}/{_maxRetryAttempts}，将在 {_retryDelay.TotalSeconds} 秒后重试");
                    if (attempt < _maxRetryAttempts)
                    {
                        await Task.Delay(_retryDelay);
                    }
                }
                catch (SQLiteException sqlEx)
                {
                    Log($"数据库错误: {sqlEx.Message}");
                    break; // 数据库错误通常不会通过重试解决
                }
                catch (Exception ex)
                {
                    Log($"备份失败(尝试 {attempt}/{_maxRetryAttempts}): {ex.Message}");
                    break; // 非IO异常通常不会通过重试解决
                }
            }

            if (!success)
            {
                Log($"备份失败，已达到最大重试次数 {_maxRetryAttempts}");
            }
        }

        private void PerformBackup()
        {
            // 使用锁确保同一时间只有一个备份操作
            lock (_backupLock)
            {
                // 获取当前日期作为子文件夹名
                string dateFolder = DateTime.Now.ToString("yyyyMMdd");
                string dailyBackupPath = Path.Combine(_backupRootPath, dateFolder);

                // 创建日期文件夹
                Directory.CreateDirectory(dailyBackupPath);

                // 生成带时间戳的备份文件名
                string timestamp = DateTime.Now.ToString("HHmmss");
                string dbName = Path.GetFileNameWithoutExtension(_sourceDbPath);
                string backupFileName = $"{dbName}_Backup_{timestamp}.db";
                string backupPath = Path.Combine(dailyBackupPath, backupFileName);
                try
                {
                    File.Copy(_sourceDbPath, backupPath, overwrite: true);

                    var flag = VerifyBackup(backupPath);

                    // 如果是新的一天，清理前一天的备份
                    if (_lastBackupDate.Date < DateTime.Today)
                    {
                        CleanupOldBackups();
                        _lastBackupDate = DateTime.Now;
                    }
                }
                catch
                {
                    // 如果备份失败，删除可能已创建的部分备份文件
                    if (File.Exists(backupPath))
                    {
                        try { File.Delete(backupPath); } catch { /* 忽略删除错误 */ }
                    }

                    throw; // 重新抛出异常
                }
            }
        }

        private static bool VerifyBackup(string backupPath)
        {
            try
            {
                // 尝试打开备份文件验证完整性
                using (var conn = new SQLiteConnection($"Data Source={backupPath};Version=3;"))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("PRAGMA quick_check;", conn))
                    {
                        string result = cmd.ExecuteScalar().ToString();
                        return result.Equals("ok", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                // 获取所有日期子文件夹
                var dateFolders = Directory.GetDirectories(_backupRootPath)
                    .Select(f => new
                    {
                        Path = f,
                        Date = DateTime.ParseExact(Path.GetFileName(f), "yyyyMMdd", null)
                    })
                    .OrderByDescending(f => f.Date)
                    .ToList();

                // 保留今天的备份，删除其他所有日期的备份
                foreach (var folder in dateFolders.Where(f => f.Date < DateTime.Today))
                {
                    try
                    {
                        Directory.Delete(folder.Path, true);
                        Log($"已删除旧备份文件夹: {folder.Path}");
                    }
                    catch (Exception ex)
                    {
                        Log($"删除旧备份文件夹失败: {folder.Path}, 错误: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"清理旧备份失败: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            string logPath = Path.Combine(_backupRootPath, "backup_log.txt");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";

            try
            {
                // 使用文件共享模式写入日志，避免锁定
                using (var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(logEntry);
                }
            }
            catch
            {
                // 日志写入失败处理
                Console.WriteLine($"无法写入日志: {logEntry}");
            }
        }

        public void Dispose()
        {
            _backupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

