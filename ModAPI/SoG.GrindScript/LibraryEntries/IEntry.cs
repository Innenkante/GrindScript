namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// IEntry represents game objects that can be created in SoG.
    /// Two different entries cannot have the same owner mod and ModID at the same time.
    /// </summary>
    internal interface IEntry<IDType> where IDType : struct
    {
        Mod Owner { get; }

        IDType GameID { get; }

        string ModID { get; }

        /// <summary>
        /// Called when the entry should be initialized.
        /// Initialization includes loading assets and doing modifications to game fields.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the entry should be cleaned up.
        /// Clean up includes unloading assets, and restoring the game's fields to their original state.
        /// </summary>
        void Cleanup();
    }
}
