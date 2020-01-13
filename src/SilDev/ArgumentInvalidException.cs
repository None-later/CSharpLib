﻿#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: ArgumentInvalidException.cs
// Version:  2020-01-13 13:02
// 
// Copyright (c) 2020, Si13n7 Developments(tm)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using Properties;

    /// <summary>
    ///     The exception that is thrown when a reference is passed to a method that
    ///     does not accept it as a valid argument.
    /// </summary>
    [Serializable]
    public class ArgumentInvalidException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ArgumentInvalidException"/>
        ///     class.
        /// </summary>
        public ArgumentInvalidException() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArgumentInvalidException"/>
        ///     class with the name of the parameter that causes this exception.
        /// </summary>
        /// <param name="paramName">
        ///     The name of the parameter that caused the exception.
        /// </param>
        public ArgumentInvalidException(string paramName) : base(paramName) =>
            Message = ExceptionMessages.ArgumentInvalidParam.FormatCurrent(paramName);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArgumentInvalidException"/>
        ///     class with the parameter name and the exception that is the cause of this
        ///     exception.
        /// </summary>
        /// <param name="paramName">
        ///     The name of the parameter that caused the exception.
        /// </param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null
        ///     reference.
        /// </param>
        public ArgumentInvalidException(string paramName, Exception innerException) : base(paramName, innerException) =>
            Message = ExceptionMessages.ArgumentInvalidParam.FormatCurrent(paramName);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArgumentInvalidException"/>
        ///     class with serialized data.
        /// </summary>
        /// <param name="info">
        ///     The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        ///     The contextual information about the source or destination.
        /// </param>
        protected ArgumentInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        ///     Gets a message that describes the current exception.
        /// </summary>
        public sealed override string Message { get; } = ExceptionMessages.ArgumentInvalid;

        /// <summary>
        ///     Sets the <see cref="SerializationInfo"/> object with the parameter name and
        ///     additional exception information.
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
