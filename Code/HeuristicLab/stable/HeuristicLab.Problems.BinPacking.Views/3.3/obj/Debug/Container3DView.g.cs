﻿#pragma checksum "..\..\Container3DView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F9BC96A128D5BBEB8FFCDFB3B574A0209FCD10A1AE7B1EAF58330E658D297E50"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace HeuristicLab.Problems.BinPacking.Views {
    
    
    /// <summary>
    /// Container3DView
    /// </summary>
    public partial class Container3DView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 36 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewport3D viewport3D1;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.PerspectiveCamera camMain;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DirectionalLight dirLightMain;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.ModelVisual3D MyModel;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.ScaleTransform3D scale;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.RotateTransform3D rotateX;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.RotateTransform3D rotateY;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\Container3DView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.ScaleTransform3D scaleZoom;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/HeuristicLab.Problems.BinPacking.Views-3.3;component/container3dview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Container3DView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 29 "..\..\Container3DView.xaml"
            ((HeuristicLab.Problems.BinPacking.Views.Container3DView)(target)).MouseMove += new System.Windows.Input.MouseEventHandler(this.Container3DView_MouseMove);
            
            #line default
            #line hidden
            
            #line 29 "..\..\Container3DView.xaml"
            ((HeuristicLab.Problems.BinPacking.Views.Container3DView)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Container3DView_MouseDown);
            
            #line default
            #line hidden
            
            #line 29 "..\..\Container3DView.xaml"
            ((HeuristicLab.Problems.BinPacking.Views.Container3DView)(target)).MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.Container3DView_MouseUp);
            
            #line default
            #line hidden
            
            #line 30 "..\..\Container3DView.xaml"
            ((HeuristicLab.Problems.BinPacking.Views.Container3DView)(target)).MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.Container3DView_OnMouseWheel);
            
            #line default
            #line hidden
            
            #line 31 "..\..\Container3DView.xaml"
            ((HeuristicLab.Problems.BinPacking.Views.Container3DView)(target)).MouseEnter += new System.Windows.Input.MouseEventHandler(this.Container3DView_OnMouseEnter);
            
            #line default
            #line hidden
            return;
            case 2:
            this.viewport3D1 = ((System.Windows.Controls.Viewport3D)(target));
            return;
            case 3:
            this.camMain = ((System.Windows.Media.Media3D.PerspectiveCamera)(target));
            return;
            case 4:
            this.dirLightMain = ((System.Windows.Media.Media3D.DirectionalLight)(target));
            return;
            case 5:
            this.MyModel = ((System.Windows.Media.Media3D.ModelVisual3D)(target));
            return;
            case 6:
            this.scale = ((System.Windows.Media.Media3D.ScaleTransform3D)(target));
            return;
            case 7:
            this.rotateX = ((System.Windows.Media.Media3D.RotateTransform3D)(target));
            return;
            case 8:
            this.rotateY = ((System.Windows.Media.Media3D.RotateTransform3D)(target));
            return;
            case 9:
            this.scaleZoom = ((System.Windows.Media.Media3D.ScaleTransform3D)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

