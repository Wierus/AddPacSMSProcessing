namespace AddPacSMSProcessing {

    /// <summary>
    /// Режимы команд терминала.
    /// </summary>
    public enum AddPacCommandMode {

        /// <summary>
        /// Обычный режим.
        /// Определяется приглашением командной строки, указанном в константе UnprivilegedModePrompt.
        /// </summary>
        UnprivilegedMode,

        /// <summary>
        /// Привилегированный режим.
        /// Определяется приглашением командной строки, указанном в константе PrivilegedModePrompt.
        /// </summary>
        PrivilegedMode,

        /// <summary>
        /// Режим настройки.
        /// Определяется приглашением командной строки, указанном в константе ConfigModePrompt.
        /// </summary>
        ConfigMode,

        /// <summary>
        /// Неизвестный режим.
        /// </summary>
        UnknownMode,

    }

}
