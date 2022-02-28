using System;
using System.Collections.Generic;
using System.Xml;

namespace IATClient.Messages
{
    class DeploymentProgressUpdate : INamedXmlSerializable
    {
        public enum EStage
        {
            unset, compilingXSLT, encryptingCode, mungingCode, processingFileManifest, creatingBackup, restoringBackup,
            initializingDeployment, finalizingDeployment, generatingIATHTML, generatingSurveyHTML, generatingIATDescriptor, generatingSurveyDescriptor,
            generatingIATScript, generatingSurveyScript, generatingIATHeaderScript, generatingSurveyHeaderScript, processingUniqueSurveyResponses, recordingProcessedJS,
            generatingAES, timerExpired, comparingDescriptors, xsltFailure, generatingIAT, generatingSurvey, backingUpIAT, mismatchedDeploymentDescriptors, success, failed
        };

        private static String[] StageMessages =
            { String.Empty, Properties.Resources.sPreparingDeploymentResources, Properties.Resources.sEncryptingCode, Properties.Resources.sMungingCode,
                Properties.Resources.sProcessingFileManifest, Properties.Resources.sCreatingBackup,
                Properties.Resources.sRestoringBackup, Properties.Resources.sInitializingDeployment, Properties.Resources.sFinalizingDeployment, Properties.Resources.sGeneratingIATHTML,
                Properties.Resources.sGeneratingSurveyHTML, Properties.Resources.sGeneratingIATDescriptor, Properties.Resources.sGeneratingSurveyDescriptor,
                Properties.Resources.sGeneratingIATScript, Properties.Resources.sGeneratingSurveyScript, Properties.Resources.sGeneratingIATHeaderScript,
                Properties.Resources.sGeneratingSurveyHeaderScript, Properties.Resources.sProcessingUniqueSurveyResponses, Properties.Resources.sRecordingProcessedJS,
                Properties.Resources.sProcessingAES, Properties.Resources.sDeploymentTimerExpired, Properties.Resources.sDeploymentComparingDescriptors,
                Properties.Resources.sDeploymentXsltFailure, Properties.Resources.sDeploymentGeneratingIAT, Properties.Resources.sDeploymentGeneratingSurvey,
                Properties.Resources.sBackingUpIAT, String.Empty, String.Empty, String.Empty
            };

        private static Dictionary<EStage, String> StateMessageDictionary = new Dictionary<EStage, String>();

        static DeploymentProgressUpdate()
        {
            Array stageArray = Enum.GetValues(typeof(EStage));
            for (int ctr = 0; ctr < stageArray.Length; ctr++)
                StateMessageDictionary[(EStage)stageArray.GetValue(ctr)] = StageMessages[ctr];
        }

        private EStage _Stage;
        private String _ActiveItem;
        private int _ProgressMin, _ProgressMax, _CurrentProgress;
        private bool _IsLastUpdate = false;
        private CServerException ex = null;

        public CServerException DeploymentException
        {
            get
            {
                return ex;
            }
        }

        public bool IsLastUpdate
        {
            get
            {
                return _IsLastUpdate;
            }
        }

        public String StatusMessage
        {
            get
            {
                if (ActiveItem != String.Empty)
                    return String.Format(StateMessageDictionary[Stage], ActiveItem);
                return StateMessageDictionary[Stage];
            }
        }

        public EStage Stage
        {
            get
            {
                return _Stage;
            }
        }

        public String ActiveItem
        {
            get
            {
                return _ActiveItem;
            }
        }

        public int ProgressMin
        {
            get
            {
                return _ProgressMin;
            }
        }

        public int ProgressMax
        {
            get
            {
                return _ProgressMax;
            }
        }

        public int CurrentProgress
        {
            get
            {
                return _CurrentProgress;
            }
        }

        public DeploymentProgressUpdate()
        {
            _Stage = EStage.unset;
            _ActiveItem = String.Empty;
            _ProgressMin = -1;
            _ProgressMax = -1;
            _CurrentProgress = -1;
        }

        public String GetName()
        {
            return "DeploymentProgressUpdate";
        }

        public void ReadXml(XmlNode node)
        {

        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _Stage = (EStage)Enum.Parse(typeof(EStage), reader.ReadElementString());
            _ActiveItem = reader.ReadElementString("ActiveElement");
            _ProgressMin = Convert.ToInt32(reader.ReadElementString("ProgressMin"));
            _ProgressMax = Convert.ToInt32(reader.ReadElementString("ProgressMax"));
            _CurrentProgress = Convert.ToInt32(reader.ReadElementString("ProgressVal"));
            _IsLastUpdate = Convert.ToBoolean(reader.ReadElementString("LastUpdate"));
            if (reader.Name == "DeploymentException")
            {
                ex = new CServerException();
                ex.ReadXml(reader);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
