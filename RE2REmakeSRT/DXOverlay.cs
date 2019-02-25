using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public class DXOverlay : IDXOverlay
    {
        private IntPtr windowHook;
        private OverlayWindow _window;
        private Graphics _graphics;

        public DXOverlay(IntPtr windowHook)
        {
            this.windowHook = windowHook;

            // it is important to set the window to visible (and topmost) if you want to see it!
            _window = new OverlayWindow()
            {
                IsVisible = true,
                IsTopmost = true
            };

            // initialize a new Graphics object
            // set everything before you call _graphics.Setup()
            _graphics = new Graphics()
            {
                MeasureFPS = false,
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = true,
                UseMultiThreadedFactories = false,
                VSync = false
            };
        }

        ~DXOverlay()
        {
            // dont forget to free resources
            _graphics.Dispose();
            _window.Dispose();
        }

        public void Initialize(Action<OverlayWindow, Graphics> initMethod)
        {
            _window.CreateWindow();
            _window.FitToWindow(windowHook, true);

            _graphics.Width = _window.Width;
            _graphics.Height = _window.Height;
            _graphics.WindowHandle = _window.Handle; // set the target handle before calling Setup()
            _graphics.Setup();

            initMethod.Invoke(_window, _graphics);
        }

        public Task Run(Action<OverlayWindow, Graphics> drawMethod, CancellationToken cToken)
        {
            return Task.Run(() =>
            {
                using (System.Timers.Timer t = new System.Timers.Timer()
                {
                    AutoReset = false,
                    Interval = 16.6d
                })
                {
                    t.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                    {
                        try
                        {
                        // Ensure this is on top of the game.
                        _window.PlaceAboveWindow(windowHook);

                        // Begin a new scene/frame.
                        _graphics.BeginScene();

                        // Clear the previous scene/frame.
                        _graphics.ClearScene();

                            drawMethod.Invoke(_window, _graphics);

                        // End the scene/frame rendering.
                        _graphics.EndScene();
                        }
                        finally
                        {
                            try { ((System.Timers.Timer)sender).Start(); }
                            catch { }
                        }
                    };
                    t.Start();

                    try { Task.WaitAll(Task.Delay(-1, cToken)); }
                    catch { }
                }
            }, cToken);
        }
    }
}
