using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoguelikeToolkit.Entities
{
    public class InheritanceGraph
    {
        private readonly static ObjectPool<Queue<string>> _queuePool = ObjectPoolProvider.Instance.Create<Queue<string>>();
        private readonly static ObjectPool<HashSet<string>> _visitedPool = ObjectPoolProvider.Instance.Create<HashSet<string>>();

        private readonly IReadOnlyDictionary<string, List<string>> _adjacencyList;
        private readonly IReadOnlyDictionary<string, EntityTemplate> _templates;

        public InheritanceGraph(params EntityTemplate[] templates) : this(templates.AsEnumerable())
        {
        }

        public InheritanceGraph(IEnumerable<EntityTemplate> templates)
        {
            //sanity check, this shouldn't happen!
            if (templates.Any(t => t.Inherits.Contains(t.Id)))
                throw new InvalidOperationException($"Self-inheritance is now allowed as it is silly. (self-inheritance found in tempalte with Id = {templates.First(t => t.Inherits.Contains(t.Id)).Id}");
            
            if (templates.Count(t => t.Inherits.Count == 0) == 0)
                throw new InvalidOperationException("No roots are found for the inheritance graph. In order to create proper inheritance graph for the entity templates, there should be at least one template *WITHOUT* any inheritance, so the inheritance could be resolved for each template.");
            
            //index inheritance
            _adjacencyList = templates.ToDictionary(
                t => t.Id, 
                t => t.Inherits.ToList());
            
            _templates = templates.ToDictionary(t => t.Id, t => t);
        }

        //in order to calculate "effective" template, we need to traverse up the inheritance chain
        public IEnumerable<EntityTemplate> GetInheritanceChainFor(EntityTemplate template)
        {
            if (template?.Inherits.Count == 0) //no need to work hard...
                yield break;

            //sanity check, not strictly needed but it is good this is here
            if (_templates.ContainsKey(template.Id) == false)
                ThrowIrrelevantTemplate(template);

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

                    if (visited.Contains(currentId))
                        throw new InvalidOperationException("Inheritance circle detected, it is not allowed to have entity templates inheritance circles");

                    visited.Add(currentId);
                    yield return _templates[currentId];

                    for (int i = 0; i < _adjacencyList[currentId].Count; i++)
                    {
                        var adjacentId = _adjacencyList[currentId][i];
                        traversalQueue.Enqueue(adjacentId);
                    }
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIrrelevantTemplate(EntityTemplate template) => 
            throw new InvalidOperationException("Cannot resolve dependency graph from irrelevant template");
    }
}
