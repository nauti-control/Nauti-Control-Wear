using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

namespace Nauti_Control_Wear.Views
{
    public abstract class BaseGaugeView : View
    {
        protected Paint _paint = new Paint(PaintFlags.AntiAlias);
        protected const float GAUGE_STROKE_WIDTH = 8f;
        protected const float TEXT_SIZE = 24f;
        protected const float MIN_VALUE = 0f;
        protected float _currentValue;
        protected float _maxValue;
        protected string _unit = string.Empty;
        protected string _label = string.Empty;

        protected BaseGaugeView(Context context) : base(context)
        {
            Initialize();
        }

        protected BaseGaugeView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            _paint.TextSize = TEXT_SIZE;
            _paint.TextAlign = Paint.Align.Center;
        }

        public virtual void UpdateValue(float value)
        {
            _currentValue = System.Math.Min(System.Math.Max(value, MIN_VALUE), _maxValue);
            Invalidate();
        }

        protected virtual void DrawBackground(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.SetStyle(Paint.Style.Fill);
            _paint.Color = Color.ParseColor("#1A1A1A");
            canvas.DrawCircle(centerX, centerY, radius, _paint);
        }

        protected virtual void DrawRings(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = Color.ParseColor("#333333");
            _paint.StrokeWidth = 2;
            _paint.SetStyle(Paint.Style.Stroke);
            for (int i = 1; i <= 3; i++)
            {
                float ringRadius = radius * (i / 3f);
                canvas.DrawCircle(centerX, centerY, ringRadius, _paint);
            }
        }

        protected virtual void DrawValueText(Canvas canvas, float centerX, float centerY, float radius)
        {
            _paint.Color = Color.White;
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawText($"{_currentValue:F1} {_unit}", centerX, centerY + radius + 30, _paint);
            canvas.DrawText(_label, centerX, centerY + radius + 60, _paint);
        }
    }
} 