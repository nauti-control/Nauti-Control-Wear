using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Path = Android.Graphics.Path;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Views
{
    public class WindGaugeView : BaseGaugeView
    {
        private readonly Paint _windPaint = new Paint(PaintFlags.AntiAlias);
        private const float ARROW_LENGTH = 0.8f;
        private const float ARROW_HEAD_LENGTH = 0.2f;
        private const float ARROW_HEAD_ANGLE = 30f;
        private const float WIND_GAUGE_STROKE_WIDTH = 5f;
        private const float WIND_TEXT_SIZE = 20f;
        private const float ANGLE_MARKER_LENGTH = 0.1f;
        private const float ANGLE_MARKER_STROKE_WIDTH = 2f;
        
        // Colors for port and starboard tack indicators
        private readonly Color _portColor = Color.ParseColor("#FF0000");      // Red (left side, 320-0)
        private readonly Color _starboardColor = Color.ParseColor("#00FF00");  // Green (right side, 0-40)

        private readonly WindGaugeVM _viewModel;

        public WindGaugeView(Context context, WindGaugeVM viewModel) : base(context)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public WindGaugeView(Context context, IAttributeSet attrs, WindGaugeVM viewModel) : base(context, attrs)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WindGaugeVM.CurrentValue) ||
                e.PropertyName == nameof(WindGaugeVM.MaxValue) ||
                e.PropertyName == nameof(WindGaugeVM.Unit) ||
                e.PropertyName == nameof(WindGaugeVM.Label) ||
                e.PropertyName == nameof(WindGaugeVM.WindAngle) ||
                e.PropertyName == nameof(WindGaugeVM.WindSpeed))
            {
                Invalidate();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            _windPaint.TextSize = WIND_TEXT_SIZE;
            _windPaint.TextAlign = Paint.Align.Center;
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
            canvas.DrawArc(rectF, -130, 40, true, _windPaint);
            
            // Starboard tack close-hauled sector (green, right side)
            _windPaint.Color = _starboardColor;
            
            // Starboard side (right, 0 to 40 degrees)
            canvas.DrawArc(rectF, -90, 40, true, _windPaint);
            
            // Reset alpha
            _windPaint.Alpha = 255;
        }

        private void DrawWindArrow(Canvas canvas, float centerX, float centerY, float radius)
        {
            // Determine arrow color based on wind angle
            if (_viewModel.IsPortTack)
            {
                _windPaint.Color = _portColor;
            }
            else if (_viewModel.IsStarboardTack)
            {
                _windPaint.Color = _starboardColor;
            }
            else
            {
                _windPaint.Color = Color.White;
            }
            
            _windPaint.StrokeWidth = WIND_GAUGE_STROKE_WIDTH;
            _windPaint.SetStyle(Paint.Style.Stroke);

            using var path = new Path();
            float angle = _viewModel.WindAngle;
            float arrowLength = radius * ARROW_LENGTH;
            float headLength = radius * ARROW_HEAD_LENGTH;

            float radians = angle * (float)Math.PI / 180f;
            float endX = centerX + arrowLength * (float)Math.Sin(radians);
            float endY = centerY - arrowLength * (float)Math.Cos(radians);

            path.MoveTo(centerX, centerY);
            path.LineTo(endX, endY);

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

            for (int angle = 0; angle < 360; angle += 30)
            {
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * ANGLE_MARKER_LENGTH;
                
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _windPaint);

                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + WIND_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{angle}°", textX, textY, _windPaint);
            }
        }

        private void DrawWindSpeed(Canvas canvas, float centerX, float centerY)
        {
            _windPaint.TextSize = WIND_TEXT_SIZE * 3.75f;
            float speedWidth = _windPaint.MeasureText($"{_viewModel.WindSpeed:F1}");
            _windPaint.TextSize = WIND_TEXT_SIZE * 1.8f;
            float unitWidth = _windPaint.MeasureText(_viewModel.Unit);
            
            float padding = WIND_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(speedWidth, unitWidth) + padding * 2;
            float boxHeight = WIND_TEXT_SIZE * 6.0f + padding * 2;
            
            _windPaint.Color = Color.ParseColor("#80000000");
            _windPaint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, WIND_TEXT_SIZE * 0.75f, WIND_TEXT_SIZE * 0.75f, _windPaint);
            
            _windPaint.Color = Color.White;
            _windPaint.TextSize = WIND_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_viewModel.WindSpeed:F1}", centerX, centerY, _windPaint);
            
            _windPaint.TextSize = WIND_TEXT_SIZE * 1.8f;
            canvas.DrawText(_viewModel.Unit, centerX, centerY + WIND_TEXT_SIZE * 3.0f, _windPaint);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.Dispose(disposing);
        }
    }
} 