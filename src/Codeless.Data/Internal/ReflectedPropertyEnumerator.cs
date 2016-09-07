using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  internal class ReflectedPropertyEnumerator : PipeValuePropertyEnumerator {
    private readonly PropertyDescriptorCollection properties;
    private readonly object obj;

    public ReflectedPropertyEnumerator(object obj) {
      CommonHelper.ConfirmNotNull(obj, "obj");
      this.obj = obj;
      this.properties = TypeDescriptor.GetProperties(obj);
    }

    protected override int GetCount() {
      return properties.Count;
    }

    protected override IEnumerator GetEnumerator() {
      foreach (PropertyDescriptor property in properties) {
        yield return property.Name;
      }
    }

    protected override PipeValue GetValue(IEnumerator enumerator) {
      return new PipeValue(properties[(string)enumerator.Current].GetValue(obj));
    }
  }
}
