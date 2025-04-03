using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Nauti_Control_Wear.ViewModels
{
    public abstract class BaseGaugeViewModel : INotifyPropertyChanged
    {
        protected float _currentValue;
        protected float _maxValue;
        protected string _unit = string.Empty;
        protected string _label = string.Empty;

        public float CurrentValue
        {
            get => _currentValue;
            protected set
            {
                if (_currentValue != value)
                {
                    _currentValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            protected set
            {
                if (_maxValue != value)
                {
                    _maxValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Unit
        {
            get => _unit;
            protected set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Label
        {
            get => _label;
            protected set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void UpdateValue(float value)
        {
            CurrentValue = Math.Min(Math.Max(value, 0), MaxValue);
        }
    }
} 