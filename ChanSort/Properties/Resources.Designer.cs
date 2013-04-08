﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.586
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ChanSort.Ui.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ChanSort.Ui.Properties.Resources", typeof(Resources).Assembly);
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
        
        internal static System.Drawing.Bitmap Donate {
            get {
                object obj = ResourceManager.GetObject("Donate", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Erase all channel data.
        /// </summary>
        internal static string MainForm_btnResetChannelData_Click_Caption {
            get {
                return ResourceManager.GetString("MainForm_btnResetChannelData_Click_Caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WARNING: All analog, DVB-C/T and DVB-S channel and transponder data will be deleted.
        ///You will have to run a full channel scan after loading this file into your TV.
        ///Proceed?.
        /// </summary>
        internal static string MainForm_btnResetChannelData_Click_Message {
            get {
                return ResourceManager.GetString("MainForm_btnResetChannelData_Click_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Supported Files|{0}|All Files (*.*)|*.
        /// </summary>
        internal static string MainForm_FileDialog_OpenFileFilter {
            get {
                return ResourceManager.GetString("MainForm_FileDialog_OpenFileFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}-Files|*{0}|All Files (*.*)|*.
        /// </summary>
        internal static string MainForm_FileDialog_SaveFileFilter {
            get {
                return ResourceManager.GetString("MainForm_FileDialog_SaveFileFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The input file contains multiple records that use the same 
        ///program number. It is possible that the TV will not accept
        ///the changes made by ChanSort.
        ///This is typically caused by running a manual transponder scan.
        ///It is recommended to use a clean input file for any modifications.
        ///To do that, turn Hotel Mode OFF, reset the TV to 
        ///factory defaults, run a new blind channel scan and turn
        ///Hotel Mode back ON, then export a new clean TLL file.
        ///.
        /// </summary>
        internal static string MainForm_LoadFiles_DupeWarningMsg {
            get {
                return ResourceManager.GetString("MainForm_LoadFiles_DupeWarningMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error loading file.
        /// </summary>
        internal static string MainForm_LoadFiles_IOException {
            get {
                return ResourceManager.GetString("MainForm_LoadFiles_IOException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data validation.
        /// </summary>
        internal static string MainForm_LoadFiles_ValidationWarningCap {
            get {
                return ResourceManager.GetString("MainForm_LoadFiles_ValidationWarningCap", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The verification of the file content showed some anomalies. Possible causes are:
        ///- The TV itself created a mess in the channel lists (which happens frequently).
        ///- The file format is partially unknown (e.g. unknown TV model or firmware).
        ///- The file has been edited with a broken program version.
        ///- ChanSort&apos;s validation rules are based on wrong assumptions.
        ///You can continue editing, but it is possibile that your TV will reject the changes..
        /// </summary>
        internal static string MainForm_LoadFiles_ValidationWarningMsg {
            get {
                return ResourceManager.GetString("MainForm_LoadFiles_ValidationWarningMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while loading the TV&apos;s data file:
        ///{0}.
        /// </summary>
        internal static string MainForm_LoadTll_Exception {
            get {
                return ResourceManager.GetString("MainForm_LoadTll_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No plugin found to read/write {0} files..
        /// </summary>
        internal static string MainForm_LoadTll_SerializerNotFound {
            get {
                return ResourceManager.GetString("MainForm_LoadTll_SerializerNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Quell-TLL konnte nicht gefunden werden:
        ///&apos;{0}&apos;.
        /// </summary>
        internal static string MainForm_LoadTll_SourceTllNotFound {
            get {
                return ResourceManager.GetString("MainForm_LoadTll_SourceTllNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are about to restore the backup file. All changes will be lost!
        ///Do you want to continue?.
        /// </summary>
        internal static string MainForm_miRestoreOriginal_ItemClick_Confirm {
            get {
                return ResourceManager.GetString("MainForm_miRestoreOriginal_ItemClick_Confirm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No backup file found: {0}.
        /// </summary>
        internal static string MainForm_miRestoreOriginal_ItemClick_NoBackup {
            get {
                return ResourceManager.GetString("MainForm_miRestoreOriginal_ItemClick_NoBackup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to copy .bak file to {0}.
        /// </summary>
        internal static string MainForm_miRestoreOriginal_Message {
            get {
                return ResourceManager.GetString("MainForm_miRestoreOriginal_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Data modified.
        /// </summary>
        internal static string MainForm_PromptSaveAndContinue_Caption {
            get {
                return ResourceManager.GetString("MainForm_PromptSaveAndContinue_Caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do you want to save the changes?.
        /// </summary>
        internal static string MainForm_PromptSaveAndContinue_Message {
            get {
                return ResourceManager.GetString("MainForm_PromptSaveAndContinue_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Restore order from channel scan.
        /// </summary>
        internal static string MainForm_RestoreScanOrder_Caption {
            get {
                return ResourceManager.GetString("MainForm_RestoreScanOrder_Caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All custom storing will be lost.
        ///Are you sure you want to restore the order from the channel scan?.
        /// </summary>
        internal static string MainForm_RestoreScanOrder_Message {
            get {
                return ResourceManager.GetString("MainForm_RestoreScanOrder_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was an error saving the file. Please make sure that
        ///- you have write permission on the file
        ///- the file is not open in another program
        ///
        ///The error message is:
        ///.
        /// </summary>
        internal static string MainForm_SaveFiles_ErrorMsg {
            get {
                return ResourceManager.GetString("MainForm_SaveFiles_ErrorMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File write error.
        /// </summary>
        internal static string MainForm_SaveFiles_ErrorTitle {
            get {
                return ResourceManager.GetString("MainForm_SaveFiles_ErrorTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occured while writing the TV data file:
        ///{0}.
        /// </summary>
        internal static string MainForm_SaveTllFile_Exception {
            get {
                return ResourceManager.GetString("MainForm_SaveTllFile_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorted TV data file was created successfully..
        /// </summary>
        internal static string MainForm_SaveTllFile_Success {
            get {
                return ResourceManager.GetString("MainForm_SaveTllFile_Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ChanSort Reference List|*.csv|SamToolBox Reference List|*.chl|All Reference Lists|*.csv;*.chl.
        /// </summary>
        internal static string MainForm_ShowOpenReferenceFileDialog_Filter {
            get {
                return ResourceManager.GetString("MainForm_ShowOpenReferenceFileDialog_Filter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Open Reference List.
        /// </summary>
        internal static string MainForm_ShowOpenReferenceFileDialog_Title {
            get {
                return ResourceManager.GetString("MainForm_ShowOpenReferenceFileDialog_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An unexpected error occured:
        ///{0}.
        /// </summary>
        internal static string MainForm_TryExecute_Exception {
            get {
                return ResourceManager.GetString("MainForm_TryExecute_Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;html&gt;
        ///&lt;body&gt;
        ///&lt;p style=&quot;font-family:Arial;font-size:12pt&quot;&gt;PayPal donation page is being opened...&lt;/p&gt;
        ///&lt;p&gt;&lt;/p&gt;
        ///&lt;p style=&quot;font-family:Arial;font-size:12pt&quot;&gt;PayPal Spendenseite wird ge&amp;ouml;ffnet...&lt;/p&gt;
        ///&lt;form action=&quot;https://www.paypal.com/cgi-bin/webscr&quot; method=&quot;post&quot;&gt;
        ///&lt;input type=&quot;hidden&quot; name=&quot;cmd&quot; value=&quot;_s-xclick&quot;&gt;
        ///&lt;input type=&quot;hidden&quot; name=&quot;encrypted&quot; value=&quot;-----BEGIN PKCS7-----MIIHVwYJKoZIhvcNAQcEoIIHSDCCB0QCAQExggEwMIIBLAIBADCBlDCBjjELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAkNBMRYwFAYDVQQHEw1Nb3VudGFpbiBW [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string paypal_button {
            get {
                return ResourceManager.GetString("paypal_button", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New Version.
        /// </summary>
        internal static string UpdateCheck_NotifyAboutNewVersion_Caption {
            get {
                return ResourceManager.GetString("UpdateCheck_NotifyAboutNewVersion_Caption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A newer version is available: v{0}.
        ///Do you want to open the download website?.
        /// </summary>
        internal static string UpdateCheck_NotifyAboutNewVersion_Message {
            get {
                return ResourceManager.GetString("UpdateCheck_NotifyAboutNewVersion_Message", resourceCulture);
            }
        }
    }
}
