using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nektra.Deviare2;

namespace PrintLogger
{
    public partial class PrintLogger : Form
    {
        private NktSpyMgr _spyMgr;
        private NktProcess _process;

        public PrintLogger()
        {
            InitializeComponent();

            _spyMgr = new NktSpyMgr();
            _spyMgr.Initialize();
            _spyMgr.OnFunctionCalled += new DNktSpyMgrEvents_OnFunctionCalledEventHandler(OnFunctionCalled);

            GetProcess("spoolsv.exe");
            if (_process == null)
            {
                MessageBox.Show("Please start \"spoolsv.exe\" before!", "Error");
                Environment.Exit(0);
            }
        }

        private void PrintLogger_Load(object sender, EventArgs e)
        {
            NktHook hook = _spyMgr.CreateHook("spoolsv.exe!PrvStartDocPrinterW", (int)(eNktHookFlags.flgRestrictAutoHookToSameExecutable & eNktHookFlags.flgOnlyPreCall));
            hook.Hook(true);
            hook.Attach(_process, true);
        }

        private bool GetProcess(string proccessName)
        {
            NktProcessesEnum enumProcess = _spyMgr.Processes();
            NktProcess tempProcess = enumProcess.First();
            while (tempProcess != null)
            {
                if (tempProcess.Name.Equals(proccessName, StringComparison.InvariantCultureIgnoreCase) && tempProcess.PlatformBits > 0 && tempProcess.PlatformBits <= IntPtr.Size * 8)
                {
                    _process = tempProcess;
                    return true;
                }
                tempProcess = enumProcess.Next();
            }

            _process = null;
            return false;
        }

        private void OnFunctionCalled(NktHook hook, NktProcess process, NktHookCallInfo hookCallInfo)
        {
            string strDocument = "Document: ";

            INktParamsEnum paramsEnum = hookCallInfo.Params();

            INktParam param = paramsEnum.First();

            param = paramsEnum.Next();

            param = paramsEnum.Next();
            if (param.PointerVal != IntPtr.Zero)
            {
                INktParamsEnum paramsEnumStruct = param.Evaluate().Fields();
                INktParam paramStruct = paramsEnumStruct.First();

                strDocument += paramStruct.ReadString();
                strDocument += "\n";
            }

            Output(strDocument);
        }

        public delegate void OutputDelegate(string strOutput);

        private void Output(string strOutput)
        {
            if (InvokeRequired)
                BeginInvoke(new OutputDelegate(Output), strOutput);
            else
                textOutput.AppendText(strOutput);
        }
    }
}
