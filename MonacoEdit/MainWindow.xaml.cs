using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Mastersign.WpfCodeEditor;
using Microsoft.Win32;
using Wpf.Ui.Appearance;
using UI = Wpf.Ui.Controls;

namespace MonacoEdit
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UI.FluentWindow
    {
        App App => (App)App.Current;

        public MainWindow()
        {
            if (App.ShowHelp)
            {
                App.ShowHelpDialog().Wait();
                App.Shutdown();
                return;
            }

            InitializeComponent();

            Loaded += (sender, args) =>
            {
                WatchSystemTheme();
                ApplicationThemeManager.ApplySystemTheme();
            };

            menu.Visibility = App.ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void WatchSystemTheme()
        {
            SystemThemeWatcher.Watch(
                this,                                  // Window class
                UI.WindowBackdropType.Mica,
                updateAccents: true                    // Whether to change accents automatically
            );
        }

        private void WindowLoadedHandler(object sender, RoutedEventArgs e)
        {
        }

        private async void EditorReadyHandler(object sender, EventArgs e)
        {
            await LoadJsonSchema(App.InitialSchemaFile, App.InitialSchemaUri);
            await LoadText(textFilename ?? App.InitialTextFile);
        }

        private async Task<bool> LoadJsonSchema(string schemaFile, string schemaUri)
        {
            if (!string.IsNullOrWhiteSpace(schemaFile))
            {
                try
                {
                    await editor.LoadJsonSchema(
                        File.ReadAllText(schemaFile, Encoding.UTF8),
                        schemaUri ?? ("file:///" + schemaFile.Replace('\\', '/')));
                }
                catch (Exception exc)
                {
                    await App.ShowError(
                        "Load Schema",
                        "Loading JSON schema failed.\r\n\r\n" + exc.Message);
                    return false;
                }
                return true;
            }
            return false;
        }

        private string textFilename;

        private async Task<bool> LoadText(string filename)
        {
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    await editor.LoadText(
                        File.ReadAllText(filename, Encoding.UTF8),
                        CodeLanguageFromFilename(filename),
                        Path.GetFileName(filename));
                    textFilename = filename;
                }
                catch (Exception exc)
                {
                    textFilename = null;
                    await App.ShowError(
                        "Load Text",
                        "Loading text from file failed.\r\n\r\n" + exc.Message);
                    return false;
                }
                return true;
            }
            return false;
        }

        private async Task<bool> SaveText(string filename)
        {
            try
            {
                var text = await editor.GetText();
                File.WriteAllText(filename, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                textFilename = filename;
            }
            catch (Exception exc)
            {
                await App.ShowError(
                    "Save Text",
                    "Saving text to file failed.\r\n\r\n" + exc.Message);
                return false;
            }
            return true;
        }

        private CodeLanguage CodeLanguageFromFilename(string filename)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            return ext switch
            {
                ".json" => CodeLanguage.Json,
                ".yaml" or ".yml" => CodeLanguage.Yaml,
                _ => CodeLanguage.Plain,
            };
        }

        private async void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "Code Files (*.json, *.yaml, *.yml)|*.json;*.yaml;*.yml",
            };
            if (dlg.ShowDialog(this) != true) return;
            await LoadText(dlg.FileName);
        }

        private void CanSaveCommandPredicate(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = textFilename is not null;
        }

        private async void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (textFilename is null) return;
            await SaveText(textFilename);
        }

        private void CanRevertCommandPredicate(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = textFilename is not null;
        }

        private async void RevertCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (textFilename is null) return;
            await LoadText(textFilename);
        }
    }
}
