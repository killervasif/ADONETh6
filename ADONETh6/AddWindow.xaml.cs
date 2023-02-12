using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Shapes;

namespace ADONETh6
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        SqlConnection? connection = null;
        DataTable? categories = null;

        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        private int categoryId { get; set; }
        public AddWindow(SqlConnection? sqlConnection, DataTable? categories)
        {
            InitializeComponent();
            DataContext = this;
            connection = sqlConnection;
            this.categories = categories;
            categoryId = -1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Categories.DataContext = categories;
            Categories.DisplayMemberPath = categories?.Columns["Name"]?.ColumnName;
        }

        private void Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Categories.SelectedItem is DataRowView rowView)
            {
                var row = rowView.Row;
                categoryId = Convert.ToInt32(row["Id"]);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void AddProduct()
        {
            try
            {
                connection?.Open();

                var command = connection?.CreateCommand();

                ArgumentNullException.ThrowIfNull(command);

                var tran = connection?.BeginTransaction();

                command.Transaction = tran;

                command.CommandText = "usp_AddProduct";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("name", SqlDbType.NVarChar);
                command.Parameters["name"].Value = ProductName;

                command.Parameters.Add("categoryId", SqlDbType.Int);
                command.Parameters["categoryId"].Value = categoryId;

                command.Parameters.Add("quantity", SqlDbType.SmallInt);
                command.Parameters["quantity"].Value = Quantity;

                command.Parameters.Add("price", SqlDbType.Money);
                command.Parameters["price"].Value = Price;

                command.ExecuteNonQuery();

                tran?.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ProductName))
                builder.Append($"{nameof(ProductName)} is empty\n");

            if (Price <= 0)
                builder.Append($"{nameof(Price)} is below or equal to 0\n");

            if (Quantity < 0)
                builder.Append($"{nameof(Quantity)} is be below 0\n");

            if (categoryId == -1)
                builder.Append($"{nameof(categoryId)} isn't choosen\n");

            if (builder.Length > 0)
            {
                MessageBox.Show(builder.ToString());
                return;
            }

            AddProduct();

            DialogResult = true;
        }
    }
}
