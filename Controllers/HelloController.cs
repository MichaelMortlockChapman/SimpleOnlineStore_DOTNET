using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SimpleOnlineStore_Dotnet.Controllers;

[ApiController]
[Route("/v1/[controller]")]
public class HelloController : ControllerBase
{
    public HelloController() { }

    [HttpGet("[action]")]
    public ActionResult<string> HelloWorld()
    {
        return Ok("hello world!");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("[action]")]
    public ActionResult<string> HelloWorldAuth()
    {
        return Ok("hello world!");
    }
}