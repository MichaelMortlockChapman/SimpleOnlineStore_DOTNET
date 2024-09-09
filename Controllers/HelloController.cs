using Microsoft.AspNetCore.Mvc;

namespace SimpleOnlineStore_Dotnet.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class HelloController : ControllerBase
{
    public HelloController() { }

    [HttpGet("[action]")]
    public ActionResult<string> HelloWorld()
    {
        return Ok("hello world!");
    }
}