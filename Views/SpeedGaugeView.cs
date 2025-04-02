using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using Path = Android.Graphics.Path;

namespace Nauti_Control_Wear.Views
{
    public class SpeedGaugeView : BaseGaugeView
    {
        private const float MAX_SPEED = 30f; // 30 knots max speed
        private const float LOW_SPEED_THRESHOLD = 2f; // 2 knots low speed warning
        private const float CRUISE_SPEED_THRESHOLD = 6f; // 6 knots typical cruising speed
        private const float HIGH_SPEED_THRESHOLD = 20f; // 20 knots high speed warning
        
        // Colors for speed ranges
        private readonly Color _lowSpeedColor = Color.ParseColor("#808080");     // Gray for very low/stopped
        private readonly Color _cruiseSpeedColor = Color.ParseColor("#00FF00"); // Green for cruising speed
        private readonly Color _highSpeedColor = Color.ParseColor("#FFA500");   // Orange for high speed
        private readonly Color _dangerSpeedColor = Color.ParseColor("#FF0000"); // Red for dangerous speed
        
        // Pointer animation
        private float _animatedValue = 0f;
        private const float ANIMATION_SPEED = 0.2f; // Animation speed factor
        
        // Speed trend tracking
        private float _previousSpeed = -1f;
        private bool _isAccelerating = false;
        private bool _isDecelerating = false;
        private const float TREND_THRESHOLD = 0.3f; // Minimum change to indicate acceleration/deceleration

        public SpeedGaugeView(Context context) : base(context)
        {
            _maxValue = MAX_SPEED;
            _unit = "kts";
            _label = "Speed";
        }

        public SpeedGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _maxValue = MAX_SPEED;
            _unit = "kts";
            _label = "Speed";
        }
        
        public override void UpdateValue(float value)
        {
            // Track speed trend
            if (_previousSpeed >= 0)
            {
                _isAccelerating = value > _previousSpeed + TREND_THRESHOLD;
                _isDecelerating = value < _previousSpeed - TREND_THRESHOLD;
            }
            _previousSpeed = value;
            
            // Use smooth animation for speed changes
            if (Math.Abs(_animatedValue - value) > 0.1f)
            {
                _animatedValue += (value - _animatedValue) * ANIMATION_SPEED;
                PostInvalidateOnAnimation();
            }
            else
            {
                _animatedValue = value;
            }
            
            base.UpdateValue(value);
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2f;
            float centerY = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - GAUGE_STROKE_WIDTH;

            DrawBackground(canvas, centerX, centerY, radius);
            DrawRings(canvas, centerX, centerY, radius);
            DrawSpeedZones(canvas, centerX, centerY, radius);
            DrawSpeedArc(canvas, centerX, centerY, radius);
            DrawSpeedMarkers(canvas, centerX, centerY, radius);
            DrawSpeedNeedle(canvas, centerX, centerY, radius);
            DrawTrendIndicator(canvas, centerX, centerY);
            DrawValueText(canvas, centerX, centerY, radius);

            // Draw speed warnings if needed
            if (_currentValue < LOW_SPEED_THRESHOLD)
            {
                DrawSpeedLabel(canvas, centerX, centerY, "LOW SPEED", _lowSpeedColor, radius);
            }
            else if (_currentValue > HIGH_SPEED_THRESHOLD)
            {
                DrawSpeedLabel(canvas, centerX, centerY, "HIGH SPEED", _highSpeedColor, radius);
            }
            else if (_currentValue >= CRUISE_SPEED_THRESHOLD && _currentValue <= HIGH_SPEED_THRESHOLD)
            {
                DrawSpeedLabel(canvas, centerX, centerY, "CRUISING", _cruiseSpeedColor, radius);
            }
        }
        
