using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) **Utilities - System**<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.04 - July 04, 2024
    /// </remarks>
    public static partial class Utils
    {

        /// <summary>
        /// Open the App Folder on Explorer
        /// </summary>
        /// <returns>(void)</returns>
        public static void OpenAppFolder()
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string appFolder = Path.GetDirectoryName(appPath);
            try
            {
                Process.Start("explorer", appFolder);
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Fatal, "Utilities", $"Error opening App Folder: {ex.Message}.");
            }
        }

        /// <summary>
        /// Show or Hide All Forms
        /// </summary>
        /// <returns>(void)</returns>
        public static void ShowOrHideAllForms()
        {
            try
            {
                bool isFormOpen = Application.OpenForms.Count > 0;
                if (isFormOpen)
                {
                    bool isOneVisible = false;
                    foreach (Form form in Application.OpenForms)
                    {
                        isOneVisible = isOneVisible || form.Visible;
                    }

                    if (isOneVisible)
                    {
                        foreach (Form form in Application.OpenForms)
                        {
                            if (form.Visible)
                                form.Hide();
                        }
                    }
                    else
                    {
                        foreach (Form form in Application.OpenForms)
                        {
                            form.Show();
                            form.WindowState = FormWindowState.Normal;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Fatal, "Utilities", $"Error (ShowOrHideAllForms). Error: {ex.Message}.");
            }
        }

        /// <summary>
        /// Open a URL on the Web Browser
        /// </summary>
        /// <param name="url">URL. Example: http:// localhost:8081 </param>
        /// <returns>(void)</returns>
        public static void OpenWebBrowser(string url)
        {
            if (!IsValidUrl(url))
            {
                Logger.Add(LogLevel.Error, "Utilities", $"Invalid URL: {url}.");
                return;
            }
            try
            {
                Process.Start("explorer", url);
            }
            catch (Exception ex)
            {
                Logger.Add(LogLevel.Error, "Utilities", $"Error (OpenWebBrowser). URL: {url}. Error: {ex.Message}.");
            }
        }

        /// <summary>
        /// Validate the URL with RegEx
        /// </summary>
        /// <param name="url">URL. Example: http:// localhost:8081 </param>
        /// <returns>(void)</returns>
        public static bool IsValidUrl(string url)
        {
            string pattern = @"^(https?|ftp):\/\/" +                    // Scheme
                             @"(([a-zA-Z0-9\-_]+\.)+[a-zA-Z]{2,}|" +    // Domain name
                             @"localhost|" +                            // Localhost
                             @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})" +   // OR IPv4
                             @"(:\d+)?(\/.*)?$";                        // Optional port and path
            return Regex.IsMatch(url, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Check if application has admin rights.
        /// </summary>
        /// <returns>(bool) True if application has been open using admin rights.</returns>
        public static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Force restart application with admin rights if needed.
        /// </summary>
        /// <returns>(void)</returns>
        public static void RestartAsAdmin()
        {
            if (!IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Process.GetCurrentProcess().MainModule.FileName;

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Logger.Add(LogLevel.Info, "Utilities", $"Error trying to open the application as an administrator!. Error: {ex.Message}.");
                }
            }
        }

        /// <summary>
        /// Get application name
        /// </summary>
        /// <returns>(string) Application Name</returns>
        public static string GetAppName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }

        /// <summary>
        /// Get main window title
        /// </summary>
        /// <returns>(string) Main Window Title</returns>
        public static string GetMainWindowTitle()
        {
            return (Application.OpenForms.Count > 0) ? Application.OpenForms[0].Text : GetAppName();
        }

        /// <summary>
        /// Copy Files of One Directory Recursively
        /// </summary>
        /// <param name="sourcePath">Source Path</param>
        /// <param name="targetPath">Target Path</param>
        /// <returns>(bool)</returns>
        public static bool CopyFilesRecursively(string sourcePath, string targetPath)
        {
            try
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Delete Files of One Directory Recursively
        /// </summary>
        /// <param name="targetPath">Target Path</param>
        /// <returns>(bool)</returns>
        public static bool DeleteFilesRecursively(string targetPath)
        {
            try
            {
                if (!Directory.Exists(targetPath))
                {
                    throw new DirectoryNotFoundException($"The directory '{targetPath}' does not exist.");
                }
                // Delete all files in the directory
                string[] files = Directory.GetFiles(targetPath);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal); // Ensure the file is not read-only
                    File.Delete(file);
                }
                // Delete all subdirectories in the directory
                string[] subDirectories = Directory.GetDirectories(targetPath);
                foreach (string subDirectory in subDirectories)
                {
                    DeleteFilesRecursively(subDirectory);
                    Directory.Delete(subDirectory);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Get Calling Assembly
        /// </summary>
        /// <returns>(Assembly)</returns>
        public static Assembly GetCallingAssembly()
        {
            // Get the stack trace for the current thread
            StackTrace stackTrace = new StackTrace();

            // Get the frame of the method that called this method (skip this frame)
            StackFrame frame = stackTrace.GetFrame((stackTrace.FrameCount-1));

            // Get the method information
            MethodBase method = frame.GetMethod();

            // Get the assembly of the calling method
            Assembly callingAssembly = method.DeclaringType.Assembly;

            return callingAssembly;
        }

        /// <summary>
        /// Find Control Recursively
        /// </summary>
        /// <param name="parent">Parent Control</param>
        /// <param name="controlName">Control Name</param>
        /// <returns>(Control) Control</returns>
        public static Control FindControlRecursive(Control parent, string controlName)
        {
            if (parent == null) return null;

            Control foundControl = parent.Controls[controlName];
            if (foundControl != null)
            {
                return foundControl;
            }

            foreach (Control child in parent.Controls)
            {
                foundControl = FindControlRecursive(child, controlName);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }
            return null;
        }

    }
}
