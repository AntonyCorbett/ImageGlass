﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Library.WinAPI;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using System.Diagnostics;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.IGMethods contains methods for dynamic binding *
 * ****************************************************** */

public partial class FrmMain
{
    /// <summary>
    /// Opens file picker to choose an image
    /// </summary>
    /// <returns></returns>
    private void IG_OpenFile()
    {
        OpenFilePicker();
    }


    /// <summary>
    /// Refreshes image viewport.
    /// </summary>
    private void IG_Refresh()
    {
        PicMain.Refresh();
    }

    /// <summary>
    /// Reloads image file.
    /// </summary>
    private void IG_Reload()
    {
        _ = ViewNextCancellableAsync(0, isSkipCache: true);
    }

    /// <summary>
    /// Reloads images list
    /// </summary>
    private void IG_ReloadList()
    {
        _ = LoadImageListAsync(Local.Images.DistinctDirs, Local.Images.GetFileName(Local.CurrentIndex));
    }

    /// <summary>
    /// Views previous image
    /// </summary>
    private void IG_ViewPreviousImage()
    {
        _ = ViewNextCancellableAsync(-1);
    }

    /// <summary>
    /// View next image
    /// </summary>
    private void IG_ViewNextImage()
    {
        _ = ViewNextCancellableAsync(1);
    }

    /// <summary>
    /// Views an image by its index
    /// </summary>
    /// <param name="index"></param>
    private void IG_GoTo(int index)
    {
        GoToImageAsync(index);
    }

    /// <summary>
    /// Views the first image in the list
    /// </summary>
    private void IG_GoToFirst()
    {
        GoToImageAsync(0);
    }

    /// <summary>
    /// Views the last image in the list
    /// </summary>
    private void IG_GoToLast()
    {
        GoToImageAsync(Local.Images.Length - 1);
    }

    /// <summary>
    /// Zooms into the image
    /// </summary>
    private void IG_ZoomIn()
    {
        PicMain.ZoomIn();
    }

    /// <summary>
    /// Zooms out of the image
    /// </summary>
    private void IG_ZoomOut()
    {
        PicMain.ZoomOut();
    }

    /// <summary>
    /// Zoom the image by a custom value
    /// </summary>
    /// <param name="factor"></param>
    private void IG_SetZoom(float factor)
    {
        PicMain.ZoomFactor = factor;
    }

    /// <summary>
    /// Sets the zoom mode value
    /// </summary>
    /// <param name="mode"><see cref="ZoomMode"/> value in string</param>
    private void IG_SetZoomMode(string mode)
    {
        Config.ZoomMode = Helpers.ParseEnum<ZoomMode>(mode);

        if (PicMain.ZoomMode == Config.ZoomMode)
        {
            PicMain.Refresh();
        }
        else
        {
            PicMain.ZoomMode = Config.ZoomMode;
        }

        // update menu items state
        MnuAutoZoom.Checked = Config.ZoomMode == ZoomMode.AutoZoom;
        MnuLockZoom.Checked = Config.ZoomMode == ZoomMode.LockZoom;
        MnuScaleToWidth.Checked = Config.ZoomMode == ZoomMode.ScaleToWidth;
        MnuScaleToHeight.Checked = Config.ZoomMode == ZoomMode.ScaleToHeight;
        MnuScaleToFill.Checked = Config.ZoomMode == ZoomMode.ScaleToFill;
        MnuScaleToFit.Checked = Config.ZoomMode == ZoomMode.ScaleToFit;

        // update toolbar items state
        UpdateToolbarItemsState();
    }

    /// <summary>
    /// Toggles <see cref="Toolbar"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleToolbar(bool? visible = null)
    {
        visible ??= !Config.IsShowToolbar;
        Config.IsShowToolbar = visible.Value;

        // Gallery bar
        Toolbar.Visible = Config.IsShowToolbar;

        // update menu item state
        MnuToggleToolbar.Checked = Config.IsShowToolbar;

        // update toolbar items state
        UpdateToolbarItemsState();

        return Config.IsShowToolbar;
    }

    /// <summary>
    /// Toggles <see cref="Gallery"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleGallery(bool? visible = null)
    {
        visible ??= !Config.IsShowThumbnail;
        Config.IsShowThumbnail = visible.Value;

        // Gallery bar
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;
        Sp1.SplitterDistance = Sp1.Height
            - Sp1.SplitterWidth
            - Gallery.ThumbnailSize.Height
            - 30;


        // update menu item state
        MnuToggleThumbnails.Checked = Config.IsShowThumbnail;

        // update toolbar items state
        UpdateToolbarItemsState();

        return Config.IsShowThumbnail;
    }

