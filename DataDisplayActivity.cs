using AndroidX.Wear.Widget;
using Nauti_Control_Wear.Adapters;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear;

[Activity(Label = "@string/datadisplay_activity")]
public class DataDisplayActivity : Activity
{

    DataDisplayActivityVM? _vm;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.data_display);
        SetupVM();
    }


    /// <summary>
    /// Setup ViewModel 
    /// </summary>
    private void SetupVM()
    {
        _vm = new DataDisplayActivityVM();


    }
}

