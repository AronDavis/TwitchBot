using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

namespace TwitchBot
{
    public class IrcClient
    {
        private string _username;
        private string _channel;

        private TcpClient _tcpClient;
        private StreamReader _inputStream;
        private StreamWriter _outputStream;
        private Queue<string> _consoleInput = new Queue<string>(); //TODO: make this concurrent queue?

        public IrcClient(string ip, int port, string username, string password)
        {
            this._username = username;
            _tcpClient = new TcpClient(ip, port);
            _inputStream = new StreamReader(_tcpClient.GetStream());
            _outputStream = new StreamWriter(_tcpClient.GetStream());

            //this is just what the server is expecting
            //creates the connection
            //connects to server, but not any specific channel yet
            _outputStream.WriteLine($"PASS {password}");
            _outputStream.WriteLine($"NICK {username}");
            _outputStream.WriteLine($"USER {username} 0 *:{username}");
            _outputStream.Flush();

            _outputStream.WriteLine("CAP REQ :twitch.tv/membership");
            _outputStream.Flush();

        }

        /// <summary>
        /// Use this for testing.
        /// </summary>
        public IrcClient()
        {
            _inputStream = new StreamReader(new MemoryStream());
            _outputStream = new StreamWriter(new MemoryStream());
        }

        public void JoinRoom(string channel)
        {
            //assume we're only working with one channel for now
            this._channel = channel;
            _outputStream.WriteLine($"JOIN #{channel}");
            _outputStream.Flush();
        }

        public void LeaveRoom(string channel)
        {
            //assume we're only working with one channel for now
            this._channel = null;
            _outputStream.WriteLine($"PART #{channel}");
            _outputStream.Flush();
        }

        public void RequestNames()
        {
            _outputStream.WriteLine($"NAMES #{_channel}");
            _outputStream.Flush();
        }

        public void SendIrcMessage(string message)
        {
            _outputStream.WriteLine(message);
            _outputStream.Flush();
        }

        public void SendChatMessage(string message)
        {
            SendIrcMessage(GenerateChatMessage(_username, message));
        }

        public string GenerateChatMessage(string username, string message)
        {
            return $":{username}!{username}@{username}.tmi.twitch.tv PRIVMSG #{_channel} :{message}";
        }

        public void ConsoleInput(string message)
        {
            _consoleInput.Enqueue(message);
        }

        public string ReadMessage()
        {
            if (_consoleInput.Count > 0)
                return _consoleInput.Dequeue();

            //TODO: this blocks...so if I send console input, it won't get read until a message comes through
            return _inputStream.ReadLine(); //NOTE: gets error when not connected to internet?
        }
    }
}
