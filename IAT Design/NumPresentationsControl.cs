using System;
using System.Windows.Forms;

namespace IATClient
{
    partial class NumPresentationsControl : UserControl
    {
        private ContentsList IATContents
        {
            get
            {
                return IAT.Contents;
            }
        }

        public Action<DialogResult> OnControlComplete;
        private CIAT IAT;


        public int[] NumPresentations
        {
            get
            {
                if (NumPresentationsGrid.Rows.Count == 0)
                    return null;
                int[] numPresentations = new int[NumPresentationsGrid.Rows.Count];
                for (int ctr = 0; ctr < numPresentations.Length; ctr++)
                    numPresentations[ctr] = Convert.ToInt32(NumPresentationsGrid.Rows[ctr].Cells[1].Value.ToString());
                return numPresentations;
            }
        }

        public NumPresentationsControl(CIAT iat)
        {
            InitializeComponent();
            IAT = iat;
            OnControlComplete = null;
            NumPresentationsGrid.AllowUserToAddRows = false;
            NumPresentationsGrid.AllowUserToDeleteRows = false;
        }

        private void NumPresentationsControl_Load(object sender, EventArgs e)
        {
            if (IATContents == null)
                throw new Exception("An attempt was made to load a NumPresentationsControl without first setting its IATContents property.");
            NumPresentationsGrid.SuspendLayout();
            DataGridViewColumn IATBlockColumn = new DataGridViewColumn();
            IATBlockColumn.HeaderCell = new DataGridViewColumnHeaderCell();
            IATBlockColumn.HeaderText = "IAT Block";
            IATBlockColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            IATBlockColumn.ReadOnly = true;
            IATBlockColumn.Resizable = DataGridViewTriState.False;
            IATBlockColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            IATBlockColumn.Width = NumPresentationsGrid.Width - TextRenderer.MeasureText("# of Presentations", System.Drawing.SystemFonts.DialogFont).Width - 20 -
                IATBlockColumn.DividerWidth;
            NumPresentationsGrid.Columns.Add(IATBlockColumn);
            DataGridViewColumn NumPresentationsColumn = new DataGridViewColumn();
            NumPresentationsColumn.HeaderCell = new DataGridViewColumnHeaderCell();
            NumPresentationsColumn.HeaderText = "# of Presentations";
            NumPresentationsColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            NumPresentationsColumn.ReadOnly = false;
            NumPresentationsColumn.Resizable = DataGridViewTriState.False;
            NumPresentationsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            NumPresentationsColumn.Width = TextRenderer.MeasureText("# of Presentations", System.Drawing.SystemFonts.DialogFont).Width + 15 - NumPresentationsColumn.DividerWidth;
            NumPresentationsGrid.Columns.Add(NumPresentationsColumn);
            for (int ctr = 0; ctr < IAT.Blocks.Count; ctr++)
            {
                DataGridViewRow gridRow = new DataGridViewRow();
                gridRow.Cells.Add(new DataGridViewTextBoxCell());
                gridRow.Cells.Add(new DataGridViewTextBoxCell());
                ((DataGridViewTextBoxCell)gridRow.Cells[0]).Value = IAT.Blocks[ctr].Name;
                if (true)  // Is7Block
                {
                    if ((ctr == 3) || (ctr == 6))
                        ((DataGridViewTextBoxCell)gridRow.Cells[1]).Value = "20";
                    else
                        ((DataGridViewTextBoxCell)gridRow.Cells[1]).Value = "10";
                }
                else
                    ((DataGridViewTextBoxCell)gridRow.Cells[1]).Value = IAT.Blocks[ctr].NumItems.ToString();
                NumPresentationsGrid.Rows.Add(gridRow);
            }

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            OnControlComplete.Invoke(DialogResult.Cancel);
        }

        private void OK_Click(object sender, EventArgs e)
        {
            OnControlComplete.Invoke(DialogResult.OK);
        }
    }
}
