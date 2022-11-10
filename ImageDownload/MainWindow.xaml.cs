using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using Flurl.Http;

using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace ImageDownload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly IWebDriver _webDriver;

        public MainWindow()
        {
            InitializeComponent();
            var options = new EdgeOptions();
            options.AddArgument("headless");
            _webDriver = new EdgeDriver(options);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 导航到输入网址: https://item.jd.com/100021890778.html
            _webDriver.Navigate().GoToUrl(ItemUrlTxt.Text);     
            
            var dir = FileNameTxt.Text;
            // 获取主要图元素
            var masterImages = _webDriver.FindElements(By.CssSelector("#spec-list > ul> li > img"));
            // 获取详情元素.
            var detailImages = _webDriver.FindElements(By.CssSelector("#J-detail-content > div.ssd-module-wrap > div"));
            // 下载主要图片
            Task.Run(async () =>
            {
                for (int i = 0; i < masterImages.Count; i++)
                {
                    var url = $"{masterImages[i].GetAttribute("src")}".Replace("n5/jfs", "n12/jfs");
                    url = url.StartsWith("https:") ? url : $"https:{url}";
                    var path = await url
                        .DownloadFileAsync(dir, $"主图-{i:000}.jpg");
                }
            });
            // 详情页图片在div的background-image里.
            Regex backgroundImageSrcRegex = new Regex("\".*?\"");
            // 下载详情页图片
            Task.Run(async () =>
            {
                for (int i = 0; i < detailImages.Count; i++)
                {
                    var background = $"{detailImages[i].GetCssValue("background-image")}";
                    var url = backgroundImageSrcRegex.Match(background).Value.Trim('"');
                    url = url.StartsWith("https:") ? url : $"https:{url}";
                    var path = await url
                        .DownloadFileAsync(dir, $"{dir}-{i:00}.jpg");
                }
            });

        }
    }
}
