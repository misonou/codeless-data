using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  [DebuggerDisplay("{Value,nq}")]
  internal class ObjectPath : Collection<ObjectPath> {
    public static readonly ObjectPath Empty = new ObjectPath();
    private static readonly Regex re = new Regex(
      @"((?!^)\$)?([^$.()][^.]*)|\$\(([^)]+)\)");

    protected ObjectPath() { }

    public ObjectPath(string str) {
      CommonHelper.ConfirmNotNull(str, "str");
      for (Match m = re.Match(str); m.Success; m = m.NextMatch()) {
        Add(m.Groups[1].Success || m.Groups[3].Success ?
           new ObjectPath(m.Groups[3].Success ? m.Groups[3].Value : m.Groups[2].Value) :
           new ConstantObjectPath(m.Groups[2].Value));
      }
      this.Value = str;
    }

    public string Value { get; protected set; }

    public PipeValue Evaluate(EvaluationContext context) {
      bool isValid;
      return Evaluate(context, false, out isValid);
    }

    public PipeValue Evaluate(EvaluationContext context, bool checkValid, out bool isValid) {
      CommonHelper.ConfirmNotNull(context, "context");
      isValid = true;
      PipeValue value = context.CurrentValue;
      if (this.Count > 0) {
        ConstantObjectPath m = this[0] as ConstantObjectPath;
        if (m != null) {
          switch (m.Value) {
            case "#":
              return value.Value is PipeValuePropertyEnumerator ? ((PipeValuePropertyEnumerator)value.Value).CurrentKey : PipeValue.Undefined;
            case "##":
              return value.Value is PipeValuePropertyEnumerator ? ((PipeValuePropertyEnumerator)value.Value).CurrentIndex : PipeValue.Undefined;
            case "#count":
              return value.Value is PipeValuePropertyEnumerator ? ((PipeValuePropertyEnumerator)value.Value).Count : 0;
          }
        }
      }
      value = value.Value is PipeValuePropertyEnumerator ? ((PipeValuePropertyEnumerator)value.Value).CurrentValue : value;
      if (this.Count > 0) {
        ConstantObjectPath m = this[0] as ConstantObjectPath;
        if (m != null) {
          int intValue;
          bool hasPropertyName = value.IsEvallable && (value.Type != PipeValueType.Object ? value[m.Value] != PipeValue.Undefined : (value.HasProperty(m.Value) || (value.IsArray && Int32.TryParse(m.Value, out intValue))));
          if (checkValid && !hasPropertyName && !context.Globals.ContainsKey(m.Value)) {
            isValid = false;
            return value;
          }
          value = hasPropertyName ? value[m.Value] : context.Globals[m.Value];
        }
      }
      for (int i = 1, len = this.Count; i < len && value.IsEvallable; i++) {
        string name = this[i] is ConstantObjectPath ? ((ConstantObjectPath)this[i]).Value : (string)this[i].Evaluate(context);
        value = value[name];
      }
      return value;
    }

    private class ConstantObjectPath : ObjectPath {
      public ConstantObjectPath(string str) {
        this.Value = str;
      }
    }
  }
}
