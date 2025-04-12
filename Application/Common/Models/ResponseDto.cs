namespace Application.Common.Models;

public class ResponseDto
{
    public bool IsSuccess { get; set; } = true;
    public string DisplayMessage { get; set; } = "";
    public List<string> ErrorMessage { get; set; } = new List<string>();
    public dynamic Result { get; set; } = new object();
}