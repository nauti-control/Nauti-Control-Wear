using Android;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.ViewModels;


namespace Nauti_Control_Wear;

[Activity(Label = "BluetoothScanActivity", MainLauncher = true)]
public class BluetoothScanActivity : Activity
{
    private static int REQUEST_ENABLE_BT = 1;
    private BluetoothManagerVM? _bluetoothManager;
    private Button? _scanButton;
    private BluetoothListAdapter? _btAdapter;

    /// <summary>
    /// On Create
    /// </summary>
    /// <param name="savedInstanceState"></param>
    protected override void OnCreate(Bundle? savedInstanceState)
    {



        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.bluetooth_scan);

        _bluetoothManager = new BluetoothManagerVM();


        ListView? mainList = FindViewById<ListView>(Resource.Id.idbtdevice);
        _scanButton = FindViewById<Button>(Resource.Id.idbtstartscan);
        if (_scanButton != null)
        {
            _scanButton.Click += ScanButton_Click;
        }

        ListView? deviceView = FindViewById<ListView>(Resource.Id.main_menu);
        if (deviceView != null)
        {

            _btAdapter = new BluetoothListAdapter(_bluetoothManager);

            deviceView.Adapter = _btAdapter;

        }

    }

    /// <summary>
    /// Scan Button Click
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event</param>
    private void ScanButton_Click(object? sender, EventArgs e)
    {

        if (CheckPermissions())
        {
            if (_bluetoothManager != null)
            {
                _bluetoothManager.StartScanning();
            }

        }
        else
        {
            DisplayPermissionsDialog();
        }

    }

    /// <summary>
    /// Check Permissions
    /// </summary>
    /// <returns>boolean response</returns>
    private bool CheckPermissions()
    {
        return (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted);

    }


    /// <summary>
    /// Display Permissions Dialog
    /// </summary>
    private void DisplayPermissionsDialog()
    {

        AlertDialog.Builder alertDiag = new AlertDialog.Builder(this);
        alertDiag.SetTitle("Location Permission");
        alertDiag.SetMessage("BT Scan requires this");
        alertDiag.SetPositiveButton("Ok", RequestPermissions);
        alertDiag.SetNegativeButton("Cancel", (senderAlert, args) =>
        {
            alertDiag.Dispose();
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
        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, 1);
    }

    /// <summary>
    /// On Request Permissions Result
    /// </summary>
    /// <param name="requestCode">Request Code</param>
    /// <param name="permissions">Permissions</param>
    /// <param name="grantResults">Grand Results</param>
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if ((requestCode == 1) && (grantResults.Length > 0) && (grantResults[0] == Permission.Granted))
        {
            if (_bluetoothManager != null)
            {
                _bluetoothManager.StartScanning();
            }
        }
        else
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (_scanButton != null)
            {
                _scanButton.Enabled = false;
            }
        }
    }
}