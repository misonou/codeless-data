using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Data {
  /// <summary>
  /// Specifies the behavior of template evaluation.
  /// </summary>
  public class EvaluateOptions {
    /// <summary>
    /// Instantiates an instance of the <see cref="EvaluateOptions"/> with default options.
    /// </summary>
    public EvaluateOptions() {
      this.Globals = new PipeGlobal();
    }

    /// <summary>
    /// Gets the collection of global values that is passed to the template evaluation.
    /// </summary>
    public PipeGlobal Globals { get; private set; }

    internal bool OutputXml { get; set; }
  }
}
