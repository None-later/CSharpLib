#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: Log.cs
// Version:  2017-01-02 10:31
// 
// Copyright (c) 2017, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    ///     Provides functionality for the catching and logging of handled or unhandled
    ///     <see cref="Exception"/>'s.
    /// </summary>
    public static class Log
    {
        private static bool _conIsOpen, _firstCall, _firstEntry;
        private static IntPtr _stdHandle = IntPtr.Zero;
        private static SafeFileHandle _sfh;
        private static FileStream _fs;
        private static StreamWriter _sw;
        private static readonly AssemblyName AssemblyEntryName = Assembly.GetEntryAssembly().GetName();
        private static readonly string AssemblyName = AssemblyEntryName.Name;
        private static readonly Version AssemblyVersion = AssemblyEntryName.Version;
        private static readonly string ConsoleTitle = $"Debug Console ('{AssemblyName}')";

        /// <summary>
        ///     true to enable the catching of unhandled <see cref="Exception"/>'s; otherwise, false.
        /// </summary>
        public static bool CatchUnhandledExceptions { get; set; } = true;

        /// <summary>
        ///     Gets the current <see cref="DebugMode"/> option how <see cref="Exception"/>'s are handled. For
        ///     more informations see <see cref="ActivateLogging(int)"/>.
        /// </summary>
        public static int DebugMode { get; private set; }

        /// <summary>
        ///     Gets the name for the current log file.
        /// </summary>
        public static string FileName => $"{AssemblyName}_{DateTime.Now:yyyy-MM-dd}.log";

        /// <summary>
        ///     <para>
        ///         Gets or sets the location of the current log file.
        ///     </para>
        ///     <para>
        ///         If the specified path doesn't exist, it is created.
        ///     </para>
        ///     <para>
        ///         If the specified path is invalid or this process doesn't have the necessary permissions
        ///         to write to this location, the location is changed to the Windows specified folder for
        ///         temporary files.
        ///     </para>
        /// </summary>
        public static string FileDir { get; set; } = Path.GetTempPath();

        /// <summary>
        ///     Gets the full path of the current log file.
        /// </summary>
        public static string FilePath { get; private set; } = Path.Combine(FileDir, FileName);

        /// <summary>
        ///     <para>
        ///         Specifies the <see cref="DebugMode"/> for the handling of <see cref="Exception"/>'s.
        ///         The <see cref="DebugMode"/> can also specified over an command line argument or an
        ///         config parameter in combination with <see cref="AllowLogging(string, string, string)"/>.
        ///         The following <see cref="DebugMode"/> options are available.
        ///     </para>
        ///     <para>
        ///         0: Logging is disabled. If <see cref="CatchUnhandledExceptions"/> is enabled, unhandled
        ///         <see cref="Exception"/>'s are discarded. This can be useful for public releases to
        ///         prevent any kind of <see cref="Exception"/> notifications to the client. Please note
        ///         that this functions may have dangerous consequences if it is used incorrectly.
        ///     </para>
        ///     <para>
        ///         1: Logging is enabled and all <see cref="Exception"/>'s are logged.
        ///     </para>
        ///     <para>
        ///         2: Logging is enabled, all <see cref="Exception"/>'s are logged, and a new
        ///         <see cref="Console"/> window is allocated for the current process to display the current
        ///         log in real time. Please note that the console is first allocated after the first logging.
        ///     </para>
        /// </summary>
        public static void ActivateLogging(int mode = 1)
        {
            DebugMode = mode;
            if (_firstCall)
                return;
            _firstCall = true;
            if (CatchUnhandledExceptions)
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) => Write(e.Exception, true, true);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => WriteUnhandled(
                    new ApplicationException("Error in the application. Sender object: '" + s + "'; Exception object: '" + e.ExceptionObject + "'"));
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Close();
            }
            if (DebugMode <= 0)
                return;
            try
            {
                FileDir = Path.GetFullPath(FileDir);
                if (!Directory.Exists(FileDir))
                    Directory.CreateDirectory(FileDir);
            }
            catch
            {
                FileDir = Path.GetTempPath();
            }
            finally
            {
                FilePath = Path.Combine(FileDir, FileName);
            }
        }

        /// <summary>
        ///     <para>
        ///         Checks the command line argument for an valid command, "/debug 2" - for example, or checks the
        ///         content of the specified configuration file to specify the current <see cref="DebugMode"/>.
        ///         For more informations see <see cref="ActivateLogging(int)"/>.
        ///     </para>
        /// </summary>
        /// <param name="iniPath">
        ///     <para>
        ///         The full path of the configuration file.
        ///     </para>
        ///     <para>
        ///         Please note that only the INI file format is accepted. This can be changed in the future to
        ///         add frequently used formats, such as the XML file format - for example.
        ///     </para>
        /// </param>
        /// <param name="section">
        ///     The section of the configuration file which hold the key with the value to specify the current
        ///     <see cref="DebugMode"/>. The value must be NULL for a non-section key.
        /// </param>
        /// <param name="key">
        ///     The key of the configuration file which hold the value, to specify the current
        ///     <see cref="DebugMode"/>.
        /// </param>
        public static void AllowLogging(string iniPath = null, string section = null, string key = "DebugMode")
        {
            var mode = 0;
            if (new Regex("/debug [0-2]|/debug \"[0-2]\"").IsMatch(Environment.CommandLine))
            {
                int i;
                if (int.TryParse(new Regex("/debug ([0-2]?)").Match(Environment.CommandLine.RemoveChar('\"'))
                                                             .Groups[1].ToString(), out i) && i > mode)
                    mode = i;
                if (mode > 0)
                    goto ACTIVATE;
            }
            if (!string.IsNullOrEmpty(iniPath) && File.Exists(iniPath))
                mode = Ini.ReadInteger(section, key, iniPath);
            ACTIVATE:
            ActivateLogging(mode);
        }

        /// <summary>
        ///     Writes the specified information into a log file.
        /// </summary>
        /// <param name="logMessage">
        ///     The message text to write.
        /// </param>
        /// <param name="exitProcess">
        ///     true to terminate this process after logging; otherwise, false.
        /// </param>
        public static void Write(string logMessage, bool exitProcess = false)
        {
            if (!_firstCall || DebugMode < 1 || string.IsNullOrEmpty(logMessage))
                return;
            var dat = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff zzz") + Environment.NewLine;
            var log = dat;
            if (!_firstEntry)
            {
                _firstEntry = true;
                if (File.Exists(FilePath))
                    log = new string('-', 120) + Environment.NewLine + Environment.NewLine + dat;
                log += "***Logging has been started***" + Environment.NewLine;
                log += "   '" + Environment.OSVersion + "'; '" + AssemblyName + "'; '" + AssemblyVersion + "'; '" + FilePath + "'" + Environment.NewLine + Environment.NewLine;
                File.AppendAllText(FilePath, log);
                log = dat;
            }
            log += logMessage + Environment.NewLine + Environment.NewLine;
            File.AppendAllText(FilePath, log);
            if (DebugMode <= 1)
            {
                if (!exitProcess)
                    return;
                Environment.ExitCode = 1;
                Environment.Exit(Environment.ExitCode);
            }
            if (!_conIsOpen)
            {
                _conIsOpen = true;
                WinApi.SafeNativeMethods.AllocConsole();
                var hWnd = WinApi.SafeNativeMethods.GetConsoleWindow();
                if (hWnd != IntPtr.Zero)
                {
                    var hMenu = WinApi.UnsafeNativeMethods.GetSystemMenu(hWnd, false);
                    if (hMenu != IntPtr.Zero)
                        WinApi.UnsafeNativeMethods.DeleteMenu(hMenu, 0xf060, 0x0);
                }
                _stdHandle = WinApi.UnsafeNativeMethods.GetStdHandle(-11);
                _sfh = new SafeFileHandle(_stdHandle, true);
                _fs = new FileStream(_sfh, FileAccess.Write);
                if (Console.Title != ConsoleTitle)
                {
                    Console.Title = ConsoleTitle;
                    Console.BufferHeight = short.MaxValue - 1;
                    Console.BufferWidth = Console.WindowWidth;
                    Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                }
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(log);
            Console.ResetColor();
            _sw = new StreamWriter(_fs, Encoding.ASCII) { AutoFlush = true };
            Console.SetOut(_sw);
            if (!exitProcess)
                return;
            Environment.ExitCode = 1;
            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>
        ///     Writes all <see cref="Exception"/> information into a log file.
        /// </summary>
        /// <param name="exception">
        ///     The handled <see cref="Exception"/> to write.
        /// </param>
        /// <param name="forceLogging">
        ///     true to enforce that <see cref="DebugMode"/> is enabled; otherwise, false.
        /// </param>
        /// <param name="exitProcess">
        ///     true to terminate this process after logging; otherwise, false.
        /// </param>
        public static void Write(Exception exception, bool forceLogging = false, bool exitProcess = false)
        {
            if (DebugMode < 1)
            {
                if (!forceLogging)
                    return;
                DebugMode = 1;
            }
            Write("Handled " + exception, exitProcess);
        }

        private static void WriteUnhandled(Exception exception)
        {
            DebugMode = 1;
            Write("Unhandled " + exception, true);
        }

        private static void Close()
        {
            if (_sfh != null && !_sfh.IsClosed)
                _sfh.Close();
            if (!Directory.Exists(FileDir))
                return;
            try
            {
                foreach (var file in Directory.GetFiles(FileDir, $"{AssemblyName}*.log", SearchOption.TopDirectoryOnly))
                {
                    if (FilePath.EqualsEx(file))
                        continue;
                    if ((DateTime.Now - new FileInfo(file).LastWriteTime).TotalDays >= 7d)
                        File.Delete(file);
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     <para>
        ///         Initializes a ne instance of the <see cref="Stopwatch"/> class to determine the loading time
        ///         of the specified <see cref="Form"/> window.
        ///     </para>
        ///     <para>
        ///         This function is ignored if <see cref="DebugMode"/> is disabled or the specified
        ///         <see cref="Form"/> is already visible.
        ///     </para>
        /// </summary>
        /// <param name="form">
        ///     The form window to determine the loading time.
        /// </param>
        public static Form AddLoadingTimeStopwatch(this Form form)
        {
            if (DebugMode <= 0)
                return form;
            try
            {
                if (Application.OpenForms.Cast<Form>().Any(x => x == form))
                    return form;
            }
            catch
            {
                return form;
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            form.Shown += (s, e) =>
            {
                stopwatch.Stop();
                Write("Stopwatch: " + form.Name + " loaded in " + stopwatch.ElapsedMilliseconds + "ms.");
            };
            return form;
        }
    }
}
