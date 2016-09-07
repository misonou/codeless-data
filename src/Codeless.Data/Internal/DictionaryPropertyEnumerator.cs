using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class DictionaryPropertyEnumerator : PipeValuePropertyEnumerator {
    private readonly IDictionary dictionary;

    public DictionaryPropertyEnumerator(IDictionary dictionary) {
      CommonHelper.ConfirmNotNull(dictionary, "dictionary");
      this.dictionary = dictionary;
    }

    protected override int GetCount() {
      return dictionary.Keys.Count;
    }

    protected override IEnumerator GetEnumerator() {
      return dictionary.Keys.GetEnumerator();
    }

    protected override PipeValue GetValue(IEnumerator enumerator) {
      return new PipeValue(dictionary[enumerator.Current]);
    }
  }
}
