﻿using System;
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
    public class IrcClient
    {
        private string username;
        private string channel;

        private TcpClient tcpClient;
        private StreamReader inputStream;
        private StreamWriter outputStream;

        public IrcClient(string ip, int port, string username, string password)
        {
            this.username = username;
            tcpClient = new TcpClient(ip, port);
            inputStream = new StreamReader(tcpClient.GetStream());
            outputStream = new StreamWriter(tcpClient.GetStream());

            //this is just what the server is expecting
            //creates the connection
            //connects to server, but not any specific channel yet
            outputStream.WriteLine("PASS " + password);
            outputStream.WriteLine("NICK " + username);
            outputStream.WriteLine("USER " + username + " 8 *:" + username);
            outputStream.Flush();
        }

        /// <summary>
        /// Use this for testing.
        /// </summary>
        public IrcClient()
        {
            inputStream = new StreamReader(new MemoryStream());
            outputStream = new StreamWriter(new MemoryStream());
        }

        public void JoinRoom(string channel)
        {
            //assume we're only working with one channel for now
            this.channel = channel;
            outputStream.WriteLine("JOIN #" + channel);
            outputStream.Flush();
        }

        public void sendIrcMessage(string message)
        {
            outputStream.WriteLine(message);
            outputStream.Flush();
        }

        public void sendChatMessage(string message)
        {
            sendIrcMessage(":" + username + "!" + username + "@" + username 
                + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message);
        }

        public string GenerateChatMessage(string username, string message)
        {
            return ":" + username + "!" + username + "@" + username
                + ".tmi.twitch.tv PRIVMSG #" + channel + " :" + message;
        }

        public void Input(string message)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(message);
            inputStream.BaseStream.Write(bytes, 0, bytes.Length);
            inputStream.BaseStream.Flush();
        }

        public string readMessage()
        {
            string message = inputStream.ReadLine(); //NOTE: gets error when not connected to internet?
            return message;
        }
    }
}
