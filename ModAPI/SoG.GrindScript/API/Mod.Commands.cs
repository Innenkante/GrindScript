using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        /// <summary>
        /// Helper method for setting up multiple commands.
        /// </summary>
        public void CreateCommands(IDictionary<string, CommandParser> parsers)
        {
            foreach (var kvp in parsers)
                CreateCommand(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Adds a new command that executes the given parser when called.
        /// The command can be executed by typing in chat "/(ModName):(command) (argList)". <para/>
        /// The command must not have whitespace in it.
        /// </summary>
        public void CreateCommand(string command, CommandParser parser)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            if (command.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Provided command contains whitespace.");
            }

            Mod mod = Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateCommand));
                return;
            }

            ModCommands[command] = parser;
        }
    }
}
