using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using System;
using System.Diagnostics;

namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothManagerVM : ScanCallback
    {
        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothLeScanner _bluetoothLeScanner;

        

        public void StartScanning()
        {
            BluetoothManager bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
            _bluetoothAdapter = bluetoothManager.Adapter;
            if (_bluetoothAdapter != null)
            {
                _bluetoothLeScanner = _bluetoothAdapter.BluetoothLeScanner;

                _bluetoothLeScanner.StartScan(this);
            }
        }

        public void StopScanning()
        {
            if (_bluetoothLeScanner != null)
            {

                _bluetoothLeScanner.StopScan(this);
            }
        }
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);
            if (result != null && result.Device != null && result.Device.Name != null)
            {
                Debug.WriteLine("Bluetooth Device Found=" + result.Device?.Name?.ToString());
            
            }
        }


        /// <summary>
        /// Connect To Device And Set Connected
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private bool ConnectToDevice(BluetoothDevice device)
        {
            bool result = false;
            try
            {
              
                result = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                // Handle connection or writing errors
            }
            return result;
        }

        /// <summary>
        /// Start looking for a device if we are disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BluetoothDeviceVM_OnDeviceDisonnected(object sender, EventArgs e)
        {

          
           
        }
    }
}