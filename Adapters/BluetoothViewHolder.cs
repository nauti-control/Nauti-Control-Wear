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
        public TextView? Name { get; set; }

        public BluetoothDeviceVM? Device { get; set; }

        public BluetoothViewHolder (TextView nameTextView)
        {
            Name = nameTextView;
        }

        public void SetDevice (BluetoothDeviceVM device)
        {
            Device = device;
            Name.Text = device.Name;

        }
    }
}
