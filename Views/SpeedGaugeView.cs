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
        private const float ARROW_LENGTH = 0.8f;
        private const float ARROW_HEAD_LENGTH = 0.2f;
        private const float ARROW_HEAD_ANGLE = 30f;
        private const float SPEED_TEXT_SIZE = 20f;
        private const float MARKER_LENGTH = 0.1f;
        private const float MARKER_STROKE_WIDTH = 2f;
        
        // Colors
        private readonly Color _needleColor = Color.White;
        private readonly Color _textColor = Color.White;
        private readonly Color _markerColor = Color.White;
        
        // Speed values
        private float _speedOverGround = 0f;
        private float _speedThroughWater = 0f;
        private bool _showSpeedOverGround = true; // Default to SOG
        
        // Button properties
        private const float BUTTON_WIDTH_RATIO = 0.5f; // Half of screen width
        private const float BUTTON_HEIGHT_RATIO = 0.15f; // 15% of screen height
        private const float BUTTON_PADDING = 20f; // Increased padding for larger button
        private const float BUTTON_SPACING = 20f; // Increased spacing from text box
        private RectF _buttonRect = new RectF();
        private bool _isButtonPressed = false;

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

        protected override void Initialize()
        {
            base.Initialize();
            _paint.TextSize = SPEED_TEXT_SIZE;
            _paint.TextAlign = Paint.Align.Center;
        }

        public override void UpdateValue(float value)
        {
            if (_showSpeedOverGround)
            {
                _speedOverGround = value;
                base.UpdateValue(value);
            }
            else
            {
                _speedThroughWater = value;
                base.UpdateValue(value);
            }
        }

        public void UpdateSpeedValues(float sog, float stw)
        {
            _speedOverGround = sog;
            _speedThroughWater = stw;
            UpdateValue(_showSpeedOverGround ? sog : stw);
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2f;
            float centerY = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - GAUGE_STROKE_WIDTH;

            DrawBackground(canvas, centerX, centerY, radius);
            DrawRings(canvas, centerX, centerY, radius);
            DrawSpeedMarkers(canvas, centerX, centerY, radius);
            DrawSpeedNeedle(canvas, centerX, centerY, radius);
            DrawSpeedValue(canvas, centerX, centerY);
            DrawSpeedTypeButton(canvas, centerX, centerY);
        }

        private void DrawSpeedNeedle(Canvas canvas, float centerX, float centerY, float radius)
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

        private void DrawSpeedMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = _markerColor;
            _paint.StrokeWidth = MARKER_STROKE_WIDTH;
            _paint.SetStyle(Paint.Style.Stroke);
            _paint.TextSize = SPEED_TEXT_SIZE * 0.6f;

            // Draw markers every 5 knots
            for (int speed = 0; speed <= MAX_SPEED; speed += 5)
            {
                float angle = (speed / _maxValue) * 360f;
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * MARKER_LENGTH;
                
                // Draw marker line
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _paint);

                // Draw speed text
                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + SPEED_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{speed}", textX, textY, _paint);
            }
        }

        private void DrawSpeedValue(Canvas canvas, float centerX, float centerY)
        {
            // Calculate text sizes and positions first
            _paint.TextSize = SPEED_TEXT_SIZE * 3.75f;
            float speedWidth = _paint.MeasureText($"{_currentValue:F1}");
            _paint.TextSize = SPEED_TEXT_SIZE * 1.8f;
            float unitWidth = _paint.MeasureText(_unit);
            
            // Calculate box dimensions with padding
            float padding = SPEED_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(speedWidth, unitWidth) + padding * 2;
            float boxHeight = SPEED_TEXT_SIZE * 6.0f + padding * 2;
            
            // Draw semi-transparent background box
            _paint.Color = Color.ParseColor("#80000000"); // Black with 50% alpha
            _paint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, SPEED_TEXT_SIZE * 0.75f, SPEED_TEXT_SIZE * 0.75f, _paint);
            
            // Draw speed value text
            _paint.Color = _textColor;
            _paint.TextSize = SPEED_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_currentValue:F1}", centerX, centerY, _paint);
            
            // Draw unit text
            _paint.TextSize = SPEED_TEXT_SIZE * 1.8f;
            canvas.DrawText(_unit, centerX, centerY + SPEED_TEXT_SIZE * 3.0f, _paint);
        }

        private void DrawSpeedTypeButton(Canvas canvas, float centerX, float centerY)
        {
            // Calculate button dimensions based on screen size
            float buttonWidth = Width * BUTTON_WIDTH_RATIO;
            float buttonHeight = Height * BUTTON_HEIGHT_RATIO;
            
            // Calculate button position (below the text box)
            float buttonX = centerX;
            float buttonY = centerY + SPEED_TEXT_SIZE * 4.0f + BUTTON_SPACING;
            
            // Create button rectangle
            _buttonRect = new RectF(
                buttonX - buttonWidth / 2,
                buttonY - buttonHeight / 2,
                buttonX + buttonWidth / 2,
                buttonY + buttonHeight / 2
            );
            
            // Draw button background
            _paint.Color = _isButtonPressed ? Color.ParseColor("#404040") : Color.ParseColor("#808080");
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawRoundRect(_buttonRect, BUTTON_PADDING, BUTTON_PADDING, _paint);
            
            // Draw button text (show alternative speed type)
            _paint.Color = Color.White;
            _paint.TextSize = SPEED_TEXT_SIZE * 2.0f; // Much larger text
            string buttonText = _showSpeedOverGround ? "STW" : "SOG";
            canvas.DrawText(buttonText, buttonX, buttonY + SPEED_TEXT_SIZE * 0.6f, _paint); // Adjusted vertical offset
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e == null) return false;
            
            if (e.Action == MotionEventActions.Down)
            {
                if (_buttonRect != null && _buttonRect.Contains(e.GetX(), e.GetY()))
                {
                    _isButtonPressed = true;
                    Invalidate();
                    return true;
                }
            }
            else if (e.Action == MotionEventActions.Up)
            {
                if (_isButtonPressed && _buttonRect != null && _buttonRect.Contains(e.GetX(), e.GetY()))
                {
                    _showSpeedOverGround = !_showSpeedOverGround;
                    UpdateValue(_showSpeedOverGround ? _speedOverGround : _speedThroughWater);
                }
                _isButtonPressed = false;
                Invalidate();
                return true;
            }
            return base.OnTouchEvent(e);
        }
    }
} 