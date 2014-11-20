using AddPacSMSProcessing.ApplicationLogging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AddPacSMSProcessing {

    public class AddPacTelnetClient {

        #region Константы Telnet-клиента

        /// <summary>
        /// Размер буфера приема.
        /// </summary>
        public const int ReceiveBufferSize = 8 * 1024;

        /// <summary>
        /// Максимальное время ожидания состояния терминала при вызове метода WaitForState (в мс).
        /// </summary>
        public const int WaitForStateTimeout = 1000;

        /// <summary>
        /// Период, через который выполняется проверка состояния терминала при вызове метода WaitForState (в мс).
        /// </summary>
        public const int WaitForStateTimeoutStep = 100;


        #endregion

        #region Поля Telnet-клиента

        private ApplicationLog log;

        /// <summary>
        /// Буфер приема.
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        /// Последнее полученное сообщение, преобразованное в строку из буфера приема в соответствии с заданной кодировкой.
        /// </summary>
        private StringBuilder receivedMessage;

        /// <summary>
        /// Кодировка, используемая для преобразования получаемых данных в строку и передаваемых данных из строки.
        /// </summary>
        private Encoding encoding;

        /// <summary>
        /// Флаг, означающий, что при предыдущем чтении из буфера приема были прочитаны все данные, и при текущем чтении следует очистить полученное сообщение.
        /// </summary>
        private bool shouldClearMessage;

        /// <summary>
        /// TCP-клиент для подключения к Telnet-серверу.
        /// </summary>
        private TcpClient client;

        #endregion

        #region Делегаты Telnet-клиента

        /// <summary>
        /// Делегат для событий, вызываемых при получении входящего сообщения.
        /// </summary>
        /// <param name="message">Входящее сообщение.</param>
        public delegate void MessageHandler(string message);

        #endregion

        #region События Telnet-клиента

        /// <summary>
        /// Событие, вызываемое сразу после получения входящего сообщения.
        /// </summary>
        public event MessageHandler MessageReceived;

        /// <summary>
        /// Событие, вызываемое сразу после отправки исходящего сообщения.
        /// </summary>
        public event MessageHandler MessageSent;

        #endregion

        #region Поля AddPac Telnet-клиента

        /// <summary>
        /// Текущее состояние конечного автомата для обработки входящих сообщений консоли.
        /// </summary>
        private AddPacTerminalState state;

        /// <summary>
        /// Текущий режим команд терминала.
        /// </summary>
        private AddPacCommandMode commandMode;

        /// <summary>
        /// Режим вывода отладочной информации в консоль.
        /// </summary>
        private AddPacDebugOutputMode debugOutputMode;

        #endregion

        #region Делегаты AddPac Telnet-клиента

        /// <summary>
        /// Делегат для события, вызываемого сразу после изменения состояния конечного автомата.
        /// </summary>
        /// <param name="state">Новое состояние конечного автомата.</param>
        public delegate void AddPacStateChangedHandler(AddPacTerminalState state);

        /// <summary>
        /// Делегат для события, вызываемого сразу после изменения режима команд терминала.
        /// </summary>
        /// <param name="mode">Новый режим команд терминала.</param>
        public delegate void AddPacCommandModeChangedHandler(AddPacCommandMode commandMode);

        /// <summary>
        /// Делегат для события, вызываемого сразу после изменения режима вывода отладочной информации в консоль.
        /// </summary>
        /// <param name="debugOutputMode">Новый режим вывода отладочной информации в консоль.</param>
        public delegate void AddPacDebugOutputModeChangeHandler(AddPacDebugOutputMode debugOutputMode);

        #endregion

        #region События AddPac Telnet-клиента

        /// <summary>
        /// Событие, вызываемое сразу после изменения состояния конечного автомата.
        /// </summary>
        public event AddPacStateChangedHandler StateChanged;

        /// <summary>
        /// Событие, вызываемое сразу после изменения режима команд терминала.
        /// </summary>
        public event AddPacCommandModeChangedHandler CommandModeChanged;

        /// <summary>
        /// Событие, вызываемое сразу после изменения режима вывода отладочной информации в консоль.
        /// </summary>
        public event AddPacDebugOutputModeChangeHandler DebugOutputModeChange;

        #endregion

        #region Поля AddPac Telnet-клиента

        /// <summary>
        /// Возвращает текущее состояние конечного автомата для обработки входящих сообщений консоли.
        /// </summary>
        public AddPacTerminalState State {
            get {
                return this.state;
            }
        }

        /// <summary>
        /// Возвращает текущий режим команд терминала.
        /// </summary>
        public AddPacCommandMode CommandMode {
            get {
                return this.commandMode;
            }
        }

        /// <summary>
        /// Возвращает текущий режим вывода отладочной информации в консоль.
        /// </summary>
        public AddPacDebugOutputMode DebugOutputMode {
            get {
                return this.debugOutputMode;
            }
        }

        #endregion

        public AddPacTelnetClient() {
            this.log = ApplicationGlobal.Log;
            this.receiveBuffer = new byte[ReceiveBufferSize];
            this.receivedMessage = new StringBuilder(ReceiveBufferSize);
            this.encoding = Encoding.ASCII;
            this.shouldClearMessage = true;
            this.state = AddPacTerminalState.NotConnected;
            this.commandMode = AddPacCommandMode.UnknownMode;
            this.debugOutputMode = AddPacDebugOutputMode.Unknown;
        }

        #region Методы Telnet-клиента

        /// <summary>
        /// Выполняет подключение к заданному IP-адресу и порту (без отправки данных для авторизации).
        /// </summary>
        /// <param name="ipAddress">IP-адрес сервера.</param>
        /// <param name="port">Порт сервера.</param>
        /// <returns>true, если подключение установлено успешно. false, иначе.</returns>
        public bool Connect(string ipAddress, int port) {
            this.client = new TcpClient();
            this.client.ReceiveBufferSize = ReceiveBufferSize;
            this.SetState(AddPacTerminalState.NotConnected);
            try {
                this.SetState(AddPacTerminalState.Connecting);
                this.client.Connect(ipAddress, port);
                this.SetState(AddPacTerminalState.Connected);
                this.SetDebugOutputMode(AddPacDebugOutputMode.Off);
                this.SetState(AddPacTerminalState.CommandSent);
                this.client.Client.BeginReceive(this.receiveBuffer, 0, ReceiveBufferSize, SocketFlags.None, this.ReceiveCallback, null);
                return true;
            }
            catch (SocketException E) {
                this.log.Write(LogSource.Client, string.Format("При подключении к серверу {0}:{1} вызвано исключение: {2}, Message: {3}, ErrorCode: {4}, SocketErrorCode: {5}, NativeErrorCode: {6}.", ipAddress, port, E.GetType().FullName, E.Message, E.ErrorCode, E.SocketErrorCode, E.NativeErrorCode));
                this.SetState(AddPacTerminalState.ConnectionError);
                return false;
            }
            catch (Exception E) {
                this.log.Write(LogSource.Client, string.Format("При подключении к серверу {0}:{1} вызвано исключение: {2}, Message: {3}.", ipAddress, port, E.GetType().FullName, E.Message));
                this.SetState(AddPacTerminalState.ConnectionError);
                return false;
            }
        }

        public void Disconnect() {
            if (this.client != null) {
                this.client.Close();
            }
        }

        /// <summary>
        /// Метод, вызываемый по завершению операции асинхронного приема данных из терминала.
        /// </summary>
        /// <param name="ar">Состояние асинхронной операции.</param>
        private void ReceiveCallback(IAsyncResult ar) {
            try {
                int bytesRead = this.client.Client.EndReceive(ar);
                int bytesAvailable = this.client.Available;
                if (bytesRead == ReceiveBufferSize) {
                    this.log.Write(LogSource.Client, "Warning! Buffer overflow!");
                }
	            if (bytesRead > 0) {
                    if (this.shouldClearMessage) {
                        this.receivedMessage.Clear();
                    }
                    this.shouldClearMessage = (bytesAvailable == 0);
                    this.receivedMessage.Append(this.encoding.GetString(this.receiveBuffer, 0, bytesRead));
                    if (bytesAvailable == 0) {
                        this.ProcessReceivedMessage(this.receivedMessage.ToString());
                    }
                    this.client.Client.BeginReceive(this.receiveBuffer, 0, ReceiveBufferSize, SocketFlags.None, this.ReceiveCallback, null);
	            }
	            else {
                    this.client.Close();
                    this.SetState(AddPacTerminalState.Disconnected);
                    this.SetCommandMode(AddPacCommandMode.UnknownMode);
                    this.SetDebugOutputMode(AddPacDebugOutputMode.Unknown);
	            }
            }
            catch (SocketException E) {
                this.log.Write(LogSource.Client, string.Format("При приеме данных вызвано исключение: {0}, Message: {1}, ErrorCode: {2}, SocketErrorCode: {3}, NativeErrorCode: {4}.", E.GetType().FullName, E.Message, E.ErrorCode, E.SocketErrorCode, E.NativeErrorCode));
                this.client.Close();
                this.SetState(AddPacTerminalState.Interrupted);
                this.SetCommandMode(AddPacCommandMode.UnknownMode);
                this.SetDebugOutputMode(AddPacDebugOutputMode.Unknown);
                return;
            }
            catch (Exception E) {
                this.log.Write(LogSource.Client, string.Format("При приеме данных вызвано исключение: {0}, Message: {1}.", E.GetType().FullName, E.Message));
                this.client.Close();
                this.SetState(AddPacTerminalState.Interrupted);
                this.SetCommandMode(AddPacCommandMode.UnknownMode);
                this.SetDebugOutputMode(AddPacDebugOutputMode.Unknown);
                return;
            }
        }

        /// <summary>
        /// Обрабатывает входящее сообщение по правилам конечного автомата.
        /// </summary>
        /// <param name="message">Входящее сообщение для обработки.</param>
        private void ProcessReceivedMessage(string message) {
            if (this.MessageReceived != null) {
                this.MessageReceived(message);
            }
            this.SetState(this.GetNextState(message));
            switch (this.state) {
                case AddPacTerminalState.UnprivilegedModePrompted: {
                    this.SetCommandMode(AddPacCommandMode.UnprivilegedMode);
                    break;
                }
                case AddPacTerminalState.PrivilegedModePrompted: {
                    this.SetCommandMode(AddPacCommandMode.PrivilegedMode);
                    break;
                }
                case AddPacTerminalState.ConfigModePrompted: {
                    this.SetCommandMode(AddPacCommandMode.ConfigMode);
                    break;
                }
            }
        }

        /// <summary>
        /// Устанавливает состояние конечного автомата и сразу вызывает событие изменения состояния.
        /// </summary>
        /// <param name="state">Новое состояние конечного автомата.</param>
        private void SetState(AddPacTerminalState state) {
            if (this.state != state) {
                this.state = state;
                if (this.StateChanged != null) {
                    this.StateChanged(this.state);
                }
            }
        }

        /// <summary>
        /// Устанавливает режим команд терминала и сразу вызывает событие изменения режима.
        /// </summary>
        /// <param name="commandMode">Новый режим команд терминала.</param>
        private void SetCommandMode(AddPacCommandMode commandMode) {
            if (this.commandMode != commandMode) {
                this.commandMode = commandMode;
                if (this.CommandModeChanged != null) {
                    this.CommandModeChanged(this.commandMode);
                }
            }
        }

        /// <summary>
        /// Устанавливает режим вывода отладочной информации в консоль.
        /// </summary>
        /// <param name="debugOutputMode">Новый режим вывода отладочной информации в консоль.</param>
        private void SetDebugOutputMode(AddPacDebugOutputMode debugOutputMode) {
            if (this.debugOutputMode != debugOutputMode) {
                this.debugOutputMode = debugOutputMode;
                if (this.DebugOutputModeChange != null) {
                    this.DebugOutputModeChange(this.debugOutputMode);
                }
            }
        }

        /// <summary>
        /// Выполняет отправку команды в консоль терминала с добавлением строки, обозначающей в данной среде начало новой строки.
        /// </summary>
        /// <param name="command">Команда для отправки в заданной кодировке.</param>
        public void SendCommand(string command) {
            try {
                byte[] data = this.encoding.GetBytes(command + Environment.NewLine);
                int bytesSent = this.client.Client.Send(data);
                if (bytesSent != data.Length) {
                    this.log.Write(LogSource.Client, string.Format("При передаче данных были отправлены не все данные. Байт запрошено для передачи: {0}, байт передано: {1}.", data.Length, bytesSent));
                }
                if (this.MessageSent != null) {
                    this.MessageSent(command);
                }
                this.SetState(AddPacTerminalState.CommandSent);
            }
            catch (SocketException E) {
                this.log.Write(LogSource.Client, string.Format("При передаче данных вызвано исключение: {0}, Message: {1}, ErrorCode: {2}, SocketErrorCode: {3}, NativeErrorCode: {4}.", E.GetType().FullName, E.Message, E.ErrorCode, E.SocketErrorCode, E.NativeErrorCode));
                if (this.client != null) {
                    this.client.Close();
                }
                this.SetState(AddPacTerminalState.Interrupted);
            }
            catch (Exception E) {
                this.log.Write(LogSource.Client, string.Format("При приеме данных вызвано исключение: {0}, Message: {1}.", E.GetType().FullName, E.Message));
                if (this.client != null) {
                    this.client.Close();
                }
                this.SetState(AddPacTerminalState.Interrupted);
            }
        }

        /// <summary>
        /// Ожидает пока состояние терминала не станет равным указанному значению, при этом блокирует текущий поток на время не больше (timeout + timeoutStep) мс.
        /// Используется для сценариев обработки команд терминала.
        /// </summary>
        /// <param name="state">Ожидаемое состояние терминала.</param>
        /// <param name="timeout">Максимальное время ожидания состояния терминала, в мс.</param>
        /// <param name="timeoutStep">Период, через который выполняется проверка состояния терминала, в мс.</param>
        /// <returns>true, если состояние терминала стало равно указанному. false, если за время timeout мс состояние терминала ни разу не было равно указанному в периоды через timeoutStep мс.</returns>
        private bool WaitForState(AddPacTerminalState state, int timeout, int timeoutStep) {
            int totalTimeout = 0;
            while (this.state != state) {
                if (totalTimeout >= timeout) {
                    this.log.Write(LogSource.Client, string.Format("Истекло время ожидания запрошенного состояния: {0} ({1} мс).", state.ToString(), timeout));
                    return false;
                }
                Thread.Sleep(timeoutStep);
                totalTimeout += timeoutStep;
            }
            return true;
        }

        /// <summary>
        /// Ожидает пока состояние терминала не станет равным любому значению из заданного списка, при этом блокирует текущий поток на время не больше (timeout + timeoutStep) мс.
        /// Используется для сценариев обработки команд терминала.
        /// </summary>
        /// <param name="state">Список состояний терминала.</param>
        /// <param name="timeout">Максимальное время ожидания состояния терминала, в мс.</param>
        /// <param name="timeoutStep">Период, через который выполняется проверка состояния терминала, в мс.</param>
        /// <returns>true, если состояние терминала стало равно любому из списка. false, если за время timeout мс состояние терминала ни разу не было равно никакому значению из списка в периоды через timeoutStep мс.</returns>
        private bool WaitForStates(AddPacTerminalState[] states, int timeout, int timeoutStep) {
            int totalTimeout = 0;
            while (Array.IndexOf(states, this.state) == -1) {
                if (totalTimeout >= timeout) {
                    this.log.Write(LogSource.Client, string.Format("Истекло время ожидания состояния из запрошенного списка состояний: {0} ({1} мс).", DataTypeUtils.ArrayToLine(states, ", "), timeout));
                    return false;
                }
                Thread.Sleep(timeoutStep);
                totalTimeout += timeoutStep;
            }
            return true;
        }

        #endregion

        #region Методы AddPac Telnet-клиента

        /// <summary>
        /// Вычисление следующего состояния конечного автомата в зависимости от полученного сообщения.
        /// </summary>
        /// <param name="message">Полученное сообщение для обработки.</param>
        /// <returns>Следующее состояние конечного автомата.</returns>
        private AddPacTerminalState GetNextState(string message) {
            switch (this.state) {
                case AddPacTerminalState.CommandSent: {
                    if (message.Contains(AddPacConsts.LoginRequestedString)) {
                        return AddPacTerminalState.LoginRequested;
                    }
                    else if (message.Contains(AddPacConsts.PasswordRequestedString)) {
                        return AddPacTerminalState.PasswordRequested;
                    }
                    else if (message.Contains(AddPacConsts.AuthorizationFailedString)) {
                        return AddPacTerminalState.AuthorizationFailed;
                    }
                    else if (message.Contains(AddPacConsts.UnprivilegedModePrompt)) {
                        return AddPacTerminalState.UnprivilegedModePrompted;
                    }
                    else if (message.Contains(AddPacConsts.PrivilegedModePrompt)) {
                        return AddPacTerminalState.PrivilegedModePrompted;
                    }
                    else if (message.Contains(AddPacConsts.ConfigModePrompt)) {
                        return AddPacTerminalState.ConfigModePrompted;
                    }
                    break;
                }
            }
            return AddPacTerminalState.Unknown;
        }

        /// <summary>
        /// Сценарий, выполняющий авторизацию с указанными логином и паролем.
        /// </summary>
        /// <param name="login">Логин для входа.</param>
        /// <param name="password">Пароль.</param>
        /// <returns>true, если авторизация прошла успешно. false, иначе.</returns>
        public bool Authorize(string login, string password) {
            if (!this.WaitForState(AddPacTerminalState.LoginRequested, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                return false;
            }
            this.SendCommand(login);
            if (!this.WaitForState(AddPacTerminalState.PasswordRequested, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                return false;
            }
            this.SendCommand(password);
            if (!this.WaitForState(AddPacTerminalState.UnprivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Сценарий, активирующий обычный режим режим из привилегированного режима или из режима настройки.
        /// </summary>
        /// <returns>true, если обычный режим активирован успешно. false, иначе.</returns>
        public bool ActivateUnprivilegedMode() {
            switch (this.commandMode) {
                case AddPacCommandMode.UnprivilegedMode: {
                    return true;
                }
                case AddPacCommandMode.PrivilegedMode: {
                    this.SendCommand(AddPacCommands.Exit);
                    if (!this.WaitForState(AddPacTerminalState.UnprivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                case AddPacCommandMode.ConfigMode: {
                    this.SendCommand(AddPacCommands.Exit);
                    if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    this.SendCommand(AddPacCommands.Exit);
                    if (!this.WaitForState(AddPacTerminalState.UnprivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                default: {
                    return false;
                }
            }
        }

        /// <summary>
        /// Сценарий, активирующий привилегированный режим из обычного режима или режима настройки.
        /// </summary>
        /// <returns>true, если привилегированный режим активирован успешно. false, иначе.</returns>
        public bool ActivatePrivilegedMode() {
            switch (this.commandMode) {
                case AddPacCommandMode.UnprivilegedMode: {
                    this.SendCommand(AddPacCommands.ActivatePrivilegedMode);
                    if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                case AddPacCommandMode.PrivilegedMode: {
                    return true;
                }
                case AddPacCommandMode.ConfigMode: {
                    this.SendCommand(AddPacCommands.Exit);
                    if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                default: {
                    return false;
                }
            }
        }

        /// <summary>
        /// Сценарий, активирующий режим настройки из обычного режима или привилегированного режима.
        /// </summary>
        /// <returns>true, если режим настройки активирован успешно. false, иначе.</returns>
        public bool ActivateConfigMode() {
            switch (this.commandMode) {
                case AddPacCommandMode.UnprivilegedMode: {
                    this.SendCommand(AddPacCommands.ActivatePrivilegedMode);
                    if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    this.SendCommand(AddPacCommands.ActivateConfigMode);
                    if (!this.WaitForState(AddPacTerminalState.ConfigModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                case AddPacCommandMode.PrivilegedMode: {
                    this.SendCommand(AddPacCommands.ActivateConfigMode);
                    if (!this.WaitForState(AddPacTerminalState.ConfigModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                        return false;
                    }
                    return true;
                }
                case AddPacCommandMode.ConfigMode: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }

        /// <summary>
        /// Включает вывод отладочной информации в консоль из обычного режима, привилегированного режима или режима настройки.
        /// </summary>
        /// <returns>true, если вывод отладочной информации в консоль включен успешно. false, иначе.</returns>
        public bool EnableDebugOutput() {
            if (this.commandMode != AddPacCommandMode.UnknownMode) {
                if (this.debugOutputMode == AddPacDebugOutputMode.On) {
                    return true;
                }
                if (!this.ActivatePrivilegedMode()) {
                    return false;
                }
                this.SendCommand(AddPacCommands.EnableDebugOutput);
                if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                    return false;
                }
                this.SetDebugOutputMode(AddPacDebugOutputMode.On);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Выключает вывод отладочной информации в консоль из обычного режима, привилегированного режима или режима настройки.
        /// </summary>
        /// <returns>true, если вывод отладочной информации в консоль выключен успешно. false, иначе.</returns>
        public bool DisableDebugOutput() {
            if (this.commandMode != AddPacCommandMode.UnknownMode) {
                if (this.debugOutputMode == AddPacDebugOutputMode.Off) {
                    return true;
                }
                if (!this.ActivatePrivilegedMode()) {
                    return false;
                }
                this.SendCommand(AddPacCommands.DisableDebugOutput);
                if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                    return false;
                }
                this.SetDebugOutputMode(AddPacDebugOutputMode.Off);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Отправляет SMS-сообщение на указанный номер с заданным текстом.
        /// </summary>
        /// <param name="slotNumber">Номер слота.</param>
        /// <param name="portNumber">Номер порта.</param>
        /// <param name="address">Номер телефона.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>true, если SMS-сообщение успешно отправлено. false, иначе.</returns>
        public bool SendSMS(int slotNumber, int portNumber, string address, string message) {
            if (this.commandMode != AddPacCommandMode.UnknownMode) {
                if (!this.ActivatePrivilegedMode()) {
                    return false;
                }
                this.SendCommand(string.Format(AddPacCommands.MobileSMSMessageSend, slotNumber, portNumber, address, message));
                if (!this.WaitForState(AddPacTerminalState.PrivilegedModePrompted, WaitForStateTimeout, WaitForStateTimeoutStep)) {
                    return false;
                }
                return true;
            }
            return false;
        }

        #endregion

    }

}
