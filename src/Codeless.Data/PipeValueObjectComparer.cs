using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  public class PipeValueObjectComparer : Comparer<PipeValue> {
    public static new readonly PipeValueObjectComparer Default = new PipeValueObjectComparer();

    public override int Compare(PipeValue x, PipeValue y) {
      if (x.IsArray && y.IsArray) {
        int result = 0;
        PipeValuePropertyEnumerator iterX = x.GetEnumerator();
        PipeValuePropertyEnumerator iterY = y.GetEnumerator();
        while (iterX.MoveNext() && iterY.MoveNext() && result == 0) {
          result = Compare(iterX.CurrentValue, iterY.CurrentValue);
        }
        return result != 0 ? result : iterX.Count - iterY.Count;
      }
      return x == y ? 0 : !x.IsEvallable || x < y ? -1 : !y.IsEvallable || x > y ? 1 : 0;
    }
  }
}
