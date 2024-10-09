using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecMan.Interfaces.BL;

namespace UserAccessManagement.Filters
{
    /// <summary>
    /// This is an action filter to check specific permissions
    /// </summary>
    public class SecurityActionFilter : IActionFilter
    {
        private readonly string _permission;
        private readonly ICurrentUserServices _currentUser;

        /// <summary>
        /// Contructor that takes on argument from DI and Another from attribute args
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="currentUser"></param>
        public SecurityActionFilter(string permission, ICurrentUserServices currentUser)
        {
            _permission = permission;
            _currentUser = currentUser;
        }


        /// <summary>
        /// This Method will be executed once the contoller method is executed
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }


        /// <summary>
        /// This Method Will be Exected before the controller method is executed
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_currentUser?.AppPermissions?
                             .SelectMany(x => x.Permission ?? Enumerable.Empty<string>())
                             .Any(x => x == _permission) == false)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
