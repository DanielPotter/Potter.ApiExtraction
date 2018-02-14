using System.Collections.Generic;

namespace Potter.ApiExtraction.Core.Generation
{
    public static class CollectionExtensions
    {
        #region Multi Value Dictionary

        public static void AddItem<TKey, TValueList, TValue>(this IDictionary<TKey, TValueList> dictionary, TKey key, TValue value)
            where TValueList : ICollection<TValue>, new()
        {
            if (dictionary.TryGetValue(key, out TValueList list) == false)
            {
                list = dictionary[key] = new TValueList();
            }

            list.Add(value);
        }

        #endregion
    }
}
