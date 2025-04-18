using System.Reflection;
using System.Windows.Forms;
using System;
using System.Collections;

namespace Fpi.Util.Reflection
{
    public partial class ReflectionExplore : Form, IComparer
    {
        public ReflectionExplore()
        {
            InitializeComponent();

            this.cmbAsm.Items.Add(string.Empty);
            foreach (string asm in ReflectionHelper.AssemblyTable.Keys)
            {
                this.cmbAsm.Items.Add(asm);
            }
            this.cmbAsm.SelectedIndex = 0;
        }

        public ReflectionExplore(Type BaseType)
            : this()
        {
            this.baseType = BaseType;
            this.cmbType.SelectedItem = BaseType;
            this.cmbType.Text = BaseType.FullName;
            this.cmbType.Tag = BaseType;
            this.cmbType.Enabled = false;
        }


        Assembly asm;
        Type baseType;

        bool _MultiSelect;
        public bool MultiSelect
        {
            get
            {
                _MultiSelect = this.listView.MultiSelect;
                return _MultiSelect;
            }
            set
            {
                _MultiSelect = value;
                this.listView.MultiSelect = value;
            }
        }

        Type _SelectedType;
        public Type SelectedType
        {
            get { return _SelectedType; }
            set { _SelectedType = value; }
        }

        Type[] _SelectedTypes;
        public Type[] SelectedTypes
        {
            get { return _SelectedTypes; }
            set { _SelectedTypes = value; }
        }

        private void cmbAsm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.cmbType.Enabled)
            {
                return;
            }

            this.cmbType.Items.Clear();

            Type[] types;
            if (this.cmbAsm.SelectedIndex == 0)
            {
                types = new Type[ReflectionHelper.TypeTable.Values.Count];
                ReflectionHelper.TypeTable.Values.CopyTo(types, 0);
            }
            else
            {
                asm = ReflectionHelper.GetAssemblyByFile(this.cmbAsm.Text);
                types = asm.GetTypes();
            }

            if (types != null)
            {
                Array.Sort(types, this);
                this.cmbType.Items.AddRange(types);
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.cmbType.Tag = this.cmbType.SelectedItem;
        }

        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (this.listView.MultiSelect)
                return;

            if (e.Item.Checked)
            {
                e.Item.Selected = true;
                foreach (ListViewItem lvi in this.listView.Items)
                {
                    if (lvi != e.Item)
                        lvi.Checked = false;
                }
            }

        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            this.listView.Items.Clear();

            baseType = this.cmbType.Tag as Type;
            if (baseType == null)
                return;

            if (this.cmbAsm.SelectedIndex != 0)
            {
                string asmFile = this.cmbAsm.Text;
                asm = ReflectionHelper.GetAssemblyByFile(asmFile);
            }
            else 
            {
                asm = null;
            }
   
            Type[] types;
            if (asm == null)
                types = ReflectionHelper.GetChildTypes(baseType);
            else
                types = ReflectionHelper.GetChildTypes(asm,baseType);

            if (types != null)
            {
                Array.Sort(types,this);
                foreach (Type type in types)
                {
                    ListViewItem lvi = new ListViewItem(new string[] { type.Assembly.ManifestModule.Name, type.FullName });
                    lvi.Tag = type;
                    this.listView.Items.Add(lvi);

                    if (_SelectedType != null && _SelectedType == type)
                    {
                        lvi.Checked = true;
                    }

                    if (_SelectedTypes != null)
                    {
                        if(Array.IndexOf(_SelectedTypes, type) >= 0 )
                        {
                            lvi.Checked = true;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("没有找到相应的子类型！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_MultiSelect)
                {
                    if (this.listView.CheckedItems.Count > 0)
                        _SelectedType = this.listView.CheckedItems[0].Tag as Type;
                }
                else 
                {
                    _SelectedTypes = new Type[this.listView.CheckedItems.Count];
                    for (int i = 0; i < _SelectedTypes.Length; i++)
                    {
                        _SelectedTypes[i] = this.listView.CheckedItems[i].Tag as Type;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSetNull_Click(object sender, EventArgs e)
        {
            _SelectedType = null;
            _SelectedTypes = new Type[0];

            foreach (ListViewItem lvi in this.listView.CheckedItems)
            {
                lvi.Checked = false;
            }
        }



        #region IComparer 成员

        public int Compare(object x, object y)
        {
            if ((x is Type) && (y is Type))
            {
                Type a = (Type)x;
                Type b = (Type)y;

                return a.FullName.CompareTo(b.FullName);
            }
            return 0;
        }

        #endregion

    }
}