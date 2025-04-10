using Android.Animation;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.Models;
using Nauti_Control_Wear.ViewController;
using Nauti_Control_Wear.ViewModels;
using Nauti_Control_Wear.Views;

namespace Nauti_Control_Wear;

[Activity(Label = "Nautical Control")]
public class CombinedControlActivity : Activity, IDataDisplayVC
{
    // ViewPager for swiping between views
    private ViewPager2? _viewPager;
    private CombinedPagerAdapter? _pagerAdapter;
    
    // Data display related fields
    private DataDisplayVM? _dataDisplayVM;
    
    // Command related fields
    private CommandActivityVM? _commandVM;
    private CommandAdapter? _commandAdapter;
    private WearableRecyclerView? _commandRecyclerView;

    /// <summary>
    /// Update Data Display
    /// </summary>
    /// <param name="data"></param>
    public void UpdateDataDisplay(BoatData data)
    {
        RunOnUiThread(() =>
        {
            _pagerAdapter?.UpdateGaugeData(data);
        });
    }

    /// <summary>
    /// On Create
    /// </summary>
    /// <param name="savedInstanceState">Saved Instance</param>
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

        SetContentView(Resource.Layout.combined_control);
        InitializeUI();
        SetupViewPager();
        SetupCommandView();
    }

    /// <summary>
    /// Initialize UI components
    /// </summary>
    private void InitializeUI()
    {
        _viewPager = FindViewById<ViewPager2>(Resource.Id.viewPager);
    }

    /// <summary>
    /// Set up the ViewPager for swiping between views
    /// </summary>
    private void SetupViewPager()
    {
        _viewPager = FindViewById<ViewPager2>(Resource.Id.viewPager);
        if (_viewPager == null) return;
        
        _pagerAdapter = new CombinedPagerAdapter(this);
        _viewPager.Adapter = _pagerAdapter;
        
        // Set offscreen page limit to keep adjacent pages in memory
        _viewPager.OffscreenPageLimit = 1;
        
        // Set initial position to command view
        _viewPager.CurrentItem = 0;

        // Initialize data display viewmodel
        _dataDisplayVM = new DataDisplayVM(this);
        
        // Update with current data if connected
        if (BluetoothDeviceVM.ConnectedInstance != null)
        {
            UpdateDataDisplay(BluetoothDeviceVM.ConnectedInstance.Data);
        }
    }

    /// <summary>
    /// Setup Command View
    /// </summary>
    private void SetupCommandView()
    {
        View? commandView = _pagerAdapter?.GetCommandView();
        if (commandView == null) return;

        // Get references to views in the command layout
        _commandRecyclerView = commandView.FindViewById<WearableRecyclerView>(Resource.Id.command_menu);
        
        // Initialize command viewmodel
        _commandVM = new CommandActivityVM();
        
        if (_commandRecyclerView != null && _commandVM.CommandMenuVM != null)
        {
            _commandRecyclerView.EdgeItemsCenteringEnabled = true;
            _commandAdapter = new CommandAdapter(_commandVM.CommandMenuVM);
            _commandAdapter.ItemClick += _commandVM.CommandMenuVM.OnItemClick;
            _commandRecyclerView.SetAdapter(_commandAdapter);
            _commandRecyclerView.SetLayoutManager(new WearableLinearLayoutManager(this));
        }
    }

    /// <summary>
    /// Handle back button to prevent going back to the scan activity
    /// </summary>
    public override void OnBackPressed()
    {
        // Show confirmation dialog instead of going back
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Exit Control");
        builder.SetMessage("Do you want to disconnect and exit?");
        builder.SetPositiveButton("Yes", (sender, args) => {
            // Disconnect Bluetooth if connected
            if (BluetoothDeviceVM.ConnectedInstance != null)
            {
                BluetoothDeviceVM.ConnectedInstance.Disconnect();
            }
            
            // Close the activity
            FinishAffinity();
        });
        builder.SetNegativeButton("No", (sender, args) => {
            // Do nothing, just dismiss the dialog
        });
        
        builder.Create()?.Show();
    }
    
    /// <summary>
    /// Clean up when activity is destroyed
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // Dispose view models
        _dataDisplayVM?.Dispose();
        _commandVM?.Dispose();
        
        // Disconnect Bluetooth if connected
        if (BluetoothDeviceVM.ConnectedInstance != null)
        {
            BluetoothDeviceVM.ConnectedInstance.Disconnect();
        }
        
        // Clean up adapters
        _commandAdapter?.Dispose();
        _pagerAdapter?.Dispose();
    }
} 