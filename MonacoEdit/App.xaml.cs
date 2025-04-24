using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;
using UI = Wpf.Ui.Controls;

namespace MonacoEdit
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public bool ShowHelp { private set; get; } = false;

        public bool ShowOpen { private set; get; } = false;

        public bool ShowSave { private set; get; } = true;

        public bool ShowRevert { private set; get; } = true;

        public bool SaveOnClose { private set; get; } = false;

        public bool ShowToolbar => ShowOpen || ShowSave;

        public string InitialSchemaFile { private set; get; } = null;

        public string InitialSchemaUri { private set; get; } = null;

        public string InitialTextFile { private set; get; } = null;

        public string SecondaryTitle { private set; get; } = null;

        public string WindowTitle { private set; get; } = Assembly.GetExecutingAssembly().GetName().Name;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            ParseCommandLineArguments(args);
        }

        private void ParseCommandLineArguments(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var hasMore = i < args.Length - 1;
                switch (args[i])
                {
                    case "--help":
                    case "-h":
                    case "/?":
                        ShowHelp = true;
                        break;
                    case "--window-title":
                    case "-w":
                        if (!hasMore) break;
                        i++;
                        WindowTitle = args[i];
                        break;
                    case "--title":
                    case "-t":
                        if (!hasMore) break;
                        i++;
                        SecondaryTitle = args[i];
                        break;
                    case "--json-schema":
                    case "-j":
                        if (!hasMore) break;
                        i++;
                        InitialSchemaFile = Path.GetFullPath(args[i]);
                        break;
                    case "--json-schema-uri":
                    case "-u":
                        if (!hasMore) break;
                        i++;
                        InitialSchemaUri = args[i];
                        break;
                    case "--allow-open":
                    case "-o":
                        ShowOpen = true;
                        break;
                    case "--no-save":
                        ShowSave = false;
                        break;
                    case "--no-revert":
                        ShowRevert = false;
                        break;
                    case "--save-on-close":
                    case "-a":
                        SaveOnClose = true;
                        break;
                    default:
                        InitialTextFile = Path.GetFullPath(args[i]);
                        break;
                }
            }
        }

        public async Task ShowHelpDialog()
        {
            ApplicationThemeManager.ApplySystemTheme();
            var dlg = new UI.MessageBox
            {
                Title = WindowTitle + " - Command Line Help",
                MaxWidth = 800,
                Content = new TextBlock
                {
                    Margin = new Thickness(10),
                    Text = """
                    Usage: MonacoEdit.exe [Options] <filename>

                    Options:

                    --help, -h, /?
                        Show this help text.
                    -w, --window-title <title>
                        Set custom window title.
                    -t, --title <file title>
                        Set file title.
                    -j, --json-schema <filename>
                        Path to a JSON schema file.
                    -u, --json-schema-uri <uri>
                        Set the URI for the JSON schema.
                        The schema name is extracted from the last part of the path of the URI.
                    -o, --allow-open
                        Show button for opening another file.
                    --no-revert
                        Do not allow reverting to the last saved state.
                    --no-save
                        Do not allow saving the file manually.
                    -a, --save-on-close
                        Automatically save the file when closing the window.
                    """,
                },
                CloseButtonAppearance = UI.ControlAppearance.Primary,
            };
            await dlg.ShowDialogAsync();
        }

        public async Task ShowError(string title, string message)
        {
            ApplicationThemeManager.ApplySystemTheme();
            var dlg = new UI.MessageBox
            {
                Title = WindowTitle + " - " + title,
                MaxWidth = 800,
                Content = new TextBlock
                {
                    Margin = new Thickness(10),
                    Text = message,
                },
                CloseButtonAppearance = UI.ControlAppearance.Primary,
            };
            await dlg.ShowDialogAsync();
        }
    }
}
