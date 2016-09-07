using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Data.Internal {
  [DebuggerDisplay("{TextValue,nq}")]
  internal class Pipe : Collection<PipeArgument> {
    private static readonly Regex reArgument = new Regex(@"\$?(?:[^\s\\]|\\.)+");

    public Pipe(string str, int index) {
      CommonHelper.ConfirmNotNull(str, "str");
      for (Match m = reArgument.Match(str); m.Success; m = m.NextMatch()) {
        Add(new PipeArgument(m.Value.Replace("\\ ", " "), index + m.Index, index + m.Index + m.Length));
      }
      this.TextValue = str;
      this.StartIndex = this[0].StartIndex;
      this.EndIndex = this[this.Count - 1].EndIndex;
    }

    public int StartIndex { get; }
    public int EndIndex { get; }
    public string TextValue { get; }

    public PipeValue Evaluate(EvaluationContext context) {
      return new PipeContext(context, this).Evaluate();
    }

    protected override void InsertItem(int index, PipeArgument item) {
      if (index > 0) {
        this[index - 1].Next = item;
      }
      base.InsertItem(index, item);
    }
  }
}
