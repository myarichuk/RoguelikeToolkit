using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoguelikeToolkit.Keyboard.Tests
{
	public class AddingRemoving
	{
		private readonly KeyMappingStore<KeyCode, string> _store = new KeyMappingStore<KeyCode, string>();

		[Fact]
		public void Can_add_single_key()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0).ToArray();

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			Assert.Equal("action1", storedKeys[0]);
		}

		[Fact]
		public void Can_add_two_single_keys_the_same_action()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			_store.AddMapping(KeyCode.Vc1, "action1");
			_store.AddMapping(KeyCode.Vc1, "action1");

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc1).ToArray();

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			Assert.Equal("action1", storedKeys[0]);
		}


		[Fact]
		public void Can_add_two_keys()
		{
			_store.AddMapping(KeyCode.Vc0, KeyCode.VcG, "action1");

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0, KeyCode.VcG).ToArray();

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			Assert.Equal("action1", storedKeys[0]);
		}

		[Fact]
		public void Can_add_two_pairs_two_keys_same_action()
		{
			_store.AddMapping(KeyCode.Vc1, KeyCode.VcG, "action1");
			_store.AddMapping(KeyCode.Vc0, KeyCode.VcG, "action1");
			_store.AddMapping(KeyCode.Vc1, KeyCode.VcG, "action1");

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc1, KeyCode.VcG).ToArray();

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			Assert.Equal("action1", storedKeys[0]);
		}

		[Fact]
		public void Can_add_three_keys()
		{
			_store.AddMapping(KeyCode.Vc0, KeyCode.VcInsert, KeyCode.VcG, "action1");

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0, KeyCode.VcInsert, KeyCode.VcG).ToArray();

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			Assert.Equal("action1", storedKeys[0]);
		}

	}
}
