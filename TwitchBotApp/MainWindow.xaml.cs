using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Threading;

using TwitchBot;
using NameGeneratorPackage;
using TwitchBot.CommandManagerPackage;

namespace TwitchBotApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _testMode = true;
        private IrcClient _irc;
        private Notification _notification;
        private string _username;

        private BotCommandManager _commandManager;

        public MainWindow()
        {
            _commandManager = new BotCommandManager();

            bool.TryParse(ConfigurationManager.AppSettings["testmode"], out _testMode);
            _username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["oauth"];

            string channel = ConfigurationManager.AppSettings["channel"];

            if (_testMode)
                _irc = new IrcClient();
            else
                _irc = new IrcClient("irc.twitch.tv", 6667, _username, password); //password from www.twitchapps.com/tmi

            //join channel
            _irc.JoinRoom(channel);

            //Add commands
            _commandManager.AddCommand("!hype", "Used to generate hype!", (message) =>
            {
                string returnMessage = "HYPE HYPE HYPE!!!!";
                _irc.SendChatMessage(returnMessage);

                UpdateChatDisplay(new Message(_irc.GenerateChatMessage(_username, returnMessage)), Colors.Blue, Colors.White);
            });
            _commandManager.AddCommand("!name", "Used to generate a random name.  Give a username afterwards to assign it to someone.", (message) =>
            {
                Regex r = new Regex(@"!name @[\w_\-]+");
                NameGenerator ng = new NameGenerator();

                string returnMessage = null;
                if (r.IsMatch(message))
                {
                    string u = message.Substring(7);
                    returnMessage = u + "'s new name is " + ng.GetName();
                }
                else
                {
                    returnMessage = ng.GetName();
                }


                _irc.SendChatMessage(returnMessage);

                UpdateChatDisplay(new Message(_irc.GenerateChatMessage(_username, returnMessage)), Colors.Blue, Colors.White);
            });

            _commandManager.AddCommand("!source", "Gets a link to the source code!", (message) =>
            {
                string returnMessage = @"https://github.com/AronDavis/TwitchBot";

                _irc.SendChatMessage(returnMessage);

                UpdateChatDisplay(new Message(_irc.GenerateChatMessage(_username, returnMessage)), Colors.Blue, Colors.White);
            });

            InitializeComponent();
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            txtChat.Document.Blocks.Clear();

            Thread mainThread = new Thread(new ThreadStart(_runMain));
            mainThread.Start();
        }

        private void _runMain()
        {
            while (true)
            {
                string incoming = _irc.ReadMessage();
                if (incoming == null || incoming.Length == 0)
                    continue;

                Console.WriteLine(incoming);
                Message message = new Message(incoming);

                switch(message.MessageType)
                {
                    case MessageTypeEnum.Unknown:
                        UpdateChatDisplay(message, Colors.Red, Colors.Black);
                        break;
                    case MessageTypeEnum.Welcome:
                    case MessageTypeEnum.YourHost:
                    case MessageTypeEnum.Created:
                    case MessageTypeEnum.MyInfo:
                    case MessageTypeEnum.MessageOfTheDay:
                    case MessageTypeEnum.Capability:
                        UpdateChatDisplay(message, Colors.Black, Colors.White);
                        break;
                    case MessageTypeEnum.Join:
                        UpdateChatDisplay(message, Colors.Black, Colors.White);
                        _irc.SendChatMessage($"Welcome, {message.Username}!");
                        break;
                    case MessageTypeEnum.Part:
                        UpdateChatDisplay(message, Colors.Black, Colors.White);
                        break;
                    case MessageTypeEnum.PrivateMessage:
                        UpdateChatDisplay(message, Colors.Red, Colors.White);
                        _showNotification(message);

                        _handleCommand(message);
                        break;
                    case MessageTypeEnum.Ping:
                        _irc.SendIrcMessage("PONG");
                        break;
                }
            }
        }

        /// <summary>
        /// Assumes that message starts with a command
        /// </summary>
        /// <param name="username"></param>
        /// <param name="message"></param>
        private void _handleCommand(Message message)
        {
            Regex r = new Regex(@"^!\w+");
            _commandManager.RunCommand(r.Match(message.Text).Value, message.Text);
        }

        public void UpdateChatDisplay(Message message, Color foregroundColor, Color backgroundColor)
        {
            if (txtChat.Dispatcher.CheckAccess())
            {
                // Add  paragraph
                txtChat.Document.Blocks.Add(MessageToParagraph(message, foregroundColor, backgroundColor));
                txtChat.ScrollToEnd();
            }
            else
            {
                txtChat.Dispatcher.Invoke(() => UpdateChatDisplay(message, foregroundColor, backgroundColor));
            }
        }

        public Paragraph MessageToParagraph(Message message, Color foregroundColor, Color backgroundColor)
        {
            // Create a paragraph with text
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0);

            if (message.Username != null)
            {

                if(message.MessageType == MessageTypeEnum.PrivateMessage)
                {
                    Run runUsername = new Run($"{message.Username}: ");
                    runUsername.Foreground = new SolidColorBrush(foregroundColor);
                    runUsername.Background = new SolidColorBrush(backgroundColor);

                    Run runMessage = new Run(message.Text);

                    para.Inlines.Add(runUsername);
                    para.Inlines.Add(runMessage);
                }
                else
                {
                    Bold boldMessage = new Bold(new Run(message.Text));
                    boldMessage.Foreground = new SolidColorBrush(foregroundColor);
                    boldMessage.Background = new SolidColorBrush(backgroundColor);
                    para.Inlines.Add(boldMessage);
                }
            }
            else
            {
                Bold boldMessage = new Bold(new Run(message.Text));
                para.Inlines.Add(boldMessage);
            }

            return para;
        }

        private void _sendMessageFromApp() //TODO: as bot
        {
            string message = txtSend.Text;
            _irc.SendChatMessage(message);

            UpdateChatDisplay(new Message(_irc.GenerateChatMessage(_username, message)), Colors.Blue, Colors.White);
            txtSend.Clear();
        }

        private void _btnSendClick(object sender, RoutedEventArgs e)
        {
            _sendMessageFromApp();
        }

        private void _txtSendKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                _sendMessageFromApp();
        }

        private void _showNotification(Message message)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (IsWindowOpen<Notification>())
                {
                    _notification.AppendMessage(MessageToParagraph(message, Colors.Red, Colors.White));
                    _notification.lblTitle.Content = "Message received at " + DateTime.Now.ToShortTimeString();
                }
                else
                    _notification = new Notification("Message received at " + DateTime.Now.ToShortTimeString(), MessageToParagraph(message, Colors.Red, Colors.White));

                _notification.Show();
            }
            else Application.Current.Dispatcher.Invoke(() => _showNotification(message));
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

        private void _btnConsoleSendClick(object sender, RoutedEventArgs e)
        {
            WriteToStreamAsInput();
        }

        private void _txtConsoleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                WriteToStreamAsInput();
        }

        private void WriteToStreamAsInput() //TODO: twitch IRC changes made this a duplicate function...
        {
            string message = txtConsole.Text;
            _irc.ConsoleInput(_irc.GenerateChatMessage(_username, message));
            txtConsole.Clear();
        }
    }
}
