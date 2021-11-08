using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    /// <summary>
    /// CExistsInFileResponse allows for the definition of a response type where the response must match one of a 
    /// series of values in a specified text file.  It allows for the option to permit each response in the file
    /// to be provided only once.
    /// </summary>
    public class CExistsInFileResponse : CResponse
    {
        /// <summary>
        /// An enumeration of the types of characters that can delimit the permissible responses in the text file
        /// </summary>
        public enum EDelimitation { space, comma, tab, linebreak };

        protected IATConfigMainForm.IATDirectoryCallback GetIATDirectory;
        
        // constant strings to represent each delimitation type
        private const String sSpaceDelimited = "Space";
        private const String sCommaDelimited = "Comma";
        private const String sTabDelimited = "Tab";
        private const String sLinebreakDelimited = "Linebreak";

        // declare a constant string to represent the iat file directory
        public const String sIATDirectory = "?IATDirectory?";

        // the type of delimitation
        private EDelimitation _Delimitation;

        /// <summary>
        /// gets or sets the type of character that delimits the permissible resposnes in the text file
        /// </summary>
        public EDelimitation Delimitation
        {
            get
            {
                return _Delimitation;
            }
            set
            {
                _Delimitation = value;
            }
        }

        // the name of the text file that contains the responses and the path of the file
        private String _FileName;
        private String _Directory;

        // gets or sets the full path of the file that contains the valid responses
        public String FullFilePath
        {
            get
            {
                if (_Directory == sIATDirectory)
                    return GetIATDirectory() + "\\" + _FileName;
                return _Directory + "\\" + _FileName;
            }
            set
            {
                _FileName = System.IO.Path.GetFileName(value);
                _Directory = System.IO.Path.GetDirectoryName(value);
            }
        }

        /// <summary>
        /// gets or sets the name of the file that contains the valid responses
        /// </summary>
        public String FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
            }
        }

        /// <summary>
        /// gets or sets the name of the directory the object's data is loaded from
        /// </summary>
        public String Directory
        {
            get
            {
                return _Directory;
            }
            set
            {
                _Directory = value;
            }
        }

        // specifies whether each response is permitted only once
        private bool _EachResponseOnlyOnce;

        /// <summary>
        /// gets or sets whether each response in the file is permitted only once
        /// </summary>
        public bool EachResponseOnlyOnce
        {
            get
            {
                return _EachResponseOnlyOnce;
            }
            set
            {
                _EachResponseOnlyOnce = value;
            }
        }

        // a flag to indicate whether the responses have been loaded
        private bool _ResponsesLoaded;

        // gets a value that indicates if the response file has been loaded
        public bool ResponsesLoaded
        {
            get
            {
                return _ResponsesLoaded;
            }
        }

        // a list of the valid response
        private List<String> _ValidResponses;

        /// <summary>
        /// gets or sets the list of valid responses 
        /// </summary>
        public List<String> ValidResponses
        {
            get
            {
                return _ValidResponses;
            }
            set
            {
                _ValidResponses = value;
                _ResponsesLoaded = true;
            }
        }

        // a flag to indicate whether the file that constains the responses should be copied to the output directory
        // when the project is saved
        private bool _CopyToOutputDirOnSave;

        /// <summary>
        /// gets or sets a flag indicating if the file that contains the valid responses should be copied to the output
        /// directory when the response type is saved via WriteToXml
        /// </summary>
        public bool CopyToOutputDirOnSave
        {
            get
            {
                return _CopyToOutputDirOnSave;
            }
            set
            {
                _CopyToOutputDirOnSave = value;
            }
        }

        // the output directory of the configuration file
        private String _OutputDirectory;

        /// <summary>
        /// gets or sets the output directory of the .iat file being modified
        /// </summary>
        public String OutputDirectory
        {
            get
            {
                return _OutputDirectory;
            }
            set
            {
                _OutputDirectory = value;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CExistsInFileResponse()
            : base(EResponseType.ExistsInFile)
        {
            _Directory = _FileName = String.Empty;
            _ResponsesLoaded = false;
            _ValidResponses = new List<String>();
            _EachResponseOnlyOnce = true;
            _Delimitation = EDelimitation.linebreak;
            _CopyToOutputDirOnSave = false;
            _OutputDirectory = String.Empty;
            GetIATDirectory = IATConfigMainForm.GetIATDirectory;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CExistsInFileResponse object to copy</param>
        public CExistsInFileResponse(CExistsInFileResponse r)
            : base(EResponseType.ExistsInFile)
        {
            _Directory = r._Directory;
            _FileName = r._FileName;
            _ResponsesLoaded = r._ResponsesLoaded;
            _Delimitation = r._Delimitation;
            _CopyToOutputDirOnSave = r._CopyToOutputDirOnSave;
            _OutputDirectory = r._OutputDirectory;
            _ValidResponses = new List<String>();
            for (int ctr = 0; ctr < r._ValidResponses.Count; ctr++)
                _ValidResponses.Add(r._ValidResponses[ctr]);
            GetIATDirectory = IATConfigMainForm.GetIATDirectory;
        }

        /// <summary>
        /// Ensures the specified response file is valid
        /// </summary>
        /// <returns>"true" if the file is or can be loaded, otherwise "false"</returns>
        public override bool IsValid()
        {
            if (ResponsesLoaded)
                return true;
            return LoadResponses();
        }

        /// <summary>
        /// Loads the object from the passed XmlNode.  Does not load the valid responses file
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            // check for the appropriate number of child nodes
            if (node.ChildNodes.Count != 4)
                return false;

            // load file name and directory
            _FileName = node.ChildNodes[0].InnerText;
            _Directory = node.ChildNodes[1].InnerText;
            if (_Directory == sIATDirectory)
                _CopyToOutputDirOnSave = true;

            // get the delimitation type
            string sDelim = node.ChildNodes[2].InnerText;
            switch (sDelim)
            {
                case sCommaDelimited:
                    _Delimitation = EDelimitation.comma;
                    break;

                case sLinebreakDelimited:
                    _Delimitation = EDelimitation.linebreak;
                    break;

                case sSpaceDelimited:
                    _Delimitation = EDelimitation.space;
                    break;

                case sTabDelimited:
                    _Delimitation = EDelimitation.tab;
                    break;

                default:
                    return false;
            }

            // get the EachResponseOnce flag
            _EachResponseOnlyOnce = Convert.ToBoolean(node.ChildNodes[3].InnerText);

            _CopyToOutputDirOnSave = false;
            _OutputDirectory = String.Empty;
            _ValidResponses.Clear();
            _ResponsesLoaded = false;
            return LoadResponses();
        }

        /// <summary>
        /// Writes the response object to an XmlTextWriter and, if the CopyToOutputDirOnSave flag is set, copies the 
        /// valid response file to OutputDirectory and sets Directory equal to OutputDirectory
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            // write start of resposne element
            writer.WriteStartElement("Response");

            // write the type of response as an attribute of the response element
            writer.WriteAttributeString("Type", CResponse.sTypeExistsInFile);

            // write the file name 
            writer.WriteElementString("FileName", _FileName);
            
            // copy to output directory if appropriate and write the directory name
            if ((_CopyToOutputDirOnSave) && (Directory != sIATDirectory))
            {
                System.IO.File.Copy(FullFilePath, GetIATDirectory() + "\\" + _FileName);
                _Directory = sIATDirectory;
            }
            writer.WriteElementString("Directory", _Directory);

            // write the delimitation type and whether each response is allowed only once
            String sDelim = String.Empty;
            switch (_Delimitation)
            {
                case EDelimitation.comma:
                    sDelim = sCommaDelimited;
                    break;

                case EDelimitation.linebreak:
                    sDelim = sLinebreakDelimited;
                    break;

                case EDelimitation.space:
                    sDelim = sSpaceDelimited;
                    break;

                case EDelimitation.tab:
                    sDelim = sTabDelimited;
                    break;
            }
            writer.WriteElementString("Delimitation", sDelim);
            writer.WriteElementString("EachResponseOnlyOnce", _EachResponseOnlyOnce.ToString());

            // close "Response" element
            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the list of acceptable responses from the valid response file
        /// </summary>
        /// <returns>"true" on success, "false" on error</returns>
        public bool LoadResponses()
        {
            String ValidResponse;
            String sLine;
            Char cDelim;
            System.IO.StreamReader sReader = null;

            // clear the list of valid responses
            ValidResponses.Clear();
            _ResponsesLoaded = false;
    
            // get the delimitation character
            switch (Delimitation)
            {
                case EDelimitation.comma:
                    cDelim = ',';
                    break;

                case EDelimitation.linebreak:
                    cDelim = '\n';
                    break;

                case EDelimitation.space:
                    cDelim = ' ';
                    break;

                case EDelimitation.tab:
                    cDelim = '\t';
                    break;

                default:
                    return false;
            }

            // try reading the valid response file
            try
            {
                // open the file
                sReader = System.IO.File.OpenText(FullFilePath);

                // read each line
                while ((sLine = sReader.ReadLine()) != null)
                {
                    // if delimitation is the line break, trim value for whitespace
                    if (Delimitation == EDelimitation.linebreak)
                    {
                        ValidResponse = sLine;
                        ValidResponse.Trim();
                        ValidResponses.Add(ValidResponse);
                    }
                    // else search through the line for each valid response
                    else
                    {
                        int startNdx = 0;
                        int ctr = 0;
                        while (ctr < sLine.Length)
                        {
                            while ((sLine[ctr] != cDelim) && (ctr + 1 < sLine.Length))
                                ctr++;
                            ValidResponse = sLine.Substring(startNdx, ctr - startNdx);
                            ValidResponse = ValidResponse.Trim(cDelim);
                            ValidResponse = ValidResponse.Trim();
                            if (ValidResponse.Length != 0)
                                ValidResponses.Add(ValidResponse);
                            startNdx = ++ctr;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format(Properties.Resources.sAttachedFileException, FullFilePath));
                ValidResponses.Clear();
                return false;
            }
            finally
            {
                if (sReader != null)
                    sReader.Dispose();
            }

            // success
            _ResponsesLoaded = true;
            return true;
        }

    }
}
