using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class EnumerablePropertyEnumerator : PipeValuePropertyEnumerator {
    private IEnumerable enumerable;
    private object currentValue;
    private int currentIndex;

    public EnumerablePropertyEnumerator(IEnumerable enumerable) {
      CommonHelper.ConfirmNotNull(enumerable, "enumerable");
      this.enumerable = enumerable;
    }

    protected override int GetCount() {
      return enumerable.OfType<object>().Count();
    }

    protected override IEnumerator GetEnumerator() {
      currentIndex = -1;
      currentValue = null;
      foreach (object obj in enumerable) {
        currentValue = obj;
        yield return (++currentIndex).ToString();
      }
    }

    protected override PipeValue GetValue(IEnumerator enumerator) {
      return new PipeValue(currentValue);
    }
  }
}
