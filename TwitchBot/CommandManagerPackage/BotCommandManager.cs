using System.Collections.Generic;
using System.Linq;

namespace TwitchBot.CommandManagerPackage
{
    public class BotCommandManager
    {
        private Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        public void RunCommand(string key, string message)
        {
            //if we can't find the key, abort
            if (!_commands.ContainsKey(key))
                return;

            _commands[key].ProcessCommand(message);
        }

        public bool AddCommand(string name, string helpText, Command.MessageAction processCommand)
        {
            //create the command based on the parameters
            Command command = new Command(name, helpText, processCommand);

            //get the key for looking the command
            string commandKey = GetKey(command);

            if (_commands.ContainsKey(commandKey))
                return false;

            _commands[commandKey] = command;

            //successfully addded command
            return true;
        }

        /// <summary>
        /// Get the key for looking up a command.
        /// </summary>
        /// <param name="command">The command to get the key from.</param>
        /// <returns></returns>
        public string GetKey(Command command)
        {
            return command.Name;
        }

        /// <summary>
        /// Get all of the keys for the available commands.
        /// </summary>
        /// <returns></returns>
        public string[] GetKeys()
        {
            return _commands.Keys.ToArray();
        }
    }
}
