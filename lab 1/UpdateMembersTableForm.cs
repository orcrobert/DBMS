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
    public partial class UpdateMembersTableForm : Form
    {
        private string operation;
        private string name, instrument;
        private int memberId, bandId;

        SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Robert\\Documents\\Metal.mdf;Integrated Security=True;Connect Timeout=30;");

        public UpdateMembersTableForm(string operation, int bandId)
        {
            InitializeComponent();
            this.operation = operation;
            this.bandId = bandId;
            this.UpdateButtonFormText();
        }

        public UpdateMembersTableForm(string operation, int memberId, int bandId, string name, string instrument)
        {
            InitializeComponent();

            this.memberId = memberId;
            this.bandId = bandId;
            this.name = name;
            this.instrument = instrument;
            this.memberId = memberId;
            this.operation = operation;
            this.UpdateButtonFormText();
        }

        private void UpdateButtonFormText()
        {
            if (this.operation == "add")
            {
                updateButton.Text = "Add member";
                this.Text = "Add Member";
            }
            else if (this.operation == "update")
            {
                updateButton.Text = "Update member";
                this.Text = "Update member";
                this.nameTextBox.Text = this.name;
                this.instrumentTextBox.Text = this.instrument;
            }
            else if (this.operation == "delete")
            {
                updateButton.Text = "Delete member";
                this.Text = "Delete member";
                this.nameTextBox.Text = this.name;
                this.instrumentTextBox.Text = this.instrument;
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            string query = "";
            string successfullMessage = "";

            if (this.operation == "add")
            {
                query = "insert into Members (BandId, MemberName, MemberInstrument) values (@BandId, @MemberName, @MemberInstrument)";
                successfullMessage = "Member added successfully!";
            }
            else if (this.operation == "delete")
            {
                query = "delete from Members where BandId = @BandId and MemberId = @MemberId";
                successfullMessage = "Member deleted successfully!";
            }
            else if (this.operation == "update")
            {
                query = "update Members set MemberName = @MemberName, MemberInstrument = @MemberInstrument where MemberId = @MemberId";
                successfullMessage = "Member updated successfully!";
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MemberId", this.memberId);
            cmd.Parameters.AddWithValue("@BandId", this.bandId);
            cmd.Parameters.AddWithValue("@MemberName", nameTextBox.Text);
            cmd.Parameters.AddWithValue("@MemberInstrument", instrumentTextBox.Text);

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