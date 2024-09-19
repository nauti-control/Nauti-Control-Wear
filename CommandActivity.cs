using Android;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear
{
    [Activity(Label = "@string/command_activity")]
    public class CommandActivity : Activity
    {
        CommandActivityVM? _vm;
        CommandAdapter? _mainMenuAdapter;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.command_menu);
            SetupVM();
     
        }



        /// <summary>
        /// Setup ViewModel 
        /// </summary>
        private void SetupVM()
        {
            _vm = new CommandActivityVM();


            WearableRecyclerView? recyclerView = FindViewById<WearableRecyclerView>(Resource.Id.command_menu);
            if (recyclerView != null)
            {
                recyclerView.EdgeItemsCenteringEnabled = true;
                _mainMenuAdapter = new CommandAdapter(_vm.CommandMenuVM);
                _mainMenuAdapter.ItemClick += _vm.CommandMenuVM.OnItemClick;
                recyclerView.SetAdapter(_mainMenuAdapter);
                recyclerView.SetLayoutManager(new WearableLinearLayoutManager(this));
            }



        }
    }
}
