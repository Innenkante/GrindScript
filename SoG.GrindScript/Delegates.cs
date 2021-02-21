using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace SoG.GrindScript
{
    public delegate void OnDrawPrototype();

    public delegate void OnPlayerTakeDamagePrototype(ref int damage, ref byte type);

    public delegate void OnPlayerKilledPrototype();

    public delegate void PostPlayerLevelUpPrototype(Player xView);

    public delegate void OnEnemyDamagedPrototype(Enemy enemy, ref int damage, ref byte type);

    public delegate void OnNPCDamagedPrototype(NPC npc, ref int damage, ref byte type);

    public delegate void OnNPCInteractionPrototype(NPC npc);

    public delegate void OnArcadiaLoadPrototype();

    public delegate void OnCustomContentLoadPrototype();

    public delegate bool OnChatParseCommandPrototype(string command, string argList, int connection);

    public delegate void OnItemUsePrototype(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend);

    public delegate void EnemyBuilderPrototype(Enemy xEnemy);

    public delegate DynamicEnvironment DynEnvBuilderPrototype(ContentManager xContent);
}
