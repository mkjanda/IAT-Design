﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4200
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace IATClient.FileManifest {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="FileManifest")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="FileManifest", IsNullable=false)]
    public partial class Manifest {
        
        private string dataRetrievalPasswordField;
        
        private FileEntity[] filesField;
        
        /// <remarks/>
        public string DataRetrievalPassword {
            get {
                return this.dataRetrievalPasswordField;
            }
            set {
                this.dataRetrievalPasswordField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Files")]
        public FileEntity[] Files {
            get {
                return this.filesField;
            }
            set {
                this.filesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Subdirectory))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(File))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="FileManifest")]
    public abstract partial class FileEntity {
        
        private string nameField;
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="FileManifest")]
    public partial class Subdirectory : FileEntity {
        
        private File[] subdirFilesField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("SubdirFiles")]
        public File[] SubdirFiles {
            get {
                return this.subdirFilesField;
            }
            set {
                this.subdirFilesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="FileManifest")]
    public partial class File : FileEntity {
        
        private long sizeField;
        
        /// <remarks/>
        public long Size {
            get {
                return this.sizeField;
            }
            set {
                this.sizeField = value;
            }
        }
    }
}
