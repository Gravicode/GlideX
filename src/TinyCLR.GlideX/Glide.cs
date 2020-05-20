﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using GHI.GlideX.Geom;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Glide;
using TinyCLR.GlideX.Properties;

namespace GHI.GlideX
{
    /// <summary>
    /// The Glide class provides core functionality.
    /// </summary>
    public static class GlideX
    {
        private static Size screenSize;
        internal static System.Drawing.Graphics screen;
        private static Window _mainWindow;
        public static IntPtr Hdc;
        private static ComboBox _dropdown;
        private static List _list;
        static GlideX()
        {
            
        }
        public static UIElement GetChildByName(string ComponentID)
        {
            if (_mainWindow != null)
            {
                var mainCanvas = _mainWindow.Child as Canvas;
                if (mainCanvas != null)
                {
                    foreach (UIElement component in mainCanvas.Children)
                    {
                        if (component.ID == ComponentID)
                        {
                            return component;
                        }
                    }
                }
            }
            return null;
        }

        
        public static bool RemoveChildByName(string ComponentID)
        {
            if (_mainWindow != null)
            {
                var mainCanvas = _mainWindow.Child as Canvas;
                if (mainCanvas != null)
                {
                    foreach (UIElement component in mainCanvas.Children)
                    {
                        if (component.ID == ComponentID)
                        {
                            mainCanvas.Children.Remove(component);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static void SetupGlide(int width,int height, int bitsPerPixel, int orientationDeg, DisplayController displayController)
        {
            Hdc = displayController.Hdc;
            LCD = new Size() { Width = width, Height = height };
            screen = System.Drawing.Graphics.FromHdc(Hdc);// new (width, height);
            IsEmulator = false;
            FitToScreen = false;

            // Show loading
            System.Drawing.Bitmap loading = Resources.GetBitmap(Resources.BitmapResources.loading);

            screen.DrawImage(loading, (LCD.Width - loading.Width) / 2, (LCD.Height - loading.Height) / 2, loading.Width, loading.Height);
            screen.Flush();
        }
        /// <summary>
        /// Returns the screen resolution.
        /// </summary>
        public static Size LCD
        {
            get
            {
                return GlideX.screenSize;
            }
            private set
            {
                GlideX.screenSize = value;
            }
        }

        /// <summary>
        /// Returns a reference to the bitmap that represents the current screen.
        /// This is only useful for drawing the bitmap to a display that does not
        /// support bitmap.flush().
        /// </summary>
        public static System.Drawing.Graphics Screen
        {
            get
            {
                return GlideX.screen;
            }
        }

        /// <summary>
        /// This method changes the underlying size of the bitmap that is drawn to
        /// the screen. Do not call this method if you are using a regular display.
        /// It is only useful when you are using a non-native display such as a
        /// SPI display like our DisplayN18.
        /// </summary>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        public static void SetScreenSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive.");

            GlideX.screenSize.Width = width;
            GlideX.screenSize.Height = height;
                 
            GlideX.screen.Dispose();
            GlideX.screen = System.Drawing.Graphics.FromHdc(Hdc); 
                 
          
        }


        public static void Flush(int x, int y, int width, int height)
        {
            // Ignore flushes if no MainWindow is set.
            if (_mainWindow == null)
                return;
            _mainWindow.Invalidate();
            //GlideX.screen.Flush();
        }

       

        /// <summary>
        /// Flushes the rectangular area to the screen.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        public static void Flush(Geom.Rectangle rect)
        {
            Flush(rect.X, rect.Y, rect.Width, rect.Height);
        }

     

        /// <summary>
        /// Indicates whether or not we're using the emulator.
        /// </summary>
        public static bool IsEmulator;

        /// <summary>
        /// Indicates whether or not to resize windows to the LCD's resolution.
        /// </summary>
        /// <remarks>This does not affect component placement. They will remain in their assigned position.</remarks>
        public static bool FitToScreen;

       
        public static Window MainWindow
        {
            get { return _mainWindow; }
            set
            {
               

                // Change to the new window
                _mainWindow = value;

                // Begin handling events
                if (_mainWindow != null)
                { 
                    // Call render after because windows only flush if they're handling events
                    _mainWindow.Invalidate();
                }
            }
        }
        /// <summary>
        /// Opens a List component.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="list">List component that needs to be opened.</param>
        public static void OpenList(object sender, List list)
        {
            if (_list == null)
            {
                _dropdown = (ComboBox)sender;
                _list = list;
                _list.TapOptionEvent += new OnTapOption(list_TapOptionEvent);

                //for (int i = 0; i < MainWindow.NumChildren; i++)
                //    MainWindow[i].Interactive = false;

                AddChild(list);
                MainWindow.Invalidate();
            }
            else
                throw new SystemException("You already have a List open.");
        }

        static void AddChild(UIElement child)
        {
            if (MainWindow != null && MainWindow.Child != null)
            {
                Canvas parent = MainWindow.Child as Canvas;
                parent.Children.Add(child);
            }
        }
        private static void list_TapOptionEvent(object sender, TapOptionEventArgs args)
        {
            _dropdown.Text = args.Label;
            _dropdown.Value = args.Value;
            _dropdown.TriggerValueChangedEvent(_dropdown);

            CloseList();
        }
        /// <summary>
        /// Closes a List component.
        /// </summary>
        public static void CloseList()
        {
            if (_list != null)
            {
                _list.TapOptionEvent -= new OnTapOption(list_TapOptionEvent);
              
                RemoveChildByName(_list.ID);
                
                _list = null;

                //for (int i = 0; i < MainWindow.NumChildren; i++)
                //    MainWindow[i].Interactive = true;

                MainWindow.Invalidate();
            }
        }
    }
}
