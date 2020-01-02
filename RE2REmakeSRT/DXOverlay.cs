using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public class DXOverlay : IDXOverlay, IDisposable
    {
        private IntPtr windowHook;
        private OverlayWindow _window;
        private Graphics _graphics;
        private double desiredDrawInterval;

        public DXOverlay(IntPtr windowHook, double desiredDrawRate = 60d)
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

            desiredDrawInterval = 1000d / desiredDrawRate;
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
                    Interval = desiredDrawInterval
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _graphics?.Dispose();
                    _window?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DXOverlay()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
