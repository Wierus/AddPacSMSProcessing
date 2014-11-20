using System;
using System.IO;

namespace AddPacSMSProcessing.ApplicationLogging {

    public class ApplicationLog {

        /// <summary>
        /// Имя директории с лог-файлами.
        /// </summary>
        public const string LogFilesDirectoryName = "Logs";

        /// <summary>
        /// Шаблон имени лог-файла.
        /// {0} - LogFileDateTemplate
        /// </summary>
        public const string LogFileNameTemplate = "AddPacSMSProcessing-{0}.log";

        /// <summary>
        /// Шаблон даты для имени лог-файла.
        /// </summary>
        public const string LogFileDateTemplate = "yyyy.MM.dd";

        /// <summary>
        /// Шаблон сообщения в лог-файле.
        /// {0} - DateTime
        /// {1} - LogSource
        /// {2} - Message
        /// </summary>
        public const string MessagePattern = "{0:yyyy.MM.dd HH:mm:ss.fff} [{1,-11}] {2}";

        /// <summary>
        /// Поток, используемый для записи в лог-файл.
        /// </summary>
        private static StreamWriter logWriter;

        private static DateTime currentDate;

        /// <summary>
        /// Событие, вызываемое при записи в лог.
        /// </summary>
        public event Action<string> WriteEvent;

        private static object thisLock = new object();

        public ApplicationLog() {
            Directory.CreateDirectory(LogFilesDirectoryName);
            currentDate = DateTime.Now.Date;
            logWriter = new StreamWriter(this.GetLogFileRelativePath(currentDate), true);
        }

        private string GetLogFileRelativePath(DateTime date) {
            return LogFilesDirectoryName + Path.DirectorySeparatorChar + string.Format(LogFileNameTemplate, date.ToString(LogFileDateTemplate));
        }

        public void Write(LogSource source, string message) {
            lock (thisLock) {
                // если новая дата
                if (currentDate.Date != DateTime.Now.Date) {
                    logWriter.WriteLine(string.Format(MessagePattern, DateTime.Now, LogSource.Log, "Для текущего дня лог-файл сейчас будет закрыт. Будет создан лог-файл для нового дня."));
                    // закрыть текущий лог-файл
                    logWriter.Close();
                    // создать новый лог-файл с новым именем
                    currentDate = DateTime.Now.Date;
                    logWriter = new StreamWriter(GetLogFileRelativePath(currentDate), true);
                    logWriter.WriteLine(string.Format(MessagePattern, DateTime.Now, LogSource.Log, "Для нового дня лог-файл только что был создан."));
                }
                if (!message.EndsWith(Environment.NewLine)) {
                    message += Environment.NewLine;
                }
                message = string.Format(MessagePattern, DateTime.Now, source, message);
                logWriter.Write(message);
                logWriter.Flush();

                Console.Write(message);
            }
            if (this.WriteEvent != null) {
                this.WriteEvent(message);
            }
        }
        
    }

}
