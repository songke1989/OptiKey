﻿using System;
using System.Windows;
using System.Windows.Interop;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Static;
using log4net;

namespace JuliusSweetland.OptiKey.Services
{
    public class WindowManipulationService : IWindowManipulationService
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Window window;
        private readonly Func<Rect?> getWindowSizeAndPositionSetting;
        private readonly Action<Rect?> setWindowSizeAndPositionSetting;
        private readonly Func<bool> getArrangeOtherWindowsSetting;
        private readonly Settings settings;

        private bool minimised;

        #endregion

        #region Ctor
        
        internal WindowManipulationService(
            Window window,
            Func<Rect?> getWindowSizeAndPositionSetting,
            Action<Rect?> setWindowSizeAndPositionSetting,
            Func<bool> getArrangeOtherWindowsSetting,
            IObservable<bool> arrangeOtherWindowsObservable,
            Settings settings)
        {
            this.window = window;
            this.getWindowSizeAndPositionSetting = getWindowSizeAndPositionSetting;
            this.setWindowSizeAndPositionSetting = setWindowSizeAndPositionSetting;
            this.getArrangeOtherWindowsSetting = getArrangeOtherWindowsSetting;
            this.settings = settings;

            InitialiseWindowSizeAndPosition(window, getWindowSizeAndPositionSetting);

            Action attachDetachArrangeOtherWindowListeners = () =>
            {
                if (getArrangeOtherWindowsSetting())
                {
                    AttachArrangeOtherWindowsListeners();
                }
                else
                {
                    DetachArrangeOtherWindowsListeners();
                }
            };
            arrangeOtherWindowsObservable.Subscribe(_ => attachDetachArrangeOtherWindowListeners());
        }

        #endregion
        
        #region Events

        public event EventHandler<Exception> Error;

        #endregion

        #region Public Methods

        //public void ArrangeWindowsHorizontally()
        //{
        //    if (window.WindowState != WindowState.Normal) return;

        //    try
        //    {
        //        var screen = window.GetScreen();
        //        var windowTopLeftInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left, window.Top));
        //        var distanceToTopBoundaryInPixels = Convert.ToInt32(windowTopLeftInScreenCoords.Y - screen.Bounds.Top);
        //        var windowBottomRightInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left + window.ActualWidth, window.Top + window.ActualHeight));
        //        var distanceToBottomBoundaryInPixels = Convert.ToInt32(screen.Bounds.Bottom - windowBottomRightInScreenCoords.Y);

        //        var taskBar = new Taskbar();
        //        var taskBarOnCurrentScreen = taskBar.Bounds.IntersectsWith(screen.Bounds);

        //        int x = screen.Bounds.Left;
        //        int width = screen.Bounds.Width;

        //        //Compensate for left/right aligned taskbar
        //        if (taskBarOnCurrentScreen
        //            && !taskBar.AutoHide
        //            && taskBar.Position == TaskbarPosition.Left)
        //        {
        //            x += taskBar.Size.Width;
        //            width -= taskBar.Size.Width;
        //        }
        //        else if (taskBarOnCurrentScreen
        //            && !taskBar.AutoHide 
        //            && taskBar.Position == TaskbarPosition.Right)
        //        {
        //            width -= taskBar.Size.Width;
        //        }

        //        if (distanceToTopBoundaryInPixels > 0 && distanceToTopBoundaryInPixels > distanceToBottomBoundaryInPixels)
        //        {
        //            //Arrange windows above OptiKey
        //            int y = screen.Bounds.Top;
        //            int height = distanceToTopBoundaryInPixels;

        //            //Compensate for top aligned taskbar
        //            if (taskBarOnCurrentScreen
        //                && !taskBar.AutoHide 
        //                && taskBar.Position == TaskbarPosition.Top)
        //            {
        //                y += taskBar.Size.Height;
        //                height -= taskBar.Size.Height;
        //            }

        //            ArrangeOtherWindows(x, y, width, height);
        //        }
        //        else if (distanceToBottomBoundaryInPixels > 0 && distanceToBottomBoundaryInPixels > distanceToTopBoundaryInPixels)
        //        {
        //            //Arrange windows below OptiKey
        //            int y = Convert.ToInt32(windowBottomRightInScreenCoords.Y);
        //            int height = distanceToBottomBoundaryInPixels;
                    
