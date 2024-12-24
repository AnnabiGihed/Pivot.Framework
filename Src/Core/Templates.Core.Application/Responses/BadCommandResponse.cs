namespace Templates.Core.Application.Responses;

public class BaseCommandResponse
{
	public BaseCommandResponse()
	{
		Success = true;
	}
	public BaseCommandResponse(string message) : this()
	{
		Message = message;
	}

	public BaseCommandResponse(string message, bool success)
	{
		Success = success;
		Message = message;
	}

	public bool Success { get; set; }
	public string Message { get; set; } = string.Empty;
	public List<string> ValidationErrors { get; set; } = new();
}