        private void DrawSpeedZones(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.SetStyle(Paint.Style.Fill);
            _paint.Alpha = 40; // Very transparent
            
            RectF rectF = new RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            
            // Low speed zone (0-2 knots) - Gray
            _paint.Color = _lowSpeedColor;
            float lowSpeedAngle = (LOW_SPEED_THRESHOLD / MAX_SPEED) * 360f;
            canvas.DrawArc(rectF, -90, lowSpeedAngle, true, _paint);
            
            // Cruising speed zone (2-6 knots) - Light green
            _paint.Color = Color.ParseColor("#80FF80"); // Lighter green
            float cruiseSpeedStartAngle = (LOW_SPEED_THRESHOLD / MAX_SPEED) * 360f;
            float cruiseSpeedSweepAngle = ((CRUISE_SPEED_THRESHOLD - LOW_SPEED_THRESHOLD) / MAX_SPEED) * 360f;
            canvas.DrawArc(rectF, -90 + cruiseSpeedStartAngle, cruiseSpeedSweepAngle, true, _paint);
            
            // Optimal cruising speed zone (6-20 knots) - Green
            _paint.Color = _cruiseSpeedColor;
            float optimalSpeedStartAngle = (CRUISE_SPEED_THRESHOLD / MAX_SPEED) * 360f;
            float optimalSpeedSweepAngle = ((HIGH_SPEED_THRESHOLD - CRUISE_SPEED_THRESHOLD) / MAX_SPEED) * 360f;
            canvas.DrawArc(rectF, -90 + optimalSpeedStartAngle, optimalSpeedSweepAngle, true, _paint);
            
            // High speed zone (20-30 knots) - Orange to Red gradient
            _paint.Color = _highSpeedColor;
            float highSpeedStartAngle = (HIGH_SPEED_THRESHOLD / MAX_SPEED) * 360f;
            float highSpeedSweepAngle = ((MAX_SPEED - HIGH_SPEED_THRESHOLD) / MAX_SPEED) * 360f;
            canvas.DrawArc(rectF, -90 + highSpeedStartAngle, highSpeedSweepAngle, true, _paint);
            
            // Reset alpha
            _paint.Alpha = 255;
        }

        private void DrawSpeedArc(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.StrokeWidth = GAUGE_STROKE_WIDTH;
            float sweepAngle = (_currentValue / _maxValue) * 360f;

            // Set arc color based on speed
            if (_currentValue < LOW_SPEED_THRESHOLD)
            {
                _paint.Color = _lowSpeedColor; // Gray for low speed
            }
            else if (_currentValue <= CRUISE_SPEED_THRESHOLD)
            {
                _paint.Color = Color.ParseColor("#80FF80"); // Light green for approaching cruise
            }
            else if (_currentValue <= HIGH_SPEED_THRESHOLD)
            {
                _paint.Color = _cruiseSpeedColor; // Green for normal speed
            }
            else
            {
                _paint.Color = _highSpeedColor; // Orange for high speed
            }

            _paint.SetStyle(Paint.Style.Stroke);
            
            // Draw arc
            RectF rectF = new RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            canvas.DrawArc(rectF, -90f, sweepAngle, false, _paint);
            
            // Draw dot at end of arc
            float angle = (-90 + sweepAngle) * (float)Math.PI / 180f;
            float dotX = centerX + radius * (float)Math.Cos(angle);
            float dotY = centerY + radius * (float)Math.Sin(angle);
            
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawCircle(dotX, dotY, GAUGE_STROKE_WIDTH, _paint);
        }

