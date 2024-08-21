using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Runtime;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothManagerVM : ScanCallback
    {
        private BluetoothAdapter? _bluetoothAdapter;

        private BluetoothLeScanner? _bluetoothLeScanner;

        private ObservableCollection<BluetoothDeviceVM> _bluetoothDeviceVMs = new ObservableCollection<BluetoothDeviceVM>();

        public ObservableCollection<BluetoothDeviceVM> BluetoothDevices
        {
            get
            {
                return _bluetoothDeviceVMs;
            }
        }



        public void StartScanning()
        {
            BluetoothManager? bluetoothManager = Android.App.Application.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            if (bluetoothManager != null)
            {
                _bluetoothAdapter = bluetoothManager.Adapter;
                if (_bluetoothAdapter != null)
                {
                    _bluetoothLeScanner = _bluetoothAdapter.BluetoothLeScanner;
                    if (_bluetoothLeScanner != null)
                    {
                        _bluetoothLeScanner.StartScan(this);
                    }
                }
            }
        }

        public void StopScanning()
        {
            if (_bluetoothLeScanner != null)
            {

                _bluetoothLeScanner.StopScan(this);
            }
        }

        /// <summary>
        /// On Scan Result
        /// </summary>
        /// <param name="callbackType">Call back</param>
        /// <param name="result">Result</param>
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            base.OnScanResult(callbackType, result);
            if (result != null && result.Device != null && result.Device.Name != null)
            {
                if (!BluetoothDevices.Select(m => m.Address==result.Device.Address).Any())
                {
                    Debug.WriteLine("Bluetooth Device Found=" + result.Device?.Name?.ToString());

                    BluetoothDeviceVM bluetoothDeviceVM = new BluetoothDeviceVM(result.Device);
                    BluetoothDevices.Add(bluetoothDeviceVM);
                }

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


    }
}