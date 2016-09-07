using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class StringPropertyEnumerator : PipeValuePropertyEnumerator {
    private readonly string value;

    public StringPropertyEnumerator(string value) {
      CommonHelper.ConfirmNotNull(value, "value");
      this.value = value;
    }

    protected override int GetCount() {
      return 1;
    }

    protected override IEnumerator GetEnumerator() {
      yield return "length";
    }

    protected override PipeValue GetValue(IEnumerator enumerator) {
      return value.Length;
    }
  }
}
