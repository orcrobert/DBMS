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

            string query = "SELECT * FROM Bands";
            SqlCommand cmd = new SqlCommand(query, conn);

            da.SelectCommand = cmd;
            ds.Clear();
            da.Fill(ds, "Bands");

            dataGridView1.DataSource = ds.Tables["Bands"];
            dataGridView1.Refresh();
        }

        private void showMembersButton_Click(object sender, EventArgs e)
        {
            showMembers(-1);
            currentTable = "Members";
            updateButtonEnablement();
        }

        private void showMembers(int tempBandId)
        {
            if (dataGridView1.SelectedRows.Count == 0 && tempBandId == -1)
            {
                MessageBox.Show("No band selected.");
                return;
            }

            int selectedBandId = (tempBandId == -1)
                ? Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value)
                : tempBandId;

            dataGridView1.DataSource = null;

            string query = "SELECT * FROM Members WHERE BandId = @BandId";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@BandId", selectedBandId);

                da.SelectCommand = cmd;
                ds.Clear();
                da.Fill(ds, "Members");

                dataGridView1.DataSource = ds.Tables["Members"];
                dataGridView1.Refresh();
            }
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
                updateBandButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                deleteBandButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                showMembersButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                addMemberButton.Enabled = false;
                deleteMemberButton.Enabled = false;
                updateMemberButton.Enabled = false;
            }
            else if (currentTable == "Members")
            {
                addBandButton.Enabled = false;
                updateBandButton.Enabled = false;
                deleteBandButton.Enabled = false;
                showMembersButton.Enabled = true;
                addMemberButton.Enabled = dataGridView1.Rows.Count > 0;
                deleteMemberButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                updateMemberButton.Enabled = dataGridView1.SelectedRows.Count > 0;
            }
        }

        private void addBandButton_Click(object sender, EventArgs e)
        {
            UpdateBandsTableForm form = new UpdateBandsTableForm("add");
            form.ShowDialog();
            showBands();
        }

        private void deleteBandButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandName = selectedRow.Cells[1].Value.ToString();
            var bandGenre = selectedRow.Cells[2].Value.ToString();
            var bandTheme = selectedRow.Cells[3].Value.ToString();

            UpdateBandsTableForm form = new UpdateBandsTableForm("delete", bandId, bandName, bandGenre, bandTheme);
            form.ShowDialog();
            showBands();
        }

        private void updateBandButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandName = selectedRow.Cells[1].Value.ToString();
            var bandGenre = selectedRow.Cells[2].Value.ToString();
            var bandTheme = selectedRow.Cells[3].Value.ToString();

            UpdateBandsTableForm form = new UpdateBandsTableForm("update", bandId, bandName, bandGenre, bandTheme);
            form.ShowDialog();
            showBands();
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
            var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);

            UpdateMembersTableForm form = new UpdateMembersTableForm("add", bandId);
            form.ShowDialog();
            showMembers(bandId);
        }

        private void deleteMemberButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var memberId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandId = Convert.ToInt32(selectedRow.Cells[1].Value);
            var memberName = selectedRow.Cells[2].Value.ToString();
            var memberInstrument = selectedRow.Cells[3].Value.ToString();

            UpdateMembersTableForm form = new UpdateMembersTableForm("delete", memberId, bandId, memberName, memberInstrument);
            form.ShowDialog();
            showMembers(bandId);
        }

        private void updateMemberButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            var memberId = Convert.ToInt32(selectedRow.Cells[0].Value);
            var bandId = Convert.ToInt32(selectedRow.Cells[1].Value);
            var memberName = selectedRow.Cells[2].Value.ToString();
            var memberInstrument = selectedRow.Cells[3].Value.ToString();

            UpdateMembersTableForm form = new UpdateMembersTableForm("update", memberId, bandId, memberName, memberInstrument);
            form.ShowDialog();
            showMembers(bandId);
        }
    }
}
