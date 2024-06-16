using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        MainActivityVM? _vm;
        MainMenuAdapter? _mainMenuAdapter;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main_menu);
            SetupVM();
     
            if (CheckPermissions())
            {
                DisplayPermissionsDialog();
            }
            else if (_vm != null) 
            {
                _vm.BluetoothManagerVM.StartScanning();
            }
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
            alertDiag.SetNegativeButton("Cancel", (senderAlert, args) => {
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
                _vm.BluetoothManagerVM.StartScanning();
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        /// <summary>
        /// Setup ViewModel 
        /// </summary>
        private void SetupVM()
        {
            _vm = new MainActivityVM();


            WearableRecyclerView recyclerView = FindViewById<WearableRecyclerView>(Resource.Id.main_menu);
            if (recyclerView != null)
            {
                recyclerView.EdgeItemsCenteringEnabled = true;
                _mainMenuAdapter = new MainMenuAdapter(_vm.MainMenuVM);
                _mainMenuAdapter.ItemClick += _vm.MainMenuVM.OnItemClick;
                recyclerView.SetAdapter(_mainMenuAdapter);
                recyclerView.SetLayoutManager(new WearableLinearLayoutManager(this));
            }



        }
    }
}
