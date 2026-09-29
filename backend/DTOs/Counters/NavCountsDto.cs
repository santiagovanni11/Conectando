namespace Conectando.Api.DTOs.Counters;

/// <summary>
/// Cantidades que la navegación muestra como aviso: cuántos mensajes sin
/// leer, cuántas solicitudes pendientes y cuántas notificaciones.
/// </summary>
public class NavCountsDto
{
    public int UnreadMessages { get; init; }
    public int PendingFriendRequests { get; init; }
    public int UnreadNotifications { get; init; }
}