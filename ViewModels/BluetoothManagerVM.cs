using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Util;
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

        public bool IsScanning { get; internal set; }

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

                        List<ScanFilter> filters = new List<ScanFilter>();
                        ScanSettings settings = new ScanSettings.Builder()
                                                       .SetScanMode(Android.Bluetooth.LE.ScanMode.Balanced)
                                                       .Build();
                        //ScanFilter? filter = new ScanFilter.Builder().SetServiceUuid(ParcelUuid.FromString("778e5a27-1cc1-4bca-994f-7b2dbe34fcc6")).Build();
                        //filters.Add(filter);
                        _bluetoothLeScanner.StartScan(filters, settings, this);

                        IsScanning = true;

                    }
                }
            }
        }

        public void StopScanning()
        {
            if (_bluetoothLeScanner != null)
            {

                _bluetoothLeScanner.StopScan(this);
                IsScanning = false;
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
                if (!BluetoothDevices.Select(m => m.Address == result.Device.Address).Any())
                {
                    System.Diagnostics.Debug.WriteLine("Bluetooth Device Found=" + result.Device?.Name?.ToString());

                    BluetoothDeviceVM bluetoothDeviceVM = new BluetoothDeviceVM(result.Device);
                    BluetoothDevices.Add(bluetoothDeviceVM);
                }

            }
        }

    }
}