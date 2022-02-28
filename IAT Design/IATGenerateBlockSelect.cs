using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace IATClient
{
    partial class IATGenerateBlockSelect : Form
    {
        private CIAT _IAT;
        private CIATBlock _FirstBlock, _SecondBlock;
        private bool _RandomizeGeneratedBlocks, _EnableAlternation;

        public CIATBlock FirstBlock
        {
            get
            {
                return _FirstBlock;
            }
        }

        public CIATBlock SecondBlock
        {
            get
            {
                return _SecondBlock;
            }
        }

        public bool RandomizeGeneratedBlocks
        {
            get
            {
                return _RandomizeGeneratedBlocks;
            }
        }

        public bool EnableAlternation
        {
            get
            {
                return _EnableAlternation;
            }
        }

        public CIAT IAT
        {
            set
            {
                _IAT = value;
                List<String> StrList = new List<String>();
                for (int ctr = 0; ctr < _IAT.Contents.Count; ctr++)
                {
                    if (_IAT.Contents[ctr].Type == ContentsItemType.IATBlock)
                        if (((CIATBlock)_IAT.Contents[ctr]).Key.KeyType != IATKeyType.DualKey)
                            StrList.Add(_IAT.Contents[ctr].Name);
                }
                FirstBlockCombo.Items.Clear();
                for (int ctr = 0; ctr < StrList.Count; ctr++)
                {
                    FirstBlockCombo.Items.Add(StrList[ctr]);
                    SecondBlockCombo.Items.Add(StrList[ctr]);
                }
                FirstBlockCombo.SelectedIndex = 0;
                SecondBlockCombo.SelectedIndex = 1;
            }
        }

        public IATGenerateBlockSelect()
        {
            InitializeComponent();
            _IAT = null;
            _FirstBlock = null;
            _SecondBlock = null;
            RandomizeGeneratedBlocksCheck.Checked = true;
            EnableAlternationCheck.Checked = true;
            _RandomizeGeneratedBlocks = true;
            _EnableAlternation = true;
        }

        private void FirstBlock_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int ctr = 0; ctr < _IAT.Contents.Count; ctr++)
                if (_IAT.Contents[ctr].Name == FirstBlockCombo.SelectedItem.ToString())
                    _FirstBlock = (CIATBlock)_IAT.Contents[ctr];
            if (FirstBlock == SecondBlock)
                OK.Enabled = false;
            else
                OK.Enabled = true;
        }

        private void SecondBlockCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int ctr = 0; ctr < _IAT.Contents.Count; ctr++)
                if (_IAT.Contents[ctr].Name == SecondBlockCombo.SelectedItem.ToString())
                    _SecondBlock = (CIATBlock)_IAT.Contents[ctr];
            if (FirstBlock == SecondBlock)
                OK.Enabled = false;
            else
                OK.Enabled = true;
        }

        private void RandomizeGeneratedBlocksCheck_CheckedChanged(object sender, EventArgs e)
        {
            _RandomizeGeneratedBlocks = RandomizeGeneratedBlocksCheck.Checked;
        }

        private void EnableAlternationCheck_CheckedChanged(object sender, EventArgs e)
        {
            _EnableAlternation = EnableAlternationCheck.Checked;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
