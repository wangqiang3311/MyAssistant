﻿#pragma checksum "..\..\..\UpdateManage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0343C2CD5ECE6DD92E6D4CD579A1321F7FFA25C8"
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
    /// UpdateManage
    /// </summary>
    public partial class UpdateManage : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 28 "..\..\..\UpdateManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox cbxAll;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\UpdateManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid ItemProject;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\UpdateManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button batStart;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\UpdateManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button batStop;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\UpdateManage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button batUpdate;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MyAssistant;component/updatemanage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UpdateManage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 3:
            this.cbxAll = ((System.Windows.Controls.CheckBox)(target));
            
            #line 28 "..\..\..\UpdateManage.xaml"
            this.cbxAll.Click += new System.Windows.RoutedEventHandler(this.cbxAll_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ItemProject = ((System.Windows.Controls.DataGrid)(target));
            
            #line 32 "..\..\..\UpdateManage.xaml"
            this.ItemProject.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this.ItemProject_MouseDoubleClick);
            
            #line default
            #line hidden
            
            #line 32 "..\..\..\UpdateManage.xaml"
            this.ItemProject.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ItemProject_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.batStart = ((System.Windows.Controls.Button)(target));
            
            #line 44 "..\..\..\UpdateManage.xaml"
            this.batStart.Click += new System.Windows.RoutedEventHandler(this.batStart_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.batStop = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\..\UpdateManage.xaml"
            this.batStop.Click += new System.Windows.RoutedEventHandler(this.batStop_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.batUpdate = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\..\UpdateManage.xaml"
            this.batUpdate.Click += new System.Windows.RoutedEventHandler(this.batUpdate_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 14 "..\..\..\UpdateManage.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.btnControl_Click);
            
            #line default
            #line hidden
            break;
            case 2:
            
            #line 15 "..\..\..\UpdateManage.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.btnUpdate_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}
