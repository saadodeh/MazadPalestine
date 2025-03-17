using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MzadPalestine.API.Documentation;

public static class NotificationsEndpointsDocumentation
{
    public static void AddNotificationsEndpointsDocumentation(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("notifications", new OpenApiInfo
        {
            Title = "MzadPalestine - Notifications API",
            Version = "v1",
            Description = "API endpoints for managing user notifications in the MzadPalestine auction platform",
            Contact = new OpenApiContact
            {
                Name = "MzadPalestine Support",
                Email = "support@mzadpalestine.com"
            }
        });

        // Example responses
        var exampleResponses = new Dictionary<string, object>
        {
            {
                "GetNotifications200Response", new
                {
                    IsSuccess = true,
                    Data = new
                    {
                        Items = new[]
                        {
                            new
                            {
                                Id = 1,
                                Title = "New Bid Received",
                                Message = "A new bid of $500 has been placed on your auction 'Vintage Watch'",
                                Type = "BidReceived",
                                IsRead = false,
                                CreatedAt = DateTime.UtcNow,
                                ActionUrl = "/auctions/123/bids",
                                ImageUrl = "/images/notifications/bid.png"
                            }
                        },
                        TotalCount = 1,
                        PageNumber = 1,
                        PageSize = 10,
                        TotalPages = 1
                    },
                    Error = (string?)null
                }
            },
            {
                "GetUnreadCount200Response", new
                {
                    IsSuccess = true,
                    Data = 5,
                    Error = (string?)null
                }
            },
            {
                "MarkAsRead200Response", new
                {
                    IsSuccess = true,
                    Data = new { },
                    Error = (string?)null
                }
            },
            {
                "MarkAllAsRead200Response", new
                {
                    IsSuccess = true,
                    Data = 10, // Number of notifications marked as read
                    Error = (string?)null
                }
            },
            {
                "Delete200Response", new
                {
                    IsSuccess = true,
                    Data = new { },
                    Error = (string?)null
                }
            },
            {
                "DeleteAll200Response", new
                {
                    IsSuccess = true,
                    Data = 15, // Number of notifications deleted
                    Error = (string?)null
                }
            },
            {
                "ErrorResponse", new
                {
                    IsSuccess = false,
                    Data = (object?)null,
                    Error = "Error message describing what went wrong"
                }
            }
        };

        // Add examples to operations
        options.RequestBodyFilter<NotificationsRequestExampleFilter>();
        options.OperationFilter<NotificationsResponseExampleFilter>(exampleResponses);
    }
}

public class NotificationsRequestExampleFilter : IRequestBodyFilter
{
    public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
    {
        // Add request examples if needed
    }
}

public class NotificationsResponseExampleFilter : IOperationFilter
{
    private readonly Dictionary<string, object> _examples;

    public NotificationsResponseExampleFilter(Dictionary<string, object> examples)
    {
        _examples = examples;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
        if (controllerName != "Notifications") return;

        // Add response examples based on the operation
        switch (context.ApiDescription.RelativePath)
        {
            case "api/Notifications":
                if (context.ApiDescription.HttpMethod == "GET")
                    AddResponseExample(operation, "200", _examples["GetNotifications200Response"]);
                else if (context.ApiDescription.HttpMethod == "DELETE")
                    AddResponseExample(operation, "200", _examples["DeleteAll200Response"]);
                break;

            case "api/Notifications/unread/count":
                AddResponseExample(operation, "200", _examples["GetUnreadCount200Response"]);
                break;

            case "api/Notifications/{id}/read":
                AddResponseExample(operation, "200", _examples["MarkAsRead200Response"]);
                break;

            case "api/Notifications/read/all":
                AddResponseExample(operation, "200", _examples["MarkAllAsRead200Response"]);
                break;

            case "api/Notifications/{id}":
                AddResponseExample(operation, "200", _examples["Delete200Response"]);
                break;
        }

        // Add error response example for all operations
        AddResponseExample(operation, "400", _examples["ErrorResponse"]);
    }

    private void AddResponseExample(OpenApiOperation operation, string statusCode, object example)
    {
        if (!operation.Responses.ContainsKey(statusCode)) return;

        var response = operation.Responses[statusCode];
        var mediaType = response.Content.FirstOrDefault().Value;
        if (mediaType == null) return;

        mediaType.Example = new Microsoft.OpenApi.Any.OpenApiString(
            System.Text.Json.JsonSerializer.Serialize(
                example,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                }));
    }
}
