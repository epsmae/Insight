﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Insight.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Insight.SvnProvider.SvnProvider,Insight.SvnProvider")]
        public string Provider {
            get {
                return ((string)(this["Provider"]));
            }
            set {
                this["Provider"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\ProjectBase")]
        public string ProjectBase {
            get {
                return ((string)(this["ProjectBase"]));
            }
            set {
                this["ProjectBase"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\Cache")]
        public string Cache {
            get {
                return ((string)(this["Cache"]));
            }
            set {
                this["Cache"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".cs;.xaml")]
        public string ExtensionsToInclude {
            get {
                return ((string)(this["ExtensionsToInclude"]));
            }
            set {
                this["ExtensionsToInclude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\Test\\,UnitTest")]
        public string PathsToExclude {
            get {
                return ((string)(this["PathsToExclude"]));
            }
            set {
                this["PathsToExclude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string PathsToInclude {
            get {
                return ((string)(this["PathsToInclude"]));
            }
            set {
                this["PathsToInclude"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string WorkItemRegEx {
            get {
                return ((string)(this["WorkItemRegEx"]));
            }
            set {
                this["WorkItemRegEx"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public int MaxWorkItemsPerCommitForSummary {
            get {
                return ((int)(this["MaxWorkItemsPerCommitForSummary"]));
            }
            set {
                this["MaxWorkItemsPerCommitForSummary"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int MinCommitsForHotspots {
            get {
                return ((int)(this["MinCommitsForHotspots"]));
            }
            set {
                this["MinCommitsForHotspots"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int MinLinesOfCodeForHotspot {
            get {
                return ((int)(this["MinLinesOfCodeForHotspot"]));
            }
            set {
                this["MinLinesOfCodeForHotspot"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("300")]
        public int MaxItemsInChangesetForChangeCoupling {
            get {
                return ((int)(this["MaxItemsInChangesetForChangeCoupling"]));
            }
            set {
                this["MaxItemsInChangesetForChangeCoupling"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public int MinCouplingForChangeCoupling {
            get {
                return ((int)(this["MinCouplingForChangeCoupling"]));
            }
            set {
                this["MinCouplingForChangeCoupling"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40")]
        public double MinDegreeForChangeCoupling {
            get {
                return ((double)(this["MinDegreeForChangeCoupling"]));
            }
            set {
                this["MinDegreeForChangeCoupling"] = value;
            }
        }
    }
}
