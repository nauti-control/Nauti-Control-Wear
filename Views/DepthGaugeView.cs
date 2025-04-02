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
        private const float CRITICAL_DEPTH = 3f; // 3 meters critical warning (immediate danger)
        private const float SHALLOW_WATER_THRESHOLD = 10f; // 10 meters shallow water warning
        private const float DEEP_WATER_THRESHOLD = 50f; // 50 meters deep water warning
        
        // Animation for shallow water alerts
        private bool _flashWarning = false;
        private long _lastFlashTime = 0;
        private const long FLASH_INTERVAL = 500; // Flash every 500ms

        // Colors
        private readonly Color _dangerColor = Color.ParseColor("#FF0000");  // Red for dangerous shallow
        private readonly Color _cautionColor = Color.ParseColor("#FFA500");  // Orange for caution
        private readonly Color _safeColor = Color.ParseColor("#00FF00");    // Green for safe depths
        private readonly Color _deepColor = Color.ParseColor("#0000FF");    // Blue for deep water

        // Depth trend tracking
        private float _previousDepth = -1f;
        private bool _isTrendUp = false;
        private bool _isTrendDown = false;
        private const float TREND_THRESHOLD = 0.5f; // Minimum change to indicate a trend

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

        public override void UpdateValue(float value)
        {
            // Track depth trend
            if (_previousDepth >= 0)
            {
                _isTrendUp = value > _previousDepth + TREND_THRESHOLD;
                _isTrendDown = value < _previousDepth - TREND_THRESHOLD;
            }
            _previousDepth = value;
            
            base.UpdateValue(value);
            
            // Flash warning animation for shallow water
            if (value <= CRITICAL_DEPTH)
            {
                long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (currentTime - _lastFlashTime > FLASH_INTERVAL)
                {
                    _flashWarning = !_flashWarning;
                    _lastFlashTime = currentTime;
                    // Force redraw for animation
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
            DrawSafetyZones(canvas, centerX, centerY, radius);
            DrawDepthArc(canvas, centerX, centerY, radius);
            DrawDepthMarkers(canvas, centerX, centerY, radius);
            DrawTrendIndicator(canvas, centerX, centerY);
            DrawValueText(canvas, centerX, centerY, radius);

            // Draw warning if in shallow water
            if (_currentValue <= CRITICAL_DEPTH)
            {
                if (_flashWarning)
                {
                    DrawWarningText(canvas, centerX, centerY, "DANGER! VERY SHALLOW!", _dangerColor, radius);
                }
            }
            else if (_currentValue < SHALLOW_WATER_THRESHOLD)
            {
                DrawWarningText(canvas, centerX, centerY, "SHALLOW WATER!", _cautionColor, radius);
            }
        }

        private void DrawSafetyZones(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.SetStyle(Paint.Style.Fill);
            _paint.Alpha = 40; // Very transparent

            RectF rectF = new RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

            // Critical danger zone (0-3m) - Red
            _paint.Color = _dangerColor;
            float criticalAngle = (CRITICAL_DEPTH / MAX_DEPTH) * 360f;
            canvas.DrawArc(rectF, -90, criticalAngle, true, _paint);

            // Caution zone (3-10m) - Orange
            _paint.Color = _cautionColor;
            float cautionAngle = ((SHALLOW_WATER_THRESHOLD - CRITICAL_DEPTH) / MAX_DEPTH) * 360f;
            canvas.DrawArc(rectF, -90 + criticalAngle, cautionAngle, true, _paint);

            // Normal zone (10-50m) - Green
            _paint.Color = _safeColor;
            float normalAngle = ((DEEP_WATER_THRESHOLD - SHALLOW_WATER_THRESHOLD) / MAX_DEPTH) * 360f;
            canvas.DrawArc(rectF, -90 + criticalAngle + cautionAngle, normalAngle, true, _paint);

            // Deep zone (50-100m) - Blue
            _paint.Color = _deepColor;
            float deepAngle = ((MAX_DEPTH - DEEP_WATER_THRESHOLD) / MAX_DEPTH) * 360f;
            canvas.DrawArc(rectF, -90 + criticalAngle + cautionAngle + normalAngle, deepAngle, true, _paint);

            // Reset alpha
            _paint.Alpha = 255;
        }

        private void DrawDepthArc(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.StrokeWidth = GAUGE_STROKE_WIDTH;
            float sweepAngle = (_currentValue / _maxValue) * 360f;
            
            // Set arc color based on depth
            if (_currentValue <= CRITICAL_DEPTH)
            {
                _paint.Color = _flashWarning ? Color.White : _dangerColor; // Flash between red and white
            }
            else if (_currentValue < SHALLOW_WATER_THRESHOLD)
            {
                _paint.Color = _cautionColor; // Orange for shallow water
            }
            else if (_currentValue < DEEP_WATER_THRESHOLD)
            {
                _paint.Color = _safeColor; // Green for normal depth
            }
            else
            {
                _paint.Color = _deepColor; // Blue for deep water
            }

            _paint.SetStyle(Paint.Style.Stroke);
            
            // Draw arc
            RectF rectF = new RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            canvas.DrawArc(rectF, -90f, sweepAngle, false, _paint);
            
            // Draw depth indicator (dot at end of arc)
            float angle = (-90 + sweepAngle) * (float)Math.PI / 180f;
            float dotX = centerX + radius * (float)Math.Cos(angle);
            float dotY = centerY + radius * (float)Math.Sin(angle);
            
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawCircle(dotX, dotY, GAUGE_STROKE_WIDTH * 1.5f, _paint);
        }

        private void DrawDepthMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = Color.White;
            _paint.SetStyle(Paint.Style.Fill);
            _paint.TextSize = TEXT_SIZE / 2;

            // Draw critical depth marker
            DrawColoredMarker(canvas, centerX, centerY, radius, CRITICAL_DEPTH, _dangerColor);
            
            // Draw shallow water threshold marker
            DrawColoredMarker(canvas, centerX, centerY, radius, SHALLOW_WATER_THRESHOLD, _cautionColor);
            
            // Draw deep water threshold marker
            DrawColoredMarker(canvas, centerX, centerY, radius, DEEP_WATER_THRESHOLD, _deepColor);

            // Draw regular depth markers at 0, 20, 40, 60, 80, 100 meters
            for (int i = 0; i <= 5; i++)
            {
                float depth = i * 20f;
                DrawMarker(canvas, centerX, centerY, radius, depth, Color.White);
            }
        }
        
        private void DrawColoredMarker(Canvas canvas, float centerX, float centerY, float radius, 
                                     float depth, Color color)
        {
            float angle = (depth / _maxValue) * 360f;
            float radian = (float)((angle - 90) * Math.PI / 180);
            float x = centerX + radius * (float)Math.Cos(radian);
            float y = centerY + radius * (float)Math.Sin(radian);

            // Draw colored marker line
            _paint.Color = color;
            _paint.StrokeWidth = 3f;
            float markerLength = 15f;
            float innerX = centerX + (radius - markerLength) * (float)Math.Cos(radian);
            float innerY = centerY + (radius - markerLength) * (float)Math.Sin(radian);
            canvas.DrawLine(x, y, innerX, innerY, _paint);

            // Draw depth value with colored text
            _paint.TextSize = TEXT_SIZE / 2;
            float textX = centerX + (radius - markerLength - 15) * (float)Math.Cos(radian);
            float textY = centerY + (radius - markerLength - 15) * (float)Math.Sin(radian);
            // Adjust text position for better readability
            textY += 5; // Slight vertical adjustment
            
            canvas.DrawText($"{depth}m", textX, textY, _paint);
        }
        
        private void DrawMarker(Canvas canvas, float centerX, float centerY, float radius, 
                              float depth, Color color)
        {
            float angle = (depth / _maxValue) * 360f;
            float radian = (float)((angle - 90) * Math.PI / 180);
            float x = centerX + radius * (float)Math.Cos(radian);
            float y = centerY + radius * (float)Math.Sin(radian);

            // Draw marker line
            _paint.Color = color;
            _paint.StrokeWidth = 2f;
            float markerLength = 10f;
            float innerX = centerX + (radius - markerLength) * (float)Math.Cos(radian);
            float innerY = centerY + (radius - markerLength) * (float)Math.Sin(radian);
            canvas.DrawLine(x, y, innerX, innerY, _paint);

            // Draw small tick marks
            if (depth % 20 == 0)
            {
                // Draw depth value
                _paint.TextSize = TEXT_SIZE / 2.5f;
                float textX = centerX + (radius + 5) * (float)Math.Cos(radian);
                float textY = centerY + (radius + 5) * (float)Math.Sin(radian);
                // Adjust text position for better readability
                textY += 5; // Slight vertical adjustment
                
                canvas.DrawText($"{depth}", textX, textY, _paint);
            }
        }
        
        private void DrawTrendIndicator(Canvas canvas, float centerX, float centerY)
        {
            if (!_isTrendUp && !_isTrendDown)
                return;
                
            _paint.SetStyle(Paint.Style.Fill);
            
            float arrowSize = 15f;
            float xCenter = centerX;
            float yCenter = centerY + 50; // Position below center
            
            Path arrowPath = new Path();
            
            if (_isTrendUp)
            {
                // Decreasing depth = increasing seabed trend = potential danger
                _paint.Color = _dangerColor;
                
                // Draw up arrow
                arrowPath.MoveTo(xCenter, yCenter - arrowSize);
                arrowPath.LineTo(xCenter + arrowSize, yCenter + arrowSize);
                arrowPath.LineTo(xCenter - arrowSize, yCenter + arrowSize);
                arrowPath.Close();
                
                canvas.DrawPath(arrowPath, _paint);
                
                // Draw text
                _paint.TextSize = TEXT_SIZE / 2;
                canvas.DrawText("Seabed Rising", xCenter, yCenter + arrowSize + 15, _paint);
            }
            else if (_isTrendDown)
            {
                // Increasing depth = decreasing seabed trend = safer
                _paint.Color = _safeColor;
                
                // Draw down arrow
                arrowPath.MoveTo(xCenter, yCenter + arrowSize);
                arrowPath.LineTo(xCenter + arrowSize, yCenter - arrowSize);
                arrowPath.LineTo(xCenter - arrowSize, yCenter - arrowSize);
                arrowPath.Close();
                
                canvas.DrawPath(arrowPath, _paint);
                
                // Draw text
                _paint.TextSize = TEXT_SIZE / 2;
                canvas.DrawText("Seabed Lowering", xCenter, yCenter + arrowSize + 15, _paint);
            }
        }

        protected override void DrawValueText(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.SetStyle(Paint.Style.Fill);
            _paint.TextSize = TEXT_SIZE;
            
            // Set color based on depth
            if (_currentValue <= CRITICAL_DEPTH)
            {
                _paint.Color = _dangerColor;
            } 
            else if (_currentValue < SHALLOW_WATER_THRESHOLD)
            {
                _paint.Color = _cautionColor;
            }
            else if (_currentValue < DEEP_WATER_THRESHOLD)
            {
                _paint.Color = _safeColor;
            }
            else
            {
                _paint.Color = _deepColor;
            }
            
            // Draw value with larger text
            _paint.TextSize = TEXT_SIZE * 1.5f;
            canvas.DrawText($"{_currentValue:F1}", centerX, centerY, _paint);
            
            // Draw unit and label
            _paint.TextSize = TEXT_SIZE;
            canvas.DrawText(_unit, centerX, centerY + TEXT_SIZE * 1.5f, _paint);
            canvas.DrawText(_label, centerX, centerY + radius + 40, _paint);
        }

        private void DrawWarningText(Canvas canvas, float centerX, float centerY, string text, Color color, float radius)
        {
            _paint.Color = color;
            _paint.TextSize = TEXT_SIZE;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawText(text, centerX, centerY - radius / 2, _paint);
        }
    }
} 