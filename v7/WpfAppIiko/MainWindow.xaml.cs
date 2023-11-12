using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
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

namespace WpfAppIiko
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly NamePipeClient client_ = new NamePipeClient();
        public MainWindow()
        {
            InitializeComponent();

            // Запускаем новую задачу на прослушку
            Task.Factory.StartNew(() => { Start(); });
        }

        public void Start()
        {
            client_.Connect(this);
        }

        public void AddTable(RTable? item)
        {
            if (item == null)
            {
                return;
            }
            bool find = false;
            foreach (ListViewItem itm in TablesList.Items)
            {
                this.Dispatcher.Invoke(() =>
                {
                    find = ((Guid)itm.Tag == item.id);
                });
                if (find)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        itm.Background = item.GetColor();
                    });
                    return;
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                ListViewItem itm = new ListViewItem();
                itm.Tag = item.id;
                itm.Content = item.id.ToString();
                itm.Background = item.GetColor();
                TablesList.Items.Add(itm);
                itm.MouseDoubleClick += client_.OnItemClick;
            });

        }
    }
}
