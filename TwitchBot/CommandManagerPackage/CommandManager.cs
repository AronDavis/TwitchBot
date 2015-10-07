using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.CommandManagerPackage
{
    public static class CommandManager
    {
        static Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        public static void RunCommand(string key, string message)
        {
            //if we can't find the key, abort
            if (!Commands.ContainsKey(key)) return;

            Commands[key].ProcessCommand(message);
        }

        public static bool AddCommand(string name, string helpText, Command.MessageAction processCommand)
        {
            //create the command based on the parameters
            Command command = new Command(name, helpText, processCommand);

            //get the key for looking the command
            string commandKey = GetKey(command);

            if (Commands.ContainsKey(commandKey)) return false;

            Commands[commandKey] = command;

            //successfully addded command
            return true;
        }

        /// <summary>
        /// Get the key for looking up a command.
        /// </summary>
        /// <param name="command">The command to get the key from.</param>
        /// <returns></returns>
        public static string GetKey(Command command)
        {
            return command.Name;
        }

        /// <summary>
        /// Get all of the keys for the available commands.
        /// </summary>
        /// <returns></returns>
        public static string[] GetKeys()
        {
            return Commands.Keys.ToArray();
        }
    }
}
