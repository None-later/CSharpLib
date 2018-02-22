#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: ProcessEx.cs
// Version:  2018-02-22 03:01
// 
// Copyright (c) 2018, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using Properties;

    /// <summary>
    ///     Provides static methods based on the <see cref="Process"/> class to enable you to start
    ///     local system processes.
    /// </summary>
    public static class ProcessEx
    {
        private static IntPtr _currentHandle;
        private static string _currentName;

        /// <summary>
        ///     Gets the handle of the current process instance.
        /// </summary>
        public static IntPtr CurrentHandle
        {
            get
            {
                if (_currentHandle != default(IntPtr))
                    return _currentHandle;
                using (var p = Process.GetCurrentProcess())
                    _currentHandle = p.Handle;
                return _currentHandle;
            }
        }

        /// <summary>
        ///     Gets the name of the current process instance.
        /// </summary>
        public static string CurrentName
        {
            get
            {
                if (_currentName != default(string))
                    return _currentName;
                using (var p = Process.GetCurrentProcess())
                    _currentName = p.ProcessName;
                return _currentName;
            }
        }

        /// <summary>
        ///     Gets the parent process of the current process instance.
        /// </summary>
        public static Process CurrentParent
        {
            get
            {
                Process parentProcess;
                using (var p = Process.GetCurrentProcess())
                    parentProcess = p.GetParent();
                return parentProcess;
            }
        }

        /// <summary>
        ///     Gets the parent process of this <see cref="Process"/>.
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> component.
        /// </param>
        public static Process GetParent(this Process process)
        {
            try
            {
                var childId = process.Id;
                var childName = Process.GetProcessById(childId).ProcessName;
                var processes = Process.GetProcessesByName(childName);
                string parentName = null;
                for (var i = 0; i < processes.Length; i++)
                {
                    parentName = i == 0 ? childName : $"{childName}#{i}";
                    var tmpId = new PerformanceCounter("Process", "ID Process", parentName);
                    if ((int)tmpId.NextValue() == childId)
                        break;
                }
                var parentId = new PerformanceCounter("Process", "Creating Process ID", parentName);
                var parentProcess = Process.GetProcessById((int)parentId.NextValue());
                return parentProcess;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
        }

        /// <summary>
        ///     Gets all active instances associated with the specified application.
        /// </summary>
        /// <param name="nameOrPath">
        ///     The filename or the full path to the application to check.
        /// </param>
        /// <param name="doubleTap">
        ///     <para>
        ///         true to try to get firstly by the path, then by name; otherwise, false.
        ///     </para>
        ///     <para>
        ///         Please note that this option has no effect if the first parameter contains
        ///         only a name.
        ///     </para>
        /// </param>
        public static IEnumerable<Process> GetInstances(string nameOrPath, bool doubleTap = false)
        {
            try
            {
                IEnumerable<Process> instances;
                var path = PathEx.Combine(default(char[]), nameOrPath);
                var name = Path.GetFileNameWithoutExtension(path);
                try
                {
                    if (!path.ContainsEx(Path.DirectorySeparatorChar) || !File.Exists(path))
                    {
                        doubleTap = true;
                        throw new ArgumentException();
                    }
                    instances = Process.GetProcesses().Where(p => p.ProcessName.EqualsEx(name) && p.MainModule.FileName.EqualsEx(path));
                }
                catch
                {
                    if (!doubleTap)
                        return null;
                    instances = Process.GetProcessesByName(name);
                }
                return instances;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
        }

        /// <summary>
        ///     Returns the number of all active instances associated with the specified
        ///     application.
        /// </summary>
        /// <param name="nameOrPath">
        ///     The filename or the full path to the application to check.
        /// </param>
        /// <param name="doubleTap">
        ///     <para>
        ///         true to try to check firstly by the path, then by name; otherwise, false.
        ///     </para>
        ///     <para>
        ///         Please note that this option has no effect if the first parameter contains
        ///         only a name.
        ///     </para>
        /// </param>
        public static int InstancesCount(string nameOrPath, bool doubleTap = false)
        {
            var count = 0;
            try
            {
                foreach (var p in GetInstances(nameOrPath, doubleTap))
                {
                    count++;
                    p?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return count;
        }

        /// <summary>
        ///     Determines whether the specified file is matched with a running process.
        /// </summary>
        /// <param name="nameOrPath">
        ///     The filename or the full path to the application to check.
        /// </param>
        /// <param name="doubleTap">
        ///     <para>
        ///         true to try to check firstly by the path, then by name; otherwise, false.
        ///     </para>
        ///     <para>
        ///         Please note that this option has no effect if the first parameter contains
        ///         only a name.
        ///     </para>
        /// </param>
        public static bool IsRunning(string nameOrPath, bool doubleTap = false) =>
            InstancesCount(nameOrPath, doubleTap) > 0;

        /// <summary>
        ///     <para>
        ///         Determines whether this <see cref="Process"/> is running in a sandboxed
        ///         environment.
        ///     </para>
        ///     <para>
        ///         Hint: This function supports only the program Sandboxie.
        ///     </para>
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> to check.
        /// </param>
        public static bool IsSandboxed(this Process process)
        {
            try
            {
                var modules = process.Modules.Cast<ProcessModule>().ToList();
                var path = modules.First(m => Path.GetFileName(m.FileName).EqualsEx("SbieDll.dll"))?.FileName;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return false;
                var info = FileVersionInfo.GetVersionInfo(path);
                return info.FileDescription.EqualsEx("Sandboxie User Mode DLL");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return false;
        }

        /// <summary>
        ///     Returns a string array containing the command-line arguments for this
        ///     <see cref="Process"/>.
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> component.
        /// </param>
        public static string[] GetCommandLineArgs(this Process process)
        {
            try
            {
                var list = new List<string>();
                var query = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}";
                using (var objs = new ManagementObjectSearcher(query))
                    list.AddRange(objs.Get().Cast<ManagementBaseObject>().Select(obj => obj["CommandLine"].ToString()));
                return list.ToArray();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
        }

        /// <summary>
        ///     Returns the command-line arguments for this process.
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> component.
        /// </param>
        public static string GetCommandLine(this Process process) =>
            process.GetCommandLineArgs().Join(' ');

        /// <summary>
        ///     <para>
        ///         Starts (or reuses) the process resource that is specified by the current
        ///         <see cref="Process"/>.StartInfo property of this <see cref="Process"/> and
        ///         associates it with the component.
        ///     </para>
        ///     <para>
        ///         If the <see cref="Process"/>.StartInfo.WorkingDirectory parameter is undefined,
        ///         it is created by <see cref="Process"/>.StartInfo.FileName parameter.
        ///     </para>
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> component to start.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process has
        ///     been started; otherwise, false.
        /// </param>
        public static Process Start(Process process, bool dispose = true)
        {
            try
            {
                process.StartInfo.FileName = PathEx.Combine(process.StartInfo.FileName);
                if (string.IsNullOrEmpty(process.StartInfo.FileName))
                    throw new ArgumentNullException(nameof(process.StartInfo.FileName));
                if (!File.Exists(process.StartInfo.FileName))
                    throw new PathNotFoundException(process.StartInfo.FileName);
                if (process.StartInfo.FileName.EndsWithEx(".lnk"))
                    process.Start();
                else
                {
                    process.StartInfo.WorkingDirectory = PathEx.Combine(process.StartInfo.WorkingDirectory);
                    if (!Directory.Exists(process.StartInfo.WorkingDirectory))
                    {
                        var workingDirectory = Path.GetDirectoryName(process.StartInfo.FileName);
                        if (string.IsNullOrEmpty(workingDirectory))
                            throw new ArgumentNullException(nameof(workingDirectory));
                        process.StartInfo.WorkingDirectory = workingDirectory;
                    }
                    if (!process.StartInfo.UseShellExecute && !process.StartInfo.CreateNoWindow && process.StartInfo.WindowStyle == ProcessWindowStyle.Hidden)
                        process.StartInfo.CreateNoWindow = true;
                    process.Start();
                }
                return process;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
            finally
            {
                if (dispose)
                    process?.Dispose();
            }
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="Process"/> class from the specified
        ///         <see cref="ProcessStartInfo"/> and starts (or reuses) the process component.
        ///     </para>
        ///     <para>
        ///         If WorkingDirectory parameter is undefined, it is created by the FileName parameter.
        ///     </para>
        /// </summary>
        /// <param name="processStartInfo">
        ///     The <see cref="ProcessStartInfo"/> component to initialize a new <see cref="Process"/>.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process has
        ///     been started; otherwise, false.
        /// </param>
        public static Process Start(ProcessStartInfo processStartInfo, bool dispose = true)
        {
            var process = new Process { StartInfo = processStartInfo };
            return Start(process, dispose);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters and starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="workingDirectory">
        ///     The working directory for the process to be started.
        /// </param>
        /// <param name="arguments">
        ///     The command-line arguments to use when starting the application.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="processWindowStyle">
        ///     The window state to use when the process is started.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, string workingDirectory, string arguments, bool verbRunAs = false, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal, bool dispose = true)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = arguments,
                    FileName = fileName,
                    Verb = verbRunAs ? "runas" : string.Empty,
                    WindowStyle = processWindowStyle,
                    WorkingDirectory = workingDirectory
                }
            };
            return Start(process, dispose);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters and starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="workingDirectory">
        ///     The working directory for the process to be started.
        /// </param>
        /// <param name="arguments">
        ///     The command-line arguments to use when starting the application.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, string workingDirectory, string arguments, bool verbRunAs, bool dispose) =>
            Start(fileName, workingDirectory, arguments, verbRunAs, ProcessWindowStyle.Normal, dispose);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters and starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="arguments">
        ///     The command-line arguments to use when starting the application.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="processWindowStyle">
        ///     The window state to use when the process is started.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/> if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, string arguments = null, bool verbRunAs = false, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal, bool dispose = true) =>
            Start(fileName, null, arguments, verbRunAs, processWindowStyle, dispose);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters and starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="arguments">
        ///     The command-line arguments to use when starting the application.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, string arguments, bool verbRunAs, bool dispose) =>
            Start(fileName, null, arguments, verbRunAs, ProcessWindowStyle.Normal, dispose);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters and starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="processWindowStyle">
        ///     The window state to use when the process is started.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, bool verbRunAs, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal, bool dispose = true) =>
            Start(fileName, null, null, verbRunAs, processWindowStyle, dispose);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class from the specified
        ///     parameters to starts (or reuses) the process component.
        /// </summary>
        /// <param name="fileName">
        ///     The application to start.
        /// </param>
        /// <param name="verbRunAs">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process
        ///     has been started; otherwise, false.
        /// </param>
        public static Process Start(string fileName, bool verbRunAs, bool dispose) =>
            Start(fileName, null, null, verbRunAs, dispose);

        /// <summary>
        ///     Retrieves all thread handles of the specified process.
        /// </summary>
        /// <param name="process">
        ///     The <see cref="Process"/> to get all thread handles.
        /// </param>
        public static IEnumerable<IntPtr> ThreadHandles(this Process process)
        {
            var handles = new List<IntPtr>();
            var threads = new WinApi.EnumThreadWndProc((hWnd, lParam) =>
            {
                handles.Add(hWnd);
                return true;
            });
            foreach (ProcessThread thread in process.Threads)
                WinApi.NativeMethods.EnumThreadWindows((uint)thread.Id, threads, IntPtr.Zero);
            return handles;
        }

        /// <summary>
        ///     Immediately closes all threads of the specified processes.
        /// </summary>
        /// <param name="processes">
        ///     The <see cref="Process"/>/es to close.
        /// </param>
        /// <param name="waitOnHandle">
        ///     Wait on the handle.
        /// </param>
        public static bool Close(IEnumerable<Process> processes, bool waitOnHandle = true)
        {
            var count = 0;
            foreach (var p in processes)
                try
                {
                    using (p)
                    {
                        foreach (var h in p.ThreadHandles())
                        {
                            WinApi.NativeHelper.PostMessage(h, 0x10, IntPtr.Zero, IntPtr.Zero);
                            if (!waitOnHandle)
                                continue;
                            var wh = new ManualResetEvent(false)
                            {
                                SafeWaitHandle = new SafeWaitHandle(h, false)
                            };
                            wh.WaitOne(100);
                        }
                        if (p?.HasExited == true)
                            count++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            return count > 0;
        }

        /// <summary>
        ///     Immediately closes all threads of the specified processes.
        /// </summary>
        /// <param name="processes">
        ///     The <see cref="Process"/>/es to close.
        /// </param>
        public static bool Close(params Process[] processes) =>
            Close(processes.ToList());

        /// <summary>
        ///     <para>
        ///         Immediately stops all specified processes.
        ///     </para>
        ///     <para>
        ///         If the current process doesn't have enough privileges to stop a specified process
        ///         it starts an invisible elevated instance of the command prompt to run taskkill.
        ///     </para>
        /// </summary>
        /// <param name="processes">
        ///     The <see cref="Process"/>/es to kill.
        /// </param>
        public static bool Terminate(IEnumerable<Process> processes)
        {
            var count = 0;
            var list = new List<string>();
            foreach (var p in processes)
            {
                string name = null;
                try
                {
                    using (p)
                    {
                        name = p.ProcessName;
                        if (!p.HasExited)
                        {
                            p.Kill();
                            count++;
                        }
                        if (p.HasExited)
                            continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                if (string.IsNullOrEmpty(name) || list.ContainsEx(name))
                    continue;
                list.Add(name);
            }
            if (list.Count == 0)
                return count > 0;
            using (var p = SendHelper.KillAllTasks(list, true, false))
                if (p?.HasExited == false)
                    p.WaitForExit();
            Tray.RefreshAsync(16);
            return count > 0;
        }

        /// <summary>
        ///     <para>
        ///         Immediately stops all specified processes.
        ///     </para>
        ///     <para>
        ///         If the current process doesn't have enough privileges to stop a specified process
        ///         it starts an invisible elevated instance of the command prompt to run taskkill.
        ///     </para>
        /// </summary>
        /// <param name="processes">
        ///     The collection of processes to kill.
        /// </param>
        public static bool Terminate(params Process[] processes) =>
            Terminate(processes.ToList());

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="Process"/> class to execute system
        ///         commands using the system command prompt ("cmd.exe").
        ///     </para>
        ///     <para>
        ///         This can be useful for an unprivileged application as a simple way to execute a
        ///         command with the highest user permissions, for example.
        ///     </para>
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="runAsAdmin">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="processWindowStyle">
        ///     The window state to use when the process is started.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process has
        ///     been started; otherwise, false.
        /// </param>
        public static Process Send(string command, bool runAsAdmin = false, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Hidden, bool dispose = true)
        {
            try
            {
                var cmd = command.Trim();
                if (cmd.StartsWithEx("/K "))
                    cmd = cmd.Substring(3).TrimStart();
                if (!cmd.StartsWithEx("/C "))
                    cmd = $"/C {cmd}";
                if (cmd.Length < 16)
                    throw new ArgumentNullException(nameof(cmd));
                var path = PathEx.Combine(Resources.CmdPath);
                if ((path + cmd).Length > 8192)
                {
                    var batch = PathEx.Combine(Path.GetTempPath(), PathEx.GetTempFileName("tmp", ".cmd"));
                    var content = cmd.Substring(3).Replace("FOR /L %", "FOR /L %%").Replace("EXIT /B", "EXIT");
                    content = string.Format(Resources.Cmd_Script, content);
                    File.WriteAllText(batch, content);
                    cmd = string.Format(Resources.Cmd_CallPre, batch);
                }
                var psi = new ProcessStartInfo
                {
                    Arguments = cmd,
                    FileName = path,
                    UseShellExecute = runAsAdmin,
                    Verb = runAsAdmin ? "runas" : string.Empty,
                    WindowStyle = processWindowStyle
                };
                var p = Start(psi, dispose);
                if (Log.DebugMode > 1)
                    Log.Write($"COMMAND EXECUTED: {cmd.Substring(3)}");
                return p;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
        }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="Process"/> class to execute system
        ///         commands using the system command prompt ("cmd.exe").
        ///     </para>
        ///     <para>
        ///         This can be useful for an unprivileged application as a simple way to execute a
        ///         command with the highest user permissions, for example.
        ///     </para>
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="runAsAdmin">
        ///     true to start the application with administrator privileges; otherwise, false.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process has
        ///     been started; otherwise, false.
        /// </param>
        public static Process Send(string command, bool runAsAdmin, bool dispose) =>
            Send(command, runAsAdmin, ProcessWindowStyle.Hidden, dispose);

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="Process"/> class to execute system
        ///         commands using the system command prompt ("cmd.exe").
        ///     </para>
        ///     <para>
        ///         This can be useful for an unprivileged application as a simple way to execute a
        ///         command with the highest user permissions, for example.
        ///     </para>
        /// </summary>
        /// <param name="command">
        ///     The command to execute.
        /// </param>
        /// <param name="processWindowStyle">
        ///     The window state to use when the process is started.
        /// </param>
        /// <param name="dispose">
        ///     true to release all resources used by the <see cref="Component"/>, if the process has
        ///     been started; otherwise, false.
        /// </param>
        public static Process Send(string command, ProcessWindowStyle processWindowStyle, bool dispose = true) =>
            Send(command, false, processWindowStyle, dispose);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Process"/> class to execute system commands
        ///     using the system command prompt ("cmd.exe") and stream its output to a console, if
        ///     available.
        /// </summary>
        /// <param name="commands">
        ///     The commands to execute.
        /// </param>
        public static bool SendEx(params string[] commands)
        {
            try
            {
                var path = PathEx.Combine(Resources.CmdPath);
                using (var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        FileName = path,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        Verb = Elevation.IsAdministrator ? "runas" : string.Empty
                    }
                })
                {
                    p.Start();
                    foreach (var s in commands)
                        p.StandardInput.WriteLine(s);
                    p.StandardInput.Flush();
                    p.StandardInput.Close();
                    Console.WriteLine(p.StandardOutput.ReadToEnd());
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Provides the functionality to handle the current principal name.
        /// </summary>
        public static class CurrentPrincipal
        {
            /// <summary>
            ///     <para>
            ///         Gets the original name of the current principal.
            ///     </para>
            ///     <para>
            ///         This variable is only set if <see cref="GetOriginalName"/> was
            ///         previously called.
            ///     </para>
            /// </summary>
            public static string Name { get; private set; }

            private static void GetPointers(out IntPtr offset, out IntPtr buffer)
            {
                var curHandle = Process.GetCurrentProcess().Handle;
                var pebBaseAddress = WinApi.NativeHelper.GetProcessBasicInformation(curHandle).PebBaseAddress;
                var processParameters = Marshal.ReadIntPtr(pebBaseAddress, 4 * IntPtr.Size);
                var unicodeSize = IntPtr.Size * 2;
                offset = processParameters.Increment(new IntPtr(4 * 4 + 5 * IntPtr.Size + unicodeSize + IntPtr.Size + unicodeSize));
                buffer = Marshal.ReadIntPtr(offset, IntPtr.Size);
            }

            /// <summary>
            ///     Changes the name of the current principal.
            /// </summary>
            public static string GetOriginalName()
            {
                if (!string.IsNullOrEmpty(Name))
                    return Name;
                try
                {
                    GetPointers(out var offset, out var buffer);
                    var len = Marshal.ReadInt16(offset);
                    if (string.IsNullOrEmpty(Name))
                        Name = Marshal.PtrToStringUni(buffer, len / 2);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
                return Name;
            }

            /// <summary>
            ///     Changes the name of the current principal.
            /// </summary>
            /// <param name="newName">
            ///     The new name for the current principal, which cannot be longer than
            ///     the original one.
            /// </param>
            public static void ChangeName(string newName)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(newName))
                        throw new ArgumentNullException(nameof(newName));
                    GetPointers(out var offset, out var buffer);
                    var len = Marshal.ReadInt16(offset);
                    if (string.IsNullOrEmpty(Name))
                        Name = Marshal.PtrToStringUni(buffer, len / 2);
                    var principalDir = Path.GetDirectoryName(Name);
                    if (string.IsNullOrEmpty(principalDir))
                        throw new PathNotFoundException(principalDir);
                    var newPrincipalName = Path.Combine(principalDir, newName);
                    if (newPrincipalName.Length > Name.Length)
                        throw new ArgumentException("The new principal name cannot be longer than the original one.");
                    var ptr = buffer;
                    foreach (var c in newPrincipalName)
                    {
                        Marshal.WriteInt16(ptr, c);
                        ptr = ptr.Increment(new IntPtr(2));
                    }
                    Marshal.WriteInt16(ptr, 0);
                    Marshal.WriteInt16(offset, (short)(newPrincipalName.Length * 2));
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }

            /// <summary>
            ///     Restores the name of the current principal.
            /// </summary>
            public static void RestoreName()
            {
                try
                {
                    if (string.IsNullOrEmpty(Name))
                        throw new InvalidOperationException();
                    GetPointers(out var offset, out var buffer);
                    foreach (var c in Name)
                    {
                        Marshal.WriteInt16(buffer, c);
                        buffer = buffer.Increment(new IntPtr(2));
                    }
                    Marshal.WriteInt16(buffer, 0);
                    Marshal.WriteInt16(offset, (short)(Name.Length * 2));
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        /// <summary>
        ///     Provides basic functionality based on <see cref="Send(string,bool,bool)"/>.
        /// </summary>
        public static class SendHelper
        {
            /// <summary>
            ///     Copies an existing file or directory to a new location.
            /// </summary>
            /// <param name="srcPath">
            ///     The path to the file or directory to copy.
            /// </param>
            /// <param name="destPath">
            ///     The name of the destination file or directory.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="wait">
            ///     true to wait indefinitely for the associated process to exit; otherwise, false.
            /// </param>
            public static bool Copy(string srcPath, string destPath, bool runAsAdmin = false, bool wait = true)
            {
                if (string.IsNullOrWhiteSpace(srcPath) || string.IsNullOrWhiteSpace(destPath))
                    return false;
                var src = PathEx.Combine(srcPath);
                if (!PathEx.DirOrFileExists(src))
                    return false;
                var dest = PathEx.Combine(destPath);
                int exitCode;
                using (var p = Send(string.Format(Resources.Cmd_Copy, src, dest), runAsAdmin, false))
                {
                    if (!wait || p?.HasExited != false)
                        return true;
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                }
                return exitCode == 0;
            }

            /// <summary>
            ///     Deletes an existing file or directory.
            /// </summary>
            /// <param name="path">
            ///     The path to the file or directory to be deleted.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="wait">
            ///     true to wait indefinitely for the associated process to exit; otherwise, false.
            /// </param>
            public static bool Delete(string path, bool runAsAdmin = false, bool wait = true)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;
                var fullPath = PathEx.Combine(path);
                if (!PathEx.DirOrFileExists(fullPath))
                    return true;
                int exitCode;
                using (var p = Send(string.Format(PathEx.IsDir(fullPath) ? Resources.Cmd_DeleteDir : Resources.Cmd_DeleteFile, fullPath), runAsAdmin, false))
                {
                    if (!wait || p?.HasExited != false)
                        return true;
                    p.WaitForExit();
                    exitCode = p.ExitCode;
                }
                return exitCode == 0;
            }

            /// <summary>
            ///     Waits before the system is instructed to delete the target at the specified path.
            /// </summary>
            /// <param name="path">
            ///     The path to the file or directory to be deleted.
            /// </param>
            /// <param name="seconds">
            ///     The time to wait in seconds.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process WaitThenDelete(string path, int seconds = 5, bool runAsAdmin = false, bool dispose = true)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;
                var fullPath = PathEx.Combine(path);
                if (!PathEx.DirOrFileExists(fullPath))
                    return null;
                var time = seconds < 1 ? 1 : seconds > 3600 ? 3600 : seconds;
                var command = string.Format(PathEx.IsDir(fullPath) ? Resources.Cmd_DeleteDir : Resources.Cmd_DeleteFile, fullPath);
                command = string.Format(Resources.Cmd_WaitThenCmd, time, command);
                return Send(command, runAsAdmin, dispose);
            }

            /// <summary>
            ///     <para>
            ///         Deletes the target at the specified path if there is no process running that
            ///         is matched with the specified process name.
            ///     </para>
            ///     <para>
            ///         If a matched process is still running, the task will wait until all matched
            ///         processes has been closed.
            ///     </para>
            /// </summary>
            /// <param name="path">
            ///     The path to the file or directory to be deleted.
            /// </param>
            /// <param name="processName">
            ///     The name of the process to be waited.
            /// </param>
            /// <param name="extension">
            ///     The file extension of the specified process.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process WaitForExitThenDelete(string path, string processName, string extension, bool runAsAdmin = false, bool dispose = true)
            {
                if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(processName))
                    return null;
                var fullPath = PathEx.Combine(path);
                if (!PathEx.DirOrFileExists(fullPath))
                    return null;
                var name = processName;
                if (!string.IsNullOrEmpty(extension) && !name.EndsWithEx(extension))
                    name += extension;
                var command = string.Format(PathEx.IsDir(fullPath) ? Resources.Cmd_DeleteDir : Resources.Cmd_DeleteFile, fullPath);
                command = string.Format(Resources.Cmd_WaitForProcThenCmd, name, command);
                return Send(command, runAsAdmin, dispose);
            }

            /// <summary>
            ///     <para>
            ///         Deletes the target at the specified path if there is no process running that
            ///         is matched with the specified process name.
            ///     </para>
            ///     <para>
            ///         If a matched process is still running, the task will wait until all matched
            ///         processes has been closed.
            ///     </para>
            /// </summary>
            /// <param name="path">
            ///     The path to the file or directory to be deleted.
            /// </param>
            /// <param name="processName">
            ///     The name of the process to be waited.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process WaitForExitThenDelete(string path, string processName, bool runAsAdmin = false, bool dispose = true) =>
                WaitForExitThenDelete(path, processName, ".exe", runAsAdmin, dispose);

            /// <summary>
            ///     Ends all processes matched by the specified process name.
            /// </summary>
            /// <param name="processName">
            ///     The name of the process to be killed.
            /// </param>
            /// <param name="extension">
            ///     The file extension of the specified process.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process KillTask(string processName, string extension, bool runAsAdmin = false, bool dispose = true)
            {
                if (string.IsNullOrWhiteSpace(processName))
                    return null;
                var name = processName;
                if (!string.IsNullOrEmpty(extension) && !name.EndsWithEx(extension))
                    name += extension;
                var command = string.Format(Resources.Cmd_Terminate, name);
                return Send(command, runAsAdmin, dispose);
            }

            /// <summary>
            ///     Ends all processes matched by the specified process name.
            /// </summary>
            /// <param name="processName">
            ///     The name of the process to be killed.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process KillTask(string processName, bool runAsAdmin = false, bool dispose = true) =>
                KillTask(processName, ".exe", runAsAdmin, dispose);

            /// <summary>
            ///     Ends all processes matched by all the specified process names.
            /// </summary>
            /// <param name="processNames">
            ///     A list of the process names to be killed.
            /// </param>
            /// <param name="extension">
            ///     The file extension of the specified processes.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process KillAllTasks(IEnumerable<string> processNames, string extension, bool runAsAdmin = false, bool dispose = true)
            {
                if (processNames == null)
                    return null;
                var names = processNames.Where(Comparison.IsNotEmpty);
                if (!string.IsNullOrWhiteSpace(extension))
                    names = names.Select(x => !x.EndsWithEx(extension) ? x + extension : x);
                var command = string.Format(Resources.Cmd_Terminate, names.Join(Resources.Cmd_TerminateJoin));
                return Send(command, runAsAdmin, dispose);
            }

            /// <summary>
            ///     Ends all processes matched by all the specified process names.
            /// </summary>
            /// <param name="processNames">
            ///     A list of the process names to be killed.
            /// </param>
            /// <param name="runAsAdmin">
            ///     true to run this task with administrator privileges; otherwise, false.
            /// </param>
            /// <param name="dispose">
            ///     true to release all resources used by the <see cref="Component"/>, if this task has
            ///     been started; otherwise, false.
            /// </param>
            public static Process KillAllTasks(IEnumerable<string> processNames, bool runAsAdmin = false, bool dispose = true) =>
                KillAllTasks(processNames, ".exe", runAsAdmin, dispose);
        }
    }
}
