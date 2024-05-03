using System; // Convert [mscorlib.dll]
using System.Collections.Generic; // HashSet<T> [System.Core.dll]
using System.IO; // Path, StreamReader [mscorlib.dll]

namespace AppUsageMonitor
{
    public class MonitorSettings
    {
        private HashSet<string> _targetPCs;

        public string OutputFolderPath {get; private set;}
        public string ProcessName {get; private set;}
        public int IntervalSeconds {get; private set;}

        public MonitorSettings()
        {
            ReadFiles();
        }

        private void ReadFiles()
        {
            // 設定ファイル置き場。
            var settingFolderPath = @"settings";

            // Output folder path
            var fileName = "output_folder";
            using (var reader = new StreamReader(Path.Combine(settingFolderPath, fileName)))
            {
                OutputFolderPath = reader.ReadLine();
            }

            // Process name
            fileName = "process_name";
            using (var reader = new StreamReader(Path.Combine(settingFolderPath, fileName)))
            {
                ProcessName = reader.ReadLine();
            }

            // Interval
            fileName = "interval_seconds";
            using (var reader = new StreamReader(Path.Combine(settingFolderPath, fileName)))
            {
                IntervalSeconds = Convert.ToInt32(reader.ReadLine());
            }

            // Target PC list
            // [TODO] #でコメントアウトとか、*で全部対象とかできるようにしたい
            _targetPCs = new HashSet<string>();
            fileName = "target_pc";
            using (var reader = new StreamReader(Path.Combine(settingFolderPath, fileName)))
            {
                while (!reader.EndOfStream)
                {
                    _targetPCs.Add(reader.ReadLine());
                }
            }
        }

        public bool IsTargetPC(string pcName)
        {
            return _targetPCs.Contains(pcName);
        }
    }
}