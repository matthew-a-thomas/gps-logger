using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Messages
{
    public class Message<T>
    {
        public T Contents { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ID { get; set; }
        public string Salt { get; set; }
        public long UnixTime { get; set; }

        protected virtual IEnumerable<object> GetFields() => new object[] { Contents, ID, Salt, UnixTime };
        public override int GetHashCode() => GetFields().Where(x => !ReferenceEquals(x, null)).Aggregate(0, (accumulate, x) => accumulate + x.GetHashCode());
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj.GetType().Equals(GetType())))
                return false;
            var ourFields = GetFields();
            var theirFields = (obj as Message<T>)?.GetFields();
            var together = ourFields.Zip(theirFields, Tuple.Create);
            var allEqual = together.All(tuple => tuple.Item1?.Equals(tuple.Item2) ?? false);
            return allEqual;
        }
    }
}