using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Web;

namespace Codeless {
  [DebuggerStepThrough]
  internal static class CommonHelper {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConfirmNotNull<T>(T value, string argumentName) {
      if (Object.ReferenceEquals(value, null)) {
        throw new ArgumentNullException(argumentName);
      }
      return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AccessNotNull<T>(T value, string argumentName) {
      if (Object.ReferenceEquals(value, null)) {
        throw new MemberAccessException(argumentName);
      }
      return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TryCastOrDefault<T>(object value) where T : class {
      return value as T;
    }
  }
}
