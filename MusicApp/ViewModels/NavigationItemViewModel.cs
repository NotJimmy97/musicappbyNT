namespace MusicApp.ViewModels
{
    public class NavigationItemViewModel
    {
        public string Title { get; set; }
        public string ViewKey { get; set; }
        public string IconSymbol { get; set; }
        public string Category { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}

