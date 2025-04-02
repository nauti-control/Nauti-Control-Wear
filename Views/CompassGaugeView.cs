using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Path = Android.Graphics.Path;
using System;

namespace Nauti_Control_Wear.Views
{
    public class CompassGaugeView : BaseGaugeView
    {
        private readonly Paint _compassPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Path _shipPath = new Path();
        private float _heading = 0f;
        private float _courseOverGround = 0f;
        private const float MAX_DEGREES = 360f;
        private const float SHIP_SIZE = 0.15f;
        private const float TICK_LENGTH = 0.1f;
        private const float COMPASS_STROKE_WIDTH = 3f;
        private const float COMPASS_TEXT_SIZE = 18f;
        private const float COG_INDICATOR_LENGTH = 0.7f;

        // Colors
        private readonly Color _headingColor = Color.Red;
        private readonly Color _cogColor = Color.Green;
        private readonly Color _ticksColor = Color.LightGray;
        
        public CompassGaugeView(Context context) : base(context)
        {
        }

        public CompassGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            _compassPaint.TextSize = COMPASS_TEXT_SIZE;
            _compassPaint.TextAlign = Paint.Align.Center;
            _maxValue = MAX_DEGREES;
            _unit = "°";
            _label = "Heading";
            
            // Initialize ship shape
            CreateShipShape();
        }

        private void CreateShipShape()
        {
            _shipPath.Reset();
            // Ship shape is defined relative to center point (will be transformed when drawing)
            _shipPath.MoveTo(0, -10);  // Bow
            _shipPath.LineTo(5, 10);   // Starboard stern
            _shipPath.LineTo(0, 5);    // Center stern
            _shipPath.LineTo(-5, 10);  // Port stern
            _shipPath.Close();         // Back to bow
        }

        public void UpdateCompassData(float heading, float cog)
        {
            _heading = NormalizeAngle(heading);
            _courseOverGround = NormalizeAngle(cog);
            UpdateValue(_heading);  // Updates the display value
        }

        private float NormalizeAngle(float angle)
        {
            // Ensure angle is between 0-360
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2f;
            float centerY = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - GAUGE_STROKE_WIDTH;

            DrawBackground(canvas, centerX, centerY, radius);
            DrawCompassRose(canvas, centerX, centerY, radius);
            DrawCardinalPoints(canvas, centerX, centerY, radius);
            DrawHeadingIndicator(canvas, centerX, centerY, radius);
            DrawCOGIndicator(canvas, centerX, centerY, radius);
            DrawValueText(canvas, centerX, centerY, radius);
        }

        private void DrawCompassRose(Canvas canvas, float centerX, float centerY, float radius)
        {
            _compassPaint.Color = _ticksColor;
            _compassPaint.StrokeWidth = COMPASS_STROKE_WIDTH;
            _compassPaint.SetStyle(Paint.Style.Stroke);
            
            // Draw outer circle
            canvas.DrawCircle(centerX, centerY, radius, _compassPaint);
            
            // Draw degree ticks
            for (int i = 0; i < 360; i += 5)
            {
                float tickAngle = i;
                float tickRadians = (float)(tickAngle * Math.PI / 180f);
                
                float startX = centerX + radius * (float)Math.Sin(tickRadians);
                float startY = centerY - radius * (float)Math.Cos(tickRadians);
                
                float tickLength = (i % 30 == 0) ? TICK_LENGTH * 2 : (i % 15 == 0) ? TICK_LENGTH * 1.5f : TICK_LENGTH;
                float endX = centerX + (radius - radius * tickLength) * (float)Math.Sin(tickRadians);
                float endY = centerY - (radius - radius * tickLength) * (float)Math.Cos(tickRadians);
                
                canvas.DrawLine(startX, startY, endX, endY, _compassPaint);
                
                // Draw degree numbers at 30° intervals
                if (i % 30 == 0)
                {
                    _compassPaint.SetStyle(Paint.Style.Fill);
                    float textX = centerX + (radius - radius * TICK_LENGTH * 3) * (float)Math.Sin(tickRadians);
                    float textY = centerY - (radius - radius * TICK_LENGTH * 3) * (float)Math.Cos(tickRadians) + 8; // +8 for vertical centering
                    canvas.DrawText(i.ToString(), textX, textY, _compassPaint);
                    _compassPaint.SetStyle(Paint.Style.Stroke);
                }
            }
        }

