﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.MQExplorerPlus.Core.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ExportMessagesSettingsView.xaml
    /// </summary>
    /// 
    [Export(typeof(IExportMessagesSettingsView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ExportMessagesSettingsView : UserControl , IExportMessagesSettingsView
    {
        public ExportMessagesSettingsView()
        {
            InitializeComponent();
        }
    }
}