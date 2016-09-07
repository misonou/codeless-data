using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  public delegate PipeValue PipeLamdbaDelegate(PipeValue obj, PipeValue index);

  /// <summary>
  /// Represents a function argument on a pipe expression.
  /// </summary>
  public class PipeLambda {
    private readonly PipeLamdbaDelegate fn;

    internal PipeLambda() { }

    public PipeLambda(PipeLamdbaDelegate fn) {
      CommonHelper.ConfirmNotNull(fn, "fn");
      this.fn = fn;
    }

    public PipeValue Invoke(PipeValue obj) {
      return InvokeInternal(obj, PipeValue.Undefined);
    }

    public PipeValue Invoke(PipeValue obj, PipeValue index) {
      return InvokeInternal(obj, index);
    }

    public PipeValue Invoke(PipeValuePropertyEnumerator enumerator) {
      CommonHelper.ConfirmNotNull(enumerator, "enumerator");
      return InvokeInternal(enumerator.CurrentValue, enumerator.CurrentKey);
    }

    protected virtual PipeValue InvokeInternal(PipeValue obj, PipeValue index) {
      return fn(obj, index);
    }
  }
}
