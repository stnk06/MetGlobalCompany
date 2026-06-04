namespace MetGlobalCompany.Application.DTOs;

/// <summary>
/// DTO для передачи распознанных данных от сервиса Dadata в ViewModel.
/// </summary>
public class DadataContractorDto
{
    public string Inn { get; set; } = string.Empty;
    public string Kpp { get; set; } = string.Empty;
    public string Ogrn { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string LegalAddress { get; set; } = string.Empty;
}