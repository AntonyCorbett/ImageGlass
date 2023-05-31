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

using DirectN;
using ImageGlass.Base;
using ImageGlass.Base.Actions;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Diagnostics;
using System.Dynamic;
using System.Media;
using System.Reflection;
using System.Text;

namespace ImageGlass.Settings;

/// <summary>
/// Provides app configuration
/// </summary>
public static class Config
{

    #region Internal properties
    private static readonly Source _source = new();
    private static CancellationTokenSource _requestUpdatingColorModeCancelToken = new();
    private static bool _isDarkMode = WinColorsApi.IsDarkMode;


    /// <summary>
    /// Gets the default set of toolbar buttons
    /// </summary>
    public static List<ToolbarItemModel> DefaultToolbarItems => new()
    {
        new()
        {
            Id = "Btn_OpenFile",
            Alignment = ToolStripItemAlignment.Right,
            Image = nameof(Theme.ToolbarIcons.OpenFile),
            OnClick = new("MnuOpenFile"),
        },
        new()
        {
            Id = "Btn_ViewPrevious",
            Image = nameof(Theme.ToolbarIcons.ViewPreviousImage),
            OnClick = new("MnuViewPrevious"),
        },
        new()
        {
            Id = "Btn_ViewNext",
            Image = nameof(Theme.ToolbarIcons.ViewNextImage),
            OnClick = new("MnuViewNext"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "Btn_AutoZoom",
            Image = nameof(Theme.ToolbarIcons.AutoZoom),
            OnClick = new("MnuAutoZoom"),
        },
        new()
        {
            Id = "Btn_LockZoom",
            Image = nameof(Theme.ToolbarIcons.LockZoom),
            OnClick = new("MnuLockZoom"),
        },
        new()
        {
            Id = "Btn_ScaleToWidth",
            Image = nameof(Theme.ToolbarIcons.ScaleToWidth),
            OnClick = new("MnuScaleToWidth"),
        },
        new()
        {
            Id = "Btn_ScaleToHeight",
            Image = nameof(Theme.ToolbarIcons.ScaleToHeight),
            OnClick = new("MnuScaleToHeight"),
        },
        new()
        {
            Id = "Btn_ScaleToFit",
            Image = nameof(Theme.ToolbarIcons.ScaleToFit),
            OnClick = new("MnuScaleToFit"),
        },
        new()
        {
            Id = "Btn_ScaleToFill",
            Image = nameof(Theme.ToolbarIcons.ScaleToFill),
            OnClick = new("MnuScaleToFill"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "Btn_Refresh",
            Image = nameof(Theme.ToolbarIcons.Refresh),
            OnClick = new("MnuRefresh"),
        },
        new()
        {
            Id = "Btn_Gallery",
            Image = nameof(Theme.ToolbarIcons.Gallery),
            CheckableConfigBinding = nameof(ShowGallery),
            OnClick = new("MnuToggleGallery"),
        },
        new()
        {
            Id = "Btn_Checkerboard",
            Image = nameof(Theme.ToolbarIcons.Checkerboard),
            CheckableConfigBinding = nameof(ShowCheckerboard),
            OnClick = new("MnuToggleCheckerboard"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "Btn_FullScreen",
            Image = nameof(Theme.ToolbarIcons.FullScreen),
            CheckableConfigBinding = nameof(EnableFullScreen),
            OnClick = new("MnuFullScreen"),
        },
        new()
        {
            Id = "Btn_Slideshow",
            Image = nameof(Theme.ToolbarIcons.Slideshow),
            CheckableConfigBinding = nameof(EnableSlideshow),
            OnClick = new("MnuSlideshow"),
        },
        new() { Type = ToolbarItemModelType.Separator },
        new()
        {
            Id = "Btn_Print",
            Image = nameof(Theme.ToolbarIcons.Print),
            OnClick = new("MnuPrint"),
        },
        new()
        {
            Id = "Btn_Delete",
            Image = nameof(Theme.ToolbarIcons.Delete),
            OnClick = new("MnuMoveToRecycleBin"),
        }
    };

    /// <summary>
    /// The default image info tags
    /// </summary>
    public static List<string> DefaultInfoItems => new()
    {
        nameof(ImageInfo.Name),
        nameof(ImageInfo.ListCount),
        nameof(ImageInfo.FrameCount),
        nameof(ImageInfo.Zoom),
        nameof(ImageInfo.Dimension),
        nameof(ImageInfo.FileSize),
        nameof(ImageInfo.ColorSpace),
        nameof(ImageInfo.ExifRating),
        nameof(ImageInfo.DateTimeAuto),
        nameof(ImageInfo.AppName),
    };


    /// <summary>
    /// Gets, sets current theme.
    /// </summary>
    public static IgTheme Theme { get; set; } = new();


    /// <summary>
    /// Occurs when the system app color is changed and does not match the current <see cref="Theme"/>'s dark mode.
    /// </summary>
    public static event RequestUpdatingColorModeHandler? RequestUpdatingColorMode;
    public delegate void RequestUpdatingColorModeHandler(SystemColorModeChangedEventArgs e);


    #endregion


    #region Setting items

    /// <summary>
    /// Gets, sets the config section of tool settings.
    /// </summary>
    public static ExpandoObject ToolSettings { get; set; } = new ExpandoObject();


    #region Boolean items

    /// <summary>
    /// Gets, sets value indicating whether the slideshow mode is enabled or not.
    /// </summary>
    public static bool EnableSlideshow { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating whether the FrmMain should be hidden when <see cref="EnableSlideshow"/> is on.
    /// </summary>
    public static bool HideMainWindowInSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value if the countdown timer is shown or not.
    /// </summary>
    public static bool ShowSlideshowCountdown { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates whether the slide show interval is random.
    /// </summary>
    public static bool UseRandomIntervalForSlideshow { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that slideshow will loop back to the first image when reaching the end of list.
    /// </summary>
    public static bool EnableLoopSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that slideshow is played in full screen, not window mode.
    /// </summary>
    public static bool EnableFullscreenSlideshow { get; set; } = true;

    /// <summary>
    /// Gets, sets value of FrmMain's frameless mode.
    /// </summary>
    public static bool EnableFrameless { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating whether the full screen mode is enabled or not.
    /// </summary>
    public static bool EnableFullScreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the toolbar should be hidden in Full screen mode
    /// </summary>
    public static bool HideToolbarInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that the gallery should be hidden in Full screen mode
    /// </summary>
    public static bool HideGalleryInFullscreen { get; set; } = false;

    /// <summary>
    /// Gets, sets value of gallery visibility
    /// </summary>
    public static bool ShowGallery { get; set; } = true;

    /// <summary>
    /// Gets, sets value whether gallery scrollbars visible
    /// </summary>
    public static bool ShowGalleryScrollbars { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that showing image file name on gallery
    /// </summary>
    public static bool ShowGalleryFileName { get; set; } = false;

    /// <summary>
    /// Gets, sets welcome picture value
    /// </summary>
    public static bool ShowWelcomeImage { get; set; } = true;

    /// <summary>
    /// Gets, sets value of visibility of toolbar when start up
    /// </summary>
    public static bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that ImageGlass will loop back viewer to the first image when reaching the end of the list.
    /// </summary>
    public static bool EnableLoopBackNavigation { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that checker board is shown or not
    /// </summary>
    public static bool ShowCheckerboard { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates whether to show checkerboard in the image region only
    /// </summary>
    public static bool ShowCheckerboardOnlyImageRegion { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating that multi instances is allowed or not
    /// </summary>
    public static bool EnableMultiInstances { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that FrmMain is always on top or not.
    /// </summary>
    public static bool EnableWindowTopMost { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when deleting image
    /// </summary>
    public static bool ShowDeleteConfirmation { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicating that Confirmation dialog is displayed when overriding the viewing image
    /// </summary>
    public static bool ShowSaveOverrideConfirmation { get; set; } = true;

    /// <summary>
    /// Gets, sets the setting to control whether the image's original modified date value is preserved on save
    /// </summary>
    public static bool ShouldPreserveModifiedDate { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that there is a new version
    /// </summary>
    public static bool ShowNewVersionIndicator { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that to toolbar buttons to be centered horizontally
    /// </summary>
    public static bool EnableCenterToolbar { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that to show last seen image on startup
    /// </summary>
    public static bool ShouldOpenLastSeenImage { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that the ColorProfile will be applied for all or only the images with embedded profile
    /// </summary>
    public static bool ShouldUseColorProfileForAll { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates whether to show or hide the Navigation Buttons on viewer
    /// </summary>
    public static bool EnableNavigationButtons { get; set; } = true;

    /// <summary>
    /// Gets, sets recursive value
    /// </summary>
    public static bool EnableRecursiveLoading { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that Windows File Explorer sort order is used if possible
    /// </summary>
    public static bool ShouldUseExplorerSortOrder { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that images order should be grouped by directory
    /// </summary>
    public static bool ShouldGroupImagesByDirectory { get; set; } = false;

    /// <summary>
    /// Gets, sets showing/loading hidden images
    /// </summary>
    public static bool ShouldLoadHiddenImages { get; set; } = false;

    /// <summary>
    /// Gets, sets value specifying that Window Fit mode is on
    /// </summary>
    public static bool EnableWindowFit { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates the window should be always center in Window Fit mode
    /// </summary>
    public static bool CenterWindowFit { get; set; } = true;

    /// <summary>
    /// Displays the embedded thumbnail for RAW formats if found.
    /// </summary>
    public static bool UseEmbeddedThumbnailRawFormats { get; set; } = false;

    /// <summary>
    /// Displays the embedded thumbnail for other formats if found.
    /// </summary>
    public static bool UseEmbeddedThumbnailOtherFormats { get; set; } = false;

    /// <summary>
    /// Gets, sets value indicates that image preview is shown while the image is being loaded.
    /// </summary>
    public static bool ShowImagePreview { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that image fading transition is used while it's being loaded.
    /// </summary>
    public static bool EnableImageTransition { get; set; } = true;

    /// <summary>
    /// Enables / Disables copy multiple files.
    /// </summary>
    public static bool EnableCopyMultipleFiles { get; set; } = true;

    /// <summary>
    /// Enables / Disables cut multiple files.
    /// </summary>
    public static bool EnableCutMultipleFiles { get; set; } = true;

    /// <summary>
    /// Enables / Disables the file system watcher.
    /// </summary>
    public static bool EnableRealTimeFileUpdate { get; set; } = true;

    /// <summary>
    /// Gets, sets value indicates that ImageGlass should open the new image file added in the viewing folder.
    /// </summary>
    public static bool ShouldAutoOpenNewAddedImage { get; set; } = false;


    #endregion // Boolean items


    #region Number items

    /// <summary>
    /// Gets, sets 'Left' position of main window
    /// </summary>
    public static int FrmMainPositionX { get; set; } = 200;

    /// <summary>
    /// Gets, sets 'Top' position of main window
    /// </summary>
    public static int FrmMainPositionY { get; set; } = 200;

    /// <summary>
    /// Gets, sets width of main window
    /// </summary>
    public static int FrmMainWidth { get; set; } = 1200;

    /// <summary>
    /// Gets, sets height of main window
    /// </summary>
    public static int FrmMainHeight { get; set; } = 800;

    /// <summary>
    /// Gets, sets 'Left' position of settings window
    /// </summary>
    public static int FrmSettingsPositionX { get; set; } = 200;

    /// <summary>
    /// Gets, sets 'Top' position of settings window
    /// </summary>
    public static int FrmSettingsPositionY { get; set; } = 200;

    /// <summary>
    /// Gets, sets width of settings window
    /// </summary>
    public static int FrmSettingsWidth { get; set; } = 1050;

    /// <summary>
    /// Gets, sets height of settings window
    /// </summary>
    public static int FrmSettingsHeight { get; set; } = 750;

    /// <summary>
    /// Gets, sets the panning speed.
    /// Value range is from 0 to 100.
    /// </summary>
    public static float PanSpeed { get; set; } = 20f;

    /// <summary>
    /// Gets, sets the zooming speed.
    /// Value range is from -500 to 500.
    /// </summary>
    public static float ZoomSpeed { get; set; } = 0;

    /// <summary>
    /// Gets, sets the version that requires to launch First-launch configuration dialog.
    /// </summary>
    public static int FirstLaunchVersion { get; set; } = 0;

    /// <summary>
    /// Gets, sets slide show interval (minimum value if it's random)
    /// </summary>
    public static float SlideshowInterval { get; set; } = 5f;

    /// <summary>
    /// Gets, sets the maximum slide show interval value
    /// </summary>
    public static float SlideshowIntervalTo { get; set; } = 5f;

    /// <summary>
    /// Gets, sets value of thumbnail dimension in pixel
    /// </summary>
    public static int ThumbnailSize { get; set; } = 50;

    /// <summary>
    /// Gets, sets the maximum size in MB of thumbnail persistent cache.
    /// </summary>
    public static int GalleryCacheSizeInMb { get; set; } = 400;

    /// <summary>
    /// Gets, sets number of thumbnail columns displayed in vertical gallery.
    /// </summary>
    public static int GalleryColumns { get; set; } = 3;

    /// <summary>
    /// Gets, sets the number of images cached by <see cref="Base.Services.ImageBooster"/>.
    /// </summary>
    public static int ImageBoosterCacheCount { get; set; } = 1;

    /// <summary>
    /// Gets, sets the maximum image dimension when caching by <see cref="Base.Services.ImageBooster"/>.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public static int ImageBoosterCacheMaxDimension { get; set; } = 8_000;

    /// <summary>
    /// Gets, sets the maximum image file size (in MB) when caching by <see cref="Base.Services.ImageBooster"/>.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public static float ImageBoosterCacheMaxFileSizeInMb { get; set; } = 100f;

    /// <summary>
    /// Gets, sets fixed width on zooming
    /// </summary>
    public static float ZoomLockValue { get; set; } = 100f;

    /// <summary>
    /// Gets, sets toolbar icon height
    /// </summary>
    public static int ToolbarIconHeight { get; set; } = Constants.TOOLBAR_ICON_HEIGHT;

    /// <summary>
    /// Gets, sets value of image quality for editting
    /// </summary>
    public static int ImageEditQuality { get; set; } = 80;

    /// <summary>
    /// Gets, sets value of duration to display the in-app message
    /// </summary>
    public static int InAppMessageDuration { get; set; } = 2000;

    /// <summary>
    /// Gets, sets the minimum width of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="UseEmbeddedThumbnailRawFormats"/> or <see cref="UseEmbeddedThumbnailOtherFormats"/> is <c>true</c>.
    /// </summary>
    public static int EmbeddedThumbnailMinWidth { get; set; } = 0;

    /// <summary>
    /// Gets, sets the minimum height of the embedded thumbnail to use for displaying
    /// image when the setting <see cref="UseEmbeddedThumbnailRawFormats"/> or <see cref="UseEmbeddedThumbnailOtherFormats"/> is <c>true</c>.
    /// </summary>
    public static int EmbeddedThumbnailMinHeight { get; set; } = 0;

    #endregion // Number items


    #region String items

    /// <summary>
    /// Gets, sets color profile string. It can be a defined name or ICC/ICM file path
    /// </summary>
    public static string ColorProfile { get; set; } = nameof(ColorProfileOption.CurrentMonitorProfile);

    /// <summary>
    /// Gets, sets the last time to check for update. Set it to "0" to disable auto-update.
    /// </summary>
    public static string AutoUpdate { get; set; } = "7/22/2010 12:13:08";

    /// <summary>
    /// Gets, sets the absolute file path of the last seen image
    /// </summary>
    public static string LastSeenImagePath { get; set; } = "";

    /// <summary>
    /// Gets, sets the theme name for dark mode.
    /// </summary>
    public static string DarkTheme { get; set; } = "";

    /// <summary>
    /// Gets, sets the theme name for light mode.
    /// </summary>
    public static string LightTheme { get; set; } = "";

    #endregion


    #region Array items

    /// <summary>
    /// Gets, sets zoom levels of the viewer
    /// </summary>
    public static float[] ZoomLevels { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets, sets the list of apps for edit action.
    /// </summary>
    public static Dictionary<string, EditApp?> EditApps { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of supported image formats
    /// </summary>
    public static HashSet<string> AllFormats { get; set; } = new();

    /// <summary>
    /// Gets, sets the list of formats that only load the first page forcefully
    /// </summary>
    public static HashSet<string> SinglePageFormats { get; set; } = new() { "*.heic;*.heif;*.psd;" };

    /// <summary>
    /// Gets, sets the list of toolbar buttons
    /// </summary>
    public static List<ToolbarItemModel> ToolbarItems { get; set; } = DefaultToolbarItems;

    /// <summary>
    /// Gets, sets the items for displaying image info
    /// </summary>
    public static List<string> InfoItems { get; set; } = DefaultInfoItems;

    /// <summary>
    /// Gets, sets hotkeys list of menu
    /// </summary>
    public static Dictionary<string, List<Hotkey>> MenuHotkeys = new();

    /// <summary>
    /// Gets, sets mouse click actions
    /// </summary>
    public static Dictionary<MouseClickEvent, ToggleAction> MouseClickActions = new();

    /// <summary>
    /// Gets, sets mouse wheel actions
    /// </summary>
    public static Dictionary<MouseWheelEvent, MouseWheelAction> MouseWheelActions = new();

    /// <summary>
    /// Gets, sets layout for FrmMain. Syntax:
    /// <c>Dictionary["ControlName", "DockStyle;order"]</c>
    /// </summary>
    public static Dictionary<string, string?> Layout { get; set; } = new();

    /// <summary>
    /// Gets, sets tools.
    /// </summary>
    public static List<IgTool?> Tools { get; set; } = new()
    {
        new IgTool()
        {
            ToolId = Constants.IGTOOL_EXIFTOOL,
            ToolName = "ExifGlass - EXIF metadata viewer",
            Executable = "exifglass",
            Argument = Constants.FILE_MACRO,
            IsIntegrated = true,
        },
    };

    #endregion


    #region Enum items

    /// <summary>
    /// Gets, sets state of main window
    /// </summary>
    public static WindowState FrmMainState { get; set; } = WindowState.Normal;

    /// <summary>
    /// Gets, sets state of settings window
    /// </summary>
    public static WindowState FrmSettingsState { get; set; } = WindowState.Normal;

    /// <summary>
    /// Gets, sets image loading order
    /// </summary>
    public static ImageOrderBy ImageLoadingOrder { get; set; } = ImageOrderBy.Name;

    /// <summary>
    /// Gets, sets image loading order type
    /// </summary>
    public static ImageOrderType ImageLoadingOrderType { get; set; } = ImageOrderType.Asc;

    /// <summary>
    /// Gets, sets zoom mode value
    /// </summary>
    public static ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>less than or equals 100%</c>.
    /// </summary>
    public static ImageInterpolation ImageInterpolationScaleDown { get; set; } = ImageInterpolation.SampleLinear;

    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    public static ImageInterpolation ImageInterpolationScaleUp { get; set; } = ImageInterpolation.NearestNeighbor;


    /// <summary>
    /// Gets, sets value indicates what happens after clicking Edit menu
    /// </summary>
    public static AfterEditAppAction AfterEditingAction { get; set; } = AfterEditAppAction.Nothing;


    /// <summary>
    /// Gets, sets the interpolation mode to render the viewing image when the zoom factor is <c>greater than 100%</c>.
    /// </summary>
    public static BackdropStyle WindowBackdrop { get; set; } = BackdropStyle.Mica;

    #endregion // Enum items


    #region Other types items

    /// <summary>
    /// Gets, sets background color of of the main window
    /// </summary>
    public static Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets, sets background color of slideshow
    /// </summary>
    public static Color SlideshowBackgroundColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets, sets language pack
    /// </summary>
    public static IgLang Language { get; set; }

    #endregion // Other types items

    #endregion // Setting items



    #region Public static functions

    /// <summary>
    /// Loads and parsse configs from file
    /// </summary>
    public static void Load()
    {
        var items = Source.LoadUserConfigs();

        // save the config for all tools
        ToolSettings = items.GetValue(nameof(ToolSettings)).GetValue(nameof(ToolSettings), new ExpandoObject());


        // Boolean values
        #region Boolean items

        EnableSlideshow = items.GetValue(nameof(EnableSlideshow), EnableSlideshow);
        HideMainWindowInSlideshow = items.GetValue(nameof(HideMainWindowInSlideshow), HideMainWindowInSlideshow);
        ShowSlideshowCountdown = items.GetValue(nameof(ShowSlideshowCountdown), ShowSlideshowCountdown);
        UseRandomIntervalForSlideshow = items.GetValue(nameof(UseRandomIntervalForSlideshow), UseRandomIntervalForSlideshow);
        EnableLoopSlideshow = items.GetValue(nameof(EnableLoopSlideshow), EnableLoopSlideshow);
        EnableFullscreenSlideshow = items.GetValue(nameof(EnableFullscreenSlideshow), EnableFullscreenSlideshow);
        EnableFrameless = items.GetValue(nameof(EnableFrameless), EnableFrameless);
        EnableFullScreen = items.GetValue(nameof(EnableFullScreen), EnableFullScreen);
        ShowGallery = items.GetValue(nameof(ShowGallery), ShowGallery);
        ShowGalleryScrollbars = items.GetValue(nameof(ShowGalleryScrollbars), ShowGalleryScrollbars);
        ShowGalleryFileName = items.GetValue(nameof(ShowGalleryFileName), ShowGalleryFileName);
        ShowWelcomeImage = items.GetValue(nameof(ShowWelcomeImage), ShowWelcomeImage);
        ShowToolbar = items.GetValue(nameof(ShowToolbar), ShowToolbar);
        EnableLoopBackNavigation = items.GetValue(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        ShowCheckerboard = items.GetValue(nameof(ShowCheckerboard), ShowCheckerboard);
        ShowCheckerboardOnlyImageRegion = items.GetValue(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        EnableMultiInstances = items.GetValue(nameof(EnableMultiInstances), EnableMultiInstances);
        EnableWindowTopMost = items.GetValue(nameof(EnableWindowTopMost), EnableWindowTopMost);
        ShowDeleteConfirmation = items.GetValue(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        ShowSaveOverrideConfirmation = items.GetValue(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        ShouldPreserveModifiedDate = items.GetValue(nameof(ShouldPreserveModifiedDate), ShouldPreserveModifiedDate);
        ShowNewVersionIndicator = items.GetValue(nameof(ShowNewVersionIndicator), ShowNewVersionIndicator);
        EnableCenterToolbar = items.GetValue(nameof(EnableCenterToolbar), EnableCenterToolbar);
        ShouldOpenLastSeenImage = items.GetValue(nameof(ShouldOpenLastSeenImage), ShouldOpenLastSeenImage);
        ShouldUseColorProfileForAll = items.GetValue(nameof(ShouldUseColorProfileForAll), ShouldUseColorProfileForAll);
        EnableNavigationButtons = items.GetValue(nameof(EnableNavigationButtons), EnableNavigationButtons);
        EnableRecursiveLoading = items.GetValue(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        ShouldUseExplorerSortOrder = items.GetValue(nameof(ShouldUseExplorerSortOrder), ShouldUseExplorerSortOrder);
        ShouldGroupImagesByDirectory = items.GetValue(nameof(ShouldGroupImagesByDirectory), ShouldGroupImagesByDirectory);
        ShouldLoadHiddenImages = items.GetValue(nameof(ShouldLoadHiddenImages), ShouldLoadHiddenImages);
        EnableWindowFit = items.GetValue(nameof(EnableWindowFit), EnableWindowFit);
        CenterWindowFit = items.GetValue(nameof(CenterWindowFit), CenterWindowFit);
        UseEmbeddedThumbnailRawFormats = items.GetValue(nameof(UseEmbeddedThumbnailRawFormats), UseEmbeddedThumbnailRawFormats);
        UseEmbeddedThumbnailOtherFormats = items.GetValue(nameof(UseEmbeddedThumbnailOtherFormats), UseEmbeddedThumbnailOtherFormats);
        ShowImagePreview = items.GetValue(nameof(ShowImagePreview), ShowImagePreview);
        EnableImageTransition = items.GetValue(nameof(EnableImageTransition), EnableImageTransition);
        EnableCopyMultipleFiles = items.GetValue(nameof(EnableCopyMultipleFiles), EnableCopyMultipleFiles);
        EnableCutMultipleFiles = items.GetValue(nameof(EnableCutMultipleFiles), EnableCutMultipleFiles);
        EnableRealTimeFileUpdate = items.GetValue(nameof(EnableRealTimeFileUpdate), EnableRealTimeFileUpdate);
        ShouldAutoOpenNewAddedImage = items.GetValue(nameof(ShouldAutoOpenNewAddedImage), ShouldAutoOpenNewAddedImage);

        HideToolbarInFullscreen = items.GetValue(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        HideGalleryInFullscreen = items.GetValue(nameof(HideGalleryInFullscreen), HideGalleryInFullscreen);

        #endregion


        // Number values
        #region Number items

        // FrmMain
        FrmMainPositionX = items.GetValue(nameof(FrmMainPositionX), FrmMainPositionX);
        FrmMainPositionY = items.GetValue(nameof(FrmMainPositionY), FrmMainPositionY);
        FrmMainWidth = items.GetValue(nameof(FrmMainWidth), FrmMainWidth);
        FrmMainHeight = items.GetValue(nameof(FrmMainHeight), FrmMainHeight);

        // FrmSettings
        FrmSettingsPositionX = items.GetValue(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        FrmSettingsPositionY = items.GetValue(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        FrmSettingsWidth = items.GetValue(nameof(FrmSettingsWidth), FrmSettingsWidth);
        FrmSettingsHeight = items.GetValue(nameof(FrmSettingsHeight), FrmSettingsHeight);

        PanSpeed = items.GetValue(nameof(PanSpeed), PanSpeed);
        ZoomSpeed = items.GetValue(nameof(ZoomSpeed), ZoomSpeed);
        FirstLaunchVersion = items.GetValue(nameof(FirstLaunchVersion), FirstLaunchVersion);

        #region Slideshow
        SlideshowInterval = items.GetValue(nameof(SlideshowInterval), SlideshowInterval);
        if (SlideshowInterval < 1) SlideshowInterval = 5f;

        SlideshowIntervalTo = items.GetValue(nameof(SlideshowIntervalTo), SlideshowIntervalTo);
        SlideshowIntervalTo = Math.Max(SlideshowIntervalTo, SlideshowInterval);
        #endregion

        #region Load gallery thumbnail width & position
        ThumbnailSize = items.GetValue(nameof(ThumbnailSize), ThumbnailSize);
        ThumbnailSize = Math.Max(20, ThumbnailSize);
        GalleryCacheSizeInMb = items.GetValue(nameof(GalleryCacheSizeInMb), GalleryCacheSizeInMb);

        GalleryColumns = items.GetValue(nameof(GalleryColumns), GalleryColumns);
        GalleryColumns = Math.Max(1, GalleryColumns);
        #endregion

        ImageBoosterCacheCount = items.GetValue(nameof(ImageBoosterCacheCount), ImageBoosterCacheCount);
        ImageBoosterCacheCount = Math.Max(0, Math.Min(ImageBoosterCacheCount, 10));

        ImageBoosterCacheMaxDimension = items.GetValue(nameof(ImageBoosterCacheMaxDimension), ImageBoosterCacheMaxDimension);
        ImageBoosterCacheMaxFileSizeInMb = items.GetValue(nameof(ImageBoosterCacheMaxFileSizeInMb), ImageBoosterCacheMaxFileSizeInMb);

        ZoomLockValue = items.GetValue(nameof(ZoomLockValue), ZoomLockValue);
        if (ZoomLockValue < 0) ZoomLockValue = 100f;

        ToolbarIconHeight = items.GetValue(nameof(ToolbarIconHeight), ToolbarIconHeight);
        ImageEditQuality = items.GetValue(nameof(ImageEditQuality), ImageEditQuality);
        InAppMessageDuration = items.GetValue(nameof(InAppMessageDuration), InAppMessageDuration);
        EmbeddedThumbnailMinWidth = items.GetValue(nameof(EmbeddedThumbnailMinWidth), EmbeddedThumbnailMinWidth);
        EmbeddedThumbnailMinHeight = items.GetValue(nameof(EmbeddedThumbnailMinHeight), EmbeddedThumbnailMinHeight);

        #endregion


        // Enum values
        #region Enum items

        FrmMainState = items.GetValue(nameof(FrmMainState), FrmMainState);
        FrmSettingsState = items.GetValue(nameof(FrmSettingsState), FrmSettingsState);
        ImageLoadingOrder = items.GetValue(nameof(ImageLoadingOrder), ImageLoadingOrder);
        ImageLoadingOrderType = items.GetValue(nameof(ImageLoadingOrderType), ImageLoadingOrderType);
        ZoomMode = items.GetValue(nameof(ZoomMode), ZoomMode);
        ImageInterpolationScaleDown = items.GetValue(nameof(ImageInterpolationScaleDown), ImageInterpolationScaleDown);
        ImageInterpolationScaleUp = items.GetValue(nameof(ImageInterpolationScaleUp), ImageInterpolationScaleUp);
        AfterEditingAction = items.GetValue(nameof(AfterEditingAction), AfterEditingAction);
        WindowBackdrop = items.GetValue(nameof(WindowBackdrop), WindowBackdrop);

        #endregion


        // String values
        #region String items

        ColorProfile = items.GetValue(nameof(ColorProfile), ColorProfile);
        ColorProfile = BHelper.GetCorrectColorProfileName(ColorProfile);

        AutoUpdate = items.GetValue(nameof(AutoUpdate), AutoUpdate);
        LastSeenImagePath = items.GetValue(nameof(LastSeenImagePath), LastSeenImagePath);
        DarkTheme = items.GetValue(nameof(DarkTheme), DarkTheme);
        LightTheme = items.GetValue(nameof(LightTheme), LightTheme);

        #endregion


        // Array values
        #region Array items

        // ZoomLevels
        ZoomLevels = items.GetSection(nameof(ZoomLevels))
            .GetChildren()
            .Select(i =>
            {
                // convert % to float
                try { return i.Get<float>() / 100f; } catch { }
                return -1;
            })
            .OrderBy(i => i)
            .Where(i => i > 0)
            .Distinct()
            .ToArray();

        #region EditApps

        EditApps = items.GetSection(nameof(EditApps))
            .GetChildren()
            .ToDictionary(
                i => i.Key.ToLowerInvariant(),
                i => i.Get<EditApp>()
            );

        #endregion

        #region ImageFormats

        var formats = items.GetValue(nameof(AllFormats), Constants.IMAGE_FORMATS);
        AllFormats = GetImageFormats(formats);

        formats = items.GetValue(nameof(SinglePageFormats), string.Join(";", SinglePageFormats));
        SinglePageFormats = GetImageFormats(formats);

        #endregion


        // toolbar items
        var toolbarItems = items.GetSection(nameof(ToolbarItems))
            .GetChildren()
            .Select(i => i.Get<ToolbarItemModel>());
        ToolbarItems = toolbarItems.Any() ? toolbarItems.ToList() : DefaultToolbarItems;


        // info items
        var infoItems = items.GetSection(nameof(InfoItems))
            .GetChildren()
            .Select(i => i.Get<string>());
        InfoItems = infoItems.Any() ? infoItems.ToList() : DefaultInfoItems;


        // hotkeys for menu
        var stringArrDict = items.GetSection(nameof(MenuHotkeys))
            .GetChildren()
            .ToDictionary(
                i => i.Key,
                i => i.GetChildren().Select(i => i.Value).ToArray()
            );
        MenuHotkeys = ParseHotkeys(stringArrDict);


        // MouseClickActions
        MouseClickActions = items.GetSection(nameof(MouseClickActions))
            .GetChildren()
            .ToDictionary(
                i => BHelper.ParseEnum<MouseClickEvent>(i.Key),
                i => i.Get<ToggleAction>());


        // MouseWheelActions
        MouseWheelActions = items.GetSection(nameof(MouseWheelActions))
            .GetChildren()
            .ToDictionary(
                i => BHelper.ParseEnum<MouseWheelEvent>(i.Key),
                i => BHelper.ParseEnum<MouseWheelAction>(i.Value));


        // Layout
        Layout = items.GetSection(nameof(Layout))
            .GetChildren()
            .ToDictionary(
                i => i.Key,
                i => i.Get<string>()
            );


        // Tools
        var toolsList = items.GetSection(nameof(Tools))
            .GetChildren()
            .Select(i => i.Get<IgTool>())
            .Where(i => i != null && !i.IsEmpty);
        if (toolsList != null && toolsList.Any())
        {
            Tools.Clear();
            Tools = toolsList.ToList();
        }

        #endregion


        // Other types values
        #region Other types items

        #region Language
        var langPath = items.GetValue(nameof(Language), "English");
        Language = new IgLang(langPath, App.StartUpDir(Dir.Languages));
        #endregion


        // must load before Theme
        #region BackgroundColor

        var bgValue = items.GetValue(nameof(BackgroundColor), string.Empty);

        if (string.IsNullOrEmpty(bgValue))
        {
            BackgroundColor = Theme.Colors.BgColor;
        }
        else
        {
            BackgroundColor = ThemeUtils.ColorFromHex(bgValue);
        }
        #endregion


        // load theme
        LoadThemePack(WinColorsApi.IsDarkMode, true, true);


        #region SlideshowBackgroundColor

        bgValue = items.GetValue(nameof(SlideshowBackgroundColor), string.Empty);
        if (!string.IsNullOrEmpty(bgValue))
        {
            SlideshowBackgroundColor = ThemeUtils.ColorFromHex(bgValue);
        }

        #endregion

        #endregion


        // initialize Magick.NET
        PhotoCodec.InitMagickNET();

        // listen to system events
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }


    /// <summary>
    /// Parses and writes configs to file
    /// </summary>
    public static async Task WriteAsync()
    {
        var jsonFile = App.ConfigDir(PathType.File, Source.UserFilename);
        var jsonObj = PrepareJsonSettingsObject();

        await BHelper.WriteJsonAsync(jsonFile, jsonObj);
    }


    /// <summary>
    /// Converts all settings to ExpandoObject for parsing JSON.
    /// </summary>
    public static dynamic PrepareJsonSettingsObject()
    {
        var settings = new ExpandoObject();

        var metadata = new ConfigMetadata()
        {
            Description = _source.Description,
            Version = _source.Version,
        };

        settings.TryAdd("_Metadata", metadata);


        #region Boolean items

        settings.TryAdd(nameof(EnableSlideshow), EnableSlideshow);
        settings.TryAdd(nameof(HideMainWindowInSlideshow), HideMainWindowInSlideshow);
        settings.TryAdd(nameof(ShowSlideshowCountdown), ShowSlideshowCountdown);
        settings.TryAdd(nameof(UseRandomIntervalForSlideshow), UseRandomIntervalForSlideshow);
        settings.TryAdd(nameof(EnableLoopSlideshow), EnableLoopSlideshow);
        settings.TryAdd(nameof(EnableFullscreenSlideshow), EnableFullscreenSlideshow);
        settings.TryAdd(nameof(EnableFrameless), EnableFrameless);
        settings.TryAdd(nameof(EnableFullScreen), EnableFullScreen);
        settings.TryAdd(nameof(ShowGallery), ShowGallery);
        settings.TryAdd(nameof(ShowGalleryScrollbars), ShowGalleryScrollbars);
        settings.TryAdd(nameof(ShowGalleryFileName), ShowGalleryFileName);
        settings.TryAdd(nameof(ShowWelcomeImage), ShowWelcomeImage);
        settings.TryAdd(nameof(ShowToolbar), ShowToolbar);
        settings.TryAdd(nameof(EnableLoopBackNavigation), EnableLoopBackNavigation);
        settings.TryAdd(nameof(ShowCheckerboard), ShowCheckerboard);
        settings.TryAdd(nameof(ShowCheckerboardOnlyImageRegion), ShowCheckerboardOnlyImageRegion);
        settings.TryAdd(nameof(EnableMultiInstances), EnableMultiInstances);
        settings.TryAdd(nameof(EnableWindowTopMost), EnableWindowTopMost);
        settings.TryAdd(nameof(ShowDeleteConfirmation), ShowDeleteConfirmation);
        settings.TryAdd(nameof(ShowSaveOverrideConfirmation), ShowSaveOverrideConfirmation);
        settings.TryAdd(nameof(ShouldPreserveModifiedDate), ShouldPreserveModifiedDate);
        settings.TryAdd(nameof(ShowNewVersionIndicator), ShowNewVersionIndicator);
        settings.TryAdd(nameof(EnableCenterToolbar), EnableCenterToolbar);
        settings.TryAdd(nameof(ShouldOpenLastSeenImage), ShouldOpenLastSeenImage);
        settings.TryAdd(nameof(ShouldUseColorProfileForAll), ShouldUseColorProfileForAll);
        settings.TryAdd(nameof(EnableNavigationButtons), EnableNavigationButtons);
        settings.TryAdd(nameof(EnableRecursiveLoading), EnableRecursiveLoading);
        settings.TryAdd(nameof(ShouldUseExplorerSortOrder), ShouldUseExplorerSortOrder);
        settings.TryAdd(nameof(ShouldGroupImagesByDirectory), ShouldGroupImagesByDirectory);
        settings.TryAdd(nameof(ShouldLoadHiddenImages), ShouldLoadHiddenImages);
        settings.TryAdd(nameof(EnableWindowFit), EnableWindowFit);
        settings.TryAdd(nameof(CenterWindowFit), CenterWindowFit);
        settings.TryAdd(nameof(UseEmbeddedThumbnailRawFormats), UseEmbeddedThumbnailRawFormats);
        settings.TryAdd(nameof(UseEmbeddedThumbnailOtherFormats), UseEmbeddedThumbnailOtherFormats);
        settings.TryAdd(nameof(ShowImagePreview), ShowImagePreview);
        settings.TryAdd(nameof(EnableImageTransition), EnableImageTransition);
        settings.TryAdd(nameof(EnableCopyMultipleFiles), EnableCopyMultipleFiles);
        settings.TryAdd(nameof(EnableCutMultipleFiles), EnableCutMultipleFiles);
        settings.TryAdd(nameof(EnableRealTimeFileUpdate), EnableRealTimeFileUpdate);
        settings.TryAdd(nameof(ShouldAutoOpenNewAddedImage), ShouldAutoOpenNewAddedImage);

        settings.TryAdd(nameof(HideToolbarInFullscreen), HideToolbarInFullscreen);
        settings.TryAdd(nameof(HideGalleryInFullscreen), HideGalleryInFullscreen);

        #endregion


        #region Number items

        // FrmMain
        settings.TryAdd(nameof(FrmMainPositionX), FrmMainPositionX);
        settings.TryAdd(nameof(FrmMainPositionY), FrmMainPositionY);
        settings.TryAdd(nameof(FrmMainWidth), FrmMainWidth);
        settings.TryAdd(nameof(FrmMainHeight), FrmMainHeight);

        // FrmSettings
        settings.TryAdd(nameof(FrmSettingsPositionX), FrmSettingsPositionX);
        settings.TryAdd(nameof(FrmSettingsPositionY), FrmSettingsPositionY);
        settings.TryAdd(nameof(FrmSettingsWidth), FrmSettingsWidth);
        settings.TryAdd(nameof(FrmSettingsHeight), FrmSettingsHeight);

        settings.TryAdd(nameof(PanSpeed), PanSpeed);
        settings.TryAdd(nameof(ZoomSpeed), ZoomSpeed);
        settings.TryAdd(nameof(FirstLaunchVersion), FirstLaunchVersion);
        settings.TryAdd(nameof(SlideshowInterval), SlideshowInterval);
        settings.TryAdd(nameof(SlideshowIntervalTo), SlideshowIntervalTo);
        settings.TryAdd(nameof(ThumbnailSize), ThumbnailSize);
        settings.TryAdd(nameof(GalleryCacheSizeInMb), GalleryCacheSizeInMb);
        settings.TryAdd(nameof(GalleryColumns), GalleryColumns);
        settings.TryAdd(nameof(ImageBoosterCacheCount), ImageBoosterCacheCount);
        settings.TryAdd(nameof(ImageBoosterCacheMaxDimension), ImageBoosterCacheMaxDimension);
        settings.TryAdd(nameof(ImageBoosterCacheMaxFileSizeInMb), ImageBoosterCacheMaxFileSizeInMb);
        settings.TryAdd(nameof(ZoomLockValue), ZoomLockValue);
        settings.TryAdd(nameof(ToolbarIconHeight), ToolbarIconHeight);
        settings.TryAdd(nameof(ImageEditQuality), ImageEditQuality);
        settings.TryAdd(nameof(InAppMessageDuration), InAppMessageDuration);
        settings.TryAdd(nameof(EmbeddedThumbnailMinWidth), EmbeddedThumbnailMinWidth);
        settings.TryAdd(nameof(EmbeddedThumbnailMinHeight), EmbeddedThumbnailMinHeight);

        #endregion


        #region Enum items

        settings.TryAdd(nameof(FrmMainState), FrmMainState.ToString());
        settings.TryAdd(nameof(FrmSettingsState), FrmSettingsState.ToString());
        settings.TryAdd(nameof(ImageLoadingOrder), ImageLoadingOrder.ToString());
        settings.TryAdd(nameof(ImageLoadingOrderType), ImageLoadingOrderType.ToString());
        settings.TryAdd(nameof(ZoomMode), ZoomMode.ToString());
        settings.TryAdd(nameof(ImageInterpolationScaleDown), ImageInterpolationScaleDown);
        settings.TryAdd(nameof(ImageInterpolationScaleUp), ImageInterpolationScaleUp);
        settings.TryAdd(nameof(AfterEditingAction), AfterEditingAction.ToString());
        settings.TryAdd(nameof(WindowBackdrop), WindowBackdrop);

        #endregion


        #region String items

        settings.TryAdd(nameof(ColorProfile), ColorProfile);
        settings.TryAdd(nameof(AutoUpdate), AutoUpdate);
        settings.TryAdd(nameof(LastSeenImagePath), LastSeenImagePath);
        settings.TryAdd(nameof(DarkTheme), DarkTheme);
        settings.TryAdd(nameof(LightTheme), LightTheme);

        #endregion


        #region Other types items

        settings.TryAdd(nameof(BackgroundColor), ThemeUtils.ColorToHex(BackgroundColor));
        settings.TryAdd(nameof(SlideshowBackgroundColor), ThemeUtils.ColorToHex(SlideshowBackgroundColor));
        settings.TryAdd(nameof(Language), Path.GetFileName(Language.FileName));

        #endregion


        #region Array items

        settings.TryAdd(nameof(ZoomLevels), ZoomLevels.Select(i => Math.Round(i * 100, 2)));
        settings.TryAdd(nameof(EditApps), EditApps);
        settings.TryAdd(nameof(AllFormats), GetImageFormats(AllFormats));
        settings.TryAdd(nameof(SinglePageFormats), GetImageFormats(SinglePageFormats));
        settings.TryAdd(nameof(InfoItems), InfoItems);
        settings.TryAdd(nameof(MenuHotkeys), ParseHotkeys(MenuHotkeys));
        settings.TryAdd(nameof(MouseClickActions), MouseClickActions);
        settings.TryAdd(nameof(MouseWheelActions), MouseWheelActions);
        settings.TryAdd(nameof(ToolbarItems), ToolbarItems);
        settings.TryAdd(nameof(Layout), Layout);
        settings.TryAdd(nameof(Tools), Tools);

        #endregion


        // Tools' settings
        settings.TryAdd(nameof(ToolSettings), (object)ToolSettings);

        return settings;
    }


    /// <summary>
    /// Gets property by name.
    /// </summary>
    public static PropertyInfo? GetProp(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        // Find the public property in Config
        var prop = typeof(Config)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(i => i.Name == name);

        return prop;
    }


    /// <summary>
    /// Loads theme pack <see cref="Config.Theme"/> and only theme colors.
    /// </summary>
    /// <param name="darkMode">
    /// Determine which theme should be loaded: <see cref="DarkTheme"/> or <see cref="LightTheme"/>.
    /// </param>
    /// <param name="useFallBackTheme">
    /// If theme pack is invalid, should load the default theme pack <see cref="Constants.DEFAULT_THEME"/>?
    /// </param>
    /// <param name="throwIfThemeInvalid">
    /// If theme pack is invalid, should throw exception?
    /// </param>
    /// <exception cref="InvalidDataException"></exception>
    public static void LoadThemePack(bool darkMode, bool useFallBackTheme, bool throwIfThemeInvalid)
    {
        var themeFolderName = darkMode ? DarkTheme : LightTheme;
        if (string.IsNullOrEmpty(themeFolderName))
        {
            themeFolderName = Constants.DEFAULT_THEME;
        }

        // theme pack is already updated
        if (themeFolderName.Equals(Theme.FolderName, StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        var th = new IgTheme(App.ConfigDir(PathType.Dir, Dir.Themes, themeFolderName));

        if (!th.IsValid)
        {
            if (useFallBackTheme)
            {
                th.Dispose();
                th = null;

                // load default theme
                th = new(App.StartUpDir(Dir.Themes, Constants.DEFAULT_THEME));
            }
        }

        if (!th.IsValid && throwIfThemeInvalid)
        {
            th.Dispose();
            th = null;

            throw new InvalidDataException($"Unable to load '{th.FolderName}' theme pack. " +
                $"Please make sure '{th.FolderName}\\{IgTheme.CONFIG_FILE}' file is valid.");
        }

        // update the name of dark/light theme
        if (darkMode) DarkTheme = th.FolderName;
        else LightTheme = th.FolderName;

        // load theme settings
        th.ReloadThemeSettings();

        // load theme colors
        th.ReloadThemeColors();

        // load background color
        if (Config.BackgroundColor == Theme.Colors.BgColor)
        {
            Config.BackgroundColor = th.Colors.BgColor;
        }


        // set to the current theme
        Theme?.Dispose();
        Theme = th;
    }


    /// <summary>
    /// Updates form icon using theme setting.
    /// </summary>
    public static void UpdateFormIcon(Form frm)
    {
        // Icon theming
        if (!Config.Theme.Settings.ShowTitlebarLogo)
        {
            frm.Icon = Icon.FromHandle(new Bitmap(64, 64).GetHicon());
            FormIconApi.SetTaskbarIcon(frm, Config.Theme.Settings.AppLogo.GetHicon());
        }
        else
        {
            frm.Icon = Icon.FromHandle(Config.Theme.Settings.AppLogo.GetHicon());
        }
    }


    /// <summary>
    /// Parses string dictionary to hotkey dictionary
    /// </summary>
    public static Dictionary<string, List<Hotkey>> ParseHotkeys(Dictionary<string, string?[]>? dict)
    {
        var result = new Dictionary<string, List<Hotkey>>();
        if (dict == null) return result;

        foreach (var item in dict)
        {
            var keyList = new List<Hotkey>();

            // sample item: { "MnuOpen": ["O", "Ctrl+O"] }
            foreach (var strKey in item.Value)
            {
                try
                {
                    keyList.Add(new Hotkey(strKey));
                }
                catch { }
            }


            if (result.ContainsKey(item.Key))
            {
                result[item.Key].Clear();
                result[item.Key].AddRange(keyList);
            }
            else
            {
                result.Add(item.Key, keyList);
            }
        };

        return result;
    }


    /// <summary>
    /// Parses hotkey dictionary to string dictionary.
    /// </summary>
    public static Dictionary<string, string[]> ParseHotkeys(Dictionary<string, List<Hotkey>> dict)
    {
        var result = dict.Select(i => new
        {
            Key = i.Key,
            Value = i.Value.Select(k => k.ToString()),
        }).ToDictionary(i => i.Key, i => i.Value.ToArray());

        return result;
    }


    /// <summary>
    /// Merge <paramref name="srcDict"/> dictionary
    /// into <paramref name="destDict"/> dictionary
    /// </summary>
    public static void MergeHotkeys(ref Dictionary<string, List<Hotkey>> destDict, Dictionary<string, List<Hotkey>> srcDict)
    {
        foreach (var item in srcDict)
        {
            if (destDict.ContainsKey(item.Key))
            {
                destDict[item.Key] = item.Value;
            }
            else
            {
                destDict.Add(item.Key, item.Value);
            }
        }
    }


    /// <summary>
    /// Gets hotkeys string from the action.
    /// </summary>
    public static string GetHotkeyString(Dictionary<string, List<Hotkey>> dict, string action)
    {
        dict.TryGetValue(action, out var hotkeyList);

        if (hotkeyList != null)
        {
            var str = string.Join(", ", hotkeyList.Select(k => k.ToString()));

            return str ?? string.Empty;
        }


        return string.Empty;
    }


    /// <summary>
    /// Gets hotkeys from the action.
    /// </summary>
    public static List<Hotkey> GetHotkey(Dictionary<string, List<Hotkey>> dict, string action)
    {
        dict.TryGetValue(action, out var hotkeyList);

        return hotkeyList ?? new List<Hotkey>(0);
    }


    /// <summary>
    /// Gets hotkey's KeyData.
    /// </summary>
    public static List<Keys> GetHotkeyData(Dictionary<string, List<Hotkey>> dict, string action, Keys defaultValue)
    {
        var keyDataList = GetHotkey(dict, action)
            .Select(k => k.KeyData).ToList();

        return keyDataList ?? new List<Keys>(1) { defaultValue };
    }


    /// <summary>
    /// Gets actions from the input hotkey.
    /// </summary>
    public static List<string> GetHotkeyActions(Dictionary<string, List<Hotkey>> dict, Hotkey hotkey)
    {
        var actions = new HashSet<string>();

        foreach (var item in dict)
        {
            foreach (var key in item.Value)
            {
                var result = string.Compare(key.ToString(), hotkey.ToString(), true);
                if (result == 0)
                {
                    actions.Add(item.Key);
                }
            }
        }

        return actions.ToList();
    }


    #region Popup functions

    /// <summary>
    /// Shows information popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowInfo(
        Form? formOwner,
        string description = "",
        string title = "",
        string heading = "",
        string details = "",
        string note = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_INFO,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        SystemSounds.Question.Play();

        return Popup.ShowDialog(description, title, heading, details, note, StatusType.Info, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Shows warning popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowWarning(
        Form? formOwner,
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        string note = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_WARNING,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        heading ??= Language["_._Warning"];
        SystemSounds.Exclamation.Play();

        return Popup.ShowDialog(description, title, heading, details, note, StatusType.Warning, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Shows error popup widow.
    /// </summary>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="note">Note text.</param>
    /// <param name="buttons">Popup buttons.</param>
    public static PopupResult ShowError(
        Form? formOwner,
        string description = "",
        string title = "",
        string? heading = null,
        string details = "",
        string note = "",
        SHSTOCKICONID? icon = SHSTOCKICONID.SIID_ERROR,
        Image? thumbnail = null,
        PopupButton buttons = PopupButton.OK,
        string optionText = "")
    {
        heading ??= Language["_._Error"];
        SystemSounds.Asterisk.Play();

        return Popup.ShowDialog(description, title, heading, details, note, StatusType.Danger, buttons, icon, thumbnail, optionText, Config.EnableWindowTopMost, formOwner);
    }


    /// <summary>
    /// Show the default error popup for igcmd/igcmd10.exe
    /// </summary>
    /// <returns><see cref="IgExitCode.Error"/></returns>
    public static int ShowDefaultIgCommandError(string igcmdExeName)
    {
        var url = "https://imageglass.org/docs/command-line-utilities";
        var langPath = $"_._IgCommandExe._DefaultError";

        var result = ShowError(null,
            title: Application.ProductName + " v" + Application.ProductVersion,
            heading: Language[$"{langPath}._Heading"],
            description: string.Format(Language[$"{langPath}._Description"], url),
            buttons: PopupButton.LearnMore_Close);


        if (result.ExitResult == PopupExitResult.OK)
        {
            BHelper.OpenUrl(url, $"{igcmdExeName}_invalid_command");
        }

        return (int)IgExitCode.Error;
    }


    /// <summary>
    /// Shows unhandled exception popup
    /// </summary>
    public static void HandleException(Exception ex)
    {
        var exeVersion = FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion;
        var appInfo = Application.ProductName + " v" + exeVersion;
        var osInfo = Environment.OSVersion.VersionString + " " + (Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");

        var langPath = $"_._UnhandledException";
        var description = Language[$"{langPath}._Description"];
        var details =
            $"Version: {appInfo}\r\n" +
            $"Release code: {Constants.APP_CODE}\r\n" +
            $"OS: {osInfo}\r\n\r\n" +

            $"----------------------------------------------------\r\n" +
            $"Error:\r\n\r\n" +
            $"{ex.Message}\r\n" +
            $"----------------------------------------------------\r\n\r\n" +

            ex.ToString();

        var result = ShowError(null,
            title: Application.ProductName + " - " + Language[langPath],
            heading: ex.Message,
            description: description,
            details: details,
            buttons: PopupButton.Continue_Quit);

        if (result.ExitResult == PopupExitResult.Cancel)
        {
            Application.Exit();
        }
    }


    #endregion // Popup functions


    #endregion // Public static functions




    #region Private functions

    // ImageFormats
    #region ImageFormats

    /// <summary>
    /// Returns distinc list of image formats.
    /// </summary>
    /// <param name="formats">The format string. E.g: *.bpm;*.jpg;</param>
    /// <returns></returns>
    public static HashSet<string> GetImageFormats(string formats)
    {
        var list = new HashSet<string>();
        var formatList = formats.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        char[] wildTrim = { '*' };

        foreach (var ext in formatList)
        {
            list.Add(ext.Trim(wildTrim));
        }

        return list;
    }

    /// <summary>
    /// Returns the image formats string. Example: <c>*.png;*.jpg;</c>
    /// </summary>
    /// <param name="list">The input HashSet</param>
    /// <returns></returns>
    public static string GetImageFormats(HashSet<string> list)
    {
        var sb = new StringBuilder(list.Count);
        foreach (var item in list)
        {
            sb.Append('*').Append(item).Append(';');
        }

        return sb.ToString();
    }

    #endregion // ImageFormats


    // System events
    #region System events

    private static void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
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
    private static void DelayTriggerRequestUpdatingColorModeEvent()
    {
        _requestUpdatingColorModeCancelToken.Cancel();
        _requestUpdatingColorModeCancelToken = new();

        _ = TriggerRequestUpdatingColorModeEventAsync(_requestUpdatingColorModeCancelToken.Token);
    }


    /// <summary>
    /// Triggers <see cref="RequestUpdatingColorMode"/> event.
    /// </summary>
    private static async Task TriggerRequestUpdatingColorModeEventAsync(CancellationToken token = default)
    {
        try
        {
            // since the message is triggered multiple times (3 - 5 times)
            await Task.Delay(10, token);
            token.ThrowIfCancellationRequested();


            var isDarkMode = WinColorsApi.IsDarkMode;
            if (_isDarkMode != isDarkMode)
            {
                _isDarkMode = isDarkMode;

                // emit event here
                RequestUpdatingColorMode?.Invoke(new SystemColorModeChangedEventArgs(_isDarkMode));
            }
        }
        catch (OperationCanceledException) { }
    }
    #endregion // System events

    #endregion // Private functions


}
