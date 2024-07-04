using System.Reflection;

namespace glitcher.core
{
    /// <summary>
    /// (Class) System Tray<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.07.04 - July 04, 2024
    /// </remarks>
    public class SystemTray
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenuStrip;

        /// <summary>
        /// Create a System Tray Icon with Context Menu
        /// </summary>
        /// <returns>(void)</returns>
        public SystemTray()
        {
            this._notifyIcon = new NotifyIcon();
            this._contextMenuStrip = new ContextMenuStrip();
            InitDefaultSettings();
        }

        /// <summary>
        /// Initialize Default Settings
        /// </summary>
        /// <returns>(void)</returns>
        private void InitDefaultSettings()
        {
            // Configure ToolTip text
            this._notifyIcon.Text = Utils.GetAppName();
            // Configure Icon
            this._notifyIcon.Visible = true;
            //Set Icon
            ChangeNotifyIcon("Resources/IconSysTray.png");
            // Add items
            this._contextMenuStrip.Items.Clear();
            AddItem("Open App Folder", OpenAppFolder_Click, "Resources/IconOpenAppFolder.png");
            AddItem("Open Log Viewer", OpenLogViewer_Click, "Resources/IconOpenLogViewer.png");
            AddItem("-");
            AddItem("Exit App", ExitApp_Click, "Resources/IconExitApp.png");
            // Add context menu
            this._notifyIcon.ContextMenuStrip = this._contextMenuStrip;
            this._notifyIcon.MouseClick += NotifyIcon_Click;
        }

        /// <summary>
        /// Event Handler: Open App Folder
        /// </summary>
        /// <returns>(void)</returns>
        private void OpenAppFolder_Click(object sender, EventArgs e)
        {
            Utils.OpenAppFolder();
        }

        /// <summary>
        /// Event Handler: Show Log Viewer
        /// </summary>
        /// <returns>(void)</returns>
        private void OpenLogViewer_Click(object sender, EventArgs e)
        {
            LogViewer.GetInstance().Show();
        }

        /// <summary>
        /// Event Handler: Exit Application
        /// </summary>
        /// <returns>(void)</returns>
        private void ExitApp_Click(object sender, EventArgs e)
        {
            this._notifyIcon.Visible = false;
            this.Dispose();
            Application.Exit();
        }

        /// <summary>
        /// Event Handler: Hide/Show All Forms
        /// </summary>
        /// <returns>(void)</returns>
        private void NotifyIcon_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Utils.ShowOrHideAllForms();
            }
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <returns>(void)</returns>
        public void Dispose()
        {
            this._notifyIcon.Dispose();
            this._notifyIcon = null;
            this._contextMenuStrip.Dispose();
            this._contextMenuStrip = null;
        }

        /// <summary>
        /// Add a new Item to Notify Context Menu (At Top)
        /// </summary>
        /// <param name="text">Text of Item</param>
        /// <param name="callback">Callback to trigger on click</param>
        /// <param name="resourcePath">Path of Resource</param>
        /// <param name="remoteAsm">Resource should be from Caller Assembly</param>
        /// <returns>(void)</returns>
        public void AddItemAtTop(string text, EventHandler? callback = null, string? resourcePath = null, bool remoteAsm = false)
        {
            if (text == "-")
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                this._contextMenuStrip.Items.Add(separator);
                return;
            }
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = text;
            if (callback != null)
                item.Click += callback;
            if (resourcePath != null)
                item.Image = Utils.GetResourceImage(resourcePath, remoteAsm); ;
            this._contextMenuStrip.Items.Insert(0 , item);
        }

        /// <summary>
        /// Add a new Item to Notify Context Menu
        /// </summary>
        /// <param name="text">Text of Item</param>
        /// <param name="callback">Callback to trigger on click</param>
        /// <param name="resourcePath">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(void)</returns>
        public void AddItem(string text, EventHandler? callback = null, string? resourcePath = null, bool remoteAsm = false)
        {
            if (text == "-")
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                this._contextMenuStrip.Items.Add(separator);
                return;
            }
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = text;
            if (callback != null)
                item.Click += callback;
            if (resourcePath != null)
                item.Image = Utils.GetResourceImage(resourcePath, remoteAsm);
            this._contextMenuStrip.Items.Add(item);
        }

        /// <summary>
        /// Change the Notify Icon
        /// </summary>
        /// <param name="resourcePath">Path of Resource</param>
        /// <param name="remoteAsm">Remote Assembly or DLL</param>
        /// <returns>(void)</returns>
        public void ChangeNotifyIcon(string resourcePath = null, bool remoteAsm = false)
        {
            this._notifyIcon.Icon = Utils.GetResourceIcon(resourcePath, remoteAsm);
        }
    }
}