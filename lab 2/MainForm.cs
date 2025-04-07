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
using System.Xml;

namespace lab_1
{
    public partial class MainForm : Form
    {
        SqlConnection conn = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\Robert\\Documents\\Metal.mdf;Integrated Security=True;Connect Timeout=30;");
        SqlDataAdapter da = new SqlDataAdapter();
        DataSet ds = new DataSet();
        private string currentTable = "Bands";
        private XmlDocument config = new XmlDocument();
        private XmlNode currentScenarioNode;
        private string masterTableName;
        private string masterPrimaryKeyColumn;
        private string masterSelectQuery;
        private string masterAddCommand;
        private string masterUpdateCommand;
        private string masterDeleteCommand;
        private string detailTableName;
        private string detailForeignKeyColumn;
        private string detailSelectQuery;
        private string detailAddCommand;
        private string detailUpdateCommand;
        private string detailDeleteCommand;
        private string currentScenarioName = "BandsAndMembers";

        public MainForm()
        {
            InitializeComponent();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                config.Load("MasterDetailConfig.xml");
                XmlNodeList scenarioNodes = config.SelectNodes("Configuration/Scenario");
                if (scenarioNodes != null)
                {
                    scenarioComboBox.Items.Clear();
                    foreach (XmlNode scenarioNode in scenarioNodes)
                    {
                        string scenarioName = scenarioNode.Attributes["name"]?.Value;
                        if (!string.IsNullOrEmpty(scenarioName))
                        {
                            scenarioComboBox.Items.Add(scenarioName);
                        }
                    }
                    if (scenarioComboBox.Items.Contains(currentScenarioName))
                    {
                        scenarioComboBox.SelectedItem = currentScenarioName;
                    }
                    else if (scenarioComboBox.Items.Count > 0)
                    {
                        scenarioComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration file: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            updateButtonEnablement();
        }

        private void scenarioComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (scenarioComboBox.SelectedItem != null)
            {
                currentScenarioName = scenarioComboBox.SelectedItem.ToString();
                LoadScenario(currentScenarioName);
            }
        }

        private void LoadScenario(string scenarioName)
        {
            ds.Clear();
            dataGridView1.DataSource = null;
            currentTable = null;
            masterTableName = null;
            detailTableName = null;
            detailForeignKeyColumn = null;
            masterPrimaryKeyColumn = null;

            currentScenarioNode = config.SelectSingleNode($"Configuration/Scenario[@name='{scenarioName}']");

            if (currentScenarioNode != null)
            {
                Text = currentScenarioNode.SelectSingleNode("FormCaption")?.InnerText;

                var masterTableNode = currentScenarioNode.SelectSingleNode("MasterTable");
                if (masterTableNode != null)
                {
                    masterTableName = masterTableNode.SelectSingleNode("Name")?.InnerText;
                    masterPrimaryKeyColumn = masterTableNode.SelectSingleNode("PrimaryKeyColumn")?.InnerText;
                    masterSelectQuery = masterTableNode.SelectSingleNode("SelectQuery")?.InnerText;
                    masterAddCommand = masterTableNode.SelectSingleNode("AddCommand")?.InnerText;
                    masterUpdateCommand = masterTableNode.SelectSingleNode("UpdateCommand")?.InnerText;
                    masterDeleteCommand = masterTableNode.SelectSingleNode("DeleteCommand")?.InnerText;

                    string masterEntity = masterTableName.Substring(0, masterTableName.Length - 1);
                    addBandButton.Text = $"Add New {masterEntity}";
                    updateBandButton.Text = $"Update {masterEntity}";
                    deleteBandButton.Text = $"Delete {masterEntity}";
                }

                var detailTableNode = currentScenarioNode.SelectSingleNode("DetailTable");
                if (detailTableNode != null)
                {
                    detailTableName = detailTableNode.SelectSingleNode("Name")?.InnerText;
                    detailForeignKeyColumn = detailTableNode.SelectSingleNode("ForeignKeyColumn")?.InnerText;
                    detailSelectQuery = detailTableNode.SelectSingleNode("SelectQuery")?.InnerText;
                    detailAddCommand = detailTableNode.SelectSingleNode("AddCommand")?.InnerText;
                    detailUpdateCommand = detailTableNode.SelectSingleNode("UpdateCommand")?.InnerText;
                    detailDeleteCommand = detailTableNode.SelectSingleNode("DeleteCommand")?.InnerText;

                    if (scenarioName == "BandsAndMembers")
                    {
                        showMembersButton.Text = "Show Members";
                        addMemberButton.Text = "Add New Member";
                        deleteMemberButton.Text = "Delete Member";
                        updateMemberButton.Text = "Update Member";
                    }
                    else if (scenarioName == "BandsAndAlbums")
                    {
                        showMembersButton.Text = "Show Albums";
                        addMemberButton.Text = "Add New Album";
                        deleteMemberButton.Text = "Delete Album";
                        updateMemberButton.Text = "Update Album";
                    }
                }

                if (!string.IsNullOrEmpty(masterTableName))
                {
                    ShowMasterData();
                    currentTable = masterTableName;
                }
            }
            updateButtonEnablement();
        }

        private void ShowMasterData()
        {
            dataGridView1.DataSource = null;

            if (!string.IsNullOrEmpty(masterSelectQuery))
            {
                string query = masterSelectQuery;
                SqlCommand cmd = new SqlCommand(query, conn);

                da.SelectCommand = cmd;
                ds.Clear();
                da.Fill(ds, masterTableName);

                dataGridView1.DataSource = ds.Tables[masterTableName];
                dataGridView1.Refresh();
            }
        }

        private void ShowDetailData(int masterId = -1)
        {
            if (dataGridView1.SelectedRows.Count == 0 && masterId == -1 && currentTable == masterTableName)
            {
                MessageBox.Show("No master record selected.");
                dataGridView1.DataSource = null;
                return;
            }

            int selectedMasterId = (masterId == -1 && currentTable == masterTableName && dataGridView1.SelectedRows.Count > 0)
                ? Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value)
                : masterId;

            dataGridView1.DataSource = null;

            if (!string.IsNullOrEmpty(detailSelectQuery))
            {
                string query = detailSelectQuery.Replace("@MasterId", "@" + masterPrimaryKeyColumn);
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@" + masterPrimaryKeyColumn, selectedMasterId);

                    da.SelectCommand = cmd;
                    ds.Clear();
                    da.Fill(ds, detailTableName);

                    dataGridView1.DataSource = ds.Tables[detailTableName];
                    dataGridView1.Refresh();
                    currentTable = detailTableName;
                    updateButtonEnablement();
                }
            }
        }

