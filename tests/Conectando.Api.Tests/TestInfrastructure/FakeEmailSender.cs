using Conectando.Api.Interfaces;

namespace Conectando.Api.Tests.TestInfrastructure;

/// <summary>
/// Remitente falso que guarda lo que se habría mandado.
///
/// Existe para que los tests puedan comprobar qué se envía sin abrir una
/// conexión de verdad. Guardar el mensaje entero y no solo un contador es lo
/// que permite afirmar sobre el contenido: que el código va en el cuerpo, o
/// que el asunto menciona la marca.
/// </summary>
public sealed class FakeEmailSender(bool configured = true) : IEmailSender
{
    public List<EmailMessage> Sent { get; } = [];

    public EmailMessage? Last => Sent.LastOrDefault();

    public bool IsConfigured { get; } = configured;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}
