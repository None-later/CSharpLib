#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: NetEx.cs
// Version:  2017-05-03 05:26
// 
// Copyright (c) 2017, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;

    /// <summary>
    ///     Provides functionality for the access of internet resources.
    /// </summary>
    public static class NetEx
    {
        private class WebClientEx : WebClient
        {
            private int Timeout { get; }
            internal WebClientEx() : this(60000) { }

            internal WebClientEx(int timeout)
            {
                Timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request != null)
                    request.Timeout = Timeout;
                return request;
            }
        }

        /// <summary>
        ///     Gets the last result defined in the previous call to the <see cref="Ping(Uri)"/> function.
        /// </summary>
        public static PingReply LastPingReply { get; private set; }

        /// <summary>
        ///     Checks the current network connection.
        /// </summary>
        /// <param name="iPv6">
        ///     true to check only the IPv6 protocol; otherwise, false to check only the IPv4 protocol.
        /// </param>
        public static bool InternetIsAvailable(bool iPv6 = false)
        {
            bool isAvailable;
            try
            {
                isAvailable = NetworkInterface.GetAllNetworkInterfaces().Any(x => x.OperationalStatus == OperationalStatus.Up);
                if (!isAvailable)
                    return false;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            isAvailable = false;
            var dns = new[,]
            {
                // Google Public DNS IPv4 addresses
                {
                    "8.8.8.8",
                    "8.8.4.4"
                },
                // Google Public DNS IPv6 addresses
                {
                    "[2001:4860:4860::8888]",
                    "[2001:4860:4860::8844]"
                }
            };
            for (var i = 0; i < dns.GetLength(0); i++)
            {
                var s = dns[Convert.ToByte(iPv6), i];
                isAvailable = Ping(s) < 3000;
                if (isAvailable)
                    break;
            }
            return isAvailable;
        }

        /// <summary>
        ///     Attempts to send an Internet Control Message Protocol (ICMP) echo message to the specified
        ///     computer, and receive a corresponding ICMP echo replay message from that computer and
        ///     returns the number of milliseconds taken for this task.
        /// </summary>
        /// <param name="uri">
        ///     The address of the server to call.
        /// </param>
        public static long Ping(Uri uri)
        {
            long roundtripTime = 3000;
            try
            {
                using (var ping = new Ping())
                {
                    LastPingReply = ping.Send(uri.Host, 3000);
                    if (LastPingReply?.Status == IPStatus.Success)
                        roundtripTime = LastPingReply.RoundtripTime;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return roundtripTime;
        }

        /// <summary>
        ///     Attempts to send an Internet Control Message Protocol (ICMP) echo message to the specified
        ///     computer, and receive a corresponding ICMP echo replay message from that computer and
        ///     returns the number of milliseconds taken for this task.
        /// </summary>
        /// <param name="host">
        ///     The address of the server to call.
        /// </param>
        public static long Ping(string host) =>
            Ping(host.ToHttpUri());

        /// <summary>
        ///     Converts this <see cref="string"/> to a <see cref="Uri"/>.
        /// </summary>
        /// <param name="str">
        ///     The <see cref="string"/> to convert.
        /// </param>
        public static Uri ToUri(this string str)
        {
            try
            {
                var u = new Uri(str);
                return u;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Converts this <see cref="string"/> to a <see cref="Uri"/> with HTTP scheme.
        /// </summary>
        /// <param name="str">
        ///     The <see cref="string"/> to convert.
        /// </param>
        public static Uri ToHttpUri(this string str)
        {
            try
            {
                var s = str;
                var u = s.ToUri();
                if (u.Scheme.EqualsEx("https", "http"))
                    return u;
                s = PathEx.AltCombine("http:", u.Host, u.PathAndQuery);
                u = s.ToUri();
                return u;
            }
            catch
            {
                try
                {
                    var s = str;
                    if (s.Contains(":/"))
                    {
                        var i = s.IndexOf(":/", StringComparison.Ordinal) + 1;
                        if (i.IsBetween(4, 6) && i < s.Length)
                            s = s.Substring(i).TrimStart('/');
                    }
                    var u = PathEx.AltCombine("http:", s).ToUri();
                    return u;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return null;
                }
            }
        }

        /// <summary>
        ///     Determines the availability of the specified internet address.
        /// </summary>
        /// <param name="uri">
        ///     The address to check.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static bool IsValid(this Uri uri, int timeout = 3000, string userAgent = null)
        {
            var statusCode = 500;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "HEAD";
                if (!string.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;
                request.Timeout = timeout;
                using (var response = (HttpWebResponse)request.GetResponse())
                    statusCode = (int)response.StatusCode;
                if (statusCode >= 500 && statusCode <= 510)
                    throw new HttpListenerException();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return statusCode >= 100 && statusCode < 400;
        }

        /// <summary>
        ///     Determines the availability of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userName">
        ///     The username associated with the credential.
        /// </param>
        /// <param name="password">
        ///     The password associated with the credential.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static bool FileIsAvailable(this Uri srcUri, string userName = null, string password = null, int timeout = 3000, string userAgent = null)
        {
            long contentLength = 0;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(srcUri);
                if (!string.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;
                request.Timeout = timeout;
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    request.Credentials = new NetworkCredential(userName, password);
                using (var response = (HttpWebResponse)request.GetResponse())
                    contentLength = response.ContentLength;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return contentLength > 0;
        }

        /// <summary>
        ///     Determines the availability of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static bool FileIsAvailable(this Uri srcUri, int timeout, string userAgent = null) =>
            srcUri.FileIsAvailable(null, null, timeout, userAgent);

        /// <summary>
        ///     Determines the availability of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userName">
        ///     The username associated with the credential.
        /// </param>
        /// <param name="password">
        ///     The password associated with the credential.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static bool FileIsAvailable(string srcUri, string userName = null, string password = null, int timeout = 3000, string userAgent = null) =>
            srcUri.ToHttpUri().FileIsAvailable(userName, password, timeout, userAgent);

        /// <summary>
        ///     Determines the availability of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static bool FileIsAvailable(string srcUri, int timeout, string userAgent = null) =>
            srcUri.ToHttpUri().FileIsAvailable(null, null, timeout, userAgent);

        /// <summary>
        ///     Gets the last date and time of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userName">
        ///     The username associated with the credential.
        /// </param>
        /// <param name="password">
        ///     The password associated with the credential.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static DateTime GetFileDate(this Uri srcUri, string userName = null, string password = null, int timeout = 3000, string userAgent = null)
        {
            var lastModified = DateTime.Now;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(srcUri);
                if (!string.IsNullOrEmpty(userAgent))
                    request.UserAgent = userAgent;
                request.Timeout = timeout;
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                    request.Credentials = new NetworkCredential(userName, password);
                using (var response = (HttpWebResponse)request.GetResponse())
                    lastModified = response.LastModified;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return lastModified;
        }

        /// <summary>
        ///     Gets the last date and time of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static DateTime GetFileDate(this Uri srcUri, int timeout, string userAgent = null) =>
            srcUri.GetFileDate(null, null, timeout, userAgent);

        /// <summary>
        ///     Gets the last date and time of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userName">
        ///     The username associated with the credential.
        /// </param>
        /// <param name="password">
        ///     The password associated with the credential.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static DateTime GetFileDate(string srcUri, string userName = null, string password = null, int timeout = 3000, string userAgent = null) =>
            srcUri.ToHttpUri().GetFileDate(userName, password, timeout, userAgent);

        /// <summary>
        ///     Gets the last date and time of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="timeout">
        ///     The time-out value in milliseconds.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static DateTime GetFileDate(string srcUri, int timeout, string userAgent = null) =>
            srcUri.ToHttpUri().GetFileDate(null, null, timeout, userAgent);

        /// <summary>
        ///     Gets the filename of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static string GetFileName(this Uri srcUri, string userAgent = null)
        {
            var name = string.Empty;
            try
            {
                using (var client = new WebClient())
                {
                    if (!string.IsNullOrEmpty(userAgent))
                        client.Headers.Add("User-Agent", userAgent);
                    using (client.OpenRead(srcUri))
                    {
                        var cd = client.ResponseHeaders["content-disposition"];
                        if (!string.IsNullOrWhiteSpace(cd))
                        {
                            var i = cd.IndexOf("filename=", StringComparison.OrdinalIgnoreCase);
                            if (i >= 0)
                                name = cd.Substring(i + 10);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            return name;
        }

        /// <summary>
        ///     Gets the filename of the specified internet resource.
        /// </summary>
        /// <param name="srcUri">
        ///     The full path of the resource to access.
        /// </param>
        /// <param name="userAgent">
        ///     The value of the User-agent HTTP header.
        /// </param>
        public static string GetFileName(string srcUri, string userAgent = null) =>
            srcUri.ToHttpUri().GetFileName(userAgent);

        /// <summary>
        ///     Provides static methods for downloading internet resources.
        /// </summary>
        public static class Transfer
        {
            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static bool DownloadFile(Uri srcUri, string destPath, string userName = null, string password = null, int timeout = 60000, string userAgent = null)
            {
                try
                {
                    var path = PathEx.Combine(destPath);
                    if (File.Exists(path))
                        File.Delete(path);
                    if (!FileIsAvailable(srcUri, userName, password, timeout, userAgent))
                        throw new PathNotFoundException(srcUri.ToString());
                    using (var wc = new WebClientEx(timeout))
                    {
                        if (!string.IsNullOrEmpty(userAgent))
                            wc.Headers.Add("User-Agent", userAgent);
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        wc.DownloadFile(srcUri, path);
                    }
                    return File.Exists(path);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return false;
                }
            }

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static bool DownloadFile(Uri srcUri, string destPath, int timeout, string userAgent = null) =>
                DownloadFile(srcUri, destPath, null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static bool DownloadFile(string srcUri, string destPath, string userName = null, string password = null, int timeout = 60000, string userAgent = null) =>
                DownloadFile(srcUri.ToHttpUri(), destPath, userName, password, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static bool DownloadFile(string srcUri, string destPath, int timeout, string userAgent = null) =>
                DownloadFile(srcUri.ToHttpUri(), destPath, null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="byte"/> array.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static byte[] DownloadData(Uri srcUri, string userName = null, string password = null, int timeout = 60000, string userAgent = null)
            {
                try
                {
                    byte[] ba;
                    using (var wc = new WebClientEx(timeout))
                    {
                        if (!string.IsNullOrEmpty(userAgent))
                            wc.Headers.Add("User-Agent", userAgent);
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        ba = wc.DownloadData(srcUri);
                    }
                    if (ba == null)
                        throw new ArgumentNullException(nameof(ba));
                    return ba;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return null;
                }
            }

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="byte"/> array.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static byte[] DownloadData(Uri srcUri, int timeout, string userAgent = null) =>
                DownloadData(srcUri, null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="byte"/> array.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static byte[] DownloadData(string srcUri, string userName = null, string password = null, int timeout = 60000, string userAgent = null) =>
                DownloadData(srcUri.ToHttpUri(), userName, password, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="byte"/> array.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static byte[] DownloadData(string srcUri, int timeout, string userAgent = null) =>
                DownloadData(srcUri.ToHttpUri(), null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="string"/>.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static string DownloadString(Uri srcUri, string userName = null, string password = null, int timeout = 60000, string userAgent = null)
            {
                try
                {
                    string s;
                    using (var wc = new WebClient())
                    {
                        if (!string.IsNullOrEmpty(userAgent))
                            wc.Headers.Add("User-Agent", userAgent);
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            wc.Credentials = new NetworkCredential(userName, password);
                        s = wc.DownloadString(srcUri);
                    }
                    if (string.IsNullOrEmpty(s))
                        throw new ArgumentNullException(nameof(s));
                    return s;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return string.Empty;
                }
            }

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="string"/>.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static string DownloadString(Uri srcUri, int timeout, string userAgent = null) =>
                DownloadString(srcUri, null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="string"/>.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static string DownloadString(string srcUri, string userName = null, string password = null, int timeout = 60000, string userAgent = null) =>
                DownloadString(srcUri.ToHttpUri(), userName, password, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource as a <see cref="string"/>.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public static string DownloadString(string srcUri, int timeout, string userAgent = null) =>
                DownloadString(srcUri.ToHttpUri(), null, null, timeout, userAgent);
        }

        /// <summary>
        ///     Provides asynchonous downloading of internet resources.
        /// </summary>
        public sealed class AsyncTransfer
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private WebClient _webClient;

            /// <summary>
            ///     Gets the address to the resource.
            /// </summary>
            public Uri Address { get; private set; }

            /// <summary>
            ///     Gets the local path of the resource.
            /// </summary>
            public string FilePath { get; private set; }

            /// <summary>
            ///     Gets the number of bytes received.
            /// </summary>
            public long BytesReceived { get; private set; }

            /// <summary>
            ///     Gets the total number of bytes received.
            /// </summary>
            public long TotalBytesToReceive { get; private set; }

            /// <summary>
            ///     Gets the total number of <see cref="BytesReceived"/> and <see cref="BytesReceived"/>
            ///     received in megabyte.
            /// </summary>
            public string DataReceived
            {
                get
                {
                    var current = BytesReceived.FormatDataSize(Reorganize.SizeUnits.MB);
                    var total = TotalBytesToReceive.FormatDataSize(Reorganize.SizeUnits.MB);
                    return current + " / " + total;
                }
            }

            /// <summary>
            ///     Gets the asynchronous progress percentage.
            /// </summary>
            public int ProgressPercentage { get; private set; }

            /// <summary>
            ///     Gets the megabyte per second of the asynchronous transfer.
            /// </summary>
            public double TransferSpeed { get; private set; }

            /// <summary>
            ///     Gets the string representation of the speed of the asynchronous transfer.
            /// </summary>
            public string TransferSpeedAd { get; private set; } = "0,00 bit/s";

            /// <summary>
            ///     Gets the total elapsed time.
            /// </summary>
            public TimeSpan TimeElapsed { get; private set; } = TimeSpan.MinValue;

            /// <summary>
            ///     Determines whether the transfer has been canceled.
            /// </summary>
            public bool HasCanceled { get; set; } = true;

            /// <summary>
            ///     Gets whether a transfer is in progress.
            /// </summary>
            public bool IsBusy
            {
                get
                {
                    try
                    {
                        return _webClient.IsBusy;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public void DownloadFile(Uri srcUri, string destPath, string userName = null, string password = null, int timeout = 60000, string userAgent = null)
            {
                try
                {
                    if (IsBusy)
                        throw new NotSupportedException("Async file download is already busy, multiple calls are not allowed.");
                    var path = PathEx.Combine(destPath);
                    if (File.Exists(path))
                        File.Delete(path);
                    using (_webClient = new WebClientEx(timeout))
                    {
                        if (!string.IsNullOrEmpty(userAgent))
                            _webClient.Headers.Add("User-Agent", userAgent);
                        _webClient.DownloadFileCompleted += DownloadFile_Completed;
                        _webClient.DownloadProgressChanged += DownloadFile_ProgressChanged;
                        Address = srcUri;
                        FilePath = path;
                        var exists = FileIsAvailable(Address, userName, password, timeout, userAgent);
                        if (!exists)
                            throw new PathNotFoundException(srcUri.ToString());
                        if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                            _webClient.Credentials = new NetworkCredential(userName, password);
                        _webClient.DownloadFileAsync(Address, FilePath);
                        _stopwatch.Start();
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    HasCanceled = true;
                    _stopwatch.Reset();
                }
            }

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public void DownloadFile(Uri srcUri, string destPath, int timeout, string userAgent = null) =>
                DownloadFile(srcUri, destPath, null, null, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="userName">
            ///     The username associated with the credential.
            /// </param>
            /// <param name="password">
            ///     The password associated with the credential.
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public void DownloadFile(string srcUri, string destPath, string userName = null, string password = null, int timeout = 60000, string userAgent = null) =>
                DownloadFile(srcUri.ToHttpUri(), destPath, userName, password, timeout, userAgent);

            /// <summary>
            ///     Downloads the specified internet resource to a local file.
            /// </summary>
            /// <param name="srcUri">
            ///     The full path of the resource to download.
            /// </param>
            /// <param name="destPath">
            ///     The local destination path of the file (environment variables are accepted).
            /// </param>
            /// <param name="timeout">
            ///     The time-out value in milliseconds.
            /// </param>
            /// <param name="userAgent">
            ///     The value of the User-agent HTTP header.
            /// </param>
            public void DownloadFile(string srcUri, string destPath, int timeout, string userAgent = null) =>
                DownloadFile(srcUri.ToHttpUri(), destPath, null, null, timeout, userAgent);

            private void DownloadFile_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                try
                {
                    BytesReceived = e.BytesReceived;
                    TotalBytesToReceive = e.TotalBytesToReceive;
                    ProgressPercentage = e.ProgressPercentage;
                    TimeElapsed = _stopwatch.Elapsed;
                    TransferSpeed = e.BytesReceived;
                    if (e.BytesReceived <= 0)
                        return;
                    TransferSpeed = TransferSpeed / 1000 / TimeElapsed.TotalSeconds;
                    TransferSpeedAd = $"{((long)(e.BytesReceived / TimeElapsed.TotalSeconds)).FormatDataSize(false).ToLower()}it/s";
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    TimeElapsed = _stopwatch.Elapsed;
                    HasCanceled = true;
                    _stopwatch.Reset();
                }
            }

            private void DownloadFile_Completed(object sender, AsyncCompletedEventArgs e)
            {
                _stopwatch.Reset();
                if (e.Cancelled)
                {
                    HasCanceled = true;
                    _webClient.Dispose();
                    try
                    {
                        if (File.Exists(FilePath))
                            File.Delete(FilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex);
                    }
                }
                else
                {
                    if (File.Exists(FilePath))
                        BytesReceived = new FileInfo(FilePath).Length;
                    HasCanceled = !(File.Exists(FilePath) && BytesReceived == TotalBytesToReceive);
                }
            }

            /// <summary>
            ///     Cancels a pending asynchonous transfer.
            /// </summary>
            public void CancelAsync()
            {
                if (IsBusy)
                    _webClient.CancelAsync();
            }
        }
    }
}
