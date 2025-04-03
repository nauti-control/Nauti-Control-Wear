using System;

namespace Nauti_Control_Wear.ViewModels
{
    public class SpeedGaugeViewModel : BaseGaugeViewModel
    {
        private const float MAX_SPEED = 30f; // 30 knots max speed
        private float _speedOverGround;
        private float _speedThroughWater;
        private bool _showSpeedOverGround = true;

        public SpeedGaugeViewModel()
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
                        UpdateValue(value);
                    }
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
                        UpdateValue(value);
                    }
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
                    UpdateValue(value ? SpeedOverGround : SpeedThroughWater);
                }
            }
        }

        public void UpdateSpeedValues(float sog, float stw)
        {
            SpeedOverGround = sog;
            SpeedThroughWater = stw;
            UpdateValue(_showSpeedOverGround ? sog : stw);
        }
    }
} 