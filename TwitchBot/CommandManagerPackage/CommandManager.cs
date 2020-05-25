using System.Collections.Generic;
using System.Linq;

namespace TwitchBot.CommandManagerPackage
{
    public static class CommandManager
    {
        static Dictionary<string, Command> Commands = new Dictionary<string, Command>() //TODO: need to allow more than just text based commands so I can send multiple messages, etc.
        {
            {
                "!help", //TODO: move/fix so we can include usernames
                new Command(
                        name: "!help", 
                        helpText: "Used to get help with commands.  !help <command>",
                        processCommand: (message) => 
                        {
                            if(message.Trim() == "!help")
                            {
                                var commandList = string.Join(" ", Commands.Values.Select(c => c.Name));
                                return $"Use \"!help <command>\" for more info.  Command list: {commandList}";
                            }

                            string[] splitMessage = message.Split(' ');
                            if(splitMessage.Length <= 1)
                                return "No command found to help with.";

                            string commandName = message.Split(' ')[1].Trim();

                            if(Commands.TryGetValue(commandName, out Command command))
                                return $"{command.Name} - {command.HelpText}";
                            
                            return $"Could not find command {commandName}";
                        }
                )
            }
        };

        public static string RunCommand(string key, string message)
        {
            //if we can't find the key, abort
            if (!Commands.ContainsKey(key))
                return null;

            return Commands[key].ProcessCommand(message);
        }

        public static bool AddCommand(string name, string helpText, Command.MessageAction processCommand)
        {
            //create the command based on the parameters
            Command command = new Command(name, helpText, processCommand);

            //get the key for looking the command
            string commandKey = GetKey(command);

            if (Commands.ContainsKey(commandKey))
                return false;

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