    /// <summary>
    /// Toggles checkerboard background visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleCheckerboard(bool? visible = null)
    {
        visible ??= !Config.IsShowCheckerBoard;
        Config.IsShowCheckerBoard = visible.Value;

        if (visible.Value)
        {
            if (Config.IsShowCheckerboardOnlyImageRegion)
            {
                PicMain.CheckerboardMode = CheckerboardMode.Image;
            }
            else
            {
                PicMain.CheckerboardMode = CheckerboardMode.Client;
            }
        }
        else
        {
            PicMain.CheckerboardMode = CheckerboardMode.None;
        }

        // update menu item state
        MnuToggleCheckerboard.Checked = visible.Value;

        // update toolbar items state
        UpdateToolbarItemsState();


        return Config.IsShowCheckerBoard;
    }

    /// <summary>
    /// Toggles form top most
    /// </summary>
    /// <param name="enableTopMost"></param>
    /// <returns></returns>
    private bool IG_ToggleTopMost(bool? enableTopMost = null)
    {
        enableTopMost ??= !Config.IsWindowAlwaysOnTop;
        Config.IsWindowAlwaysOnTop = enableTopMost.Value;

        // Gallery bar
        TopMost = Config.IsWindowAlwaysOnTop;

        // update menu item state
        MnuToggleTopMost.Checked = TopMost;

        return Config.IsWindowAlwaysOnTop;
    }


    private void IG_ReportIssue()
    {
        try
        {
            // TODO:
            Process.Start("https://github.com/d2phap/ImageGlass/issues");
        }
        catch { }
    }


    private void IG_About()
    {
        var archInfo = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var appVersion = Application.ProductVersion.ToString() + $" ({archInfo})";

        var btnDonate = new TaskDialogButton("Donate", allowCloseDialog: false);
        var btnClose = new TaskDialogButton("Close", allowCloseDialog: true);

        btnDonate.Click += (object? sender, EventArgs e) =>
        {
            try
            {
                // TODO:
                Process.Start("https://imageglass.org/source#donation?utm_source=app_" + App.Version + "&utm_medium=app_click&utm_campaign=app_donation");
            }
            catch { }
        };


        TaskDialog.ShowDialog(new()
        {
            Icon = TaskDialogIcon.Information,
            Caption = $"About {Application.ProductName}",

            Heading = $"Version: {appVersion}",
            Text = $"Copyright © 2010-{DateTime.Now.Year} by Dương Diệu Pháp.\r\n" +
                $"All rights reserved.\r\n\r\n" +
                $"Homepage: https://imageglass.org\r\n" +
                $"GitHub: https://github.com/d2phap/ImageGlass" +
                $"",

            Buttons = new TaskDialogButtonCollection { btnDonate, btnClose },
        });
    }


    private void IG_Settings()
    {
        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true,
        };

        Process.Start(psi);
    }


    private void IG_Exit()
    {
        Application.Exit();
    }


    private void IG_Print()
    {
        // image error
        if (PicMain.Source == ImageSource.Null)
        {
            return;
        }

        var currentFile = Local.Images.GetFileName(Local.CurrentIndex);
        var fileToPrint = currentFile;

        if (Local.IsTempMemoryData || Local.Metadata?.FramesCount == 1)
        {
            // TODO: // save image to temp file
            //fileToPrint = SaveTemporaryMemoryData();
        }
        // rename ext FAX -> TIFF to multipage printing
        else if (Path.GetExtension(currentFile).Equals(".FAX", StringComparison.OrdinalIgnoreCase))
        {
            fileToPrint = App.ConfigDir(PathType.File, Dir.Temporary, Path.GetFileNameWithoutExtension(currentFile) + ".tiff");
            File.Copy(currentFile, fileToPrint, true);
        }

        PrintService.OpenPrintPictures(fileToPrint);

        // TODO:
        //try
        //{
        //    PrintService.OpenPrintPictures(fileToPrint);
        //}
        //catch
        //{
        //    fileToPrint = SaveTemporaryMemoryData();
        //    PrintService.OpenPrintPictures(fileToPrint);
        //}
    }

}

