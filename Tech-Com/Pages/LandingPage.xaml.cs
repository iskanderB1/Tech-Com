namespace Tech_Com.Pages;

public partial class LandingPage : ContentPage
{
	public LandingPage()
	{
		InitializeComponent();
	}

    private void UserMode(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRTicket());
    }

    private void ServerMode(object sender, EventArgs e)
    {
        Navigation.PushAsync(new UnitTestQRScanner());
    }
}