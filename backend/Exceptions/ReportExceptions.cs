namespace Conectando.Api.Exceptions;

public class InvalidReportReasonException : ApiException
{
    public InvalidReportReasonException() : base(StatusCodes.Status400BadRequest, "El motivo de la denuncia no es válido.") { }
}

public class ReportDetailsTooLongException : ApiException
{
    public ReportDetailsTooLongException(int max) : base(StatusCodes.Status400BadRequest, $"El detalle no puede superar los {max} caracteres.") { }
}