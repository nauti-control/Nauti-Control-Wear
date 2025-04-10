using System;
using System.Collections.Generic;

namespace Nauti_Control_Wear.ViewModels
{
    public class CommandActivityVM : IDisposable
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
            CommandMenuVM = new CommandMenuVM();
            BluetoothManagerVM = new BluetoothManagerVM();
        }

        public void Dispose()
        {
            // CommandMenuVM doesn't implement IDisposable
            BluetoothManagerVM?.Dispose();
        }
    }
}