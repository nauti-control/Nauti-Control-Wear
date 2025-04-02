using Android.Widget;
using Nauti_Control_Wear.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauti_Control_Wear.Adapters
{
    public class BluetoothViewHolder : Java.Lang.Object
    {
        private TextView _name;
        public BluetoothDeviceVM? Device { get; private set; }

        public BluetoothViewHolder(TextView nameTextView)
        {
            _name = nameTextView ?? throw new ArgumentNullException(nameof(nameTextView));
        }

        public void SetDevice(BluetoothDeviceVM device)
        {
            Device = device ?? throw new ArgumentNullException(nameof(device));
            _name.Text = device.Name;
        }
    }
}
