using Newtonsoft.Json;
using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SQLite.Net;
using SQLite.Net.Attributes;
using System.Threading.Tasks;
using System.IO;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPLocalDatabase
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
       
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btn1_Click(object sender, RoutedEventArgs e)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(settingsFileName) as IStorageFile;

            if (file == null)
            {
                settings = defaultSettings;
                var newFile = await folder.CreateFileAsync(settingsFileName, CreationCollisionOption.ReplaceExisting);
                var text = JsonConvert.SerializeObject(settings);
                await FileIO.WriteTextAsync(newFile, text);
            }
            else // file is not null, so read it
            {
                var text = await FileIO.ReadTextAsync(file);
                settings = JsonConvert.DeserializeObject<Setting[]>(text);
            }

            ShowSettings();
        }

        private async void btn2_Click(object sender, RoutedEventArgs e)
        {
            if (settings == null || settings.Length == 0)
            { settings = defaultSettings; }

            settings[0].Value = DateTime.Now.Second.ToString();
            var folder = ApplicationData.Current.LocalFolder;
            var newFile = await folder.CreateFileAsync(settingsFileName, CreationCollisionOption.ReplaceExisting);
            var text = JsonConvert.SerializeObject(settings);
            await FileIO.WriteTextAsync(newFile, text);

            ShowSettings();
        }

        private void ShowSettings()
        {
            string settingsText = string.Join(", ", settings.Select(setting => $"{ setting.Name } = { setting.Value }"));
            Result.Text = settingsText;
        }

        string settingsFileName = "settings.json";
        Setting[] settings = Array.Empty<Setting>();
        Setting[] defaultSettings = new Setting[]
        {
            new Setting()
            {
                Name = "Last Selected",
                Value = "0"
            },

            new Setting()
            {
                Name = "Preferred Variety",
                Value = "Juliet"
            }
        };

        private async Task<SQLiteConnection> OpenOrRecreateConnectionAsync(bool recreate = false)
        {
            var fileName = "Garden.sqlite";
            var folder = ApplicationData.Current.LocalFolder;

            if (recreate)
            {
                var file = await folder.TryGetItemAsync(fileName);
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }

            var sqlPath = Path.Combine(folder.Path, fileName);

            return new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), sqlPath);
        }

        private async void btn3_Click(object sender, RoutedEventArgs e)
        {
            using (SQLiteConnection conn = await OpenOrRecreateConnectionAsync(true))
            {
                conn.CreateTable<Garden>();
                foreach (var plant in Garden.garden)
                {
                    conn.InsertOrReplace(plant);
                }
            }
        }

        private void btn4_Click(object sender, RoutedEventArgs e)
        {

        }
    }// End partial class MainPage
    public class Setting
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Garden
    {
        public static Plant[] garden = new Plant[]
        {
            new Plant()
            {
                Id = "0",
                Name = "Spinach",
                Category = "Greens"
            },

            new Plant()
            {
                Id = "1",
                Name = "Bell Pepper",
                Category = "Fruit"
            },

            new Plant()
            {
                Id = "2",
                Name = "Carrots",
                Category = "Root"
            }
        };
    }

    public class Plant
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
