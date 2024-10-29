using System.Configuration;

namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) Configuration Manager (XML)<br/>
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez<br/>
    /// Last modified: 2024.07.20 - July 20, 2024<br/>
    /// </remarks>

    public class ConfigManagerXML
    {
        #region Private Properties

        private Configuration? _config = null;
        public string configFilename { get; set; } = "settings.config";

        #endregion

        #region Constructor and Settings

        public ConfigManagerXML(string configFilename = "settings.config")
        {
            this.configFilename = configFilename;
            //Logger.Add(LogLevel.Info, "Config Manager", $"Object Created.");
        }

        public void Start()
        {
            if (_config != null)
            {
                //Logger.Add(LogLevel.OnlyDebug, "Config Manager", $"Already Started.");
                return;
            }
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFilename;
            _config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            if (!File.Exists(_config.FilePath))
                _config.Save(ConfigurationSaveMode.Modified);
        }

        #endregion

        #region Get / Set Configurations Keys

        public string? Get(string key)
        {
            if (_config == null)
            {
                //Logger.Add(LogLevel.Fatal, "Config Manager", $"Null Configuration Instance.");
                MessageBox.Show($"Null Configuration Instance.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            if (!_config.AppSettings.Settings.AllKeys.Contains(key))
            {
                //Logger.Add(LogLevel.Error, "Config Manager", $"Configuration Key <{key}> doesn't exist.");
                MessageBox.Show($"Configuration Key <{key}> doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return _config.AppSettings.Settings[key].Value;
        }

        public bool Set(string key, string value = "")
        {
            if (_config == null)
            {
                //Logger.Add(LogLevel.Fatal, "Config Manager", $"Null Configuration Instance.");
                MessageBox.Show($"Null Configuration Instance.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            try
            {
                string? currentValue = Get(key);
                if (currentValue == null)
                {
                    _config.AppSettings.Settings.Add(key, value);
                    _config.Save(ConfigurationSaveMode.Modified);
                    //Logger.Add(LogLevel.Warning, "Config Manager", $"Configuration Key <{key}> doesn't exist but created.");
                }
                _config.AppSettings.Settings[key].Value = value;
                _config.Save(ConfigurationSaveMode.Modified);
                //Logger.Add(LogLevel.Info, "Config Manager", $"Setting successfully saved: [{key}={value}]");
                return true;
            }
            catch
            {
                //Logger.Add(LogLevel.Error, "Config Manager", $"Error saving the setting: [{key}={value}]");
                MessageBox.Show($"Error saving the setting: [{key}={value}]", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Key To Var (string, int, bool)

        public string KeyToVar(string key, out string variable, string defaultValue = "")
        {
            string? value = Get(key);
            if (!string.IsNullOrEmpty(value))
                variable = value;
            else
                variable = defaultValue;
            return variable;
        }

        public int KeyToVar(string key, out int variable, int defaultValue = 0)
        {
            string? value = Get(key);
            if (!string.IsNullOrEmpty(value))
            {
                if (int.TryParse(value, out int intValue))
                    variable = intValue;
                else
                    variable = defaultValue;
            }
            else
            {
                variable = defaultValue;
            }
            return variable;
        }

        public bool KeyToVar(string key, out bool variable, bool defaultValue = false)
        {
            string? value = Get(key);
            if (!string.IsNullOrEmpty(value))
            {
                string[] trueValues = { "true", "TRUE", "True", "1" };
                variable = trueValues.Contains(value);
            }
            else
            {
                variable = defaultValue;
            }
            return variable;
        }

        #endregion

        #region Key To Control / Control to Key (Texbox, CheckBox, ComboBox, NumericUpDown, CheckedListBox, DateTimePicker

        public void ValueToControl(Control control, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            switch (control)
            {
                case TextBox textBox:
                    if (textBox.PasswordChar != '\0')
                        textBox.Text = Utils.Base64Decode(value);
                    else
                        textBox.Text = value;
                    break;
                case CheckBox checkBox:
                    if (bool.TryParse(value, out var isChecked))
                    {
                        checkBox.Checked = isChecked;
                    }
                    break;
                case ComboBox comboBox:
                    for (int i = 0; i < comboBox.Items.Count; i++)
                    {
                        string _value = comboBox.Items[i].ToString();
                        if (comboBox.Items[i].ToString() == value.ToString())
                        {
                            comboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    //comboBox.SelectedItem = value;
                    break;
                case NumericUpDown numericUpDown:
                    if (int.TryParse(value, out var intValue))
                    {
                        numericUpDown.Value = intValue;
                    }
                    break;
                case CheckedListBox checkedListBox:
                    string[] values = value.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                    // Uncheck all items
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                        checkedListBox.SetItemChecked(i, false);
                    // Check values on
                    foreach (string value_ in values)
                    {
                        for (int i = 0; i < checkedListBox.Items.Count; i++)
                        {
                            if (checkedListBox.Items[i].ToString() == value_)
                            {
                                checkedListBox.SetItemChecked(i, true);
                                break;
                            }
                        }
                    }
                    break;
                case DateTimePicker dateTimePicker:
                    DateTime dateTime = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    dateTimePicker.Value = dateTime;
                    break;
            }
        }

        public string ControlToValue(Control control)
        {
            switch (control)
            {
                case TextBox textBox:
                    if (textBox.PasswordChar != '\0')
                        return Utils.Base64Encode(textBox.Text);
                    return textBox.Text;
                case CheckBox checkBox:
                    return (checkBox.Checked) ? "True" : "False";
                case ComboBox comboBox:
                    return (comboBox.SelectedIndex >= 0) ? comboBox.SelectedItem.ToString() : "";
                case NumericUpDown numericUpDown:
                    return numericUpDown.Value.ToString();
                case CheckedListBox checkedListBox:
                    string[] checkedItems = new string[checkedListBox.CheckedItems.Count];
                    checkedListBox.CheckedItems.CopyTo(checkedItems, 0);
                    return string.Join("||", checkedItems);
                case DateTimePicker dateTimePicker:
                    return dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm:ss");
                default:
                    return "";
            }
        }

        public void UpdateCtrlFromKey(Control Parent, string ControlName, string key)
        {
            try
            {
                Control control = Utils.FindControlRecursive(Parent, ControlName);
                string? value = Get(key);
                ValueToControl(control, value);
            }
            catch (Exception ex)
            {
                //Logger.Add(LogLevel.Error, "Config Manager", $"Update Control <{ControlName}> from Key <{key}> Failed. Error: {ex.Message}.");
                MessageBox.Show($"Update Control <{ControlName}> from Key <{key}> Failed. Error: {ex.Message}.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateKeyFromControl(Control Parent, string ControlName, string key)
        {
            try
            {
                Control control = Utils.FindControlRecursive(Parent, ControlName);
                string? value = ControlToValue(control);
                Set(key, value);
            }
            catch (Exception ex)
            {
                //Logger.Add(LogLevel.Error, "Config Manager", $"Update Key <{key}> from Control <{ControlName}> Failed. Error: {ex.Message}.");
                MessageBox.Show($"Update Key <{key}> from Control <{ControlName}> Failed. Error: {ex.Message}.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
