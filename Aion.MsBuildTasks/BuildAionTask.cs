using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Aion.MsBuildTasks
{
    public class BuildAionTask : Task
    {
        public override bool Execute()
        {
            Log.LogMessage(string.Format("Starting to build '{0}'", ApplicationToBuild));

            try
            {
                var aionBuilder = new AionBuilder(AionBuildProcess, null, Log);

                var worker = new BackgroundWorker();
                worker.DoWork += WorkerDoWork;
                worker.WorkerSupportsCancellation = true;
                worker.RunWorkerAsync();

                aionBuilder.Build(ApplicationToBuild, ShouldRestoreCodeFromApp, BuildTimeOutInSeconds);

                worker.CancelAsync();

                Log.LogMessage(string.Format("Finished building '{0}'{1}", ApplicationToBuild, Environment.NewLine));

                return aionBuilder.IsBuildSuccesful;
            }
            catch (Exception e)
            {
                Log.LogError(e.Message + " > " + e.StackTrace);
                return false;
            }
        }

        void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var aionProcessKiller = new AionProcessKiller(Log, AionBuildProcess);
            for (var i = 0; i < BuildTimeOutInSeconds; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                Thread.Sleep(1000);
            }
            if (!e.Cancel)
            {
                aionProcessKiller.Kill();
            }
        }

        #region Properties

        [Required]
        public string ApplicationToBuild { get; set; }

        private string _aionBuildProcess = "respawn.exe"; //default aion builder
        public string AionBuildProcess
        {
            get { return _aionBuildProcess; }
            set { _aionBuildProcess = value; }
        }

        private int _buildTimeOutInSeconds = 60; //default build timeout
        public int BuildTimeOutInSeconds
        {
            get { return _buildTimeOutInSeconds; }
            set { _buildTimeOutInSeconds = value; }
        }

        private bool _shouldRestoreCodeFromApp = true; // default restore code from app before build
        public bool ShouldRestoreCodeFromApp
        {
            get { return _shouldRestoreCodeFromApp; }
            set { _shouldRestoreCodeFromApp = value; }
        }

        #endregion

    }
}
