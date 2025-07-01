using QuestHubClient.ViewModels;
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

namespace QuestHubClient.Views
{
    /// <summary>
    /// Lógica de interacción para NewPostView.xaml
    /// </summary>
    public partial class NewPostView : Page
    {
        public NewPostView(NewPostViewModel newPostViewModel)
        {
            InitializeComponent();
            this.DataContext = newPostViewModel;
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (DataContext is NewPostViewModel viewModel)
                    {
                        viewModel.AddFiles(files);
                    }
                }

                // Restaurar estilo normal
                if (sender is Border border)
                {
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"));
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CED4DA"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar archivos: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;

                // Cambiar estilo cuando se arrastra sobre el área
                if (sender is Border border)
                {
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            // Restaurar estilo normal cuando se sale del área
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"));
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CED4DA"));
            }
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }
    }
}
