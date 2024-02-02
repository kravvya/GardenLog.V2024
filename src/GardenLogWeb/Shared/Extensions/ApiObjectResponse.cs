namespace GardenLogWeb.Shared.Extensions;

public class ApiObjectResponse<T> : ApiResponse
{
	public T? Response { get; internal set; }

}
