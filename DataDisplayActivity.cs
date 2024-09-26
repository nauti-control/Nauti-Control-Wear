using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.Models;
using Nauti_Control_Wear.ViewController;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear;

[Activity(Label = "@string/datadisplay_activity")]
public class DataDisplayActivity : Activity, IDataDisplayVC
{

    DataDisplayActivityVM? _vm;
    TextView? _windSpeed;
    TextView? _windAngle;
    TextView? _depth;
    TextView? _sog;
    TextView? _stw;
    TextView? _cog;
    TextView? _hdg;


    /// <summary>
    /// Update Data Display
    /// </summary>
    /// <param name="data"></param>
    public void UpdateDataDisplay(BoatData data)
    {
        _windSpeed.Text = string.Format("{0} kts", data.AWS);
        _windAngle.Text = string.Format("{0} °", data.AWA);
        _depth.Text = string.Format("{0} M", data.DPT);
        _sog.Text = string.Format("{0} kts", data.SOG);
        _stw.Text = string.Format("{0} kts", data.STW);
        _cog.Text = string.Format("{0} °", data.COG);
        _hdg.Text = string.Format("{0} °", data.HDG);
    }

    /// <summary>
    /// On Create
    /// </summary>
    /// <param name="savedInstanceState">Saved Instance</param>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.data_display);
        GetControlReferences();
        SetupVM();

    }


    /// <summary>
    /// Setup ViewModel 
    /// </summary>
    private void SetupVM()
    {
        _vm = new DataDisplayActivityVM(this);


    }

    /// <summary>
    /// Gets Control References
    /// </summary>
    private void GetControlReferences()
    {
        _windSpeed = FindViewById<TextView>(Resource.Id.windspeed);
        _windAngle = FindViewById<TextView>(Resource.Id.windangle);
        _depth = FindViewById<TextView>(Resource.Id.depth);
        _sog = FindViewById<TextView>(Resource.Id.sog);
        _stw = FindViewById<TextView>(Resource.Id.stw);
        _cog = FindViewById<TextView>(Resource.Id.cog);
        _hdg = FindViewById<TextView>(Resource.Id.hdg);
    }




}

