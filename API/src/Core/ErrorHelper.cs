using System;

namespace SoG.Modding
{
    /// <summary>
    /// Helper class for error handling.
    /// </summary>
    internal static class ErrorHelper
    {
        public static string UseThisDuringLoad => $"This method should be called only during {nameof(Mod.Load)}.";

        public static string UseThisAfterLoad => $"This method cannot be called during {nameof(Mod.Load)}. Try calling it in {nameof(Mod.PostLoad)} instead.";

        public static string DuplicateModID => $"A game object with the given ModID already exists. ModIDs must be distinct between game objects of the same type.";

        public static string AudioEntryAlreadyCreated => "An audio entry for this mod has already been created. You can only call this method once.";

        public static string NoWhiteSpaceInCommand => "Commands must not have any whitespace.";

        public static string ObjectCreationDisabled => "Creating game objects is now allowed for mods with disabled object creation.";

        public static string OutOfIdentifiers => "Ran out of identifiers for the given object type. Smells of big modding energy!";

        public static string BadIDType => "The IP type requested does not support allocating.";

        public static string InternalError => "Something bad happened inside the modding tool. Report this to the developer, please!";

        public static string UnknownEntry => "Failed to retrieve a valid mod entry in a method that doesn't use default entries.";

        public static void ThrowIfNotLoading(Mod mod)
        {
            if (!mod.InLoad)
            {
                throw new InvalidOperationException(UseThisDuringLoad);
            }
        }

        public static void ThrowIfLoading(Mod mod)
        {
            if (mod.InLoad)
            {
                throw new InvalidOperationException(UseThisAfterLoad);
            }
        }

        public static void AssertLoading(Mod mod, Action statement)
        {
            if (!mod.InLoad)
            {
                throw new InvalidOperationException(UseThisDuringLoad);
            }
            else
            {
                statement();
            }
        }

        public static void ThrowIfObjectCreationDisabled(Mod mod)
        {
            if (mod.DisableObjectCreation)
                throw new InvalidOperationException(ObjectCreationDisabled);
        }

        public static void ThrowIfDuplicateEntry<IDType, EntryType>(Mod mod, string modID)
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            bool existingEntry = mod.Manager.Library.GetModEntry<IDType, EntryType>(mod, modID, out _);

            if (existingEntry)
                throw new InvalidOperationException(DuplicateModID);
        }

        public static void Assert(bool trueCondition, string exceptionMessage = null)
        {
            if (!trueCondition)
            {
                throw new InvalidOperationException(exceptionMessage ?? InternalError);
            }
        }
    }
}
