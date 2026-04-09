using System.Net.Http.Json;
string baseUrl = "https://localhost:7185";

using HttpClient client = new HttpClient();
client.BaseAddress = new Uri(baseUrl);

var calculationData = new
{
    A = 10.5, 
    B = 3.2   
};

Console.WriteLine($"Отправляем данные на сервер: A = {calculationData.A}, B = {calculationData.B}");

try
{
    HttpResponseMessage response = await client.PostAsJsonAsync("api/calculator/calculate", calculationData);

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
        if (result != null && result.ContainsKey("результат"))
        {
            Console.WriteLine($"Результат от сервера: {result["результат"]}");
        }
        else
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ответ сервера: {responseBody}");
        }
    }
    else
    {
        string error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Ошибка сервера ({response.StatusCode}): {error}");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Ошибка подключения к серверу. Убедитесь, что API запущен по адресу {baseUrl}. Детали: {ex.Message}");
}

Console.WriteLine("\nНажмите любую клавишу для выхода...");
Console.ReadKey();