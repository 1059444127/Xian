﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClearCanvas.Web.Services {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class ApplicationServiceSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ApplicationServiceSettings defaultInstance = ((ApplicationServiceSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ApplicationServiceSettings())));
        
        public static ApplicationServiceSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int MaximumSimultaneousApplications {
            get {
                return ((int)(this["MaximumSimultaneousApplications"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("256")]
        public int MinimumFreeMemoryMB {
            get {
                return ((int)(this["MinimumFreeMemoryMB"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int SessionPollingIntervalSeconds {
            get {
                return ((int)(this["SessionPollingIntervalSeconds"]));
            }
        }
    }
}
