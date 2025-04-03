using System;

namespace Nauti_Control_Wear.ViewModels
{
    public class WindGaugeViewModel : BaseGaugeViewModel
    {
        private const float MAX_WIND_SPEED = 50f;
        private float _windAngle;
        private float _windSpeed;
        private const float CLOSE_HAULED_ANGLE = 40f;

        public WindGaugeViewModel()
        {
            MaxValue = MAX_WIND_SPEED;
            Unit = "kts";
            Label = "Wind";
        }

        public float WindAngle
        {
            get => _windAngle;
            private set
            {
                if (_windAngle != value)
                {
                    _windAngle = NormalizeAngle(value);
                }
            }
        }

        public float WindSpeed
        {
            get => _windSpeed;
            private set
            {
                if (_windSpeed != value)
                {
                    _windSpeed = value;
                    UpdateValue(value);
                }
            }
        }

        public bool IsPortTack => WindAngle > 320 || WindAngle < 0;
        public bool IsStarboardTack => WindAngle > 0 && WindAngle < 40;
        public bool IsCloseHauled => IsPortTack || IsStarboardTack;

        public void UpdateWindData(float angle, float speed)
        {
            WindAngle = angle;
            WindSpeed = speed;
        }

        private float NormalizeAngle(float angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }
    }
} 