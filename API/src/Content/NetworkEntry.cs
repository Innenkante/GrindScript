using System.Collections.Generic;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Contains data for sending custom packets between clients and servers.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class NetworkEntry : Entry<GrindScriptID.NetworkID>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override GrindScriptID.NetworkID GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal Dictionary<ushort, ServerSideParser> serverSide { get; } = new Dictionary<ushort, ServerSideParser>();

        internal Dictionary<ushort, ClientSideParser> clientSide { get; } = new Dictionary<ushort, ClientSideParser>();

        #endregion

        #region Public Interface

        /// <summary>
        /// Sets a parser for the given packet ID. <para/>
        /// The parser will be called whenever servers receive a packet of the given type.
        /// </summary>
        /// <param name="packet"> The ID of the packet. You can choose any value you want (but keep it the same for the client side parser). </param>
        /// <param name="parser"> The parser to call when a packet is received. </param>
        public void SetServerSideParser(ushort packet, ServerSideParser parser)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            if (parser == null)
            {
                serverSide.Remove(packet);
            }
            else
            {
                serverSide[packet] = parser;
            }
        }

        /// <summary>
        /// Sets a parser for the given packet ID. <para/>
        /// The parser will be called whenever clients receive a packet of the given type.
        /// </summary>
        /// <param name="packet"> The ID of the packet. You can choose any value you want (but keep it the same for the server side parser). </param>
        /// <param name="parser"> The parser to call when a packet is received. </param>
        public void SetClientSideParser(ushort packet, ClientSideParser parser)
        {
            ErrorHelper.ThrowIfNotLoading(Mod);

            if (parser == null)
            {
                clientSide.Remove(packet);
            }
            else
            {
                clientSide[packet] = parser;
            }
        }

        #endregion

        internal NetworkEntry() { }

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
