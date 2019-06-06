using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FindStockService
{
    public partial class Service1 : ServiceBase
    {
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Config                                                          |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private static Plog log = new Plog();
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Constructor                                                     |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public Service1()
        {
            InitializeComponent();
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Helper Function                                                 |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public void OnDebug()
        {
            this.OnStart(null);
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | OnStart Function                                                |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        protected override void OnStart(string[] args)
        {
            log.LOGI("Service starting");
            this.timer1.Start();
            this.timer2.Start();
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | OnStop Function                                                 |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        protected override void OnStop()
        {
            log.LOGI("Service stop");
            this.timer1.Stop();
            this.timer2.Stop();
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Timer Function                                                  |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private void Timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var getDataKaohoon = new FundamentalKaohoon();
            getDataKaohoon.Run();
        }
        private void Timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var getDataNews = new FundamentalNews();
            getDataNews.Run();
        }
    }
}
