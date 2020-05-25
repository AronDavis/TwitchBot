using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.CommandManagerPackage
{
    public sealed class Command
    {
        public const string IDENTIFIER = "!";

        public string HelpText { get; private set; }
        public string Name { get; private set; }

        public delegate string MessageAction(string message);
        public MessageAction ProcessCommand;

        internal Command(string name, string helpText, MessageAction processCommand)
        {
            Name = name;
            HelpText = helpText;

            ProcessCommand = processCommand;
        }

    }
}
