namespace TwitchBot.CommandManagerPackage
{
    public sealed class Command
    {
        public const string IDENTIFIER = "!";

        public string HelpText { get; private set; }
        public string Name { get; private set; }

        public delegate void MessageAction(string message);
        public MessageAction ProcessCommand;

        internal Command(string name, string helpText, MessageAction processCommand)
        {
            Name = name;
            HelpText = helpText;

            ProcessCommand = processCommand;
        }

    }
}
