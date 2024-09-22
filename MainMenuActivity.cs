using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear;

[Activity(Label = "Main Menu")]
public class MainMenuActivity : Activity
{
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
       


    }
}