   M S B A M L   `   `   `  ����>  :RD3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:RD3.Views	RD3.Views  5      /clr-namespace:RD3.ExtensionsRD3.Extensions  5          RD3.Views.LoginView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      _ [Microsoft.Xaml.Behaviors, Version=1.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a3i+http://schemas.microsoft.com/xaml/behaviors  5      #localclr-namespace:RD3.Views  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5	      _ [MaterialDesignThemes.Wpf, Version=5.0.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962a:md1http://materialdesigninxaml.net/winfx/xaml/themes  5
      'passclr-namespace:RD3.Extensions  5      [	 WPrism.DryIoc.Wpf, Version=8.1.97.5141, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59T
 PPrism.Wpf, Version=8.1.97.5141, Culture=neutral, PublicKeyToken=40ee6c3a2184dc59&prismhttp://prismlibrary.com/ 	 
 5      $	��600��5      $	��350��5      # 
 Prism.Services.Dialogs.Dialog    WindowStyle  5      �� 5   
    �� 
TargetType" �6   .�5      ��  �� Property � �� �� Value$	 600��6&   �� 5       � ��$	 350��6'   �� 5       �@�SizeToContent$ WidthAndHeight=�6.   �� 5       �@�
ResizeMode$ NoResize=�6+   �� 5       �@ WindowStartupLocation$ CenterScreen=�6C   5      .��5      � 
������� � ColumnDefinitions 5   
   �� 5      $
��1.5* �6   �� 5      .+�5   
   '  !MaterialDesignThemes.Wpf.Snackbar  -   5          Name$ LoginSnakeBar��5       $��2��5!       K� ZIndex$ 1��5"         MessageQueue 5#      4  .MaterialDesignThemes.Wpf.MessageQueueExtension  �� 5%   
   $��50q�6   $��/Images/login.png��6   7  1MaterialDesignThemes.Wpf.Transitions.Transitioner  5'   
   $��1��6    �� SelectedIndex 6*   �� *SelectIndex+	 �� FallbackValue$	 0��.@�5(      <  6MaterialDesignThemes.Wpf.Transitions.TransitionerSlide  .��5)      ]� $��15q�6   $��Center=�6(   .+�5*      �� $
��0,10q�5+      $	��Top=�5,      $��22(�5-      $
��Bold"�5.      $��欢迎使用��5/      �� 50      $
��0,10q�51      $��Center=�52      '  !HandyControl.Controls.InfoElement
   Placeholder$
 请输入账号��53      $	��Top=�54         TextBoxExtend#��[  55      ��56      �� *
UserName+G� 57      $
��0,10q�58      $
 请输入密码��59      -  'HandyControl.Controls.PasswordBoxAttach   PasswordLength$ 0��5:      $	��Top=�5<       G� PasswordChar$ *��5=        PasswordBoxExtend#���  5>      '   !RD3.Extensions.PassWordExtensions   PassWord 5;      �� *
PassWord+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�*	  $Microsoft.Xaml.Behaviors.Interaction 	  	Behaviors 5?      %
   RD3.Extensions.PasswordBehavior
  5@      5B      �� 5D      $��Login��5F      $��登录系统��5G      $	��Top=�5H      ��5E      �� *ExecuteCommand+5H      ]� 5J      $	��0,5q�5K      $��False��5L      $y�Hidden=�5M      .+�5N      �� $��注册账号��6$    	  Triggers 5O      +  %Microsoft.Xaml.Behaviors.EventTrigger  5P   "      	EventName$ MouseLeftButtonDown��61   *  $Microsoft.Xaml.Behaviors.TriggerBase   Actions. 5Q   &   2  ,Microsoft.Xaml.Behaviors.InvokeCommandAction     CommandParameter$ ResgiterPage��6_      Command 6<   �� *ExecuteCommand+6_   5R   #   5T      �� 5U      $��Right=�6$   $��忘记密码?��6;   5V      5W      5X        5Z      .��5[      ]� $��15q�6   $��Center=�6(   .+�5\      �� $
��0,10q�5]      $	��Top=�5^      $��22(�5_      $
��Bold"�5`      $��注册账号��5a      �� 5c      $	��0,5q�5d      )  #MaterialDesignThemes.Wpf.HintAssist   Hint$ 请输入用户名��5e      $	��Top=�5f      ��5g      �� *UserDto.Account+�� 5h      $	��0,5q�5i      $ 请输入账号��5j      $	��Top=�5k      ��5l      �� *UserDto.UserName+G� 5n      $	��0,5q�5o      $ 请输入密码��5p      $	��Top=�5r       5q      �� *UserDto.PassWord+$ TwoWay=�$ PropertyChanged=� 5s      
  5t      5v      G� 5x      $	��0,5q�5y      $ 请再次输入密码��5z      $	��Top=�5|       5{      �� *UserDto.NewPassWord+$ TwoWay=�$ PropertyChanged=� 5}      
  5~      5�      �� 5�      $��Resgiter��5�      $��注册账号��5�      $	��Top=�5�      ��5�      �� *ExecuteCommand+5�      �� 5�      $
��0,10q�5�      $��Return��5�      $��返回登录��5�      $	��Top=�5�        ButtonPrimary#��[ 5�      ��5�      �� *ExecuteCommand+5�      5�      5�      5�      5�      5�      