using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Path = Android.Graphics.Path;

namespace Nauti_Control_Wear.Views
{
    public class WindGaugeView : BaseGaugeView
    {
        private readonly Paint _windPaint = new Paint(PaintFlags.AntiAlias);
        private float _windAngle = 0f;
        private float _windSpeed = 0f;
        private const float MAX_WIND_SPEED = 50f;
        private const float MIN_WIND_SPEED = 0f;
        private const float ARROW_LENGTH = 0.8f;
        private const float ARROW_HEAD_LENGTH = 0.2f;
        private const float ARROW_HEAD_ANGLE = 30f;
        private const float WIND_GAUGE_STROKE_WIDTH = 5f;
        private const float WIND_TEXT_SIZE = 20f;
        private const float ANGLE_MARKER_LENGTH = 0.1f;
        private const float ANGLE_MARKER_STROKE_WIDTH = 2f;
        
        // Close-hauled angle range (typically 30-45 degrees depending on boat type)
        private const float CLOSE_HAULED_ANGLE = 40f;
        
        // Colors for port and starboard tack indicators
        private readonly Color _portColor = Color.ParseColor("#FF0000");      // Red (left side, 320-0)
        private readonly Color _starboardColor = Color.ParseColor("#00FF00");  // Green (right side, 0-40)

        public WindGaugeView(Context context) : base(context)
        {
        }

        public WindGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();
            _windPaint.TextSize = WIND_TEXT_SIZE;
            _windPaint.TextAlign = Paint.Align.Center;
            _maxValue = MAX_WIND_SPEED;
            _unit = "kts";
            _label = "Wind";
        }

        public void UpdateWindData(float angle, float speed)
        {
            // Normalize angle to 0-360 range
            _windAngle = angle;
            while (_windAngle < 0) _windAngle += 360;
            while (_windAngle >= 360) _windAngle -= 360;
            
            _windSpeed = speed;
            UpdateValue(speed);
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2f;
            float centerY = Height / 2f;
            float radius = Math.Min(Width, Height) / 2f - GAUGE_STROKE_WIDTH;

            DrawBackground(canvas, centerX, centerY, radius);
            DrawRings(canvas, centerX, centerY, radius);
            DrawCloseHauledSectors(canvas, centerX, centerY, radius);
            DrawWindArrow(canvas, centerX, centerY, radius);
            DrawAngleMarkers(canvas, centerX, centerY, radius);
            DrawWindSpeed(canvas, centerX, centerY);
        }

        private void DrawCloseHauledSectors(Canvas canvas, float centerX, float centerY, float radius)
        {
            // Port tack close-hauled sector (red, left side)
            _windPaint.Color = _portColor;
            _windPaint.Alpha = 75; // Semi-transparent
            _windPaint.SetStyle(Paint.Style.Fill);
            
            RectF rectF = new RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
            
            // Port side (left, 320 to 0 degrees)
            // Note: DrawArc measures angles clockwise from 3 o'clock, so we need to adjust
            // 320 degrees becomes -40 degrees from 3 o'clock
            canvas.DrawArc(rectF, -40, CLOSE_HAULED_ANGLE, true, _windPaint);
            
            // Starboard tack close-hauled sector (green, right side)
            _windPaint.Color = _starboardColor;
            
            // Starboard side (right, 0 to 40 degrees)
            // Note: DrawArc measures angles clockwise from 3 o'clock, so we need to adjust
            // 0 degrees becomes -90 degrees from 3 o'clock
            canvas.DrawArc(rectF, -90, CLOSE_HAULED_ANGLE, true, _windPaint);
            
            // Reset alpha
            _windPaint.Alpha = 255;
        }

        private void DrawWindArrow(Canvas canvas, float centerX, float centerY, float radius)
        {
            // Determine arrow color based on wind angle
            if (_windAngle > 320 || _windAngle < 0)
            {
                _windPaint.Color = _portColor; // Port tack (red, left side)
            }
            else if (_windAngle > 0 && _windAngle < 40)
            {
                _windPaint.Color = _starboardColor; // Starboard tack (green, right side)
            }
            else
            {
                _windPaint.Color = Color.White; // Not close-hauled
            }
            
            _windPaint.StrokeWidth = WIND_GAUGE_STROKE_WIDTH;
            _windPaint.SetStyle(Paint.Style.Stroke);

            using var path = new Path();
            float angle = _windAngle; // No need to adjust as 0 is at top
            float arrowLength = radius * ARROW_LENGTH;
            float headLength = radius * ARROW_HEAD_LENGTH;

            // Calculate arrow points
            float radians = angle * (float)Math.PI / 180f;
            float endX = centerX + arrowLength * (float)Math.Sin(radians); // Use sin for x to get 0 at top
            float endY = centerY - arrowLength * (float)Math.Cos(radians); // Use -cos for y to get 0 at top

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

            canvas.DrawPath(path, _windPaint);
        }

        private void DrawAngleMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _windPaint.Color = Color.White;
            _windPaint.StrokeWidth = ANGLE_MARKER_STROKE_WIDTH;
            _windPaint.SetStyle(Paint.Style.Stroke);
            _windPaint.TextSize = WIND_TEXT_SIZE * 0.6f;

            // Draw markers every 30 degrees
            for (int angle = 0; angle < 360; angle += 30)
            {
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * ANGLE_MARKER_LENGTH;
                
                // Draw marker line
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _windPaint);

                // Draw angle text
                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + WIND_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{angle}°", textX, textY, _windPaint);
            }
        }

        private void DrawWindSpeed(Canvas canvas, float centerX, float centerY)
        {
            _windPaint.Color = Color.White;
            _windPaint.SetStyle(Paint.Style.Fill);
            _windPaint.TextSize = WIND_TEXT_SIZE * 1.5f;
            
            // Draw wind speed in center
            canvas.DrawText($"{_windSpeed:F1}", centerX, centerY, _windPaint);
            _windPaint.TextSize = WIND_TEXT_SIZE;
            canvas.DrawText("kts", centerX, centerY + WIND_TEXT_SIZE * 1.2f, _windPaint);
        }
    }
} 