        private void showBandsButton_Click(object sender, EventArgs e)
        {
            ShowMasterData();
            currentTable = masterTableName;
            updateButtonEnablement();
        }

        private void showMembersButton_Click(object sender, EventArgs e)
        {
            ShowDetailData(-1);
            currentTable = detailTableName;
            updateButtonEnablement();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            updateButtonEnablement();
        }

        private void updateButtonEnablement()
        {
            if (currentTable == masterTableName)
            {
                addBandButton.Enabled = true;
                updateBandButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                deleteBandButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                showMembersButton.Enabled = dataGridView1.SelectedRows.Count > 0;
                addMemberButton.Enabled = false;
                deleteMemberButton.Enabled = false;
                updateMemberButton.Enabled = false;
            }
            else if (currentTable == detailTableName)
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
            if (currentScenarioNode != null)
            {
                string masterTableName = currentScenarioNode.SelectSingleNode("MasterTable")?.SelectSingleNode("Name")?.InnerText;
                UpdateBandsTableForm form = new UpdateBandsTableForm("add");
                form.Text = $"Add New {masterTableName}";
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ShowMasterData();
                }
            }
        }

        private void deleteBandButton_Click(object sender, EventArgs e)
        {
            if (currentScenarioNode != null && dataGridView1.SelectedRows.Count > 0 && currentTable == masterTableName && !string.IsNullOrEmpty(masterDeleteCommand))
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                var primaryKeyValue = selectedRow.Cells[0].Value;

                using (SqlCommand cmd = new SqlCommand(masterDeleteCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@" + masterPrimaryKeyColumn, primaryKeyValue);

                    try
                    {
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"{masterTableName} deleted successfully.");
                            ShowMasterData();
                        }
                        else
                        {
                            MessageBox.Show($"Error deleting {masterTableName}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting {masterTableName}: {ex.Message}");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void updateBandButton_Click(object sender, EventArgs e)
        {
            if (currentScenarioNode != null && dataGridView1.SelectedRows.Count > 0 && currentTable == masterTableName)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                var bandId = Convert.ToInt32(selectedRow.Cells[0].Value);
                var bandName = selectedRow.Cells[1].Value.ToString();
                var bandGenre = selectedRow.Cells[2].Value.ToString();
                var bandTheme = selectedRow.Cells[3].Value.ToString();

                UpdateBandsTableForm form = new UpdateBandsTableForm("update", bandId, bandName, bandGenre, bandTheme);
                form.Text = $"Update {masterTableName}";
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ShowMasterData();
                }
            }
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            if (currentScenarioNode != null && dataGridView1.SelectedRows.Count > 0 && currentTable == masterTableName)
            {
                DataGridViewRow selectedMasterRow = dataGridView1.SelectedRows[0];
                var masterId = Convert.ToInt32(selectedMasterRow.Cells[0].Value);

                UpdateMembersTableForm form = new UpdateMembersTableForm("add", masterId);
                form.Text = $"Add New {detailTableName}";
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ShowDetailData(masterId);
                }
            }
        }

        private void deleteMemberButton_Click(object sender, EventArgs e)
        {
            if (currentScenarioNode != null && dataGridView1.SelectedRows.Count > 0 && currentTable == detailTableName && !string.IsNullOrEmpty(detailDeleteCommand))
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                var primaryKeyValue = selectedRow.Cells[0].Value;

                using (SqlCommand cmd = new SqlCommand(detailDeleteCommand, conn))
                {
                    cmd.Parameters.AddWithValue("@MemberId", primaryKeyValue);

                    try
                    {
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"{detailTableName} deleted successfully.");
                            ShowDetailData(Convert.ToInt32(dataGridView1.Rows[selectedRow.Index].Cells[1].Value));
                        }
                        else
                        {
                            MessageBox.Show($"Error deleting {detailTableName}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting {detailTableName}: {ex.Message}");
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void updateMemberButton_Click(object sender, EventArgs e)
        {
            if (currentScenarioNode != null && dataGridView1.SelectedRows.Count > 0 && currentTable == detailTableName)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                var memberId = Convert.ToInt32(selectedRow.Cells[0].Value);
                var bandId = Convert.ToInt32(selectedRow.Cells[1].Value);
                var memberName = selectedRow.Cells[2].Value.ToString();
                var memberInstrument = selectedRow.Cells[3].Value.ToString();

                UpdateMembersTableForm form = new UpdateMembersTableForm("update", memberId, bandId, memberName, memberInstrument);
                form.Text = $"Update {detailTableName}";
                if (form.ShowDialog() == DialogResult.OK)
                {
                    ShowDetailData(bandId);
                }
            }
        }
    }
}