        private void DrawSpeedNeedle(Canvas canvas, float centerX, float centerY, float radius)
        {
            // Use animatedValue for smooth transitions
            float sweepAngle = (_animatedValue / _maxValue) * 360f;
            float angle = (-90 + sweepAngle) * (float)Math.PI / 180f;
            
            // Select needle color based on speed
            if (_animatedValue < LOW_SPEED_THRESHOLD)
            {
                _paint.Color = _lowSpeedColor;
            }
            else if (_animatedValue <= CRUISE_SPEED_THRESHOLD)
            {
                _paint.Color = Color.ParseColor("#80FF80");
            }
            else if (_animatedValue <= HIGH_SPEED_THRESHOLD)
            {
                _paint.Color = _cruiseSpeedColor;
            }
            else
            {
                _paint.Color = _highSpeedColor;
            }
            
            // Draw needle
            float needleLength = radius * 0.85f;
            float baseWidth = 8f;
            _paint.StrokeWidth = 2f;
            // Use both fill and stroke for the needle
            _paint.SetStyle(Paint.Style.Fill);
            
            // Create a path for the needle
            Path needlePath = new Path();
            float tipX = centerX + needleLength * (float)Math.Cos(angle);
            float tipY = centerY + needleLength * (float)Math.Sin(angle);
            
            // Calculate base points perpendicular to needle angle
            float baseAngle = angle + (float)Math.PI/2;
            float baseX1 = centerX + baseWidth * (float)Math.Cos(baseAngle);
            float baseY1 = centerY + baseWidth * (float)Math.Sin(baseAngle);
            float baseX2 = centerX - baseWidth * (float)Math.Cos(baseAngle);
            float baseY2 = centerY - baseWidth * (float)Math.Sin(baseAngle);
            
            // Draw needle
            needlePath.MoveTo(tipX, tipY);
            needlePath.LineTo(baseX1, baseY1);
            needlePath.LineTo(baseX2, baseY2);
            needlePath.Close();
            
            canvas.DrawPath(needlePath, _paint);
            
            // Set stroke for outline
            _paint.SetStyle(Paint.Style.Stroke);
            canvas.DrawPath(needlePath, _paint);
            
            // Draw center hub
            _paint.Color = Color.White;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawCircle(centerX, centerY, baseWidth, _paint);
        }
        
        private void DrawTrendIndicator(Canvas canvas, float centerX, float centerY)
        {
            if (!_isAccelerating && !_isDecelerating)
                return;
                
            _paint.SetStyle(Paint.Style.Fill);
            
            float arrowSize = 12f;
            float xCenter = centerX;
            float yCenter = centerY + 40; // Position below center
            
            Path arrowPath = new Path();
            
            if (_isAccelerating)
            {
                // Increasing speed
                _paint.Color = (_currentValue > HIGH_SPEED_THRESHOLD) ? _dangerSpeedColor : _cruiseSpeedColor;
                
                // Draw right arrow
                arrowPath.MoveTo(xCenter + arrowSize, yCenter);
                arrowPath.LineTo(xCenter - arrowSize/2, yCenter + arrowSize);
                arrowPath.LineTo(xCenter - arrowSize/2, yCenter - arrowSize);
                arrowPath.Close();
                
                canvas.DrawPath(arrowPath, _paint);
                
                // Draw second arrow for strong acceleration
                if (_currentValue > _previousSpeed + TREND_THRESHOLD * 2)
                {
                    arrowPath.Reset();
                    arrowPath.MoveTo(xCenter + arrowSize - 15, yCenter);
                    arrowPath.LineTo(xCenter - arrowSize/2 - 15, yCenter + arrowSize);
                    arrowPath.LineTo(xCenter - arrowSize/2 - 15, yCenter - arrowSize);
                    arrowPath.Close();
                    
                    canvas.DrawPath(arrowPath, _paint);
                }
            }
            else if (_isDecelerating)
            {
                // Decreasing speed
                _paint.Color = _lowSpeedColor;
                
                // Draw left arrow
                arrowPath.MoveTo(xCenter - arrowSize, yCenter);
                arrowPath.LineTo(xCenter + arrowSize/2, yCenter + arrowSize);
                arrowPath.LineTo(xCenter + arrowSize/2, yCenter - arrowSize);
                arrowPath.Close();
                
                canvas.DrawPath(arrowPath, _paint);
                
                // Draw second arrow for strong deceleration
                if (_currentValue < _previousSpeed - TREND_THRESHOLD * 2)
                {
                    arrowPath.Reset();
                    arrowPath.MoveTo(xCenter - arrowSize + 15, yCenter);
                    arrowPath.LineTo(xCenter + arrowSize/2 + 15, yCenter + arrowSize);
                    arrowPath.LineTo(xCenter + arrowSize/2 + 15, yCenter - arrowSize);
                    arrowPath.Close();
                    
                    canvas.DrawPath(arrowPath, _paint);
                }
            }
        }

        private void DrawSpeedMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = Color.White;
            _paint.SetStyle(Paint.Style.Fill);
            _paint.TextSize = TEXT_SIZE / 2;

