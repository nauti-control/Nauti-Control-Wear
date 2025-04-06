using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Android;
using Android.Animation;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.ViewModels;
using System.Threading;
namespace Nauti_Control_Wear;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class BluetoothScanActivity : Activity
{
    /// <summary>
    /// Request Enable BT
    /// </summary>
    private const int REQUEST_ENABLE_BT = 1;
    /// <summary>
    /// Request Permissions
    /// </summary>
    private const int REQUEST_PERMISSIONS = 2;
    /// <summary>
    /// Connection timeout in milliseconds (30 seconds)
    /// </summary>
    private const int CONNECTION_TIMEOUT = 30000;
    /// <summary>
    /// Bluetooth Manager
    /// </summary>
    private BluetoothManagerVM _bluetoothManager = null!;
    /// <summary>
    /// Scan Button
    /// </summary>
    private Button? _scanButton;
    /// <summary>
    /// BT Device Adapter
    /// </summary>
    private BluetoothDeviceAdapter? _btAdapter;
    /// <summary>
    /// Device RecyclerView
    /// </summary>
    private WearableRecyclerView? _deviceList;
    /// <summary>
    /// Connection overlay
    /// </summary>
    private FrameLayout? _connectionOverlay;
    /// <summary>
    /// Connection text
    /// </summary>
    private TextView? _connectionText;
    /// <summary>
    /// Connection timeout handler
    /// </summary>
    private Handler? _timeoutHandler;
    /// <summary>
    /// Flag to track if we're in connection process
    /// </summary>
    private bool _isConnecting;
    /// <summary>
    /// Currently connecting device
    /// </summary>
    private BluetoothDeviceVM? _connectingDevice;

    /// <summary>
    /// On Create
    /// </summary>
    /// <param name="savedInstanceState"></param>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Keep screen on and prevent going to watch face
        if (Window != null)
        {
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.AddFlags(WindowManagerFlags.TurnScreenOn);
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
            Window.AddFlags(WindowManagerFlags.DismissKeyguard);
        }

        // Set content view early to ensure UI is available
        SetContentView(Resource.Layout.bluetooth_scan);

