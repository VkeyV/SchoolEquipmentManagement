using Microsoft.AspNetCore.Mvc;

namespace SchoolEquipmentManagement.Web.Controllers;

public abstract class AppController : Controller
{
    protected void AddOperationError(Exception exception, string key = "")
    {
        ModelState.AddModelError(key, GetOperationErrorMessage(exception));
    }

    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    protected void SetErrorMessage(Exception exception)
    {
        SetErrorMessage(GetOperationErrorMessage(exception));
    }

    protected static string GetOperationErrorMessage(Exception exception)
    {
        return exception.InnerException?.Message ?? exception.Message;
    }
}
