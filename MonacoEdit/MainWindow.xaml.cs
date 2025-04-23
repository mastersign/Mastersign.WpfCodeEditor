using System;
using System.Collections.Generic;
using System.Linq;
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

namespace MonacoEdit
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtURL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                editor.Navigate(txtURL.Text);
            }
        }

        private async void EditorReadyHandler(object sender, EventArgs e)
        {
            var schema = """
            {
                type: 'object',
                properties: {
                name: {
                    type: 'string',
                    description: 'The person’s display name'
                },
                age: {
                    type: 'integer',
                    description: 'How old is the person in years?'
                },
                occupation: {
                    enum: ['Delivery person', 'Software engineer', 'Astronaut']
                }
                }
            }
            """;
            var uri = "https://mastersign.de/demo.json";

            await editor.LoadJsonSchema(schema, uri);

            var text = "name: Mr T";

            await editor.LoadText(text, Mastersign.WpfCodeEditor.CodeLanguage.Yaml, "test.yml");
        }
    }
}
