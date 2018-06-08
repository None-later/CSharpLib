#region auto-generated FILE INFORMATION

// ==============================================
// This file is distributed under the MIT License
// ==============================================
// 
// Filename: NotifyBox.cs
// Version:  2018-06-08 21:20
// 
// Copyright (c) 2018, Si13n7 Developments (r)
// All rights reserved.
// ______________________________________________

#endregion

namespace SilDev
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Media;
    using System.Threading;
    using System.Windows.Forms;
    using Timer = System.Windows.Forms.Timer;

    /// <summary>
    ///     Provides enumerated constants used to retrieves the play sound of the notify box.
    /// </summary>
    public enum NotifyBoxSound
    {
#pragma warning disable CS1591
        None = 0,
        Asterisk = 1,
        Warning = 2,
        Notify = 3,
        Question = 4,
#pragma warning restore CS1591
    }

    /// <summary>
    ///     Provides enumerated constants used to retrieves the start position of the notify box.
    /// </summary>
    public enum NotifyBoxStartPosition
    {
#pragma warning disable CS1591
        Center = 0,
        CenterLeft = 1,
        CenterRight = 2,
        BottomLeft = 3,
        BottomRight = 4,
        TopLeft = 5,
        TopRight = 6
#pragma warning restore CS1591
    }

    /// <summary>
    ///     Represents a notification window, simliar with a system tray notification, which presents a
    ///     notification to the user.
    /// </summary>
    public class NotifyBox
    {
        private double _opacity = .90d;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyBox"/> class.
        /// </summary>
        /// <param name="opacity">
        ///     The opacity level.
        /// </param>
        /// <param name="backColor">
        ///     The background color.
        /// </param>
        /// <param name="borderColor">
        ///     The border color.
        /// </param>
        /// <param name="captionColor">
        ///     The caption color.
        /// </param>
        /// <param name="textColor">
        ///     The text color.
        /// </param>
        /// <param name="topMost">
        ///     true to place the notify box above all non-topmost windows; otherwise, false.
        /// </param>
        public NotifyBox(double opacity = .90d, Color backColor = default(Color), Color borderColor = default(Color), Color captionColor = default(Color), Color textColor = default(Color), bool topMost = false)
        {
            Opacity = opacity;
            if (backColor != default(Color))
                BackColor = backColor;
            if (borderColor != default(Color))
                BorderColor = borderColor;
            if (captionColor != default(Color))
                CaptionColor = captionColor;
            if (textColor != default(Color))
                TextColor = textColor;
            TopMost = topMost;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyBox"/> class.
        /// </summary>
        public NotifyBox(double opacity, bool topMost) : this(opacity, default(Color), default(Color), default(Color), default(Color), topMost) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyBox"/> class.
        /// </summary>
        public NotifyBox(bool topMost) : this(.90d, default(Color), default(Color), default(Color), default(Color), topMost) { }

        /// <summary>
        ///     Gets or sets the opacity level for the notify box.
        /// </summary>
        public double Opacity
        {
            get => _opacity;
            set => _opacity = value < .2d ? .2d : value > 1d ? 1d : value;
        }

        /// <summary>
        ///     Gets or sets the background color for the notify box.
        /// </summary>
        public Color BackColor { get; set; } = SystemColors.Menu;

        /// <summary>
        ///     Gets or sets the border color for the notify box.
        /// </summary>
        public Color BorderColor { get; set; } = SystemColors.MenuHighlight;

        /// <summary>
        ///     Gets or sets the caption color for the notify box.
        /// </summary>
        public Color CaptionColor { get; set; } = SystemColors.MenuHighlight;

        /// <summary>
        ///     Gets or sets the text color for the notify box.
        /// </summary>
        public Color TextColor { get; set; } = SystemColors.MenuText;

        /// <summary>
        ///     Specifies that the notify box is placed above all non-topmost windows.
        /// </summary>
        public bool TopMost { get; set; }

        private NotifyBoxForm NotifyWindow { get; set; }

        private Thread NotifyThread { get; set; }

        /// <summary>
        ///     Gets a value indicating the execution status of the current notify box.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                try
                {
                    return NotifyThread.IsAlive;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Displays a notify box with the specified text, caption, position, sound, duration, and borders.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="sound">
        ///     The play sound for the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        public void Show(string text, string caption, NotifyBoxStartPosition position = NotifyBoxStartPosition.BottomRight, NotifyBoxSound sound = NotifyBoxSound.None, ushort duration = 0)
        {
            try
            {
                if (IsAlive)
                    throw new NotSupportedException();
                NotifyWindow = new NotifyBoxForm(text, caption, position, duration, TopMost, _opacity, BackColor, BorderColor, CaptionColor, TextColor);
                NotifyThread = new Thread(() => NotifyWindow.ShowDialog());
                NotifyThread.Start();
                switch (sound)
                {
                    case NotifyBoxSound.Asterisk:
                        SystemSounds.Asterisk.Play();
                        break;
                    case NotifyBoxSound.Warning:
                        SystemSounds.Hand.Play();
                        break;
                    case NotifyBoxSound.Question:
                        SystemSounds.Question.Play();
                        break;
                    case NotifyBoxSound.Notify:
                        var wavPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media\\Windows Notify System Generic.wav");
                        if (File.Exists(wavPath))
                            new SoundPlayer(wavPath).Play();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        ///     Displays a notify box with the specified text, caption, position, duration, and borders.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        public void Show(string text, string caption, NotifyBoxStartPosition position, ushort duration) =>
            Show(text, caption, position, NotifyBoxSound.None, duration);

        /// <summary>
        ///     Displays a notify box with the specified text, caption, sound, duration, and borders.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="sound">
        ///     The play sound for the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        public void Show(string text, string caption, NotifyBoxSound sound, ushort duration = 0) =>
            Show(text, caption, NotifyBoxStartPosition.BottomRight, sound, duration);

        /// <summary>
        ///     Displays a notify box with the specified text, caption, duration, and borders.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        public void Show(string text, string caption, ushort duration) =>
            Show(text, caption, NotifyBoxStartPosition.BottomRight, NotifyBoxSound.None, duration);

        /// <summary>
        ///     Closes the current notify box.
        /// </summary>
        public void Close()
        {
            try
            {
                NotifyWindow?.Close();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        /// <summary>
        ///     Terminates the current notify box.
        /// </summary>
        public void Abort()
        {
            Close();
            try
            {
                if (IsAlive)
                    NotifyThread?.Abort();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private sealed class NotifyBoxForm : Form
        {
            private readonly BackgroundWorker _bgWorker;
            private readonly IContainer _components;
            private readonly int _duration;
            private readonly double _opacity;
            private readonly Label _textLabel;
            private readonly Timer _timer, _timer2;
            private bool _visible;

            public NotifyBoxForm(string text, string title, NotifyBoxStartPosition position, ushort duration, bool topMost, double opacity, Color backColor, Color borderColor, Color captionColor, Color textColor)
            {
                _components = new Container();
                _duration = Convert.ToInt32(duration).IsBetween(1, 999) ? 1000 : duration;
                _opacity = opacity;
                SuspendLayout();
                var titleLabel = new Label
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Tahoma", 11.25f, FontStyle.Bold),
                    ForeColor = captionColor,
                    Location = new Point(3, 3),
                    Text = title
                };
                Controls.Add(titleLabel);
                _textLabel = new Label
                {
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Font = new Font("Tahoma", 8.25f, FontStyle.Regular),
                    ForeColor = textColor,
                    Location = new Point(8, 24),
                    Text = text
                };
                Controls.Add(_textLabel);
                if (backColor != borderColor)
                    for (var i = 0; i < 4; i++)
                        Controls.Add(new Panel
                        {
                            AutoSize = false,
                            BackColor = borderColor,
                            Dock = i == 0 ? DockStyle.Top : i == 1 ? DockStyle.Right : i == 2 ? DockStyle.Bottom : DockStyle.Left,
                            Location = new Point(0, 0),
                            Size = new Size(1, 1)
                        });
                _bgWorker = new BackgroundWorker();
                if (_duration > 0)
                {
                    _bgWorker.DoWork += (sender, args) => Thread.Sleep(_duration - 20);
                    _bgWorker.RunWorkerCompleted += (sender, args) => Close();
                }
                _timer = new Timer(_components)
                {
                    Interval = 1
                };
                _timer.Tick += FadeInOutTimer_Tick;
                _timer2 = new Timer(_components)
                {
                    Interval = byte.MaxValue
                };
                _timer2.Tick += ProgressDotsTimer_Tick;
                AutoScaleDimensions = new SizeF(96f, 96f);
                AutoScaleMode = AutoScaleMode.Dpi;
                BackColor = backColor;
                ClientSize = new Size(48, 44);
                Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                ForeColor = textColor;
                FormBorderStyle = FormBorderStyle.None;
                Opacity = 0d;
                ShowIcon = false;
                ShowInTaskbar = false;
                Size = new Size((titleLabel.Size.Width < _textLabel.Size.Width ? _textLabel.Size.Width : titleLabel.Size.Width) + 12, titleLabel.Size.Height + _textLabel.Size.Height + 12);
                StartPosition = position == NotifyBoxStartPosition.Center ? FormStartPosition.CenterScreen : FormStartPosition.Manual;
                if (StartPosition == FormStartPosition.Manual)
                {
                    var taskBarLocation = TaskBar.GetLocation();
                    var taskBarSize = TaskBar.GetSize();
                    switch (position)
                    {
                        case NotifyBoxStartPosition.CenterLeft:
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.BottomLeft:
                            Location = new Point(taskBarLocation == TaskBarLocation.Left ? taskBarSize + 3 : 3, Location.Y);
                            break;
                        case NotifyBoxStartPosition.CenterRight:
                        case NotifyBoxStartPosition.TopRight:
                        case NotifyBoxStartPosition.BottomRight:
                            Location = new Point(Screen.PrimaryScreen.Bounds.Width - Width - (taskBarLocation == TaskBarLocation.Right ? taskBarSize + 3 : 3), Location.Y);
                            break;
                    }
                    switch (position)
                    {
                        case NotifyBoxStartPosition.CenterLeft:
                        case NotifyBoxStartPosition.CenterRight:
                            Location = new Point(Location.X, Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2);
                            break;
                        case NotifyBoxStartPosition.BottomLeft:
                        case NotifyBoxStartPosition.BottomRight:
                            Location = new Point(Location.X, Screen.PrimaryScreen.Bounds.Height - Height - (taskBarLocation == TaskBarLocation.Bottom ? taskBarSize + 3 : 3));
                            break;
                        case NotifyBoxStartPosition.TopLeft:
                        case NotifyBoxStartPosition.TopRight:
                            Location = new Point(Location.X, taskBarLocation == TaskBarLocation.Top ? taskBarSize + 3 : 3);
                            break;
                    }
                }
                TopMost = topMost;
                Shown += NotifyForm_Shown;
                FormClosing += NotifyForm_FormClosing;
                ResumeLayout(false);
                PerformLayout();
            }

            /// <summary>
            ///     Disposes of the resources (other than memory) used by the <see cref="Form"/>.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _components.Dispose();
                base.Dispose(disposing);
            }

            private void NotifyForm_Shown(object sender, EventArgs e)
            {
                _timer.Enabled = true;
                if (_textLabel.Text.EndsWith(" . . ."))
                    _timer2.Enabled = true;
                if (_duration > 0)
                    _bgWorker.RunWorkerAsync();
            }

            private void NotifyForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (!_visible)
                    return;
                _timer.Enabled = true;
                e.Cancel = true;
            }

            private void FadeInOutTimer_Tick(object sender, EventArgs e)
            {
                if (!(sender is Timer timer))
                    return;
                if (!_visible && Opacity < _opacity)
                {
                    Opacity += .025d;
                    return;
                }
                if (_visible && Opacity > 0)
                {
                    Opacity -= .05d;
                    return;
                }
                _visible = !_visible;
                if (!_visible)
                    Close();
                timer.Enabled = false;
            }

            private void ProgressDotsTimer_Tick(object sender, EventArgs e)
            {
                if (!(sender is Timer timer))
                    return;
                var s = _textLabel.Text;
                if (!s.EndsWith(" ."))
                {
                    timer.Enabled = false;
                    return;
                }
                if (s.EndsWith(" . . ."))
                    while (s.EndsWith(" . ."))
                        s = s.Replace(" . .", " .");
                else
                    s = s.EndsWith(" . .") ? s.Replace(" . .", " . . .") : s.Replace(" .", " . .");
                _textLabel.Text = s;
            }
        }
    }

    /// <summary>
    ///     Displays a notification window, simliar with a system tray notification, which presents a
    ///     notification to the user.
    /// </summary>
    public static class NotifyBoxEx
    {
        /// <summary>
        ///     Displays a notification box with the specified parameters.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="sound">
        ///     The play sound for the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        /// <param name="topMost">
        ///     true to place the notify box above all non-topmost windows; otherwise, false.
        /// </param>
        /// <param name="opacity">
        ///     The opacity level.
        /// </param>
        /// <param name="backColor">
        ///     The background color.
        /// </param>
        /// <param name="borderColor">
        ///     The border color.
        /// </param>
        /// <param name="captionColor">
        ///     The caption color.
        /// </param>
        /// <param name="textColor">
        ///     The text color.
        /// </param>
        public static NotifyBox Show(string text, string caption, NotifyBoxStartPosition position = NotifyBoxStartPosition.BottomRight, NotifyBoxSound sound = NotifyBoxSound.None, ushort duration = 5000, bool topMost = true, double opacity = .90d, Color backColor = default(Color), Color borderColor = default(Color), Color captionColor = default(Color), Color textColor = default(Color))
        {
            var notifyBox = new NotifyBox(opacity, backColor, borderColor, captionColor, textColor, topMost);
            notifyBox.Show(text, caption, position, sound, duration);
            return notifyBox;
        }

        /// <summary>
        ///     Displays a notification box with the specified parameters for 5 seconds.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="sound">
        ///     The play sound for the notify box.
        /// </param>
        /// <param name="topMost">
        ///     true to place the notify box above all non-topmost windows; otherwise, false.
        /// </param>
        /// <param name="opacity">
        ///     The opacity level.
        /// </param>
        /// <param name="backColor">
        ///     The background color.
        /// </param>
        /// <param name="borderColor">
        ///     The border color.
        /// </param>
        /// <param name="captionColor">
        ///     The caption color.
        /// </param>
        /// <param name="textColor">
        ///     The text color.
        /// </param>
        public static NotifyBox Show(string text, string caption, NotifyBoxStartPosition position, NotifyBoxSound sound, bool topMost, double opacity = .90d, Color backColor = default(Color), Color borderColor = default(Color), Color captionColor = default(Color), Color textColor = default(Color)) =>
            Show(text, caption, position, sound, 5000, topMost, opacity, backColor, borderColor, captionColor, textColor);

        /// <summary>
        ///     Displays a notification box with the specified parameters.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="duration">
        ///     The duration of the time, in milliseconds, which the notify box remains active.
        /// </param>
        /// <param name="topMost">
        ///     true to place the notify box above all non-topmost windows; otherwise, false.
        /// </param>
        /// <param name="opacity">
        ///     The opacity level.
        /// </param>
        /// <param name="backColor">
        ///     The background color.
        /// </param>
        /// <param name="borderColor">
        ///     The border color.
        /// </param>
        /// <param name="captionColor">
        ///     The caption color.
        /// </param>
        /// <param name="textColor">
        ///     The text color.
        /// </param>
        public static NotifyBox Show(string text, string caption, NotifyBoxStartPosition position, ushort duration, bool topMost = true, double opacity = .90d, Color backColor = default(Color), Color borderColor = default(Color), Color captionColor = default(Color), Color textColor = default(Color)) =>
            Show(text, caption, position, NotifyBoxSound.None, duration, topMost, opacity, backColor, borderColor, captionColor, textColor);

        /// <summary>
        ///     Displays a notification box with the specified parameters for 5 seconds.
        /// </summary>
        /// <param name="text">
        ///     The notification text to display in the notify box.
        /// </param>
        /// <param name="caption">
        ///     The caption text to display in the notify box.
        /// </param>
        /// <param name="position">
        ///     The window position for the notify box.
        /// </param>
        /// <param name="topMost">
        ///     true to place the notify box above all non-topmost windows; otherwise, false.
        /// </param>
        /// <param name="opacity">
        ///     The opacity level.
        /// </param>
        /// <param name="backColor">
        ///     The background color.
        /// </param>
        /// <param name="borderColor">
        ///     The border color.
        /// </param>
        /// <param name="captionColor">
        ///     The caption color.
        /// </param>
        /// <param name="textColor">
        ///     The text color.
        /// </param>
        public static NotifyBox Show(string text, string caption, NotifyBoxStartPosition position, bool topMost, double opacity = .90d, Color backColor = default(Color), Color borderColor = default(Color), Color captionColor = default(Color), Color textColor = default(Color)) =>
            Show(text, caption, position, NotifyBoxSound.None, 5000, topMost, opacity, backColor, borderColor, captionColor, textColor);
    }
}
