using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using Path = Android.Graphics.Path;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Views
{
    public class DepthGaugeView : BaseGaugeView
    {
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
        private readonly Color _warningColor = Color.ParseColor("#FF0000");

        private readonly DepthGaugeViewModel _viewModel;

        public DepthGaugeView(Context context, DepthGaugeViewModel viewModel) : base(context)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public DepthGaugeView(Context context, IAttributeSet attrs, DepthGaugeViewModel viewModel) : base(context, attrs)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DepthGaugeViewModel.CurrentValue) ||
                e.PropertyName == nameof(DepthGaugeViewModel.MaxValue) ||
                e.PropertyName == nameof(DepthGaugeViewModel.Unit) ||
                e.PropertyName == nameof(DepthGaugeViewModel.Label) ||
                e.PropertyName == nameof(DepthGaugeViewModel.FlashWarning))
            {
                Invalidate();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            _paint.TextSize = DEPTH_TEXT_SIZE;
            _paint.TextAlign = Paint.Align.Center;
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

            if (_viewModel.IsCriticalDepth)
            {
                if (_viewModel.FlashWarning)
                {
                    DrawWarningText(canvas, centerX, centerY, "DANGER! VERY SHALLOW!", _warningColor, radius);
                }
            }
            else if (_viewModel.IsShallowWater)
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

        private void DrawDepthMarkers(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = _markerColor;
            _paint.StrokeWidth = MARKER_STROKE_WIDTH;
            _paint.SetStyle(Paint.Style.Stroke);
            _paint.TextSize = DEPTH_TEXT_SIZE * 0.6f;

            for (int depth = 0; depth <= _viewModel.MaxValue; depth += 20)
            {
                float angle = (depth / _viewModel.MaxValue) * 360f;
                float radians = angle * (float)Math.PI / 180f;
                float markerLength = radius * MARKER_LENGTH;
                
                float startX = centerX + (radius - markerLength) * (float)Math.Sin(radians);
                float startY = centerY - (radius - markerLength) * (float)Math.Cos(radians);
                float endX = centerX + radius * (float)Math.Sin(radians);
                float endY = centerY - radius * (float)Math.Cos(radians);
                
                canvas.DrawLine(startX, startY, endX, endY, _paint);

                float textX = centerX + (radius + 20) * (float)Math.Sin(radians);
                float textY = centerY - (radius + 20) * (float)Math.Cos(radians) + DEPTH_TEXT_SIZE * 0.3f;
                canvas.DrawText($"{depth}", textX, textY, _paint);
            }
        }

        private void DrawDepthValue(Canvas canvas, float centerX, float centerY)
        {
            _paint.TextSize = DEPTH_TEXT_SIZE * 3.75f;
            float depthWidth = _paint.MeasureText($"{_viewModel.CurrentValue:F1}");
            _paint.TextSize = DEPTH_TEXT_SIZE * 1.8f;
            float unitWidth = _paint.MeasureText(_viewModel.Unit);
            
            float padding = DEPTH_TEXT_SIZE * 0.75f;
            float boxWidth = Math.Max(depthWidth, unitWidth) + padding * 2;
            float boxHeight = DEPTH_TEXT_SIZE * 6.0f + padding * 2;
            
            _paint.Color = Color.ParseColor("#80000000");
            _paint.SetStyle(Paint.Style.Fill);
            RectF boxRect = new RectF(
                centerX - boxWidth / 2,
                centerY - boxHeight / 2,
                centerX + boxWidth / 2,
                centerY + boxHeight / 2
            );
            canvas.DrawRoundRect(boxRect, DEPTH_TEXT_SIZE * 0.75f, DEPTH_TEXT_SIZE * 0.75f, _paint);
            
            _paint.Color = _textColor;
            _paint.TextSize = DEPTH_TEXT_SIZE * 3.75f;
            canvas.DrawText($"{_viewModel.CurrentValue:F1}", centerX, centerY, _paint);
            
            _paint.TextSize = DEPTH_TEXT_SIZE * 1.8f;
            canvas.DrawText(_viewModel.Unit, centerX, centerY + DEPTH_TEXT_SIZE * 3.0f, _paint);
        }

        private void DrawWarningText(Canvas canvas, float centerX, float centerY, string text, Color color, float radius)
        {
            _paint.Color = color;
            _paint.TextSize = DEPTH_TEXT_SIZE;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawText(text, centerX, centerY - radius / 2, _paint);
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