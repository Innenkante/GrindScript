﻿using SoG.Modding.API;
using SoG.Modding.Core;
using System;
using System.IO;

namespace SoG.Modding.ModUtils
{
    /// <summary>
    /// Provides helper methods for networking.
    /// </summary>
    public static class NetUtils
    {
        /// <summary> The message type of modded game packets. </summary>
        public const byte ModPacketType = 222;

        /// <summary> Returns true if the game is currently not in a multiplayer session, false otherwise. </summary>
        public static bool IsLocal
        {
            get => Globals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.LocalOnly;
        }

        /// <summary> Returns true if the game is currently a client, false otherwise. </summary>
        public static bool IsClient
        {
            get => Globals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Client;
        }

        /// <summary> Returns true if the game is currently a server, false otherwise. </summary>
        public static bool IsServer
        {
            get => Globals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Server;
        }

        public static bool IsLocalOrServer => IsServer || IsLocal;

        /// <summary>
        /// Writes the header for mod packets.
        /// </summary>
        internal static OutMessage WriteModData(Mod mod, ushort packetID, Action<BinaryWriter> data)
        {
            MemoryStream stream = new MemoryStream();

            if (stream == null)
                throw new ArgumentException(nameof(data) + " must not return null");

            OutMessage msg = new OutMessage(stream);
            msg.Write(ModPacketType);
            msg.Write(mod.ModIndex);
            msg.Write(packetID);
            data.Invoke(new BinaryWriter(stream));

            return msg;
        }

        /// <summary>
        /// Reads the mod and the packetID from the given mod packet, assuming that ModPacketType has already been read.
        /// </summary>
        internal static void ReadModData(InMessage msg, out Mod mod, out ushort packetID)
        {
            int modIndex = msg.ReadInt32();

            packetID = msg.ReadUInt16();
            mod = Globals.API.Loader.Mods[modIndex];
        }
    }
}
