using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ChanSort.Api
{
    public static class DepencencyChecker
    {
        public static bool IsVCRedistInstalled()
        {
            string dependenciesPath = @"SOFTWARE\Classes\Installer\Dependencies";

            using (RegistryKey dependencies = Registry.LocalMachine.OpenSubKey(dependenciesPath))
            {
                if (dependencies == null) return false;

                foreach (string subKeyName in dependencies.GetSubKeyNames().Where(n => !n.ToLower().Contains("dotnet") && !n.ToLower().Contains("microsoft")))
                {
                    using (RegistryKey subDir = Registry.LocalMachine.OpenSubKey(dependenciesPath + "\\" + subKeyName))
                    {
                        var value = subDir.GetValue("DisplayName")?.ToString() ?? null;
                        if (string.IsNullOrEmpty(value)) continue;

                        if (!Environment.Is64BitProcess)
                        {
                            if (Regex.IsMatch(value, @"C\+\+ 201[0-9].*\(x86\)"))
                                return true;
                        }
                        else
                        {
                            if (Regex.IsMatch(value, @"C\+\+ 201[0-9].*\(x64\)"))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static void AssertVCRedistInstalled()
        {
            if (!IsVCRedistInstalled())
                throw new FileLoadException("Please download and install the Microsoft Visual C++ Redistributable Package (" + (Environment.Is64BitProcess?"x64":"x86") + ")");
        }
    }
}
