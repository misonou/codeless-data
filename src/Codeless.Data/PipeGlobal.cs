using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  /// <summary>
  /// Represents a collection of global values accessible regardless of the current context in the evaluation stack.
  /// </summary>
  public class PipeGlobal : IDictionary<string, PipeValue> {
    private readonly Dictionary<string, PipeValue> dictionary = new Dictionary<string, PipeValue>();
    private readonly PipeGlobal parent;

    /// <summary>
    /// Instantiate the <see cref="PipeGlobal"/> class.
    /// </summary>
    public PipeGlobal() { }

    /// <summary>
    /// Instantiate the <see cref="PipeGlobal"/> class initialized with given entries.
    /// Dictionary keys are converted to string values through <see cref="PipeValue"/> string conversion.
    /// If two distinct keys give the same string representation, exception will be thrown.
    /// </summary>
    /// <param name="ht"></param>
    public PipeGlobal(IDictionary ht) {
      foreach (DictionaryEntry e in ht) {
        Add(new PipeValue(e.Key).ToString(), new PipeValue(e.Value));
      }
    }

    /// <summary>
    /// Instantiate the <see cref="PipeGlobal"/> class inheriting entries in another <see cref="PipeGlobal"/> instance.
    /// This newly created instance will expose the inherited entries but not modifying them.
    /// Setting value with the same key that appears in the parent <see cref="PipeGlobal"/>instance will mask that entry with the new value.
    /// </summary>
    /// <param name="parent"></param>
    public PipeGlobal(PipeGlobal parent) {
      this.parent = parent;
    }

    /// <summary>
    /// Gets or sets value associated with the given key.
    /// If the given key is not present in the dictionary, <see cref="PipeValue.Undefined"/> is returned.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public PipeValue this[string key] {
      get {
        PipeValue value;
        if (dictionary.TryGetValue(key, out value)) {
          return value;
        }
        if (parent != null) {
          return parent[key];
        }
        return PipeValue.Undefined;
      }
      set {
        dictionary[key] = value;
      }
    }

    /// <summary>
    /// Gets the number of entries contained including inherited entries if any.
    /// </summary>
    public int Count {
      get { return dictionary.Count + (parent != null ? parent.Count : 0); }
    }

    public void Add(string key, object value) {
      dictionary.Add(key, new PipeValue(value));
    }

    public void Add(string key, PipeValue value) {
      dictionary.Add(key, value);
    }

    public void Clear() {
      dictionary.Clear();
    }

    public bool ContainsKey(string key) {
      return dictionary.ContainsKey(key) || (parent != null && parent.ContainsKey(key));
    }

    public bool Remove(string key) {
      return dictionary.Remove(key);
    }

    public bool TryGetValue(string key, out PipeValue value) {
      if (dictionary.TryGetValue(key, out value)) {
        return true;
      }
      if (parent != null) {
        return parent.TryGetValue(key, out value);
      }
      return false;
    }

    #region Interfaces
    bool ICollection<KeyValuePair<string, PipeValue>>.IsReadOnly {
      get { return false; }
    }

    ICollection<string> IDictionary<string, PipeValue>.Keys {
      get {
        throw new NotImplementedException();
      }
    }

    ICollection<PipeValue> IDictionary<string, PipeValue>.Values {
      get {
        throw new NotImplementedException();
      }
    }

    void ICollection<KeyValuePair<string, PipeValue>>.Add(KeyValuePair<string, PipeValue> item) {
      dictionary.Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<string, PipeValue>>.Contains(KeyValuePair<string, PipeValue> item) {
      throw new NotImplementedException();
    }

    bool ICollection<KeyValuePair<string, PipeValue>>.Remove(KeyValuePair<string, PipeValue> item) {
      throw new NotImplementedException();
    }

    void ICollection<KeyValuePair<string, PipeValue>>.CopyTo(KeyValuePair<string, PipeValue>[] array, int arrayIndex) {
      throw new NotImplementedException();
    }

    IEnumerator<KeyValuePair<string, PipeValue>> IEnumerable<KeyValuePair<string, PipeValue>>.GetEnumerator() {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      throw new NotImplementedException();
    }
    #endregion
  }
}
