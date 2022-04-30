using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RoguelikeToolkit.PersistentArray
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly", Justification = "Existing implementation is enough (no managed resources to dispose!)")]
	public unsafe class PersistentArray<TCell> : IReadOnlyList<TCell>,IDisposable where TCell : unmanaged
	{
		private static readonly int CellSize = Marshal.SizeOf<TCell>();
		private bool _disposedValue;
		private readonly MemoryMappedFile _mmf;
		private readonly MemoryMappedViewAccessor _accessor;
		private readonly int _size;
		private readonly byte* _basePtr;
		public int Count => _size;

		public TCell this[int index]
		{
			get => *(TCell*)(_basePtr + (CellSize * index));
			set => *(TCell*)(_basePtr + (CellSize * index)) = value;
		}

		public PersistentArray(int size): this(Guid.NewGuid().ToString(), size)
		{
		}

		public PersistentArray(string arrayName, int size)
		{
			_mmf = MemoryMappedFile.CreateOrOpen(
				arrayName,
				CellSize * size);
			_accessor = _mmf.CreateViewAccessor();
			_accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _basePtr);
			_size = size;
		}

		~PersistentArray() => Dispose();

		public void Dispose()
		{
			if (!_disposedValue)
			{
				_disposedValue = true;
				_accessor.SafeMemoryMappedViewHandle.ReleasePointer();
				_accessor.Dispose();
				_mmf?.Dispose();
				GC.SuppressFinalize(this);
			}

		}

		public IEnumerator<TCell> GetEnumerator() => new ArrayEnumerator(this);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private struct ArrayEnumerator: IEnumerator<TCell>
		{
			private readonly PersistentArray<TCell> _parent;
			private int _currentIndex;

			public ArrayEnumerator(PersistentArray<TCell> parent)
			{
				_parent = parent;
				_currentIndex = 0;
			}

			public TCell Current => _parent[_currentIndex];

			object IEnumerator.Current => _parent[_currentIndex];

			public void Dispose()
			{
				// Method intentionally left empty - nothing to do.
			}

			public bool MoveNext() => ++_currentIndex < _parent.Count;
			public void Reset() => _currentIndex = 0;
		}
	}
}
