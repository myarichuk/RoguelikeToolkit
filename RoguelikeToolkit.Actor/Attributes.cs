using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Security;
using System.Text;

namespace RoguelikeToolkit.Actor
{
    public class Attributes : DynamicObject, IEnumerable<KeyValuePair<string, object>>
    {
        private readonly ExpandoObject _values = new ExpandoObject();

        public IEnumerator<KeyValuePair<string, dynamic>> GetEnumerator() =>
            ((IDictionary<string, object>)_values).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IDictionary<string, object>)_values).GetEnumerator();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var values = (IDictionary<string, object>) _values;
            return values.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var values = (IDictionary<string, object>) _values;
            if (values.ContainsKey(binder.Name))
                values[binder.Name] = value;
            else
                values.Add(binder.Name, value);

            return true;
        }
    }
}
