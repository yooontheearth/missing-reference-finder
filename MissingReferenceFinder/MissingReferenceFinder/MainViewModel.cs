using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MissingReferenceFinder
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int _progressValue;
        private int _progressMaxValue;
        private string _directoryPath;
        private string _statusText;
        private List<AssemblyInfoListItem> _assemblyList;
        private List<AssemblyInfoListItem> _missingAssemblyList;

        public MainViewModel()
        {
            CheckCommand = new DelegateCommand(t =>
            {
                try
                {
                    StatusText = "Checking...";
                    var asmList = Directory.GetFiles(DirectoryPath)
                                           .Where(f => f.EndsWith(".dll"))
                                           .OrderBy(f => f).ToList();
                    ProgressValue = 0;
                    ProgressMaxValue = asmList.Count;
                    AssemblyList = asmList.Select(ReadAssemblyFromDirectory).ToList();
                    MissingAssemblyList = AssemblyList.Where(a => a.HasMissingChild).ToList();
                    StatusText = string.Format("Done ({0} dlls)", asmList.Count);
                }
                catch (Exception ex)
                {
                    StatusText = ex.Message;
                }
            });

            DirecotryChooseCommand = new DelegateCommand(t =>
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.ShowNewFolderButton = false;
                dialog.SelectedPath = DirectoryPath;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DirectoryPath = dialog.SelectedPath;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public DelegateCommand CheckCommand { get; set; }
        public DelegateCommand DirecotryChooseCommand { get; set; }
        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged1("ProgressValue");
                System.Windows.Forms.Application.DoEvents();
            }
        }
        public int ProgressMaxValue
        {
            get { return _progressMaxValue; }
            set
            {
                _progressMaxValue = value;
                OnPropertyChanged1("ProgressMaxValue");
            }
        }
        public string DirectoryPath
        {
            get { return _directoryPath; }
            set
            {
                _directoryPath = value;
                OnPropertyChanged1("DirectoryPath");
            }
        }
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                OnPropertyChanged1("StatusText");
            }
        }
        public List<AssemblyInfoListItem> AssemblyList
        {
            get { return _assemblyList; }
            set
            {
                _assemblyList = value;
                OnPropertyChanged1("AssemblyList");
            }
        }
        public List<AssemblyInfoListItem> MissingAssemblyList
        {
            get { return _missingAssemblyList; }
            set
            {
                _missingAssemblyList = value;
                OnPropertyChanged1("MissingAssemblyList");
            }
        }

        protected void OnPropertyChanged1([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private AssemblyInfoListItem ReadAssemblyFromDirectory(string filename)
        {
            var item = new AssemblyInfoListItem();
            try
            {
                var asm = Assembly.LoadFrom(filename);
                item.Name = asm.GetName().Name;
                item.Version = asm.GetName().Version.ToString();
                item.ReferredAssemblies = asm.GetReferencedAssemblies().OrderBy(a => a.Name).Select(ReadReferredAssembly).ToList();
            }
            catch (Exception ex)
            {
                item.HasError = true;
                item.ErrorText = ex.ToString();
            }
            ProgressValue++;
            return item;
        }

        private AssemblyInfoListItem ReadReferredAssembly(AssemblyName referredAsm)
        {
            var referredItem = new AssemblyInfoListItem
            {
                Name = referredAsm.Name,
                Version = referredAsm.Version.ToString()
            };
            try
            {
                // MEMO : Check if it's in GAC
                referredItem.IsInGAC = Assembly.Load(referredAsm.FullName).GlobalAssemblyCache;
            }
            catch
            {
                try
                {
                    // MEMO : Check if it's in the directory
                    var loadedAsm = Assembly.LoadFrom(Path.Combine(DirectoryPath, referredAsm.Name + ".dll"));
                    referredItem.IsMissing = loadedAsm.GetName().Version != referredAsm.Version;
                }
                catch
                {
                    referredItem.IsMissing = true;
                }
            }
            return referredItem;
        }
    }
}
