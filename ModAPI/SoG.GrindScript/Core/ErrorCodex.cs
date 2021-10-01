namespace SoG.Modding
{
    /// <summary>
    /// Holds a list of generic error messages.
    /// </summary>
    internal static class ErrorCodex
    {
        public static string UseOnlyDuringLoad => $"Game objects can not be created outside of {nameof(Mod.Load)}.";

        public static string DuplicateModID => $"A game object with the given ModID already exists. ModIDs must be distinct between game objects of the same type.";

        public static string AudioAlreadyInitialized => "Audio for this mod has already been created. Calling this method multiple times is not supported (yet).";

        public static string NoWhiteSpaceInCommand => "Commands must not have any whitespace.";

        public static string ObjectCreationDisabled => "Creating game objects is now allowed for mods with disabled object creation.";
    }
}
