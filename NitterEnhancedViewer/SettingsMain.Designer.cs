﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NitterEnhancedViewer {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class SettingsMain : global::System.Configuration.ApplicationSettingsBase {
        
        private static SettingsMain defaultInstance = ((SettingsMain)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new SettingsMain())));
        
        public static SettingsMain Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("800")]
        public double MainWindowWidth {
            get {
                return ((double)(this["MainWindowWidth"]));
            }
            set {
                this["MainWindowWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("600")]
        public double MainWindowHeight {
            get {
                return ((double)(this["MainWindowHeight"]));
            }
            set {
                this["MainWindowHeight"] = value;
            }
        }
    }
}
