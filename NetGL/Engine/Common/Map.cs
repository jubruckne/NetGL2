namespace NetGL;

public static class DictionaryExtensions {
    public static TKey FindKeyByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value) {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));

        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            if (value != null && value.Equals(pair.Value)) return pair.Key;

        throw new Exception("the value is not found in the dictionary");
    }
}