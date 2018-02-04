﻿#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: DirectoryEx.cs
// Version:  2018-02-04 07:35
// 
// Copyright (c) 2018, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Provides static methods based on the <see cref="Directory"/> class to perform
    ///     directory operations.
    /// </summary>
    public static class DirectoryEx
    {
        /// <summary>
        ///     Determines whether the specified path specifies the specified attributes.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member that contains the directory to check.
        /// </param>
        /// <param name="attr">
        ///     The attributes to match.
        /// </param>
        public static bool MatchAttributes(this DirectoryInfo dirInfo, FileAttributes attr)
        {
            try
            {
                if (!dirInfo.Exists)
                    return false;
                var da = dirInfo.Attributes;
                return (da & attr) != 0;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Determines whether the specified path specifies the specified attributes.
        /// </summary>
        /// <param name="path">
        ///     The directory to check.
        /// </param>
        /// <param name="attr">
        ///     The attributes to match.
        /// </param>
        public static bool MatchAttributes(string path, FileAttributes attr)
        {
            try
            {
                var src = PathEx.Combine(path);
                var di = new DirectoryInfo(src);
                return di.MatchAttributes(attr);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Determines whether the specified directory is hidden.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member that contains the directory to check.
        /// </param>
        public static bool IsHidden(this DirectoryInfo dirInfo) =>
            dirInfo?.MatchAttributes(FileAttributes.Hidden) == true;

        /// <summary>
        ///     Determines whether the specified directory is hidden.
        /// </summary>
        /// <param name="path">
        ///     The file to check.
        /// </param>
        public static bool IsHidden(string path) =>
            MatchAttributes(path, FileAttributes.Hidden);

        /// <summary>
        ///     Determines whether the specified directory is specified as reparse point.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member that contains the directory to check.
        /// </param>
        public static bool IsLink(this DirectoryInfo dirInfo) =>
            dirInfo?.MatchAttributes(FileAttributes.ReparsePoint) == true;

        /// <summary>
        ///     Determines whether the specified directory is specified as reparse point.
        /// </summary>
        /// <param name="path">
        ///     The directory to check.
        /// </param>
        public static bool IsLink(string path)
        {
            try
            {
                var src = PathEx.Combine(path);
                var di = new DirectoryInfo(src);
                return di.IsLink();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Sets the specified attributes for the specified directory.
        /// </summary>
        /// <param name="path">
        ///     The directory to change.
        /// </param>
        /// <param name="attr">
        ///     The attributes to set.
        /// </param>
        public static void SetAttributes(string path, FileAttributes attr)
        {
            try
            {
                var src = PathEx.Combine(path);
                var di = new DirectoryInfo(src);
                if (!di.Exists)
                    return;
                if (attr != FileAttributes.Normal)
                    di.Attributes |= attr;
                else
                    di.Attributes = attr;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        ///     Copies an existing directory to a new location.
        /// </summary>
        /// <param name="srcDir">
        ///     The directory to copy.
        /// </param>
        /// <param name="destDir">
        ///     The fully qualified name of the destination directory.
        /// </param>
        /// <param name="subDirs">
        ///     true to inlcude subdirectories; otherwise, false.
        /// </param>
        /// <param name="overwrite">
        ///     true to allow existing files to be overwritten; otherwise, false.
        /// </param>
        public static bool Copy(string srcDir, string destDir, bool subDirs = true, bool overwrite = false)
        {
            var src = PathEx.Combine(srcDir);
            try
            {
                var di = new DirectoryInfo(src);
                if (!di.Exists)
                    throw new PathNotFoundException(di.FullName);
                var dest = PathEx.Combine(destDir);
                if (!Directory.Exists(dest))
                    Directory.CreateDirectory(dest);
                foreach (var f in di.GetFiles())
                {
                    var p = Path.Combine(dest, f.Name);
                    if (File.Exists(p) && !overwrite)
                        continue;
                    f.CopyTo(p, overwrite);
                }
                if (!subDirs)
                    return true;
                if (di.EnumerateDirectories().Any(x => !x.Copy(Path.Combine(dest, x.Name), true, overwrite)))
                    throw new OperationCanceledException();
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Copies an existing directory to a new location.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member that contains the directory to copy.
        /// </param>
        /// <param name="destDir">
        ///     The fully qualified name of the destination directory.
        /// </param>
        /// <param name="subDirs">
        ///     true to inlcude subdirectories; otherwise, false.
        /// </param>
        /// <param name="overwrite">
        ///     true to allow existing files to be overwritten; otherwise, false.
        /// </param>
        public static bool Copy(this DirectoryInfo dirInfo, string destDir, bool subDirs = true, bool overwrite = false) =>
            Copy(dirInfo?.FullName, destDir, subDirs, overwrite);

        /// <summary>
        ///     Copies an existing directory to a new location and deletes the source
        ///     directory if this task has been completed successfully.
        /// </summary>
        /// <param name="srcDir">
        ///     The directory to move.
        /// </param>
        /// <param name="destDir">
        ///     The fully qualified name of the destination directory.
        /// </param>
        /// <param name="overwrite">
        ///     true to allow existing files to be overwritten; otherwise, false.
        /// </param>
        public static bool Move(string srcDir, string destDir, bool overwrite = false)
        {
            if (!Copy(srcDir, destDir, overwrite))
                return false;
            var src = PathEx.Combine(srcDir);
            var dest = PathEx.Combine(destDir);
            try
            {
                if (!overwrite || GetFullHashCode(src) != GetFullHashCode(dest))
                    throw new AggregateException();
                Directory.Delete(src, true);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        /// <summary>
        ///     Deletes the specified directory, if it exists.
        /// </summary>
        /// <param name="path">
        ///     The path of the directory to be deleted.
        /// </param>
        /// <exception cref="IOException">
        ///     See <see cref="Directory.Delete(string, bool)"/>.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        ///     See <see cref="Directory.Delete(string, bool)"/>.
        /// </exception>
        public static bool Delete(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            var dir = PathEx.Combine(path);
            if (!Directory.Exists(dir))
                return true;
            Directory.Delete(dir, true);
            return true;
        }

        /// <summary>
        ///     Tries to delete the specified directory.
        /// </summary>
        /// <param name="path">
        ///     The path of the directory to be deleted.
        /// </param>
        public static bool TryDelete(string path)
        {
            try
            {
                return Delete(path);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Returns the hash code for the specified directory instance member.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member to get the hash code.
        /// </param>
        /// <param name="size">
        ///     true to include the size of each file; otherwise, false.
        /// </param>
        public static int GetFullHashCode(this DirectoryInfo dirInfo, bool size = true)
        {
            try
            {
                var sb = new StringBuilder();
                long len = 0;
                foreach (var fi in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    sb.Append(fi.Name);
                    if (size)
                        len += fi.Length;
                }
                return size ? $"{len}{sb}".GetHashCode() : sb.ToString().GetHashCode();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        /// <summary>
        ///     Returns the hash code for the specified directory instance member.
        /// </summary>
        /// <param name="dir">
        ///     The directory to get the hash code.
        /// </param>
        /// <param name="size">
        ///     true to include the size of each file; otherwise, false.
        /// </param>
        public static int GetFullHashCode(string dir, bool size = true)
        {
            try
            {
                var di = new DirectoryInfo(dir);
                return di.GetFullHashCode(size);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return -1;
            }
        }

        /// <summary>
        ///     Gets the full size, in bytes, of the specified directory instance member.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance to get the size.
        /// </param>
        public static long GetSize(this DirectoryInfo dirInfo)
        {
            try
            {
                var size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                return size;
            }
            catch (OverflowException)
            {
                return long.MaxValue;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        ///     Gets the full size, in bytes, of the specified directory instance member.
        /// </summary>
        /// <param name="dir">
        ///     The directory instance to get the size.
        /// </param>
        public static long GetSize(string dir)
        {
            try
            {
                var di = new DirectoryInfo(dir);
                return di.GetSize();
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        ///     Creates a link to the specified directory.
        /// </summary>
        /// <param name="targetPath">
        ///     The directory to be linked.
        /// </param>
        /// <param name="linkPath">
        ///     The fully qualified name of the new link.
        /// </param>
        /// <param name="startArgs">
        ///     The arguments which applies when this shortcut is executed.
        /// </param>
        /// <param name="linkIcon">
        ///     The icon resource path for this shortcut.
        /// </param>
        /// <param name="skipExists">
        ///     true to skip existing shortcuts, even if the target path of
        ///     the same; otherwise, false.
        /// </param>
        public static bool CreateShortcut(string targetPath, string linkPath, string startArgs = null, string linkIcon = null, bool skipExists = false) =>
            PathEx.IsDir(targetPath) && PathEx.CreateShortcut(targetPath, linkPath, startArgs, linkIcon, skipExists);

        /// <summary>
        ///     Creates a link to the specified path.
        /// </summary>
        /// <param name="targetPath">
        ///     The directory to be linked.
        /// </param>
        /// <param name="linkPath">
        ///     The fully qualified name of the new link.
        /// </param>
        /// <param name="startArgs">
        ///     The arguments which applies when this shortcut is executed.
        /// </param>
        /// <param name="skipExists">
        ///     true to skip existing shortcuts, even if the target path of
        ///     the same; otherwise, false.
        /// </param>
        public static bool CreateShortcut(string targetPath, string linkPath, string startArgs, bool skipExists) =>
            CreateShortcut(targetPath, linkPath, startArgs, null, skipExists);

        /// <summary>
        ///     Creates a link to the specified path.
        /// </summary>
        /// <param name="targetPath">
        ///     The directory to be linked.
        /// </param>
        /// <param name="linkPath">
        ///     The fully qualified name of the new link.
        /// </param>
        /// <param name="skipExists">
        ///     true to skip existing shortcuts, even if the target path of
        ///     the same; otherwise, false.
        /// </param>
        public static bool CreateShortcut(string targetPath, string linkPath, bool skipExists) =>
            CreateShortcut(targetPath, linkPath, null, null, skipExists);

        /// <summary>
        ///     Returns the target path of the specified link.
        ///     <para>
        ///         The target path is returned only if the specified target is an existing
        ///         directory.
        ///     </para>
        /// </summary>
        /// <param name="path">
        ///     The shortcut path to get the target path.
        /// </param>
        public static string GetShortcutTarget(string path)
        {
            var target = PathEx.GetShortcutTarget(path);
            return PathEx.IsDir(target) ? target : string.Empty;
        }

        /// <summary>
        ///     Creates a symbolic link to the specified directory based on command prompt which
        ///     allows a simple solution for the elevated execution of this order.
        /// </summary>
        /// <param name="linkPath">
        ///     The fully qualified name of the new link.
        /// </param>
        /// <param name="srcDir">
        ///     The directory to be linked.
        /// </param>
        /// <param name="backup">
        ///     true to create an backup for existing files; otherwise, false.
        /// </param>
        /// <param name="elevated">
        ///     true to create this link with highest privileges; otherwise, false.
        /// </param>
        public static bool CreateSymbolicLink(string linkPath, string srcDir, bool backup = false, bool elevated = false) =>
            PathEx.CreateSymbolicLink(linkPath, srcDir, true, backup, elevated);

        /// <summary>
        ///     Removes an symbolic link of the specified directory link based on command prompt
        ///     which allows a simple solution for the elevated execution of this order.
        /// </summary>
        /// <param name="path">
        ///     The link to be removed.
        /// </param>
        /// <param name="backup">
        ///     true to restore found backups; otherwise, false.
        /// </param>
        /// <param name="elevated">
        ///     true to remove this link with highest privileges; otherwise, false.
        /// </param>
        public static bool DestroySymbolicLink(string path, bool backup = false, bool elevated = false) =>
            PathEx.DestroySymbolicLink(path, true, backup, elevated);

        /// <summary>
        ///     Find out which processes have a lock on the files of this directory instance member.
        /// </summary>
        /// <param name="dirInfo">
        ///     The directory instance member to check.
        /// </param>
        public static List<Process> GetLocks(this DirectoryInfo dirInfo)
        {
            var list = new List<Process>();
            try
            {
                var sa = Directory.GetFiles(dirInfo.FullName, "*", SearchOption.AllDirectories);
                list = PathEx.GetLocks(sa);
                if (list.Any())
                    list = list.Distinct().ToList();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return list;
        }
    }
}
