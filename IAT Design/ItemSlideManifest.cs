﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.5485
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.1432.
// 
namespace IATClient {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/ItemSlideManifest.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ItemSlideManifest.xsd", IsNullable=false)]
    public partial class ItemSlideManifest {
        
        private TItemSlideEntry[] itemSlideEntriesField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemSlideEntries")]
        public TItemSlideEntry[] ItemSlideEntries {
            get {
                return this.itemSlideEntriesField;
            }
            set {
                this.itemSlideEntriesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ItemSlideManifest.xsd")]
    public partial class TItemSlideEntry {
        
        private uint[] itemsField;
        
        private string slideFileNameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Items")]
        public uint[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SlideFileName {
            get {
                return this.slideFileNameField;
            }
            set {
                this.slideFileNameField = value;
            }
        }
    }
}