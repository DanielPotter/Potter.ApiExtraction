﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.7.2046.0.
// 
namespace Potter.ApiExtraction.Core.Configuration {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    [System.Xml.Serialization.XmlRootAttribute("Api", Namespace="http://schemas.danielrpotter.com/api/configuration/2017", IsNullable=false)]
    public partial class ApiConfiguration {
        
        private AssemblyElement assemblyField;
        
        private ApiConfigurationTypes typesField;
        
        /// <remarks/>
        public AssemblyElement Assembly {
            get {
                return this.assemblyField;
            }
            set {
                this.assemblyField = value;
            }
        }
        
        /// <remarks/>
        public ApiConfigurationTypes Types {
            get {
                return this.typesField;
            }
            set {
                this.typesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public partial class AssemblyElement {
        
        private string nameField;
        
        private string locationField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Location {
            get {
                return this.locationField;
            }
            set {
                this.locationField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NamespaceSelector))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TypeSelector))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public abstract partial class MemberSelector {
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public partial class NamespaceSelector : MemberSelector {
        
        private bool recursiveField;
        
        public NamespaceSelector() {
            this.recursiveField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Recursive {
            get {
                return this.recursiveField;
            }
            set {
                this.recursiveField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public partial class TypeSelector : MemberSelector {
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public partial class ApiConfigurationTypes {
        
        private MemberSelector[] itemsField;
        
        private TypeMode modeField;
        
        private bool includeObsoleteField;
        
        public ApiConfigurationTypes() {
            this.modeField = TypeMode.Blacklist;
            this.includeObsoleteField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Namespace", typeof(NamespaceSelector))]
        [System.Xml.Serialization.XmlElementAttribute("Type", typeof(TypeSelector))]
        public MemberSelector[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(TypeMode.Blacklist)]
        public TypeMode Mode {
            get {
                return this.modeField;
            }
            set {
                this.modeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IncludeObsolete {
            get {
                return this.includeObsoleteField;
            }
            set {
                this.includeObsoleteField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.7.2046.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.danielrpotter.com/api/configuration/2017")]
    public enum TypeMode {
        
        /// <remarks/>
        Whitelist,
        
        /// <remarks/>
        Blacklist,
    }
}
