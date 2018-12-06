using System.IO;
using System.Windows;
using ArrayDisplay.File;
using ArrayDisplay.net;
using log4net;
using log4net.Config;

namespace ArrayDisplay {
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public class App : Application {
        public static ILog log = LogManager.GetLogger("MyLogger");
        public Dataproc proc;

        protected override void OnStartup(StartupEventArgs e) {
            RelativeDirectory rd = new RelativeDirectory();
            string log4NetConfigFilePath = Path.Combine(rd.Path, "Log4net\\Log4net.config");
            XmlConfigurator.ConfigureAndWatch(new FileInfo(log4NetConfigFilePath));
            
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
        }
    }
}
