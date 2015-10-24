using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Message
    {
        public bool isJoin = false;
        public bool isLeave = false;
        //TODO: Make sure this works 
        public Message(string incoming)
        {
            if(incoming.IndexOf("!") >= 0) Username = incoming.Substring(1, incoming.IndexOf("!") - 1);

            if (incoming.StartsWith("PING")) Text = "PING";
            else
            {
                string[] incomingSplit = incoming.Split(' ');

                if (incomingSplit.Length > 1)
                {
                    switch(incomingSplit[1])
                    {
                        case "JOIN":
                            isJoin = true;
                            Text = Username + " joined!";
                            break;
                        case "PART":
                            isLeave = true;
                            Text = Username + " Left!";
                            break;
                        case "PRIVMSG":
                            Text = incoming.Substring(incoming.IndexOf(" :") + 2);
                            break;
                        default:
                            Text = incoming;
                            break;
                    }
                }
                else Text = incoming.Substring(incoming.IndexOf(" :") + 2);
            }
        }

        public string Username;
        public string Text;
    }
}
