using System;

namespace Nauti_Control_Wear.ViewModels
{
    public class CompassGaugeViewModel : BaseGaugeViewModel
    {
        private const float MAX_DEGREES = 360f;
        private float _heading;
        private float _courseOverGround;
        private bool _showHeading = true;

        public CompassGaugeViewModel()
        {
            MaxValue = MAX_DEGREES;
            Unit = "°";
            Label = "Heading";
        }

        public float Heading
        {
            get => _heading;
            private set
            {
                if (_heading != value)
                {
                    _heading = NormalizeAngle(value);
                    if (_showHeading)
                    {
                        UpdateValue(value);
                    }
                }
            }
        }

        public float CourseOverGround
        {
            get => _courseOverGround;
            private set
            {
                if (_courseOverGround != value)
                {
                    _courseOverGround = NormalizeAngle(value);
                    if (!_showHeading)
                    {
                        UpdateValue(value);
                    }
                }
            }
        }

        public bool ShowHeading
        {
            get => _showHeading;
            set
            {
                if (_showHeading != value)
                {
                    _showHeading = value;
                    UpdateValue(value ? Heading : CourseOverGround);
                }
            }
        }

        public void UpdateCompassData(float heading, float cog)
        {
            Heading = heading;
            CourseOverGround = cog;
            UpdateValue(_showHeading ? heading : cog);
        }

        private float NormalizeAngle(float angle)
        {
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            return angle;
        }
    }
} 