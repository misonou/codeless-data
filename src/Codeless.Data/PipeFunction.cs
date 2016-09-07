using Codeless.Data.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Xml;

namespace Codeless.Data {
  /// <summary>
  /// An abstract class representing a pipe function that can be invoked with a given pipe context.
  /// </summary>
  public abstract class PipeFunction {
    /// <summary>
    /// Encapsulates a native method that takes arbitrary number of arguments or function arguments from pipe.
    /// </summary>
    /// <param name="context">Pipe context.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Variadic(PipeContext context);

    /// <summary>
    /// Encapsulates a native method that takes no arguments from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func0(PipeValue value);

    /// <summary>
    /// Encapsulates a native method that takes one argument from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <param name="arg1">First argument from pipe.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func1(PipeValue value, PipeValue arg1);

    /// <summary>
    /// Encapsulates a native method that takes two arguments from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <param name="arg1">First argument from pipe.</param>
    /// <param name="arg2">Second argument from pipe.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func2(PipeValue value, PipeValue arg1, PipeValue arg2);

    /// <summary>
    /// Encapsulates a native method that takes three arguments from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <param name="arg1">First argument from pipe.</param>
    /// <param name="arg2">Second argument from pipe.</param>
    /// <param name="arg3">Third argument from pipe.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func3(PipeValue value, PipeValue arg1, PipeValue arg2, PipeValue arg3);

    /// <summary>
    /// Encapsulates a native method that takes three arguments from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <param name="arg1">First argument from pipe.</param>
    /// <param name="arg2">Second argument from pipe.</param>
    /// <param name="arg3">Third argument from pipe.</param>
    /// <param name="arg4">Forth argument from pipe.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func4(PipeValue value, PipeValue arg1, PipeValue arg2, PipeValue arg3, PipeValue arg4);

    /// <summary>
    /// Encapsulates a native method that takes three arguments from pipe.
    /// </summary>
    /// <param name="value">Piped value from object path or previous pipe function.</param>
    /// <param name="arg1">First argument from pipe.</param>
    /// <param name="arg2">Second argument from pipe.</param>
    /// <param name="arg3">Third argument from pipe.</param>
    /// <param name="arg4">Forth argument from pipe.</param>
    /// <param name="arg5">Fifth argument from pipe.</param>
    /// <returns>Result of the pipe function.</returns>
    public delegate PipeValue Func5(PipeValue value, PipeValue arg1, PipeValue arg2, PipeValue arg3, PipeValue arg4, PipeValue arg5);

    /// <summary>
    /// Invokes the pipe function with the specified context.
    /// </summary>
    /// <param name="context">Pipe context.</param>
    /// <returns>Result of the pipe function.</returns>
    public abstract PipeValue Invoke(PipeContext context);

    /// <summary>
    /// Creates a pipe function that takes arbitrary number of arguments or function arguments.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Variadic"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Variadic fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new VariadicPipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes no arguments from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func0"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func0 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func0PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes one argument from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func1"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func1 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func1PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes two arguments from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func2"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func2 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func2PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes three arguments from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func3"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func3 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func3PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes four arguments from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func4"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func4 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func4PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function that takes five arguments from pipe.
    /// </summary>
    /// <param name="fn">A delegate that encapsulates a method that match the signature of <see cref="Func5"/>.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(Func5 fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      return new Func5PipeFunction(fn);
    }

    /// <summary>
    /// Creates a pipe function.
    /// </summary>
    /// <param name="fn">A <see cref="MethodInfo"/> instance representing a named or anonymous method.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(MethodInfo fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      ParameterInfo[] p = fn.GetParameters();
      if (fn.ReturnType == typeof(PipeValue)) {
        if (p.Length == 1 && p[0].ParameterType == typeof(PipeContext)) {
          return Create(CreateDelegate<Variadic>(fn));
        }
        if (p.All(v => v.ParameterType == typeof(PipeValue))) {
          switch (p.Length) {
            case 1:
              return Create(CreateDelegate<Func0>(fn));
            case 2:
              return Create(CreateDelegate<Func1>(fn));
            case 3:
              return Create(CreateDelegate<Func2>(fn));
            case 4:
              return Create(CreateDelegate<Func3>(fn));
            case 5:
              return Create(CreateDelegate<Func4>(fn));
            case 6:
              return Create(CreateDelegate<Func5>(fn));
          }
        }
      }
      throw new NotSupportedException(String.Format("Unsupported pipe function type {0}", fn));
    }

    /// <summary>
    /// Creates a pipe function that evaluates the specified template on the piped value.
    /// </summary>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    /// <returns>The constructed pipe function that can be invoked with a given pipe context.</returns>
    public static PipeFunction Create(string template) {
      CommonHelper.ConfirmNotNull(template, "template");
      return new EvaluatedPipeFunction(template);
    }

    private static T CreateDelegate<T>(MethodInfo fn) {
      return (T)(object)Delegate.CreateDelegate(typeof(T), fn);
    }

    #region Implemented classes
    private class EvaluatedPipeFunction : PipeFunction {
      private readonly TokenList tokens;

      public EvaluatedPipeFunction(string template) {
        this.tokens = TokenList.FromString(template);
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeExecutionException[] exceptions;
        object result = EvaluationContext.Evaluate(tokens, context.Value, context.EvaluationContext.Options, out exceptions);
        foreach (PipeExecutionException ex in exceptions) {
          context.EvaluationContext.AddException(ex);
        }
        if (context.EvaluationContext.Options.OutputXml) {
          context.EvaluationContext.CurrentXmlElement.AppendChild((XmlNode)result);
          return PipeValue.Undefined;
        }
        return result.ToString();
      }
    }

    private class VariadicPipeFunction : PipeFunction {
      private readonly Variadic fn;

      public VariadicPipeFunction(Variadic fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        return fn(context);
      }
    }

    private class Func0PipeFunction : PipeFunction {
      private readonly Func0 fn;

      public Func0PipeFunction(Func0 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        return fn(context.Value);
      }
    }

    private class Func1PipeFunction : PipeFunction {
      private readonly Func1 fn;

      public Func1PipeFunction(Func1 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeValue[] args = context.TakeArguments(1);
        return fn(context.Value, args[0]);
      }
    }

    private class Func2PipeFunction : PipeFunction {
      private readonly Func2 fn;

      public Func2PipeFunction(Func2 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeValue[] args = context.TakeArguments(2);
        return fn(context.Value, args[0], args[1]);
      }
    }

    private class Func3PipeFunction : PipeFunction {
      private readonly Func3 fn;

      public Func3PipeFunction(Func3 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeValue[] args = context.TakeArguments(3);
        return fn(context.Value, args[0], args[1], args[2]);
      }
    }

    private class Func4PipeFunction : PipeFunction {
      private readonly Func4 fn;

      public Func4PipeFunction(Func4 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeValue[] args = context.TakeArguments(4);
        return fn(context.Value, args[0], args[1], args[2], args[3]);
      }
    }

    private class Func5PipeFunction : PipeFunction {
      private readonly Func5 fn;

      public Func5PipeFunction(Func5 fn) {
        this.fn = fn;
      }

      public override PipeValue Invoke(PipeContext context) {
        PipeValue[] args = context.TakeArguments(5);
        return fn(context.Value, args[0], args[1], args[2], args[3], args[4]);
      }
    }
    #endregion
  }
}
