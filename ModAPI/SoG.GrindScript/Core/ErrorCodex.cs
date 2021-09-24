namespace SoG.Modding
{
    /// <summary>
    /// Holds a list of generic error messages.
    /// </summary>
    internal static class ErrorCodex
    {
        public static string UseOnlyDuringLoad => $"Game objects can not be created outside of {nameof(Mod.Load)}.";

        public static string DuplicateModID => $"A game object with the given ModID already exists. ModIDs must be distinct between game objects of the same ID type.";

        public static string AudioAlreadyInitialized => "Audio for this mod has already been created. Do not call this method twice.";

        public static string NoWhiteSpaceInCommand => "Commands must not have any whitespace.";
    }
}
