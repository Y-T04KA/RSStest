using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RSStest.Controllers
{
    public class SettingsController : Controller
    {
       /* public IActionResult Index()
        //public string Index()
        {
            return View();
            //return "Settings screen placeholder";
        }*/
       
       public ContentResult Index()
        {
            var feed = Configuration["feeds:feed"];//работает только пока ссылка одна, способа читать произвольное число ссылок я не нашёл
            var updaterate = Configuration["updaterate"];
            var useproxy = Configuration["useproxy"];
            var proxyadress = Configuration["proxyadress"];
            var proxylogin = Configuration["proxylogin"];
            var proxypass = Configuration["proxypass"];
            return Content($"Feeds: {feed} \n" + $"Update Rate: {updaterate} \n" + $"Use Proxy: {useproxy} \n" + $"Proxy: {proxyadress} \n" + $"Proxy login: {proxylogin} \n" + $"Proxy pass: {proxypass} \n");
        }
        
        private readonly IConfiguration Configuration;

        public SettingsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

    }
}