        //            //Compensate for bottom aligned taskbar
        //            if (taskBarOnCurrentScreen
        //                && !taskBar.AutoHide 
        //                && taskBar.Position == TaskbarPosition.Bottom)
        //            {
        //                height -= taskBar.Size.Height;
        //            }
                    
        //            ArrangeOtherWindows(x, y, width, height);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PublishError(this, ex);
        //    }
        //}

        //public void ArrangeWindowsVertically()
        //{
        //    if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

        //    try
        //    {
        //        var screen = window.GetScreen();
        //        var windowTopLeftInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left, window.Top));
        //        var distanceToLeftBoundaryInPixels = Convert.ToInt32(windowTopLeftInScreenCoords.X - screen.Bounds.Left);
        //        var windowBottomRightInScreenCoords = window.GetTransformToDevice().Transform(new Point(window.Left + window.ActualWidth, window.Top + window.ActualHeight));
        //        var distanceToRightBoundaryInPixels = Convert.ToInt32(screen.Bounds.Right - windowBottomRightInScreenCoords.X);

        //        var taskBar = new Taskbar();
        //        var taskBarOnCurrentScreen = taskBar.Bounds.IntersectsWith(screen.Bounds);

        //        int y = screen.Bounds.Top;
        //        int height = screen.Bounds.Height;

        //        //Compensate for top/bottom aligned taskbar
        //        if (taskBarOnCurrentScreen
        //            && !taskBar.AutoHide 
        //            && taskBar.Position == TaskbarPosition.Top)
        //        {
        //            y += taskBar.Size.Height;
        //            height -= taskBar.Size.Height;
        //        }
        //        else if (taskBarOnCurrentScreen
        //            && !taskBar.AutoHide 
        //            && taskBar.Position == TaskbarPosition.Bottom)
        //        {
        //            height -= taskBar.Size.Height;
        //        }

        //        if (distanceToLeftBoundaryInPixels > 0 && distanceToLeftBoundaryInPixels > distanceToRightBoundaryInPixels)
        //        {
        //            //Arrange windows to left of OptiKey
        //            int x = screen.Bounds.Left;
        //            int width = distanceToLeftBoundaryInPixels;

        //            //Compensate for left aligned taskbar
        //            if (taskBarOnCurrentScreen
        //                && !taskBar.AutoHide 
        //                && taskBar.Position == TaskbarPosition.Left)
        //            {
        //                x += taskBar.Size.Width;
        //                width -= taskBar.Size.Width;
        //            }

        //            ArrangeOtherWindows(x, y, width, height);
        //        }
        //        else if (distanceToRightBoundaryInPixels > 0 && distanceToRightBoundaryInPixels > distanceToLeftBoundaryInPixels)
        //        {
        //            //Arrange windows to right of OptiKey
        //            int x = Convert.ToInt32(windowBottomRightInScreenCoords.X);
        //            int width = distanceToRightBoundaryInPixels;
                    
        //            //Compensate for right aligned taskbar
        //            if (taskBarOnCurrentScreen
        //                && !taskBar.AutoHide 
        //                && taskBar.Position == TaskbarPosition.Right)
        //            {
        //                width -= taskBar.Size.Width;
        //            }

        //            ArrangeOtherWindows(x, y, width, height);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        PublishError(this, ex);
        //    }
        //}

        public void DecreaseOpacity()
        {
            window.Opacity -= 0.1;
            if (window.Opacity < 0.1)
            {
                window.Opacity = 0.1;
            }
        }

        public void ExpandToBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                window.Height += yAdjustment;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToBottomAndLeft(double pixels)
        {
            ExpandToBottom(pixels);
            ExpandToLeft(pixels);
        }

        public void ExpandToBottomAndRight(double pixels)
        {
            ExpandToBottom(pixels);
            ExpandToRight(pixels);
        }
        
