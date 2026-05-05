namespace TombLauncher.Contracts.Ai;

public class ServiceAvailabilityResponse
{
    private ServiceAvailabilityResponse(){}
    public bool IsReachable { get; set; }
    public string? Error { get; set; }

    public static ServiceAvailabilityResponse AvailableResponse { get; } =
        new ServiceAvailabilityResponse() { IsReachable = true };

    public static ServiceAvailabilityResponse NotAvailableResponse(string error)
    {
        return new ServiceAvailabilityResponse() { Error = error, IsReachable = false };
    }
}