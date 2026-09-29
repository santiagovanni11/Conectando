namespace Conectando.Api.DTOs;

/// <summary>
/// Envuelve un valor para distinguir "campo no provisto" (HasValue == false,
/// valor por defecto) de un "valor provisto" (HasValue == true), permitiendo
/// limpiar campos con un null explícito en un PATCH.
/// </summary>
public class Optional<T>
{
    public T? Value { get; set; }
    public bool HasValue { get; set; } = false;

    public static Optional<T> Of(T? value)
    {
        var result = new Optional<T>();
        result.Value = value;
        result.HasValue = true;
        return result;
    }
}