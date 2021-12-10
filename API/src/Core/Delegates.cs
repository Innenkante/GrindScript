using System.IO;
using System;

namespace SoG.Modding
{
    /// <summary> 
    /// Delegate that parses a chat command. 
    /// </summary>
    /// <param name="args"> The argument list. </param>
    /// <param name="connection"> The connection identifier of the player. </param>
    public delegate void CommandParser(string[] args, int connection);

    /// <summary>
    /// Delegate that initializes the blueprint of a level.
    /// </summary>
    /// <param name="blueprint"> The level blueprint to initialize. </param>
    public delegate void LevelBuilder(LevelBlueprint blueprint);

    /// <summary>
    /// Delegate that loads a level, after objects in the level blueprint have been created.
    /// </summary>
    /// <param name="staticOnly"> Whenever to limit instantiation to static (non-networked) objects only. </param>
    public delegate void LevelLoader(bool staticOnly);

    /// <summary>
    /// Delegate that initializes an enemy.
    /// </summary>
    /// <param name="enemy"> The enemy to initialize. </param>
    public delegate void EnemyBuilder(Enemy enemy);

    /// <summary>
    /// Delegate that initializes a spell.
    /// </summary>
    /// <param name="powerLevel"> The spell's level. </param>
    /// <param name="overrideRegion"> The region to use for content load. A value of <see cref="Level.WorldRegion.NotLoaded"/> means "use the current region". </param>
    public delegate ISpellInstance SpellBuilder(int powerLevel, Level.WorldRegion overrideRegion);

    /// <summary>
    /// Delegate that parses an incoming client message.
    /// </summary>
    /// <param name="msg"> The message data. </param>
    /// <param name="connection"> The connection ID of the client. </param>
    ///
    public delegate void ServerSideParser(BinaryReader msg, long connection);

    /// <summary>
    /// Delegate that parses an incoming server message.
    /// </summary>
    /// <param name="msg"> The message data. </param>
    public delegate void ClientSideParser(BinaryReader msg);
}
