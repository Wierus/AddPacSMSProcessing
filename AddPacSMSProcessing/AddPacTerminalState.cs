namespace AddPacSMSProcessing {

    /// <summary>
    /// Состояния терминала до или после приема команды.
    /// </summary>
    public enum AddPacTerminalState {

        #region Состояния Telnet-клиента

        /// <summary>
        /// Терминал не подключен.
        /// Значение устанавливается при инициализации терминала.
        /// </summary>
        NotConnected,

        /// <summary>
        /// Отправлен запрос на подключение, ожидание ответа.
        /// </summary>
        Connecting,

        /// <summary>
        /// Подключение установлено.
        /// </summary>
        Connected,

        /// <summary>
        /// Произошла ошибка при подключении.
        /// </summary>
        ConnectionError,

        /// <summary>
        /// Подключение отключено.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Подключение прервано.
        /// </summary>
        Interrupted,

        /// <summary>
        /// Отправлена команда, ожидание ответа.
        /// </summary>
        CommandSent,

        /// <summary>
        /// Состояние терминала не определено.
        /// </summary>
        Unknown,

        #endregion

        #region Состояния AddPac Telnet-клиента

        /// <summary>
        /// Запрос логина для подключения.
        /// </summary>
        LoginRequested,

        /// <summary>
        /// Запрос пароля для подключения.
        /// </summary>
        PasswordRequested,

        /// <summary>
        /// Неуспешная авторизация.
        /// </summary>
        AuthorizationFailed,

        /// <summary>
        /// Запрос команды в обычном режиме.
        /// </summary>
        UnprivilegedModePrompted,

        /// <summary>
        /// Запрос команды в привилегированном режиме.
        /// </summary>
        PrivilegedModePrompted,

        /// <summary>
        /// Запрос команды в режиме настройки.
        /// </summary>
        ConfigModePrompted,

        #endregion

    }

}
