using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChatSerializableDictionary<TKey, TValue>
{
  [SerializeField] private List<TKey> keys = new List<TKey>();
  [SerializeField] private List<TValue> values = new List<TValue>();

  public Dictionary<TKey, TValue> ToDictionary()
  {
    var dictionary = new Dictionary<TKey, TValue>();
    for (int i = 0; i < keys.Count; i++)
    {
      if (i < values.Count) // 防止越界
      {
        dictionary[keys[i]] = values[i];
      }
    }
    return dictionary;
  }

  public void FromDictionary(Dictionary<TKey, TValue> dictionary)
  {
    keys.Clear();
    values.Clear();

    foreach (var pair in dictionary)
    {
      keys.Add(pair.Key);
      values.Add(pair.Value);
    }
  }
}