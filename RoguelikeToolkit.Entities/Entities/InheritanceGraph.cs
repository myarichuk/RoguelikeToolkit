using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public class InheritanceGraph
    {
        private readonly static ObjectPool<Queue<string>> _queuePool = ObjectPoolProvider.Instance.Create(new ThreadSafeObjectPoolPolicy<Queue<string>>());
        private readonly static ObjectPool<HashSet<string>> _visitedPool = ObjectPoolProvider.Instance.Create(new ThreadSafeObjectPoolPolicy<HashSet<string>>());

        private readonly IReadOnlyDictionary<string, List<string>> _adjacencyList;
        private readonly IReadOnlyDictionary<string, EntityTemplate> _templates;

        public InheritanceGraph(params EntityTemplate[] templates) : this(templates.AsEnumerable())
        {
        }

        public InheritanceGraph(IReadOnlyDictionary<string, EntityTemplate> templates)
        {
            ValidateAndThrowIfNeeded(templates.Values);

            _templates = templates;
            _adjacencyList = templates.Values
                .ToDictionary(t => t.Id,
                              t => t.Inherits.ToList(), StringComparer.InvariantCultureIgnoreCase);
        }

        public InheritanceGraph(IEnumerable<EntityTemplate> templates)
        {
            ValidateAndThrowIfNeeded(templates);

            _adjacencyList = templates
                .ToDictionary(t => t.Id,
                              t => t.Inherits.ToList(), StringComparer.InvariantCultureIgnoreCase);

            _templates = templates.ToDictionary(t => t.Id, t => t, StringComparer.InvariantCultureIgnoreCase);
        }

        //in order to calculate "effective" template, we need to traverse up the inheritance chain
        public IEnumerable<EntityTemplate> GetInheritanceChainFor(EntityTemplate template)
        {
            //sanity check, not strictly needed but it is good this is here
            if (_templates.ContainsKey(template.Id) == false)
                ThrowIrrelevantTemplate(template);


            if (template?.Inherits.Count == 0) //no need to work hard...
            {
                yield return template; //if we have zero inheritance, ensure we return the tempalte we started inheritance search
                yield break;
            }

            Queue<string> traversalQueue = null;
            HashSet<string> visited = null;
            try
            {
                traversalQueue = _queuePool.Get();
                visited = _visitedPool.Get();
                traversalQueue.Enqueue(template.Id);
                while (traversalQueue.Count > 0)
                {
                    var currentId = traversalQueue.Dequeue();

                    if (visited.Contains(currentId)) //ignore circles
                        continue;

                    visited.Add(currentId);
                    yield return _templates[currentId];

                    for (int i = 0; i < _adjacencyList[currentId].Count; i++)
                        traversalQueue.Enqueue(_adjacencyList[currentId][i]);
                }
            }
            finally
            {
                if (traversalQueue != null)
                {
                    traversalQueue.Clear();
                    _queuePool.Return(traversalQueue);
                }

                if (visited != null)
                {
                    visited.Clear();
                    _visitedPool.Return(visited);
                }
            }
        }

        #region Helpers

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIrrelevantTemplate(EntityTemplate template) => 
            throw new InvalidOperationException("Cannot resolve dependency graph from irrelevant template");

        private static void ValidateAndThrowIfNeeded(IEnumerable<EntityTemplate> templates)
        {
            //sanity check, this shouldn't happen!
            if (templates.Any(t => t.Inherits.Contains(t.Id)))
                throw new InvalidOperationException($"Self-inheritance is now allowed as it is silly. (self-inheritance found in template with Id = {templates.First(t => t.Inherits.Contains(t.Id)).Id}");
        }

        #endregion
    }
}
