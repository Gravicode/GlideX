
using GHI.GlideX;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using TestGlideX.Properties;

namespace TestGlideX
{
    class Program : Application
    {
        public Program(DisplayController d) : base(d)
        {
        }

        private static void Main()
        {
            try
            {

                //TestScreen();
                TestGlideX();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

       

        static void TestScreen()
        {

            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);
            var app = new Program(lcd.display);
            app.Run(Program.CreateWindow(lcd.display));
        }

        private static Window CreateWindow(DisplayController display)
        {
            var window = new Window
            {
                Height = (int)display.ActiveConfiguration.Height,
                Width = (int)display.ActiveConfiguration.Width
            };

            window.Background = new LinearGradientBrush
                (Colors.Blue, Colors.Teal, 0, 0, window.Width, window.Height);

            window.Visibility = Visibility.Visible;

            return window;
        }
        static Program app;
        private const int SCREEN_WIDTH = 480;
        private const int SCREEN_HEIGHT = 272;
        private static void TestGlideX()
        {
            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);
            //must be declared before setup glide..
            app = new Program(lcd.display);

            GlideX.SetupGlide(SCREEN_WIDTH, SCREEN_HEIGHT, 96, 0, lcd.display);
            string GlideXML = Resources.GetString(Resources.StringResources.SampleForm);  
            
            //Resources.GetString(Resources.StringResources.Window)
            Window window = GlideLoader.LoadWindow(GlideXML);
            GlideX.MainWindow = window;
            var txt = (Text)GlideX.GetChildByName("TxtTest");
            var btn = (Button)GlideX.GetChildByName("btn");
            if (btn != null)
            {
                btn.Click += (a,b) =>
                {
                    txt.TextContent = "Welcome to Glide for TinyCLR 2 - Cheers from Mif ;)";
                    Debug.WriteLine("Button tapped.");

                    window.Invalidate();
                    txt.Invalidate();
                };
            }
            
            //GlideTouch.Initialize();
            /*
            GHI.Glide.UI.Button btn = (GHI.Glide.UI.Button)window.GetChildByName("btn");
            GHI.Glide.UI.TextBlock txt = (GHI.Glide.UI.TextBlock)window.GetChildByName("TxtTest");
            btn.TapEvent += (object sender) =>
            {
                txt.Text = "Welcome to Glide for TinyCLR 2 - Cheers from Mif ;)";
                Debug.WriteLine("Button tapped.");

                window.Invalidate();
                txt.Invalidate();
            };*/



            lcd.CapacitiveScreenReleased += Lcd_CapacitiveScreenReleased;
            lcd.CapacitiveScreenPressed += Lcd_CapacitiveScreenPressed;
            lcd.CapacitiveScreenMove += Lcd_CapacitiveScreenMove;

            Graphics.OnFlushEvent += (IntPtr hdc, byte[] data) =>
            {
                lcd.display.DrawBuffer(0, 0, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, SCREEN_WIDTH, data, 0);
            };

           

            app.Run(GlideX.MainWindow);
            //Thread.Sleep(Timeout.Infinite);
        }


        #region Lcd Capacitive Touch Events
        /// <summary>
        /// Function called when released event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenReleased(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you release the lcd at X:" + e.X + " ,Y:" + e.Y);
            app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Up, DateTime.UtcNow);
           
            //GlideTouch.RaiseTouchUpEvent(e.X, e.Y);
        }

        /// <summary>
        /// Function called when pressed event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenPressed(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you press the lcd at X:" + e.X + " ,Y:" + e.Y);
            //GlideTouch.RaiseTouchDownEvent(e.X, e.Y);
        }

        private static void Lcd_CapacitiveScreenMove(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you move finger on the lcd at X:" + e.X + " ,Y:" + e.Y);
            //GlideTouch.RaiseTouchMoveEvent(sender, new TouchEventArgs(new  GHI.Glide.Geom.Point(e.X,e.Y)));
        }
        #endregion
    }
   
}
