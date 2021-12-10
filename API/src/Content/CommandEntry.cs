using System;
using System.Collections.Generic;
using System.Linq;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Defines custom commands that can be entered from the in-game chat. <para/>
    /// All modded commands are called by using the "/{<see cref="Mod.NameID"/>}:{Command} [args] format. <para/>
    /// For instance, you can use "/GrindScript:Help" to invoke the mod tool's help command.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class CommandEntry : Entry<GrindScriptID.CommandID>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override GrindScriptID.CommandID GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal Dictionary<string, CommandParser> commands = new Dictionary<string, CommandParser>();

        #endregion

        #region Public Interface

        /// <summary>
        /// Sets an action to be done when the given mod command is entered. <para/>
        /// The command "Help", if not defined explicitly, will print a list of commands defined by the mod.
        /// </summary>
        /// <param name="command"> The target command </param>
        /// <param name="parser"> The action to trigger. A value of null removes the command. </param>
        public void SetCommand(string command, CommandParser parser)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (parser == null)
            {
                commands.Remove(command);
            }
            else
            {
                commands.Add(command, parser);
            }
        }

        /// <summary>
        /// Gets all of the commands defined by this mod.
        /// </summary>
        /// <remarks> 
        /// This method can also be used outside of <see cref="Mod.Load"/>.
        /// </remarks>
        /// <returns> The command list. </returns>
        public List<string> GetCommandList()
        {
            return commands.Select(x => x.Key).ToList();
        }

        #endregion

        internal CommandEntry() { }

        internal override void Initialize()
        {
            // Nothing to do
        }

        internal override void Cleanup()
        {
            // Nothing to do
        }
    }
}
