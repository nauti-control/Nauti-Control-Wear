using System.ComponentModel;

namespace Nauti_Control_Wear.ViewModels
{
    public class SpeedGaugeVM : BaseGaugeVM
    {
        private const float MAX_SPEED = 20f;
        private float _speedOverGround;
        private float _speedThroughWater;
        private bool _showSpeedOverGround;

        public SpeedGaugeVM()
        {
            MaxValue = MAX_SPEED;
            Unit = "kts";
            Label = "Speed";
        }

        public float SpeedOverGround
        {
            get => _speedOverGround;
            private set
            {
                if (_speedOverGround != value)
                {
                    _speedOverGround = value;
                    if (_showSpeedOverGround)
                    {
                        base.UpdateValue(value);
                    }
                    OnPropertyChanged();
                }
            }
        }

        public float SpeedThroughWater
        {
            get => _speedThroughWater;
            private set
            {
                if (_speedThroughWater != value)
                {
                    _speedThroughWater = value;
                    if (!_showSpeedOverGround)
                    {
                        base.UpdateValue(value);
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowSpeedOverGround
        {
            get => _showSpeedOverGround;
            set
            {
                if (_showSpeedOverGround != value)
                {
                    _showSpeedOverGround = value;
                    base.UpdateValue(value ? SpeedOverGround : SpeedThroughWater);
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateSpeedValues(float sog, float stw)
        {
            SpeedOverGround = sog;
            SpeedThroughWater = stw;
            base.UpdateValue(_showSpeedOverGround ? sog : stw);
        }
    }
} 