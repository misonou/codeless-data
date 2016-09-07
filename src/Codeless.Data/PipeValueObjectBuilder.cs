using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  public class PipeValueObjectBuilder {
    private readonly List<object> array = new List<object>();
    private readonly bool isArray;

    public PipeValueObjectBuilder(bool isArray) {
      this.isArray = isArray;
    }

    public PipeValueObjectBuilder(IEnumerable values)
      : this(true) {
      CommonHelper.ConfirmNotNull(values, "values");
      foreach (object value in values) {
        array.Add(new PipeValue(value).Value);
      }
    }

    public PipeValueObjectBuilder(IEnumerable<PipeValue> values)
      : this(values, false) { }

    public PipeValueObjectBuilder(IEnumerable<PipeValue> values, bool flatten)
      : this(true) {
      CommonHelper.ConfirmNotNull(values, "values");
      foreach (PipeValue value in values) {
        if (flatten && value.IsArray) {
          foreach (object obj in (IEnumerable)value.Value) {
            array.Add(new PipeValue(obj).Value);
          }
        } else {
          array.Add(value.Value);
        }
      }
    }

    public PipeValueObjectBuilder(IDictionary values)
      : this(false) {
      CommonHelper.ConfirmNotNull(values, "values");
      foreach (string key in values.Keys.OfType<string>()) {
        array.Add(new KeyValuePair<string, object>(key, new PipeValue(values[key]).Value));
      }
    }

    public int Count {
      get { return array.Count; }
    }

    public void Add(PipeValue value, string key) {
      if (isArray) {
        array.Add(value.Value);
      } else {
        array.Add(new KeyValuePair<string, object>(key, value.Value));
      }
    }

    public PipeValue ToPipeValue() {
      if (isArray) {
        return array.ToArray();
      }
      ICollection<KeyValuePair<string, object>> dictionary = new Dictionary<string, object>();
      foreach (KeyValuePair<string, object> e in array) {
        dictionary.Add(e);
      }
      return new PipeValue(dictionary);
    }

    public static implicit operator PipeValue(PipeValueObjectBuilder collection) {
      return collection.ToPipeValue();
    }
  }
}
