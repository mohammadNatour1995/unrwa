using AdmiUI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace UI.Helpers
{
    public abstract class BaseWebController : Controller
    {
        protected IHttpClientHelper _HttpClientHelper { get; set; }
        protected IHttpContextAccessor _HttpContextAccessor { get; set; }

        protected BaseWebController(IHttpClientHelper httpClientHelper, IHttpContextAccessor httpContextAccessor)
        {
            _HttpClientHelper = httpClientHelper;
            _HttpContextAccessor = httpContextAccessor;
        }
    }
}
