using AdmiUI.Helpers;
using Microsoft.AspNetCore.Mvc;
using UI.Helpers;
using System.Diagnostics;
using Domain.Dtos;
using Domain.Dtos.Lookup;

namespace UI.Controllers;

public class HomeController : BaseWebController
{
    private readonly ILogger<HomeController> _logger;
    private readonly string _apiBaseUrl;

   

    public IActionResult Index()
    {
        ViewBag.ApiBaseUrl = _apiBaseUrl;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Builder()
    {
        ViewBag.ApiBaseUrl = _apiBaseUrl;
        return View();
    }

   
    #region Constructor
    public HomeController(IHttpClientHelper httpClientHelper, IHttpContextAccessor httpContextAccessor, ILogger<HomeController> logger, IConfiguration configuration) : base(httpClientHelper, httpContextAccessor)
    {
        _logger = logger;
        _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7239";
    }
    #endregion

    #region Pages
    public async Task<IActionResult> Dashboard()
    {
        if (await _HttpClientHelper.IsAuthenticatedAsync())
            return View();
        return RedirectToAction("Signin", "Account", new { returnUrl = Request.Path + Request.QueryString });
    }
    public IActionResult WhatsNew()
    {
        return View();
    }
    #endregion

    #region API's
    [HttpPost]
    public async Task<BaseResponse<LogsStatisticsDto>> GetLogsStatistics()
        => await _HttpClientHelper.Send<LogsStatisticsDto>(new { }, "api/Exceptions/Statistics", HttpMethod.Get);
    #endregion
}
