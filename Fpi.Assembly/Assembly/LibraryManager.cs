using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Util.Reflection;
using Fpi.Xml;

namespace Fpi.Assembly
{
    public class LibraryManager : BaseNode
    {
        public bool active;
        public NodeList initMembers;

        private LibraryManager()
        {
            loadXml();
            LoadAssembly();
        }

        private static readonly object syncObj = new object();
        private static LibraryManager _instance;

        public static LibraryManager GetInstance()
        {
            lock (syncObj)
            {
                if (_instance == null)
                {
                    _instance = new LibraryManager();
                }
            }
            return _instance;
        }

        public override BaseNode Init(XmlNode node)
        {
            active = true;
            initMembers = new NodeList();
            return base.Init(node);
        }

        public string[] GetTypeNameList(string asmFile)
        {
            Type[] list = GetTypeList(asmFile);
            string[] names = new string[list.Length];
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = list[i].FullName;
            }
            return names;
        }

        public Type[] GetTypeList(string asmFile)
        {
            System.Reflection.Assembly asm = ReflectionHelper.GetAssemblyByFile(asmFile);
            Type[] list = asm.GetTypes(); //GetExportedTypes in PC
            return list;
        }

        public string[] GetInitMemberNamesList(string asmFile)
        {
            ArrayList list = new ArrayList();
            if (this.initMembers != null)
            {
                foreach (InitMember im in initMembers)
                {
                    if (im.ownerDLL == asmFile && im.Existed)
                    {
                        list.Add(im);
                    }
                }
            }
            string[] rv = new string[list.Count];
            for (int i = 0; i < rv.Length; i++)
            {
                rv[i] = ((InitMember) list[i]).id;
            }
            return rv;
        }


        public IInitialization[] GetAllInitClass()
        {
            ArrayList list = GetAllExistedInitMember();
            IInitialization[] rv = new IInitialization[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                rv[i] = ((InitMember) list[i]).GetInitInstance();
            }
            return rv;
        }


        //public DialogResult ShowManagerForm()
        //{
        //    Fpi.Assembly.UI.PC.FormManager form = new Fpi.Assembly.UI.PC.FormManager();

        //    DialogResult res = form.ShowDialog();
        //    return res;
        //}



        private string INIT_INTERFACE = typeof (IInitialization).FullName;

        private void LoadAssembly()
        {
            Hashtable typeTable = ReflectionHelper.TypeTable;

            foreach (Type type in typeTable.Values)
            {
                if (type.IsClass)
                {
                    object ob = type.GetInterface(INIT_INTERFACE, true);
                    if (ob != null)
                    {
                        AddInitMember(type);
                    }
                }
            }

            //移除不再有效的节点
            if (this.initMembers != null)
            {
                for (int i = 0; i < this.initMembers.GetCount(); i++)
                {
                    InitMember node = (InitMember) this.initMembers[i];
                    if (!node.Existed)
                    {
                        this.initMembers.Remove(node);
                    }
                }
            }

            base.Save();
        }

        private void AddInitMember(Type type)
        {
            if (this.initMembers == null)
            {
                this.initMembers = new NodeList();
            }

            InitMember node = new InitMember(type.FullName, type.Name);
            node.ownerDLL = type.Module.Name;
            node.Existed = true;
            if (!this.initMembers.Add(node))
            {
                //若添加不成功，则说明已经相同项
                ((InitMember) initMembers[node.id]).Existed = true;
            }
        }


        public ArrayList GetAllExistedInitMember()
        {
            ArrayList list = new ArrayList();

            if (this.initMembers != null)
            {
                foreach (InitMember im in initMembers)
                {
                    if (im.Existed)
                    {
                        list.Add(im);
                    }
                }
            }
            return list;
        }


        public void InitAllClass(IInitializationListener listener)
        {
            if (!active)
            {
                return;
            }
            ArrayList list = GetAllExistedInitMember();
            IInitialization instance = null;

            foreach (InitMember im in list)
            {
                try
                {
                    if (!im.active)
                    {
                        continue;
                    }
                    instance = im.GetInitInstance();
                    if (instance == null) continue;
                    if (listener != null)
                    {
                        listener.BeforeInit(im);
                    }

                    instance.Initialize();

                    if (listener != null)
                    {
                        listener.AfterInit(im);
                    }
                }
                catch (Exception ex)
                {
                    if (listener == null)
                    {
                        throw ex;
                    }
                    listener.OnInitException(im, new InitException(ex.Message,ex), im.initExceptionIsFatal);
                    if (im.initExceptionIsFatal)
                    {
                        break;
                    }
                }
            }
        }

        public string[] GetChildTypeNames(Type baseType)
        {
            ArrayList list = new ArrayList();
            Hashtable AssemblyTable = ReflectionHelper.AssemblyTable;

            int count = 0;
            foreach (System.Reflection.Assembly asm in AssemblyTable.Keys)
            {
                string[] formNames = ReflectionHelper.GetChildTypeNames(asm, baseType);
                count += formNames.Length;
            }

            string[] rv = new string[count];
            list.CopyTo(rv);

            return rv;
        }
    }
}