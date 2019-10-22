﻿#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: Xml.cs
// Version:  2019-10-22 16:28
// 
// Copyright (c) 2019, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    ///     Provides basic functionality for the XML format.
    /// </summary>
    public static class Xml
    {
        /// <summary>
        ///     Serializes the specified object graph into a string representation of an XML
        ///     document.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The type of the source.
        /// </typeparam>
        /// <param name="source">
        ///     The object graph to serialize.
        /// </param>
        public static string Serialize<TSource>(TSource source)
        {
            try
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                string result;
                using (var sw = new StringWriter())
                {
                    new XmlSerializer(typeof(TSource)).Serialize(sw, source);
                    result = sw.ToString();
                }
                return result;
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex);
                return null;
            }
        }

        /// <summary>
        ///     Creates a new XML file, writes the specified object graph into to the XML file,
        ///     and then closes the file.
        /// </summary>
        /// <typeparam name="TSource">
        ///     The type of the source.
        /// </typeparam>
        /// <param name="path">
        ///     The XML file to create.
        /// </param>
        /// <param name="source">
        ///     The object graph to write to the file.
        /// </param>
        /// <param name="overwrite">
        ///     true to allow an existing file to be overwritten; otherwise, false.
        /// </param>
        public static bool SerializeToFile<TSource>(string path, TSource source, bool overwrite = true)
        {
            try
            {
                if (source == null)
                    throw new ArgumentNullException(nameof(source));
                var dest = PathEx.Combine(path);
                using (var fs = new FileStream(dest, overwrite ? FileMode.Create : FileMode.CreateNew))
                    new XmlSerializer(typeof(TSource)).Serialize(fs, source);
                return true;
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Deserializes a string representation of an XML document into an object graph.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type of the result.
        /// </typeparam>
        /// <param name="source">
        ///     The string representation of an XML document to deserialize.
        /// </param>
        /// <param name="defValue">
        ///     The default value.
        /// </param>
        public static TResult Deserialize<TResult>(string source, TResult defValue = default)
        {
            try
            {
                if (string.IsNullOrEmpty(source))
                    throw new ArgumentNullException(nameof(source));
                TResult result;
#if unsafeAllowed
                using (var sr = new StringReader(source))
                    result = (TResult)new XmlSerializer(typeof(TResult)).Deserialize(sr);
#else
                using (var xr = XmlReader.Create(source, new XmlReaderSettings { Async = false }))
                    result = (TResult)new XmlSerializer(typeof(TResult)).Deserialize(xr);
#endif
                return result;
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex);
                return defValue;
            }
        }

        /// <summary>
        ///     Deserializes the specified XML file into an object graph.
        /// </summary>
        /// <typeparam name="TResult">
        ///     The type of the result.
        /// </typeparam>
        /// <param name="path">
        ///     The XML file to deserialize.
        /// </param>
        /// <param name="defValue">
        ///     The default value.
        /// </param>
        public static TResult DeserializeFile<TResult>(string path, TResult defValue = default)
        {
            try
            {
                var src = PathEx.Combine(path);
                if (!File.Exists(src))
                    return defValue;
                TResult result;
#if unsafeAllowed
                using (var fs = new FileStream(src, FileMode.Open, FileAccess.Read))
                {
                    fs = null;
                    result = (TResult)new XmlSerializer(typeof(TResult)).Deserialize(fs);
                }
#else
                var fs = default(FileStream);
                try
                {
                    fs = new FileStream(src, FileMode.Open, FileAccess.Read);
                    using (var xr = XmlReader.Create(fs, new XmlReaderSettings { Async = false }))
                    {
                        fs = null;
                        result = (TResult)new XmlSerializer(typeof(TResult)).Deserialize(xr);
                    }
                }
                finally
                {
                    fs?.Dispose();
                }
#endif
                return result;
            }
            catch (Exception ex) when (ex.IsCaught())
            {
                Log.Write(ex);
                return defValue;
            }
        }
    }
}
