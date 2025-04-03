using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using Path = Android.Graphics.Path;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Views
{
    public class SpeedGaugeView : BaseGaugeView
    {
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
        
        // Button properties
        private const float BUTTON_WIDTH_RATIO = 0.5f;
        private const float BUTTON_HEIGHT_RATIO = 0.15f;
        private const float BUTTON_PADDING = 20f;
        private const float BUTTON_SPACING = 20f;
        private RectF _buttonRect = new RectF();
        private bool _isButtonPressed = false;

        private readonly SpeedGaugeViewModel _viewModel;

        public SpeedGaugeView(Context context, SpeedGaugeViewModel viewModel) : base(context)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public SpeedGaugeView(Context context, IAttributeSet attrs, SpeedGaugeViewModel viewModel) : base(context, attrs)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SpeedGaugeViewModel.CurrentValue) ||
                e.PropertyName == nameof(SpeedGaugeViewModel.MaxValue) ||
                e.PropertyName == nameof(SpeedGaugeViewModel.Unit) ||
                e.PropertyName == nameof(SpeedGaugeViewModel.Label))
            {
                Invalidate();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            _paint.TextSize = SPEED_TEXT_SIZE;
            _paint.TextAlign = Paint.Align.Center;
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
            float angle = (_viewModel.CurrentValue / _viewModel.MaxValue) * 360f;
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

            canvas.DrawPath(path, _paint);
        }

        private void DrawSpeedMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = _markerColor;
            _paint.StrokeWidth = MARKER_STROKE_WIDTH;
            _paint.SetStyle(Paint.Style.Stroke);
            _paint.TextSize = SPEED_TEXT_SIZE * 0.6f;

            for (int speed = 0; speed <= _viewModel.MaxValue; speed += 5)
            {
                float angle = (speed / _viewModel.MaxValue) * 360f;
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * MARKER_LENGTH;
                
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _paint);

                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + SPEED_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{speed}", textX, textY, _paint);
            }
        }

        private void DrawSpeedValue(Canvas canvas, float centerX, float centerY)
        {
            _paint.TextSize = SPEED_TEXT_SIZE * 3.75f;
            float speedWidth = _paint.MeasureText($"{_viewModel.CurrentValue:F1}");
            _paint.TextSize = SPEED_TEXT_SIZE * 1.8f;
            float unitWidth = _paint.MeasureText(_viewModel.Unit);
            
            float padding = SPEED_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(speedWidth, unitWidth) + padding * 2;
            float boxHeight = SPEED_TEXT_SIZE * 6.0f + padding * 2;
            
            _paint.Color = Color.ParseColor("#80000000");
            _paint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, SPEED_TEXT_SIZE * 0.75f, SPEED_TEXT_SIZE * 0.75f, _paint);
            
            _paint.Color = _textColor;
            _paint.TextSize = SPEED_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_viewModel.CurrentValue:F1}", centerX, centerY, _paint);
            
            _paint.TextSize = SPEED_TEXT_SIZE * 1.8f;
            canvas.DrawText(_viewModel.Unit, centerX, centerY + SPEED_TEXT_SIZE * 3.0f, _paint);
        }

        private void DrawSpeedTypeButton(Canvas canvas, float centerX, float centerY)
        {
            float buttonWidth = Width * BUTTON_WIDTH_RATIO;
            float buttonHeight = Height * BUTTON_HEIGHT_RATIO;
            
            float buttonX = centerX;
            float buttonY = centerY + SPEED_TEXT_SIZE * 4.0f + BUTTON_SPACING;
            
            _buttonRect = new RectF(
                buttonX - buttonWidth / 2,
                buttonY - buttonHeight / 2,
                buttonX + buttonWidth / 2,
                buttonY + buttonHeight / 2
            );
            
            _paint.Color = _isButtonPressed ? Color.ParseColor("#404040") : Color.ParseColor("#808080");
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawRoundRect(_buttonRect, BUTTON_PADDING, BUTTON_PADDING, _paint);
            
            _paint.Color = Color.White;
            _paint.TextSize = SPEED_TEXT_SIZE * 2.0f;
            string buttonText = _viewModel.ShowSpeedOverGround ? "STW" : "SOG";
            canvas.DrawText(buttonText, buttonX, buttonY + SPEED_TEXT_SIZE * 0.6f, _paint);
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
                    _viewModel.ShowSpeedOverGround = !_viewModel.ShowSpeedOverGround;
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