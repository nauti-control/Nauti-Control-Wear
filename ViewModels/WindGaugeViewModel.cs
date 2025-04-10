using System.ComponentModel;

namespace Nauti_Control_Wear.ViewModels
{
    public class WindGaugeVM : BaseGaugeVM
    {
        private const float MAX_WIND_SPEED = 50f;
        private const float MAX_WIND_ANGLE = 360f;
        private float _windAngle;
        private float _windSpeed;
        private bool _isPortTack;
        private bool _isStarboardTack;

        public WindGaugeVM()
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
                    _windAngle = value;
                    UpdateTackStatus();
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPortTack
        {
            get => _isPortTack;
            private set
            {
                if (_isPortTack != value)
                {
                    _isPortTack = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsStarboardTack
        {
            get => _isStarboardTack;
            private set
            {
                if (_isStarboardTack != value)
                {
                    _isStarboardTack = value;
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateWindData(float windAngle, float windSpeed)
        {
            WindAngle = windAngle;
            WindSpeed = windSpeed;
            base.UpdateValue(windSpeed);
        }

        private void UpdateTackStatus()
        {
            // Normalize angle to 0-360 range
            float normalizedAngle = WindAngle % 360f;
            if (normalizedAngle < 0) normalizedAngle += 360f;

            // Port tack: wind angle between 320° and 0°
            IsPortTack = normalizedAngle >= 320f || normalizedAngle <= 40f;

            // Starboard tack: wind angle between 0° and 40°
            IsStarboardTack = normalizedAngle >= 0f && normalizedAngle <= 40f;
        }
    }
} 