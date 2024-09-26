using Nauti_Control_Wear.ViewController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauti_Control_Wear.ViewModels
{
    public class DataDisplayActivityVM
    {

        /// <summary>
        /// View Controler
        /// </summary>
        private IDataDisplayVC _dataDisplayVC;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataDisplayVC"></param>
        public DataDisplayActivityVM(IDataDisplayVC dataDisplayVC)
        {
            _dataDisplayVC = dataDisplayVC;
            if (BluetoothDeviceVM.ConnectedInstance != null)
            {
                BluetoothDeviceVM.ConnectedInstance.OnDataUpdated += ConnectedInstance_OnDataUpdated;
            }
        }

        /// <summary>
        /// On Data Updated Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectedInstance_OnDataUpdated(object? sender, EventArgs e)
        {
            BluetoothDeviceVM? device = sender as BluetoothDeviceVM;
            if (device != null)
            {
                _dataDisplayVC.UpdateDataDisplay(device.Data);
            }
        }
    }
}
