﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;
using ComputeSharp.Core.Extensions;
using ComputeSharp.Exceptions;
using Microsoft.Toolkit.Diagnostics;

namespace ComputeSharp
{
    /// <summary>
    /// An attribute that contains info on a processed shader method that can be executed within a shader.
    /// Instances of this attribute are generated from method annotated with <see cref="ShaderMethodAttribute"/>.
    /// </summary>
    /// <remarks>This attribute is not meant to be directly used by applications using ComputeSharp.</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This attribute is meant to be used from the source generator only")]
    public sealed class ShaderMethodSourceAttribute : Attribute
    {
        /// <summary>
        /// The identifier for the invoke method, for late binding.
        /// </summary>
        internal const string InvokeMethodIdentifier = "__<NAME>__";

        /// <summary>
        /// The source code for the target entry point method.
        /// </summary>
        private readonly string invokeMethod;

        /// <summary>
        /// Creates a new <see cref="ShaderMethodSourceAttribute"/> instance with the specified parameters.
        /// </summary>
        /// <param name="methodName">The fully qualified name of the current method.</param>
        /// <param name="types">The collection of custom types.</param>
        /// <param name="invokeMethod">The source code for the target entry point method.</param>
        /// <param name="methods">The collection of processed methods.</param>
        public ShaderMethodSourceAttribute(string methodName, string[] types, string invokeMethod, string[] methods)
        {
            this.invokeMethod = invokeMethod;

            MethodName = methodName;
            Types = types;
            Methods = methods;
        }

        /// <summary>
        /// Gets the fully qualified name of the shader type.
        /// </summary>
        internal string MethodName { get; }

        /// <summary>
        /// Gets the collection of processed custom types.
        /// </summary>
        internal IReadOnlyCollection<string> Types { get; }

        /// <summary>
        /// Gets the collection of processed methods.
        /// </summary>
        internal IReadOnlyCollection<string> Methods { get; }

        /// <summary>
        /// Gets the mapped source code for the current method.
        /// </summary>
        /// <param name="name">The name to bind the method to.</param>
        /// <returns>The mapped source code for the current mehtod.</returns>
        [Pure]
        internal string GetMappedInvokeMethod(string name)
        {
            return this.invokeMethod.Replace(InvokeMethodIdentifier, name);
        }

        /// <summary>
        /// Gets the associated <see cref="ShaderMethodSourceAttribute"/> instance for a specified type.
        /// </summary>
        /// <typeparam name="T">The shader type to get the attribute for.</typeparam>
        /// <returns>The associated <see cref="IComputeShaderSourceAttribute"/> instance for type <typeparamref name="T"/>.</returns>
        [Pure]
        internal static ShaderMethodSourceAttribute GetForDelegate(Delegate function)
        {
            Guard.IsTrue(function.Method.IsStatic, "Captured delegates need to wrap static methods");

            var attributes = function.Method.DeclaringType.Assembly.GetCustomAttributes<ShaderMethodSourceAttribute>();
            string methodName = function.Method.GetFullName();

            foreach (var attribute in attributes)
            {
                if (attribute.MethodName.Equals(methodName))
                {
                    return attribute;
                }
            }

            return MissingMethodSourceException.Throw(function);
        }
    }
}