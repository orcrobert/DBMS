using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab_1
{
    public partial class UpdateBandsTableForm : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Robert\\Documents\\Metal.mdf;Integrated Security=True;Connect Timeout=30;");
        SqlDataAdapter da = new SqlDataAdapter();

        private string operation;
        private string bandName, bandGenre, bandTheme;
        private int bandId;

        public UpdateBandsTableForm(string operation)
        {
            InitializeComponent();
            this.operation = operation;
            this.UpdateButtonFormText();
        }

        public UpdateBandsTableForm(string operation, int bandId, string bandName, string bandGenre, string bandTheme)
        {
            InitializeComponent();
            this.operation = operation;
            this.bandName = bandName;
            this.bandGenre = bandGenre;
            this.bandTheme = bandTheme;
            this.bandId = bandId;
            this.UpdateButtonFormText();
        }

        private void UpdateButtonFormText()
        {
            if (this.operation == "add")
                addBandButton.Text = "Add Band";
            else if (this.operation == "delete")
            {
                addBandButton.Text = "Delete Band";
                bandNameTb.Text = bandName;
                genreTb.Text = bandGenre;
                themeTb.Text = bandTheme;
                this.Text = "Delete Band";
            }
            else if (this.operation == "update")
            {
                addBandButton.Text = "Update Band";
                bandNameTb.Text = bandName;
                genreTb.Text = bandGenre;
                themeTb.Text = bandTheme;
                this.Text = "Update Band";
            }
        }

        private void deleteFromMembers()
        {
            string query = "delete from Members where BandId = @BandId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@BandId", this.bandId);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting from child table: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void addBandButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(bandNameTb.Text) || string.IsNullOrWhiteSpace(genreTb.Text) || string.IsNullOrWhiteSpace(themeTb.Text))
            {
                MessageBox.Show("Please fill in all the fields.");
                return;
            }

            string query = "";
            string successfullMessage = "";

            if (this.operation == "add")
            {
                query = "insert into Bands (BandName, BandGenre, BandTheme) values (@BandName, @BandGenre, @BandTheme)";
                successfullMessage = "Band added successfully";
            }
            if (this.operation == "delete") 
            { 
                query = "delete from Bands where BandName = @BandName and BandGenre = @BandGenre and BandTheme = @BandTheme";
                successfullMessage = "Band deleted successfully";
                deleteFromMembers();
            }
            if (this.operation == "update")
            {
                query = "update Bands set BandName = @BandName, BandGenre = @BandGenre, BandTheme = @BandTheme where BandId = @BandId";
                successfullMessage = "Band updated successfully";
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@BandName", bandNameTb.Text);
            cmd.Parameters.AddWithValue("@BandGenre", genreTb.Text);
            cmd.Parameters.AddWithValue("@BandTheme", themeTb.Text);

            if (this.operation == "update" || this.operation == "delete")
            {
                cmd.Parameters.AddWithValue("@BandId", this.bandId);
            }

            try
            {
                conn.Open();

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show(successfullMessage);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("There was an error in processing your request.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
