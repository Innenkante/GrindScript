using Quests;
using SoG.Modding.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SoG.Modding
{
    internal class Library
    {
        private static MethodInfo s_initializeStorage = AccessTools.Method(typeof(Library), nameof(InitializeStorage));

        private static MethodInfo s_cleanupStorage = AccessTools.Method(typeof(Library), nameof(CleanupStorage));

        private static MethodInfo s_getModStorage = AccessTools.Method(typeof(Library), nameof(GetAllEntries));

        private static MethodInfo s_removeModEntries = AccessTools.Method(typeof(Library), nameof(RemoveModEntriesFromStorage));

        private Dictionary<Type, IDictionary> _typeStorage = new Dictionary<Type, IDictionary>();

        /// <summary>
        /// Creates a new Library, and instantiates it using supported game object types.
        /// </summary>
        public Library()
        {
            GameObjectStuff.SetupLibrary(this);
        }

        /// <summary>
        /// Gets the entry associated with the given ID.
        /// Returns true if there exists a storage containing an entry for the specified key, false otherwise.
        /// </summary>
        public bool GetEntry<IDType, EntryType>(IDType gameID, out EntryType entry)
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            if (_typeStorage.TryGetValue(typeof(Dictionary<IDType, EntryType>), out IDictionary storage))
            {
                var typedStorage = (Dictionary<IDType, EntryType>)storage;

                if (typedStorage.TryGetValue(gameID, out entry))
                {
                    return true;
                }
            }

            entry = default;
            return false;
        }

        /// <summary>
        /// Gets the entry associated with the given modID.
        /// Returns true if there exists a storage containing an entry for the specified key, false otherwise.
        /// </summary>
        public bool GetModEntry<IDType, EntryType>(Mod mod, string modID, out EntryType entry)
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            if (_typeStorage.TryGetValue(typeof(Dictionary<IDType, EntryType>), out IDictionary storage))
            {
                var typedStorage = (Dictionary<IDType, EntryType>)storage;

                entry = typedStorage.Values.FirstOrDefault(x => x.Mod == mod && x.ModID == modID);
                return entry != null;
            }

            entry = default;
            return false;
        }

        /// <summary>
        /// Gets the storage associated with the given ID and EntryType.
        /// </summary>
        public Dictionary<IDType, EntryType> GetAllEntries<IDType, EntryType>()
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            if (_typeStorage.TryGetValue(typeof(Dictionary<IDType, EntryType>), out IDictionary storage))
            {
                return (Dictionary<IDType, EntryType>)storage;
            }

            return null;
        }

        /// <summary>
        /// Gets the storage associated with the given ID and EntryType, containing items from a mod.
        /// </summary>
        public Dictionary<IDType, EntryType> GetModEntries<IDType, EntryType>(Mod mod)
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            if (_typeStorage.TryGetValue(typeof(Dictionary<IDType, EntryType>), out IDictionary storage))
            {
                var typedStorage = (Dictionary<IDType, EntryType>)storage;

                return typedStorage.Where(x => x.Value.Mod == mod).ToDictionary(x => x.Key, x=> x.Value);
            }

            return new Dictionary<IDType, EntryType>();
        }

        /// <summary>
        /// Removes all entries from a given mod. 
        /// This method doesn't cleanup entries. Call <see cref="CleanupEntries"/> before removing them.
        /// </summary>
        public void RemoveModEntries(Mod mod)
        {
            foreach (var pair in new Dictionary<Type, IDictionary>(_typeStorage))
            {
                s_removeModEntries.MakeGenericMethod(pair.Key.GenericTypeArguments).Invoke(this, new object[] { mod });
            }
        }

        /// <summary>
        /// Removes all entries.
        /// This method doesn't cleanup entries. Call <see cref="CleanupEntries"/> before removing them.
        /// </summary>
        public void RemoveAllEntries()
        {
            foreach (var pair in _typeStorage)
            {
                pair.Value.Clear();
            }
        }

        /// <summary>
        /// Calls <see cref="IEntry.Initialize"/> for each entry.
        /// </summary>
        public void InitializeEntries()
        {
            foreach (var pair in _typeStorage)
            {
                s_initializeStorage.MakeGenericMethod(pair.Key.GenericTypeArguments).Invoke(this, null);
            }
        }

        /// <summary>
        /// Calls <see cref="IEntry.Cleanup"/> for each entry.
        /// </summary>
        public void CleanupEntries()
        {
            foreach (var pair in _typeStorage)
            {
                s_cleanupStorage.MakeGenericMethod(pair.Key.GenericTypeArguments).Invoke(this, null);
            }
        }

        /// <summary>
        /// Gets a view of the library where all entries are part of the given mod.
        /// </summary>
        public Library GetModView(Mod mod)
        {
            Library view = new Library();

            foreach (var pair in _typeStorage)
            {
                IDictionary modStorage = s_getModStorage.MakeGenericMethod(pair.Key.GenericTypeArguments).Invoke(this, new[] { mod }) as IDictionary;

                view._typeStorage[pair.Key] = modStorage;
            }

            return view;
        }

        #region Helper methods

        private void InitializeStorage<IDType, EntryType>()
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            var typedStorage = (Dictionary<IDType, EntryType>)_typeStorage[typeof(Dictionary<IDType, EntryType>)];

            foreach (var pair in typedStorage)
            {
                pair.Value.Initialize();
            }
        }

        private void CleanupStorage<IDType, EntryType>()
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            var typedStorage = (Dictionary<IDType, EntryType>)_typeStorage[typeof(Dictionary<IDType, EntryType>)];

            foreach (var pair in typedStorage)
            {
                pair.Value.Cleanup();
            }
        }

        private void RemoveModEntriesFromStorage<IDType, EntryType>(Mod mod)
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            var typedStorage = (Dictionary<IDType, EntryType>)_typeStorage[typeof(Dictionary<IDType, EntryType>)];

            _typeStorage[typeof(Dictionary<IDType, EntryType>)] = typedStorage.Where(x => x.Value.Mod != mod).ToDictionary(x => x.Key, x => x.Value);
        }

        internal void CreateStorage<IDType, EntryType>()
            where IDType : struct
            where EntryType : Entry<IDType>
        {
            _typeStorage.Add(typeof(Dictionary<IDType, EntryType>), new Dictionary<IDType, EntryType>());
        }

        #endregion
    }
}
