// SerializableDictionary.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChatSerializableDictionary1<TKey, TValue> : ISerializationCallbackReceiver
{
  [SerializeField] private List<TKey> keys = new List<TKey>();
  [SerializeField] private List<TValue> values = new List<TValue>();

  public Dictionary<TKey, TValue> ToDictionary()
  {
    var dict = new Dictionary<TKey, TValue>();
    for (int i = 0; i < keys.Count; i++)
    {
      dict[keys[i]] = values[i];
    }
    return dict;
  }

  public void OnBeforeSerialize() { }
  public void OnAfterDeserialize() { }
}
