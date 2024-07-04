using System.Diagnostics;
using System.Reflection;

namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) **Utilities - Resources**<br/><br/>
    /// **Important**<br/>
    /// To add complete project folder as embedded resources, 
    /// please add the folder as embedded resource in the *.csproj definition:<br/>
    /// <code>&lt;ItemGroup&gt; &lt;EmbeddedResource Include = "Resources\*"&gt; &lt;ItemGroup&gt;</code>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.04 - July 04, 2024
    /// </remarks>
    public static partial class Utils
    {
        private static Assembly _assembly = null;

        /// <summary>
        /// Set Assembly (App)
        /// </summary>
        /// <param name="assembly">Remote Assembly</param>
        /// <returns>(void)</returns>
        public static void SetAssemblyRemote(Assembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        /// Get Assembly (App or DLL)
        /// </summary>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(Assembly)</returns>
        public static Assembly GetAssembly(bool remoteAsm)
        {
            Assembly assembly = _assembly;

            // Get Assembly 
            if ((remoteAsm) && (assembly == null))
                assembly = Utils.GetCallingAssembly();
            if (!remoteAsm)
                assembly = Assembly.GetExecutingAssembly();
            return assembly;
        }

        /// <summary>
        /// Get Resource Path from a Folder Path
        /// </summary>
        /// <param name="path">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(string) Resource Path Name</returns>
        public static string? GetResourcePath(string path, bool remoteAsm)
        {
            // Get Assembly
            Assembly assembly = GetAssembly(remoteAsm);

            // Get Assembly List Resources
            List<string> resourceNames = new List<string>(assembly.GetManifestResourceNames());

            // Search Resource
            string _path = path.TrimStart('/').TrimEnd('/');
            _path = _path.Replace(@"\", "/");
            _path = _path.Replace(@"//", "/");
            _path = _path.Replace(@"/", ".");
            _path = $"{assembly.GetName().Name}.{_path}";
            _path = resourceNames.FirstOrDefault(r => r.Equals(_path), "");

            return (!string.IsNullOrEmpty(_path)) ? _path : null;
        }

        /// <summary>
        /// Get Stream From Resource
        /// </summary>
        /// <param name="path">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(Image)</returns>
        public static Stream? GetResourceStream(string path, bool remoteAsm)
        {
            // Get Assembly
            Assembly assembly = GetAssembly(remoteAsm);

            // Get Resource path
            string _path = Utils.GetResourcePath(path, remoteAsm);

            // Return Found Resource or Default Image
            if (!string.IsNullOrEmpty(_path))
            {
                Stream stream = assembly.GetManifestResourceStream(_path);
                return stream;
            }
            return null;
        }

        /// <summary>
        /// Get Image From Resource
        /// </summary>
        /// <param name="path">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(Image)</returns>
        public static Image GetResourceImage(string path, bool remoteAsm)
        {
            // Get Assembly
            Assembly assembly = GetAssembly(remoteAsm);

            // Get Resource path
            string _path = Utils.GetResourcePath(path, remoteAsm);

            // Return Found Resource or Default Image
            if (!string.IsNullOrEmpty(_path))
            {
                Stream stream = assembly.GetManifestResourceStream(_path);
                return System.Drawing.Image.FromStream(stream);
            }
            return System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
        }

        /// <summary>
        /// Get Icon From Resource
        /// </summary>
        /// <param name="path">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(Icon)</returns>
        public static Icon GetResourceIcon(string path, bool remoteAsm)
        {
            // Get Assembly
            Assembly assembly = GetAssembly(remoteAsm);

            // Get Resource path
            string _path = Utils.GetResourcePath(path, remoteAsm);

            // Return Found Resource or Default Image
            if (!string.IsNullOrEmpty(_path))
            {
                Stream stream = assembly.GetManifestResourceStream(_path);
                return Icon.FromHandle(new Bitmap(stream).GetHicon());
            }
            return System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
    }
}
