using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace GenerateESGReport;

public class Function
{
    private readonly IConfiguration _configuration;

    public Function()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        // You can access your configuration settings like this:
        // var exampleSetting = _configuration["ExampleSetting"];
    }
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation($"ESG report generation request received at: {DateTime.UtcNow}");

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = "ESG Report Generate Succesfully!", fileId = "001", accessLink = "link" }),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}
