using Microsoft.AspNetCore.Mvc.Filters;

namespace TmsApi.Api.Filters;

public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method; //ADD:  add a specific method get the audio filter


        logger.LogInformation("TMS API call: {Method} {Route}", method, route);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var status = context.HttpContext.Response.StatusCode;
        logger.LogInformation("TMS API response: {StatusCode}", status);
    }
}
