﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SSASDiag.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SSASDiag.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Icon similar to (Icon).
        /// </summary>
        internal static System.Drawing.Icon Microsoft_107 {
            get {
                object obj = ResourceManager.GetObject("Microsoft_107", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] Microsoft_AnalysisServices {
            get {
                object obj = ResourceManager.GetObject("Microsoft_AnalysisServices", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] Microsoft_AnalysisServices_Core {
            get {
                object obj = ResourceManager.GetObject("Microsoft_AnalysisServices_Core", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] Microsoft_SqlServer_ConnectionInfo {
            get {
                object obj = ResourceManager.GetObject("Microsoft_SqlServer_ConnectionInfo", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] Microsoft_SqlServer_ConnectionInfoExtended {
            get {
                object obj = ResourceManager.GetObject("Microsoft_SqlServer_ConnectionInfoExtended", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Batch xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot; xmlns:soap=&quot;http://schemas.xmlsoap.org/soap/envelope/&quot;&gt;
        ///	&lt;Create xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot;&gt;
        ///		&lt;ObjectDefinition&gt;
        ///			&lt;Trace&gt;
        ///				&lt;LogFileName/&gt;
        ///				&lt;ID/&gt;
        ///				&lt;LogFileSize/&gt;&lt;LogFileRollover/&gt;
        ///				&lt;Name/&gt;
        ///				&lt;AutoRestart/&gt;&lt;StartTime/&gt;&lt;StopTime/&gt;
        ///				&lt;LogFileAppend&gt;false&lt;/LogFileAppend&gt;
        ///				&lt;Events&gt;
        ///					&lt;Event&gt;
        ///						&lt;EventID&gt;15&lt;/EventID&gt;
        ///						&lt;Columns&gt;
        ///							&lt;ColumnID&gt;32&lt;/ColumnID&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ProfilerTraceStartXMLA {
            get {
                return ResourceManager.GetString("ProfilerTraceStartXMLA", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Delete xmlns=&quot;http://schemas.microsoft.com/analysisservices/2003/engine&quot; xmlns:soap=&quot;http://schemas.xmlsoap.org/soap/envelope/&quot;&gt;
        ///	&lt;Object&gt;
        ///		&lt;TraceID/&gt;
        ///	&lt;/Object&gt;
        ///&lt;/Delete&gt;.
        /// </summary>
        internal static string ProfilerTraceStopXMLA {
            get {
                return ResourceManager.GetString("ProfilerTraceStopXMLA", resourceCulture);
            }
        }
    }
}
