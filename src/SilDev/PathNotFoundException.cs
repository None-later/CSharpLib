﻿#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: PathNotFoundException.cs
// Version:  2019-01-30 10:22
// 
// Copyright (c) 2019, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    /// <summary>
    ///     The exception thrown when an attempt to access an target fails.
    /// </summary>
    [Serializable]
    public class PathNotFoundException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PathNotFoundException"/> class.
        /// </summary>
        public PathNotFoundException() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathNotFoundException"/> class
        ///     with the target that causes this exception.
        /// </summary>
        /// <param name="target">
        ///     The target that caused the exception.
        /// </param>
        public PathNotFoundException(string target) : base(target) =>
            Message = $"Could not find target \'{target}\'.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathNotFoundException"/> class
        ///     with a specified error message and the exception that is the cause of this
        ///     exception.
        /// </summary>
        /// <param name="target">
        ///     The target that caused the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference.
        /// </param>
        public PathNotFoundException(string target, Exception innerException) : base(target, innerException) =>
            Message = $"Could not find target \'{target}\'.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="PathNotFoundException"/> class
        ///     with serialized data.
        /// </summary>
        protected PathNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        public sealed override string Message { get; } = "Unable to find the target from the specified path.";

        /// <summary>
        ///     Sets the <see cref="SerializationInfo"/> object with the target and additional
        ///     exception information.
        /// </summary>
        /// <param name="info">
        ///     The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        ///     The contextual information about the source or destination.
        /// </param>
        [SecurityCritical]
        public new virtual void GetObjectData(SerializationInfo info, StreamingContext context) =>
            base.GetObjectData(info, context);
    }
}
