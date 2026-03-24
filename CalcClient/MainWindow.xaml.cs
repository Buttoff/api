using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using CalcClient.Models;

namespace CalcClient;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7049") // Ваш URL из браузера
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    // Обработчик кнопки "Рассчитать"
    private async void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Считываем данные
            if (!double.TryParse(TextBoxA.Text, out double a) ||
                !double.TryParse(TextBoxB.Text, out double b))
            {
                MessageBox.Show("Введите корректные числа!");
                return;
            }

            // Создаем запрос
            var request = new CalcRequest { A = a, B = b };

            // Отправляем POST запрос на /api/calculator/calculate
            var response = await _httpClient.PostAsJsonAsync("api/calculator/calculate", request);

            // Проверяем успешность
            if (response.IsSuccessStatusCode)
            {
                // Читаем результат (число)
                var result = await response.Content.ReadAsStringAsync();
                ResultTextBox.Text = result;
            }
            else
            {
                MessageBox.Show($"Ошибка сервера: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения: {ex.Message}. Убедитесь, что сервер запущен.");
        }
    }

    // Обработчик кнопки "Показать историю"
    private async void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Показываем прогресс-бар
            LoadingProgressBar.Visibility = Visibility.Visible;
            HistoryButton.IsEnabled = false;

            // Отправляем GET запрос на /api/calculator
            var history = await _httpClient.GetFromJsonAsync<List<Calculation>>("api/calculator");

            var displayItems = new List<dynamic>();
            foreach (var item in history)
            {
                displayItems.Add(new
                {
                    DisplayText = $"{item.A} + {item.B} = {item.Result} ({item.CreatedAt:dd.MM.yy HH:mm})"
                });
            }

            // Присваиваем список источнику данных
            HistoryListBox.ItemsSource = displayItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка получения истории: {ex.Message}");
        }
        finally
        {
            // Прячем прогресс-бар
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            HistoryButton.IsEnabled = true;
        }
    }
    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(DeleteIdTextBox.Text, out int id))
        {
            MessageBox.Show("Введите корректный ID");
            return;
        }

        var response = await _httpClient.DeleteAsync($"api/calculator/{id}");
        if (response.IsSuccessStatusCode)
        {
            MessageBox.Show("Запись удалена");
            // Обновить историю
            HistoryButton_Click(sender, e);
        }
        else
        {
            MessageBox.Show($"Ошибка удаления: {response.StatusCode}");
        }
    }

    // Обработчик быстрого вычисления через GET
    private async void QuickCalcButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(TextBoxA.Text, out double a) ||
            !double.TryParse(TextBoxB.Text, out double b))
        {
            MessageBox.Show("Введите корректные числа!");
            return;
        }

        // Используем маршрут calculate-quick с query-параметрами
        var response = await _httpClient.GetAsync($"api/calculator/calculate-quick?a={a}&b={b}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            QuickResultTextBox.Text = result;
        }
        else
        {
            MessageBox.Show($"Ошибка: {response.StatusCode}");
        }
    }
    private async void DtoPostButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(TextBoxA.Text, out double a) ||
            !double.TryParse(TextBoxB.Text, out double b))
        {
            MessageBox.Show("Введите корректные числа!");
            return;
        }

        var request = new { a, b };
        var response = await _httpClient.PostAsJsonAsync("api/calculator/calculate-dto", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            MessageBox.Show($"DTO ответ: {result}", "Результат");
        }
    }

    private async void DtoListButton_Click(object sender, RoutedEventArgs e)
    {
        var dtoList = await _httpClient.GetFromJsonAsync<List<CalculationDto>>("api/calculator/list");

        var displayItems = dtoList.Select(d => new
        {
            DisplayText = $"{d.A} {GetOperationSymbol(d.Operation)} {d.B} = {d.Result}"
        }).ToList();

        HistoryListBox.ItemsSource = displayItems;
    }

    private async void StatsButton_Click(object sender, RoutedEventArgs e)
    {
        var stats = await _httpClient.GetFromJsonAsync<dynamic>("api/calculator/stats");
        MessageBox.Show($"Всего операций: {stats.totalCalculations}\nСредний результат: {stats.averageResult}", "Статистика");
    }

    private string GetOperationSymbol(string operation)
    {
        return operation switch
        {
            "Сложение" => "+",
            "Умножение" => "×",
            "Деление" => "÷",
            "Возведение в степень" => "^",
            _ => "?"
        };
    }
}