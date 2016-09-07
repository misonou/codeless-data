using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class EmptyPropertyEnumerator : PipeValuePropertyEnumerator {
    public static readonly EmptyPropertyEnumerator Default = new EmptyPropertyEnumerator();

    private EmptyPropertyEnumerator() { }

    protected override int GetCount() {
      return 0;
    }

    protected override IEnumerator GetEnumerator() {
      yield break;
    }

    protected override PipeValue GetValue(IEnumerator enumerator) {
      throw new NotImplementedException();
    }
  }
}
