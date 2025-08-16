using CustomApp;
using DryIoc;
using Fpi.Communication.Interfaces;
using Fpi.Xml;
using ImTools;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using XZ.OnnxRun;

namespace RD3.ViewModels
{
    /// <summary>
    /// 新添加-hdb
    /// </summary>
    public class CameraImageViewModel : BaseViewModel,IDialogAware
    {
        public XCloudSDK.PXSDK_MessageCallBack _mainCallback;//回调函数

        private bool isPause = false;

        private string _deviceInfo;

        public int _hUser = 0;//用户句柄

        public int _hRealPlay = 0;//预览播放句柄

        private ObservableCollection<CameraSetting> _cameraSettingCol = [];
        public ObservableCollection<CameraSetting> CameraSettingCol
        {
            get { return _cameraSettingCol; }
            set { SetProperty(ref _cameraSettingCol, value); }
        }

        private string onnxPath;
        private OnnxRuner onnxRuner;

        public string Title => "视频监测";

        public event Action<IDialogResult> RequestClose;

        private BitmapImage _img;
        public BitmapImage Img
        {
            get => _img;
            set { _img = value; }
        }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        private bool _recording = false;
        public bool Recording
        {
            get => _recording;
            set { SetProperty(ref _recording, value); }
        }

        private bool _connected = false;
        public bool Connected
        {
            get => _connected;
            set { SetProperty(ref _connected, value); }
        }

        public DelegateCommand AFCommand => new(() => 
        {
            int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AFPump);
            if (pumpNo < 0) return;
            PeristalticPumpControlParam param = new()
            {
                PumpNo = pumpNo,
                Pump = PeristalticPump.AFPump,
                ControlMode = PumpControlMode.Direct,
                FlowSpeed = CurrentDeviceParameter.DefoamingSetting.FlowSpeed,
                FlowCapacity = CurrentDeviceParameter.DefoamingSetting.FlowCapacity
            };
            CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
        });

        public CameraImageViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            onnxPath = FileConst.DataDirectory + "\\xp.onnx";
            onnxRuner = new OnnxRuner(onnxPath);

            InitSDK();

