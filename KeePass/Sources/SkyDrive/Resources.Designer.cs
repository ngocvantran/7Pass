﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18046
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KeePass.Sources.SkyDrive {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KeePass.Sources.SkyDrive.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to client_id={0}&amp;redirect_uri={1}&amp;client_secret={2}&amp;code={3}&amp;grant_type=authorization_code.
        /// </summary>
        internal static string AuthTokenData {
            get {
                return ResourceManager.GetString("AuthTokenData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://login.live.com/oauth20_token.srf.
        /// </summary>
        internal static string AuthTokenUrl {
            get {
                return ResourceManager.GetString("AuthTokenUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://login.live.com/oauth20_authorize.srf?client_id={0}&amp;scope=wl.skydrive_update%20wl.offline_access&amp;response_type=code&amp;redirect_uri={1}&amp;display=Touch&amp;theme={2}.
        /// </summary>
        internal static string AuthUrl {
            get {
                return ResourceManager.GetString("AuthUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to client_id={0}&amp;redirect_uri={2}&amp;grant_type=refresh_token&amp;refresh_token={3}
        ///.
        /// </summary>
        internal static string TokenRefreshData {
            get {
                return ResourceManager.GetString("TokenRefreshData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://oauth.live.com/token?client_id={0}&amp;client_secret={1}&amp;redirect_uri={2}&amp;grant_type=refresh_token&amp;refresh_token={3}.
        /// </summary>
        internal static string TokenRefreshUrl {
            get {
                return ResourceManager.GetString("TokenRefreshUrl", resourceCulture);
            }
        }
    }
}
