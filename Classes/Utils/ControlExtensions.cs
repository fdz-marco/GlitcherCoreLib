namespace glitcher.core
{
    /// <summary>
    /// (Class: Static~Global) **Controls - Extension Methods**<br/>
    /// Add the posibility to update controls in the UI from different threads/async tasks in a safe way.
    /// </summary>
    /// <remarks>
    /// Author: Marco Fernandez (marcofdz.com / glitcher.dev)<br/>
    /// Last modified: 2024.06.17 - June 17, 2024
    /// </remarks>
    public static partial class ControlExtensions
    {

        /// <summary>
        /// Update a property of Control in a SafeThread way.<br/>
        /// <example>Example:<br/>
        /// <code>textBox1.UpdateProperty((ctrl, value) => ctrl.Text = value, "Texto Actualizado");</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="value">Reference to value</param>
        /// <param name="updateAction">Update function: (ctrl,value) => { }</param>
        /// <returns>(void)</returns>
        public static void UpdateProperty<TControl, TProperty>
            (this TControl control, Action<TControl, TProperty> updateAction, TProperty value) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => updateAction(control, value)));
            }
            else
            {
                updateAction(control, value);
            }
        }

        /// <summary>
        /// Update a property of Control in a SafeThread way (Asynchronous).<br/>
        /// <example>Example:<br/>
        /// <code>await textBox1.UpdatePropertyAsync((ctrl, value) => ctrl.Text = value, "Updated Text");</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="value">Reference to value</param>
        /// <param name="updateAction">Update function: (ctrl,value) => { }</param>
        /// <returns>(Task)</returns>
        public static Task UpdatePropertyAsync<TControl, TProperty>
            (this TControl control, Action<TControl, TProperty> updateAction, TProperty value) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                return Task.Run(() => control.InvokeAsync(new Action(() => updateAction(control, value))));
            }
            else
            {
                return Task.Run(() => control.Invoke(new Action(() => updateAction(control, value))));
                //return Task.Run(() => updateAction(control, value));
            }
        }

        /// <summary>
        /// Call a method (without arguments) of Control in a SafeThread way.<br/>
        /// <example>Example:<br/>
        /// <code>textBox1.CallMethod(ctrl => ctrl.Clear());</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="methodAction">Function: (ctrl) => { }</param>
        /// <returns>(void)</returns>
        public static void CallMethod<TControl>
            (this TControl control, Action<TControl> methodAction) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => methodAction(control)));
            }
            else
            {
                methodAction(control);
            }
        }

        /// <summary>
        /// Call a method (without arguments) of Control in a SafeThread way (Asynchronous).<br/>
        /// <example>Example:<br/>
        /// <code>await textBox1.CallMethodAsync(ctrl => ctrl.Clear());</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="methodAction">Function: (ctrl) => { }</param>
        /// <returns>(Task)</returns>
        public static Task CallMethodAsync<TControl>
            (this TControl control, Action<TControl> methodAction) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                return Task.Run(() => control.InvokeAsync(new Action(() => methodAction(control))));
            }
            else
            {
                if (control.Parent == null)
                    return Task.Run(() => { });
                return Task.Run(() => control.Invoke(new Action(() => methodAction(control))));
                //return Task.Run(() => methodAction(control));
            }
        }

        /// <summary>
        /// Call a method (with arguments) of Control in a SafeThread way.<br/>
        /// <example>Example:<br/>
        /// <code>textBox1.CallMethod((ctrl, value) => ctrl.AppendText(value), "Texto Añadido");</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="methodAction">Function: (ctrl) => { }</param>
        /// <param name="arg">Argument</param>
        /// <returns>(void)</returns>
        public static void CallMethod<TControl, TArg>
            (this TControl control, Action<TControl, TArg> methodAction, TArg arg) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new Action(() => methodAction(control, arg)));
            }
            else
            {
                methodAction(control, arg);
            }
        }

        /// <summary>
        /// Call a method (with arguments) of Control in a SafeThread way (Asynchronous).<br/>
        /// <example>Example:<br/>
        /// <code>await textBox1.CallMethodAsync((ctrl, value) => ctrl.AppendText(value), "Appended Text");</code>
        /// </example>
        /// </summary>
        /// <param name="control">Reference to Control</param>
        /// <param name="methodAction">Function: (ctrl) => { }</param>
        /// <param name="arg">Argument</param>
        /// <returns>(Task)</returns>
        public static Task CallMethodAsync<TControl, TArg>
            (this TControl control, Action<TControl, TArg> methodAction, TArg arg) where TControl : Control
        {
            if (control.InvokeRequired)
            {
                return Task.Run(() => control.InvokeAsync(new Action(() => methodAction(control, arg))));
            }
            else
            {
                if (control.Parent == null)
                    return Task.Run(() => { });
                return Task.Run(() => control.Invoke(new Action(() => methodAction(control, arg))));
                //return Task.Run(() => methodAction(control, arg));
            }
        }

        /// <summary>
        /// Invoke Async (Asynchronous).
        /// </summary>
        /// <param name="control">Reference to Control.</param>
        /// <param name="methodAction">Function to execute on asynchronious Invoke.</param>
        public static async Task InvokeAsync(this Control control, Action methodAction)
        {
            if (control.InvokeRequired)
            {
                await Task.Run(() => control.Invoke(methodAction));
            }
            else
            {
                methodAction();
            }
        }

    }
}