﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeless.Data.Internal;
using System.Web.UI;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Codeless.Data {
  /// <summary>
  /// Encapsulates a method that resolves unknown pipe function based on the specified name.
  /// </summary>
  /// <param name="name">The name of pipe function.</param>
  /// <param name="next">A delegate that when called the next resolver delegate is returned.</param>
  /// <returns>Resolved pipe function based on the specified name.</returns>
  public delegate PipeFunction PipeFunctionResolver(string name, GetNextPipeFunctionResolverDelegate next);

  /// <summary>
  /// Encapsulates a method that when called the next resolver delegate is returned.
  /// </summary>
  /// <returns>A delegate encapsulating a method that resolves unknown pipe function based on the specified name.</returns>
  public delegate PipeFunctionResolver GetNextPipeFunctionResolverDelegate();

  /// <summary>
  /// Provides methods to use with waterpipe template.
  /// </summary>
  public static class Waterpipe {
    /// <summary>
    /// Evaluates the template against the specified value.
    /// </summary>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    /// <param name="value">An object defining the root context.</param>
    /// <returns></returns>
    public static string Evaluate(string template, object value) {
      return Evaluate(template, value, new EvaluateOptions());
    }

    /// <summary>
    /// Evaluates the template against the specified value and options.
    /// </summary>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    /// <param name="value">An object defining the root context.</param>
    /// <param name="options">Options that alter rendering behaviors.</param>
    /// <returns></returns>
    public static string Evaluate(string template, object value, EvaluateOptions options) {
      PipeExecutionException[] exceptions;
      object result = EvaluationContext.Evaluate(template, new PipeValue(value), options, out exceptions);
      if (options.OutputXml) {
        return ((XmlNode)result).OuterXml;
      }
      return result.ToString();
    }

    /// <summary>
    /// Evaluates the template against the specified value.
    /// </summary>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    /// <param name="value">An object defining the root context.</param>
    /// <returns></returns>
    public static XmlDocument EvaluateAsXml(string template, object value) {
      return EvaluateAsXml(template, value, new EvaluateOptions());
    }

    /// <summary>
    /// Evaluates the template against the specified value and options.
    /// </summary>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    /// <param name="value">An object defining the root context.</param>
    /// <param name="options">Options that alter rendering behaviors.</param>
    /// <returns></returns>
    public static XmlDocument EvaluateAsXml(string template, object value, EvaluateOptions options) {
      CommonHelper.ConfirmNotNull(options, "options");
      PipeExecutionException[] exceptions;
      options.OutputXml = true;
      XmlNode result = (XmlNode)EvaluationContext.Evaluate(template, new PipeValue(value), options, out exceptions);
      return result.OwnerDocument;
    }

    /// <summary>
    /// Registers a pipe function resolvers that handles unknown pipe functions.
    /// </summary>
    /// <param name="resolver"></param>
    public static void RegisterFunctionResolver(PipeFunctionResolver resolver) {
      EvaluationContext.RegisterFunctionResolver(resolver);
    }

    /// <summary>
    /// Registers a pipe function that run the specified template with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="template">A string that represents a valid waterpipe template.</param>
    public static void RegisterFunction(string name, string template) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(template));
    }

    /// <summary>
    /// Registers a pipe function that takes arbitrary number of arguments or function arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Variadic fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes no arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func0 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes one arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func1 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes two arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func2 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes three arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func3 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes four arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func4 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }

    /// <summary>
    /// Registers a pipe function that takes five arguments from pipe with the specified name.
    /// </summary>
    /// <param name="name">A string specifying the name of the pipe function.</param>
    /// <param name="fn">A delegate encapsulating the native method.</param>
    public static void RegisterFunction(string name, PipeFunction.Func5 fn) {
      EvaluationContext.RegisterFunction(name, PipeFunction.Create(fn));
    }
  }
}
