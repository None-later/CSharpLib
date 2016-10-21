#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: ResourcesEx.cs
// Version:  2016-10-21 08:27
// 
// Copyright (c) 2016, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    ///     Provides static methods for the usage of data resources.
    /// </summary>
    public static class ResourcesEx
    {
        /// <summary>
        ///     Provides enumerates values for the symbol index of the system file
        ///     "imageres.dll".
        /// </summary>
        public enum ImageresIconIndex : uint
        {
            Asterisk = 0x4c,
            Barrier = 0x51,
            BmpFile = 0x42,
            Cam = 0x29,
            Cd = 0x38,
            CdR = 0x39,
            CdRom = 0x3a,
            CdRw = 0x3b,
            Chip = 0x1d,
            Clipboard = 0xf1,
            Close = 0xeb,
            CommandPrompt = 0x106,
            Computer = 0x68,
            Defrag = 0x6a,
            Desktop = 0x69,
            Directory = 0x3,
            DirectorySearch = 0xd,
            DiscDrive = 0x19,
            DllFile = 0x3e,
            Dvd = 0x33,
            DvdDrive = 0x20,
            DvdR = 0x21,
            DvdRam = 0x22,
            DvdRom = 0x23,
            DvdRw = 0x24,
            Eject = 0xa7,
            Error = 0x5d,
            ExeFile = 0xb,
            Explorer = 0xcb,
            Favorite = 0xcc,
            FloppyDrive = 0x17,
            Games = 0xa,
            HardDrive = 0x1e,
            Help = 0x5e,
            HelpShield = 0x63,
            InfFile = 0x40,
            Install = 0x52,
            JpgFile = 0x43,
            Key = 0x4d,
            Network = 0xaa,
            OneDrive = 0xdc,
            Play = 0x118,
            Pin = 0xea,
            PngFile = 0x4e,
            Printer = 0x2e,
            Question = 0x5e,
            RecycleBinEmpty = 0x32,
            RecycleBinFull = 0x31,
            Retry = 0xfb,
            Run = 0x5f,
            Screensaver = 0x60,
            Search = 0xa8,
            Security = 0x36,
            SharedMarker = 0x9b,
            Sharing = 0x53,
            ShortcutMarker = 0x9a,
            Stop = 0xcf,
            SystemControl = 0x16,
            SystemDrive = 0x1f,
            TaskManager = 0x90,
            Undo = 0xff,
            UnknownDrive = 0x46,
            Unpin = 0xe9,
            User = 0xd0,
            UserDir = 0x75,
            Uac = 0x49,
            Warning = 0x4f,
            ZipFile = 0xa5
        }

        /// <summary>
        ///     Returns the specified <see cref="Icon"/> resource of a file.
        /// </summary>
        /// <param name="path">
        ///     The file to read.
        /// </param>
        /// <param name="index">
        ///     The index of the icon to extract.
        /// </param>
        /// <param name="large">
        ///     true to return the large image; otherwise, false.
        /// </param>
        public static Icon GetIconFromFile(string path, int index = 0, bool large = false)
        {
            try
            {
                path = PathEx.Combine(path);
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException();
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                var ptrs = new IntPtr[1];
                var file = PathEx.Combine(path);
                if (!File.Exists(file))
                    throw new FileNotFoundException();
                WinApi.UnsafeNativeMethods.ExtractIconEx(file, index, large ? ptrs : new IntPtr[1], !large ? ptrs : new IntPtr[1], 1);
                var ptr = ptrs[0];
                if (ptr == IntPtr.Zero)
                    throw new ArgumentNullException();
                var ico = (Icon)Icon.FromHandle(ptr).Clone();
                WinApi.UnsafeNativeMethods.DestroyIcon(ptr);
                return ico;
            }
            catch (ArgumentNullException ex)
            {
                Log.Write(ex);
            }
            catch (FileNotFoundException ex)
            {
                Log.Write(ex.Message + " (Path: '" + path + "'; Index: '" + index + "')", ex.StackTrace);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message + " (Path: '" + path + "'; Index: '" + index + "')", ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        ///     Returns the specified <see cref="Icon"/> resource from the system file
        ///     "imageres.dll".
        /// </summary>
        /// <param name="index">
        ///     The index of the icon to extract.
        /// </param>
        /// <param name="large">
        ///     true to return the large image; otherwise, false.
        /// </param>
        /// <param name="location">
        ///     The directory where the "imageres.dll" file is located.
        /// </param>
        public static Icon GetSystemIcon(ImageresIconIndex index, bool large = false, string location = "%system%")
        {
            try
            {
                var path = PathEx.Combine(location);
                if (Data.IsDir(path))
                    path = PathEx.Combine(path, "imageres.dll");
                if (!File.Exists(path))
                    path = PathEx.Combine("%system%\\imageres.dll");
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                var ico = GetIconFromFile(path, (int)index, large);
                return ico;
            }
            catch (FileNotFoundException ex)
            {
                Log.Write(ex);
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Returns the specified <see cref="Icon"/> resource from the system file
        ///     "imageres.dll".
        /// </summary>
        /// <param name="index">
        ///     The index of the icon to extract.
        /// </param>
        /// <param name="location">
        ///     The directory where the "imageres.dll" file is located.
        /// </param>
        public static Icon GetSystemIcon(ImageresIconIndex index, string location) =>
            GetSystemIcon(index, false, location);

        /// <summary>
        ///     Returns an file type icon of the specified file.
        /// </summary>
        /// <param name="path">
        ///     The file to get the file type icon.
        /// </param>
        /// <param name="large">
        ///     true to return the large image; otherwise, false.
        /// </param>
        public static Icon GetFileTypeIcon(string path, bool large = false)
        {
            try
            {
                path = PathEx.Combine(path);
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException();
                if (!File.Exists(path))
                    throw new FileNotFoundException();
                var shfi = new WinApi.SHFILEINFO();
                var flags = WinApi.GetFileInfoFunc.SHGFI_ICON | WinApi.GetFileInfoFunc.SHGFI_USEFILEATTRIBUTES;
                flags |= large ? WinApi.GetFileInfoFunc.SHGFI_LARGEICON : WinApi.GetFileInfoFunc.SHGFI_SMALLICON;
                WinApi.SafeNativeMethods.SHGetFileInfo(path, 0x80, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
                var ico = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                WinApi.UnsafeNativeMethods.DestroyIcon(shfi.hIcon);
                return ico;
            }
            catch (ArgumentNullException ex)
            {
                Log.Write(ex);
            }
            catch (FileNotFoundException ex)
            {
                Log.Write(ex.Message + " (Path: '" + path + "')", ex.StackTrace);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message + " (Path: '" + path + "')", ex.StackTrace);
            }
            return null;
        }

        /// <summary>
        ///     Extracts the specified resources from the current process to a new file.
        /// </summary>
        /// <param name="resData">
        ///     The resource to extract.
        /// </param>
        /// <param name="destPath">
        ///     The file to create (environment variables are accepted).
        /// </param>
        /// <param name="reverseBytes">
        ///     true to invert the order of the bytes in the specified sequence before extracting;
        ///     otherwise, false.
        /// </param>
        public static void Extract(byte[] resData, string destPath, bool reverseBytes = false)
        {
            try
            {
                var path = PathEx.Combine(destPath);
                if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException();
                var dir = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(dir))
                    throw new ArgumentNullException();
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (var ms = new MemoryStream(resData))
                {
                    var data = ms.ToArray();
                    if (reverseBytes)
                        data = data.Reverse().ToArray();
                    using (var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                        fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        ///     Displays a dialog box that prompts to the user to browse the icon resource of a file.
        ///     <see cref="OpenFileDialog"/>
        /// </summary>
        public sealed class IconBrowserDialog : Form
        {
            private readonly Button _button;
            private readonly IContainer _components = null;
            private readonly Panel _panel;
            private readonly TextBox _textBox;

            /// <summary>
            ///     Initializes an instance of the <see cref="IconBrowserDialog"/> class.
            /// </summary>
            /// <param name="path">
            ///     The path of the file to open.
            /// </param>
            /// <param name="backColor">
            ///     The background color of the dialog box.
            /// </param>
            /// <param name="foreColor">
            ///     The foreground color of the dialog box.
            /// </param>
            /// <param name="buttonFace">
            ///     The button color of the dialog box.
            /// </param>
            /// <param name="buttonText">
            ///     The button text color of the dialog box.
            /// </param>
            /// <param name="buttonHighlight">
            ///     The button highlight color of the dialog box.
            /// </param>
            public IconBrowserDialog(string path = "%system%\\imageres.dll", Color? backColor = null, Color? foreColor = null, Color? buttonFace = null, Color? buttonText = null, Color? buttonHighlight = null)
            {
                SuspendLayout();
                var resPath = PathEx.Combine(path);
                if (Data.IsDir(resPath))
                    resPath = PathEx.Combine(path, "imageres.dll");
                if (!File.Exists(resPath))
                    resPath = PathEx.Combine("%system%", "imageres.dll");
                var resLoc = Path.GetDirectoryName(resPath);
                BackColor = backColor ?? SystemColors.Control;
                ForeColor = foreColor ?? SystemColors.ControlText;
                Font = new Font("Consolas", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                Icon = GetSystemIcon(ImageresIconIndex.DirectorySearch, true, resLoc);
                MaximizeBox = false;
                MaximumSize = new Size(680, Screen.FromHandle(Handle).WorkingArea.Height);
                MinimizeBox = false;
                MinimumSize = new Size(680, 448);
                Name = "IconBrowserForm";
                Size = MinimumSize;
                SizeGripStyle = SizeGripStyle.Hide;
                StartPosition = FormStartPosition.CenterScreen;
                Text = "Icon Resource Browser";
                var tableLayoutPanel = new TableLayoutPanel
                {
                    BackColor = Color.Transparent,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Name = "tableLayoutPanel",
                    RowCount = 2
                };
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
                Controls.Add(tableLayoutPanel);
                _panel = new Panel
                {
                    AutoScroll = true,
                    BackColor = buttonFace ?? SystemColors.ButtonFace,
                    BorderStyle = BorderStyle.FixedSingle,
                    ForeColor = buttonText ?? SystemColors.ControlText,
                    Dock = DockStyle.Fill,
                    Name = "panel",
                    TabIndex = 0
                };
                _panel.Scroll += (s, e) => ((Panel)s).Update();
                tableLayoutPanel.Controls.Add(_panel, 0, 0);
                var innerTableLayoutPanel = new TableLayoutPanel
                {
                    BackColor = Color.Transparent,
                    ColumnCount = 2,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Name = "innerTableLayoutPanel"
                };
                innerTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                innerTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
                tableLayoutPanel.Controls.Add(innerTableLayoutPanel, 0, 1);
                _textBox = new TextBox
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Top,
                    Font = Font,
                    Name = "textBox",
                    TabIndex = 1
                };
                _textBox.TextChanged += TextBox_TextChanged;
                innerTableLayoutPanel.Controls.Add(_textBox, 0, 0);
                var buttonPanel = new Panel
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.Transparent,
                    BorderStyle = BorderStyle.FixedSingle,
                    Name = "buttonPanel",
                    Size = new Size(20, 20)
                };
                innerTableLayoutPanel.Controls.Add(buttonPanel, 1, 0);
                _button = new Button
                {
                    BackColor = buttonFace ?? SystemColors.ButtonFace,
                    BackgroundImage = GetSystemIcon(ImageresIconIndex.Directory, false, resLoc).ToBitmap(),
                    BackgroundImageLayout = ImageLayout.Zoom,
                    Dock = DockStyle.Fill,
                    FlatStyle = FlatStyle.Flat,
                    Font = Font,
                    ForeColor = buttonText ?? SystemColors.ControlText,
                    Name = "button",
                    TabIndex = 2,
                    UseVisualStyleBackColor = false
                };
                _button.FlatAppearance.BorderSize = 0;
                _button.FlatAppearance.MouseOverBackColor = buttonHighlight ?? ProfessionalColors.ButtonSelectedHighlight;
                _button.Click += Button_Click;
                buttonPanel.Controls.Add(_button);
                ResumeLayout(false);
                PerformLayout();
                _textBox.Text = PathEx.Combine(path);
                if (!File.Exists(_textBox.Text))
                    _textBox.Text = resPath;
                if (File.Exists(_textBox.Text))
                    ShowIconResources(_textBox.Text);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _components?.Dispose();
                base.Dispose(disposing);
            }

            private void TextBox_TextChanged(object sender, EventArgs e)
            {
                var path = PathEx.Combine(((TextBox)sender).Text);
                if (path.EndsWithEx(".ico", ".exe", ".dll") && File.Exists(path))
                    ShowIconResources(path);
            }

            private void Button_Click(object sender, EventArgs e)
            {
                using (var dialog = new OpenFileDialog
                {
                    InitialDirectory = PathEx.LocalDir,
                    Multiselect = false,
                    RestoreDirectory = false
                })
                {
                    dialog.ShowDialog(new Form { ShowIcon = false, TopMost = true });
                    if (!string.IsNullOrWhiteSpace(dialog.FileName))
                        _textBox.Text = dialog.FileName;
                }
                if (!_panel.Focus())
                    _panel.Select();
            }

            private void ShowIconResources(string path)
            {
                try
                {
                    var boxes = new IconBox[short.MaxValue];
                    if (_panel.Controls.Count > 0)
                        _panel.Controls.Clear();
                    for (var i = 0; i < short.MaxValue; i++)
                        try
                        {
                            boxes[i] = new IconBox(path, i, _button.BackColor, _button.ForeColor, _button.FlatAppearance.MouseOverBackColor);
                            _panel.Controls.Add(boxes[i]);
                        }
                        catch
                        {
                            break;
                        }
                    if (boxes[0] == null)
                        return;
                    var max = _panel.Width / boxes[0].Width;
                    for (var i = 0; i < boxes.Length; i++)
                    {
                        if (boxes[i] == null)
                            continue;
                        var line = i / max;
                        var column = i - line * max;
                        boxes[i].Location = new Point(column * boxes[i].Width, line * boxes[i].Height);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }

            private sealed class IconBox : UserControl
            {
                private static IntPtr[] _icons;
                private static string _file;
                private readonly Button _button;
                private readonly IContainer _components = null;

                public IconBox(string path, int index, Color? buttonFace = null, Color? buttonText = null, Color? buttonHighlight = null)
                {
                    SuspendLayout();
                    BackColor = buttonFace ?? SystemColors.ButtonFace;
                    ForeColor = buttonText ?? SystemColors.ControlText;
                    Name = "IconBox";
                    Size = new Size(58, 62);
                    _button = new Button
                    {
                        BackColor = BackColor,
                        FlatStyle = FlatStyle.Flat,
                        ForeColor = ForeColor,
                        ImageAlign = ContentAlignment.TopCenter,
                        Location = new Point(3, 3),
                        Name = "button",
                        Size = new Size(52, 56),
                        TabIndex = 0,
                        TextAlign = ContentAlignment.BottomCenter,
                        UseVisualStyleBackColor = false
                    };
                    _button.FlatAppearance.BorderSize = 0;
                    _button.FlatAppearance.MouseOverBackColor = buttonHighlight ?? ProfessionalColors.ButtonSelectedHighlight;
                    _button.Click += Button_Click;
                    Controls.Add(_button);
                    ResumeLayout(false);
                    if (_file != null && _file != path)
                        _icons = null;
                    _file = path;
                    var myIcon = GetIcons(index);
                    _button.Image = new Bitmap(myIcon.ToBitmap(), myIcon.Width, myIcon.Height);
                    _button.Text = index.ToString();
                }

                protected override void Dispose(bool disposing)
                {
                    if (disposing)
                        _components?.Dispose();
                    base.Dispose(disposing);
                }

                private static Icon GetIcons(int index)
                {
                    if (_icons != null)
                        return index > _icons.Length - 1 ? null : Icon.FromHandle(_icons[index]);
                    _icons = new IntPtr[short.MaxValue];
                    WinApi.UnsafeNativeMethods.ExtractIconEx(_file, 0, _icons, new IntPtr[short.MaxValue], short.MaxValue);
                    return index > _icons.Length - 1 ? null : Icon.FromHandle(_icons[index]);
                }

                private void Button_Click(object sender, EventArgs e)
                {
                    if (ParentForm == null)
                        return;
                    ParentForm.Text = File.Exists(_file) ? $"{_file},{_button.Text}" : string.Empty;
                    ParentForm.Close();
                }
            }
        }
    }
}
