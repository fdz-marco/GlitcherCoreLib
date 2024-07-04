using System.Net.Sockets;
using System.Net.NetworkInformation;

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
        /// Get Local IPs
        /// </summary>
        /// <returns>(List<string>) List of string with the IPs</returns>
        public static List<string> GetAllLocalIPv4(bool local)
        {
            List<string>? ipAddrList = new List<string>();
            ipAddrList.Add("127.0.0.1");
            ipAddrList.Add("localhost");

            if (local) 
                return ipAddrList;

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return ipAddrList.Distinct().ToList();
        }

        /// <summary>
        /// Get Mime Type by file extension.
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <returns>(string) Mime Type</returns>
        public static string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".html": return "text/html";
                case ".htm": return "text/html";
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".json": return "application/json";
                case ".txt": return "text/plain";
                case ".xml": return "application/xml";
                case ".csv": return "text/csv";
                case ".zip": return "application/zip";
                case ".mp3": return "video/mpeg";
                case ".mp4": return "video/mp4";
                case ".pdf": return "application/pdf";
                case ".php": return "application/x-httpd-php";
                case ".svg": return "image/svg+xml";
                case ".ttf": return "font/ttf";
                case ".woff": return "font/woff";
                case ".woff2": return "font/woff2";
                case ".wasm": return "application/wasm";
                case "": return "text/html";
                default: return "application/octet-stream";
            }
        }

        /// <summary>
        /// Get OS from User Agent
        /// </summary>
        /// <param name="userAgent">User Agent</param>
        /// <returns>(string) Operative System</returns>
        public static string GetOSFromUserAgent(string userAgent)
        {
			if (userAgent == null)
                return "Empty OS";
            if (userAgent.Contains("Windows NT 10.0"))
                return "Windows 10";
            else if (userAgent.Contains("Windows NT 6.3"))
                return "Windows 8.1";
            else if (userAgent.Contains("Windows NT 6.2"))
                return "Windows 8";
            else if (userAgent.Contains("Windows NT 6.1"))
                return "Windows 7";
            else if (userAgent.Contains("Windows NT 6.0"))
                return "Windows Vista";
            else if (userAgent.Contains("Windows NT 5.1"))
                return "Windows XP";
            else if (userAgent.Contains("Mac OS X"))
                return "Mac OS X";
            else if (userAgent.Contains("Linux"))
                return "Linux";
            else if (userAgent.Contains("Android"))
                return "Android";
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "iOS";
            else
                return "Unknown OS";
        }

        /// <summary>
        /// Get all the EndPoints with Port
        /// </summary>
        /// <param name="port">Port of EndPoints</param>
        /// <param name="includeHTTP">Include HTTP</param>
        /// <param name="includeWS">Include WS</param>
        /// <param name="includeSecure">Include Secure (HTTPS/WSS)</param>
        /// <param name="local">Only Local EndPoints</param>
        /// <returns>(List<string>) List of Endpoints with Port </returns>
        public static List<string>? GetEndPointsWithPort(int port, bool includeHTTP = true, bool includeWS = false, bool includeSecure = false,  bool local = true)
        {
            List<string> endpoints = new List<string>();

            // Add Local EndPoints
            if (includeHTTP) endpoints.AddRange(Utils.GetAllLocalIPv4(true).Select(x => $"http://{x}:{port}/").ToList());
            if (includeHTTP && includeSecure) endpoints.AddRange(Utils.GetAllLocalIPv4(true).Select(x => $"https://{x}:{port}/").ToList());
            if (includeWS) endpoints.AddRange(Utils.GetAllLocalIPv4(true).Select(x => $"ws://{x}:{port}/").ToList());
            if (includeWS && includeSecure) endpoints.AddRange(Utils.GetAllLocalIPv4(true).Select(x => $"wss://{x}:{port}/").ToList());

            // Return only local
            if (local)
                return endpoints.Distinct().ToList();

            // Check admin rights to check if is possible to use all endpoints or only local
            if (!Utils.IsRunAsAdmin())
            {
                DialogResult dialogResult = MessageBox.Show($"Application needs admin rights to listen all domains ports. " +
                    "This include all IP Addresses of all network cards. " +
                    "Do you want to restart application with admin privilages?", "Administrator Privilages", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Utils.RestartAsAdmin();
                    return null;
                }
                else
                {
                    MessageBox.Show("Only local domains ports will be used.", "Administrator Privilages");
                    return endpoints.Distinct().ToList();
                }
            }
            else
            {
                // Add AllEndPoints Available (All Network Cards)
                if (includeHTTP) endpoints.AddRange(Utils.GetAllLocalIPv4(false).Select(x => $"http://{x}:{port}/").ToList());
                if (includeHTTP && includeSecure) endpoints.AddRange(Utils.GetAllLocalIPv4(false).Select(x => $"https://{x}:{port}/").ToList());
                if (includeWS) endpoints.AddRange(Utils.GetAllLocalIPv4(false).Select(x => $"ws://{x}:{port}/").ToList());
                if (includeWS && includeSecure) endpoints.AddRange(Utils.GetAllLocalIPv4(false).Select(x => $"wss://{x}:{port}/").ToList());
                return endpoints.Distinct().ToList();
            }
        }
    }
}