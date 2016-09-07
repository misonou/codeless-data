using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Data {
  public static class PipeValueExtension {
    public static PipeValue[] CopyAsArray(this PipeValue arr) {
      if (arr.IsArray) {
        return ((IEnumerable)arr.Value).OfType<object>().Select(v => new PipeValue(v)).ToArray();
      }
      return new[] { arr };
    }

    public static PipeValue Where(this PipeValue value, PipeLambda filter) {
      CommonHelper.ConfirmNotNull(filter, "filter");
      PipeValuePropertyEnumerator enumerator = value.GetEnumerator();
      PipeValueObjectBuilder collection = new PipeValueObjectBuilder(value.IsArray);
      while (enumerator.MoveNext()) {
        if ((bool)filter.Invoke(enumerator)) {
          collection.Add(enumerator.CurrentValue, enumerator.CurrentKey);
        }
      }
      return collection;
    }

    public static PipeValue Map(this PipeValue value, PipeLambda map) {
      CommonHelper.ConfirmNotNull(map, "map");
      PipeValuePropertyEnumerator enumerator = value.GetEnumerator();
      PipeValueObjectBuilder collection = new PipeValueObjectBuilder(value.IsArray);
      while (enumerator.MoveNext()) {
        collection.Add(map.Invoke(enumerator), enumerator.CurrentKey);
      }
      return collection;
    }

    public static PipeValue First(this PipeValue value, PipeLambda fn, bool returnBoolean = false, bool negate = false) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      PipeValuePropertyEnumerator enumerator = value.GetEnumerator();
      while (enumerator.MoveNext()) {
        if (negate ^ (bool)fn.Invoke(enumerator)) {
          return returnBoolean ? true : enumerator.CurrentValue;
        }
      }
      return returnBoolean ? false : PipeValue.Undefined;
    }
  }
}
