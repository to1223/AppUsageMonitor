using System; // Environment, etc. [mscorlib.dll], Uri [System.dll]
using System.Drawing; // Icon [System.Drawing.dll]
using System.Threading; // Mutex [mscorlib.dll]
using System.Windows; // Application, MessageBox [PresentationFramework.dll]
using System.Windows.Forms; // NotifyIcon etc. [System.Windows.Forms.dll]

namespace AppUsageMonitor
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //常駐終了時に開放するために保存しておく
        private System.Windows.Forms.ContextMenuStrip _menu;
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private MonitorService _monitorService;

        // 2重起動を防止
        private const string _mutexName = "AppUsageMonitor";
        // ここでnewせず、Startupでnewした方が、リソース管理しやすい？
        private Mutex _mutex = new Mutex(false, _mutexName);

        protected override void OnStartup(StartupEventArgs e)
        {
            // Mutexの所有権を要求
            if (!_mutex.WaitOne(0, false))
            {
                // [TODO] ログ出力。とりあえずMessageBox出す？
                System.Windows.MessageBox.Show(
                    "AppUsageMonitorは既に起動しています。",
                    "二重起動防止",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                // インスタンスが所有しているリソースを開放
                _mutex.Close();
                _mutex = null;

                // アプリケーションを終了
                Environment.Exit(0);
            }

            // 設定ファイルを読み込み、監視対象PCでない場合はアプリケーションを終了する
            var monitorSettings = new MonitorSettings();
            if (!monitorSettings.IsTargetPC(Environment.MachineName))
            {
                // Debug用
                System.Windows.MessageBox.Show(
                    "監視対象PCではないので終了します",
                    "監視対象外",
                    MessageBoxButton.OK
                );

                // インスタンスが所有しているリソースを開放
                _mutex.Close();
                _mutex = null;

                // アプリケーションを終了
                Environment.Exit(0);
            }

            // 出力フォルダが存在しない場合、終了？またはそのまま走らせて、例外処理？

            //継承元のOnStartupを呼び出す
            base.OnStartup(e);

            // [TODO] この辺の処理をInitializeTaskTrayUIとかにして切り出したい

            //アイコンの取得
            var icon = GetResourceStream(new Uri("Resources/Icons/monitor_eye_icon_138368/monitor_eye_icon_138368_64px.ico", UriKind.Relative)).Stream;

            //コンテキストメニューを作成
            _menu = CreateMenu();

            //通知領域にアイコンを表示
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = new System.Drawing.Icon(icon),
                Text = "AppUsageMonitor",
                ContextMenuStrip = _menu
            };

            // モニタリング開始
            _monitorService = new MonitorService();
            _monitorService.Start();
        }

        /// <summary>
        /// 終了時の処理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            _monitorService.Dispose();
            _menu.Dispose();
            _notifyIcon.Dispose();

            // Mutexの開放
            if (_mutex != null)
            {
                // 共有リソースであるMutexを開放
                _mutex.ReleaseMutex();

                // インスタンスが所有しているリソースを開放
                _mutex.Close();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// コンテキストメニューの表示
        /// </summary>
        /// <returns></returns>
        private ContextMenuStrip CreateMenu()
        {
            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add("AppUsageMonitorを終了", null, (s, e) => { Shutdown(); });
            return menu;
        }
    }
}
