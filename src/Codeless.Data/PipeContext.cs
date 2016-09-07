using Codeless.Data.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Codeless.Data {
  public class InvalidPipeFunctionException : Exception {
    public InvalidPipeFunctionException(string name)
      : base(String.Format("Invalid pipe function '{0}'", name)) { }
  }

  public class PipeExecutionException : WaterpipeException {
    internal PipeExecutionException(string message, Exception innerException, CallSite callsite)
      : base(message, innerException, callsite) { }
  }

  public class PipeContext {
    private readonly EvaluationContext context;
    private readonly Pipe pipe;
    private readonly int start;
    private readonly int end;
    private int i;

    internal PipeContext(EvaluationContext context, Pipe pipe) {
      CommonHelper.ConfirmNotNull(context, "context");
      CommonHelper.ConfirmNotNull(pipe, "pipe");
      this.context = context;
      this.pipe = pipe;
      this.end = pipe.Count - 1;
    }

    private PipeContext(PipeContext instance, int start, int end)
      : this(instance.context, instance.pipe) {
      this.start = start;
      this.end = end;
    }

    public PipeValue Value { get; internal set; }

    internal EvaluationContext EvaluationContext {
      get { return context; }
    }

    public PipeGlobal Globals {
      get { return context.Globals; }
    }

    public bool HasArgument() {
      return HasArgument(false);
    }

    public bool HasArgument(bool reqObjectPath) {
      return i <= end && (!reqObjectPath || pipe[i].TextValue[0] == '$');
    }

    public PipeValue TakeArgument() {
      if (i <= end) {
        if (pipe[i].TextValue[0] == '$') {
          return pipe[i++].ObjectPath.Evaluate(context);
        }
        return pipe[i++].Value;
      }
      return PipeValue.Undefined;
    }

    public PipeValue[] TakeArguments(int count) {
      PipeValue[] arr = new PipeValue[count];
      for (int i = 0; i < count; i++) {
        arr[i] = TakeArgument();
      }
      return arr;
    }

    public PipeLambda TakeFunction() {
      int start = i;
      if (start <= end && pipe[start].Length > 0) {
        int len = pipe[start].Length;
        i += len + 1;
        return new PipeLambdaHostClass(this, start + 1, start + len - 1);
      }
      return null;
    }

    public PipeLambda TakeArgumentAsConstantFunction() {
      PipeValue value = TakeArgument();
      return new PipeLambda((x, y) => value);
    }

    public PipeLambda TakeArgumentAsKeyFunction() {
      string value = (string)TakeArgument();
      return new PipeLambda((x, y) => x[value]);
    }

    internal PipeValue Evaluate() {
      bool isValid;
      PipeValue input = ObjectPath.Empty.Evaluate(context);
      PipeValue value = (pipe.Count > start ? pipe[start].ObjectPath : ObjectPath.Empty).Evaluate(context, true, out isValid);
      List<PipeValue> returnArray = new List<PipeValue>();

      i = start + (isValid ? 1 : 0);
      while (i <= end) {
        int startpos = i;
        string name = pipe[i++].TextValue;
        if (name == "|") {
          returnArray.Add(value);
          value = HasArgument(true) ? TakeArgument() : input;
        } else if (name == "&&" || name == "||") {
          if ((bool)value ^ (name == "&&")) {
            break;
          }
          PipeLambda fn = TakeFunction();
          if (fn != null) {
            value = fn.Invoke(input);
          } else if (HasArgument(true)) {
            value = TakeArgument();
          } else {
            value = input;
          }
        } else {
          this.Value = value;
          try {
            PipeFunction fn;
            try {
              fn = context.ResolveFunction(name);
            } catch (InvalidPipeFunctionException) {
              if (startpos == start) {
                value = PipeValue.Undefined;
                continue;
              }
              throw;
            }
            value = fn.Invoke(this);
          } catch (Exception ex) {
            if (ex is TargetInvocationException) {
              ex = ex.InnerException;
            }
            PipeExecutionException wrappedException = new PipeExecutionException(ex.Message, ex, new WaterpipeException.CallSite {
              InputString = context.InputString,
              ConstructStart = pipe.StartIndex,
              ConstructEnd = pipe.EndIndex,
              HighlightStart = pipe[startpos].StartIndex,
              HighlightEnd = pipe[i - 1].EndIndex
            });
            context.AddException(wrappedException);
          }
        }
      }
      if (returnArray.Count > 0) {
        returnArray.Add(value);
        return new PipeValueObjectBuilder(returnArray, true);
      }
      return value;
    }

    private class PipeLambdaHostClass : PipeLambda {
      private readonly PipeContext pipe;

      public PipeLambdaHostClass(PipeContext evaluator, int start, int end) {
        this.pipe = new PipeContext(evaluator, start, end);
      }

      protected override PipeValue InvokeInternal(PipeValue obj, PipeValue index) {
        try {
          Hashtable ht = new Hashtable { { index.ToString(), obj } };
          IEnumerator enumerator = new PipeValue(ht).GetEnumerator();
          enumerator.MoveNext();
          pipe.context.PushObjectStack(enumerator);
          return pipe.Evaluate();
        } finally {
          pipe.context.PopObjectStack();
        }
      }
    }
  }
}
