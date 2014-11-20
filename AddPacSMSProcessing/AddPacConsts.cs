namespace AddPacSMSProcessing {

    public static class AddPacConsts {

        /// <summary>
        /// Строка, выводимая в консоль при запросе логина.
        /// </summary>
        public const string LoginRequestedString = "Login:";

        /// <summary>
        /// Строка, выводимая в консоль при запросе пароля.
        /// </summary>
        public const string PasswordRequestedString = "Password:";
        
        /// <summary>
        /// Строка, выводимая в консоль при неуспешной авторизации.
        /// </summary>
        public const string AuthorizationFailedString = "Bad passwords, too many failures!";
        
        /// <summary>
        /// Приглашение командной строки в обычном режиме.
        /// Обычный режим устанавливается после успешной проверки логина и пароля.
        /// Возврат в обычный режим происходит из привилегированного режима командами: "disable", "exit" или "quit".
        /// </summary>
        public const string UnprivilegedModePrompt = "GS1002>";

        /// <summary>
        /// Приглашение командной строки в привилегированном режиме.
        /// Вход в привилегированный режим происходит из обычного режима командой: "enable".
        /// Возврат в привилегированный режим происходит из режима настройки командами: "exit" или "quit".
        /// </summary>
        public const string PrivilegedModePrompt = "GS1002#";

        /// <summary>
        /// Приглашение командной строки в режиме настройки.
        /// Вход в режим настройки происходит из привилегированного режима командой "configure terminal".
        /// </summary>
        public const string ConfigModePrompt = "GS1002(config)#";

        /// <summary>
        /// Неизвестная команда.
        /// </summary>
        public const string UnknownCommand = "Unknown command.";
    
    }

}
