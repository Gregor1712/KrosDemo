using Microsoft.AspNetCore.Mvc;

namespace KrosDemo.Api.Controllers;

public class BaseController : ControllerBase
{
    protected ObjectResult MissingIfMatch() => Problem(
        statusCode: StatusCodes.Status428PreconditionRequired,
        title: "Missing or invalid If-Match header",
        detail: "Provide the resource's current ETag in the If-Match header to perform this conditional operation.");
}