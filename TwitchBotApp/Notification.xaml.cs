using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;

namespace TwitchBotApp
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {
        public Notification(string title, Paragraph message)
        {
            InitializeComponent();

            //force to bottom right
            Rect desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top = desktopWorkingArea.Bottom - Height;

            //set title
            lblTitle.Content = title;

            //set message
            txtMessage.Document.Blocks.Clear();
            txtMessage.Document.Blocks.Add(message);
            txtMessage.ScrollToEnd();

            
            ShowActivated = false;
        }

        public void AppendMessage(Paragraph additionalText)
        {
            txtMessage.Document.Blocks.Add(additionalText);
            txtMessage.ScrollToEnd();
            Opacity = 1;
        }

        DispatcherTimer fadeTimer = new DispatcherTimer();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fadeTimer.Interval = TimeSpan.FromMilliseconds(50);
            fadeTimer.Start();
            fadeTimer.Tick += FadeTimer_Tick;
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            Opacity -= .01;

            if (Opacity <= 0) Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            fadeTimer.Stop();
            Opacity = 1;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            fadeTimer.Start();
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            fadeTimer.Start();
        }
    }
}
