using System.Security.Cryptography;
using System.Text;

namespace Order.Api.Idempotency;

/// <summary>
/// Технический класс для вычисления SHA-256 хэша тела HTTP-запроса.
/// Хэш используется как часть ключа идемпотентности.
/// </summary>
public static class RequestHashCalculator
{
    /// <summary>
    /// Вычисляет SHA-256 хэш от текущего содержимого HTTP Body.
    /// После вычисления позиция потока сбрасывается в начало (Position = 0), чтобы последующие обработчики могли прочитать тело запроса.
    ///
    /// ВАЖНО: Метод не десериализует JSON. Он работает с сырыми байтами, что гарантирует, что идентичные HTTP-запросы (включая пробелы в JSON) получат одинаковый хэш.
    /// </summary>
    /// <param name="httpRequest">Текущий HTTP запрос.</param>
    /// <returns>Строка SHA-256 хэша в hex-формате (64 символа).</returns>
    public static string ComputeHash(HttpRequest httpRequest)
    {
        // Включаем буферизацию тела, чтобы его можно было читать несколько раз
        httpRequest.EnableBuffering();

        // Читаем тело запроса в массив байтов
        using var reader = new StreamReader(httpRequest.Body, Encoding.UTF8, leaveOpen: true);
        var body = reader.ReadToEndAsync().GetAwaiter().GetResult();
        var bytes = Encoding.UTF8.GetBytes(body);

        // Вычисляем SHA-256 хэш
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(bytes);
        var hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();

        // ВАЖНО: Сбрасываем позицию потока, чтобы ASP.NET Core мог десериализовать запрос
        httpRequest.Body.Position = 0;

        return hashHex;
    }
}