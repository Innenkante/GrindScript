namespace SoG.Modding.LibraryEntries
{
    /// <summary>
    /// ISaveableEntry represent objects that are saved by SoG in one way or another.
    /// As a result, additional information is required for ensuring consistency.
    /// </summary>
    interface IEntry<IDType> where IDType : struct
    {
        Mod Owner { get; }

        IDType GameID { get; }

        string ModID { get; }
    }
}
