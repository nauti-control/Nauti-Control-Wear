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
        /// <summary>
        /// VM
        /// </summary>
        CommandActivityVM? _vm;
        /// <summary>
        /// Main Menu Adapter
        /// </summary>
        CommandAdapter? _mainMenuAdapter;
        /// <summary>
        /// On Create
        /// </summary>
        /// <param name="savedInstanceState">Saved Instance</param>
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
