
using System;
using System.Collections.Generic;

namespace Nauti_Control_Wear.ViewModels
{
    public class CommandActivityVM
    {
        /// <summary>
        /// Command Menu VM
        /// </summary>
        public CommandMenuVM? CommandMenuVM { get; private set; }
        /// <summary>
        /// Bluetooth Manager VM
        /// </summary>
        public BluetoothManagerVM BluetoothManagerVM { get; private set; }
        /// <summary>
        /// Command Activty Constructor
        /// </summary>
        public CommandActivityVM()
        {
            BluetoothManagerVM = new BluetoothManagerVM();
            CommandMenuVM = new CommandMenuVM();



        }


    }
}