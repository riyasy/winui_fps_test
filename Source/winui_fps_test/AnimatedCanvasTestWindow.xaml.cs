using System;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;

namespace winui_fps_test;

public sealed partial class AnimatedCanvasTestWindow : Window
{
    private bool _isPlaying = true;
    private double _angle = 0.0, _timeAcc = 0.0;
    private int _trailHead = 0, _frameCount = 0;
    private readonly Vector2[] _trail = new Vector2[40];

    private CanvasRadialGradientBrush _ballBrush;
    private readonly Color _colGreen = Color.FromArgb(255, 0, 255, 136);
    private readonly Color _colGreenDim = Color.FromArgb(255, 0, 140, 70);
    private readonly Color _colBlue = Color.FromArgb(255, 0, 180, 255);
    private readonly Color _colGray = Color.FromArgb(255, 100, 100, 100);

	public AnimatedCanvasTestWindow()
	{
		InitializeComponent();
		AnimationCanvas.CreateResources += OnAnimationCanvasOnCreateResources;
		AnimationCanvas.Draw += OnAnimationCanvasDraw;
		// RENDERING APPROACH: Uses CanvasAnimatedControl with built-in Update and Draw loops
		AnimationCanvas.Update += OnAnimationCanvasUpdate;
	}

    private void OnAnimationCanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
    {
        // Clean up and recreate gradient brush (e.g., device lost)
        _ballBrush?.Dispose();
        _ballBrush = new CanvasRadialGradientBrush(sender, [
            new() { Color = Colors.White, Position = 0.0f },
            new() { Color = _colGreen, Position = 0.45f },
            new() { Color = _colGreenDim, Position = 1.0f }
        ])
        { RadiusX = 25.2f, RadiusY = 25.2f };
    }

	// UPDATE LOOP: Exclusively handles game logic and animation state
	private void OnAnimationCanvasUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
	{
		if (_isPlaying)
			_angle = (_angle + 1.8 * args.Timing.ElapsedTime.TotalSeconds) % (Math.PI * 2);
	}

	// FPS CALCULATOR: Tracks frame count and calculates FPS every 0.25 seconds (called from Draw thread)
	private void CalculateFPS(double dt)
	{
		_timeAcc += dt;
		_frameCount++;
		if (_timeAcc >= 0.25)
		{
			// We are on a background thread! Route UI updates to the DispatcherQueue.
			DispatcherQueue.TryEnqueue(() =>
			{
				FpsText.Text = $"FPS: {Math.Round(_frameCount / _timeAcc)}";
				_timeAcc = _frameCount = 0;
			});
		}
	}

	// DRAW LOOP: Exclusively handles rendering and FPS calculation
	private void OnAnimationCanvasDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
	{
		// 0. Calculate FPS
		// Call the separate FPS function using the exact time since the last frame was drawn
		CalculateFPS(args.Timing.ElapsedTime.TotalSeconds);

        var ds = args.DrawingSession;
        var center = new Vector2((float)sender.Size.Width / 2f, (float)sender.Size.Height / 2f);
        float orbitR = MathF.Min(center.X * 2, center.Y * 2) * 0.36f;

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
        for (int i = 0; i < 40; i++)
        {
            var pt = _trail[(_trailHead - 1 - i + 40) % 40];
            if (pt == Vector2.Zero) continue;

            float t = 1f - i / 40f;
            var color = Color.FromArgb((byte)(t * t * 180),
                (byte)(_colBlue.R * t + _colGreen.R * (1 - t)),
                (byte)(_colBlue.G * t + _colGreen.G * (1 - t)),
                (byte)(_colBlue.B * t + _colGreen.B * (1 - t)));

            ds.FillCircle(pt, 9.9f * t, color);
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

        // Pauses the background thread loops to save resources
        AnimationCanvas.Paused = !_isPlaying;

        if (!_isPlaying)
            // Reset FPS counters when paused
            _timeAcc = _frameCount = 0;
    }
}