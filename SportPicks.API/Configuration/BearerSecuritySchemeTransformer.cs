using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace SportPicks.API.Configuration;

/// <summary>
/// Transformer to add JWT Bearer authentication to OpenAPI specification for Scalar UI
/// Only applies security requirements to endpoints that actually require authorization
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Add JWT Bearer security scheme definition
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Scheme = "bearer",
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT Bearer token authentication. Enter your token without the 'Bearer' prefix.",
            Reference = new OpenApiReference
            {
                Id = "Bearer",
                Type = ReferenceType.SecurityScheme
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = jwtSecurityScheme;

        // Create security requirement for Bearer auth
        var bearerAuthRequirement = new OpenApiSecurityRequirement
        {
            {
                jwtSecurityScheme,
                Array.Empty<string>()
            }
        };

        // Only apply security to operations that actually require authorization
        foreach (var pathItem in document.Paths.Values)
        {
            foreach (var operation in pathItem.Operations.Values)
            {
                // Check if operation has any authorization requirements
                if (HasAuthorizationAttribute(operation, context))
                {
                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(bearerAuthRequirement);
                }
                else
                {
                    // Ensure public endpoints don't have security requirements
                    operation.Security?.Clear();
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if an operation requires authorization based on controller/action attributes
    /// </summary>
    private static bool HasAuthorizationAttribute(OpenApiOperation operation, OpenApiDocumentTransformerContext context)
    {
        // Get the action method and controller type from the operation
        if (operation.Extensions.TryGetValue("x-aspnetcore-operation", out var operationExt))
        {
            if (operationExt is Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription)
            {
                var actionDescriptor = apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
                
                if (actionDescriptor != null)
                {
                    // Check if action has [Authorize] attribute
                    if (actionDescriptor.MethodInfo.GetCustomAttribute<AuthorizeAttribute>() != null)
                        return true;

                    // Check if action has [AllowAnonymous] attribute (overrides controller-level [Authorize])
                    if (actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                        return false;

                    // Check if controller has [Authorize] attribute
                    if (actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AuthorizeAttribute>() != null)
                        return true;
                }
            }
        }

        return false;
    }
}