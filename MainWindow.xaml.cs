using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Diagnostics;

namespace DownloadApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckVersion();
        }

        private void DeleteFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    StatusText.Text = $"Folder '{folderPath}' has been deleted.";
                }
                else
                {
                    StatusText.Text = $"Folder '{folderPath}' does not exist.";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error deleting folder: {ex.Message}";
            }
        }

        private void UnarchiveFile(string zipFilePath, string destinationPath)
        {
            try
            {
                StatusText.Text = "กำลังลงไฟล์...";
                ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
                StatusText.Text = "ไฟล์แตกเรียบร้อยแล้ว";

                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                    StatusText.Text += " และไฟล์ ZIP ถูกลบ.";
                }

                CheckVersion();
            }
            catch (Exception ex)
            {
                StatusText.Text = "การแตกไฟล์ล้มเหลว";
            }
        }

        private async void CheckVersion()
        {
            try
            {
                string versionFilePath = @"E:\SteamLibrary\steamapps\common\Limbus Company\LimbusCompany_Data\Lang\version.json";
                string localVersion = "0.0.0";

                if (File.Exists(versionFilePath))
                {
                    string jsonContent = File.ReadAllText(versionFilePath);
                    var json = JObject.Parse(jsonContent);
                    localVersion = json["version"].ToString();

                    if (!IsValidVersion(localVersion))
                    {
                        StatusText.Text = "Invalid version format in version.json";
                        return;
                    }

                    VersionText.Text = $"version: {localVersion}";
                }
                else
                {
                    VersionText.Text = "ยังไม่ได้ติดตั้ง";
                }

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync("https://limbusth-api.onrender.com/api/get_version");
                    var json = JObject.Parse(response);
                    string latestVersion = json["version"].ToString();
                    string downloadUrl = json["download_url"].ToString();

                    ApiVersionText.Text = $"version ล่าสุด {latestVersion} (จาก API)";

                    if (!IsValidVersion(latestVersion))
                    {
                        StatusText.Text = "Invalid version format from API";
                        return;
                    }

                    if (Version.Parse(localVersion) < Version.Parse(latestVersion))
                    {
                        StatusText.Text = "Outdated version";
                    }
                    else if (Version.Parse(localVersion) == Version.Parse(latestVersion))
                    {
                        StatusText.Text = "Latest version";
                    }
                    else
                    {
                        StatusText.Text = "Checking version...";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private async void InstallFileButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Updating...";
            await DownloadAndInstallFiles();
            StatusText.Text = "Updated";
        }

        private async Task DownloadAndInstallFiles()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string folderPath = @"E:\SteamLibrary\steamapps\common\Limbus Company\LimbusCompany_Data\Lang";
                    DeleteFolder(folderPath);

                    StatusText.Text = "กำลังดาวน์โหลดไฟล์...";

                    var response = await client.GetStringAsync("https://limbusth-api.onrender.com/api/get_version");
                    var json = JObject.Parse(response);
                    string downloadUrl = json["download_url"].ToString();

                    string destinationPath = @"E:\SteamLibrary\steamapps\common\Limbus Company\LimbusCompany_Data";

                    var fileBytes = await client.GetByteArrayAsync(downloadUrl);
                    string filePath = Path.Combine(destinationPath, "LimbusTH.zip");

                    await File.WriteAllBytesAsync(filePath, fileBytes);
                    StatusText.Text = "ไฟล์ดาวน์โหลดเสร็จ";

                    UnarchiveFile(filePath, destinationPath);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "การดาวน์โหลดล้มเหลว";
            }
        }

        private bool IsValidVersion(string version)
        {
            return Version.TryParse(version, out _);
        }

        private void OpenLinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = "https://discord.gg/6H9SHXNGjD";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error opening link: {ex.Message}";
            }
        }
    }
}
