using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace AspireApp.ServiceDefaults.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public static class Utils
    {
        public const string StandardDateTimeFormat = "yyyy-MM-dd_HH:mm:ss:fff";

        public static void EventLog(string entryType, string message)
        {
            Console.WriteLine(MaskedwithTimestamp(message, entryType.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="method"></param>
        /// <param name="message"></param>
        /// <param name="recipient"></param>
        /// <param name="node"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string MaskedwithTimestamp(string message, string logLevel = "", string sender = "", string recipient = "", DateTime? dateTime = null)
        {
            if (!string.IsNullOrEmpty(logLevel)) logLevel = $"[{logLevel}]";
            if (!string.IsNullOrEmpty(sender)) sender = $"{sender}: ";
            if (!string.IsNullOrEmpty(recipient)) recipient = $" -> {recipient}";
            if (dateTime == null) dateTime = DateTime.Now;

            return $"{logLevel} {((DateTime)dateTime).ToString(StandardDateTimeFormat)} {sender}{message}{recipient}";
        }

    }
}
