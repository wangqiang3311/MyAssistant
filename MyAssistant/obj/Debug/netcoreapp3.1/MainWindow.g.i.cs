﻿#pragma checksum "..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "FDF2454A1457CF9CCDFC3199994C2676325CF83D"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using MyAssistant;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace MyAssistant {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem MyTask;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem publishManage;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem modbusPackage;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem TCPTools;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem waterUnPackage;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem doTest;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem doReceive;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBoxItem doExport;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.2.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MyAssistant;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.2.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MyTask = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 20 "..\..\..\MainWindow.xaml"
            this.MyTask.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.MyTask_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.publishManage = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 22 "..\..\..\MainWindow.xaml"
            this.publishManage.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.publishManage_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.modbusPackage = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 23 "..\..\..\MainWindow.xaml"
            this.modbusPackage.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.modbusPackage_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.TCPTools = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 24 "..\..\..\MainWindow.xaml"
            this.TCPTools.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.TCPTools_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.waterUnPackage = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 25 "..\..\..\MainWindow.xaml"
            this.waterUnPackage.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.waterUnPackage_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.doTest = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 26 "..\..\..\MainWindow.xaml"
            this.doTest.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.doTest_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 7:
            this.doReceive = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 27 "..\..\..\MainWindow.xaml"
            this.doReceive.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.doReceive_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            case 8:
            this.doExport = ((System.Windows.Controls.ListBoxItem)(target));
            
            #line 28 "..\..\..\MainWindow.xaml"
            this.doExport.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.doExport_MouseDoubleClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

