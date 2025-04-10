using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;
using System;
using System.Collections.Specialized;

namespace Nauti_Control_Wear.Adapters
{
    public class BluetoothDeviceAdapter : RecyclerView.Adapter
    {
        private readonly BluetoothManagerVM _bluetoothManager;
        private readonly Action<BluetoothDeviceVM>? _itemClickListener;

        public BluetoothDeviceAdapter(BluetoothManagerVM bluetoothManager, Action<BluetoothDeviceVM>? onItemClickListener = null)
        {
            _bluetoothManager = bluetoothManager ?? throw new ArgumentNullException(nameof(bluetoothManager));
            _itemClickListener = onItemClickListener;
            
            // Subscribe to collection changes
            _bluetoothManager.BluetoothDevices.CollectionChanged += OnBluetoothDevicesCollectionChanged;
        }

        private void OnBluetoothDevicesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // Use a safer approach for UI thread updates
                var activity = Android.App.Application.Context as Android.App.Activity;
                if (activity != null)
                {
                    activity.RunOnUiThread(() => {
                        try 
                        {
                            int count = _bluetoothManager.BluetoothDevices.Count;
                            System.Diagnostics.Debug.WriteLine($"Device collection changed: {count} devices available");
                            NotifyDataSetChanged();
                        }
                        catch (Exception ex) 
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in NotifyDataSetChanged: {ex.Message}");
                        }
                    });
                }
                else
                {
                    // Fallback to Handler approach
                    if (Looper.MainLooper != null && Looper.MainLooper != Looper.MyLooper())
                    {
                        new Handler(Looper.MainLooper).Post(() => {
                            try
                            {
                                NotifyDataSetChanged();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Handler error: {ex.Message}");
                            }
                        });
                    }
                    else
                    {
                        // Direct call as last resort
                        NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling collection change: {ex.Message}");
            }
        }

        public override int ItemCount => _bluetoothManager.BluetoothDevices.Count;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the item layout
            View? itemView = LayoutInflater.From(parent.Context)?.Inflate(
                Resource.Layout.bluetooth_item, parent, false);
                
            if (itemView == null)
            {
                throw new InvalidOperationException("Could not inflate bluetooth_item layout");
            }
                
            // Create and return a new view holder
            return new BluetoothDeviceViewHolder(itemView, OnItemClick);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is BluetoothDeviceViewHolder viewHolder && position < _bluetoothManager.BluetoothDevices.Count)
            {
                try
                {
                    BluetoothDeviceVM device = _bluetoothManager.BluetoothDevices[position];
                    viewHolder.Bind(device);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error binding device at position {position}: {ex.Message}");
                }
            }
        }

        private void OnItemClick(int position)
        {
            if (position >= 0 && position < _bluetoothManager.BluetoothDevices.Count)
            {
                _itemClickListener?.Invoke(_bluetoothManager.BluetoothDevices[position]);
            }
        }

        public new void Dispose()
        {
            _bluetoothManager.BluetoothDevices.CollectionChanged -= OnBluetoothDevicesCollectionChanged;
            base.Dispose();
        }
    }

    public class BluetoothDeviceViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _nameTextView;
        private readonly TextView _addressTextView;
        private readonly ImageView _iconImageView;
        private readonly ImageView _signalImageView;
        private readonly ImageView _statusImageView;
        private BluetoothDeviceVM? _device;

        public BluetoothDeviceViewHolder(View itemView, Action<int> clickListener) 
            : base(itemView)
        {
            // Find views
            _nameTextView = itemView.FindViewById<TextView>(Resource.Id.bt_text) 
                ?? throw new InvalidOperationException("Could not find bt_text view");
                
            _addressTextView = itemView.FindViewById<TextView>(Resource.Id.bt_address)
                ?? throw new InvalidOperationException("Could not find bt_address view");
                
            _iconImageView = itemView.FindViewById<ImageView>(Resource.Id.bt_icon)
                ?? throw new InvalidOperationException("Could not find bt_icon view");
                
            _signalImageView = itemView.FindViewById<ImageView>(Resource.Id.bt_signal)
                ?? throw new InvalidOperationException("Could not find bt_signal view");
                
            _statusImageView = itemView.FindViewById<ImageView>(Resource.Id.bt_status)
                ?? throw new InvalidOperationException("Could not find bt_status view");

            // Set up click listener
            itemView.Click += (sender, e) => clickListener(AbsoluteAdapterPosition);
        }

        public void Bind(BluetoothDeviceVM device)
        {
            _device = device;
            
            // Set device name and address
            _nameTextView.Text = device.Name;
            _addressTextView.Text = device.Address ?? "Unknown Address";
            
            // Set icon tint based on connection state
            if (device.Connected)
            {
                _statusImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#2196f3")); // device_connected color
                    
                _iconImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#2196f3")); // device_connected color
            }
            else if (device.Connecting)
            {
                _statusImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#ff9800")); // device_connecting color
                    
                _iconImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#ff9800")); // device_connecting color
            }
            else
            {
                _statusImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#4caf50")); // device_available color
                
                _iconImageView.SetColorFilter(
                    Android.Graphics.Color.ParseColor("#0277bd")); // primaryColor
            }
        }
    }
} 