using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  [DebuggerDisplay("{TextValue,nq}")]
  internal class PipeArgument {
    private ObjectPath objectPath;
    private int? length;

    public PipeArgument(string str, int startIndex, int endIndex) {
      this.TextValue = str;
      this.StartIndex = startIndex;
      this.EndIndex = endIndex;
      this.Value = ParseValue(str);
    }

    public int StartIndex { get; }
    public int EndIndex { get; }
    public string TextValue { get; }
    public PipeValue Value { get; }
    public PipeArgument Next { get; set; }

    public int Length {
      get {
        if (length == null) {
          length = 0;
          if (this.TextValue == "[") {
            PipeArgument t = this.Next;
            int i = 1;
            int count = 1;
            for (; t != null; t = t.Next, i++) {
              if (t.TextValue == "[") {
                count++;
              } else if (t.TextValue == "]" && --count == 0) {
                length = i;
                break;
              }
            }
          }
        }
        return length.Value;
      }
    }

    public ObjectPath ObjectPath {
      get {
        if (objectPath == null) {
          objectPath = new ObjectPath(this.TextValue);
        }
        return objectPath;
      }
    }

    private static PipeValue ParseValue(string str) {
      switch (str) {
        case "true":
          return true;
        case "false":
          return false;
        case "undefined":
          return PipeValue.Undefined;
        case "null":
          return PipeValue.Null;
        case "0":
          return 0;
      }
      PipeValue number = +new PipeValue(str);
      if (!Double.NaN.Equals(number.Value)) {
        return number;
      }
      return str;
    }
  }
}
