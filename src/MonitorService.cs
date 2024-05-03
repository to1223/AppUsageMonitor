using Microsoft.Win32; // SystemEvents [System.dll]
using System; // Environment, etc. [mscorlib.dll]
using System.Diagnostics; // Process [System.dll]
using System.IO; // Directory, Path, File [mscorlib.dll]
using System.Threading; // Thread [mscorlib.dll]
using System.Threading.Tasks; // Task [mscorlib.dll]

// [TODO] ユーザー名とPC名は大文字小文字を区別しないので、大文字に正規化する
namespace AppUsageMonitor
{
    // [TODO] イベントを使うので、IDisposableにする。
    public class MonitorService
    {
        // Countup用。常に使用するので、スレッドプールじゃないものを作成。
        private Thread _thread = null;
        private bool _disposed = false;
        private bool _displayLocked = false;
        private MonitorSettings _monitorSettings;

        public MonitorService()
        {
            _monitorSettings = new MonitorSettings();

            // Subscribe SessionSwitch event to detect display lock/unlock.
            SystemEvents.SessionSwitch += this.OnSessionSwitch;
        }

        /// <summary>
        /// 一定周期で、処理を呼び出す。無限ループなので、別スレッドで実行すること。
        /// </summary>
        private void IntervalCheck()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(_monitorSettings.IntervalSeconds * 1000);

                // この変数の使い方問題ない？
                var intervalTask = new Task(
                    () =>
                    {
                        // 監視結果の取得
                        var isProcessActive = Process.GetProcessesByName(_monitorSettings.ProcessName).Length > 0;

                        // 監視結果オブジェクトの作成
                        var record = new MonitorRecord(
                            DateTime.Now,
                            Environment.UserName,
                            _displayLocked,
                            isProcessActive
                        );

                        // ファイルへの書き出し
                        WriteRecord(record);                        
                    }
                );
                intervalTask.Start();
            }
        }

        /// <summary>
        /// ファイルへ書き出す
        /// </summary>
        /// <param name="record"></param>
        private void WriteRecord(MonitorRecord record)
        {
            // 出力用フォルダがない
            // ほんとは例外とか出して、ログ出力させるべき
            if (!System.IO.Directory.Exists(_monitorSettings.OutputFolderPath)) return;
            
            // [TODO] PC名は全部大文字にする
            var outputFileName = string.Format(
                "{0}_{1}.txt",
                record.Timestamp.ToString("yyyy-MM-dd"), // 0
                Environment.MachineName // 1
            );

            // ファイルがなければ作成して追記
            var outputFilePath = Path.Combine(
                _monitorSettings.OutputFolderPath,
                outputFileName
            );
            if (!File.Exists(outputFilePath)) File.Create(outputFilePath);
            using (var writer = new StreamWriter(outputFilePath, append: true))
            {
                writer.WriteLine(record.ToString());
            }
        }

        /// <summary>
        /// スレッドを作成して、モニター開始
        /// </summary>
        public void Start()
        {
            if (_thread != null) return;

            _thread = new Thread(IntervalCheck);
            _thread.Start();
        }

        /// <summary>
        /// スレッドを停止して、モニター停止
        /// </summary>
        public void Dispose()
        {
            // スレッドの停止
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }

            // Unsubscribe SessionSwitch event
            SystemEvents.SessionSwitch -= this.OnSessionSwitch;

            _disposed = true;

        }

        /// <summary>
        /// 画面ロックの検出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    _displayLocked = true;
                    break;

                case SessionSwitchReason.SessionUnlock:
                    _displayLocked = false;
                    break;
            }
        }
    }
}