using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;


namespace Nauti_Control_Wear;

[Activity(Label = "BluetoothScanActivity",MainLauncher = true)]
public class BluetoothScanActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.bluetooth_scan);

        if (CheckPermissions())
        {
            DisplayPermissionsDialog();
        }

        ListView? mainList = FindViewById<ListView>(Resource.Id.idbtdevice);

    }


    private bool CheckPermissions()
    {
        return (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted);

    }


    private void DisplayPermissionsDialog()
    {

        AlertDialog.Builder alertDiag = new AlertDialog.Builder(this);
        alertDiag.SetTitle("Permission");
        alertDiag.SetMessage("App requires bluetooth");
        alertDiag.SetPositiveButton("Ok", RequestPermissions);
        alertDiag.SetNegativeButton("Cancel", (senderAlert, args) =>
        {
            alertDiag.Dispose();
        });
        Dialog diag = alertDiag.Create();
        diag.Show();


    }

    private void RequestPermissions(object senderAlert, DialogClickEventArgs args)
    {
        ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.AccessFineLocation }, 1);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if ((requestCode == 1) && (grantResults.Length > 0) && (grantResults[0] == Permission.Granted))
        {
           
        }
        else
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}