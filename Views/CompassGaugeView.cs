using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Path = Android.Graphics.Path;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Views
{
    public class CompassGaugeView : BaseGaugeView
    {
        private readonly Paint _compassPaint = new Paint(PaintFlags.AntiAlias);
        private readonly Path _shipPath = new Path();
        private const float SHIP_SIZE = 0.08f;
        private const float TICK_LENGTH = 0.1f;
        private const float COMPASS_STROKE_WIDTH = 3f;
        private const float COMPASS_TEXT_SIZE = 18f;
        private const float SHIP_OFFSET = 0.3f;

        // Colors
        private readonly Color _headingColor = Color.Red;
        private readonly Color _cogColor = Color.Green;
        private readonly Color _ticksColor = Color.White;
        private readonly Color _textColor = Color.White;
        
        // Button properties
        private const float BUTTON_WIDTH_RATIO = 0.5f;
        private const float BUTTON_HEIGHT_RATIO = 0.15f;
        private const float BUTTON_PADDING = 20f;
        private const float BUTTON_SPACING = 20f;
        private RectF _buttonRect = new RectF();
        private bool _isButtonPressed = false;

        private readonly CompassGaugeViewModel _viewModel;

        public CompassGaugeView(Context context, CompassGaugeViewModel viewModel) : base(context)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            CreateShipShape();
        }

        public CompassGaugeView(Context context, IAttributeSet attrs, CompassGaugeViewModel viewModel) : base(context, attrs)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            CreateShipShape();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CompassGaugeViewModel.CurrentValue) ||
                e.PropertyName == nameof(CompassGaugeViewModel.MaxValue) ||
                e.PropertyName == nameof(CompassGaugeViewModel.Unit) ||
                e.PropertyName == nameof(CompassGaugeViewModel.Label) ||
                e.PropertyName == nameof(CompassGaugeViewModel.Heading) ||
                e.PropertyName == nameof(CompassGaugeViewModel.CourseOverGround))
            {
                Invalidate();
            }
        }

        private void CreateShipShape()
        {
            _shipPath.Reset();
            _shipPath.MoveTo(0, -10);  // Bow
            _shipPath.LineTo(5, 10);   // Starboard stern
            _shipPath.LineTo(0, 5);    // Center stern
            _shipPath.LineTo(-5, 10);  // Port stern
            _shipPath.Close();         // Back to bow
        }

        protected override void Initialize()
        {
            base.Initialize();
            _compassPaint.TextSize = COMPASS_TEXT_SIZE;
            _compassPaint.TextAlign = Paint.Align.Center;
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
            DrawValueText(canvas, centerX, centerY);
            DrawCompassTypeButton(canvas, centerX, centerY);
        }

        private void DrawCompassRose(Canvas canvas, float centerX, float centerY, float radius)
        {
            _compassPaint.Color = _ticksColor;
            _compassPaint.StrokeWidth = COMPASS_STROKE_WIDTH;
            _compassPaint.SetStyle(Paint.Style.Stroke);
            
            canvas.DrawCircle(centerX, centerY, radius, _compassPaint);
            
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
                
                if (i % 30 == 0)
                {
                    _compassPaint.SetStyle(Paint.Style.Fill);
                    float textX = centerX + (radius - radius * TICK_LENGTH * 3) * (float)Math.Sin(tickRadians);
                    float textY = centerY - (radius - radius * TICK_LENGTH * 3) * (float)Math.Cos(tickRadians) + 8;
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
                float y = centerY - (radius - radius * TICK_LENGTH * 5) * (float)Math.Cos(radians) + 8;

                _compassPaint.Color = (cardinalPoints[i] == "N") ? Color.Red : Color.White;
                canvas.DrawText(cardinalPoints[i], x, y, _compassPaint);
            }
            
            _compassPaint.TextSize = COMPASS_TEXT_SIZE;
        }

        private void DrawHeadingIndicator(Canvas canvas, float centerX, float centerY, float radius)
        {
            canvas.Save();
            
            float shipY = centerY - radius * SHIP_OFFSET;
            canvas.Translate(centerX, shipY);
            
            float shipSize = radius * SHIP_SIZE;
            canvas.Scale(shipSize, shipSize);
            
            _compassPaint.Color = _headingColor;
            _compassPaint.SetStyle(Paint.Style.Fill);
            canvas.DrawPath(_shipPath, _compassPaint);
            
            canvas.Restore();
        }

        private void DrawValueText(Canvas canvas, float centerX, float centerY)
        {
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 3.75f;
            float valueWidth = _compassPaint.MeasureText($"{_viewModel.CurrentValue:F0}");
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 1.8f;
            float unitWidth = _compassPaint.MeasureText(_viewModel.Unit);
            
            float padding = COMPASS_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(valueWidth, unitWidth) + padding * 2;
            float boxHeight = COMPASS_TEXT_SIZE * 6.0f + padding * 2;
            
            _compassPaint.Color = Color.ParseColor("#80000000");
            _compassPaint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, COMPASS_TEXT_SIZE * 0.75f, COMPASS_TEXT_SIZE * 0.75f, _compassPaint);
            
            _compassPaint.Color = _textColor;
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_viewModel.CurrentValue:F0}", centerX, centerY, _compassPaint);
            
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 1.8f;
            canvas.DrawText(_viewModel.Unit, centerX, centerY + COMPASS_TEXT_SIZE * 3.0f, _compassPaint);
        }

        private void DrawCompassTypeButton(Canvas canvas, float centerX, float centerY)
        {
            float buttonWidth = Width * BUTTON_WIDTH_RATIO;
            float buttonHeight = Height * BUTTON_HEIGHT_RATIO;
            
            float buttonX = centerX;
            float buttonY = centerY + COMPASS_TEXT_SIZE * 4.0f + BUTTON_SPACING;
            
            _buttonRect = new RectF(
                buttonX - buttonWidth / 2,
                buttonY - buttonHeight / 2,
                buttonX + buttonWidth / 2,
                buttonY + buttonHeight / 2
            );
            
            _compassPaint.Color = _isButtonPressed ? Color.ParseColor("#404040") : Color.ParseColor("#808080");
            _compassPaint.SetStyle(Paint.Style.Fill);
            canvas.DrawRoundRect(_buttonRect, BUTTON_PADDING, BUTTON_PADDING, _compassPaint);
            
            _compassPaint.Color = Color.White;
            _compassPaint.TextSize = COMPASS_TEXT_SIZE * 2.0f;
            string buttonText = _viewModel.ShowHeading ? "COG" : "HDG";
            canvas.DrawText(buttonText, buttonX, buttonY + COMPASS_TEXT_SIZE * 0.6f, _compassPaint);
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
                    _viewModel.ShowHeading = !_viewModel.ShowHeading;
                }
                _isButtonPressed = false;
                Invalidate();
                return true;
            }
            return base.OnTouchEvent(e);
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