            // Draw colored speed threshold markers
            DrawColoredMarker(canvas, centerX, centerY, radius, LOW_SPEED_THRESHOLD, _lowSpeedColor);
            DrawColoredMarker(canvas, centerX, centerY, radius, CRUISE_SPEED_THRESHOLD, _cruiseSpeedColor);
            DrawColoredMarker(canvas, centerX, centerY, radius, HIGH_SPEED_THRESHOLD, _highSpeedColor);
            
            // Draw regular speed markers
            for (int i = 0; i <= 6; i++)
            {
                float speed = i * 5f;
                DrawMarker(canvas, centerX, centerY, radius, speed, Color.White);
            }
            
            // Draw minor ticks
            for (int i = 0; i < 30; i++)
            {
                if (i % 5 != 0) // Skip positions where major ticks are
                {
                    float speed = i;
                    float angle = (speed / MAX_SPEED) * 360f;
                    float radian = (float)((angle - 90) * Math.PI / 180);
                    float x = centerX + radius * (float)Math.Cos(radian);
                    float y = centerY + radius * (float)Math.Sin(radian);
                    
                    // Draw smaller tick mark
                    _paint.Color = Color.Gray;
                    _paint.StrokeWidth = 1f;
                    float tickLength = 5f;
                    float innerX = centerX + (radius - tickLength) * (float)Math.Cos(radian);
                    float innerY = centerY + (radius - tickLength) * (float)Math.Sin(radian);
                    canvas.DrawLine(x, y, innerX, innerY, _paint);
                }
            }
        }
        
        private void DrawColoredMarker(Canvas canvas, float centerX, float centerY, float radius, 
                                     float speed, Color color)
        {
            float angle = (speed / MAX_SPEED) * 360f;
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

            // Draw speed value with colored text
            _paint.TextSize = TEXT_SIZE / 2;
            float textX = centerX + (radius - markerLength - 15) * (float)Math.Cos(radian);
            float textY = centerY + (radius - markerLength - 15) * (float)Math.Sin(radian);
            // Adjust text position for better readability
            textY += 5; // Slight vertical adjustment
            
            canvas.DrawText($"{speed}", textX, textY, _paint);
        }
        
        private void DrawMarker(Canvas canvas, float centerX, float centerY, float radius, 
                              float speed, Color color)
        {
            float angle = (speed / MAX_SPEED) * 360f;
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

            // Only draw labels for major markers (multiples of 5)
            if (speed % 5 == 0)
            {
                // Draw speed value
                _paint.TextSize = TEXT_SIZE / 2.5f;
                float textX = centerX + (radius + 5) * (float)Math.Cos(radian);
                float textY = centerY + (radius + 5) * (float)Math.Sin(radian);
                // Adjust text position for better readability
                textY += 5; // Slight vertical adjustment
                
                canvas.DrawText($"{speed}", textX, textY, _paint);
            }
        }

        protected override void DrawValueText(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.SetStyle(Paint.Style.Fill);
            
            // Set text color based on speed
            if (_currentValue < LOW_SPEED_THRESHOLD)
            {
                _paint.Color = _lowSpeedColor;
            }
            else if (_currentValue <= CRUISE_SPEED_THRESHOLD)
            {
                _paint.Color = Color.ParseColor("#80FF80");
            }
            else if (_currentValue <= HIGH_SPEED_THRESHOLD)
            {
                _paint.Color = _cruiseSpeedColor;
            }
            else
            {
                _paint.Color = _highSpeedColor;
            }
            
            // Draw main speed value
            _paint.TextSize = TEXT_SIZE * 1.5f;
            canvas.DrawText($"{_currentValue:F1}", centerX, centerY, _paint);
            
            // Draw unit and label
            _paint.TextSize = TEXT_SIZE;
            canvas.DrawText(_unit, centerX, centerY + TEXT_SIZE * 1.5f, _paint);
            canvas.DrawText(_label, centerX, centerY + radius + 40, _paint);
        }

        private void DrawSpeedLabel(Canvas canvas, float centerX, float centerY, string text, Color color, float radius)
        {
            _paint.Color = color;
            _paint.TextSize = TEXT_SIZE;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawText(text, centerX, centerY - radius / 2, _paint);
        }
    }
} 