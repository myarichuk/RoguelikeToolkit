using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoguelikeToolkit.Keyboard.Tests
{
	public class Removing
	{
		private readonly KeyMappingStore<KeyCode, string> _store = new KeyMappingStore<KeyCode, string>();

		[Fact]
		public void Can_remove_single_key()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			_store.RemoveMappingOf(KeyCode.Vc0);

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0);

			Assert.NotNull(storedKeys);
			Assert.Empty(storedKeys);
		}

		[Fact]
		public void Should_not_remove_not_related_mappings()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			_store.AddMapping(KeyCode.Vc1, KeyCode.Vc0, "action1");

			_store.RemoveMappingOf(KeyCode.Vc0);

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc1 ,KeyCode.Vc0);

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);
		}

		[Fact]
		public void Can_remove_two_single_keys_the_same_action()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			_store.AddMapping(KeyCode.Vc0, "action1");

			_store.RemoveMappingOf(KeyCode.Vc0);

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0);

			Assert.NotNull(storedKeys);
			Assert.Empty(storedKeys);
		}

		[Fact]
		public void Can_remove_mapping_with_multiple_keys()
		{
			_store.AddMapping(KeyCode.Vc0, KeyCode.Vc1, "action1");
			_store.AddMapping(KeyCode.Vc0, "action1");

			_store.RemoveMappingOf(KeyCode.Vc0, KeyCode.Vc1);

			var storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0);

			Assert.NotNull(storedKeys);
			Assert.Single(storedKeys);

			storedKeys = _store.FetchActionIDsFor(KeyCode.Vc0, KeyCode.Vc1);
			Assert.Empty(storedKeys);
		}

	}
}
