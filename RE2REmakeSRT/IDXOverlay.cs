using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RE2REmakeSRT
{
    public interface IDXOverlay
    {
        void Initialize(Action<OverlayWindow, Graphics> initMethod);
        Task Run(Action<OverlayWindow, Graphics> drawMethod, CancellationToken cToken);
    }
}
