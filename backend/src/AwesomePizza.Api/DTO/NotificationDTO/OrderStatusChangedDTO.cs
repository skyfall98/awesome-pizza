namespace AwesomePizza.Api.DTO.NotificationDTO;

// Small on purpose: the client uses it as a hint and gets the full order from the REST API
public class OrderStatusChangedDTO
{
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
