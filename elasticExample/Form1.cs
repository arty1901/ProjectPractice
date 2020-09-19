using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elasticExample
{
    public partial class Form1 : Form
    {
        private Elasticsearch elasticsearch;
        private int idCounter = 1;

        public Form1()
        {
            InitializeComponent();
            elasticsearch = new Elasticsearch( "http://localhost:9200", "my_index" );

        }

        private async void addButton_Click( object sender, EventArgs e )
        {
            if ( CheckTextBoxes() )
            {
                MessageBox.Show( "All fields must be filled" );
                return;
            }

            string title = titleTextBox.Text,
                author = authorTextBox.Text,
                format = formatTextBox.Text;

            int price = int.Parse( priceTextBox.Text );

            BookModel book = new BookModel { Id = idCounter, Author = author, Title = title, Format = format, Price = price };

            Result result = await elasticsearch.IndexBook( book );

            if ( result == Result.Created )
            {
                MessageBox.Show( $"Book with id={idCounter} created", "Success", MessageBoxButtons.OK );
                idCounter++;
            }
            else
                MessageBox.Show( "Oops...", "Error", MessageBoxButtons.OK );
        }

        private async void deleteButton_Click( object sender, EventArgs e )
        {
            int bookID = int.Parse( deleteBookTextBox.Text );
            Result result = await elasticsearch.DeleteBook(bookID);

            if ( result == Result.Deleted )
                MessageBox.Show( $"Book with id={bookID} deleted", "Success", MessageBoxButtons.OK );
            else
                MessageBox.Show( "Oops...", "Error", MessageBoxButtons.OK );
        }

        private void getButton_Click( object sender, EventArgs e )
        {
            int id = int.Parse( getByIDTextBox.Text );

            BookModel bookModel = elasticsearch.GetById( id );
            ShowResult( bookModel );
        }

        private async void findButton_Click( object sender, EventArgs e )
        {
            string query = findTextBox.Text;

            IReadOnlyCollection<BookModel> result = await elasticsearch.SearchBook( query );

            if ( result.Count == 1 )
                ShowResult( result.First() );
        }

        private void ShowResult( BookModel book )
        {
            StringBuilder builder = new StringBuilder();
            builder.Append( $"Title: {book.Title}\r\n" );
            builder.Append( $"Author: {book.Author}\r\n" );
            builder.Append( $"Price: {book.Price}\r\n" );
            builder.Append( $"Format: {book.Format}\r\n" );

            MessageBox.Show( builder.ToString(), "Result", MessageBoxButtons.OK );
        }

        private bool CheckTextBoxes()
        {
            return string.IsNullOrEmpty( titleTextBox.Text ) ||
                string.IsNullOrEmpty( authorTextBox.Text ) ||
                string.IsNullOrEmpty( priceTextBox.Text ) ||
                string.IsNullOrEmpty( formatTextBox.Text );
        }
    }
}
