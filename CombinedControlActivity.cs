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
    private GaugePagerAdapter? _gaugeAdapter;
    private ViewPager2? _gaugePager;
    private TextView? _windSpeed;
    private TextView? _windAngle;
    private TextView? _depth;
    private TextView? _sog;
    private TextView? _stw;
    private TextView? _hdg;
    
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
            if (_gaugeAdapter != null)
            {
                // Wind gauge (position 0)
                if (_gaugeAdapter.GetGauge(0) is WindGaugeView windGauge)
                {
                    windGauge.UpdateWindData((float)data.AWA, (float)data.AWS);
                }
                
                // Depth gauge (position 1)
                if (_gaugeAdapter.GetGauge(1) is DepthGaugeView depthGauge)
                {
                    depthGauge.UpdateValue((float)data.DPT);
                }
                
                // Speed gauge (position 2)
                if (_gaugeAdapter.GetGauge(2) is SpeedGaugeView speedGauge)
                {
                    // Use SOG (speed over ground) for the gauge
                    speedGauge.UpdateValue((float)data.SOG);
                }
                
                // Compass gauge (position 3)
                if (_gaugeAdapter.GetGauge(3) is CompassGaugeView compassGauge)
                {
                    // Update with heading and course over ground
                    compassGauge.UpdateCompassData((float)data.HDG, (float)data.COG);
                }
            }

            // Update text displays
            if (_windAngle != null) _windAngle.Text = $"{data.AWA:F1}°";
            if (_windSpeed != null) _windSpeed.Text = $"{data.AWS:F1} kts";
            if (_depth != null) _depth.Text = $"{data.DPT:F1} M";
            if (_sog != null) _sog.Text = $"{data.SOG:F1} kts";
            if (_stw != null) _stw.Text = $"{data.STW:F1} kts";
            if (_hdg != null) _hdg.Text = $"{data.HDG:F1}°";
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
        SetupDataDisplay();
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
    }

    /// <summary>
    /// Setup Data Display View
    /// </summary>
    private void SetupDataDisplay()
    {
        View? dataView = _pagerAdapter?.GetDataDisplayView();
        if (dataView == null) return;

        // Get references to views in the data display layout
        _gaugePager = dataView.FindViewById<ViewPager2>(Resource.Id.gauge_pager);
        _windSpeed = dataView.FindViewById<TextView>(Resource.Id.windspeed);
        _windAngle = dataView.FindViewById<TextView>(Resource.Id.windangle);
        _depth = dataView.FindViewById<TextView>(Resource.Id.depth);
        _sog = dataView.FindViewById<TextView>(Resource.Id.sog);
        _stw = dataView.FindViewById<TextView>(Resource.Id.stw);
        _hdg = dataView.FindViewById<TextView>(Resource.Id.hdg);

        if (_gaugePager != null)
        {
            // Setup ViewPager with gauge adapter
            _gaugeAdapter = new GaugePagerAdapter(this);
            _gaugePager.Adapter = _gaugeAdapter;
            
            // Set page transformer for smooth animations
            _gaugePager.SetPageTransformer(new GaugePageTransformer());
            
            // Set offscreen page limit to keep adjacent pages in memory
            _gaugePager.OffscreenPageLimit = 1;
        }

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
        
        // Clean up gauge pager
        if (_gaugePager != null)
        {
            _gaugePager.Adapter = null;
        }
        
        // Clean up adapters
        _gaugeAdapter?.Dispose();
        _commandAdapter?.Dispose();
        _pagerAdapter?.Dispose();
    }
} 