        try
        {
            // Initialize the timeout handler
            _timeoutHandler = new Handler(Looper.MainLooper!);
            
            _bluetoothManager = new BluetoothManagerVM();
            _bluetoothManager.OnScanStatusChanged += BluetoothManager_OnScanStatusChanged;
            _bluetoothManager.OnScanFailedEvent += BluetoothManager_OnScanFailed;

            // Find UI elements
            _deviceList = FindViewById<WearableRecyclerView>(Resource.Id.idbtdevice);
            if (_deviceList == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find device list view");
                DisplayErrorDialog("Error initializing UI");
                return;
            }

            _scanButton = FindViewById<Button>(Resource.Id.idbtstartscan);
            if (_scanButton == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find scan button");
                DisplayErrorDialog("Error initializing UI");
                return;
            }

            _connectionOverlay = FindViewById<FrameLayout>(Resource.Id.connection_overlay);
            _connectionText = FindViewById<TextView>(Resource.Id.connection_text);

            // Set up UI elements
            _scanButton.Click += ScanButton_Click;
            
            // Configure the curved layout for better watch UX
            _deviceList.EdgeItemsCenteringEnabled = true;
            _deviceList.HasFixedSize = true;
            
            // Create a LinearLayoutManager for the RecyclerView
            var layoutManager = new LinearLayoutManager(this);
            _deviceList.SetLayoutManager(layoutManager);
            
            // Set up adapter with our manager and device click handler
            _btAdapter = new BluetoothDeviceAdapter(_bluetoothManager, Device_Click);
            _deviceList.SetAdapter(_btAdapter);
            
            // Debug info
            System.Diagnostics.Debug.WriteLine($"RecyclerView initialized with adapter");
            System.Diagnostics.Debug.WriteLine($"Current device count: {_bluetoothManager.BluetoothDevices.Count}");
            
            // Check if device supports Bluetooth LE
            CheckBluetoothSupport();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnCreate: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            DisplayErrorDialog("Error initializing app");
        }
    }

    /// <summary>
    /// Updates the scan status UI
    /// </summary>
    private void UpdateScanStatus(bool isScanning)
    {
        RunOnUiThread(() => {
            if (_scanButton != null)
            {
                // Update button text and icon
                if (isScanning)
                {
                    _scanButton.Text = "Stop";
                    
                    // Use the spinner animation directly on the button
                    _scanButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.spinner_animation, 0, 0, 0);
                    var buttonDrawable = _scanButton.GetCompoundDrawables()[0];
                    if (buttonDrawable is IAnimatable animation)
                    {
                        animation.Start();
                    }
                }
                else
                {
                    _scanButton.Text = "Scan";
                    
                    // Show static scan icon on button
                    _scanButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.scan_spinner, 0, 0, 0);
                }
            }
        });
    }
    
    /// <summary>
    /// Shows the connection overlay
    /// </summary>
    /// <param name="deviceName">Name of device being connected to</param>
    private void ShowConnectionOverlay(string deviceName)
    {
        RunOnUiThread(() => {
            if (_connectionOverlay != null && _connectionText != null)
            {
                _connectionText.Text = $"Connecting to {deviceName}...";
                
                // Set visibility to visible
                _connectionOverlay.Visibility = ViewStates.Visible;
                
                // Apply fade-in animation
                _connectionOverlay.Alpha = 0f;
                _connectionOverlay.Animate()?.
                    Alpha(1f)
                    .SetDuration(250)
                    .SetInterpolator(new Android.Views.Animations.DecelerateInterpolator())
                    .Start();
            }
        });
    }
    
    /// <summary>
    /// Hides the connection overlay
    /// </summary>
    private void HideConnectionOverlay()
    {
        RunOnUiThread(() => {
            if (_connectionOverlay != null)
            {
                // Apply fade-out animation
                _connectionOverlay.Animate()?
                    .Alpha(0f)
                    .SetDuration(200)
                    .SetInterpolator(new Android.Views.Animations.AccelerateInterpolator())
                    .WithEndAction(new Java.Lang.Runnable(() => {
                        _connectionOverlay.Visibility = ViewStates.Gone;
                    }))
                    .Start();
            }
        });
    }

    /// <summary>
    /// Handle activity result (Bluetooth enable request)
    /// </summary>
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        
        if (requestCode == REQUEST_ENABLE_BT)
        {
            if (resultCode == Result.Ok)
            {
                // Bluetooth was enabled
                try
                {
                    Context? context = this;
                    if (context != null)
                    {
                        Toast? toast = Toast.MakeText(context, "Bluetooth enabled", ToastLength.Short);
                        if (toast != null) 
                        {
                            toast.Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing toast: {ex.Message}");
                }
            }
            else
            {
                // User declined to enable Bluetooth
                DisplayErrorDialog("Bluetooth must be enabled to use this app");
                if (_scanButton != null)
                    _scanButton.Enabled = false;
            }
        }
    }

    private void BluetoothManager_OnScanStatusChanged(object? sender, EventArgs e)
    {
        if (_scanButton == null) return;
        
        UpdateScanStatus(_bluetoothManager.IsScanning);
    }

    /// <summary>
    /// Handle scan failures
    /// </summary>
    private void BluetoothManager_OnScanFailed(object? sender, ScanFailure errorCode)
    {
        RunOnUiThread(() => {
            string errorMessage = GetScanFailureMessage(errorCode);
            DisplayErrorDialog($"Scan failed: {errorMessage}");
            UpdateScanStatus(false);
        });
    }

    /// <summary>
    /// Get human-readable message for scan failure
    /// </summary>
    private string GetScanFailureMessage(ScanFailure errorCode)
    {
        return errorCode switch
        {
            ScanFailure.AlreadyStarted => "Scan already started",
            ScanFailure.ApplicationRegistrationFailed => "Application registration failed",
            ScanFailure.FeatureUnsupported => "Feature not supported",
            ScanFailure.InternalError => "Internal error",
            _ => $"Unknown error ({errorCode})"
        };
    }

    /// <summary>
    /// Device click handler
    /// </summary>
    private void Device_Click(BluetoothDeviceVM device)
    {
        if (device == null || _isConnecting) return;
        
        _bluetoothManager.StopScanning();
        UpdateScanStatus(false);
        
        // Store reference to connecting device
        _connectingDevice = device;
        _isConnecting = true;
        
        // Show connection overlay
        ShowConnectionOverlay(device.Name ?? "device");
        
        try
        {
            // Set up event handlers for connection
            device.OnConnected += Device_OnConnected;
            device.OnDeviceDisonnected += Device_OnDisconnected;
            device.OnConnectionTimeout += Device_OnConnectionTimeout;
            
            // Set UI connection timeout (30 seconds)
            StartConnectionTimeout();
            
            // Connect to the device
            device.Connect();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error connecting to device: {ex.Message}");
            CancelConnectionTimeout();
            CleanupConnectionAttempt();
            HideConnectionOverlay();
            DisplayErrorDialog($"Error connecting to device: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Start connection timeout timer
    /// </summary>
    private void StartConnectionTimeout()
    {
        // Remove any existing timeout
        CancelConnectionTimeout();
        
        // Start new timeout
        _timeoutHandler?.PostDelayed(ConnectionTimeoutRunnable, CONNECTION_TIMEOUT);
        System.Diagnostics.Debug.WriteLine($"Connection timeout started ({CONNECTION_TIMEOUT}ms)");
    }
    
    /// <summary>
    /// Cancel connection timeout timer
    /// </summary>
    private void CancelConnectionTimeout()
    {
        _timeoutHandler?.RemoveCallbacks(ConnectionTimeoutRunnable);
    }
    
    /// <summary>
    /// Connection timeout runnable
    /// </summary>
    private void ConnectionTimeoutRunnable()
    {
        // Only proceed if we're still in connecting state
        if (!_isConnecting) return;
        
        System.Diagnostics.Debug.WriteLine("UI-level connection timeout reached");
        
        // Cleanup connection attempt
        CleanupConnectionAttempt();
        
        // Update UI
        RunOnUiThread(() => {
            HideConnectionOverlay();
            DisplayErrorDialog("Connection timed out. Please try again.");
        });
    }
    
    /// <summary>
    /// Clean up connection attempt (unregister events, reset state)
    /// </summary>
    private void CleanupConnectionAttempt()
    {
        if (_connectingDevice != null)
        {
            // Unregister event handlers
            _connectingDevice.OnConnected -= Device_OnConnected;
            _connectingDevice.OnDeviceDisonnected -= Device_OnDisconnected;
            _connectingDevice.OnConnectionTimeout -= Device_OnConnectionTimeout;
            
            // Disconnect if needed
            try
            {
                _connectingDevice.Disconnect();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disconnecting: {ex.Message}");
            }
            
            _connectingDevice = null;
        }
        
        _isConnecting = false;
    }
    
    /// <summary>
    /// Device connected handler
    /// </summary>
    private void Device_OnConnected(object? sender, EventArgs e)
    {
        // Cancel the timeout since connection was successful
        CancelConnectionTimeout();
        _isConnecting = false;
        
        RunOnUiThread(() => {
            HideConnectionOverlay();
            
            if (sender is BluetoothDeviceVM device)
            {
                Toast.MakeText(this, $"Connected to {device.Name}", ToastLength.Short)?.Show();
                
                // Start the combined control activity
                try 
                {
                    var intent = new Intent(this, typeof(CombinedControlActivity));
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(intent);
                    
                    // Apply a smooth crossfade transition animation using our custom animations
                    #pragma warning disable CA1422 // Suppress warning about deprecated API
                    OverridePendingTransition(Resource.Animation.fade_in, Resource.Animation.fade_out);
                    #pragma warning restore CA1422
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error starting control activity: {ex.Message}");
                    DisplayErrorDialog("Error starting control interface");
                }
            }
        });
    }
    
    /// <summary>
    /// Device disconnected handler
    /// </summary>
    private void Device_OnDisconnected(object? sender, EventArgs e)
    {
        // Cancel the timeout
        CancelConnectionTimeout();
        _isConnecting = false;
        
        RunOnUiThread(() => {
            HideConnectionOverlay();
            
            if (sender is BluetoothDeviceVM device)
            {
                Toast.MakeText(this, $"Disconnected from {device.Name}", ToastLength.Short)?.Show();
            }
        });
    }
    
    /// <summary>
    /// Connection timeout handler
    /// </summary>
    private void Device_OnConnectionTimeout(object? sender, EventArgs e)
    {
        // The device-level timeout happened, so cancel our UI timeout
        CancelConnectionTimeout();
        _isConnecting = false;
        
        RunOnUiThread(() => {
            HideConnectionOverlay();
            
            if (sender is BluetoothDeviceVM device)
            {
                DisplayErrorDialog($"Connection to {device.Name} timed out");
            }
        });
    }

    /// <summary>
    /// Scan button click handler
    /// </summary>
    private void ScanButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_bluetoothManager.IsScanning)
            {
                // Stop scanning
                _bluetoothManager.StopScanning();
                UpdateScanStatus(false);
                System.Diagnostics.Debug.WriteLine("Scan stopped by user");
            }
            else
            {
                // Check permissions first
                if (!CheckPermissions())
                {
                    DisplayPermissionsDialog();
                    return;
                }

                // Start scanning
                _bluetoothManager.StartScanning();
                UpdateScanStatus(true);
                System.Diagnostics.Debug.WriteLine("Scan started by user");
                System.Diagnostics.Debug.WriteLine($"Current device count: {_bluetoothManager.BluetoothDevices.Count}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in scan button click: {ex.Message}");
            DisplayErrorDialog("Error starting scan");
            UpdateScanStatus(false);
        }
    }

    /// <summary>
    /// Check Permissions
    /// </summary>
    /// <returns>boolean response</returns>
    private bool CheckPermissions()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+
        {
            #pragma warning disable CA1416 // Platform compatibility
            return ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothScan) == Permission.Granted
                && ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) == Permission.Granted;
            #pragma warning restore CA1416
        }
        
        return ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted;
    }

    /// <summary>
    /// Display Permissions Dialog
    /// </summary>
    private void DisplayErrorDialog(string error)
    {
        var alertDiag = new AlertDialog.Builder(this);
        alertDiag.SetTitle("Error");
        alertDiag.SetMessage(error);
        alertDiag.SetNegativeButton("OK", (senderAlert, args) => alertDiag.Dispose());
        
        var diag = alertDiag.Create();
        diag?.Show();
    }

    /// <summary>
    /// Display Permissions Dialog
    /// </summary>
    private void DisplayPermissionsDialog()
    {
        AlertDialog.Builder alertDiag = new AlertDialog.Builder(this);
        alertDiag.SetTitle("Bluetooth Permissions");
        
        string message = Build.VERSION.SdkInt >= BuildVersionCodes.S
            ? "This app requires Bluetooth scan and connect permissions to discover and connect to devices."
            : "This app requires location permission to scan for Bluetooth devices (required by Android).";
            
        alertDiag.SetMessage(message);
        alertDiag.SetPositiveButton("Ok", RequestPermissions);
        alertDiag.SetNegativeButton("Cancel", (senderAlert, args) =>
        {
            alertDiag.Dispose();
            if (_scanButton != null)
            {
                _scanButton.Enabled = false;
            }
        });
        Dialog? diag = alertDiag.Create();
        if (diag != null)
        {
            diag.Show();
        }
    }

    /// <summary>
    /// Request Permissions
    /// </summary>
    /// <param name="senderAlert">Sender Alert</param>
    /// <param name="args">Args</param>
    private void RequestPermissions(object? senderAlert, DialogClickEventArgs args)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+
        {
            #pragma warning disable CA1416 // Platform compatibility
            ActivityCompat.RequestPermissions(this, 
                new string[] { 
                    Manifest.Permission.BluetoothScan, 
                    Manifest.Permission.BluetoothConnect 
                }, 
                REQUEST_PERMISSIONS);
            #pragma warning restore CA1416
        }
        else
        {
            ActivityCompat.RequestPermissions(this, 
                new string[] { Manifest.Permission.AccessFineLocation }, 
                REQUEST_PERMISSIONS);
        }
    }

    /// <summary>
    /// On Request Permissions Result
    /// </summary>
    /// <param name="requestCode">Request Code</param>
    /// <param name="permissions">Permissions</param>
    /// <param name="grantResults">Grand Results</param>
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if ((requestCode == REQUEST_PERMISSIONS) && (grantResults.Length > 0))
        {
            bool allPermissionsGranted = true;
            foreach (var result in grantResults)
            {
                if (result != Permission.Granted)
                {
                    allPermissionsGranted = false;
                    break;
                }
            }
            
            if (allPermissionsGranted)
            {
                if (_bluetoothManager != null)
                {
                    _bluetoothManager.BluetoothDevices.Clear();
                    _bluetoothManager.StartScanning();
                }
            }
            else
            {
                // Some permissions were denied
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                DisplayErrorDialog("Required permissions were denied. Some features may not work properly.");
                if (_scanButton != null)
                {
                    _scanButton.Enabled = false;
                }
            }
        }
        else
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
    
    /// <summary>
    /// Clean up when activity is paused
    /// </summary>
    protected override void OnPause()
    {
        base.OnPause();
        if (_bluetoothManager != null && _bluetoothManager.IsScanning)
        {
            _bluetoothManager.StopScanning();
        }
    }
    
    /// <summary>
    /// Clean up when activity is destroyed
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Cancel any pending timeouts
        CancelConnectionTimeout();
        
        if (_bluetoothManager != null)
        {
            _bluetoothManager.OnScanStatusChanged -= BluetoothManager_OnScanStatusChanged;
            _bluetoothManager.OnScanFailedEvent -= BluetoothManager_OnScanFailed;
            _bluetoothManager.StopScanning();
        }
        
        // Unregister event handlers
        if (_scanButton != null)
        {
            _scanButton.Click -= ScanButton_Click;
        }
        
        // Dispose adapter
        _btAdapter?.Dispose();
        
        // Remove any pending callbacks
        _timeoutHandler?.RemoveCallbacksAndMessages(null);
        _timeoutHandler = null;
    }

    /// <summary>
    /// Check if the device supports Bluetooth LE
    /// </summary>
    private void CheckBluetoothSupport()
    {
        // Check if Bluetooth is supported on this device
        BluetoothAdapter? adapter = null;
        try 
        {
            #pragma warning disable CA1422 // Suppress warning about deprecated API
            adapter = BluetoothAdapter.DefaultAdapter;
            #pragma warning restore CA1422
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error accessing BluetoothAdapter: {ex.Message}");
        }

        if (adapter == null)
        {
            UpdateScanStatus(false);
            DisplayErrorDialog("Bluetooth is not available on this device");
            if (_scanButton != null)
                _scanButton.Enabled = false;
            return;
        }

        // Check if Bluetooth LE is supported
        bool isBluetoothLeSupported = false;
        try
        {
            isBluetoothLeSupported = PackageManager?.HasSystemFeature(PackageManager.FeatureBluetoothLe) ?? false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking Bluetooth LE support: {ex.Message}");
        }
        
        if (!isBluetoothLeSupported)
        {
            UpdateScanStatus(false);
            DisplayErrorDialog("Bluetooth LE is not supported on this device");
            if (_scanButton != null)
                _scanButton.Enabled = false;
            return;
        }

        // Check if Bluetooth is enabled
        if (!adapter.IsEnabled)
        {
            UpdateScanStatus(false);
            var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
        }
    }
}