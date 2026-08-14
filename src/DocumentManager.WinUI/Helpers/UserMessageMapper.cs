namespace DocumentManager.WinUI.Helpers;

public static class UserMessageMapper
{
    public static string FromException(Exception exception) => exception switch
    {
        OperationCanceledException => "La operación fue cancelada.",
        FileNotFoundException => "No se encontró el archivo. Comprueba que no haya sido movido o eliminado.",
        UnauthorizedAccessException => "Windows no permitió acceder al archivo o carpeta seleccionada.",
        InvalidDataException => "El documento no es válido, está dañado o no puede procesarse.",
        IOException => "No fue posible leer o guardar el archivo. Comprueba la carpeta y vuelve a intentarlo.",
        InvalidOperationException when !string.IsNullOrWhiteSpace(exception.Message) => exception.Message,
        _ => "Ocurrió un problema inesperado. Vuelve a intentarlo; si continúa, reinicia la aplicación.",
    };
}

