using Bubble.model;
using Bubble.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Bubble
{
    public partial class MainForm : Form
    {
        

        public Speaker_DM speaker_dm;

        public struct Speaker_DM
        {
            public List<string> namelist;
            public int dm_count;
            public int speaker_count;
            public int max_audience;
        }

        public object Dispatcher { get; private set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
             
            if (button1.Text == "connect")
            {
                Console.WriteLine("the gui thread is: " + Thread.CurrentThread.ManagedThreadId.ToString());

                LiveServerImp server = null;
                switch (comboBox1.Text)
                {
                    case "bilibili":
                        server = new BiliBili(textBox1.Text);
                        LiveServerUtil.addServer("bilibili", server);
                        break;
                    case "twitch":
                        server = new Twitch(textBox1.Text, "oauth:hwrxd2lx25h3i327bcm8u6w78lii9i");
                        LiveServerUtil.addServer("twitch", server);
                        break;
                    default: return;
                }
                
                //BiliBili bilibili = new BiliBili("5210066");
                //LiveServerUtil.addServer("bilibili",bilibili);
                LiveServerUtil.runAllServer();
                
                button1.Text = "disconnect";
            }
            else if (button1.Text == "disconnect")
            {
                LiveServerUtil.stopAllServer();

                button1.Text = "connect";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FormUtil.formManager.Add("mainForm", this);
        }

        private delegate void dm_delegate(EnumCommentType dm_type, string name, string msg);
        public void dm_invoke(EnumCommentType dm_type, string name, string msg)
        {
            this.Invoke(new dm_delegate(dm_get), dm_type, name, msg);
        }
        public void dm_gift_add_to_list(string name, string msg)
        {
            string giftname;
            string count;
            giftname = msg.Split('×')[0].Split('出')[1];
            count = msg.Split('×')[1];

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Text == name && listView1.Items[i].SubItems[2].Text == giftname)
                {
                    listView1.Items[i].SubItems[1].Text = (Convert.ToInt32(listView1.Items[i].SubItems[1].Text) + Convert.ToInt32(count)).ToString();
                    return;
                }
            }

            ListViewItem lv = new ListViewItem();

            lv.Text = name;
            lv.SubItems.Add(count);
            lv.SubItems.Add(giftname);
            listView1.Items.Add(lv);
            lv = null;

        }
        private void statistic(EnumCommentType dm_type, string name)
        {
            return ;
            if (dm_type == EnumCommentType.HEART)
            {
                if (Convert.ToInt32(name) > speaker_dm.max_audience)
                {
                    speaker_dm.max_audience = Convert.ToInt32(name);
                }
                return;
            }
            else
            {
                //统计弹幕 发言人，弹幕数量
                if (speaker_dm.namelist.Contains(name) == false)
                {
                    speaker_dm.namelist.Add(name);
                    speaker_dm.speaker_count++;
                }
                speaker_dm.dm_count++;
            }


        }
        public void dm_get(EnumCommentType dm_type, string name, string msg)
        {
            try
            {

                if (dm_type == EnumCommentType.HEART)
                {
                    label1.Text = "Watching Now: " + msg;
                    statistic(dm_type, msg);

                }
                else if (dm_type == EnumCommentType.GIFT)
                {
                    richTextBox1.AppendText("recevie gift: " + name + msg + "\n");
                    dm_gift_add_to_list(name, msg);
                }
                else if (dm_type == EnumCommentType.WELCOME)
                {
                    //Console.WriteLine("Vip: " + name + "\n");
                    richTextBox1.AppendText("VIP: " + name + "\n");
                }
                else if (dm_type == EnumCommentType.MSG)
                {
                    //Console.WriteLine("MSG: " + name + "说: " + msg + "\n");
                    richTextBox1.AppendText("MSG: " + name + " said : " + msg + "\n");
                    statistic(dm_type, name);
                }
                else if (dm_type == EnumCommentType.DEBUG)
                {
                    richTextBox1.AppendText(msg + "\n");

                }
                Object obj = null;
                FormUtil.formManager.TryGetValue("slider", out obj);
                if (obj != null)
                {
                    BubbleSilder.MainWindow slider = (BubbleSilder.MainWindow)obj;
                    if (dm_type == EnumCommentType.WELCOME && checkBox3.Checked == false)
                    {
                        return;
                    }
                    else if (dm_type == EnumCommentType.GIFT && checkBox2.Checked == false)
                    {
                        return;
                    }
                    else
                    {
                        slider.GetDANMU(dm_type, name, msg);
                    }
                }

            }
            catch
            {
                Console.WriteLine("failed to dm_get()");
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {

        }





        private void tabPage3_Enter(object sender, EventArgs e)
        {
            label2.Text =
                "弹幕姬本次运行统计\n\n弹幕数： "
                + speaker_dm.dm_count.ToString() + "\n\n参与弹幕人数： "
                + speaker_dm.speaker_count + "\n\n最高人数： "
                + speaker_dm.max_audience;
        }


        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.Left = (SystemInformation.WorkingArea.Size.Width) * (double)(hScrollBar1.Value) / ((double)hScrollBar1.Maximum);
            Setting.sliderX = silder.Left;
            label5.Text = ((double)(hScrollBar1.Value) / ((double)hScrollBar1.Maximum)).ToString();
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;

            label5.Text = (silder.Left / (double)SystemInformation.WorkingArea.Size.Width).ToString();
            label6.Text = (silder.Top / (double)SystemInformation.WorkingArea.Size.Height).ToString();
            label10.Text = silder.animation_in.ToString();
            label11.Text = silder.animation_keep.ToString();
            label12.Text = silder.animation_dis.ToString();
            label14.Text = silder.font_size.ToString();
            label15.Text = silder.window_width.ToString();

            hScrollBar1.Value = (int)((silder.Left / (double)SystemInformation.WorkingArea.Size.Width) * hScrollBar1.Maximum);

            hScrollBar2.Value = (int)((silder.Top / (double)SystemInformation.WorkingArea.Size.Height) * hScrollBar2.Maximum);

            hScrollBar3.Value = (int)(((double)silder.animation_in / 6000.0) * hScrollBar3.Maximum);
            hScrollBar4.Value = (int)(((double)silder.animation_keep / 6000.0) * hScrollBar4.Maximum);
            hScrollBar5.Value = (int)(((double)silder.animation_dis / 6000.0) * hScrollBar5.Maximum);
            hScrollBar6.Value = (int)((silder.font_size / 100.0) * hScrollBar6.Maximum);
            hScrollBar7.Value = (int)(((double)silder.window_width / 1000.0) * hScrollBar7.Maximum);

        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;

            silder.Top = (SystemInformation.WorkingArea.Size.Height) * (double)(hScrollBar2.Value) / ((double)hScrollBar2.Maximum);
            Setting.sliderY = silder.Top;
            label6.Text = ((double)(hScrollBar2.Value) / ((double)hScrollBar2.Maximum)).ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            dm_invoke(EnumCommentType.DEBUG, null, "this is a test comment");
        }





        private void hScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.animation_in = (int)(((double)hScrollBar3.Value / (double)hScrollBar3.Maximum) * 6000);
            Setting.shadeInTime = silder.animation_in;
            label10.Text = silder.animation_in.ToString();
        }

        private void hScrollBar4_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.animation_keep = (int)(((double)hScrollBar4.Value / (double)hScrollBar4.Maximum) * 6000);
            Setting.existTime = silder.animation_keep;
            label11.Text = silder.animation_keep.ToString();
        }

        private void hScrollBar5_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.animation_dis = (int)(((double)hScrollBar5.Value / (double)hScrollBar5.Maximum) * 6000);
            Setting.shadeOutTime = silder.animation_dis;
            label12.Text = silder.animation_dis.ToString();
        }

        private void hScrollBar6_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.font_size = ((double)hScrollBar6.Value / (double)hScrollBar6.Maximum * 100);
            Setting.sliderFontSize = silder.font_size;
            label14.Text = silder.font_size.ToString();
        }

        private void hScrollBar7_Scroll(object sender, ScrollEventArgs e)
        {
            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;
            silder.window_width = (int)((double)hScrollBar7.Value / (double)hScrollBar7.Maximum * 1000);
            Setting.sliderWitdh = silder.window_width;
            label15.Text = silder.window_width.ToString();
            silder.stackpanel.Width = silder.window_width;
        }
        private void setting_initialize()
        {

            Setting.sliderX = 0;
            Setting.sliderY = 0;
            Setting.shadeInTime = 500;
            Setting.existTime = 5000;
            Setting.shadeOutTime = 500;
            Setting.sliderFontSize = 15;
            Setting.sliderWitdh = 200;
            Setting.isShownWelcome = true;
            Setting.isShownGift = true;
            UIUtil.writeSettings();
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (File.Exists("setting.json") == false)
            {
                UIUtil.writeSettings();
            }

            UIUtil.readSettings();
            checkBox3.Checked = Setting.isShownWelcome;
            checkBox2.Checked = Setting.isShownGift;
            if (checkBox1.Checked == true)
            {

                var slider = new BubbleSilder.MainWindow();
                FormUtil.formManager.Add("slider", slider);
                slider.Initialzie(Setting.sliderX, Setting.sliderY, Setting.shadeInTime, Setting.existTime, Setting.shadeOutTime, Setting.sliderFontSize, Setting.sliderWitdh);
                slider.Show();
            }
            else
            { 

                Object obj;
                FormUtil.formManager.TryGetValue("slider", out obj);
                if (obj == null) return;
                BubbleSilder.MainWindow slider = (BubbleSilder.MainWindow)obj;
                slider.Close();
                slider = null;
                FormUtil.formManager.Remove("slider");
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int welcome, gift;
            if (checkBox3.Checked == true)
            {
                welcome = 1;
            }
            else
            {
                welcome = 0;
            }
            if (checkBox2.Checked == true)
            {
                gift = 1;
            }
            else
            {
                gift = 0;
            }
            UIUtil.writeSettings();
            //write the setting.ini

            System.Environment.Exit(System.Environment.ExitCode);//kill all thread and exit
        }

        private void button2_Click(object sender, EventArgs e)
        {
            checkBox3.Checked = true;
            checkBox2.Checked = true;
            setting_initialize();
            tabPage4_Enter(sender, e);

            Object obj = null;
            FormUtil.formManager.TryGetValue("slider", out obj);
            if (obj == null)
            {
                return;
            }
            BubbleSilder.MainWindow silder = (BubbleSilder.MainWindow)obj;

            silder.Initialzie(0, 0, 500, 5000, 500, 15, 200);
            tabPage4_Enter(sender,e);

        }


        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
