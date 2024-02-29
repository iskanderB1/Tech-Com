using Tech_Com.Pages;

namespace Tech_Com
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LandingPage());

        }
    }
}
