using Codeless.Data.Internal;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using static System.Type;

namespace Codeless.Data {
  public enum PipeValueType {
    Undefined,
    Object,
    Number,
    String,
    Boolean
  }

  internal enum PipeNumberType {
    Invalid,
    Int32,
    Int64,
    Double
  }

  public class PipeValueIndexingException : Exception {
    internal PipeValueIndexingException(string message)
      : base(message) { }
  }

  /// <summary>
  /// Represents a dynamic value in pipe executions to mimic behaviors to values in ECMAScript.
  /// </summary>
  [JsonConverter(typeof(PipeValueJsonConverter))]
  public struct PipeValue : IEquatable<PipeValue>, IComparable<PipeValue>, IEnumerable<string> {
    private static readonly object undefined = new object();

    /// <summary>
    /// Represents a null value. It is equivalent to *null* in C# and similar to *null* in ECMAScript.
    /// </summary>
    public static readonly PipeValue Null = new PipeValue(null);
    /// <summary>
    /// Represents an undefined value. It is similar to *undefined* in ECMAScript which could be returned when accessing an undefined property.
    /// </summary>
    public static readonly PipeValue Undefined = new PipeValue(undefined);

    private readonly object value;

    /// <summary>
    /// Instantiate an instance of the <see cref="PipeValue"/> class with the specified native value.
    /// </summary>
    /// <param name="value">A native object.</param>
    public PipeValue(object value) {
      if (value is PipeValue) {
        this.value = ((PipeValue)value).value;
      } else {
        this.value = value;
      }
    }

    /// <summary>
    /// Gets value of the specified property from the object.
    /// </summary>
    /// <param name="index">Property name.</param>
    /// <returns>Value associated with the property name, -or- <see cref="Undefined"/> if property does not exist.</returns>
    public PipeValue this[string index] {
      get {
        if (!this.IsEvallable) {
          throw new PipeValueIndexingException("Cannot index to undefined or null object");
        }
        if (this.Type == PipeValueType.Object) {
          if (value is IDictionary) {
            try {
              return new PipeValue(((IDictionary)value)[index]);
            } catch (KeyNotFoundException) {
              return Undefined;
            }
          }
          if (value is ICollection) {
            int intValue;
            if (Int32.TryParse(index, out intValue) && intValue < ((ICollection)value).Count) {
              return new PipeValue(((IList)value)[intValue]);
            }
          }
          PropertyDescriptor pd = TypeDescriptor.GetProperties(value).Find(index, true);
          if (pd != null) {
            return new PipeValue(pd.GetValue(value));
          }
          return Undefined;
        }
        if (this.Type == PipeValueType.String) {
          if (index == "length") {
            return ((string)value).Length;
          }
          int intValue;
          if (Int32.TryParse(index, out intValue) && intValue < ((string)value).Length) {
            return new String(((string)value)[intValue], 1);
          }
        }
        return Undefined;
      }
    }

    /// <summary>
    /// Gets the type of value represented by the <see cref="PipeValue"/> instance.
    /// </summary>
    public PipeValueType Type {
      get {
        if (value == undefined) {
          return PipeValueType.Undefined;
        }
        if (value == null) {
          return PipeValueType.Object;
        }
        switch (GetTypeCode(value.GetType())) {
          case TypeCode.Boolean:
            return PipeValueType.Boolean;
          case TypeCode.Byte:
          case TypeCode.Char:
          case TypeCode.Decimal:
          case TypeCode.Double:
          case TypeCode.Int16:
          case TypeCode.Int32:
          case TypeCode.Int64:
          case TypeCode.SByte:
          case TypeCode.Single:
          case TypeCode.UInt16:
          case TypeCode.UInt32:
          case TypeCode.UInt64:
            return PipeValueType.Number;
          case TypeCode.String:
            return PipeValueType.String;
          default:
            return PipeValueType.Object;
        }
      }
    }

    public bool IsEvallable {
      get { return value != null && value != undefined; }
    }

    public bool IsArray {
      get { return this.Type == PipeValueType.Object && value is IEnumerable && !(value is IDictionary); }
    }

    internal object Value {
      get { return value; }
    }

    internal PipeNumberType NumberType {
      get {
        switch (GetTypeCode(value.GetType())) {
          case TypeCode.SByte:
          case TypeCode.Byte:
          case TypeCode.Char:
          case TypeCode.Int16:
          case TypeCode.UInt16:
          case TypeCode.Int32:
            return PipeNumberType.Int32;
          case TypeCode.UInt32:
          case TypeCode.Int64:
            return PipeNumberType.Int64;
          case TypeCode.UInt64:
          case TypeCode.Single:
          case TypeCode.Double:
            return PipeNumberType.Double;
        }
        return PipeNumberType.Invalid;
      }
    }

