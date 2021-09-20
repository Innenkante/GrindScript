using Microsoft.Xna.Framework.Content;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Used to process a chat command.
    /// </summary>
    public delegate void CommandParser(string argList, int connection);

    /// <summary>
    /// Used to create a basic template for a level.
    /// </summary>
    public delegate void LevelBuilder(LevelBlueprint blueprint);

    /// <summary>
    /// Used to create advanced objects for a level and run state-dependent code,
    /// after it has been instantiated from its level blueprint.
    /// </summary>
    public delegate void LevelLoader(bool staticOnly);

    /// <summary>
    /// Used to instantiate and edit an enemy.
    /// </summary>
    public delegate void EnemyBuilder(Enemy enemy);

    /// <summary>
    /// Used to instantiate a spell.
    /// The value returned must be non-null.
    /// </summary>
    public delegate ISpellInstance SpellBuilder(int powerLevel, Level.WorldRegion overrideRegion);
}
