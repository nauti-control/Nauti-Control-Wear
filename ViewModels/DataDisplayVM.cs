using Nauti_Control_Wear.Models;
using Nauti_Control_Wear.ViewController;

namespace Nauti_Control_Wear.ViewModels
{
    public class DataDisplayVM
    {
        private readonly IDataDisplayVC _dataDisplayVC;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataDisplayVC">Data display view controller interface</param>
        public DataDisplayVM(IDataDisplayVC dataDisplayVC)
        {
            _dataDisplayVC = dataDisplayVC;
            
            // Register for data updates if we have a connected device
            if (BluetoothDeviceVM.ConnectedInstance != null)
            {
                BluetoothDeviceVM.ConnectedInstance.OnDataUpdated += ConnectedDevice_OnDataUpdated;
            }
        }

        /// <summary>
        /// Handle data updated events from the connected device
        /// </summary>
        private void ConnectedDevice_OnDataUpdated(object? sender, System.EventArgs e)
        {
            if (sender is BluetoothDeviceVM device)
            {
                _dataDisplayVC.UpdateDataDisplay(device.Data);
            }
        }
        
        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Dispose()
        {
            // Unregister from data update events
            if (BluetoothDeviceVM.ConnectedInstance != null)
            {
                BluetoothDeviceVM.ConnectedInstance.OnDataUpdated -= ConnectedDevice_OnDataUpdated;
            }
        }
    }
} 