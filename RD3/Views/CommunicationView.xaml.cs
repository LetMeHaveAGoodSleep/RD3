﻿using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// CommunicationView.xaml 的交互逻辑
    /// </summary>
    public partial class CommunicationView : UserControl
    {
        public CommunicationView()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ((CommunicationViewModel)this.DataContext)?.CloseCommand.Execute();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            ClientConfig client = dataGrid.SelectedItem as ClientConfig;
            ((CommunicationViewModel)this.DataContext)?.EditCommand.Execute(client);
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            ClientConfig client = dataGrid.SelectedItem as ClientConfig;
            ((CommunicationViewModel)this.DataContext)?.DeleteCommand.Execute(client);
        }
    }
}