            CameraSettingCol = CameraSettingManager.GetInstance().CameraCol;
        }


        //SDK初始化
        public void InitSDK()
        {
            //Config.ini里填写初始化相关的参数：开发者鉴权信息、配置文件目录等
            string sInit = "{\"LogLevel\":8,\"TempPath\":\"\",\"ConfigPath\":\"\",\"PlatUUID\":\"\",\"PlatAppKey\":\"\",\"PlatAppSecret\":\"\",\"PlatMovedCard\":0,\"ServerIP\":\"\",\"ServerPort\":34567,\"InitType\":0}";
            if (File.Exists("./Config.ini"))
            {
                //如果是合法的Json，就读取配置文件，否则使用默认参数
                string sFileContext = File.ReadAllText("./Config.ini");
                if (sFileContext.StartsWith("{") && sFileContext.EndsWith("}"))
                {
                    sInit = sFileContext;
                }
                else
                {
                    File.WriteAllText("./Config.ini", sInit);
                }
            }
            else
            {
                File.WriteAllText("./Config.ini", sInit);
            }
            Directory.CreateDirectory("./Record");

            XCloudSDK.XCloudSDK_Init(sInit);
            _mainCallback = new XCloudSDK.PXSDK_MessageCallBack(XCloudSDKCallBack);
            _hUser = XCloudSDK.XCloudSDK_RegisterCallback(_mainCallback, IntPtr.Zero);

            // 60005:指定渲染方式，默认使用DirectX
            // 1:DirectX  2:OpenGL  3:GDI
            // XCloudSDK.XSDK_SetSDKIntAttr(60005, 2);

        }

        // 回调函数
        unsafe int XCloudSDKCallBack(System.Int32 hObject, int nMsgId, int nParam1, int nParam2, int nParam3, string szString, IntPtr pObject, Int64 lParam, int nSeq, IntPtr pUserData, IntPtr pMsg)
        {
            Console.WriteLine("MsgId:" + nMsgId + " P1:" + nParam1 + " P2:" + nParam2 + " P3:" + nParam3 + "Seq:" + nSeq);
            Console.WriteLine("Str:" + szString.ToString());

            //如果需要处理回调来的数据，在这里操作

            //实时预览结果回调
            if (nMsgId == (int)ESXSDK_CMD.ESXSDK_MEDIA_START_REAL_PLAY)
            {
                if (nParam1 >= 0)
                {
                    _connected = true;
                    //预览打开成功
                }
                else
                {
                    _connected = false;
                    MessageBox.Show(string.Format("错误信息：{0}", CameraAlarmConfig.GetValue(nParam1.ToString())));
                }
            }

            //抓图&录像结果回调
            if (nMsgId == (int)EUIMSG.EUIMSG_PLAY_SAVE_IMAGE_FILE || nMsgId == (int)EUIMSG.EUIMSG_RECORD_STOP)
            {
                if (nParam1 >= 0)
                {
                    //MessageBox.Show("Success");
                }
                else
                {
                    MessageBox.Show(string.Format("错误信息：{0}", CameraAlarmConfig.GetValue(nParam1.ToString())));
                    //MessageBox.Show("Error : " + CameraAlarmConfig.GetValue(nParam1.ToString()));
                }
            }
            return 0;
        }

        //实时预览
        public void RealPlay(string sDevId)
        {
            _hRealPlay = XCloudSDK.XCloudSDK_Device_MediaRealPlay(_hUser, sDevId, 0, 0, AppSession.CameraHandle, 0, "");
        }

        private void ImageReceiver_RecieveImageEvent(string name, byte[] imageBytes)
        {
            // if (CurrentDeviceParameter.Name != name) return;
            if (CurrentDeviceParameter.Name.Contains(name) || name.Contains(CurrentDeviceParameter.Name))
            {
                string dateTimeStr = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                string imageFileName = imageFileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\CameraImages\\PIC00022.jpg";//测试图片
                if (imageBytes != null)
                {
                    imageFileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\CameraImages\\{dateTimeStr}.jpg";
                    if (!File.Exists(imageFileName))
                    {
                        File.Create(imageFileName).Close();
                    }
                    //using (MemoryStream ms = new MemoryStream(imageBytes, true))//接收并保存图片
                    //{
                    //    System.Drawing.Image image = System.Drawing.Image.FromStream(ms); 
                    //    image.Save(imageFileName, ImageFormat.Jpeg);
                    //}

                    using (var ms = new MemoryStream(imageBytes))
                    {
                        try
                        {
                            // 验证图像格式（可选）
                            var image = Image.FromStream(ms, true, true);
                            Image bitmap= new Bitmap(image); // 创建独立副本，避免原流关闭后失效
                            bitmap.Save(imageFileName, ImageFormat.Jpeg);
                        }
                        catch (ArgumentException ex)
                        {
                            // 处理无效图像数据（如损坏文件）
                            throw new InvalidOperationException("无法从字节流创建图像", ex);
                        }
                    }
                }
                else //测试分支
                {
                    if (!File.Exists(imageFileName))//如果没有测试图片则跳出
                        return;
                }

                string resultFileName = AppDomain.CurrentDomain.BaseDirectory + "CameraImages\\Processed\\Json\\" + dateTimeStr + ".json";
                onnxRuner.Prediction(imageFileName, onnxPath, resultFileName);//视觉检测消泡

                if (onnxRuner.BoxMsgs.Count == 2)
                {
                    bool defoamingFlag = ((onnxRuner.BoxMsgs[1].Rect.Height + 0.0f) / onnxRuner.BoxMsgs[0].Rect.Height) > CurrentDeviceParameter.DefoamingSetting.DefoamingThreshold ? true : false;

                    if (true)
                    {
                        Bitmap bitmap = new Bitmap(imageFileName);
                        Graphics g = Graphics.FromImage(bitmap);

                        foreach (BoxMessage box in onnxRuner.BoxMsgs)//画方框
                        {
                            g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Red, 3f), new System.Drawing.Rectangle(box.Rect.X, box.Rect.Y, box.Rect.Width, box.Rect.Height));
                        }

                        float fontSize = 30f;
                        fontSize = MathF.Round(fontSize / 2592 * bitmap.Width * 1.5f, 1) ;
                        //写入信息
                        string info = $"采集时间:{dateTimeStr}";
                        System.Drawing.Font font = new System.Drawing.Font("微软雅黑", fontSize, System.Drawing.FontStyle.Bold);
                        int height = (int)g.MeasureString(info, font).Height;
                        int width = 10 + (int)g.MeasureString(info, font).Width;
                        g.DrawString(info, font, new SolidBrush(System.Drawing.Color.Red), new PointF(bitmap.Width - width, height));
                        g.DrawLine(Pens.Red, new PointF(0, 0), new PointF(50, 50));

                        string result = defoamingFlag ? "需要" : "无需";
                        info = $"检测结果:{result}消泡";
                        width = 10 + (int)g.MeasureString(info, font).Width;
                        height = height + (int)g.MeasureString(info, font).Height;
                        g.DrawString(info, font, new SolidBrush(System.Drawing.Color.Red), new PointF(bitmap.Width - width, height));
                        info = string.Format("阈值:{0}", CurrentDeviceParameter.DefoamingSetting.DefoamingThreshold);
                        width = 10 + (int)g.MeasureString(info, font).Width;
                        height = height + (int)g.MeasureString(info, font).Height;
                        g.DrawString(info, font, new SolidBrush(System.Drawing.Color.Red), new PointF(bitmap.Width - width, height));


                        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}\\CameraImages\\Processed\\Images"))
                        {
                            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}\\CameraImages\\Processed\\Images");
                        }
                        string newImageFileName = $"{AppDomain.CurrentDomain.BaseDirectory}\\CameraImages\\Processed\\Images\\{CurrentDeviceParameter.Name}_{dateTimeStr}_1.jpg";
                        bitmap.Save(newImageFileName);
                        imageFileName = newImageFileName;
                    }

                    if (defoamingFlag)//是否自动消泡
                    {
                        if (CurrentDeviceParameter.DefoamingSetting.AutoDefoaming)
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AFPump);
                            if (pumpNo < 0) return;
                            PeristalticPumpControlParam param = new()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.AFPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = CurrentDeviceParameter.DefoamingSetting.FlowSpeed,
                                FlowCapacity = CurrentDeviceParameter.DefoamingSetting.FlowCapacity
                            };
                            CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        }
                        else
                        {
                            CommandWrapper.SetSoundLightAlarm(CurrentDeviceParameter.Name, new AlarmParam()
                            {
                                BlueLightEnable = SwitchMode.Open,
                                BuzzerEnable = SwitchMode.Open,
                                GreenLightEnable = SwitchMode.Open,
                                RedLightEnable = SwitchMode.Open
                            });
                        }
                    }
                }

                aggregator.SendMessage(CurrentDeviceParameter.Name, nameof(OnnxRuner), imageFileName);
            }
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand SnapCommand => new(() => Snap());

        public DelegateCommand VideoCommand => new(() => 
        {
            StartRecord();
        });

        public DelegateCommand StopVideoCommand => new(() =>
        {
            StopRecord();
        });

        public DelegateCommand PauseCommand => new(() =>
        {
            if (!isPause)
            {
                isPause = true;
            }
            else
            {
                isPause = false;
            }
            XCloudSDK.XCloudSDK_Device_MediaPause(_hRealPlay, isPause);
        });

        private void Snap()
        {
            if (_hRealPlay <= 0 || !_connected)
            {
                return;
            }
            DateTime dt = System.DateTime.Now;
            int y = dt.Year;
            int m = dt.Month;
            int d = dt.Day;
            int h = dt.Hour;
            int min = dt.Minute;
            int s = dt.Second;
            string strPath = string.Format(".\\Record\\{0}{1}{2}{3}{4}{5}.jpg", y, m, d, h, min, s);
            XCloudSDK.XCloudSDK_Play_MediaSnapImage(_hRealPlay, strPath);
        }

        private void StartRecord()
        {
            if (_hRealPlay <= 0 || !_connected)
            {
                return;
            }
            DateTime dt = System.DateTime.Now;
            int y = dt.Year;
            int m = dt.Month;
            int d = dt.Day;
            int h = dt.Hour;
            int min = dt.Minute;
            int s = dt.Second;
            string strPath = string.Format(".\\Record\\{0}{1}{2}{3}{4}{5}.mp4", y, m, d, h, min, s);
            XCloudSDK.XCloudSDK_Play_StartRecord(_hRealPlay, strPath);

            Recording = true;
        }

        private void StopRecord()
        {
            if (_hRealPlay <= 0 || !_connected)
            {
                return;
            }
            XCloudSDK.XCloudSDK_Play_StopRecord(_hRealPlay);

            Recording = false;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            CameraSettingManager.GetInstance().Save();

            //XCloudSDK.XCloudSDK_Device_StopMediaPlay(_hRealPlay);
            //XCloudSDK.XCloudSDK_Device_DevLogout(_deviceInfo);
            XCloudSDK.XCloudSDK_Device_DeleteDevsInfo(_deviceInfo);
            _hRealPlay = 0;
            _hUser = 0;

            //XCloudSDK.XCloudSDK_UnInit();
        }
        
        public void OnDialogOpened(IDialogParameters parameters)
        {
            CurrentDeviceParameter = parameters.GetValue<DeviceParameter>(nameof(DeviceParameter));

            var currentCamera = CameraSettingCol.FindFirst(t => t.ReactorId == CurrentDeviceParameter.Name);
            XCloudSDK.XCloudSDK_Device_SetLocalUserNameAndPwd(currentCamera.DeviceInfo, currentCamera.UserName, currentCamera.Password);
            _deviceInfo = currentCamera.DeviceInfo;
            RealPlay(_deviceInfo);
        }
    }

    public class Defoaming
    {
        public float speed;
        public float value;
        public bool autoDefoaming;
        public float threshold;
    }
}
