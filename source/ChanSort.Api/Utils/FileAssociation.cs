using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Win32;

namespace ChanSort.Api
{
    #region class FileAssociation
    public class FileAssociation
    {
        public string Extension { get; }
        public string ProgId { get; }
        public string FileTypeDescription { get; }
        public string CommandLine { get; }
        public string IconPath { get; }

        public FileAssociation(string ext, string progId, string descr, string commandLine, string iconPath)
        {
            Extension = ext;
            ProgId = progId;
            FileTypeDescription = descr;
            CommandLine = commandLine;
            IconPath = iconPath;
        }
    }
    #endregion

    public static class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void CreateMissingAssociations(IEnumerable<string> fileExtensions)
        {
            ProcessModule processModule = Process.GetCurrentProcess().MainModule;

            if (processModule != null)
            {
                string filePath = processModule.FileName;
                if (!string.IsNullOrEmpty(filePath))
                {
                    string cmdLine = "\"" + filePath + "\" \"%1\"";
                    string directoryName = Path.GetDirectoryName(filePath);

                    if (!string.IsNullOrEmpty(directoryName))
                        EnsureAssociationsSet(fileExtensions.Select(ext => new FileAssociation(ext, "ChanSort" + ext, "TV Channel List (" + ext + ")", cmdLine, Path.Combine(directoryName, "ChanSort.ico"))).ToList());
                }
            }
        }

        public static void EnsureAssociationsSet(IEnumerable<FileAssociation> associations)
        {
            if (associations.Aggregate(false, (current, assoc) => current | SetAssociation(assoc.Extension, assoc.ProgId, assoc.FileTypeDescription, assoc.CommandLine, assoc.IconPath)))
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool SetAssociation(string extension, string progId, string fileTypeDescription, string commandLine, string iconPath)
        {
            bool madeChanges = false;

            madeChanges |= SetValue($@"Software\Classes\{extension}\OpenWithProgids", progId, "");
            madeChanges |= SetValue($@"Software\Classes\{progId}", null, fileTypeDescription);
            madeChanges |= SetValue($@"Software\Classes\{progId}\shell\open\command", null, commandLine, true);
            madeChanges |= SetValue($@"Software\Classes\{progId}\DefaultIcon", null, iconPath);
            madeChanges |= SetValue($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithProgids", progId, "");

            return madeChanges;
        }

        private static bool SetValue(string keyPath, string name, string value, bool force = false)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (key != null)
                {
                    if (key.GetValue(name) is string currentValue)
                    {
                        if (currentValue == value || !force)
                            return false;
                    }

                    key.SetValue(name, value);

                    return true;
                }

                return false;
            }
        }

        public static void DeleteAssociations(IEnumerable<string> extensions)
        {
            foreach (string ext in extensions)
            {
                string progId = "ChanSort" + ext;
                DeleteValue($@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\OpenWithProgids", progId);
                Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}", false);
                DeleteValue($@"Software\Classes\{ext}\OpenWithProgids", progId);
            }

            void DeleteValue(string keyPath, string name)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
                {
                    if (key == null)
                        return;

                    if (name == null)
                        key.SetValue(null, "");
                    else
                        key.DeleteValue(name, false);
                }
            }
        }
    }
}