using AddPacSMSProcessing.ApplicationLogging;

namespace AddPacSMSProcessing {

    public static class ApplicationGlobal {

        public static readonly ApplicationLog Log;

        static ApplicationGlobal() {
            Log = new ApplicationLog();
        }

    }

}
