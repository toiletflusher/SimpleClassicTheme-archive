﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClassicTheme
{
    public abstract class InstallableUtility
    {
                                                                                                                                                    //Display Name				Executable name		Download URL/Resource Name												Install directory											Installed file																            Install commandline			(Uninstall executable															Uninstall arguments
        public static InstallableUtilityWithInstallerFromWeb 		SevenPlusTaskbarTweaker  	= new InstallableUtilityWithInstallerFromWeb(		"7+ Taskbar Tweaker", 		"7tt.exe", 			"https://rammichael.com/downloads/7tt_setup.exe",						"%userprofile%\\AppData\\Roaming\\7+ Taskbar Tweaker", 		"%userprofile%\\AppData\\Roaming\\Programs\\7+ Taskbar Tweaker\\uninstall.exe", 		"/S", 						("%userprofile%\\AppData\\Roaming\\7+ Taskbar Tweaker\\uninstall.exe", 			"/S"));
        public static InstallableUtilityWithInstallerFromWeb		StartIsBackPlusPlus			= new InstallableUtilityWithInstallerFromWeb(		"StartIsBack++",			"sib.exe",			"https://s3.amazonaws.com/startisback/StartIsBackPlusPlus_setup.exe",	"%userprofile%\\AppData\\Local\\StartIsBack",				"%userprofile%\\AppData\\Local\\StartIsBack\\StartIsBackCfg.exe",			            "/silent",					("%userprofile%\\AppData\\Local\\StartIsBack\\StartIsBackCfg.exe", 				"/uninstall"));
        public static InstallableUtilityWithInstallerFromResource 	ClassicTaskManager			= new InstallableUtilityWithInstallerFromResource(	"Classic Task Manager", 	"ctm.exe", 			"classicTaskManager",													"C:\\Program Files\\ClassicTaskmgr",						"C:\\Program Files\\ClassicTaskmgr\\unins000.exe",							            "/silent",					("C:\\Program Files\\ClassicTaskmgr\\unins000.exe",								"/silent"));
        public static InstallableUtilityWithInstallerFromResource 	FolderOptionsX				= new InstallableUtilityWithInstallerFromResource(	"Folder Options X",		 	"fox.exe", 			"folderOptionsX",														"C:\\Program Files\\T800 Productions\\Folder Options X",	"C:\\Program Files\\T800 Productions\\Folder Options X\\unins000.exe",		            "/silent",					("C:\\Program Files\\T800 Productions\\Folder Options X\\unins000.exe",			"/silent"));
        public static InstallableUtilityWithInstallerFromResource   OpenShell                   = new InstallableUtilityWithInstallerFromResource(  "Open-Shell",               "oss.exe",          "openShellSetup",                                                       "C:\\Program Files\\Open-Shell",                            "C:\\Program Files\\Open-Shell\\OpenShell.chm",                                         "/qn /norestart",           ("C:\\Windows\\System32\\msiexec.exe",                                          "/X{FD722BB1-4960-455F-89C6-EFAEB79527EF} /qn /norestart"));

        public string Name;
        public string SafeNameForFile;
        public abstract bool IsInstalled { get; }
        public abstract int Install();
        public abstract int Uninstall();
    }

    public class InstallableUtilityWithInstallerFromResource : InstallableUtility
    {
        public string ResourceName = "";
        public string InstallationDirectory = "";
        public string InstallationFile = "";
        public string InstallParameters = "";
        public (string, string) UninstallCommandline = ("", "");

        public override bool IsInstalled => Directory.Exists(InstallationDirectory) && File.Exists(InstallationFile);

        public InstallableUtilityWithInstallerFromResource(string name, string safeNameForFile, string resourceName, string installationDirectory, string installationFile, string installParameters, (string, string) uninstallCommandline)
        {
            //Prepare local appdata path for uninstallation of certain utilities
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
                path = Directory.GetParent(path).ToString();

            Name = name;
            SafeNameForFile = safeNameForFile;
            ResourceName = resourceName;
            InstallationDirectory = installationDirectory.Replace("%userprofile%", path);
            InstallationFile = installationFile.Replace("%userprofile%", path);
            InstallParameters = installParameters;
            UninstallCommandline = (uninstallCommandline.Item1.Replace("%userprofile%", path), uninstallCommandline.Item2);
        }

        public override int Install()
        {
            byte[] installer = (byte[])Properties.Resources.ResourceManager.GetObject(ResourceName);

            string path = $"C:\\SCT\\{SafeNameForFile}";
            File.WriteAllBytes(path, installer);
            Process p = Process.Start(path, InstallParameters);
            p.WaitForExit();
            File.Delete(path);

            return p.ExitCode;
        }

        public override int Uninstall()
        {
            string executable = UninstallCommandline.Item1;
            string arguments = UninstallCommandline.Item2;

            Process p = Process.Start(executable, arguments);
            p.WaitForExit();

            return p.ExitCode;
        }
    }

    public class InstallableUtilityWithInstallerFromWeb : InstallableUtility
    {
        public string DownloadURI = "";
        public string InstallationDirectory = "";
        public string InstallationFile = "";
        public string InstallParameters = "";
        public (string, string) UninstallCommandline = ("", "");

        public override bool IsInstalled => Directory.Exists(InstallationDirectory) && File.Exists(InstallationFile);

        public InstallableUtilityWithInstallerFromWeb(string name, string safeNameForFile, string downloadURI, string installationDirectory, string installationFile, string installParameters, (string, string) uninstallCommandline)
        {
            //Prepare local appdata path for uninstallation of certain utilities
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
                path = Directory.GetParent(path).ToString();

            Name = name;
            SafeNameForFile = safeNameForFile; 
            DownloadURI = downloadURI;
            InstallationDirectory = installationDirectory.Replace("%userprofile%", path);
            InstallationFile = installationFile.Replace("%userprofile%", path);
            InstallParameters = installParameters;
            UninstallCommandline = (uninstallCommandline.Item1.Replace("%userprofile%", path), uninstallCommandline.Item2);
        }

        public override int Install()
        {
            byte[] installer;
            using (WebClient c = new WebClient() { Proxy = null })
                installer = c.DownloadData(DownloadURI);

            string path = $"C:\\SCT\\{SafeNameForFile}";
            File.WriteAllBytes(path, installer);
            Process p = Process.Start(path, InstallParameters);
            p.WaitForExit();
            File.Delete(path);

            return p.ExitCode;
        }

        public override int Uninstall()
        {
            string executable = UninstallCommandline.Item1;
            string arguments = UninstallCommandline.Item2;

            Process p = Process.Start(executable, arguments);
            p.WaitForExit();

            return p.ExitCode;
        }
    }
}
