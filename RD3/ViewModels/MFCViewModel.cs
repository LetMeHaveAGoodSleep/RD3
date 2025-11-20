using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Controller;
using RD3.Shared;
using RD3.Views;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class MFCViewModel : BaseViewModel
    {
        private MFCController _controller = new();
        public MFCController Controller
        {
            get => _controller;
            private set { SetProperty(ref _controller, value); }
        }

        private MFCInfo _mfcInfo = new();
        public MFCInfo MFCInfo
        {
            get => _mfcInfo;
            set
            {
                SetProperty(ref _mfcInfo, value);
                Controller = new();
                Controller.MFCInfo = value;
            }
        }


        public DelegateCommand MFCSettingCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {nameof(MFCInfo),MFCInfo }
            };
            DialogHostService.ShowOnce(nameof(MFCSettingView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
            });
        });

        public MFCViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            Thread thread = new Thread(new ThreadStart(() => 
            {
                while (true)
                {
                    if (MFCInfo.MFCIndex != -1)
                    {
                        try
                        {
                            string deviceID = "G01";
                            foreach (var item in ClockSupervisor.realDatasDic.Keys)
                            {
                                deviceID = item;
                                break;
                            }
                            if (!ClockSupervisor.realDatasDic.ContainsKey(deviceID) || ClockSupervisor.realDatasDic[deviceID].Count < 1)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }
                            int index = ClockSupervisor.realDatasDic[deviceID].Count - 1;
                            var realTimeParam = ClockSupervisor.realDatasDic[deviceID][index];

                            var type = realTimeParam.GetType();
                            var properties = type.GetProperties();
                            foreach (var item in properties.Where(t => t.CanRead && t.CanWrite))
                            {
                                if (item.Name == $"MFC{MFCInfo.MFCIndex}FlowRate")
                                {
                                    object value = item.GetValue(realTimeParam);
                                    MFCInfo.FlowRate = Convert.ToSingle(value);
                                }
                                else if (item.Name == $"MFC{MFCInfo.MFCIndex}FlowCapacity")
                                {
                                    object value = item.GetValue(realTimeParam);
                                    MFCInfo.FlowCapacity = Convert.ToSingle(value);
                                }
                            }
                        }
                        catch (Exception ex) 
                        { 

                        }
                    }
                    Thread.Sleep(1000);
                }
            }));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();


            Thread thread1 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Controller.MFCInfo != null)
                        {
                            if (Controller.MFCInfo.IsControling && !Controller.MFCInfo.LastIsControling)
                            {
                                Controller.StartWork();
                            }
                            else if (Controller.MFCInfo.LastIsControling && !Controller.MFCInfo.IsControling)
                            {
                                Controller.StopWork();
                            }
                            Controller.MFCInfo.IsControling = Controller.MFCInfo.IsControling;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            }));
            thread1.IsBackground = true;
            thread1.Priority = ThreadPriority.BelowNormal;
            thread1.Start();
        }
    }
}
