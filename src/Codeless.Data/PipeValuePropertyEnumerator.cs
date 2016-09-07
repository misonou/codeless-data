using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  public abstract class PipeValuePropertyEnumerator : IEnumerator, IEnumerator<string> {
    private readonly Lazy<int> count;
    private IEnumerator enumerator;

    internal PipeValuePropertyEnumerator() {
      count = new Lazy<int>(GetCount);
    }

    public int Count { get { return count.Value; } }
    public int CurrentIndex { get; private set; }
    public string CurrentKey { get; private set; }
    public PipeValue CurrentValue { get; private set; }

    protected abstract int GetCount();
    protected abstract IEnumerator GetEnumerator();
    protected abstract PipeValue GetValue(IEnumerator enumerator);
    
    public bool MoveNext() {
      if (enumerator == null) {
        enumerator = GetEnumerator();
      }
      if (enumerator.MoveNext()) {
        CurrentIndex++;
        CurrentKey = (string)enumerator.Current;
        CurrentValue = GetValue(enumerator);
        return true;
      }
      return false;
    }

    public void Reset() {
      CurrentIndex = -1;
      CurrentKey = String.Empty;
      CurrentValue = PipeValue.Undefined;
      enumerator = null;
    }

    void IDisposable.Dispose() { }

    object IEnumerator.Current {
      get { return CurrentKey; }
    }

    string IEnumerator<string>.Current {
      get { return CurrentKey; }
    }
  }
}
