using Bubble.bilibili;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bubble
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //BiliBili bilibili = new BiliBili("7734200");
            //bilibili.run();

            Twitch twitch = new Twitch("shuteye_orange", "oauth:ngtb5qelxsp0anvaorv0x7y5ibmlgm");
            twitch.run();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
