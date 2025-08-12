using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
// using TodoApi.Data;
using TodoApi.Models;


namespace TodoApi.Infrastructure
{
    /// <summary>
    /// Represents a service result that encapsulates the outcome of an operation.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the operation.</typeparam>
    [ExcludeFromCodeCoverage]
    public class ServiceResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the data returned by the operation.
        /// </summary>
        /// 
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation failed.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets or sets additional error details.
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code associated with the result.
        /// </summary>
        public int StatusCode { get; set; } = 200;

        /// <summary>
        /// Creates a successful service result.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <returns>A successful service result.</returns>
        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Creates a successful service result for creation operations.
        /// </summary>
        /// <param name="data">The created data to return.</param>
        /// <returns>A successful service result with 201 status code.</returns>
        public static ServiceResult<T> Created(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                StatusCode = 201
            };
        }

        /// <summary>
        /// Creates a failed service result.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="errorDetails">Additional error details.</param>
        /// <returns>A failed service result.</returns>
        public static ServiceResult<T> Failure(string error, int statusCode = 400, string? errorDetails = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error,
                ErrorDetails = errorDetails,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Creates a not found service result.
        /// </summary>
        /// <param name="error">The error message.</param>
        /// <returns>A not found service result.</returns>
        public static ServiceResult<T> NotFound(string error)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Error = error,
                StatusCode = 404
            };
        }
    }
}
