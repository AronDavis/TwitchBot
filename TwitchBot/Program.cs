using System;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;

using TwitchBot.CommandManagerPackage;

namespace TwitchBot
{
    class Program
    {
        private static IrcClient irc;
        static void Main(string[] args)
        {
            string password = ConfigurationManager.AppSettings["oauth"];

            //password from www.twitchapps.com/tmi
            //include the "oauth:" portion
            irc = new IrcClient("irc.twitch.tv", 6667, "mrsheila", password);

            //join channel
            irc.JoinRoom("voxdavis");

            CommandManager.AddCommand("!test", "This is a test.", (message) => { irc.sentChatMessage("test!"); });

            //irc.sendIrcMessage("PING");

            //TODO: gross, change!
            while (true)
            {
                string message = irc.readMessage();
                if (message == null || message.Length == 0) continue;

                Console.WriteLine(message);
                                
                if (message.IndexOf("!") >= 0)
                {
                    handleChatMessage(message);
                }
                else if(message.StartsWith("PING"))
                {
                    irc.sendIrcMessage("PONG");
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

        private static void handleCommand(string username, string message)
        {
            Regex r = new Regex(@"!\w+");
            CommandManager.RunCommand(r.Match(message).Value, message);


            if (message == "!hype")
            {
                irc.sentChatMessage("@" + username + " HYPE HYPE HYPE!!!! at " + DateTime.Now.ToLongTimeString());
            }
            else if (message == "!name")
            {
                NameGenerator ng = new NameGenerator();
                irc.sentChatMessage(ng.GetName());
            }
            else if(message.StartsWith("!name"))
            {
                r = new Regex(@"!name @[\w_\-]+");

                if (r.IsMatch(message))
                {
                    NameGenerator ng = new NameGenerator();
                    string u = message.Substring(7);
                    irc.sentChatMessage(u + "'s new name is " + ng.GetName());
                }
            }
        }

    }
}
