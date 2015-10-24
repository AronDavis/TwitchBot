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
using System.IO;
using System.Diagnostics;

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

            if (_testMode) irc = new IrcClient();
            else irc = new IrcClient("irc.twitch.tv", 6667, "mrsheila", password); //password from www.twitchapps.com/tmi

            //join channel
            irc.JoinRoom("voxdavis");

            //Add commands
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


            while (true)
            {
                string incoming = irc.ReadMessage();
                if (incoming == null || incoming.Length == 0) continue;

                Message message = new Message(incoming);

                if (message.Username != null && message.Text.Length > 0) handleChatMessage(message);
                else if (message.Text.StartsWith("PING")) irc.SendIrcMessage("PONG");

                Console.WriteLine(incoming);
            }
        }

        private void handleChatMessage(Message message)
        {
            UpdateChatDisplay(message, Colors.Red);
            showNotification(message);

            if (message.isJoin) irc.SendChatMessage("Welcome, " + message.Username + "!");
            else if (message.isLeave) irc.SendChatMessage("Bye, " + message.Username + "!");
            else if (message.Text[0] == '!') handleCommand(message);
        }

        /// <summary>
        /// Assumes that message starts with a command
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        private void handleCommand(Message message)
        {
            Regex r = new Regex(@"^!\w+");
            string returnMessage = ComManager.RunCommand(r.Match(message.Text).Value, message.Text);

            if (!String.IsNullOrEmpty(returnMessage))
            {
                irc.SendChatMessage(returnMessage);

                UpdateChatDisplay(new Message(irc.GenerateChatMessage("MrSheila", returnMessage)), Colors.Blue);
            }
        }

        public void UpdateChatDisplay(Message message, Color color)
        {
            if (txtChat.Dispatcher.CheckAccess())
            {
                // Add  paragraph
                txtChat.Document.Blocks.Add(MessageToParagraph(message, color));
                txtChat.ScrollToEnd();
            }
            else
            {
                txtChat.Dispatcher.Invoke(() => UpdateChatDisplay(message, color));
            }
        }

        public Paragraph MessageToParagraph(Message message, Color color)
        {
            // Create a paragraph with text
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0);

            if (message.Username != null)
            {

                if(message.isJoin || message.isLeave)
                {
                    Bold boldMessage = new Bold(new Run(message.Text));
                    para.Inlines.Add(boldMessage);
                }
                else
                {
                    Run runUsername = new Run(message.Username + ": ");
                    runUsername.Foreground = new SolidColorBrush(color);

                    Run runMessage = new Run(message.Text);

                    para.Inlines.Add(runUsername);
                    para.Inlines.Add(runMessage);
                }
            }
            else
            {
                Bold boldMessage = new Bold(new Run(message.Text));
                para.Inlines.Add(boldMessage);
            }

            return para;
        }

        private void SendMessageFromApp() //TODO: as bot
        {
            string message = txtSend.Text;
            irc.SendChatMessage(message);

            UpdateChatDisplay(new Message(irc.GenerateChatMessage("MrSheila", message)), Colors.Blue);
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

        private void showNotification(Message message)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (IsWindowOpen<Notification>())
                {
                    notification.AppendMessage(MessageToParagraph(message, Colors.Red));
                    notification.lblTitle.Content = "Message received at " + DateTime.Now.ToShortTimeString();
                }
                else notification = new Notification("Message received at " + DateTime.Now.ToShortTimeString(), MessageToParagraph(message, Colors.Red));

                notification.Show();
            }
            else Application.Current.Dispatcher.Invoke(() => showNotification(message));
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

        private void btnConsoleSend_Click(object sender, RoutedEventArgs e)
        {
            WriteToStreamAsInput();
        }

        private void txtConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) WriteToStreamAsInput();
        }

        private void WriteToStreamAsInput()
        {
            string message = txtConsole.Text;
            irc.ConsoleInput(irc.GenerateChatMessage("VoxDavis", message));
            txtConsole.Clear();
        }
    }
}