        private void DrawCardinalPoints(Canvas canvas, float centerX, float centerY, float radius)
        {
            _compassPaint.Color = Color.White;
            _compassPaint.SetStyle(Paint.Style.Fill);
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 1.2f;

            string[] cardinalPoints = { "N", "E", "S", "W" };
            float[] angles = { 0, 90, 180, 270 };

            for (int i = 0; i < cardinalPoints.Length; i++)
            {
                float radians = (float)(angles[i] * Math.PI / 180f);
                float x = centerX + (radius - radius * TICK_LENGTH * 5) * (float)Math.Sin(radians);
                float y = centerY - (radius - radius * TICK_LENGTH * 5) * (float)Math.Cos(radians) + 8; // +8 for vertical centering

                _compassPaint.Color = (cardinalPoints[i] == "N") ? Color.Red : Color.White;
                canvas.DrawText(cardinalPoints[i], x, y, _compassPaint);
            }
            
            _compassPaint.TextSize = COMPASS_TEXT_SIZE;
        }

        private void DrawHeadingIndicator(Canvas canvas, float centerX, float centerY, float radius)
        {
            canvas.Save();
            
            // Draw ship icon at center pointing in heading direction
            canvas.Translate(centerX, centerY);
            canvas.Rotate(_heading);
            
            float shipSize = radius * SHIP_SIZE;
            canvas.Scale(shipSize, shipSize);
            
            _compassPaint.Color = _headingColor;
            _compassPaint.SetStyle(Paint.Style.Fill);
            canvas.DrawPath(_shipPath, _compassPaint);
            
            canvas.Restore();
        }

        private void DrawCOGIndicator(Canvas canvas, float centerX, float centerY, float radius)
        {
            _compassPaint.Color = _cogColor;
            _compassPaint.StrokeWidth = COMPASS_STROKE_WIDTH;
            _compassPaint.SetStyle(Paint.Style.Stroke);
            
            float cogRadians = (float)(_courseOverGround * Math.PI / 180f);
            float length = radius * COG_INDICATOR_LENGTH;
            
            // Draw COG line
            float endX = centerX + length * (float)Math.Sin(cogRadians);
            float endY = centerY - length * (float)Math.Cos(cogRadians);
            
            canvas.DrawLine(centerX, centerY, endX, endY, _compassPaint);
            
            // Draw arrowhead
            float arrowSize = radius * 0.05f;
            float arrowAngle1 = (float)((_courseOverGround + 150) * Math.PI / 180f);
            float arrowAngle2 = (float)((_courseOverGround - 150) * Math.PI / 180f);
            
            float arrow1X = endX + arrowSize * (float)Math.Sin(arrowAngle1);
            float arrow1Y = endY - arrowSize * (float)Math.Cos(arrowAngle1);
            float arrow2X = endX + arrowSize * (float)Math.Sin(arrowAngle2);
            float arrow2Y = endY - arrowSize * (float)Math.Cos(arrowAngle2);
            
            canvas.DrawLine(endX, endY, arrow1X, arrow1Y, _compassPaint);
            canvas.DrawLine(endX, endY, arrow2X, arrow2Y, _compassPaint);
        }

        protected override void DrawValueText(Canvas canvas, float centerX, float centerY, float radius)
        {
            _compassPaint.Color = Color.White;
            _compassPaint.SetStyle(Paint.Style.Fill);
            
            // Draw heading
            _compassPaint.Color = _headingColor;
            canvas.DrawText($"HDG: {_heading:F0}°", centerX, centerY + radius + 30, _compassPaint);
            
            // Draw COG
            _compassPaint.Color = _cogColor;
            canvas.DrawText($"COG: {_courseOverGround:F0}°", centerX, centerY + radius + 60, _compassPaint);
        }
    }
} 