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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;

using TwitchBot;
using TwitchBot.CommandManagerPackage;
using ComManager = TwitchBot.CommandManagerPackage.CommandManager;
using NameGeneratorPackage;

namespace TwitchBotApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static bool _testMode = true;
        private static IrcClient irc;
        private static Notification notification;

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            txtChat.Document.Blocks.Clear();
            Thread mainThread = new Thread(new ThreadStart(RunMain));
            mainThread.Start();
        }

        void RunMain()
        {
            bool.TryParse(ConfigurationManager.AppSettings["testmode"], out _testMode);
            string password = ConfigurationManager.AppSettings["oauth"];

            //password from www.twitchapps.com/tmi
            //include the "oauth:" portion
            irc = new IrcClient("irc.twitch.tv", 6667, "mrsheila", password);

            //join channel
            irc.JoinRoom("voxdavis");

            ComManager.AddCommand("!hype", "Used to generate hype!", (message) => { return "HYPE HYPE HYPE!!!!"; });
            ComManager.AddCommand("!name", "Used to generate a random name.  Give a username afterwards to assign it to someone.", (message) =>
            {
                Regex r = new Regex(@"!name @[\w_\-]+");
                NameGenerator ng = new NameGenerator();

                if (r.IsMatch(message))
                {
                    string u = message.Substring(7);
                    return u + "'s new name is " + ng.GetName();
                }
                else
                {
                    return ng.GetName();
                }

            });
            ComManager.AddCommand("!source", "Gets a link to the source code!", (message) => { return @"https://github.com/AronDavis/TwitchBot"; });


            if (_testMode)
            {
                while (true)
                {
                    string message = irc.readMessage();
                    if (message == null || message.Length == 0) continue;

                    if (message[0] == '!')
                    {
                        handleCommand("TestUser", message);
                    }
                }
            }
            else
            {
                while (true)
                {
                    string message = irc.readMessage();
                    if (message == null || message.Length == 0) continue;

                    Console.WriteLine(message);

                    if (message.IndexOf("!") >= 0) handleChatMessage(message);
                    else if (message.StartsWith("PING")) irc.sendIrcMessage("PONG");
                }
            }
        }

        private void handleChatMessage(string message)
        {
            string username = message.Substring(1, message.IndexOf("!") - 1);
            message = message.Substring(message.IndexOf(":") + 1);
            message = message.Substring(message.IndexOf(":") + 1);

            UpdateChatDisplay(username, message, Colors.Red);
            showNotification(message); //TODO: fix this method
            if (message[0] == '!')
            {
                handleCommand(username, message);
            }
        }

        /// <summary>
        /// Assumes that message starts with a command
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        private void handleCommand(string username, string message)
        {
            Regex r = new Regex(@"^!\w+");
            string returnMessage = ComManager.RunCommand(r.Match(message).Value, message);
            irc.sentChatMessage(returnMessage);
            UpdateChatDisplay("MrSheila", returnMessage, Colors.Blue);
        }

        delegate void updateChatDelegate(string user, string message, Color color);
        public void UpdateChatDisplay(string user, string message, Color color)
        {
            if (txtChat.Dispatcher.CheckAccess())
            {
                // Create a paragraph with text
                Paragraph para = new Paragraph();
                para.Margin = new Thickness(0);

                Run runUsername = new Run(user + ": ");
                runUsername.Foreground = new SolidColorBrush(color);

                Run runMessage = new Run(message);

                para.Inlines.Add(runUsername);
                para.Inlines.Add(runMessage);

                // Add the paragraph
                txtChat.Document.Blocks.Add(para);
                txtChat.ScrollToEnd();
            }
            else
            {
                updateChatDelegate updater = new updateChatDelegate(UpdateChatDisplay);
                txtChat.Dispatcher.Invoke(updater, user, message, color);
            }
        }

        private void SendMessageFromApp()
        {
            string message = txtSend.Text;
            irc.sentChatMessage(message);
            UpdateChatDisplay("MrSheila", message, Colors.Blue);
            txtSend.Clear();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessageFromApp();
        }

        private void txtSend_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) SendMessageFromApp();
        }

        private void showNotification(string title)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (IsWindowOpen<Notification>())
                {
                    notification.lblTitle.Content = title;
                    notification.Opacity = 1;
                }
                else notification = new Notification(title);

                notification.Show();
            }
            else Application.Current.Dispatcher.Invoke(() => showNotification(title));
        }

        public static bool IsWindowOpen<T>(string name = null) where T : Window
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                return string.IsNullOrEmpty(name)
                    ? Application.Current.Windows.OfType<T>().Any()
                    : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
            }
            else
            {
                bool result = false;
                Application.Current.Dispatcher.Invoke(() => result = IsWindowOpen<T>(name));
                return result;
            }

        }
    }
}
