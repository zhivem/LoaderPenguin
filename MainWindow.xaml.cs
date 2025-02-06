using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace LoaderPenguin
{
    public partial class MainWindow : Window
    {
        private readonly string publicUrl = "https://disk.yandex.ru/d/ckFPOTqcG7XsTg";
        private readonly string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");
        private readonly string extractPath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string mainExe = "DPI Penguin.exe";
        private readonly string internalFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_internal");

        public MainWindow()
        {
            if (!IsRunningAsAdmin())
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                    Verb = "runas" 
                };
                Process.Start(startInfo);
                Application.Current.Shutdown(); 
                return;
            }

            InitializeComponent();
            StartUpdateProcess();
        }

        private bool IsRunningAsAdmin()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private async void StartUpdateProcess()
        {
            try
            {
                StopProcessesAndServices();

                if (File.Exists(Path.Combine(extractPath, mainExe)))
                {
                    File.Delete(Path.Combine(extractPath, mainExe));
                }

                if (Directory.Exists(internalFolder))
                {
                    Directory.Delete(internalFolder, true);
                }

                StatusLabel.Content = "Загрузка обновления...";
                await DownloadFileFromYandexDisk(publicUrl, downloadPath, DownloadProgressBar);

                // Ожидание освобождения файла перед извлечением
                WaitForFileRelease(downloadPath);

                StatusLabel.Content = "Распаковка обновления...";
                await ExtractArchive(downloadPath, extractPath, ExtractProgressBar);

                File.Delete(downloadPath);

                RestartMainApp();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/zhivem/DPI-Penguin/releases",
                    UseShellExecute = true
                });
                Close();
            }
        }

        private void StopProcessesAndServices()
        {
            KillProcess("DPI Penguin");
            KillProcess("winws");
            StopService("WinDivert");
        }

        private void KillProcess(string processName)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке процесса {processName}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopService(string serviceName)
        {
            try
            {
                var startInfo = new ProcessStartInfo("net", $"stop {serviceName}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var process = Process.Start(startInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке службы {serviceName}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DownloadFileFromYandexDisk(string publicUrl, string outputPath, ProgressBar progressBar)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync($"https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key={publicUrl}");
            dynamic jsonResponse = JsonConvert.DeserializeObject(response);
            string downloadUrl = jsonResponse.href;

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                HttpResponseMessage downloadResponse = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                downloadResponse.EnsureSuccessStatusCode();

                Stream contentStream = await downloadResponse.Content.ReadAsStreamAsync();

                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    long totalBytes = downloadResponse.Content.Headers.ContentLength ?? 1;
                    long totalRead = 0;
                    int read;

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalRead += read;
                        int progress = (int)(totalRead * 100 / totalBytes);
                        Dispatcher.Invoke(() => progressBar.Value = progress);
                    }
                } // Файл закрывается автоматически
            }
            else
            {
                throw new Exception("Не удалось получить ссылку на файл для скачивания.");
            }
        }

        private async Task ExtractArchive(string zipFilePath, string destinationPath, ProgressBar progressBar)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
                    {
                        int totalFiles = archive.Entries.Count;
                        int extracted = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string filePath = Path.Combine(destinationPath, entry.FullName);

                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }

                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                                entry.ExtractToFile(filePath, true);
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            extracted++;
                            int progress = (int)((double)extracted / totalFiles * 100);
                            Dispatcher.Invoke(() => progressBar.Value = progress);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show($"Ошибка при извлечении архива: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private void WaitForFileRelease(string filePath)
        {
            int attempts = 10;
            while (attempts > 0)
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return; // Файл доступен
                    }
                }
                catch (IOException)
                {
                    Task.Delay(500).Wait();
                    attempts--;
                }
            }
        }

        private void RestartMainApp()
        {
            string exePath = Path.Combine(extractPath, mainExe);
            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show($"Не удалось найти файл: {exePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
