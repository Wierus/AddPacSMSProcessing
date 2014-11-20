namespace AddPacSMSProcessing {

    public static class AddPacCommands {

        /// <summary>
        /// Активирует привилегированный режим из обычного режима.
        /// </summary>
        public const string ActivatePrivilegedMode = "enable";

        /// <summary>
        /// Активирует режим настройки из привилегированного режима.
        /// </summary>
        public const string ActivateConfigMode = "configure terminal";

        /// <summary>
        /// Возвращает в предыдущий режим или закрывает сеанс консоли.
        /// </summary>
        public const string Exit = "exit";

        /// <summary>
        /// Включает вывод отладочной информации в консоль.
        /// Выполняется из привилегированного режима.
        /// </summary>
        public const string EnableDebugOutput = "terminal monitor";

        /// <summary>
        /// Выключает вывод отладочной информации в консоль.
        /// Выполняется из привилегированного режима.
        /// </summary>
        public const string DisableDebugOutput = "no terminal monitor";

        /// <summary>
        /// Выключает вывод любой отладочной информации.
        /// Выполняется из привилегированного режима.
        /// </summary>
        public const string DisableDebugAll = "no debug all";

        /// <summary>
        /// Отправляет SMS-сообщение на указанный номер с заданным текстом.
        /// Выполняется из привилегированного режима.
        /// {0} - Номер слота &lt;0-0&gt;.
        /// {1} - Номер порта &lt;0-1&gt;.
        /// {2} - Номер телефона.
        /// {3} - Текст сообщения.
        /// </summary>
        public const string MobileSMSMessageSend = "mobile {0} {1} sms message send {2} {3}";

        /// <summary>
        /// Получает уровень сигнала RSSI (Received Signal Strength & Quality).
        /// {0} - Номер слота &lt;0-0&gt;.
        /// {1} - Номер порта &lt;0-1&gt;.
        /// </summary>
        public const string ShowMobileRSSI = "show mobile {0} {1} rssi";

    }

}
