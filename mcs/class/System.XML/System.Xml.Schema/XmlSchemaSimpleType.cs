// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;
using System.Xml;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaSimpleType.
	/// </summary>
	public class XmlSchemaSimpleType : XmlSchemaType
	{
		private XmlSchemaSimpleTypeContent content;
		//compilation vars
		internal bool islocal = true; // Assuming local means we have to specify islocal=false only in XmlSchema
		private int errorCount;

		public XmlSchemaSimpleType()
		{
		}

		[XmlElement("restriction",typeof(XmlSchemaSimpleTypeRestriction),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("list",typeof(XmlSchemaSimpleTypeList),Namespace="http://www.w3.org/2001/XMLSchema")]
		[XmlElement("union",typeof(XmlSchemaSimpleTypeUnion),Namespace="http://www.w3.org/2001/XMLSchema")]
		public XmlSchemaSimpleTypeContent Content
		{
			get{ return  content; } 
			set{ content = value; }
		}

		/// <remarks>
		/// For a simple Type:
		///		1. Content must be present
		///		2. id if present, must have be a valid ID
		///		a) If the simpletype is local
		///			1-	are from <xs:complexType name="simpleType"> and <xs:complexType name="localSimpleType">
		///			1. name  is prohibited
		///			2. final is prohibited
		///		b) If the simpletype is toplevel
		///			1-  are from <xs:complexType name="simpleType"> and <xs:complexType name="topLevelSimpleType">
		///			1. name is required, type must be NCName
		///			2. Content is required
		///			3. final can have values : #all | (list | union | restriction)
		///			4. If final is set, finalResolved is same as final (but within the values of b.3)
		///			5. If final is not set, the finalDefault of the schema (ie. only #all and restriction)
		///			6. Base type is:
		///				4.1 If restriction is chosen,the base type of restriction or elements
		///				4.2 otherwise simple ur-type
		/// </remarks>
		[MonoTODO]
		internal int Compile(ValidationEventHandler h, XmlSchemaInfo info)
		{
			errorCount = 0;

			if(this.islocal) // a
			{
				if(this.Name != null) // a.1
					error(h,"Name is prohibited in a local simpletype");
				if(this.Final != XmlSchemaDerivationMethod.None) //a.2
					error(h,"Final is prohibited in a local simpletype");
			}
			else //b
			{
				if(this.Name == null) //b.1
					error(h,"Name is required in top level simpletype");
				else if(!XmlSchemaUtil.CheckNCName(this.Name)) // b.1.2
					error(h,"name attribute of a simpleType must be NCName");
				else
					this.qName = new XmlQualifiedName(this.Name,info.targetNS);
				
				//NOTE: Although the FinalResolved can be Empty, it is not a valid value for Final
				//DEVIATION: If an error occurs, the finaldefault is always consulted. This deviates
				//			 from the way MS implementation works.
				switch(this.Final) //b.3, b.4
				{
						// Invalid values: Throw error and use "prohibited substitutions"
					case XmlSchemaDerivationMethod.Substitution:
						error(h,"substition is not a valid value for final in a simpletype");
						goto case XmlSchemaDerivationMethod.None;
					case XmlSchemaDerivationMethod.Extension:
						error(h,"extension is not a valid value for final in a simpletype");
						goto case XmlSchemaDerivationMethod.None;
					case XmlSchemaDerivationMethod.Empty:
						error(h,"empty is not a valid value for final in simpletype");
						goto case XmlSchemaDerivationMethod.None;
						//valid cases:
					case XmlSchemaDerivationMethod.All:
						this.finalResolved = XmlSchemaDerivationMethod.All;
						break;
					case XmlSchemaDerivationMethod.List:
						this.finalResolved = XmlSchemaDerivationMethod.List;
						break;
					case XmlSchemaDerivationMethod.Union:
						this.finalResolved = XmlSchemaDerivationMethod.Union;
						break;
					case XmlSchemaDerivationMethod.Restriction:
						this.finalResolved = XmlSchemaDerivationMethod.Restriction;
						break;
						// If mutliple values are specified
					default:
						error(h,"simpletype can't have more than one value for final");
						goto case XmlSchemaDerivationMethod.None;
						// use assignment from finaldefault on schema.
						// The possible values of finalDefault on schema are #all | List of (extension | restriction)
						// Of these, the only possible values for us are #all | restriction.
					case XmlSchemaDerivationMethod.None: // b.5
						if(info.finalDefault == XmlSchemaDerivationMethod.All)
							finalResolved = XmlSchemaDerivationMethod.All;
						else // Either Restriction or Empty
							finalResolved = info.finalDefault & XmlSchemaDerivationMethod.Restriction;
						break;
				}
			}

			if(this.Id != null && !XmlSchemaUtil.CheckID(this.Id))
				error(h,"id must be a valid ID");

			if(this.Content == null) //a.3,b.2
				error(h,"Content is required in a simpletype");
			else if(Content is XmlSchemaSimpleTypeRestriction)
			{
				errorCount += ((XmlSchemaSimpleTypeRestriction)Content).Compile(h,info);
			}
			else if(Content is XmlSchemaSimpleTypeList)
			{
				errorCount += ((XmlSchemaSimpleTypeList)Content).Compile(h,info);
			}
			else if(Content is XmlSchemaSimpleTypeUnion)
			{
				errorCount += ((XmlSchemaSimpleTypeUnion)Content).Compile(h,info);
			}
			return errorCount;
		}
		
		[MonoTODO]
		internal int Validate(ValidationEventHandler h)
		{
			return errorCount;
		}
		
		internal void error(ValidationEventHandler handle,string message)
		{
			this.errorCount++;
			ValidationHandler.RaiseValidationError(handle,this,message);
		}
	}
}
