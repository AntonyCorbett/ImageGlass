﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using Microsoft.Win32;

namespace ImageGlass.UI;

/// <summary>
/// Modern form with dark mode and backdrop support.
/// </summary>
public partial class ModernForm : Form
{
    private bool _isDarkMode = true;
    private BackdropStyle _backdropStyle = BackdropStyle.Mica;
    private Padding _backdropMargin = new(-1);
    private int _dpi = DpiApi.DPI_DEFAULT;
    private CancellationTokenSource _systemAccentColorChangedCancelToken = new();
    private CancellationTokenSource _requestUpdatingColorModeCancelToken = new();


    #region Public properties

    /// <summary>
    /// Enable transparent background.
    /// </summary>
    public virtual bool EnableTransparent { get; set; } = true;


    /// <summary>
    /// Enables or disables form dark mode.
    /// </summary>
    public virtual bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _isDarkMode = value;
            SetDarkMode(value);
        }
    }


    /// <summary>
    /// Gets, sets window backdrop.
    /// </summary>
    public virtual BackdropStyle BackdropStyle
    {
        get => _backdropStyle;
        set
        {
            _backdropStyle = value;
            SetBackdrop(value);
        }
    }


    /// <summary>
    /// Gets, sets the backdrop margin.
    /// </summary>
    public virtual Padding BackdropMargin
    {
        get => _backdropMargin;
        set
        {
            _backdropMargin = value;
            _ = WindowApi.SetWindowFrame(Handle, _backdropMargin);
        }
    }


    /// <summary>
    /// Gets, sets the keys to close the <see cref="ModernForm"/>.
    /// </summary>
    public virtual Keys CloseFormHotkey { get; set; } = Keys.None;


    /// <summary>
    /// Enables or disables shortcut key handling in parent form.
    /// </summary>
    public virtual bool EnableParentShortcut { get; set; } = false;


    /// <summary>
    /// Gets the current DPI. Default value is <c>96</c>.
    /// </summary>
    public virtual int Dpi => _dpi;


    /// <summary>
    /// Gets the current DPI scaling. Default value is <c>1.0f</c>.
    /// </summary>
    public virtual float DpiScale => _dpi / 96;


    /// <summary>
    /// Gets, sets the value indicates that <see cref="DpiApi.CurrentDpi"/> should be updated when form DPI is changed.
    /// </summary>
    public virtual bool EnableDpiApiUpdate { get; set; } = false;


    /// <summary>
    /// Occurs when the Maximize button on title bar is clicked.
    /// </summary>
    public event MaximizeButtonClickedHandler? MaximizeButtonClicked;
    public delegate void MaximizeButtonClickedHandler(EventArgs e);


    /// <summary>
    /// Occurs when the Restore button on title bar is clicked.
    /// </summary>
    public event RestoreButtonClickedHandler? RestoreButtonClicked;
    public delegate void RestoreButtonClickedHandler(EventArgs e);


    /// <summary>
    /// Occurs when the system accent color is changed.
    /// </summary>
    public event SystemAccentColorChangedHandler? SystemAccentColorChanged;
    public delegate void SystemAccentColorChangedHandler(SystemAccentColorChangedEventArgs e);


    /// <summary>
    /// Occurs when the system app color is changed and does not match the <see cref="IsDarkMode"/> value.
    /// </summary>
    public event RequestUpdatingColorModeHandler? RequestUpdatingColorMode;
    public delegate void RequestUpdatingColorModeHandler(SystemColorModeChangedEventArgs e);

    #endregion // Public properties


    /// <summary>
    /// Initializes the new instance of <see cref="ModernForm"/>.
    /// </summary>
    public ModernForm()
    {
        InitializeComponent();
        SizeGripStyle = SizeGripStyle.Hide;

        _dpi = DeviceDpi;

        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }


    // Protected / virtual functions
    #region Protected / virtual functions

    protected override void OnHandleCreated(EventArgs e)
    {
        if (!DesignMode)
        {
            ApplyTheme(_isDarkMode);
        }

        base.OnHandleCreated(e);
    }


    protected override void WndProc(ref Message m)
    {
        // WM_SYSCOMMAND
        if (m.Msg == 0x0112)
        {
            // When user clicks on MAXIMIZE button on title bar
            if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
            {
                // The window is being maximized
                MaximizeButtonClicked?.Invoke(EventArgs.Empty);
            }
            // When user clicks on the RESTORE button on title bar
            else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
            {
                // The window is being restored
                RestoreButtonClicked?.Invoke(EventArgs.Empty);
            }
        }
        else if (m.Msg == DpiApi.WM_DPICHANGED)
        {
            // get new dpi value
            _dpi = (short)m.WParam;

            OnDpiChanged();
        }
        // WM_DWMCOLORIZATIONCOLORCHANGED: accent color changed
        else if (m.Msg == 0x0320)
        {
            DelayTriggerSystemAccentColorChangedEvent();
        }


        base.WndProc(ref m);
    }
    

    /// <summary>
    /// Triggers <see cref="SystemAccentColorChanged"/> event.
    /// </summary>
    protected virtual void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        // emits the event
        SystemAccentColorChanged?.Invoke(e);

        // the event is not handled
        if (!e.Handled)
        {
            Invalidate(true);
        }
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    protected virtual void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // emits the event
        RequestUpdatingColorMode?.Invoke(e);
    }


    /// <summary>
    /// Occurs when window's DPI is changed.
    /// </summary>
    protected virtual void OnDpiChanged()
    {
        if (EnableDpiApiUpdate)
        {
            DpiApi.CurrentDpi = _dpi;
        }
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!DesignMode
            && EnableTransparent
            && _backdropStyle != BackdropStyle.Default
            && BackdropMargin.Vertical == 0 && BackdropMargin.Horizontal == 0)
        {
            WindowApi.SetTransparentBlackBackground(e.Graphics, Bounds);
        }
    }


    /// <summary>
    /// Apply theme of the window.
    /// </summary>
    protected virtual void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        _isDarkMode = darkMode;
        _backdropStyle = style ?? _backdropStyle;

        if (DesignMode) return;

        SetDarkMode(_isDarkMode);
        SetBackdrop(_backdropStyle);
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // disable parent form shotcuts
        if (!EnableParentShortcut)
        {
            return false;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (CloseFormHotkey != Keys.None && e.KeyData.Equals(CloseFormHotkey))
        {
            CloseFormByKeys();
        }
    }


    /// <summary>
    /// Closes the window when <see cref="CloseFormHotkey"/> is pressed.
    /// </summary>
    protected virtual void CloseFormByKeys()
    {
        Close();
    }

    #endregion // Protected / virtual functions


    // Private functions
    #region Private functions

    /// <summary>
    /// Sets window backdrop.
    /// </summary>
    private void SetBackdrop(BackdropStyle style)
    {
        var backupBgColor = BackColor;
        if (style != BackdropStyle.Default && EnableTransparent)
        {
            // back color must be black
            BackColor = Color.Black;
        }

        // set backdrop style
        var succeeded = WindowApi.SetWindowBackdrop(Handle, (DWM_SYSTEMBACKDROP_TYPE)style);
        var margin = (succeeded && style != BackdropStyle.Default && EnableTransparent)
            ? BackdropMargin
            : new Padding(0);

        if (!succeeded)
        {
            BackColor = backupBgColor;
        }

        // set window frame
        _ = WindowApi.SetWindowFrame(Handle, margin);
    }


    /// <summary>
    /// Sets window dark mode.
    /// </summary>
    private void SetDarkMode(bool enable)
    {
        WindowApi.SetImmersiveDarkMode(Handle, enable);
    }


    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // User settings changed:
        // - Color mode: dark / light
        // - Transparency
        // - Accent color
        // - others...
        if (e.Category == UserPreferenceCategory.General)
        {
            DelayTriggerRequestUpdatingColorModeEvent();
        }
    }


    /// <summary>
    /// Delays triggering <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    private void DelayTriggerRequestUpdatingColorModeEvent()
    {
        _requestUpdatingColorModeCancelToken.Cancel();
        _requestUpdatingColorModeCancelToken = new();

        _ = TriggerRequestUpdatingColorModeEventAsync(_requestUpdatingColorModeCancelToken.Token);
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    private async Task TriggerRequestUpdatingColorModeEventAsync(CancellationToken token = default)
    {
        try
        {
            // since the message is triggered multiple times (3 - 5 times)
            await Task.Delay(200, token);
            token.ThrowIfCancellationRequested();

            var eventArgs = new SystemColorModeChangedEventArgs();
            if (IsDarkMode != eventArgs.IsDarkMode)
            {
                // emit event here
                OnRequestUpdatingColorMode(eventArgs);
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Delays triggering <see cref="SystemAccentColorChanged"/> event.
    /// </summary>
    private void DelayTriggerSystemAccentColorChangedEvent()
    {
        _systemAccentColorChangedCancelToken.Cancel();
        _systemAccentColorChangedCancelToken = new();

        _ = TriggerSystemAccentColorChangedEventAsync(_systemAccentColorChangedCancelToken.Token);
    }


    /// <summary>
    /// Triggers <see cref="SystemAccentColorChanged"/> event.
    /// </summary>
    private async Task TriggerSystemAccentColorChangedEventAsync(CancellationToken token = default)
    {
        try
        {
            // since the message WM_DWMCOLORIZATIONCOLORCHANGED is triggered
            // multiple times (3 - 5 times)
            await Task.Delay(200, token);
            token.ThrowIfCancellationRequested();

            // emit event here
            OnSystemAccentColorChanged(new SystemAccentColorChangedEventArgs());
        }
        catch (OperationCanceledException) { }
    }


    #endregion // Private functions

}

