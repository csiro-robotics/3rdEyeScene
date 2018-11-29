#if UNITY_STANDALONE_WIN

using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Ookii.Dialogs;

namespace SFB {
    // For fullscreen support
    // - WindowWrapper class and GetActiveWindow() are required for modal file dialog.
    // - "PlayerSettings/Visible In Background" should be enabled, otherwise when file dialog opened app window minimizes automatically.

    public class WindowWrapper : IWin32Window {
        private IntPtr _hwnd;
        public WindowWrapper(IntPtr handle) { _hwnd = handle; }
        public IntPtr Handle { get { return _hwnd; } }
    }

    public class StandaloneFileBrowserWindows : IStandaloneFileBrowser {
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return OpenFilePanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Extensions = extensions,
                Multiselect = multiselect
            });
        }

        public string[] OpenFilePanel(BrowserParameters args) {
            var fd = new VistaOpenFileDialog();
            fd.Title = args.Title;
            if (args.Extensions != null) {
                fd.Filter = GetFilterFromFileExtensionList(args.Extensions);
                fd.FilterIndex = (args.FilterIndex > 0) ? args.FilterIndex : 1;
            }
            else {
                fd.Filter = string.Empty;
            }
            fd.Multiselect = args.Multiselect;
            if (!string.IsNullOrEmpty(args.Directory)) {
                fd.FileName = GetDirectoryPath(args.Directory);
            }
            var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
            var filenames = res == DialogResult.OK ? fd.FileNames : new string[0];
            fd.Dispose();
            return filenames;
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public void OpenFilePanelAsync(BrowserParameters args, Action<string[]> cb) {
            cb.Invoke(OpenFilePanel(args));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return OpenFolderPanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                Multiselect = multiselect
            });
        }

        public string[] OpenFolderPanel(BrowserParameters args) {
            var fd = new VistaFolderBrowserDialog();
            fd.Description = args.Title;
            if (!string.IsNullOrEmpty(args.Directory)) {
                fd.SelectedPath = GetDirectoryPath(args.Directory);
            }
            var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
            var filenames = res == DialogResult.OK ? new []{ fd.SelectedPath } : new string[0];
            fd.Dispose();
            return filenames;
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            cb.Invoke(OpenFolderPanel(title, directory, multiselect));
        }

        public void OpenFolderPanelAsync(BrowserParameters args, Action<string[]> cb) {
            cb.Invoke(OpenFolderPanel(args));
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            return SaveFilePanel(new BrowserParameters() {
                Title = title,
                Directory = directory,
                DefaultName = defaultName,
                Extensions = extensions,
                AddExtension = true
            });
        }

        public string SaveFilePanel(BrowserParameters args) {
            var fd = new VistaSaveFileDialog();
            fd.Title = args.Title;

            var finalFilename = "";

            if (!string.IsNullOrEmpty(args.Directory)) {
                finalFilename = GetDirectoryPath(args.Directory);
            }

            if (!string.IsNullOrEmpty(args.DefaultName)) {
                finalFilename += args.DefaultName;
            }

            fd.FileName = finalFilename;
            if (args.Extensions != null) {
                fd.Filter = GetFilterFromFileExtensionList(args.Extensions);
                fd.FilterIndex = (args.FilterIndex > 0) ? args.FilterIndex : 1;
                if (!string.IsNullOrEmpty(args.DefaultExt)) {
                    fd.DefaultExt = args.DefaultExt;
                }
                else {
                    fd.DefaultExt = args.Extensions[0].Extensions[0];
                }
                fd.AddExtension = args.AddExtension;
            }
            else {
                fd.DefaultExt = string.Empty;
                fd.Filter = string.Empty;
                fd.AddExtension = false;
            }
            var res = fd.ShowDialog(new WindowWrapper(GetActiveWindow()));
            var filename = res == DialogResult.OK ? fd.FileName : "";
            fd.Dispose();
            return filename;
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        public void SaveFilePanelAsync(BrowserParameters args, Action<string> cb) {
            cb.Invoke(SaveFilePanel(args));
        }

        // .NET Framework FileDialog Filter format
        // https://msdn.microsoft.com/en-us/library/microsoft.win32.filedialog.filter
        private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions) {
            var filterString = "";
            foreach (var filter in extensions) {
                filterString += filter.Name + "(";

                foreach (var ext in filter.Extensions) {
                    filterString += "*." + ext + ",";
                }

                filterString = filterString.Remove(filterString.Length - 1);
                filterString += ") |";

                foreach (var ext in filter.Extensions) {
                    filterString += "*." + ext + "; ";
                }

                filterString += "|";
            }
            filterString = filterString.Remove(filterString.Length - 1);
            return filterString;
        }

        private static string GetDirectoryPath(string directory) {
            var directoryPath = Path.GetFullPath(directory);
            if (!directoryPath.EndsWith("\\")) {
                directoryPath += "\\";
            }
            return Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar;
        }
    }
}

#endif