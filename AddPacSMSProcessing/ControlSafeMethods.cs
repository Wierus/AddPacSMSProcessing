using System;
using System.Windows.Controls;

namespace AddPacSMSProcessing {

    /// <summary>
    /// Потокобезопасные методы для работы с элементами пользовательского интерфейса.
    /// </summary>
    public static class ControlSafeMethods {

        public static void InvokeControlActionSafe(Control control, Action action) {
            control.Dispatcher.Invoke(action);
        }

        public static void BeginInvokeControlActionSafe(Control control, Action action) {
            control.Dispatcher.BeginInvoke(action);
        }

        public static void InvokeControlActionSafe<T0>(Control control, Action<T0> action, object arg0) {
            control.Dispatcher.Invoke(action, arg0);
        }

        public static void BeginInvokeControlActionSafe<T0>(Control control, Action<T0> action, object arg0) {
            control.Dispatcher.BeginInvoke(action, arg0);
        }

        public static void SetControlContentSafe(ContentControl control, object content) {
            control.Dispatcher.Invoke((Action)delegate() { control.Content = content; });
        }

        public static void SetControlIsEnabledSafe(Control control, bool isEnabled) {
            control.Dispatcher.Invoke((Action)delegate() { control.IsEnabled = isEnabled; });
        }

        public static void SetControlTextSafe(TextBox control, string text) {
            control.Dispatcher.Invoke((Action)delegate() { control.Text = text; });
        }

    }

}
