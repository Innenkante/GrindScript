using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SoG.Modding
{
    /// <summary>
    /// Bookkeeping class for IDs.
    /// </summary>
    internal class IDAllocator
	{
		public IDAllocator()
		{
			_idStart = new ReadOnlyDictionary<Type, object>(GameObjectStuff.CreateIDStart());
			_idEnd = new ReadOnlyDictionary<Type, object>(GameObjectStuff.CreateIDEnd());

			_id = new Dictionary<Type, object>();

			foreach (var pair in _idStart)
            {
				_id[pair.Key] = pair.Value;
            }
        }

		private IReadOnlyDictionary<Type, object> _idStart;

		private IReadOnlyDictionary<Type, object> _idEnd;

		private Dictionary<Type, object> _id;

		/// <summary>
		/// Gets the starting ID of the given ID type.
		/// </summary>
		public IDType GetIDStart<IDType>() where IDType : struct, Enum
		{
			if (!_idStart.TryGetValue(typeof(IDType), out object startID))
			{
				throw new InvalidOperationException(ErrorHelper.BadIDType);
			}

			if (!(startID is IDType))
			{
				throw new InvalidOperationException(ErrorHelper.InternalError);
			}

			return (IDType)startID;
		}

		/// <summary>
		/// Gets the next ID of the given ID type.
		/// You need to call <see cref="AllocateID{IDType}"/> to actually allocate the ID.
		/// </summary>
		public IDType GetIDNext<IDType>() where IDType : struct, Enum
		{
			if (!_id.TryGetValue(typeof(IDType), out object id))
			{
				throw new InvalidOperationException(ErrorHelper.BadIDType);
			}

			if (!(id is IDType))
			{
				throw new InvalidOperationException(ErrorHelper.InternalError);
			}

			return (IDType)id;
		}

		/// <summary>
		/// Gets the last ID of the given ID type.
		/// IDs after the end are considered as available for temporary use.
		/// </summary>
		public IDType GetIDEnd<IDType>() where IDType : struct
        {
			if (!_idEnd.TryGetValue(typeof(IDType), out object maxID))
            {
				throw new InvalidOperationException(ErrorHelper.BadIDType);
			}

			if (!(maxID is IDType))
            {
				throw new InvalidOperationException(ErrorHelper.InternalError);
			}

			return (IDType)maxID;
		}

		/// <summary>
		/// Allocates the next ID and returns it.
		/// </summary>
		public IDType AllocateID<IDType>() where IDType : struct
		{
			if (!_id.TryGetValue(typeof(IDType), out object id))
            {
				throw new InvalidOperationException(ErrorHelper.BadIDType);
            }

			if (!(id is IDType))
            {
				throw new InvalidOperationException(ErrorHelper.InternalError);
			}

			object nextID;
			if (typeof(IDType).IsEnum)
			{
				nextID = Enum.ToObject(typeof(IDType), Convert.ToInt64(id) + 1);
			}
			else
            {
				throw new InvalidOperationException(ErrorHelper.InternalError);
			}

			if (nextID.Equals(GetIDEnd<IDType>()))
            {
				throw new InvalidOperationException(ErrorHelper.OutOfIdentifiers);
			}

			_id[typeof(IDType)] = nextID;

			return (IDType)id;
        }

		/// <summary>
		/// Resets all IDs. This should be used during unloading only.
		/// </summary>
		public void Reset()
        {
			foreach (var pair in new Dictionary<Type, object>(_id))
            {
				_id[pair.Key] = _idStart[pair.Key];
            }
		}
	}
}