using System;
using System.Diagnostics;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

namespace winui_fps_test;

public sealed partial class DispatcherQueueAnimatorWindow : Window
{
    private bool _isPlaying = true;
    private double _angle = 0.0, _timeAcc = 0.0;
    private int _trailHead = 0, _frameCount = 0;
    private readonly Vector2[] _trail = new Vector2[40];

    private TimeSpan _lastRenderTime = TimeSpan.Zero;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly DispatcherQueueTimer _timer;

    private CanvasRadialGradientBrush _ballBrush;
    private readonly Color _colGreen = Color.FromArgb(255, 0, 255, 136);
    private readonly Color _colGreenDim = Color.FromArgb(255, 0, 140, 70);
    private readonly Color _colBlue = Color.FromArgb(255, 0, 180, 255);
    private readonly Color _colGray = Color.FromArgb(255, 100, 100, 100);

    public DispatcherQueueAnimatorWindow()
    {
        InitializeComponent();
        AnimationCanvas.CreateResources += OnAnimationCanvasOnCreateResources;
        AnimationCanvas.Draw += OnAnimationCanvasDraw;

        // RENDERING APPROACH: Uses DispatcherQueue timer to continuously invalidate the canvas.
        // Timer strictly used to constantly poke the canvas to redraw
        _timer = this.DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(15);
        _timer.Tick += OnTimerOnTick;
        _timer.Start();
    }

    private void OnAnimationCanvasOnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
    {
        // Clean up and recreate gradient brush (e.g., device lost)
        _ballBrush?.Dispose(); // Clean up if recreating (e.g., device lost)
        _ballBrush = new CanvasRadialGradientBrush(sender, [
            new() { Color = Colors.White, Position = 0.0f },
            new() { Color = _colGreen, Position = 0.45f },
            new() { Color = _colGreenDim, Position = 1.0f }
        ])
        { RadiusX = 25.2f, RadiusY = 25.2f };
    }

    // TIMER TICK: Invalidates canvas to trigger Draw event
    private void OnTimerOnTick(DispatcherQueueTimer s, object e)
    {
        System.Diagnostics.Debug.WriteLine("Timer Tick");
        AnimationCanvas.Invalidate();
    }

    // FPS CALCULATOR: Tracks frame count and calculates FPS every 0.25 seconds
    private void CalculateFPS()
    {
        var now = _stopwatch.Elapsed;
        if (_lastRenderTime != TimeSpan.Zero && now > _lastRenderTime)
        {
            // ---  FPS Calculation ---
            var dt = (now - _lastRenderTime).TotalSeconds;
            _timeAcc += dt;
            _frameCount++;
            if (_timeAcc >= 0.25)
            {
                FpsText.Text = $"FPS: {Math.Round(_frameCount / _timeAcc)}";
                _timeAcc = _frameCount = 0;
            }

            if (_isPlaying)
                _angle = (_angle + 1.8 * dt) % (Math.PI * 2);
        }
        _lastRenderTime = now;        
    }

    // DRAW LOOP: Handles all rendering and FPS calculation
    private void OnAnimationCanvasDraw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine("OnAnimationCanvasDraw");

        // 0. Calculate FPS
        CalculateFPS();

        var ds = args.DrawingSession;
        var center = new Vector2((float)sender.ActualWidth / 2f, (float)sender.ActualHeight / 2f);
        var orbitR = MathF.Min(center.X * 2, center.Y * 2) * 0.36f;

        // 1. Orbit & Crosshair
        ds.DrawCircle(center, orbitR, _colGray, 1.0f);
        ds.DrawLine(center.X - 12, center.Y, center.X + 12, center.Y, _colGreen, 1f);
        ds.DrawLine(center.X, center.Y - 12, center.X, center.Y + 12, _colGreen, 1f);
        ds.DrawCircle(center, 3.5f, _colGreen, 1.5f);

        // 2. Ball Pos & Trail Update
        var ballPos = center + new Vector2(orbitR * MathF.Cos((float)_angle), orbitR * MathF.Sin((float)_angle));
        if (_isPlaying)
        {
            _trail[_trailHead] = ballPos;
            _trailHead = (_trailHead + 1) % 40;
        }

        // 3. Draw Trail
        for (var i = 0; i < 40; i++)
        {
            var pt = _trail[(_trailHead - 1 - i + 40) % 40];
            if (pt == Vector2.Zero) continue;

            var t = 1f - i / 40f;
            var color = Color.FromArgb((byte)(t * t * 180),
                (byte)(_colBlue.R * t + _colGreen.R * (1 - t)),
                (byte)(_colBlue.G * t + _colGreen.G * (1 - t)),
                (byte)(_colBlue.B * t + _colGreen.B * (1 - t)));

            ds.FillCircle(pt, 9.9f * t, color); // 18(Radius) * 0.55 = 9.9
        }

        // 4. Connector Line
        ds.DrawLine(center, ballPos, _colGray, 1.0f);

        // 5. Main Ball & Rim
        if (_ballBrush != null) // Safety check during startup
        {
            _ballBrush.Center = ballPos - new Vector2(5.4f, 6.3f); // Only update the center point
            ds.FillCircle(ballPos, 18f, _ballBrush);
        }
        ds.DrawCircle(ballPos, 18f, _colGreen, 1.5f);
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        _isPlaying = !_isPlaying;
        PlayPauseButton.Content = _isPlaying ? "Pause" : "Play";

        if (!_isPlaying)
        {
            // Reset FPS counters when paused
            _timeAcc = _frameCount = 0; // Reset new FPS counters
            _lastRenderTime = TimeSpan.Zero;
        }
    }
}