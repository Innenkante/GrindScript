namespace SoG.Modding
{
    /// <summary>
    /// Represents a modded game object.
    /// </summary>
    public abstract class Entry<IDType> where IDType : struct
    {
        /// <summary>
        /// Gets the mod that created this entry.
        /// </summary>
        public abstract Mod Mod { get; internal set; }

        /// <summary>
        /// Gets the mod ID of this entry.
        /// </summary>
        public abstract string ModID { get; internal set; }

        /// <summary>
        /// Gets the vanilla ID of this entry.
        /// </summary>
        public abstract IDType GameID { get; internal set; }

        internal abstract void Initialize();

        internal abstract void Cleanup();
    }
}