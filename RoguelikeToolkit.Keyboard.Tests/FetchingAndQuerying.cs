using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RoguelikeToolkit.Keyboard.Tests
{
	public class FetchingAndQuerying
	{
		private readonly KeyMappingStore<KeyCode, string> _store = new KeyMappingStore<KeyCode, string>();

		[Fact]
		public void Should_fetch_empty_array_nothing_found()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			_store.AddMapping(KeyCode.Vc1, KeyCode.Vc2, "action2");

			var actionIDs = _store.FetchActionIDsFor(KeyCode.Vc0, KeyCode.VcEquals);
			Assert.Empty(actionIDs);
		}

		[Fact]
		public void Can_find_single_key()
		{
			_store.AddMapping(KeyCode.Vc0, "action1");
			Assert.True(_store.HasMappingFor(KeyCode.Vc0));
			Assert.False(_store.HasMappingFor(KeyCode.Vc1));
		}

		[Fact]
		public void Can_find_two_keys()
		{
			_store.AddMapping(KeyCode.Vc0, KeyCode.Vc1, "action1");
			Assert.True(_store.HasMappingFor(KeyCode.Vc0, KeyCode.Vc1));
			Assert.False(_store.HasMappingFor(KeyCode.Vc1, KeyCode.Vc3));
		}

		[Fact]
		public void Can_find_three_keys()
		{
			_store.AddMapping(KeyCode.Vc0, KeyCode.Vc1, KeyCode.VcG, "action1");
			Assert.True(_store.HasMappingFor(KeyCode.Vc0, KeyCode.Vc1, KeyCode.VcG));
			Assert.False(_store.HasMappingFor(KeyCode.Vc1, KeyCode.Vc3, KeyCode.VcG));
		}
	}
}
