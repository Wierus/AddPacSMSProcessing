using System;
using System.Xml;

namespace AddPacSMSProcessing {

    /// <summary>
    /// Настройки программы, загружаемые из XML-файла.
    /// </summary>
    public class ApplicationSettings {

        /// <summary>
        /// Имя XML-файла настроек.
        /// </summary>
        public const string SettingsFileName = "Settings.xml";

        /// <summary>
        /// XML-документ.
        /// </summary>
        private XmlDocument xmlDocument;

        /// <summary>
        /// Корневой элемент XML-документа.
        /// </summary>
        private XmlElement rootElement;

        public const string ServerIPAddressSettingName = "ServerIPAddress";

        public const string ServerPortSettingName      = "ServerPort";

        public const string ServerLoginSettingName     = "ServerLogin";

        public const string ServerPasswordSettingName  = "ServerPassword";

        public const string DefaultSlotNumberName      = "DefaultSlotNumber";

        public const string DefaultPortNumberName      = "DefaultPortNumber";

        /// <summary>
        /// IP-адрес сервера.
        /// </summary>
        private string serverIPAddress;

        /// <summary>
        /// Порт сервера для Telnet-соединения.
        /// </summary>
        private int serverPort;

        /// <summary>
        /// Логин для входа.
        /// </summary>
        private string serverLogin;

        /// <summary>
        /// Пароль.
        /// </summary>
        private string serverPassword;

        /// <summary>
        /// Номер слота по умолчанию, используемого при работе с мобильным интерфейсом.
        /// </summary>
        private int defaultSlotNumber;

        /// <summary>
        /// Номер порта по умолчанию, используемого при работе с мобильным интерфейсом.
        /// </summary>
        private int defaultPortNumber;

        public string ServerIPAddress {
            get {
                return this.serverIPAddress;
            }
        }

        public int ServerPort {
            get {
                return this.serverPort;
            }
        }

        public string ServerLogin {
            get {
                return this.serverLogin;
            }
        }

        public string ServerPassword {
            get {
                return this.serverPassword;
            }
        }


        public int DefaultSlotNumber {
            get {
                return this.defaultSlotNumber;
            }
        }

        public int DefaultPortNumber {
            get {
                return this.defaultPortNumber;
            }
        }

        /// <summary>
        /// Конструктор настроек.
        /// Загружает настройки из файла.
        /// </summary>
        public ApplicationSettings() {
            this.xmlDocument = new XmlDocument();
            this.xmlDocument.Load(SettingsFileName);
            this.rootElement = this.xmlDocument.DocumentElement;
            this.LoadSettingsFromXmlDocument();
        }

        /// <summary>
        /// Загружает настройки из XML-документа.
        /// </summary>
        private void LoadSettingsFromXmlDocument() {
            XmlNode settingNode;
            // ServerIPAddress
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", ServerIPAddressSettingName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", ServerIPAddressSettingName));
            }
            this.serverIPAddress = settingNode.Value;
            // ServerPort
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", ServerPortSettingName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", ServerPortSettingName));
            }
            if (!int.TryParse(settingNode.Value, out this.serverPort)) {
                throw new FormatException(string.Format("Неверное значение элемента \"{0}\" при загрузке из файла настроек.", ServerPortSettingName));
            }
            // ServerLogin
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", ServerLoginSettingName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", ServerLoginSettingName));
            }
            this.serverLogin = settingNode.Value;
            // ServerPassword
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", ServerPasswordSettingName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", ServerPasswordSettingName));
            }
            this.serverPassword = settingNode.Value;
            // DefaultSlotNumber
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", DefaultSlotNumberName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", DefaultSlotNumberName));
            }
            if (!int.TryParse(settingNode.Value, out this.defaultSlotNumber)) {
                throw new FormatException(string.Format("Неверное значение элемента \"{0}\" при загрузке из файла настроек.", DefaultSlotNumberName));
            }
            // DefaultPortNumber
            settingNode = this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/@value", DefaultPortNumberName));
            if (settingNode == null) {
                throw new XmlException(string.Format("Элемент \"{0}\" не найден в файле настроек.", DefaultPortNumberName));
            }
            if (!int.TryParse(settingNode.Value, out this.defaultPortNumber)) {
                throw new FormatException(string.Format("Неверное значение элемента \"{0}\" при загрузке из файла настроек.", DefaultPortNumberName));
            }
        }

        /// <summary>
        /// Сохраняет настройки в XML-документ.
        /// </summary>
        private void SaveSettingsToXmlDocument() {
            // ServerIPAddress
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", ServerIPAddressSettingName)).InnerText = this.serverIPAddress;
            // ServerPort
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", ServerPortSettingName)).InnerText = this.serverPort.ToString();
            // ServerLogin
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", ServerLoginSettingName)).InnerText = this.serverLogin;
            // ServerPassword
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", ServerPasswordSettingName)).InnerText = this.serverPassword;
            // DefaultSlotNumber
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", DefaultSlotNumberName)).InnerText = this.defaultSlotNumber.ToString();
            // DefaultPortNumber
            this.rootElement.SelectSingleNode(string.Format("Setting[@name=\"{0}\"]/value", DefaultPortNumberName)).InnerText = this.defaultPortNumber.ToString();
        }

        /// <summary>
        /// Сохраняет настройки в файл.
        /// </summary>
        public void SaveSettingsToFile() {
            this.SaveSettingsToXmlDocument();
            this.xmlDocument.Save(SettingsFileName);
        }

    }

}
