using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Configuration;

namespace TwitchBot
{
    class IrcClient
    {
        private bool _testMode = true;
        private string username;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;


        public IrcClient(string ip, int port, string username, string password)
        {
            bool.TryParse(ConfigurationManager.AppSettings["testmode"], out _testMode);

            this.username = username;
            if(!_testMode)
            {
                tcpClient = new TcpClient(ip, port);
                inputStream = new StreamReader(tcpClient.GetStream());
                outputStream = new StreamWriter(tcpClient.GetStream());
            }
            else
            {
                inputStream = new StreamReader(Console.OpenStandardInput());
                outputStream = new StreamWriter(Console.OpenStandardOutput());
            }

            //TODO: lookup why
            //this is just what the server is expecting
            //creates the connection
            //connects to server, but not any specific channel yet
            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 *:" + username);
            outputStream.Flush();
        }

        public void JoinRoom(string channel)
        {
            //TODO: oh god this is ugly, fix!!!
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void sentChatMessage(string message)
        {
            sendIrcMessage(":" + username + "!" + username + "@" + username 
                + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public string readMessage()
        {
            string message = inputStream.ReadLine();
            return message;
        }
    }
}
