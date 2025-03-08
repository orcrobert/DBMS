using System.Data;
using System.Data.SqlClient;

namespace lab_1
{
    public partial class MainForm : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Robert\\Documents\\Metal.mdf;Integrated Security=True;Connect Timeout=30;");
        SqlDataAdapter da = new SqlDataAdapter();
        DataSet ds = new DataSet();

        private string currentTable = "Bands";

        public MainForm()
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            showBands();
            updateButtonEnablement();
        }

        private void showBandsButton_Click(object sender, EventArgs e)
        {
            showBands();
            currentTable = "Bands";
            updateButtonEnablement();
        }

        private void showBands()
        {
            dataGridView1.DataSource = null;

            string query = "select * from Bands";
            SqlCommand cmd = new SqlCommand(query, conn);

            da.SelectCommand = cmd;
            ds.Clear();
            da.Fill(ds, "Bands");

            dataGridView1.DataSource = ds.Tables["Bands"];
            dataGridView1.Refresh();
        }

        private void showMembersButton_Click(object sender, EventArgs e)
        {
            showMembers();
            currentTable = "Members";
            updateButtonEnablement();
        }

        private void showMembers()
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            var selectedBandId = selectedRow.Cells["BandId"].Value;

            dataGridView1.Columns.Clear();

            string query = "select * from Members where BandId = @BandId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@BandId", selectedBandId);

            da.SelectCommand = cmd;
            ds.Clear();
            da.Fill(ds, "Members");


            dataGridView1.DataSource = ds.Tables["Members"];
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            updateButtonEnablement();
        }

        private void updateButtonEnablement()
        {
            if (currentTable == "Bands")
            {
                addBandButton.Enabled = true;
                updateBandButton.Enabled = false;
                deleteBandButton.Enabled = false;
                showMembersButton.Enabled = false;
                addMemberButton.Enabled = false;
                deleteMemberButton.Enabled = false;
                updateMemberButton.Enabled = false;

                if (dataGridView1.SelectedRows.Count != 0)
                {
                    updateBandButton.Enabled = true;
                    deleteBandButton.Enabled = true;
                    showMembersButton.Enabled = true;
                }
            }
            else if (currentTable == "Members")
            {
                addBandButton.Enabled = false;
                updateBandButton.Enabled = false;
                deleteBandButton.Enabled = false;
                showMembersButton.Enabled = true;
                addMemberButton.Enabled = true;
                deleteMemberButton.Enabled = false;
                updateMemberButton.Enabled = false;

                if (dataGridView1.SelectedRows.Count != 0)
                {
                    addMemberButton.Enabled = true;
                    deleteMemberButton.Enabled = true;
                    updateMemberButton.Enabled = true;
                }
            }
        }

        private void addBandButton_Click(object sender, EventArgs e)
        {
            string operation = "add";
            UpdateBandsTableForm form = new UpdateBandsTableForm(operation);

            form.ShowDialog();
            showBands();
        }

        private void deleteBandButton_Click(object sender, EventArgs e)
        {
            string operation = "delete";

            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandName = selectedRow.Cells[1].Value.ToString();
            var bandGenre = selectedRow.Cells[2].Value.ToString();
            var bandTheme = selectedRow.Cells[3].Value.ToString();

            UpdateBandsTableForm form = new UpdateBandsTableForm(operation, bandId, bandName, bandGenre, bandTheme);

            form.ShowDialog();
            showBands();
        }

        private void updateBandButton_Click(object sender, EventArgs e)
        {
            string operation = "update";
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandName = selectedRow.Cells[1].Value.ToString();
            var bandGenre = selectedRow.Cells[2].Value.ToString();
            var bandTheme = selectedRow.Cells[3].Value.ToString();

            UpdateBandsTableForm form = new UpdateBandsTableForm(operation, bandId, bandName, bandGenre, bandTheme);

            form.ShowDialog();
            showBands();
        }
    }
}