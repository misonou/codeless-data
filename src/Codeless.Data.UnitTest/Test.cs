using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codeless.Data.UnitTest {
  [TestClass]
  public class Test {
    static object obj;

    static void Use(object obj) {
      Test.obj = obj;
    }

    static void Run(string template, string expected) {
      Assert.AreEqual(expected, Waterpipe.Evaluate(template, obj));
    }

    [TestMethod]
    public void TestObjectPath() {
      Use(new { zero = 0, @string = "foo", array = new[] { 1, 2, 3, 4 } });
      Run("{{string}}", "foo");
      Run("{{string.0}}", "f");
      Run("{{string.length}}", "3");
      Run("{{array}}", "[1,2,3,4]");
      Run("{{array.$zero}}", "1");
      Run("{{array.$(array.0)}}", "2");
      Run("{{array.foo.bar}}", "");
    }

    [TestMethod]
    public void TestHtmlEncode() {
      Use(new { @string = "<b>This is HTML.</b>" });
      Run("{{string}}", "&lt;b&gt;This is HTML.&lt;/b&gt;");
      Run("{{&string}}", "<b>This is HTML.</b>");
    }

    [TestMethod]
    public void TestTestingFunction() {
      Use(new { @bool = true, @string = "true", foo = new[] { 1, 3, 5 }, bar = new[] { 1, 3 }, baz = new[] { 2, 4 } });
      Run("{{bool == $string}}", "true");
      Run("{{foo.1 < 3}}", "false");
      Run("{{foo.3 > 3}}", "false");
      Run("{{foo > $bar}}", "true");
      Run("{{baz > $foo}}", "true");
      Run("{{foo.0 between 0 2}}", "true");
      Run("{{string contains true}}", "true");
      Run("{{string like /^true/i}}", "true");
    }

    [TestMethod]
    public void TestConditionalFunction() {
      Use(new { zero = 0, falsy = false, nil = PipeValue.Null, undef = PipeValue.Undefined, empty = "" });
      Run("{{zero or 1}}", "1");
      Run("{{falsy or 1}}", "1");
      Run("{{nil or 1}}", "1");
      Run("{{undef or 1}}", "1");
      Run("{{empty or 1}}", "1");

      Use(new { isTrue = true, isFalse = false, strYes = "oui", strNo = "non" });
      Run("{{isTrue choose yes no}}", "yes");
      Run("{{isTrue choose $strYes $strNo}}", "oui");
      Run("{{? [ $isTrue ] yes no}}", "yes");
      Run("{{? [ $isFalse ] [ $strYes upper ] [ $strNo upper ]}}", "NON");
    }

    [TestMethod]
    public void TestStringFunction() {
      Use(new { @string = "foo", @string2 = "foo/bar/baz" });
      Run("{{string concat bar}}", "foobar");
      Run("{{string2 replace / -}}", "foo-bar/baz");
      Run("{{string2 replace /(\\w)(\\w*)/g [ $1 upper concat $2 ]}}", "Foo/Bar/Baz");
      Run("{{string2 substr 0 3}}", "foo");

      Use(new { @string = " foobar ", foo = "baz", bar = "/baz", @string2 = "foo/bar/baz" });
      Run("{{foo padstart /}}", "/baz");
      Run("{{bar padstart /}}", "/baz");
      Run("{{foo removestart /}}", "baz");
      Run("{{bar removestart /}}", "baz");
      Run("{{string2 cutbefore /}}", "foo");
      Run("{{string2 cutbeforelast /}}", "foo/bar");
      Run("{{string2 cutafter /}}", "bar/baz");
      Run("{{string2 cutafterlast /}}", "baz");

      Use(new { @string = "fooBAZfooBAZ", length = 2 });
      Run("{{string split BAZ join ,}}", "foo,foo");
      Run("{{length repeat foo}}", "foofoofoo");
      Run("{{string upper}}", "FOOBAZFOOBAZ");
      Run("{{string lower}}", "foobazfoobaz");
      Run("{{string ucfirst}}", "FooBAZfooBAZ");
    }

    [TestMethod]
    public void TestMathFunction() {
      Use(new { @int = 100, @float = 10.4 });
      Run("{{int max 10}}", "10");
      Run("{{int max 1000}}", "100");
      Run("{{int min 10}}", "100");
      Run("{{int min 1000}}", "1000");
      Run("{{float round}}", "10");
      Run("{{float floor}}", "10");
      Run("{{float ceil}}", "11");
      Run("{{int + 1}}", "101");
      Run("{{int - 1}}", "99");
      Run("{{int * 2}}", "200");
      Run("{{int / 2}}", "50");
      Run("{{int % 1}}", "0");
    }

    [TestMethod]
    public void TestIterableFunction() {
      Use(new { values = new { foo = 1, bar = 2 }, array = new[] { 1, 2, 3, 4 } });
      Run("{{&values keys}}", "[\"foo\",\"bar\"]");
      Run("{{array keys}}", "[0,1,2,3]");

      Use(new {
        array = new[] {
          new { key = 1, value = 5 },
          new { key = 2, value = 6 },
          new { key = 3, value = 7 },
          new { key = 4, value = 8 }
        }
      });
      Run("{{array map value}}", "[5,6,7,8]");
      Run("{{array map [ $value + 1 ]}}", "[6,7,8,9]");
      Run("{{array map [ $key | $value ]}}", "[[1,5],[2,6],[3,7],[4,8]]");

      Use(new {
        simple = new[] { 4, 2, 3, 1 },
        complex = new[] {
          new { key = 3, value = 2, id = "(3,2)" },
          new { key = 3, value = 1, id = "(3,1)" },
          new { key = 1, value = 4, id = "(1,4)" },
          new { key = 2, value = 3, id = "(2,3)" }
        }
      });
      Run("{{simple sort}}", "[1,2,3,4]");
      Run("{{&complex sortby key map id}}", "[\"(1,4)\",\"(2,3)\",\"(3,2)\",\"(3,1)\"]");
      Run("{{&complex sortby [ $key | $value ] map id}}", "[\"(1,4)\",\"(2,3)\",\"(3,1)\",\"(3,2)\"]");

      Use(new[] { 1, 2, 3, 4 });
      Run("{{reverse}}", "[4,3,2,1]");
      Run("{{where [ odd ]}}", "[1,3]");
      Run("{{first [ even ]}}", "2");
      Run("{{any [ even ]}}", "true");
      Run("{{all [ even ]}}", "false");
    }

    [TestMethod]
    public void TestFormatFunction() {
      Use(new { foo = "baz", bar = 1 });
      Run("{{&:query}}", "foo=baz&bar=1");

      Use(new { timestamp = 1370000000000, @string = "2013-05-31" });
      Run("{{timestamp :date yyyy-MM-dd}}", "2013-05-31");
      Run("{{timestamp :date f}}", "Friday, May 31, 2013 7:33 PM");
      Run("{{string :date f}}", "Friday, May 31, 2013 8:00 AM");
      Run("{{string :date M}}", "May 31");

      //Use(new { foo = 65530, bar = 0.5 });
      //Run("{{foo :printf %x}}", "65530");
      //Run("{{bar :printf %.2f}}", "0.50");
    }
  }
}