        public void ExpandToLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                var widthBeforeAdjustment = window.ActualWidth;
                window.Width += xAdjustment;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;
                
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                
                window.Width += xAdjustment;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);

                var heightBeforeAdjustment = window.ActualHeight;
                window.Height += yAdjustment;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ExpandToTopAndLeft(double pixels)
        {
            ExpandToTop(pixels);
            ExpandToLeft(pixels);
        }

        public void ExpandToTopAndRight(double pixels)
        {
            ExpandToTop(pixels);
            ExpandToRight(pixels);
        }

        public void FillPercentageOfScreen(double horizontalPercentage, double verticalPercentage)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var screenBottomRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));

                var screenWidth = (screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X);
                var screenHeight = (screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y);

                var distanceFromLeftBoundary = ((1d - (horizontalPercentage / 100)) / 2d) * screenWidth;
                var distanceFromTopBoundary = ((1d - (verticalPercentage / 100)) / 2d) * screenHeight;

                window.Left = screenTopLeftInWpfCoords.X + distanceFromLeftBoundary;
                window.Top = screenTopLeftInWpfCoords.Y + distanceFromTopBoundary;

                var width = (horizontalPercentage / 100) * screenWidth;
                var height = (verticalPercentage / 100) * screenHeight;

                window.Width = width;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth

                window.Height = height;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Height property out of sync with the ActualHeight
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinWHeight - not doing this leaves the Height property out of sync with the ActualHeight

            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void IncreaseOpacity()
        {
            window.Opacity += 0.1;
            if (window.Opacity > 1)
            {
                window.Opacity = 1;
            }
        }
        
        public void Maximise()
        {
            window.WindowState = WindowState.Maximized;
            minimised = false;
        }

        public void Minimise()
        {
            minimised = true;
            window.Width = Settings.Default.MinimisedWidthInPixels / Graphics.DipScalingFactorX;
            window.Height = Settings.Default.MinimisedHeightInPixels / Graphics.DipScalingFactorY;
            //TODO:Switch on minimise location
            MoveToBottomAndLeftBoundaries();
        }
        
        public void MoveToBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Top += yAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        public void MoveToBottomAndLeft(double pixels)
        {
            MoveToBottom(pixels);
            MoveToLeft(pixels);
        }
        
        public void MoveToBottomAndLeftBoundaries()
        {
            MoveToBottomBoundary();
            MoveToLeftBoundary();
        }
        
        public void MoveToBottomAndRight(double pixels)
        {
            MoveToBottom(pixels);
            MoveToRight(pixels);
        }
        
        public void MoveToBottomAndRightBoundaries()
        {
            MoveToBottomBoundary();
            MoveToRightBoundary();
        }
        
        public void MoveToBottomBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary;
                window.Top += yAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Left -= xAdjustment;
                
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToLeftBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary;
                window.Left -= xAdjustment;
                
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Left += xAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToRightBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {   
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary;
                window.Left += xAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void MoveToTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : pixels.CoerceToUpperLimit(distanceToBoundary);
                window.Top -= yAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        public void MoveToTopAndLeft(double pixels)
        {
            MoveToTop(pixels);
            MoveToLeft(pixels);
        }
        
        public void MoveToTopAndLeftBoundaries()
        {
            MoveToTopBoundary();
            MoveToLeftBoundary();
        }
        
        public void MoveToTopAndRight(double pixels)
        {
            MoveToTop(pixels);
            MoveToRight(pixels);
        }
        
        public void MoveToTopAndRightBoundaries()
        {
            MoveToTopBoundary();
            MoveToRightBoundary();
        }

        public void MoveToTopBoundary()
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary;
                window.Top -= yAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void Restore()
        {
            window.WindowState = WindowState.Normal;
            var rect = getWindowSizeAndPositionSetting();
            if (rect.HasValue)
            {
                window.Top = rect.Value.Top;
                window.Left = rect.Value.Left;
                window.Width = rect.Value.Width;
                window.Height = rect.Value.Height;
            }
            minimised = false;
        }

        public void ShrinkFromBottom(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenBottomLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Bottom));
                var distanceToBoundary = screenBottomLeftInWpfCoords.Y - (window.Top + window.ActualHeight);
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                window.Height += yAdjustment;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromBottomAndLeft(double pixels)
        {
            ShrinkFromBottom(pixels);
            ShrinkFromLeft(pixels);
        }

        public void ShrinkFromBottomAndRight(double pixels)
        {
            ShrinkFromBottom(pixels);
            ShrinkFromRight(pixels);
        }

        public void ShrinkFromLeft(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Left - screenTopLeftInWpfCoords.X;
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                var widthBeforeAdjustment = window.ActualWidth;
                window.Width += xAdjustment;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
                var actualXAdjustment = window.ActualWidth - widthBeforeAdjustment; //WPF may have coerced the adjustment
                window.Left -= actualXAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromRight(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopRightInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Top));
                var distanceToBoundary = screenTopRightInWpfCoords.X - (window.Left + window.ActualWidth);
                var xAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                window.Width += xAdjustment;
                window.Width = window.Width.CoerceToUpperLimit(window.MaxWidth); //Manually coerce the value to respect the MaxWidth - not doing this leaves the Width property out of sync with the ActualWidth
                window.Width = window.Width.CoerceToLowerLimit(window.MinWidth); //Manually coerce the value to respect the MinWidth - not doing this leaves the Width property out of sync with the ActualWidth
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromTop(double pixels)
        {
            if (window.WindowState == WindowState.Maximized || window.WindowState == WindowState.Minimized) return;

            try
            {
                var screen = window.GetScreen();
                var screenTopLeftInWpfCoords = window.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                var distanceToBoundary = window.Top - screenTopLeftInWpfCoords.Y;
                var yAdjustment = distanceToBoundary < 0 ? distanceToBoundary : 0 - pixels;

                var heightBeforeAdjustment = window.ActualHeight;
                window.Height += yAdjustment;
                window.Height = window.Height.CoerceToUpperLimit(window.MaxHeight); //Manually coerce the value to respect the MaxHeight - not doing this leaves the Width property out of sync with the ActualWidth
                window.Height = window.Height.CoerceToLowerLimit(window.MinHeight); //Manually coerce the value to respect the MinHeight - not doing this leaves the Width property out of sync with the ActualWidth
                var actualYAdjustment = window.ActualHeight - heightBeforeAdjustment; //WPF may have coerced the adjustment
                window.Top -= actualYAdjustment;
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        public void ShrinkFromTopAndLeft(double pixels)
        {
            ShrinkFromTop(pixels);
            ShrinkFromLeft(pixels);
        }

        public void ShrinkFromTopAndRight(double pixels)
        {
            ShrinkFromTop(pixels);
            ShrinkFromRight(pixels);
        }

        #endregion

        #region Private Methods

        private void AttachArrangeOtherWindowsListeners()
        {
            //THROTTLE LISTENERS as they reacts to size/position changes and we change width/height/top/left one at a time
            //TODO: Ensure size/position listeners are fired on state changes (maximise/minimised/our minimised)
            //Window size & position
            //Minimised - our version = maximise other windows
            //Window state
            //Other top level windows created
            //Other top level windows restored from minimised
            //Application exit = maximise

            window.Closing += (sender, args) =>
            {
                //Maximise all windows if they were arranged at some point
                var thisHwnd = new WindowInteropHelper(window).Handle;
                var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
                foreach (var otherWindow in otherWindows)
                {
                    Log.DebugFormat("Maximising window '{0}' before we exit", otherWindow.Item1);
                    PInvoke.ShowWindow(otherWindow.Item2, (int)WindowShowStyle.Maximize);
                }
            };

            ArrangeOtherWindows();
        }

        private void DetachArrangeOtherWindowsListeners()
        {

            ArrangeWindowsMaximised();
        }

        private void ArrangeOtherWindows()
        {
            try
            {
                if (window.WindowState == WindowState.Maximized) return;

                if (window.WindowState == WindowState.Minimized || minimised)
                {
                    ArrangeWindowsMaximised();
                    return;
                }


                //if window.WindowState == Minimised OR our version of minimised then maximise other windows
                //else calculate biggest free space and fill it (considering taskbar)
                
                int x = 0, y = 0, width = 0, height = 0; //TODO: Calculate

                var thisHwnd = new WindowInteropHelper(window).Handle;
                var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
                foreach (var otherWindow in otherWindows)
                {
                    Log.DebugFormat("Restoring and arranging window '{0}' to position ({1},{2}) and size ({3},{4})",
                        otherWindow.Item1, x, y, width, height);
                    var otherWindowHandle = otherWindow.Item2;
                    PInvoke.ShowWindow(otherWindowHandle, (int) WindowShowStyle.ShowNormal);
                        //Restore windows as resizing windows that are in a minimised/maximised state can break the minimise/maximise button
                    PInvoke.SetWindowPos(otherWindowHandle, IntPtr.Zero, x, y, width, height,
                        SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOOWNERZORDER |
                        SetWindowPosFlags.SWP_NOZORDER);
                }
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        private void ArrangeWindowsMaximised()
        {
            try
            {
                var thisHwnd = new WindowInteropHelper(window).Handle;
                var otherWindows = Windows.GetVisibleOverlappedWindows(thisHwnd);
                foreach (var otherWindow in otherWindows)
                {
                    Log.DebugFormat("Maximising window '{0}'", otherWindow.Item1);
                    PInvoke.ShowWindow(otherWindow.Item2, (int)WindowShowStyle.ShowMaximized);
                }
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }

        private void InitialiseWindowSizeAndPosition(Window win, Func<Rect?> getSizeAndPositionFromSetting)
        {
            try
            {
                var windowSizeAndPosition = getSizeAndPositionFromSetting();
                if (windowSizeAndPosition != null)
                {
                    //Coerce size and position to screen
                    var screen = win.GetScreen();
                    var screenTopLeftInWpfCoords = win.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                    var screenBottomRightInWpfCoords = win.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));
                    var screenHeight = screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y;
                    var screenWidth = screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X;

                    window.Height = windowSizeAndPosition.Value.Height > screenHeight ? screenHeight : windowSizeAndPosition.Value.Height;
                    window.Width = windowSizeAndPosition.Value.Width > screenWidth ? screenWidth : windowSizeAndPosition.Value.Width;

                    window.Top = windowSizeAndPosition.Value.Top < screenTopLeftInWpfCoords.Y
                        ? screenTopLeftInWpfCoords.Y
                        : windowSizeAndPosition.Value.Top + window.Height > screenBottomRightInWpfCoords.Y
                            ? screenBottomRightInWpfCoords.Y - window.Height
                            : windowSizeAndPosition.Value.Top;

                    window.Left = windowSizeAndPosition.Value.Left < screenTopLeftInWpfCoords.X
                        ? screenTopLeftInWpfCoords.X
                        : windowSizeAndPosition.Value.Left + window.Width > screenBottomRightInWpfCoords.X
                            ? screenBottomRightInWpfCoords.X - window.Width
                            : windowSizeAndPosition.Value.Left;
                }
                else
                {
                    //Calculate initial size and position
                    var screen = win.GetScreen();
                    var screenTopLeftInWpfCoords = win.GetTransformFromDevice().Transform(new Point(screen.Bounds.Left, screen.Bounds.Top));
                    var screenBottomRightInWpfCoords = win.GetTransformFromDevice().Transform(new Point(screen.Bounds.Right, screen.Bounds.Bottom));
                    win.Left = screenTopLeftInWpfCoords.X;
                    win.Top = screenTopLeftInWpfCoords.Y;
                    win.Width = screenBottomRightInWpfCoords.X - screenTopLeftInWpfCoords.X;
                    win.Height = screenBottomRightInWpfCoords.Y - screenTopLeftInWpfCoords.Y;
                }

                PersistWindowSizeAndPosition();
            }
            catch (Exception ex)
            {
                PublishError(this, ex);
            }
        }
        
        private void PersistWindowSizeAndPosition()
        {
            if (window.WindowState == WindowState.Normal)
            {
                setWindowSizeAndPositionSetting(new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight));
                settings.Save();
            }
        }

        #endregion

        #region Publish Error

        private void PublishError(object sender, Exception ex)
        {
            Log.Error("Publishing Error event (if there are any listeners)", ex);
            if (Error != null)
            {
                Error(sender, ex);
            }
        }

        #endregion
    }
}
