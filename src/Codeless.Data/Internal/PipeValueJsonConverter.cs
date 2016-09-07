using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class PipeValueJsonConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
      return objectType == typeof(PipeValue);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
      throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      WriteJson(writer, new PipeValue(value), serializer);
    }

    private void WriteJson(JsonWriter writer, PipeValue value, JsonSerializer serializer) {
      switch (value.Type) {
        case PipeValueType.Undefined:
          break;
        case PipeValueType.String:
          writer.WriteValue((string)value.Value);
          break;
        case PipeValueType.Number:
          switch (Type.GetTypeCode(value.Value.GetType())) {
            case TypeCode.Byte:
              writer.WriteValue((byte)value.Value);
              break;
            case TypeCode.Char:
              writer.WriteValue((char)value.Value);
              break;
            case TypeCode.Decimal:
              writer.WriteValue((decimal)value.Value);
              break;
            case TypeCode.Double:
              writer.WriteValue((double)value.Value);
              break;
            case TypeCode.Int16:
              writer.WriteValue((short)value.Value);
              break;
            case TypeCode.Int32:
              writer.WriteValue((int)value.Value);
              break;
            case TypeCode.Int64:
              writer.WriteValue((long)value.Value);
              break;
            case TypeCode.SByte:
              writer.WriteValue((sbyte)value.Value);
              break;
            case TypeCode.Single:
              writer.WriteValue((float)value.Value);
              break;
            case TypeCode.UInt16:
              writer.WriteValue((ushort)value.Value);
              break;
            case TypeCode.UInt32:
              writer.WriteValue((uint)value.Value);
              break;
            case TypeCode.UInt64:
              writer.WriteValue((ulong)value.Value);
              break;
          }
          break;
        case PipeValueType.Boolean:
          writer.WriteValue((bool)value.Value);
          break;
        case PipeValueType.Object:
          if (!value.IsEvallable) {
            writer.WriteNull();
          } else if (value.IsArray) {
            writer.WriteStartArray();
            foreach (object item in (IEnumerable)value.Value) {
              WriteJson(writer, new PipeValue(item), serializer);
            }
            writer.WriteEndArray();
          } else {
            writer.WriteStartObject();
            PipeValuePropertyEnumerator enumerator = value.GetEnumerator();
            while (enumerator.MoveNext()) {
              if (enumerator.CurrentValue != PipeValue.Undefined) {
                writer.WritePropertyName(enumerator.CurrentKey);
                WriteJson(writer, enumerator.CurrentValue, serializer);
              }
            }
            writer.WriteEndObject();
          }
          break;
      }
    }
  }
}
