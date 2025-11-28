using Microsoft.Win32.TaskScheduler;
using RD3.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace RD3
{
    /// <summary>
    /// 任务计划程序自启工具类（替代注册表）
    /// </summary>
    public static class TaskSchedulerAutoStartHelper
    {
        // 任务唯一名称（不可重复）
        private const string TaskName = "RD3.Pad.AutoStart";

        /// <summary>
        /// 获取程序EXE的完整路径
        /// </summary>
        private static string AppExePath
        {
            get
            {
                try
                {
                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    return Path.GetFullPath(exePath);
                }
                catch
                {
                    var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
                    return entryAssembly != null ? Path.GetFullPath(entryAssembly.Location) : string.Empty;
                }
            }
        }

        /// <summary>
        /// 检查是否已配置任务计划自启
        /// </summary>
        /// <returns>是否开启自启</returns>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (var ts = new TaskService())
                {
                    // 检查任务是否存在且启用
                    var task = ts.GetTask(TaskName);
                    return task != null && task.State == TaskState.Ready;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置任务计划自启
        /// </summary>
        /// <param name="enable">是否开启自启</param>
        /// <returns>是否设置成功</returns>
        public static bool SetAutoStart(bool enable)
        {
            // 验证程序路径是否有效
            if (enable && (string.IsNullOrEmpty(AppExePath) || !File.Exists(AppExePath)))
            {
                return false;
            }

            try
            {
                using (var ts = new TaskService())
                {
                    if (enable)
                    {
                        // 1. 创建任务定义
                        var taskDefinition = ts.NewTask();
                        taskDefinition.RegistrationInfo.Description = "RD3程序开机自启任务";

                        // 2. 配置触发器：登录时启动，延迟10秒
                        var trigger = new LogonTrigger
                        {
                            Delay = TimeSpan.FromSeconds(10), // 延迟启动，避开系统高峰
                            Enabled = true
                        };
                        taskDefinition.Triggers.Add(trigger);

                        // 3. 配置操作：启动程序（强制设置工作目录）
                        var action = new ExecAction
                        (
                            AppExePath, // 程序EXE路径
                            arguments: null, // 启动参数（无则为null）
                            workingDirectory: Path.GetDirectoryName(AppExePath) // 强制工作目录为EXE所在目录
                        );
                        taskDefinition.Actions.Add(action);

                        // 4. 配置任务权限（关键）
                        taskDefinition.Principal.RunLevel = TaskRunLevel.Highest; // 最高权限
                        taskDefinition.Settings.AllowDemandStart = true; // 允许手动运行
                        taskDefinition.Settings.Enabled = true;
                        taskDefinition.Settings.RestartCount = 3; // 启动失败重试3次
                        taskDefinition.Settings.RestartInterval = TimeSpan.FromMinutes(1); // 1分钟间隔
                        taskDefinition.Settings.StopIfGoingOnBatteries = false; // 电池模式不停止
                        taskDefinition.Settings.WakeToRun = false; // 不唤醒计算机

                        // 5. 注册任务（到根目录，便于查找）
                        ts.RootFolder.RegisterTaskDefinition
                        (
                            TaskName,
                            taskDefinition,
                            TaskCreation.CreateOrUpdate, // 存在则更新，不存在则创建
                            null, // 任务运行的用户（null为当前用户）
                            null, // 用户密码（null为当前用户密码）
                            TaskLogonType.InteractiveToken // 交互登录（显示界面）
                        );
                        LogHelper.Debug("开机自启设置成功！");
                        return true;
                    }
                    else
                    {
                        // 删除任务
                        if (ts.GetTask(TaskName) != null)
                        {
                            ts.RootFolder.DeleteTask(TaskName);
                        }
                        LogHelper.Debug("已关闭开机自启！");
                        return true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}