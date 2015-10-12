using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Message
    {
        //TODO: Make sure this works 
        public Message(string incoming)
        {
            if(incoming.IndexOf("!") >= 0) Username = incoming.Substring(1, incoming.IndexOf("!") - 1);

            Text = incoming.Substring(incoming.IndexOf(" :") + 2);
        }

        public string Username;
        public string Text;
    }
}