    internal PipeNumberType GetNumberCoercion(PipeValue other) {
      PipeNumberType typeX = this.NumberType;
      PipeNumberType typeY = other.NumberType;
      if (typeX == PipeNumberType.Invalid || typeY == PipeNumberType.Invalid) {
        return PipeNumberType.Invalid;
      }
      if (typeX == PipeNumberType.Double || typeY == PipeNumberType.Double) {
        return PipeNumberType.Double;
      }
      if (typeX == PipeNumberType.Int64 || typeY == PipeNumberType.Int64) {
        return PipeNumberType.Int64;
      }
      return PipeNumberType.Int32;
    }

    public bool HasProperty(string name) {
      if (this.Type == PipeValueType.Object) {
        if (value is IDictionary) {
          try {
            object v = new PipeValue(((IDictionary)value)[name]);
            return true;
          } catch (KeyNotFoundException) {
            return false;
          }
        }
        if (value is ICollection) {
          int intValue;
          return (Int32.TryParse(name, out intValue) && intValue < ((ICollection)value).Count);
        }
        return TypeDescriptor.GetProperties(value).Find(name, true) != null;
      }
      if (this.Type == PipeValueType.String) {
        if (name == "length") {
          return true;
        }
        int intValue;
        return (Int32.TryParse(name, out intValue) && intValue < ((string)value).Length);
      }
      return false;
    }

    public PipeValuePropertyEnumerator GetEnumerator() {
      switch (this.Type) {
        case PipeValueType.Object:
          if (value is IDictionary) {
            return new DictionaryPropertyEnumerator((IDictionary)value);
          }
          if (value is IEnumerable) {
            return new EnumerablePropertyEnumerator((IEnumerable)value);
          }
          return new ReflectedPropertyEnumerator(value);
        case PipeValueType.String:
          return new StringPropertyEnumerator((string)value);
        default:
          return EmptyPropertyEnumerator.Default;
      }
    }

    public bool Equals(PipeValue other) {
      switch (GetNumberCoercion(other)) {
        case PipeNumberType.Double:
          return ToDouble().Equals(other.ToDouble());
        case PipeNumberType.Int64:
          return ToInt64().Equals(other.ToInt64());
        case PipeNumberType.Int32:
          return ToInt32().Equals(other.ToInt32());
        default:
          return ToString().Equals(other.ToString());
      }
    }

    public int CompareTo(PipeValue other) {
      switch (GetNumberCoercion(other)) {
        case PipeNumberType.Double:
          return ToDouble().CompareTo(other.ToDouble());
        case PipeNumberType.Int64:
          return ToInt64().CompareTo(other.ToInt64());
        case PipeNumberType.Int32:
          return ToInt32().CompareTo(other.ToInt32());
        default:
          return ToString().CompareTo(other.ToString());
      }
    }

    public override string ToString() {
      switch (this.Type) {
        case PipeValueType.Boolean:
          return (bool)value ? "true" : "false";
        case PipeValueType.Undefined:
          return "undefined";
        case PipeValueType.Object:
          return value == null ? "null" : "[object Object]";
        default:
          return value.ToString();
      }
    }

    public bool ToBoolean() {
      if (!IsEvallable || "".Equals(value) || (0).Equals(value) || false.Equals(value)) {
        return false;
      }
      return true;
    }

    public double ToDouble() {
      PipeValue value = NumberType == PipeNumberType.Invalid ? +this : this;
      return Convert.ToDouble(value.value);
    }

    public int ToInt32() {
      PipeValue value = NumberType == PipeNumberType.Invalid ? +this : this;
      return Convert.ToInt32(value.value);
    }

    public long ToInt64() {
      PipeValue value = NumberType == PipeNumberType.Invalid ? +this : this;
      return Convert.ToInt64(value.value);
    }

    #region Object Operations
    public static explicit operator string(PipeValue value) {
      return value.ToString();
    }

    public static explicit operator bool(PipeValue value) {
      return value.ToBoolean();
    }

    public static explicit operator double(PipeValue value) {
      return value.ToDouble();
    }

    public static explicit operator int(PipeValue value) {
      return value.ToInt32();
    }

    public static explicit operator long(PipeValue value) {
      return value.ToInt64();
    }

    public static implicit operator PipeValue(string value) {
      return new PipeValue(value);
    }

    public static implicit operator PipeValue(bool value) {
      return new PipeValue(value);
    }

    public static implicit operator PipeValue(double value) {
      return new PipeValue(value);
    }

    public static implicit operator PipeValue(int value) {
      return new PipeValue(value);
    }

    public static implicit operator PipeValue(long value) {
      return new PipeValue(value);
    }

    public static implicit operator PipeValue(Array value) {
      return new PipeValue(value);
    }

    public static bool operator ==(PipeValue x, PipeValue y) {
      return x.Equals(y);
    }

    public static bool operator !=(PipeValue x, PipeValue y) {
      return !x.Equals(y);
    }

    public static bool operator <(PipeValue x, PipeValue y) {
      return x.CompareTo(y) < 0;
    }

