using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GitHub.Internals
{
	internal class ConcurrentSet<T>
	{
		static readonly byte One = 1;
		static readonly byte Zero = 0;

		ConcurrentDictionary<T, byte> _dictionary = new ConcurrentDictionary<T, byte>();

		public bool IsEmpty => _dictionary.IsEmpty;
		public ICollection<T> Items => _dictionary.Keys;

		public bool ContainsKey(T item) => _dictionary.ContainsKey(item);

		public bool TryAdd(T item)
		{
			byte data = _dictionary.AddOrUpdate(
				item,
				Zero,
				(key, value) => (byte)(value + One));

			return data == Zero;
		}

		public bool TryRemove(T item)
		{
			byte _;
			return _dictionary.TryRemove(item, out _);
		}
	}
}