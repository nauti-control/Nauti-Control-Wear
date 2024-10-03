using Android.Bluetooth;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Java.Util;
using Nauti_Control_Wear.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothDeviceVM : BluetoothGattCallback
    {
        private const string ServiceUUID = "778e5a27-1cc1-4bca-994f-7b2dbe34fcc6";

        private const string CmdCharacteristicUUID = "46ba71f1-c22c-42ae-832c-81414bde99ee";

        private const string AwaCharacteristicUUID = "fd3e532c-f882-499a-b2fd-c68cca630949";

        private const string AwsCharacteristicUUID = "6dce0177-7b5c-4274-a726-9202e292ec0c";

        private const string StwCharacteristicUUID = "83e0c967-4352-4187-9feb-d7cd3c89e01d";

        private const string SogCharacteristicUUID = "5eacdf71-af9a-458c-86db-e247db17e399";

        private const string CogCharacteristicUUID = "dc0439f8-23bc-44b0-8e0c-a8e06272f4fa";

        private const string HdgCharacteristicUUID = "799ee17b-5f1a-4962-b236-ac80185d9186";

        private const string DptCharacteristicUUID = "7fdda184-b61a-4358-9213-41a5f934a7bb";

        private List<BluetoothGattCharacteristic?> _toSubscribe = new List<BluetoothGattCharacteristic?>();

        public static BluetoothDeviceVM? ConnectedInstance { get; set; }

        public BoatData Data { get; set; }

        public bool Connected { get; set; }

        public string? Name
        {
            get
            {
                return _device.Name;
            }
        }

        ///
        public string? Address
        {
            get { return _device.Address; }
        }

        private BluetoothGattCharacteristic? _cmdCharacteristic;
        private BluetoothGattCharacteristic? _awaCharacteristic;
        private BluetoothGattCharacteristic? _awsCharacteristic;
        private BluetoothGattCharacteristic? _stwCharacteristic;
        private BluetoothGattCharacteristic? _sogCharacteristic;
        private BluetoothGattCharacteristic? _cogCharacteristic;
        private BluetoothGattCharacteristic? _hdgCharacteristic;
        private BluetoothGattCharacteristic? _dptCharacteristic;
        private BluetoothGatt? _gatt;
        private BluetoothDevice _device;

        public event EventHandler? OnDeviceDisonnected;

        public event EventHandler? OnConnected;

        public event EventHandler? OnDataUpdated;


        public BluetoothDeviceVM(BluetoothDevice device)
        {
            _device = device;
            Data = new BoatData();
        }

        public void Connect()
        {
            if (_device != null)
            {
                _gatt = _device.ConnectGatt(null, false, this);
            }

        }

        public void Disconnect()
        {
            if (_gatt != null)
            {
                _gatt.Disconnect();
            }

        }

        public void SendCommand(int command)
        {
            if (_cmdCharacteristic != null && _gatt != null)
            {
                _cmdCharacteristic.SetValue(command.ToString());
                _gatt.WriteCharacteristic(_cmdCharacteristic);
            }
        }
        public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
        {

            if (newState == ProfileState.Connected && gatt != null)
            {


                // Discover services and characteristics
                gatt.DiscoverServices();
            }
            else if (newState == ProfileState.Disconnected)
            {
                if (OnDeviceDisonnected != null)
                {
                    OnDeviceDisonnected(this, null);
                }
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
        {
            if (status == GattStatus.Success && gatt != null)
            {

                // Find the characteristic you want to write to
                BluetoothGattService? gattService = gatt.GetService(UUID.FromString(ServiceUUID));

                if (gattService != null)
                {
                    _cmdCharacteristic = gattService.GetCharacteristic(UUID.FromString(CmdCharacteristicUUID));

                    if (_cmdCharacteristic != null)
                    {
                        if (OnConnected != null)
                        {

                            Connected = true;
                            ConnectedInstance = this;
                            OnConnected(this, null);


                        }
                    }

                    _awsCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwsCharacteristicUUID));
                    if (_awsCharacteristic != null) _toSubscribe.Add(_awsCharacteristic);

                    _awaCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwaCharacteristicUUID));
                    if (_awaCharacteristic != null) _toSubscribe.Add(_awaCharacteristic);

                    _stwCharacteristic = gattService.GetCharacteristic(UUID.FromString(StwCharacteristicUUID));
                    if (_stwCharacteristic != null) _toSubscribe.Add(_stwCharacteristic);

                    _cogCharacteristic = gattService.GetCharacteristic(UUID.FromString(CogCharacteristicUUID));
                    if (_cogCharacteristic != null) _toSubscribe.Add(_cogCharacteristic);

                    _sogCharacteristic = gattService.GetCharacteristic(UUID.FromString(SogCharacteristicUUID));
                    if (_sogCharacteristic != null) _toSubscribe.Add(_sogCharacteristic);

                    _hdgCharacteristic = gattService.GetCharacteristic(UUID.FromString(HdgCharacteristicUUID));
                    if (_hdgCharacteristic != null) _toSubscribe.Add(_hdgCharacteristic);

                    _dptCharacteristic = gattService.GetCharacteristic(UUID.FromString(DptCharacteristicUUID));
                    if (_dptCharacteristic != null) _toSubscribe.Add(_dptCharacteristic);

                    EnableNotification(gatt);

                }
            }
            else
            {
                Disconnect();
            }
        }



        /// <summary>
        /// Enable Notification
        /// </summary>
        /// <param name="gatt"></param>
        private void EnableNotification(BluetoothGatt? gatt)
        {
            BluetoothGattCharacteristic? characteristic = _toSubscribe.First();
            if (characteristic != null)
            {
                gatt.SetCharacteristicNotification(characteristic, true);

                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());

                gatt.WriteDescriptor(descriptor);

            }
        }

        /// <summary>
        /// On Descriptor Write
        /// </summary>
        /// <param name="gatt">gatt</param>
        /// <param name="descriptor">descriptor</param>
        /// <param name="status">status/param>
        public override void OnDescriptorWrite(BluetoothGatt? gatt, BluetoothGattDescriptor? descriptor, [GeneratedEnum] GattStatus status)
        {
            base.OnDescriptorWrite(gatt, descriptor, status);
            if (_toSubscribe.Count > 1)
            {
                _toSubscribe.RemoveAt(0);
                EnableNotification(gatt);
            }

        }

        /// <summary>
        /// Await Characteristic Changes
        /// </summary>
        /// <param name="gatt">Gatt</param>
        /// <param name="characteristic">Characteristic</param>
        /// <param name="value">Value</param>

        public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            if (characteristic != null)
            {
                byte[]? value = characteristic.GetValue();

                if (value != null)
                {
                    double dobValue = BitConverter.ToDouble(value);
                    if (UUID.FromString(AwaCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.AWA = dobValue;
                    }
                    else if (UUID.FromString(AwsCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.AWS = dobValue;
                    }
                    else if (UUID.FromString(StwCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.STW = dobValue;
                    }
                    else if (UUID.FromString(SogCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.SOG = dobValue;
                    }
                    else if (UUID.FromString(CogCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.COG = dobValue;
                    }
                    else if (UUID.FromString(HdgCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.HDG = dobValue;
                    }
                    else if (UUID.FromString(DptCharacteristicUUID).Equals(characteristic.Uuid))
                    {
                        Data.DPT = dobValue;
                    }
                    if (OnDataUpdated != null)
                    {

                        OnDataUpdated(this, null);
                    }
                }



            }
        }
    }
}