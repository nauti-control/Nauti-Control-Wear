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

        public BluetoothViewHolder (TextView nameTextView)
        {
            Name = nameTextView;
        }
    }
}
