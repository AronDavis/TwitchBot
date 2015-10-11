using System;
using System.Configuration;
using System.Text.RegularExpressions;
using TwitchBot.CommandManagerPackage;

namespace TwitchBot
{
    class Program
    {
        private static bool _testMode = true;
        private static IrcClient irc;
        static void Main(string[] args)
        {
            bool.TryParse(ConfigurationManager.AppSettings["testmode"], out _testMode);
            string password = ConfigurationManager.AppSettings["oauth"];

            //password from www.twitchapps.com/tmi
            //include the "oauth:" portion
            irc = new IrcClient("irc.twitch.tv", 6667, "mrsheila", password);

            //join channel
            irc.JoinRoom("voxdavis");

            CommandManager.AddCommand("!hype", "Used to generate hype!", (message) => { return "HYPE HYPE HYPE!!!!"; });
            CommandManager.AddCommand("!name", "Used to generate a random name.  Give a username afterwards to assign it to someone.", (message) =>
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
            CommandManager.AddCommand("!source", "Gets a link to the source code!", (message) => { return @"https://github.com/AronDavis/TwitchBot"; });


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

        private static void handleChatMessage(string message)
        {
            string username = message.Substring(1, message.IndexOf("!") - 1);
            message = message.Substring(message.IndexOf(":") + 1);
            message = message.Substring(message.IndexOf(":") + 1);

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
        private static void handleCommand(string username, string message)
        {
            Regex r = new Regex(@"^!\w+");
            irc.sentChatMessage(CommandManager.RunCommand(r.Match(message).Value, message));

        }

    }
}
