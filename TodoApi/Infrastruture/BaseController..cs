namespace TodoApi.Infrastructure
{
 /// <summary>
    /// Base controller class that provides common functionality for handling service results.
    /// </summary>
    public abstract class BaseController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        /// <summary>
        /// Converts a service result to an appropriate action result.
        /// </summary>
        /// <typeparam name="T">The type of data in the service result.</typeparam>
        /// <param name="result">The service result to convert.</param>
        /// <returns>An action result based on the service result.</returns>
        protected Microsoft.AspNetCore.Mvc.ActionResult<T> ServiceResult<T>(ServiceResult<T> result)
        {
            if (result.IsSuccess)
            {
                return result.StatusCode switch
                {
                    201 => Created("", result.Data),
                    _ => Ok(result.Data)
                };
            }

            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                400 => BadRequest(result.Error),
                _ => StatusCode(result.StatusCode, result.Error)
            };
        }
    }

}