using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace MissingReferenceFinder
{
    public partial class MainWindow : Window
    {
        private const string PreferenceFileName = "preferences.xml";

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            Title += Assembly.GetExecutingAssembly().GetName().Version;

            ReadPreferences(viewModel);
            Closing += (sender, args) => SavePreferences(viewModel);
        }

        private void ReadPreferences(MainViewModel mainViewModel)
        {
            if (File.Exists(PreferenceFileName))
            {
                var doc = new XmlDocument();
                doc.Load(PreferenceFileName);
                mainViewModel.DirectoryPath = doc["DirectoryPath"].InnerText;
            }
        }

        private void SavePreferences(MainViewModel mainViewModel)
        {
            var doc = new XmlDocument();
            var elem = doc.CreateElement("DirectoryPath");
            elem.InnerText = mainViewModel.DirectoryPath;
            doc.AppendChild(elem);
            doc.Save(PreferenceFileName);
        }
    }
}
