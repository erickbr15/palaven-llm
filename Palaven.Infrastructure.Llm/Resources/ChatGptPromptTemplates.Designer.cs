﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Palaven.Infrastructure.Llm.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ChatGptPromptTemplates {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ChatGptPromptTemplates() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Palaven.Infrastructure.Llm.Resources.ChatGptPromptTemplates", typeof(ChatGptPromptTemplates).Assembly);
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
        ///   Looks up a localized string similar to You are a very capable AI assitant that will answer the user query using the additional provided information.
        ///If there is any additional information it will be enclosed by the tags &lt;additional_info&gt;&lt;/additional_info&gt;.
        ///Only answer what the user is asking. Use the additional information to generate an answer.
        ///Do not pay attention to math formulas.
        ///If there is no additional information then try to do your best and answer the user query. 
        ///If you do not know the answer just response &quot;I don&apos;t know&quot;.
        /// </summary>
        internal static string ChatGPTPromptSystemRole {
            get {
                return ResourceManager.GetString("ChatGPTPromptSystemRole", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Query: {user_query}
        ///{additional_info}.
        /// </summary>
        internal static string ChatGPTPromptUserRole {
            get {
                return ResourceManager.GetString("ChatGPTPromptUserRole", resourceCulture);
            }
        }
    }
}
