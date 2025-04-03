using System;

namespace Nauti_Control_Wear.ViewModels
{
    public class DepthGaugeViewModel : BaseGaugeViewModel
    {
        private const float MAX_DEPTH = 100f; // 100 meters max depth
        private const float CRITICAL_DEPTH = 3f; // 3 meters critical warning
        private const float SHALLOW_WATER_THRESHOLD = 10f; // 10 meters shallow water warning
        private bool _flashWarning;
        private long _lastFlashTime;
        private const long FLASH_INTERVAL = 500; // Flash every 500ms

        public DepthGaugeViewModel()
        {
            MaxValue = MAX_DEPTH;
            Unit = "M";
            Label = "Depth";
        }

        public bool IsCriticalDepth => CurrentValue <= CRITICAL_DEPTH;
        public bool IsShallowWater => CurrentValue < SHALLOW_WATER_THRESHOLD;
        public bool FlashWarning
        {
            get => _flashWarning;
            private set
            {
                if (_flashWarning != value)
                {
                    _flashWarning = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void UpdateValue(float value)
        {
            base.UpdateValue(value);
            UpdateWarningFlash();
        }

        private void UpdateWarningFlash()
        {
            if (IsCriticalDepth)
            {
                long currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (currentTime - _lastFlashTime > FLASH_INTERVAL)
                {
                    FlashWarning = !FlashWarning;
                    _lastFlashTime = currentTime;
                }
            }
            else
            {
                FlashWarning = false;
            }
        }
    }
} 