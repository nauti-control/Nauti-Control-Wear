using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear
{
    [Activity(Label = "@string/app_name")]
    public class MainActivity : Activity
    {
        MainActivityVM? _vm;
        MainMenuAdapter? _mainMenuAdapter;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main_menu);
            SetupVM();
     
        }



        /// <summary>
        /// Setup ViewModel 
        /// </summary>
        private void SetupVM()
        {
            _vm = new MainActivityVM();


            WearableRecyclerView? recyclerView = FindViewById<WearableRecyclerView>(Resource.Id.main_menu);
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
