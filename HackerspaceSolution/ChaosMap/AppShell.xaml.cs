using ChaosMap.Views; // wichtig für DetailsPage

namespace ChaosMap
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // 🔗 Route registrieren, damit wir später navigieren können
            Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        }
    }
}