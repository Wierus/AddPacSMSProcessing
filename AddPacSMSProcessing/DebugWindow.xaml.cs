using AddPacSMSProcessing.ApplicationLogging;
using System.ComponentModel;
using System.Windows;

namespace AddPacSMSProcessing {

    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window {

        private ApplicationLog log;
        
        private ApplicationSettings settings;

        private AddPacTelnetClient client;

        /// <summary>
        /// Флаг, показывающий следует ли прятать окно вместо его закрытия (при вызове метода Close).
        /// </summary>
        private bool shouldHideWindow;

        public bool ShouldHideWindows {
            get {
                return this.shouldHideWindow;
            }
            set {
                this.shouldHideWindow = value;
            }
        }

        public DebugWindow() {
            this.InitializeComponent();
        }

        public DebugWindow(ApplicationLog log, ApplicationSettings settings, AddPacTelnetClient client) : this() {
            this.log = log;
            this.settings = settings;
            this.client = client;
            this.log.WriteEvent += this.log_WriteEvent;
            ControlSafeMethods.InvokeControlActionSafe<string>(this.StateLabel, this.RefreshStateLabel, this.client.State.ToString());
            ControlSafeMethods.InvokeControlActionSafe<string>(this.CommandModeLabel, this.RefreshCommandModeLabel, this.client.CommandMode.ToString());
            ControlSafeMethods.InvokeControlActionSafe<string>(this.DebugOutputModeLabel, this.RefreshDebugOutputModeLabel, this.client.DebugOutputMode.ToString());
            this.client.MessageReceived       += this.client_MessageReceived;
            this.client.MessageSent           += this.client_MessageSent;
            this.client.StateChanged          += this.client_StateChanged;
            this.client.CommandModeChanged    += this.client_CommandModeChanged;
            this.client.DebugOutputModeChange += this.client_DebugOutputModeChange;
            this.ServerAddress.Content = string.Format("{0}:{1}", this.settings.ServerIPAddress, this.settings.ServerPort);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.LogTextBox.ScrollToEnd();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (this.shouldHideWindow) {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void log_WriteEvent(string message) {
            ControlSafeMethods.BeginInvokeControlActionSafe<string>(this.LogTextBox, this.AppendMessageToLogTextBox, message);
        }

        /// <summary>
        /// Добавляет сообщение в лог.
        /// </summary>
        /// <param name="message">Сообщение, добавляемое в лог.</param>
        private void AppendMessageToLogTextBox(string message) {
            this.LogTextBox.AppendText(message);
            this.LogTextBox.ScrollToEnd();
        }
        
        private void RefreshStateLabel(string state) {
            this.StateLabel.Content = state;
        }

        private void RefreshCommandModeLabel(string commandMode) {
            this.CommandModeLabel.Content = commandMode;
        }

        private void RefreshDebugOutputModeLabel(string debugOutputMode) {
            this.DebugOutputModeLabel.Content = debugOutputMode;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Подключение к серверу {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.Connect(this.settings.ServerIPAddress, this.settings.ServerPort)) {
                this.log.Write(LogSource.Application, string.Format("При подключении к серверу {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Подключение к серверу {0}:{1} установлено успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Отключение от сервера {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            this.client.Disconnect();
            this.log.Write(LogSource.Application, string.Format("Отключение от сервера {0}:{1} выполнено успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void AuthorizeButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Авторизация на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.Authorize(this.settings.ServerLogin, this.settings.ServerPassword)) {
                this.log.Write(LogSource.Application, string.Format("При авторизации на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Авторизация на сервере {0}:{1} выполнена успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void ActivateUnprivilegedModeButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Активация обычного режима на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.ActivateUnprivilegedMode()) {
                this.log.Write(LogSource.Application, string.Format("При активации обычного режима на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Активация обычного режима на сервере {0}:{1} выполнена успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void ActivatePrivilegedModeButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Активация привилегированного режима на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.ActivatePrivilegedMode()) {
                this.log.Write(LogSource.Application, string.Format("При активации привилегированного режима на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Активация привилегированного режима на сервере {0}:{1} выполнена успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void ActivateConfigModeButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Активация режима настройки на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.ActivateConfigMode()) {
                this.log.Write(LogSource.Application, string.Format("При активации режима настройки на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Активация режима настройки на сервере {0}:{1} выполнена успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void EnableDebugOutputButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Включение вывода отладочной информации в консоль на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.EnableDebugOutput()) {
                this.log.Write(LogSource.Application, string.Format("При включении вывода отладочной информации в консоль на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Включение вывода отладочной информации в консоль на сервере {0}:{1} выполнено успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void DisableDebugOutputButton_Click(object sender, RoutedEventArgs e) {
            this.log.Write(LogSource.Application, string.Format("Выключение вывода отладочной информации в консоль на сервере {0}:{1}...", this.settings.ServerIPAddress, this.settings.ServerPort));
            if (!this.client.DisableDebugOutput()) {
                this.log.Write(LogSource.Application, string.Format("При выключении вывода отладочной информации в консоль на сервере {0}:{1} произошла ошибка.", this.settings.ServerIPAddress, this.settings.ServerPort));
                return;
            }
            this.log.Write(LogSource.Application, string.Format("Выключение вывода отладочной информации в консоль на сервере {0}:{1} выполнено успешно.", this.settings.ServerIPAddress, this.settings.ServerPort));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) {
            this.client.SendCommand(this.CommandTextBox.Text);
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e) {
            this.LogTextBox.Clear();
        }

        private void client_MessageReceived(string message) {
            this.log.Write(LogSource.Client, string.Format("RX: {0}", message));
        }

        private void client_MessageSent(string message) {
            this.log.Write(LogSource.Client, string.Format("TX: {0}", message));
        }

        private void client_StateChanged(AddPacTerminalState state) {
            this.log.Write(LogSource.Client, string.Format("State changed to: {0}", state));
            ControlSafeMethods.InvokeControlActionSafe<string>(this.StateLabel, this.RefreshStateLabel, state.ToString());
        }

        private void client_CommandModeChanged(AddPacCommandMode commandMode) {
            this.log.Write(LogSource.Client, string.Format("Command mode changed to: {0}", commandMode));
            ControlSafeMethods.InvokeControlActionSafe<string>(this.CommandModeLabel, this.RefreshCommandModeLabel, commandMode.ToString());
        }

        private void client_DebugOutputModeChange(AddPacDebugOutputMode debugOutputMode) {
            this.log.Write(LogSource.Client, string.Format("Debug output mode changed to: {0}", debugOutputMode));
            ControlSafeMethods.InvokeControlActionSafe<string>(this.DebugOutputModeLabel, this.RefreshDebugOutputModeLabel, debugOutputMode.ToString());
        }

    }

}
