﻿#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: EnvironmentEx.cs
// Version:  2017-10-31 02:10
// 
// Copyright (c) 2017, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using Microsoft.Win32;

    /// <summary>
    ///     Provides static methods based on the <see cref="Environment"/> class to provide informations
    ///     about the current environment.
    /// </summary>
    public static class EnvironmentEx
    {
        private static List<string> _cmdLineArgs;
        private static bool _cmdLineArgsQuotes;
        private static string _commandLine;
        private static Version _version;

        /// <summary>
        ///     Gets a <see cref="System.Version"/> object that describes the exact major, minor, build
        ///     and revision numbers of the common language runtime.
        /// </summary>
        public static Version Version
        {
            get
            {
                if (_version != default(Version))
                    return _version;
                try
                {
                    var envDir = PathEx.Combine("%WinDir%\\Microsoft.NET",
#if x64
                                                "Framework64"
#else
                                                "Framework"
#endif
                                               );
                    foreach (var dir in Directory.EnumerateDirectories(envDir).Reverse())
                    {
                        var path = Path.Combine(dir, "System.dll");
                        if (!File.Exists(path))
                            continue;
                        _version = Data.GetVersion(path);
                        break;
                    }
                    if (_version < Environment.Version)
                        throw new ArgumentException();
                }
                catch
                {
                    const string keyPath = "SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full";
                    try
                    {
                        var version = Reg.Read(Registry.LocalMachine, keyPath, "Version", string.Empty);
                        if (string.IsNullOrWhiteSpace(version))
                            throw new ArgumentException();
                        version = version.Split('.').Select(s => s.Length > 1 ? s.TrimStart('0') : s).Join('.');
                        _version = new Version(version);
                    }
                    catch
                    {
                        var release = Reg.Read(Registry.LocalMachine, keyPath, "Release", 0);
                        if (release >= 461308)
                            _version = new Version(4, 7, 2);
                        else if (release >= 460805)
                            _version = new Version(4, 7, 1);
                        else if (release >= 460798)
                            _version = new Version(4, 7);
                        else if (release >= 394802)
                            _version = new Version(4, 6, 2);
                        else if (release >= 394254)
                            _version = new Version(4, 6, 1);
                        else if (release >= 393295)
                            _version = new Version(4, 6);
                        else if (release >= 379893)
                            _version = new Version(4, 5, 2);
                        else if (release >= 378675)
                            _version = new Version(4, 5, 1);
                        else if (release >= 378389)
                            _version = new Version(4, 5);
                        else
                            _version = Environment.Version;
                    }
                }
                return _version;
            }
        }

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string <see cref="List{T}"/>
        ///     containing the command-line arguments for the current process.
        /// </summary>
        /// <param name="sort">
        ///     true to sort the arguments ascended with the rules of
        ///     <see cref="Comparison.AlphanumericComparer(bool)"/> before returning the arguments;
        ///     otherwise, false.
        /// </param>
        /// <param name="skip">
        ///     The number of arguments to skip before returning the remaining arguments.
        /// </param>
        /// <param name="quotes">
        ///     true to store the arguments in quotation marks which containing spaces; otherwise,
        ///     false.
        /// </param>
        public static List<string> CommandLineArgs(bool sort = true, int skip = 1, bool quotes = true)
        {
            if (_cmdLineArgs?.Count == Environment.GetCommandLineArgs().Length - skip && quotes == _cmdLineArgsQuotes)
                return _cmdLineArgs;
            _cmdLineArgs = new List<string>();
            _cmdLineArgsQuotes = quotes;
            if (Environment.GetCommandLineArgs().Length <= skip)
                return _cmdLineArgs;
            string arg = null;
            var defaultArgs = Environment.GetCommandLineArgs().ToList();
            if (defaultArgs.Any(x => (arg = x).EqualsEx($"/{Log.DebugKey}")))
            {
                var index = defaultArgs.IndexOf(arg);
                defaultArgs.RemoveAt(index);
                if (defaultArgs.Count > index)
                    defaultArgs.RemoveAt(index);
            }
            if (defaultArgs.Count > skip)
                defaultArgs = defaultArgs.Skip(skip).ToList();
            if (sort)
                defaultArgs = defaultArgs.OrderBy(x => x, new Comparison.AlphanumericComparer()).ToList();
            _cmdLineArgs = quotes ? defaultArgs.Select(x => x.Any(char.IsWhiteSpace) ? $"\"{x}\"" : x).ToList() : defaultArgs;
            return _cmdLineArgs;
        }

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string <see cref="List{T}"/>
        ///     containing the command-line arguments for the current process.
        /// </summary>
        /// <param name="sort">
        ///     true to sort the arguments ascended with the rules of
        ///     <see cref="Comparison.AlphanumericComparer(bool)"/> before returning the arguments;
        ///     otherwise, false.
        /// </param>
        /// <param name="quotes">
        ///     true to store the arguments in quotation marks which containing spaces; otherwise,
        ///     false.
        /// </param>
        public static List<string> CommandLineArgs(bool sort, bool quotes) =>
            CommandLineArgs(sort, 1, quotes);

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string <see cref="List{T}"/>
        ///     containing the command-line arguments for the current process.
        /// </summary>
        /// <param name="skip">
        ///     The number of arguments to skip before returning the remaining arguments.
        /// </param>
        public static List<string> CommandLineArgs(int skip) =>
            CommandLineArgs(true, skip);

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string containing the
        ///     command-line arguments for the current process.
        /// </summary>
        /// <param name="sort">
        ///     true to sort the arguments ascended with the rules of
        ///     <see cref="Comparison.AlphanumericComparer(bool)"/> before returning the arguments;
        ///     otherwise, false.
        /// </param>
        /// <param name="skip">
        ///     The number of arguments to skip before returning the remaining arguments.
        /// </param>
        /// <param name="quotes">
        ///     true to store the arguments in quotation marks which containing spaces; otherwise,
        ///     false.
        /// </param>
        public static string CommandLine(bool sort = true, int skip = 1, bool quotes = true)
        {
            if (CommandLineArgs(sort).Count > 0)
                _commandLine = CommandLineArgs(sort, skip, quotes).Join(" ");
            return _commandLine ?? string.Empty;
        }

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string containing the
        ///     command-line arguments for the current process.
        /// </summary>
        /// <param name="sort">
        ///     true to sort the arguments ascended with the rules of
        ///     <see cref="Comparison.AlphanumericComparer(bool)"/> before returning the arguments;
        ///     otherwise, false.
        /// </param>
        /// <param name="quotes">
        ///     true to store the arguments in quotation marks which containing spaces; otherwise,
        ///     false.
        /// </param>
        public static string CommandLine(bool sort, bool quotes) =>
            CommandLine(true, 1, quotes);

        /// <summary>
        ///     Provides filtering and sorting options, and returns a string containing the
        ///     command-line arguments for the current process.
        /// </summary>
        /// <param name="skip">
        ///     The number of arguments to skip before returning the remaining arguments.
        /// </param>
        public static string CommandLine(int skip) =>
            CommandLine(true, skip);

        /// <summary>
        ///     <para>
        ///         Retrieves the value of an environment variable from the current process.
        ///     </para>
        ///     <para>
        ///         <c>
        ///             Hint:
        ///         </c>
        ///         Allows <see cref="Environment.SpecialFolder"/> names inlcuding a
        ///         keyword "CurDir" to get the current code base location based
        ///         on <see cref="Assembly.GetEntryAssembly()"/>.CodeBase.
        ///     </para>
        /// </summary>
        /// <param name="variable">
        ///     The name of the environment variable.
        /// </param>
        /// <param name="lower">
        ///     true to convert the result to lowercase; otherwise, false.
        /// </param>
        public static string GetVariableValue(string variable, bool lower = false)
        {
            var output = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(variable))
                    throw new ArgumentNullException(nameof(variable));
                variable = variable.RemoveChar('%');
                if (variable.EqualsEx("CurrentDir", "CurDir"))
                    output = PathEx.LocalDir;
                else
                    try
                    {
                        var match = Enum.GetNames(typeof(Environment.SpecialFolder)).First(s => variable.EqualsEx(s));
                        if (!Enum.TryParse(match, out Environment.SpecialFolder specialFolder))
                            throw new ArgumentException();
                        output = Environment.GetFolderPath(specialFolder);
                    }
                    catch
                    {
                        output = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                                            .First(x => variable.EqualsEx(x.Key.ToString())).Value.ToString();
                    }
                if (lower)
                    output = output?.ToLower();
            }
            catch (InvalidOperationException ex)
            {
                if (Log.DebugMode > 1)
                    Log.Write(ex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return output;
        }

        /// <summary>
        ///     Converts the specified path to an existing environment variable, if possible.
        /// </summary>
        /// <param name="path">
        ///     The path to convert.
        /// </param>
        /// <param name="curDir">
        ///     true to consider the <see cref="Assembly.GetEntryAssembly()"/>.CodeBase value;
        ///     otherwise, false.
        /// </param>
        /// <param name="special">
        ///     true to consider the <see cref="Environment.SpecialFolder"/> values;
        ///     otherwise, false.
        /// </param>
        public static string GetVariablePath(string path, bool curDir = true, bool special = true)
        {
            var output = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));
                var current = path.TrimEnd(Path.DirectorySeparatorChar);
                for (var i = 0; i < 2; i++)
                {
                    if (curDir && current.EqualsEx(GetVariableValue("CurDir")))
                        output = "CurDir";
                    else
                        try
                        {
                            if (!special)
                                throw new NotSupportedException();
                            output = Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>()
                                         .First(s => current.EqualsEx(Environment.GetFolderPath(s))).ToString();
                        }
                        catch
                        {
                            output = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                                                .First(x => current.EqualsEx(x.Value.ToString())).Key.ToString();
                        }
                    if (!string.IsNullOrWhiteSpace(output))
                        break;
                    current += Path.DirectorySeparatorChar;
                }
                if (!string.IsNullOrWhiteSpace(output))
                    output = $"%{output}%";
            }
            catch (InvalidOperationException ex)
            {
                if (Log.DebugMode > 1)
                    Log.Write(ex);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return output;
        }

        /// <summary>
        ///     Returns a new string in which all occurrences in this instance are replaced
        ///     with a valid environment variable.
        /// </summary>
        /// <param name="path">
        ///     The path to convert.
        /// </param>
        /// <param name="curDir">
        ///     true to consider the <see cref="Assembly.GetEntryAssembly()"/>.CodeBase value;
        ///     otherwise, false.
        /// </param>
        /// <param name="special">
        ///     true to consider the <see cref="Environment.SpecialFolder"/> values;
        ///     otherwise, false.
        /// </param>
        public static string GetVariablePathFull(string path, bool curDir = true, bool special = true)
        {
            var output = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));
                var levels = path.Count(c => c == Path.DirectorySeparatorChar) + 1;
                var current = PathEx.Combine(path);
                for (var i = 0; i < levels; i++)
                {
                    output = GetVariablePath(current, curDir, special);
                    if (!string.IsNullOrEmpty(output))
                        break;
                    current = PathEx.Combine(current, "..").Trim(Path.DirectorySeparatorChar);
                }
                if (string.IsNullOrWhiteSpace(output))
                    throw new ArgumentNullException(nameof(output));
                var sub = path.Substring(current.Length).Trim(Path.DirectorySeparatorChar);
                output = output.Trim(Path.DirectorySeparatorChar);
                output = Path.Combine(output, sub);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return !string.IsNullOrWhiteSpace(output) ? output : path;
        }

        /// <summary>
        ///     Provides functionality to verify the installation of redistributable packages.
        /// </summary>
        public static class Redist
        {
            /// <summary>
            ///     Provides identity flags of redistributable packages.
            /// </summary>
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public enum Flags
            {
                /// <summary>
                ///     Microsoft Visual C++ 2005 Redistributable Package (x86).
                /// </summary>
                VC2005X86,

                /// <summary>
                ///     Microsoft Visual C++ 2005 Redistributable Package (x64).
                /// </summary>
                VC2005X64,

                /// <summary>
                ///     Microsoft Visual C++ 2008 Redistributable Package (x86).
                /// </summary>
                VC2008X86,

                /// <summary>
                ///     Microsoft Visual C++ 2008 Redistributable Package (x64).
                /// </summary>
                VC2008X64,

                /// <summary>
                ///     Microsoft Visual C++ 2010 Redistributable Package (x86).
                /// </summary>
                VC2010X86,

                /// <summary>
                ///     Microsoft Visual C++ 2010 Redistributable Package (x64).
                /// </summary>
                VC2010X64,

                /// <summary>
                ///     Microsoft Visual C++ 2012 Redistributable Package (x86).
                /// </summary>
                VC2012X86,

                /// <summary>
                ///     Microsoft Visual C++ 2012 Redistributable Package (x64).
                /// </summary>
                VC2012X64,

                /// <summary>
                ///     Microsoft Visual C++ 2013 Redistributable Package (x86).
                /// </summary>
                VC2013X86,

                /// <summary>
                ///     Microsoft Visual C++ 2013 Redistributable Package (x64).
                /// </summary>
                VC2013X64,

                /// <summary>
                ///     Microsoft Visual C++ 2015 Redistributable Package (x86).
                /// </summary>
                VC2015X86,

                /// <summary>
                ///     Microsoft Visual C++ 2015 Redistributable Package (x64).
                /// </summary>
                VC2015X64,

                /// <summary>
                ///     Microsoft Visual C++ 2017 Redistributable Package (x86).
                /// </summary>
                VC2017X86,

                /// <summary>
                ///     Microsoft Visual C++ 2017 Redistributable Package (x64).
                /// </summary>
                VC2017X64
            }

            private static string[] _displayNames;

            /// <summary>
            ///     Returns the display names of all installed Microsoft Visual C++ redistributable packages.
            /// </summary>
            /// <param name="refresh">
            ///     true to refresh all names; otherwise, false to get the cached names from previous call.
            ///     <para>
            ///         Please note that this parameter is always true if this function has never been called
            ///         before.
            ///     </para>
            /// </param>
            public static string[] GetDisplayNames(bool refresh = false)
            {
                try
                {
                    if (!refresh && _displayNames != null)
                        return _displayNames;
                    var comparer = new Comparison.AlphanumericComparer();
                    var entries = new[]
                    {
                        "DisplayName",
                        "ProductName"
                    };
                    var names = Reg.GetSubKeyTree(Registry.LocalMachine, "SOFTWARE\\Classes\\Installer", 3000)
                                   .SelectMany(x => entries, (x, y) => Reg.ReadString(Registry.LocalMachine, x, y))
                                   .Where(x => x.StartsWithEx("Microsoft Visual C++"))
                                   .OrderBy(x => x, comparer);
                    _displayNames = names.ToArray();
                    return _displayNames;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return null;
                }
            }

            /// <summary>
            ///     Determines whether the specified redistributable package is installed.
            /// </summary>
            /// <param name="keys">
            ///     The redistributable package keys to check.
            /// </param>
            public static bool IsInstalled(params Flags[] keys)
            {
                try
                {
                    var result = false;
                    var names = GetDisplayNames();
                    foreach (var key in keys.Select(x => x.ToString()))
                    {
                        var year = key.Substring(2, 4);
                        var arch = key.Substring(6);
                        Recheck:
                        switch (year)
                        {
                            case "2005":
                                result = names.Any(x => x.Contains(year) && (arch.EqualsEx("x64") && x.ContainsEx(arch) || !x.ContainsEx("x64")));
                                break;
                            default:
                                result = names.Any(x => x.Contains(year) && x.ContainsEx(arch));
                                break;
                        }
                        if (result)
                            continue;
                        switch (year)
                        {
                            case "2015":
                                year = "2017";
                                goto Recheck;
                        }
                        break;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return false;
                }
            }
        }

        /// <summary>
        ///     Provides functionality to handle system restore points.
        /// </summary>
        public static class SystemRestore
        {
            /// <summary>
            ///     The type of event. For more information, see <see cref="Create"/>.
            /// </summary>
            public enum EventType
            {
                /// <summary>
                ///     A system change has begun. A subsequent nested call does not create a new restore
                ///     point.
                ///     <para>
                ///         Subsequent calls must use <see cref="EndNestedSystemChange"/>, not
                ///         <see cref="EndSystemChange"/>.
                ///     </para>
                /// </summary>
                BeginNestedSystemChange = 0x66,

                /// <summary>
                ///     A system change has begun.
                /// </summary>
                BeginSystemChange = 0x64,

                /// <summary>
                ///     A system change has ended.
                /// </summary>
                EndNestedSystemChange = 0x67,

                /// <summary>
                ///     A system change has ended.
                /// </summary>
                EndSystemChange = 0x65
            }

            /// <summary>
            ///     The type of restore point. For more information, see <see cref="Create"/>.
            /// </summary>
            public enum PointType
            {
                /// <summary>
                ///     An application has been installed.
                /// </summary>
                ApplicationInstall = 0x0,

                /// <summary>
                ///     An application has been uninstalled.
                /// </summary>
                ApplicationUninstall = 0x1,

                /// <summary>
                ///     An application needs to delete the restore point it created. For example, an
                ///     application would use this flag when a user cancels an installation.
                /// </summary>
                CancelledOperation = 0xd,

                /// <summary>
                ///     A device driver has been installed.
                /// </summary>
                DeviceDriverInstall = 0xa,

                /// <summary>
                ///     An application has had features added or removed.
                /// </summary>
                ModifySettings = 0xc
            }

            /// <summary>
            ///     Determines whether the system restoring is enabled.
            /// </summary>
            public static bool IsEnabled =>
                Reg.Read(Registry.LocalMachine, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore", "RPSessionInterval", 0) > 0;

            /// <summary>
            ///     Creates a restore point on the local system.
            /// </summary>
            /// <param name="description">
            ///     The description to be displayed so the user can easily identify a restore point.
            /// </param>
            /// <param name="eventType">
            ///     The type of event.
            /// </param>
            /// <param name="restorePointType">
            ///     The type of restore point.
            /// </param>
            public static void Create(string description, EventType eventType, PointType restorePointType)
            {
                try
                {
                    if (!IsEnabled)
                        throw new WarningException("System restoring is disabled.");
                    var mScope = new ManagementScope("\\\\localhost\\root\\default");
                    var mPath = new ManagementPath("SystemRestore");
                    var options = new ObjectGetOptions();
                    using (var mClass = new ManagementClass(mScope, mPath, options))
                        using (var parameters = mClass.GetMethodParameters("CreateRestorePoint"))
                        {
                            parameters["Description"] = description;
                            parameters["EventType"] = (int)eventType;
                            parameters["RestorePointType"] = (int)restorePointType;
                            mClass.InvokeMethod("CreateRestorePoint", parameters, null);
                        }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }
    }
}
