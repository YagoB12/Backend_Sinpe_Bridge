using Backend_Bridge.DTOs;

namespace Backend_Bridge.Services.Interfaces
{
    public interface ISmsParserService
    {
        ParsedSmsDto Parse(string messageBody);
    }
}