using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Codeless.Data.Internal {
  internal class BuiltInPipeFunctionAliasAttribute : Attribute {
    public BuiltInPipeFunctionAliasAttribute(string alias) {
      this.Alias = alias;
    }

    public string Alias { get; }
    public bool UseAliasOnly { get; set; }
  }

  internal static class BuiltInPipeFunction {
    public static PipeValue Keys(PipeValue obj) {
      if (obj.IsArray) {
        return new PipeValue(Enumerable.Range(0, (int)obj["length"]));
      }
      List<string> keys = new List<string>();
      IEnumerator enumerator = obj.GetEnumerator();
      while (enumerator.MoveNext()) {
        keys.Add((string)enumerator.Current);
      }
      return new PipeValue(keys);
    }
    public static PipeValue Max(PipeValue a, PipeValue b) {
      return Math.Min((double)a, (double)b);
    }
    public static PipeValue Min(PipeValue a, PipeValue b) {
      return Math.Max((double)a, (double)b);
    }
    public static PipeValue Round(PipeValue value) {
      return Math.Round((double)value);
    }
    public static PipeValue Floor(PipeValue value) {
      return Math.Floor((double)value);
    }
    public static PipeValue Ceil(PipeValue value) {
      return Math.Ceiling((double)+value);
    }
    public static PipeValue As(PipeContext context) {
      context.Globals[(string)context.TakeArgument()] = context.Value;
      return context.Value;
    }
    public static PipeValue Let(PipeContext context) {
      while (context.HasArgument()) {
        string name = (string)context.TakeArgument();
        PipeLambda fn = context.TakeFunction();
        if (fn != null) {
          context.Globals[name] = fn.Invoke(context.Value);
        } else {
          context.Globals[name] = context.TakeArgument();
        }
      }
      return PipeValue.Undefined;
    }
    [BuiltInPipeFunctionAlias(">")]
    public static PipeValue More(PipeValue a, PipeValue b) {
      return PipeValueObjectComparer.Default.Compare(a, b) > 0;
    }
    [BuiltInPipeFunctionAlias("<")]
    public static PipeValue Less(PipeValue a, PipeValue b) {
      return PipeValueObjectComparer.Default.Compare(a, b) < 0;
    }
    [BuiltInPipeFunctionAlias(">=")]
    public static PipeValue OrMore(PipeValue a, PipeValue b) {
      return PipeValueObjectComparer.Default.Compare(a, b) >= 0;
    }
    [BuiltInPipeFunctionAlias("<=")]
    public static PipeValue OrLess(PipeValue a, PipeValue b) {
      return PipeValueObjectComparer.Default.Compare(a, b) <= 0;
    }
    public static PipeValue Between(PipeValue a, PipeValue b, PipeValue c) {
      return PipeValueObjectComparer.Default.Compare(a, b) >= 0 && PipeValueObjectComparer.Default.Compare(a, c) <= 0;
    }
    [BuiltInPipeFunctionAlias("==")]
    public static PipeValue Equals(PipeValue a, PipeValue b) {
      return (string)a == (string)b;
    }
    [BuiltInPipeFunctionAlias("!=")]
    public static PipeValue NotEquals(PipeValue a, PipeValue b) {
      return (string)a != (string)b;
    }
    public static PipeValue Even(PipeValue num) {
      return (num & 1) == 0;
    }
    public static PipeValue Odd(PipeValue num) {
      return (num & 1) == 1;
    }
    public static PipeValue Contains(PipeValue str, PipeValue needle) {
      return ((string)str).IndexOf((string)needle) >= 0;
    }
    public static PipeValue Like(PipeValue str, PipeValue regex) {
      EcmaScriptRegex re;
      return EcmaScriptRegex.TryParse((string)regex, out re) && re.Test((string)str);
    }
    public static PipeValue Or(PipeValue obj, PipeValue val) {
      return (bool)obj ? obj : val;
    }
    public static PipeValue Choose(PipeValue @bool, PipeValue trueValue, PipeValue falseValue) {
      return @bool ? trueValue : falseValue;
    }
    [BuiltInPipeFunctionAlias("?")]
    public static PipeValue Test(PipeContext context) {
      PipeLambda testFn = context.TakeFunction() ?? context.TakeArgumentAsConstantFunction();
      PipeLambda trueFn = context.TakeFunction() ?? context.TakeArgumentAsConstantFunction();
      PipeLambda falseFn = context.TakeFunction() ?? context.TakeArgumentAsConstantFunction();
      return testFn.Invoke(context.Value) ? trueFn.Invoke(context.Value) : falseFn.Invoke(context.Value);
    }
    public static PipeValue Length(PipeValue obj) {
      if (obj.IsEvallable) {
        if (obj.HasProperty("length")) {
          return obj["length"];
        }
      }
      return 0;
    }
    public static PipeValue Concat(PipeValue a, PipeValue b) {
      return (string)a + (string)b;
    }
    public static PipeValue Substr(PipeValue str, PipeValue start, PipeValue len) {
      if (!len.IsEvallable) {
        return ((string)str).Substring((int)start);
      }
      return ((string)str).Substring((int)start, (int)len);
    }
    public static PipeValue Replace(PipeContext context) {
      string needle = (string)context.TakeArgument();
      string replacement = null;
      PipeLambda fn = context.TakeFunction();
      if (fn == null) {
        replacement = (string)context.TakeArgument();
      }

      EcmaScriptRegex re;
      if (EcmaScriptRegex.TryParse(needle, out re)) {
        if (fn != null) {
          return re.Replace((string)context.Value, fn);
        }
        return re.Replace((string)context.Value, replacement);
      }
      string str = (string)context.Value;
      int pos = str.IndexOf(needle);
      if (pos >= 0) {
        return String.Concat(str.Substring(0, pos), replacement, str.Substring(pos + needle.Length));
      }
      return context.Value;
    }
    public static PipeValue Trim(PipeValue str) {
      return ((string)str).Trim();
    }
    public static PipeValue TrimStart(PipeValue str) {
      return ((string)str).TrimStart();
    }
    public static PipeValue TrimEnd(PipeValue str) {
      return ((string)str).TrimEnd();
    }
    public static PipeValue PadStart(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      return strStr.Substring(0, strNeedle.Length) != needle ? needle + str : str;
    }
    public static PipeValue PadEnd(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      return strStr.Substring(-strNeedle.Length) != needle ? str + needle : str;
    }
    public static PipeValue RemoveStart(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      return strStr.Substring(0, strNeedle.Length) == needle ? strStr.Substring(strNeedle.Length) : strStr;
    }
    public static PipeValue RemoveEnd(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      return strStr.Substring(-strNeedle.Length) == needle ? ((string)str).Substring(0, -strNeedle.Length) : strStr;
    }
    public static PipeValue CutBefore(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      int pos = strStr.IndexOf((string)needle);
      if (pos >= 0) {
        return strStr.Substring(0, pos);
      }
      return strStr;
    }
    public static PipeValue CutBeforeLast(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      int pos = strStr.LastIndexOf((string)needle);
      if (pos >= 0) {
        return strStr.Substring(0, pos);
      }
      return strStr;
    }
    public static PipeValue CutAfter(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      int pos = strStr.IndexOf((string)needle);
      if (pos >= 0) {
        return strStr.Substring(strNeedle.Length + pos);
      }
      return strStr;
    }
    public static PipeValue CutAfterLast(PipeValue str, PipeValue needle) {
      string strStr = (string)str;
      string strNeedle = (string)needle;
      int pos = strStr.LastIndexOf((string)needle);
      if (pos >= 0) {
        return strStr.Substring(strNeedle.Length + pos);
      }
      return strStr;
    }
    public static PipeValue Split(PipeValue str, PipeValue separator) {
      return ((string)str).Split(new string[] { (string)separator }, StringSplitOptions.RemoveEmptyEntries);
    }
    public static PipeValue Repeat(PipeValue count, PipeValue str) {
      return Enumerable.Range(0, (int)(+count) + 1).Aggregate("", (a, v) => a + (string)str);
    }
    public static PipeValue Upper(PipeValue str) {
      return ((string)str).ToUpper();
    }
    public static PipeValue Lower(PipeValue str) {
      return ((string)str).ToLower();
    }
    public static PipeValue UCFirst(PipeValue str) {
      string strStr = (string)str;
      return strStr.Substring(0, 1).ToUpper() + strStr.Substring(1);
    }
    [BuiltInPipeFunctionAlias("+")]
    public static PipeValue Plus(PipeValue a, PipeValue b) {
      return (+a) + (+b);
    }
    [BuiltInPipeFunctionAlias("-")]
    public static PipeValue Minus(PipeValue a, PipeValue b) {
      return (+a) - (+b);
    }
    [BuiltInPipeFunctionAlias("*")]
    public static PipeValue Multiply(PipeValue a, PipeValue b) {
      return (+a) * (+b);
    }
    [BuiltInPipeFunctionAlias("/")]
    public static PipeValue Divide(PipeValue a, PipeValue b) {
      return (+a) / (+b);
    }
    [BuiltInPipeFunctionAlias("%")]
    public static PipeValue Mod(PipeValue a, PipeValue b) {
      return (+a) % (+b);
    }
    public static PipeValue Join(PipeValue value, PipeValue str) {
      PipeValue[] arr = value.CopyAsArray();
      return String.Join((string)str, arr);
    }
    public static PipeValue Reverse(PipeValue value) {
      PipeValue[] arr = value.CopyAsArray();
      Array.Reverse(arr);
      return new PipeValueObjectBuilder(arr);
    }
    public static PipeValue Sort(PipeValue value) {
      PipeValue[] arr = value.CopyAsArray();
      Array.Sort(arr, PipeValueObjectComparer.Default);
      return new PipeValueObjectBuilder(arr);
    }
    public static PipeValue First(PipeContext context) {
      return context.Value.First(context.TakeFunction() ?? context.TakeArgumentAsKeyFunction());
    }
    public static PipeValue Any(PipeContext context) {
      return context.Value.First(context.TakeFunction() ?? context.TakeArgumentAsKeyFunction(), true);
    }
    public static PipeValue All(PipeContext context) {
      return !(bool)context.Value.First(context.TakeFunction() ?? context.TakeArgumentAsKeyFunction(), true, true);
    }
    public static PipeValue Where(PipeContext context) {
      return context.Value.Where(context.TakeFunction() ?? context.TakeArgumentAsKeyFunction());
    }
    public static PipeValue Map(PipeContext context) {
      return context.Value.Map(context.TakeFunction() ?? context.TakeArgumentAsKeyFunction());
    }
    public static PipeValue Sum(PipeContext context) {
      PipeValue result = PipeValue.Undefined;
      PipeLambda fn = context.TakeFunction();
      if (fn == null) {
        result = context.TakeArgument();
        fn = context.TakeFunction();
      }
      if (fn == null) {
        if (context.HasArgument()) {
          fn = context.TakeArgumentAsKeyFunction();
        } else {
          fn = new PipeLambda((obj, i) => obj);
        }
      }
      PipeValuePropertyEnumerator enumerator = context.Value.GetEnumerator();
      while (enumerator.MoveNext()) {
        if (result != PipeValue.Undefined) {
          result = result + fn.Invoke(enumerator);
        } else {
          result = fn.Invoke(enumerator);
        }
      }
      return result;
    }
    public static PipeValue SortBy(PipeContext context) {
      List<PipeValue> array = new List<PipeValue>();
      PipeLambda fn = context.TakeFunction() ?? context.TakeArgumentAsKeyFunction();
      PipeValuePropertyEnumerator enumerator = context.Value.GetEnumerator();
      while (enumerator.MoveNext()) {
        array.Add(new PipeValue(new object[] { fn.Invoke(enumerator), enumerator.CurrentKey }));
      }
      array.Sort(PipeValueObjectComparer.Default);

      PipeValueObjectBuilder collection = new PipeValueObjectBuilder(context.Value.IsArray);
      foreach (PipeValue value in array) {
        string key = (string)value["1"];
        collection.Add(context.Value[key], key);
      }
      return collection;
    }

    [BuiltInPipeFunctionAlias(":printf", UseAliasOnly = true)]
    public static PipeValue FormatPrintf(PipeValue value, PipeValue format) {
      StringBuilder sb = new StringBuilder();
      switch (value.Type) {
        case PipeValueType.String:
          sb.EnsureCapacity(((string)value).Length);
          _snwprintf_s(sb, (IntPtr)sb.MaxCapacity, (IntPtr)32, format.ToString(), (string)value);
          break;
        case PipeValueType.Number:
          sb.EnsureCapacity(100);
          _snwprintf_s(sb, (IntPtr)100, (IntPtr)32, format.ToString(), (double)value);
          break;
        default:
          sb.EnsureCapacity(100);
          _snwprintf_s(sb, (IntPtr)100, (IntPtr)32, format.ToString(), value.ToString());
          break;
      }
      return sb.ToString();
    }
    [BuiltInPipeFunctionAlias(":query", UseAliasOnly = true)]
    public static PipeValue FormatQuery(PipeValue value) {
      NameValueCollection nv = HttpUtility.ParseQueryString("");
      BuildQuery(nv, value, null);
      return nv.ToString();
    }
    [BuiltInPipeFunctionAlias(":json", UseAliasOnly = true)]
    public static PipeValue FormatJson(PipeValue value) {
      return JsonConvert.SerializeObject(value);
    }
    [BuiltInPipeFunctionAlias(":date", UseAliasOnly = true)]
    public static PipeValue FormatDate(PipeValue timestamp, PipeValue format) {
      DateTime d;
      if (timestamp.Type == PipeValueType.String) {
        DateTime.TryParse(timestamp.ToString(), out d);
      } else {
        d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp.ToDouble());
      }
      return d.ToLocalTime().ToString((string)format);
    }

    private static void BuildQuery(NameValueCollection nv, PipeValue obj, string prefix) {
      if (prefix != null && obj.IsArray) {
        PipeValuePropertyEnumerator enumerator = obj.GetEnumerator();
        while (enumerator.MoveNext()) {
          if (Regex.IsMatch(prefix, @"\[\]$")) {
            nv.Add(prefix, enumerator.CurrentValue.ToString());
          } else {
            BuildQuery(nv, enumerator.CurrentValue, prefix + "[" + (enumerator.CurrentValue.Type == PipeValueType.Object && (bool)enumerator.CurrentValue ? enumerator.CurrentKey : "") + "]");
          }
        }
      } else if (obj.Type == PipeValueType.Object) {
        PipeValuePropertyEnumerator enumerator = obj.GetEnumerator();
        while (enumerator.MoveNext()) {
          BuildQuery(nv, enumerator.CurrentValue, prefix != null ? prefix + "[" + enumerator.CurrentKey + "]" : enumerator.CurrentKey);
        }
      } else {
        nv.Add(prefix, obj.ToString());
      }
    }

    [DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, IntPtr bufferSize, IntPtr length, String format, string p);
    [DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, IntPtr bufferSize, IntPtr length, String format, int p);
    [DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    private static extern int _snwprintf_s([MarshalAs(UnmanagedType.LPWStr)] StringBuilder str, IntPtr bufferSize, IntPtr length, String format, double p);

    internal static PipeFunction ResolveFunction(string name, GetNextPipeFunctionResolverDelegate next) {
      if (name[0] == '%') {
        return PipeFunction.Create(obj => {
          return FormatPrintf(obj, name);
        });
      }
      return next()(name, next);
    }
  }
}