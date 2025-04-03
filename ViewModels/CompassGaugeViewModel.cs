using System.ComponentModel;

namespace Nauti_Control_Wear.ViewModels
{
    public class CompassGaugeVM : BaseGaugeVM
    {
        private const float MAX_ANGLE = 360f;
        private float _heading;
        private float _courseOverGround;
        private bool _showHeading;

        public CompassGaugeVM()
        {
            MaxValue = MAX_ANGLE;
            Unit = "°";
            Label = "Compass";
        }

        public float Heading
        {
            get => _heading;
            private set
            {
                if (_heading != value)
                {
                    _heading = value;
                    if (_showHeading)
                    {
                        base.UpdateValue(value);
                    }
                    OnPropertyChanged();
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
                    _courseOverGround = value;
                    if (!_showHeading)
                    {
                        base.UpdateValue(value);
                    }
                    OnPropertyChanged();
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
                    base.UpdateValue(value ? Heading : CourseOverGround);
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateCompassData(float heading, float cog)
        {
            Heading = heading;
            CourseOverGround = cog;
            base.UpdateValue(_showHeading ? heading : cog);
        }
    }
} 