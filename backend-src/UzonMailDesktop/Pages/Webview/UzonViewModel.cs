using Microsoft.Extensions.Configuration;
using UZonMailDesktop.Pages.Conductors;

namespace UZonMailDesktop.Pages.Webview
{
    class UzonViewModel : RouteScreen
    {
        private string url;

        public string URL
        {
            get => url;
            set
            {
                url = value;
                NotifyOfPropertyChange(() => URL);
            }
        }

        public UzonViewModel(IConfiguration config)
        {
            var url = config.GetSection("Webview2Url").Value!.ToString();
            URL = url??"";
        }
    }
}
