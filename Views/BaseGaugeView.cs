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

        // Common button properties
        protected const float BUTTON_WIDTH_RATIO = 0.5f;
        protected const float BUTTON_HEIGHT_RATIO = 0.15f;
        protected const float BUTTON_PADDING = 20f;
        protected const float BUTTON_SPACING = 20f;
        protected const float BUTTON_PRESSED_ALPHA = 0.7f;
        protected const float BUTTON_NORMAL_ALPHA = 1.0f;
        protected const float BUTTON_CORNER_RADIUS = 25f;
        protected const float BUTTON_TEXT_SIZE_MULTIPLIER = 2.0f;
        protected const float BUTTON_TEXT_Y_OFFSET = 0.6f;
        
        protected RectF _buttonRect = new RectF();
        protected bool _isButtonPressed = false;
        protected float _buttonAlpha = BUTTON_NORMAL_ALPHA;

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

        protected virtual void DrawModeButton(Canvas canvas, float centerX, float centerY, float textSize, string buttonText)
        {
            float buttonWidth = Width * BUTTON_WIDTH_RATIO;
            float buttonHeight = Height * BUTTON_HEIGHT_RATIO;
            
            float buttonX = centerX;
            float buttonY = centerY + textSize * 4.0f + BUTTON_SPACING;
            
            _buttonRect = new RectF(
                buttonX - buttonWidth / 2,
                buttonY - buttonHeight / 2,
                buttonX + buttonWidth / 2,
                buttonY + buttonHeight / 2
            );
            
            // Draw button background with alpha
            _paint.Color = Color.ParseColor("#4CAF50");
            _paint.Alpha = (int)(255 * _buttonAlpha);
            _paint.SetStyle(Paint.Style.Fill);
            canvas.DrawRoundRect(_buttonRect, BUTTON_CORNER_RADIUS, BUTTON_CORNER_RADIUS, _paint);
            
            // Draw button text
            _paint.Color = Color.White;
            _paint.Alpha = 255;
            _paint.TextSize = textSize * BUTTON_TEXT_SIZE_MULTIPLIER;
            canvas.DrawText(buttonText, buttonX, buttonY + textSize * BUTTON_TEXT_Y_OFFSET, _paint);
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e == null) return false;
            
            if (e.Action == MotionEventActions.Down)
            {
                if (_buttonRect != null && _buttonRect.Contains(e.GetX(), e.GetY()))
                {
                    _isButtonPressed = true;
                    _buttonAlpha = BUTTON_PRESSED_ALPHA;
                    Invalidate();
                    return true;
                }
            }
            else if (e.Action == MotionEventActions.Up)
            {
                if (_isButtonPressed)
                {
                    _isButtonPressed = false;
                    _buttonAlpha = BUTTON_NORMAL_ALPHA;
                    Invalidate();
                    return true;
                }
            }
            return base.OnTouchEvent(e);
        }
    }
} 