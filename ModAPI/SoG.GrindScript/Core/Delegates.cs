using System.IO;

namespace SoG.Modding
{
    /// <summary>
    /// Used to process a chat command.
    /// </summary>
    /// <param name="argList"> The arguments passed to the command. </param>
    /// <param name="connection"> The connection identifier. </param>
    public delegate void CommandParser(string argList, int connection);

    /// <summary>
    /// Used to create a basic template for a level.
    /// </summary>
    /// <param name="blueprint">The level blueprint that should be initialized.</param>
    public delegate void LevelBuilder(LevelBlueprint blueprint);

    /// <summary>
    /// Used to create advanced objects for a level and run state-dependent code,
    /// after it has been instantiated from its level blueprint.
    /// </summary>
    /// <param name="staticOnly">Whenever only static (local) objects should be instantiated.</param>
    public delegate void LevelLoader(bool staticOnly);

    /// <summary>
    /// Used to instantiate and edit an enemy.
    /// </summary>
    /// <param name="enemy">The enemy to initialize.</param>
    public delegate void EnemyBuilder(Enemy enemy);

    /// <summary>
    /// Used to instantiate a spell.
    /// The value returned must be non-null.
    /// </summary>
    /// <param name="overrideRegion">The region to use for content load. If NotLoaded, use the current region's content manager.</param>
    /// <param name="powerLevel">The spell's requested level.</param>
    public delegate ISpellInstance SpellBuilder(int powerLevel, Level.WorldRegion overrideRegion);

    /// <summary>
    /// Used to parse messages coming from clients.
    /// </summary>
    /// <param name="msg">The data that the message contains.</param>
    /// <param name="connection">The connection ID of the client that sent the message.</param>
    public delegate void ClientMessageParser(BinaryReader msg, long connection);

    /// <summary>
    /// Used to parse messages coming from the server.
    /// </summary>
    /// <param name="msg">The data that the message contains.</param>
    public delegate void ServerMessageParser(BinaryReader msg);


}
