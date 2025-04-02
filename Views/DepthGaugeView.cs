using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using Path = Android.Graphics.Path;

namespace Nauti_Control_Wear.Views
{
    public class DepthGaugeView : BaseGaugeView
    {
        private const float MAX_DEPTH = 100f; // 100 meters max depth
        private const float ARROW_LENGTH = 0.8f;
        private const float ARROW_HEAD_LENGTH = 0.2f;
        private const float ARROW_HEAD_ANGLE = 30f;
        private const float DEPTH_TEXT_SIZE = 20f;
        private const float MARKER_LENGTH = 0.1f;
        private const float MARKER_STROKE_WIDTH = 2f;
        
        // Colors
        private readonly Color _needleColor = Color.White;
        private readonly Color _textColor = Color.White;
        private readonly Color _markerColor = Color.White;
        private readonly Color _warningColor = Color.ParseColor("#FF0000");  // Red for warnings

        // Warning thresholds
        private const float CRITICAL_DEPTH = 3f; // 3 meters critical warning
        private const float SHALLOW_WATER_THRESHOLD = 10f; // 10 meters shallow water warning
        
        // Animation for shallow water alerts
        private bool _flashWarning = false;
        private long _lastFlashTime = 0;
        private const long FLASH_INTERVAL = 500; // Flash every 500ms

        public DepthGaugeView(Context context) : base(context)
        {
            _maxValue = MAX_DEPTH;
            _unit = "M";
            _label = "Depth";
        }

        public DepthGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _maxValue = MAX_DEPTH;
            _unit = "M";
            _label = "Depth";
        }

        protected override void Initialize()
        {
            base.Initialize();
            _paint.TextSize = DEPTH_TEXT_SIZE;
            _paint.TextAlign = Paint.Align.Center;
        }

        public override void UpdateValue(float value)
        {
            base.UpdateValue(value);
            
            // Flash warning animation for shallow water
            if (value <= CRITICAL_DEPTH)
            {
                long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (currentTime - _lastFlashTime > FLASH_INTERVAL)
                {
                    _flashWarning = !_flashWarning;
                    _lastFlashTime = currentTime;
                    PostInvalidateOnAnimation();
                }
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2f;
            float centerY = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - GAUGE_STROKE_WIDTH;

            DrawBackground(canvas, centerX, centerY, radius);
            DrawRings(canvas, centerX, centerY, radius);
            DrawDepthMarkers(canvas, centerX, centerY, radius);
            DrawDepthNeedle(canvas, centerX, centerY, radius);
            DrawDepthValue(canvas, centerX, centerY);

            // Draw warning if in shallow water
            if (_currentValue <= CRITICAL_DEPTH)
            {
                if (_flashWarning)
                {
                    DrawWarningText(canvas, centerX, centerY, "DANGER! VERY SHALLOW!", _warningColor, radius);
                }
            }
            else if (_currentValue < SHALLOW_WATER_THRESHOLD)
            {
                DrawWarningText(canvas, centerX, centerY, "SHALLOW WATER!", _warningColor, radius);
            }
        }

        private void DrawDepthNeedle(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = _needleColor;
            _paint.StrokeWidth = GAUGE_STROKE_WIDTH;
            _paint.SetStyle(Paint.Style.Stroke);

            using var path = new Path();
            float angle = (_currentValue / _maxValue) * 360f;
            float arrowLength = radius * ARROW_LENGTH;
            float headLength = radius * ARROW_HEAD_LENGTH;

            // Calculate arrow points (0° at top)
            float radians = angle * (float)Math.PI / 180f;
            float endX = centerX + arrowLength * (float)Math.Sin(radians);
            float endY = centerY - arrowLength * (float)Math.Cos(radians);

            // Draw arrow shaft
            path.MoveTo(centerX, centerY);
            path.LineTo(endX, endY);

            // Calculate and draw arrow head
            float headAngle1 = (angle + ARROW_HEAD_ANGLE) * (float)Math.PI / 180f;
            float headAngle2 = (angle - ARROW_HEAD_ANGLE) * (float)Math.PI / 180f;

            float head1X = endX - headLength * (float)Math.Sin(headAngle1);
            float head1Y = endY + headLength * (float)Math.Cos(headAngle1);
            float head2X = endX - headLength * (float)Math.Sin(headAngle2);
            float head2Y = endY + headLength * (float)Math.Cos(headAngle2);

            path.MoveTo(endX, endY);
            path.LineTo(head1X, head1Y);
            path.MoveTo(endX, endY);
            path.LineTo(head2X, head2Y);

            canvas.DrawPath(path, _paint);
        }

        private void DrawDepthMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = _markerColor;
            _paint.StrokeWidth = MARKER_STROKE_WIDTH;
            _paint.SetStyle(Paint.Style.Stroke);
            _paint.TextSize = DEPTH_TEXT_SIZE * 0.6f;

            // Draw markers every 20 meters
            for (int depth = 0; depth <= MAX_DEPTH; depth += 20)
            {
                float angle = (depth / _maxValue) * 360f;
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * MARKER_LENGTH;
                
                // Draw marker line
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _paint);

                // Draw depth text
                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + DEPTH_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{depth}", textX, textY, _paint);
            }
        }

        private void DrawDepthValue(Canvas canvas, float centerX, float centerY)
        {
            // Calculate text sizes and positions first
            _paint.TextSize = DEPTH_TEXT_SIZE * 3.75f;
            float depthWidth = _paint.MeasureText($"{_currentValue:F1}");
            _paint.TextSize = DEPTH_TEXT_SIZE * 1.8f;
            float unitWidth = _paint.MeasureText(_unit);
            
            // Calculate box dimensions with padding
            float padding = DEPTH_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(depthWidth, unitWidth) + padding * 2;
            float boxHeight = DEPTH_TEXT_SIZE * 6.0f + padding * 2;
            
            // Draw semi-transparent background box
            _paint.Color = Color.ParseColor("#80000000"); // Black with 50% alpha
            _paint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, DEPTH_TEXT_SIZE * 0.75f, DEPTH_TEXT_SIZE * 0.75f, _paint);
            
            // Draw depth value text
            _paint.Color = _textColor;
            _paint.TextSize = DEPTH_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_currentValue:F1}", centerX, centerY, _paint);
            
            // Draw unit text
            _paint.TextSize = DEPTH_TEXT_SIZE * 1.8f;
            canvas.DrawText(_unit, centerX, centerY + DEPTH_TEXT_SIZE * 3.0f, _paint);
        }

        private void DrawWarningText(Canvas canvas, float centerX, float centerY, string text, Color color, float radius)
        {
            _paint.Color = color;
            _paint.TextSize = DEPTH_TEXT_SIZE;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawText(text, centerX, centerY - radius / 2, _paint);
        }
    }
} 