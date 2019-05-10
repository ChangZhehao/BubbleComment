using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BubbleSilder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class DANMU
        {
            public struct DM_INFO
            {
                public TextBlock textblock;
                public DateTime time;
            };

        }
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = (-20);
        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);
        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

        public MainWindow()
        {
            InitializeComponent();
            #region 鼠标穿透
            this.Topmost = true;
            this.SourceInitialized += delegate
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
            #endregion
        }
        public enum Dm_Type//invoke后的弹幕返回类型
        {
            MSG = 0,
            GIFT = 1,
            WELCOME = 2,
            HEART = 3,
            SYS = 4,
            DEBUG = 5
        }
        public void GetDANMU(object _type, object _name, object _msg)
        {

            Dm_Type type = (Dm_Type)_type;
            string name = _name as string;
            string msg = _msg as string;
            DANMU.DM_INFO dm_info = new DANMU.DM_INFO();
            dm_info.textblock = new TextBlock();
            dm_info.textblock.Text = "";
            dm_info.textblock.Width = this.Width;
            dm_info.textblock.Background = Brushes.Black;
            dm_info.textblock.FontSize = font_size;//字号
            dm_info.textblock.TextAlignment = TextAlignment.Left;
            dm_info.textblock.TextWrapping = TextWrapping.Wrap;
            dm_info.textblock.MaxWidth = stackpanel.Width;
            if (type == Dm_Type.HEART)
            {
                //Console.WriteLine("在线人数: " + msg + "\n");
                return;

            }
            else if (type == Dm_Type.GIFT)
            {
                dm_info.textblock.Inlines.Add(new Run("recevie gift: ") { Foreground = Brushes.Red });
                dm_info.textblock.Inlines.Add(new Run(name + msg) { Foreground = Brushes.White });

            }
            else if (type == Dm_Type.WELCOME)
            {
                dm_info.textblock.Inlines.Add(new Run("VIP: ") { Foreground = Brushes.Red });
                dm_info.textblock.Inlines.Add(new Run(name) { Foreground = Brushes.Yellow });
                dm_info.textblock.Inlines.Add(new Run(" entrys channel.") { Foreground = Brushes.White });
            }
            else if (type == Dm_Type.MSG)
            {
                dm_info.textblock.Inlines.Add(new Run(name) { Foreground = Brushes.Yellow });
                dm_info.textblock.Inlines.Add(new Run(" said: " + msg) { Foreground = Brushes.White });
            }
            else if (type == Dm_Type.DEBUG)
            {
                dm_info.textblock.Inlines.Add(new Run(msg) { Foreground = Brushes.Yellow });
            }

            dm_info.time = DateTime.Now;
            /*只有在updatelaayout()后才能获取真实的actualheight值
             *所以先更新后获取值
             * 在把textblock归零
             * 在更新重做
             * 
             */
            double h;
            stackpanel.Children.Insert(0, dm_info.textblock);
            stackpanel.UpdateLayout();
            h = dm_info.textblock.ActualHeight;
            dm_info.textblock.Height = 0;
            stackpanel.UpdateLayout();
            move(dm_info.textblock, h);

        }
        #region 弹幕淡入淡出动画效果
        public int animation_in = 500;
        public int animation_keep = 5000;
        public int animation_dis = 500;
        public double font_size = 15;
        public int window_width = 200;
        void move(TextBlock textblock, double h)
        {
            double length = textblock.ActualHeight;

            DoubleAnimation a = new DoubleAnimation();
            a.From = 0;
            a.To = h;
            a.Duration = new Duration(TimeSpan.FromMilliseconds(animation_in));
            textblock.BeginAnimation(Border.HeightProperty, a);


            appear(textblock);//开始动画
        }

        private void appear(TextBlock textblock)
        {
            //动画淡入
            /*
            DoubleAnimation a = new DoubleAnimation();
            a.From = this.Top;
            a.To = this.Top-(dockPanel.ActualHeight+this.Top-(SystemParameters.PrimaryScreenHeight));
            a.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            this.BeginAnimation(Window.TopProperty, a);
            */

            DoubleAnimation daV = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(animation_in)));
            Storyboard.SetTarget(daV, textblock);
            daV.Completed += new EventHandler(sustain);
            textblock.BeginAnimation(UIElement.OpacityProperty, daV);


        }

        private void sustain(object sender, EventArgs e)
        {
            AnimationTimeline timeline = (sender as AnimationClock).Timeline;
            TextBlock uiElement = Storyboard.GetTarget(timeline) as TextBlock;
            //动画维持
            DoubleAnimation daV = new DoubleAnimation(1, 1, new Duration(TimeSpan.FromMilliseconds(animation_keep)));

            daV.Completed += new EventHandler(disappear);
            Storyboard.SetTarget(daV, uiElement);
            uiElement.BeginAnimation(UIElement.OpacityProperty, daV);

        }
        private void disappear(object sender, EventArgs e)
        {
            AnimationTimeline timeline = (sender as AnimationClock).Timeline;
            TextBlock uiElement = Storyboard.GetTarget(timeline) as TextBlock;
            //动画淡出
            DoubleAnimation daV = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(animation_dis)));

            daV.Completed += new EventHandler(remove);
            Storyboard.SetTarget(daV, uiElement);
            uiElement.BeginAnimation(UIElement.OpacityProperty, daV);
        }
        private void remove(object sender, EventArgs e)
        {
            AnimationTimeline timeline = (sender as AnimationClock).Timeline;
            TextBlock uiElement = Storyboard.GetTarget(timeline) as TextBlock;

            stackpanel.Children.Remove(uiElement);
            stackpanel.UpdateLayout();

        }
        #endregion

        private void mainwindow_Initialized(object sender, EventArgs e)
        {

        }
        public void Initialzie(double X, double Y, int IN, int KEEP, int DIS, double FONTSIZE, int WIDTH)
        {
            this.Top = Y;
            this.Left = X;
            animation_in = IN;
            animation_keep = KEEP;
            animation_dis = DIS;
            font_size = FONTSIZE;
            window_width = WIDTH;

            if (stackpanel != null)
            {
                stackpanel.Width = WIDTH;
            }
        }



        private void stackpanel_Initialized(object sender, EventArgs e)
        {


            this.Top = 0;
            this.Left = 0;
            stackpanel.Width = window_width;
            stackpanel.Height = SystemParameters.PrimaryScreenHeight;
            stackpanel.Background = Brushes.Transparent;
            stackpanel.Opacity = 0.8;
            stackpanel.UpdateLayout();
        }
    }
}
