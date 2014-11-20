using System.Collections.Specialized;
using System.Text;

namespace AddPacSMSProcessing {

    public static class DataTypeUtils {

        public static string ArrayToLine<T>(T[] array, string separator) {
            if (array == null) {
                return null;
            }
            if (array.Length == 0) {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length - 1; i++) {
                sb.Append(array[i]).Append(separator);
            }
            sb.Append(array[array.Length - 1]);
            return sb.ToString();
        }

        public static string StringCollectionToLine(StringCollection collection, string separator) {
            if (collection == null) {
                return null;
            }
            if (collection.Count == 0) {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < collection.Count - 1; i++) {
                sb.Append(collection[i]).Append(separator);
            }
            sb.Append(collection[collection.Count - 1]);
            return sb.ToString();
        }

    }

}
