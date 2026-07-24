public class GeneralResponse <T>
{
    public required int code { get; set; }
    public required string message { get; set; }
    public T? data {get; set;}
}