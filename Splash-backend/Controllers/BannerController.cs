using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Splash_backend
{
    [Route("banner")]
    public class BannerController
    {
        [HttpGet]
        public RedirectResult Get()
        {
            return new RedirectResult("/banner.jpg");
        }
    }
}