    public static bool operator >(PipeValue x, PipeValue y) {
      return x.CompareTo(y) > 0;
    }

    public static bool operator <=(PipeValue x, PipeValue y) {
      return x.CompareTo(y) <= 0;
    }

    public static bool operator >=(PipeValue x, PipeValue y) {
      return x.CompareTo(y) >= 0;
    }

    public static bool operator true(PipeValue x) {
      return x.ToBoolean();
    }

    public static bool operator false(PipeValue x) {
      return !x.ToBoolean();
    }

    public static PipeValue operator +(PipeValue x) {
      int intValue;
      long longValue;
      double doubleValue;
      switch (x.Type) {
        case PipeValueType.Object:
          return new PipeValue(x != Null ? 1 : 0);
        case PipeValueType.String:
          if (Int32.TryParse((string)x.value, out intValue)) {
            return new PipeValue(intValue);
          }
          if (Int64.TryParse((string)x.value, out longValue)) {
            return new PipeValue(longValue);
          }
          if (Double.TryParse((string)x.value, out doubleValue)) {
            return new PipeValue(doubleValue);
          }
          return new PipeValue(Double.NaN);
        case PipeValueType.Number:
          return x;
        case PipeValueType.Boolean:
          return new PipeValue((bool)x.value ? 1 : 0);
      }
      return new PipeValue(Double.NaN);
    }

    public static PipeValue operator -(PipeValue x) {
      x = +x;
      switch (x.NumberType) {
        case PipeNumberType.Int32:
          return new PipeValue(-(int)x.value);
        case PipeNumberType.Int64:
          return new PipeValue(-(long)x.value);
        case PipeNumberType.Double:
          return new PipeValue(-(double)x.value);
      }
      return new PipeValue(Double.NaN);
    }

    public static PipeValue operator +(PipeValue x, PipeValue y) {
      switch (x.GetNumberCoercion(y)) {
        case PipeNumberType.Double:
          return new PipeValue(x.ToDouble() + y.ToDouble());
        case PipeNumberType.Int64:
          return new PipeValue(x.ToInt64() + y.ToInt64());
        case PipeNumberType.Int32:
          return new PipeValue(x.ToInt32() + y.ToInt32());
        default:
          return new PipeValue(x.ToString() + y.ToString());
      }
    }

    public static PipeValue operator -(PipeValue x, PipeValue y) {
      return (+x) + (-y);
    }

    public static PipeValue operator *(PipeValue x, PipeValue y) {
      x = +x;
      y = +y;
      switch (x.GetNumberCoercion(y)) {
        case PipeNumberType.Double:
          return new PipeValue(x.ToDouble() * y.ToDouble());
        case PipeNumberType.Int64:
          return new PipeValue(x.ToInt64() * y.ToInt64());
        case PipeNumberType.Int32:
          return new PipeValue(x.ToInt32() * y.ToInt32());
      }
      throw new InvalidOperationException();
    }

    public static PipeValue operator /(PipeValue x, PipeValue y) {
      x = +x;
      y = +y;
      return new PipeValue(x.ToDouble() / y.ToDouble());
    }

    public static PipeValue operator %(PipeValue x, PipeValue y) {
      x = +x;
      y = +y;
      switch (x.GetNumberCoercion(y)) {
        case PipeNumberType.Double:
          return new PipeValue(x.ToDouble() % y.ToDouble());
        case PipeNumberType.Int64:
          return new PipeValue(x.ToInt64() % y.ToInt64());
        case PipeNumberType.Int32:
          return new PipeValue(x.ToInt32() % y.ToInt32());
      }
      throw new InvalidOperationException();
    }

    public static PipeValue operator &(PipeValue x, long y) {
      return new PipeValue((+x).ToInt64() & y);
    }

    public static PipeValue operator &(PipeValue x, PipeValue y) {
      return new PipeValue((+x).ToInt64() & (+y).ToInt64());
    }

    public static PipeValue operator |(PipeValue x, long y) {
      return new PipeValue((+x).ToInt64() | y);
    }

    public static PipeValue operator |(PipeValue x, PipeValue y) {
      return new PipeValue((+x).ToInt64() | (+y).ToInt64());
    }

    public static PipeValue operator <<(PipeValue x, int y) {
      return new PipeValue(unchecked((+x).ToInt64() << y));
    }

    public static PipeValue operator >>(PipeValue x, int y) {
      return new PipeValue((+x).ToInt64() >> y);
    }

    public override bool Equals(object obj) {
      if (obj is PipeValue) {
        return Equals((PipeValue)obj);
      }
      return base.Equals(obj);
    }

    public override int GetHashCode() {
      if (value == null) {
        return 0;
      }
      return value.GetHashCode();
    }
    #endregion

    #region IEnumerable
    IEnumerator<string> IEnumerable<string>.GetEnumerator() {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
    #endregion
  }
}
