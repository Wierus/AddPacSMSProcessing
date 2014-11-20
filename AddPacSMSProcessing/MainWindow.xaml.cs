using AddPacSMSProcessing.ApplicationLogging;
using System;
using System.ComponentModel;
using System.Windows;

namespace AddPacSMSProcessing {

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private ApplicationLog log;

        private ApplicationSettings settings;

        private AddPacTelnetClient client;

        private DebugWindow debugWindow;

        public MainWindow() {
            this.InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                this.log = ApplicationGlobal.Log;
                this.log.Write(LogSource.Application, "Приложение запущено.");
                this.settings = new ApplicationSettings();
                this.log.Write(LogSource.Application, "Настройки из файла загружены.");
                this.client = new AddPacTelnetClient();
                this.debugWindow = new DebugWindow(this.log, this.settings, this.client);
                this.debugWindow.ShouldHideWindows = true;
                this.client.Connect(this.settings.ServerIPAddress, this.settings.ServerPort);
                this.client.Authorize(this.settings.ServerLogin, this.settings.ServerPassword);
            }
            catch (Exception E) {
                this.log.Write(LogSource.Application, string.Format("Вызвано исключение: {0}, Message: {1}.", E.GetType().FullName, E.Message));
                MessageBox.Show(E.Message, E.GetType().FullName);
                this.Close();
            }
        }
        
        private void Window_Closing(object sender, CancelEventArgs e) {
            this.debugWindow.ShouldHideWindows = false;
            this.debugWindow.Close();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e) {

        }

        private void DebugMenuItem_Click(object sender, RoutedEventArgs e) {
            this.debugWindow.Show();
            this.debugWindow.Activate();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.client.SendSMS(this.settings.DefaultSlotNumber, this.settings.DefaultPortNumber, this.AddressTextBox.Text, this.MessageTextBox.Text);
        }

    }

}
