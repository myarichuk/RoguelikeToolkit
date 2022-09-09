using System;
using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;

namespace RoguelikeToolkit.Entities.Factory
{
	//a utility to traverse the entity graph
	internal struct EntityGraphWalker
	{
		private readonly EntityTemplate _root;
		private readonly GraphTraversalType _traversalType;

		private static readonly ObjectPool<Queue<EntityTemplate>> QueuePool =
			ObjectPoolProvider.Instance.Create<Queue<EntityTemplate>>();

		public EntityGraphWalker(EntityTemplate root, GraphTraversalType traversalType = GraphTraversalType.BFS)
		{
			_root = root;
			_traversalType = traversalType;
		}

		public void Traverse(Action<EntityTemplate> visitorFunc)
		{
			Queue<EntityTemplate> traversalQueue = null;
			try
			{
				traversalQueue = QueuePool.Get();
				traversalQueue.Enqueue(_root);

				while (traversalQueue.TryDequeue(out var currentNode))
				{
					if(_traversalType == GraphTraversalType.BFS)
						visitorFunc(currentNode);

					foreach(var childNode in currentNode.EmbeddedTemplates)
						traversalQueue.Enqueue(childNode);

					if(_traversalType == GraphTraversalType.DFS)
						visitorFunc(currentNode);
				}
			}
			finally
			{
				if(traversalQueue != null)
					QueuePool.Return(traversalQueue);
			}
		}
	}
}
