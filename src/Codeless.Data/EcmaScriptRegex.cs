using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Data {
  /// <summary>
  /// Represents a ECMAScript-like regular expression object.
  /// </summary>
  public class EcmaScriptRegex {
    private static readonly ConcurrentDictionary<string, EcmaScriptRegex> cache = new ConcurrentDictionary<string, EcmaScriptRegex>();
    private readonly Regex re;

    private EcmaScriptRegex(Regex re, bool global) {
      this.re = re;
      this.Global = global;
    }

    /// <summary>
    /// Indicates that the regular expression should be tested against all possible matches in a string.
    /// </summary>
    public bool Global { get; }

    /// <summary>
    /// Indicates that a multiline input string should be treated as multiple lines. 
    /// In such case "^" and "$" change from matching at only the start or end of the entire string to the start or end of any line within the string.
    /// </summary>
    public bool Multiline {
      get { return re.Options.HasFlag(RegexOptions.Multiline); }
    }

    /// <summary>
    /// Indicates that case should be ignored while attempting a match in a string.
    /// </summary>
    public bool IgnoreCase {
      get { return re.Options.HasFlag(RegexOptions.IgnoreCase); }
    }

    /// <summary>
    /// Tests whether there is any occurences in the specified string that matches the pattern.
    /// </summary>
    /// <param name="input">A string to test against.</param>
    /// <returns></returns>
    public bool Test(string input) {
      return re.IsMatch(input);
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of pipe function argument.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A pipe function argument.</param>
    /// <returns></returns>
    public string Replace(string input, PipeLambda replacement) {
      return Replace(input, new PipeLambdaMatchEvaluator(input, replacement).MatchEvaluator);
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the value returned from the invocation of native method.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">A delegate escapulating a method that returns replacement string for the specifc occurence.</param>
    /// <returns></returns>
    public string Replace(string input, MatchEvaluator replacement) {
      if (Global) {
        return re.Replace(input, replacement);
      } else {
        return re.Replace(input, replacement, 1);
      }
    }

    /// <summary>
    /// Replaces occurences of substrings that matches the pattern by the specified replacement.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <param name="replacement">Replacement string.</param>
    /// <returns></returns>
    public string Replace(string input, string replacement) {
      if (Global) {
        return re.Replace(input, replacement);
      } else {
        return re.Replace(input, replacement, 1);
      }
    }

    /// <summary>
    /// Parses the given string into an instance of the <see cref="EcmaScriptRegex"/> class if the string represents a valid ECMAScript-compatible regular expression.
    /// </summary>
    /// <param name="str">A string representing a valid ECMAScript-compatible regular expression.</param>
    /// <param name="re">A reference to a variable that the parsed regular expression object is set to.</param>
    /// <returns>Returns *true* if the given string represents a valid ECMAScript-compatible regular expression; or *false* otherwise.</returns>
    public static bool TryParse(string str, out EcmaScriptRegex re) {
      CommonHelper.ConfirmNotNull(str, "str");
      if (str.Length > 0 && str[0] == '/') {
        if (!cache.TryGetValue(str, out re)) {
          Match m = Regex.Match(str, @"\/((?![*+?])(?:[^\r\n\[/\\]|\\.|\[(?:[^\r\n\]\\]|\\.)*\])+)\/((?:g(?:im?|mi?)?|i(?:gm?|mg?)?|m(?:gi?|ig?)?)?)");
          if (m.Success) {
            RegexOptions options = 0;
            if (m.Groups[2].Value.Contains('i')) {
              options |= RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
            }
            if (m.Groups[2].Value.Contains('m')) {
              options |= RegexOptions.Multiline | RegexOptions.ECMAScript;
            }
            re = new EcmaScriptRegex(new Regex(m.Groups[1].Value, options), m.Groups[2].Value.Contains('g'));
            cache.TryAdd(str, re);
            return true;
          }
          cache.TryAdd(str, null);
        }
      }
      re = null;
      return false;
    }

    private class PipeLambdaMatchEvaluator {
      private readonly string input;
      private readonly PipeLambda replacement;

      public PipeLambdaMatchEvaluator(string input, PipeLambda replacement) {
        this.input = input;
        this.replacement = replacement;
      }

      public string MatchEvaluator(Match m) {
        PipeValueObjectBuilder collection = new PipeValueObjectBuilder(false);
        foreach (Group group in m.Groups) {
          collection.Add(group.Value, collection.Count.ToString());
        }
        collection.Add(m.Index, "index");
        collection.Add(input, "input");
        return (string)replacement.Invoke(collection);
      }
    }
  }
}
