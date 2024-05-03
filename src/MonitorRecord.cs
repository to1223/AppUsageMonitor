using System; // DateTime [mscorlib.dll]

namespace AppUsageMonitor
{
    /// <summary>
    /// 監視結果
    /// </summary>
    public class MonitorRecord
    {
        public DateTime Timestamp { get; private set; }
        public string UserName { get; private set; }
        public bool IsDisplayLocked { get; private set; }
        public bool IsProcessActive { get; private set; }

        public MonitorRecord(
            DateTime timestamp,
            string userName,
            bool isDisplayLocked,
            bool isProcessActive
            )
        {
            Timestamp = timestamp;
            UserName = userName;
            IsDisplayLocked = isDisplayLocked;
            IsProcessActive = isProcessActive;
        }

        /// <summary>
        /// 記録情報を1行の文字列として返す
        /// </summary>
        /// <returns></returns>
        public string ToString()
        {
            return string.Format(
                "{0} {1} {2} {3}",
                Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), // 0
                UserName, // 1
                IsDisplayLocked ? "Locked" : "Unlocked", // 2
                IsProcessActive ? "Up" : "Down" // 3
            );
        }
    }
}