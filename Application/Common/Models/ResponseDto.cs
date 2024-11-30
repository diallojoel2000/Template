namespace Application.Common.Models;

public class ResponseDto
{
    public ResponseDto()
    {
        ErrorMessage = new List<string>();
    }
    public bool IsSuccess { get; set; } = true;
    public string DisplayMessage { get; set; }
    public List<string> ErrorMessage { get; set; }
    public dynamic Result { get; set; }
}