using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MallenomDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7295/api/images"; 

        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient(); 
            LoadImagesAsync(); // Загрузка изображений при запуске
        }

        private void ImagesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            bool isImageSelected = ImagesDataGrid.SelectedItem != null;

           
            UpdateButton.IsEnabled = isImageSelected;
            DeleteButton.IsEnabled = isImageSelected;
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                // Запрос к API для получения всех изображений
                var images = await _httpClient.GetFromJsonAsync<List<ImageModel>>($"{_apiBaseUrl}/all");

               
                var imageViewModels = images.Select(image => new ImageViewModel
                {
                    Id = image.Id,
                    Name = image.Name,
                    ImageSource = LoadImageFromBytes(image.Data) 
                }).ToList();

                
                ImagesDataGrid.ItemsSource = imageViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке изображений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Преобразование byte[] в BitmapImage
        private BitmapImage LoadImageFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var bitmap = new BitmapImage();
            using (var memoryStream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();
            }
            return bitmap;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Добавить изображение",
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = openFileDialog.FileName;
                    var fileBytes = File.ReadAllBytes(filePath);

                    // Отправка изображения в API
                    var content = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                    var response = await _httpClient.PostAsync($"{_apiBaseUrl}/add", content);
                    response.EnsureSuccessStatusCode();

                    //Обновление 
                    await LoadImagesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedImage = ImagesDataGrid.SelectedItem as ImageViewModel;
            if (selectedImage == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Изменить изображение",
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = openFileDialog.FileName;
                    var fileBytes = File.ReadAllBytes(filePath);

                    // Отправка изображения в API
                    var content = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                    var response = await _httpClient.PutAsync($"{_apiBaseUrl}/update/{selectedImage.Id}", content);
                    response.EnsureSuccessStatusCode();

                    //Обновление списка
                    await LoadImagesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

            var selectedImage = ImagesDataGrid.SelectedItem as ImageViewModel;
            if (selectedImage == null) return;

            try
            {
                // Удаление изображения через API
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/delete/{selectedImage.Id}");
                response.EnsureSuccessStatusCode();

                // Обновление
                await LoadImagesAsync();
                MessageBox.Show("Изображение удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
