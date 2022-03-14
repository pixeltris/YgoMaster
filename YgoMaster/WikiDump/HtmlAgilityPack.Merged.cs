#if WITH_WIKI_DUMPER
#pragma warning disable CS0105

#define FX40

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Text;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

#region

using System;
using System.Diagnostics;

#endregion
// ReSharper disable InconsistentNaming


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System;
using System.Diagnostics;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;


using System;
using System.IO;
using System.Text;


using System.Xml.XPath;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System.Xml;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

// ReSharper disable InconsistentNaming

using System;
using System.Xml.XPath;


// Description: Html Agility Pack - HTML Parsers, selectors, traversors, manupulators.
// Website & Documentation: http://html-agility-pack.net
// Forum & Issues: https://github.com/zzzprojects/html-agility-pack
// License: https://github.com/zzzprojects/html-agility-pack/blob/master/LICENSE
// More projects: http://www.zzzprojects.com/
// Copyright � ZZZ Projects Inc. 2014 - 2017. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
#pragma warning disable 0649

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
#if !NETSTANDARD
using System.Security.Permissions;
#else
using System.Linq;
#endif
using System.Text;
using System.Xml;
using Microsoft.Win32;
#if NET45 || NETSTANDARD
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
#endif

#if FX40 || FX45
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
#endif

#endregion


using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System.IO;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.IO;
using System.Text;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;


// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Collections.Generic;


using System;
using System.Collections.Generic;
using System.Text;


using System;
using System.Collections.Generic;
using System.Text;


using System;
using System.Collections.Generic;
using System.Text;



//////////////////////////////////////////////////////////////////////////
// File: crc32.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to compute CRC32.
    /// </summary>
    public class Crc32
    {
        #region Fields

        private uint _crc32;

        #endregion

        #region Static Members

        private static uint[] crc_32_tab = // CRC polynomial 0xedb88320 
            {
                0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f,
                0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
                0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2,
                0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
                0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
                0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
                0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
                0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
                0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423,
                0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106,
                0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
                0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d,
                0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
                0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
                0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7,
                0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
                0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa,
                0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
                0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
                0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84,
                0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
                0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
                0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
                0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e,
                0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55,
                0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28,
                0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
                0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f,
                0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
                0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
                0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
                0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69,
                0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
                0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc,
                0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693,
                0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
            };

        #endregion

        #region Properties

        internal uint CheckSum
        {
            get { return _crc32; }
            set { _crc32 = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compute a checksum for a given array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to compute the checksum for.</param>
        /// <returns>The computed checksum.</returns>
        public static uint CRC32Bytes(byte[] bytes)
        {
            uint oldcrc32;
            oldcrc32 = 0xFFFFFFFF;
            int len = bytes.Length;

            for (int i = 0; len > 0; i++)
            {
                --len;
                oldcrc32 = UPDC32(bytes[len], oldcrc32);
            }
            return ~oldcrc32;
        }

        /// <summary>
        /// Compute a checksum for a given string.
        /// </summary>
        /// <param name="text">The string to compute the checksum for.</param>
        /// <returns>The computed checksum.</returns>
        public static uint CRC32String(string text)
        {
            uint oldcrc32;
            oldcrc32 = 0xFFFFFFFF;
            int len = text.Length;
            ushort uCharVal;
            byte lowByte, hiByte;

            for (int i = 0; len > 0; i++)
            {
                --len;
                uCharVal = text[len];
                unchecked
                {
                    lowByte = (byte) (uCharVal & 0x00ff);
                    hiByte = (byte) (uCharVal >> 8);
                }
                oldcrc32 = UPDC32(hiByte, oldcrc32);
                oldcrc32 = UPDC32(lowByte, oldcrc32);
            }

            return ~oldcrc32;
        }

        #endregion

        #region Internal Methods

        internal uint AddToCRC32(int c)
        {
            return AddToCRC32((ushort) c);
        }

        internal uint AddToCRC32(ushort c)
        {
            byte lowByte, hiByte;
            lowByte = (byte) (c & 0x00ff);
            hiByte = (byte) (c >> 8);
            _crc32 = UPDC32(hiByte, _crc32);
            _crc32 = UPDC32(lowByte, _crc32);
            return ~_crc32;
        }

        #endregion

        #region Private Methods

        private static uint UPDC32(byte octet, uint crc)
        {
            return (crc_32_tab[((crc) ^ (octet)) & 0xff] ^ ((crc) >> 8));
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: EncodingFoundException.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class EncodingFoundException : Exception
    {
        #region Fields

        private Encoding _encoding;

        #endregion

        #region Constructors

        internal EncodingFoundException(Encoding encoding)
        {
            _encoding = encoding;
        }

        #endregion

        #region Properties

        internal Encoding Encoding
        {
            get { return _encoding; }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlAttribute.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an HTML attribute.
    /// </summary>
    [DebuggerDisplay("Name: {OriginalName}, Value: {Value}")]
    public class HtmlAttribute : IComparable
    {
        #region Fields

        private int _line;
        internal int _lineposition;
        internal string _name;
        internal int _namelength;
        internal int _namestartindex;
        internal HtmlDocument _ownerdocument; // attribute can exists without a node
        internal HtmlNode _ownernode;
        private AttributeValueQuote _quoteType = AttributeValueQuote.DoubleQuote;
        internal int _streamposition;
        internal string _value;
        internal int _valuelength;
        internal int _valuestartindex;

        #endregion

        #region Constructors

        internal HtmlAttribute(HtmlDocument ownerdocument)
        {
            _ownerdocument = ownerdocument;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the line number of this attribute in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the column number of this attribute in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the qualified name of the attribute.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = _ownerdocument.Text.Substring(_namestartindex, _namelength);
                }
                return _name.ToLower();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _name = value;
                if (_ownernode != null)
                {
                    _ownernode.SetChanged();
                }
            }
        }

        /// <summary>
        /// Name of attribute with original case
        /// </summary>
        public string OriginalName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the HTML document to which this attribute belongs.
        /// </summary>
        public HtmlDocument OwnerDocument
        {
            get { return _ownerdocument; }
        }

        /// <summary>
        /// Gets the HTML node to which this attribute belongs.
        /// </summary>
        public HtmlNode OwnerNode
        {
            get { return _ownernode; }
        }

        /// <summary>
        /// Specifies what type of quote the data should be wrapped in
        /// </summary>
        public AttributeValueQuote QuoteType
        {
            get { return _quoteType; }
            set { _quoteType = value; }
        }

        /// <summary>
        /// Gets the stream position of this attribute in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamposition; }
        }

        /// <summary>
        /// Gets or sets the value of the attribute.
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _ownerdocument.Text.Substring(_valuestartindex, _valuelength);
                }

                return _value;
            }
            set
            {
                _value = value;

                if (_ownernode != null)
                {
                    _ownernode.SetChanged();
                }
            }
        }

        /// <summary>
        /// Gets the DeEntitized value of the attribute.
        /// </summary>
        public string DeEntitizeValue
        {
            get
            {
                return HtmlEntity.DeEntitize(Value);
            }
        }

        internal string XmlName
        {
            get { return HtmlDocument.GetXmlName(Name); }
        }

        internal string XmlValue
        {
            get { return Value; }
        }

        /// <summary>
        /// Gets a valid XPath string that points to this Attribute
        /// </summary>
        public string XPath
        {
            get
            {
                string basePath = (OwnerNode == null) ? "/" : OwnerNode.XPath + "/";
                return basePath + GetRelativeXpath();
            }
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another attribute. Comparison is based on attributes' name.
        /// </summary>
        /// <param name="obj">An attribute to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the names comparison.</returns>
        public int CompareTo(object obj)
        {
            HtmlAttribute att = obj as HtmlAttribute;
            if (att == null)
            {
                throw new ArgumentException("obj");
            }
            return Name.CompareTo(att.Name);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a duplicate of this attribute.
        /// </summary>
        /// <returns>The cloned attribute.</returns>
        public HtmlAttribute Clone()
        {
            HtmlAttribute att = new HtmlAttribute(_ownerdocument);
            att.Name = Name;
            att.Value = Value;
            att.QuoteType = QuoteType;
            return att;
        }

        /// <summary>
        /// Removes this attribute from it's parents collection
        /// </summary>
        public void Remove()
        {
            _ownernode.Attributes.Remove(this);
        }

        #endregion

        #region Private Methods

        private string GetRelativeXpath()
        {
            if (OwnerNode == null)
                return Name;

            int i = 1;
            foreach (HtmlAttribute node in OwnerNode.Attributes)
            {
                if (node.Name != Name) continue;

                if (node == this)
                    break;

                i++;
            }
            return "@" + Name + "[" + i + "]";
        }

        #endregion
    }

    /// <summary>
    /// An Enum representing different types of Quotes used for surrounding attribute values
    /// </summary>
    public enum AttributeValueQuote
    {
        /// <summary>
        /// A single quote mark '
        /// </summary>
        SingleQuote,
        /// <summary>
        /// A double quote mark "
        /// </summary>
        DoubleQuote
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlAttributeCollection.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a combined list and collection of HTML nodes.
    /// </summary>
    public class HtmlAttributeCollection : IList<HtmlAttribute>
    {
        #region Fields

        internal Dictionary<string, HtmlAttribute> Hashitems = new Dictionary<string, HtmlAttribute>();
        private HtmlNode _ownernode;
        private List<HtmlAttribute> items = new List<HtmlAttribute>();

        #endregion

        #region Constructors

        internal HtmlAttributeCollection(HtmlNode ownernode)
        {
            _ownernode = ownernode;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a given attribute from the list using its name.
        /// </summary>
        public HtmlAttribute this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                HtmlAttribute value;
                return Hashitems.TryGetValue(name.ToLower(), out value) ? value : null;
            }
            set { Append(value); }
        }

        #endregion

        #region IList<HtmlAttribute> Members

        /// <summary>
        /// Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        /// Gets readonly status of colelction
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the attribute at the specified index.
        /// </summary>
        public HtmlAttribute this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        /// <summary>
        /// Adds supplied item to collection
        /// </summary>
        /// <param name="item"></param>
        public void Add(HtmlAttribute item)
        {
            Append(item);
        }

        /// <summary>
        /// Explicit clear
        /// </summary>
        void ICollection<HtmlAttribute>.Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Retreives existence of supplied item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlAttribute item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Copies collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlAttribute[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get Explicit enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<HtmlAttribute> IEnumerable<HtmlAttribute>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Explicit non-generic enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Retrieves the index for the supplied item, -1 if not found
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlAttribute item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// Inserts given item into collection at supplied index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, HtmlAttribute item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            Hashitems[item.Name] = item;
            item._ownernode = _ownernode;
            items.Insert(index, item);

            _ownernode.SetChanged();
        }

        /// <summary>
        /// Explicit collection remove
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool ICollection<HtmlAttribute>.Remove(HtmlAttribute item)
        {
            return items.Remove(item);
        }

        /// <summary>
        /// Removes the attribute at the specified index.
        /// </summary>
        /// <param name="index">The index of the attribute to remove.</param>
        public void RemoveAt(int index)
        {
            HtmlAttribute att = items[index];
            Hashitems.Remove(att.Name);
            items.RemoveAt(index);

            _ownernode.SetChanged();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new attribute to the collection with the given values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(string name, string value)
        {
            Append(name, value);
        }

        /// <summary>
        /// Inserts the specified attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="newAttribute">The attribute to insert. May not be null.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(HtmlAttribute newAttribute)
        {
            if (newAttribute == null)
            {
                throw new ArgumentNullException("newAttribute");
            }

            Hashitems[newAttribute.Name] = newAttribute;
            newAttribute._ownernode = _ownernode;
            items.Add(newAttribute);

            _ownernode.SetChanged();
            return newAttribute;
        }

        /// <summary>
        /// Creates and inserts a new attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="name">The name of the attribute to insert.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(string name)
        {
            HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name);
            return Append(att);
        }

        /// <summary>
        /// Creates and inserts a new attribute as the last attribute in the collection.
        /// </summary>
        /// <param name="name">The name of the attribute to insert.</param>
        /// <param name="value">The value of the attribute to insert.</param>
        /// <returns>The appended attribute.</returns>
        public HtmlAttribute Append(string name, string value)
        {
            HtmlAttribute att = _ownernode._ownerdocument.CreateAttribute(name, value);
            return Append(att);
        }

        /// <summary>
        /// Checks for existance of attribute with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name.Equals(name.ToLower()))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts the specified attribute as the first node in the collection.
        /// </summary>
        /// <param name="newAttribute">The attribute to insert. May not be null.</param>
        /// <returns>The prepended attribute.</returns>
        public HtmlAttribute Prepend(HtmlAttribute newAttribute)
        {
            Insert(0, newAttribute);
            return newAttribute;
        }

        /// <summary>
        /// Removes a given attribute from the list.
        /// </summary>
        /// <param name="attribute">The attribute to remove. May not be null.</param>
        public void Remove(HtmlAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            int index = GetAttributeIndex(attribute);
            if (index == -1)
            {
                throw new IndexOutOfRangeException();
            }
            RemoveAt(index);
        }

        /// <summary>
        /// Removes an attribute from the list, using its name. If there are more than one attributes with this name, they will all be removed.
        /// </summary>
        /// <param name="name">The attribute's name. May not be null.</param>
        public void Remove(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string lname = name.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                HtmlAttribute att = items[i];
                if (att.Name == lname)
                {
                    RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove all attributes in the list.
        /// </summary>
        public void RemoveAll()
        {
            Hashitems.Clear();
            items.Clear();

            _ownernode.SetChanged();
        }

        #endregion

        #region LINQ Methods

        /// <summary>
        /// Returns all attributes with specified name. Handles case insentivity
        /// </summary>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns></returns>
        public IEnumerable<HtmlAttribute> AttributesWithName(string attributeName)
        {
            attributeName = attributeName.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name.Equals(attributeName))
                    yield return items[i];
            }
        }

        /// <summary>
        /// Removes all attributes from the collection
        /// </summary>
        public void Remove()
        {
            items.Clear();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Clears the attribute collection
        /// </summary>
        internal void Clear()
        {
            Hashitems.Clear();
            items.Clear();
        }

        internal int GetAttributeIndex(HtmlAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            for (int i = 0; i < items.Count; i++)
            {
                if ((items[i]) == attribute)
                    return i;
            }
            return -1;
        }

        internal int GetAttributeIndex(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            string lname = name.ToLower();
            for (int i = 0; i < items.Count; i++)
            {
                if ((items[i]).Name == lname)
                    return i;
            }
            return -1;
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlCmdLine.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class HtmlCmdLine
    {
        #region Static Members

        internal static bool Help;

        #endregion

        #region Constructors

        static HtmlCmdLine()
        {
            Help = false;
            ParseArgs();
        }

        #endregion

        #region Internal Methods

        internal static string GetOption(string name, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetStringArg(args[i], name, ref p);
            }
            return p;
        }

        internal static string GetOption(int index, string def)
        {
            string p = def;
            string[] args = Environment.GetCommandLineArgs();
            int j = 0;
            for (int i = 1; i < args.Length; i++)
            {
                if (GetStringArg(args[i], ref p))
                {
                    if (index == j)
                        return p;
                    else
                        p = def;
                    j++;
                }
            }
            return p;
        }

        internal static bool GetOption(string name, bool def)
        {
            bool p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetBoolArg(args[i], name, ref p);
            }
            return p;
        }

        internal static int GetOption(string name, int def)
        {
            int p = def;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                GetIntArg(args[i], name, ref p);
            }
            return p;
        }

        #endregion

        #region Private Methods

        private static void GetBoolArg(string Arg, string Name, ref bool ArgValue)
        {
            if (Arg.Length < (Name.Length + 1)) // -name is 1 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
                ArgValue = true;
        }

        private static void GetIntArg(string Arg, string Name, ref int ArgValue)
        {
            if (Arg.Length < (Name.Length + 3)) // -name:12 is 3 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
            {
                try
                {
                    ArgValue = Convert.ToInt32(Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2));
                }
                catch
                {
                }
            }
        }

        private static bool GetStringArg(string Arg, ref string ArgValue)
        {
            if (('/' == Arg[0]) || ('-' == Arg[0]))
                return false;
            ArgValue = Arg;
            return true;
        }

        private static void GetStringArg(string Arg, string Name, ref string ArgValue)
        {
            if (Arg.Length < (Name.Length + 3)) // -name:x is 3 more than name
                return;
            if (('/' != Arg[0]) && ('-' != Arg[0])) // not a param
                return;
            if (Arg.Substring(1, Name.Length).ToLower() == Name.ToLower())
                ArgValue = Arg.Substring(Name.Length + 2, Arg.Length - Name.Length - 2);
        }

        private static void ParseArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 1; i < args.Length; i++)
            {
                // help
                GetBoolArg(args[i], "?", ref Help);
                GetBoolArg(args[i], "h", ref Help);
                GetBoolArg(args[i], "help", ref Help);
            }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlCommentNode.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an HTML comment.
    /// </summary>
    public class HtmlCommentNode : HtmlNode
    {
        #region Fields

        private string _comment;

        #endregion

        #region Constructors

        internal HtmlCommentNode(HtmlDocument ownerdocument, int index)
            :
                base(HtmlNodeType.Comment, ownerdocument, index)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the comment text of the node.
        /// </summary>
        public string Comment
        {
            get
            {
                if (_comment == null)
                {
                    return base.InnerHtml;
                }
                return _comment;
            }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
        /// </summary>
        public override string InnerHtml
        {
            get
            {
                if (_comment == null)
                {
                    return base.InnerHtml;
                }
                return _comment;
            }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or Sets the object and its content in HTML.
        /// </summary>
        public override string OuterHtml
        {
            get
            {
                if (_comment == null)
                {
                    return base.OuterHtml;
                }
                return "<!--" + _comment + "-->";
            }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlConsoleListener.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class HtmlConsoleListener : TraceListener
    {
#region Public Methods

        public override void Write(string Message)
        {
            Write(Message, "");
        }

        public override void Write(string Message, string Category)
        {
            Console.Write("T:" + Category + ": " + Message);
        }

        public override void WriteLine(string Message)
        {
            Write(Message + "\n");
        }

        public override void WriteLine(string Message, string Category)
        {
            Write(Message + "\n", Category);
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlDocument.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a complete HTML document.
    /// </summary>
    public partial class HtmlDocument
    {
        #region Manager

        public static bool DisableBehavaiorTagP;

        #endregion

        #region Fields

        /// <summary>
        /// Defines the max level we would go deep into the html document
        /// </summary>
        private static int _maxDepthLevel = int.MaxValue;

        private int _c;
        private Crc32 _crc32;
        private HtmlAttribute _currentattribute;
        private HtmlNode _currentnode;
        private Encoding _declaredencoding;
        private HtmlNode _documentnode;
        private bool _fullcomment;
        private int _index;
        internal Dictionary<string, HtmlNode> Lastnodes = new Dictionary<string, HtmlNode>();
        private HtmlNode _lastparentnode;
        private int _line;
        private int _lineposition, _maxlineposition;
        internal Dictionary<string, HtmlNode> Nodesid;
        private ParseState _oldstate;
        private bool _onlyDetectEncoding;
        internal Dictionary<int, HtmlNode> Openednodes;
        private List<HtmlParseError> _parseerrors = new List<HtmlParseError>();
        private string _remainder;
        private int _remainderOffset;
        private ParseState _state;
        private Encoding _streamencoding;
        internal string Text;

        // public props

        /// <summary>
        /// Adds Debugging attributes to node. Default is false.
        /// </summary>
        public bool OptionAddDebuggingAttributes;

        /// <summary>
        /// Defines if closing for non closed nodes must be done at the end or directly in the document.
        /// Setting this to true can actually change how browsers render the page. Default is false.
        /// </summary>
        public bool OptionAutoCloseOnEnd; // close errors at the end

        /// <summary>
        /// Defines if non closed nodes will be checked at the end of parsing. Default is true.
        /// </summary>
        public bool OptionCheckSyntax = true;

        /// <summary>
        /// Defines if a checksum must be computed for the document while parsing. Default is false.
        /// </summary>
        public bool OptionComputeChecksum;

        /// <summary>
		/// Defines if SelectNodes method will return null or empty collection when no node matched the XPath expression.
        /// Setting this to true will return empty collection and false will return null. Default is false.
		/// </summary>
		public bool OptionEmptyCollection = false;


        /// <summary>
        /// Defines the default stream encoding to use. Default is System.Text.Encoding.Default.
        /// </summary>
        public Encoding OptionDefaultStreamEncoding;

        /// <summary>
        /// Defines if source text must be extracted while parsing errors.
        /// If the document has a lot of errors, or cascading errors, parsing performance can be dramatically affected if set to true.
        /// Default is false.
        /// </summary>
        public bool OptionExtractErrorSourceText;

        // turning this on can dramatically slow performance if a lot of errors are detected

        /// <summary>
        /// Defines the maximum length of source text or parse errors. Default is 100.
        /// </summary>
        public int OptionExtractErrorSourceTextMaxLength = 100;

        /// <summary>
        /// Defines if LI, TR, TH, TD tags must be partially fixed when nesting errors are detected. Default is false.
        /// </summary>
        public bool OptionFixNestedTags; // fix li, tr, th, td tags

        /// <summary>
        /// Defines if output must conform to XML, instead of HTML.
        /// </summary>
        public bool OptionOutputAsXml;

        /// <summary>
        /// Defines if attribute value output must be optimized (not bound with double quotes if it is possible). Default is false.
        /// </summary>
        public bool OptionOutputOptimizeAttributeValues;

        /// <summary>
        /// Defines if name must be output with it's original case. Useful for asp.net tags and attributes
        /// </summary>
        public bool OptionOutputOriginalCase;

        /// <summary>
        /// Defines if name must be output in uppercase. Default is false.
        /// </summary>
        public bool OptionOutputUpperCase;

        /// <summary>
        /// Defines if declared encoding must be read from the document.
        /// Declared encoding is determined using the meta http-equiv="content-type" content="text/html;charset=XXXXX" html node.
        /// Default is true.
        /// </summary>
        public bool OptionReadEncoding = true;

        /// <summary>
        /// Defines the name of a node that will throw the StopperNodeException when found as an end node. Default is null.
        /// </summary>
        public string OptionStopperNodeName;

        /// <summary>
        /// Defines if the 'id' attribute must be specifically used. Default is true.
        /// </summary>
        public bool OptionUseIdAttribute = true;

        /// <summary>
        /// Defines if empty nodes must be written as closed during output. Default is false.
        /// </summary>
        public bool OptionWriteEmptyNodes;

        #endregion

        #region Static Members

        internal static readonly string HtmlExceptionRefNotChild = "Reference node must be a child of this node";

        internal static readonly string HtmlExceptionUseIdAttributeFalse = "You need to set UseIdAttribute property to true to enable this feature";

        internal static readonly string HtmlExceptionClassDoesNotExist = "Class name doesn't exist";

        internal static readonly string HtmlExceptionClassExists = "Class name already exists";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of an HTML document.
        /// </summary>
        public HtmlDocument()
        {
            _documentnode = CreateNode(HtmlNodeType.Document, 0);
#if SILVERLIGHT || METRO || NETSTANDARD
            OptionDefaultStreamEncoding = Encoding.UTF8;
#else
            OptionDefaultStreamEncoding = Encoding.Default;
#endif

        }

        #endregion

        #region Properties

        /// <summary>
        /// Defines the max level we would go deep into the html document. If this depth level is exceeded, and exception is
        /// thrown.
        /// </summary>
        public static int MaxDepthLevel
        {
            get { return _maxDepthLevel; }
            set { _maxDepthLevel = value; }
        }

        /// <summary>
        /// Gets the document CRC32 checksum if OptionComputeChecksum was set to true before parsing, 0 otherwise.
        /// </summary>
        public int CheckSum
        {
            get { return _crc32 == null ? 0 : (int) _crc32.CheckSum; }
        }

        /// <summary>
        /// Gets the document's declared encoding.
        /// Declared encoding is determined using the meta http-equiv="content-type" content="text/html;charset=XXXXX" html node.
        /// </summary>
        public Encoding DeclaredEncoding
        {
            get { return _declaredencoding; }
        }

        /// <summary>
        /// Gets the root node of the document.
        /// </summary>
        public HtmlNode DocumentNode
        {
            get { return _documentnode; }
        }

        /// <summary>
        /// Gets the document's output encoding.
        /// </summary>
        public Encoding Encoding
        {
            get { return GetOutEncoding(); }
        }

        /// <summary>
        /// Gets a list of parse errors found in the document.
        /// </summary>
        public IEnumerable<HtmlParseError> ParseErrors
        {
            get { return _parseerrors; }
        }

        /// <summary>
        /// Gets the remaining text.
        /// Will always be null if OptionStopperNodeName is null.
        /// </summary>
        public string Remainder
        {
            get { return _remainder; }
        }

        /// <summary>
        /// Gets the offset of Remainder in the original Html text.
        /// If OptionStopperNodeName is null, this will return the length of the original Html text.
        /// </summary>
        public int RemainderOffset
        {
            get { return _remainderOffset; }
        }

        /// <summary>
        /// Gets the document's stream encoding.
        /// </summary>
        public Encoding StreamEncoding
        {
            get { return _streamencoding; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a valid XML name.
        /// </summary>
        /// <param name="name">Any text.</param>
        /// <returns>A string that is a valid XML name.</returns>
        public static string GetXmlName(string name)
        {
            string xmlname = string.Empty;
            bool nameisok = true;
            for (int i = 0; i < name.Length; i++)
            {
                // names are lcase
                // note: we are very limited here, too much?
                if (((name[i] >= 'a') && (name[i] <= 'z')) ||
                    ((name[i] >= '0') && (name[i] <= '9')) ||
                    //					(name[i]==':') || (name[i]=='_') || (name[i]=='-') || (name[i]=='.')) // these are bads in fact
                    (name[i] == '_') || (name[i] == '-') || (name[i] == '.'))
                {
                    xmlname += name[i];
                }
                else
                {
                    nameisok = false;
                    byte[] bytes = Encoding.UTF8.GetBytes(new char[] {name[i]});
                    for (int j = 0; j < bytes.Length; j++)
                    {
                        xmlname += bytes[j].ToString("x2");
                    }
                    xmlname += "_";
                }
            }
            if (nameisok)
            {
                return xmlname;
            }
            return "_" + xmlname;
        }

        /// <summary>
        /// Applies HTML encoding to a specified string.
        /// </summary>
        /// <param name="html">The input string to encode. May not be null.</param>
        /// <returns>The encoded string.</returns>
        public static string HtmlEncode(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            // replace & by &amp; but only once!
            Regex rx = new Regex("&(?!(amp;)|(lt;)|(gt;)|(quot;))", RegexOptions.IgnoreCase);
            return rx.Replace(html, "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        /// <summary>
        /// Determines if the specified character is considered as a whitespace character.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>true if if the specified character is considered as a whitespace character.</returns>
        public static bool IsWhiteSpace(int c)
        {
            if ((c == 10) || (c == 13) || (c == 32) || (c == 9))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates an HTML attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute. May not be null.</param>
        /// <returns>The new HTML attribute.</returns>
        public HtmlAttribute CreateAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            HtmlAttribute att = CreateAttribute();
            att.Name = name;
            return att;
        }

        /// <summary>
        /// Creates an HTML attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the attribute. May not be null.</param>
        /// <param name="value">The value of the attribute.</param>
        /// <returns>The new HTML attribute.</returns>
        public HtmlAttribute CreateAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlAttribute att = CreateAttribute(name);
            att.Value = value;
            return att;
        }

        /// <summary>
        /// Creates an HTML comment node.
        /// </summary>
        /// <returns>The new HTML comment node.</returns>
        public HtmlCommentNode CreateComment()
        {
            return (HtmlCommentNode) CreateNode(HtmlNodeType.Comment);
        }

        /// <summary>
        /// Creates an HTML comment node with the specified comment text.
        /// </summary>
        /// <param name="comment">The comment text. May not be null.</param>
        /// <returns>The new HTML comment node.</returns>
        public HtmlCommentNode CreateComment(string comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException("comment");
            }
            HtmlCommentNode c = CreateComment();
            c.Comment = comment;
            return c;
        }

        /// <summary>
        /// Creates an HTML element node with the specified name.
        /// </summary>
        /// <param name="name">The qualified name of the element. May not be null.</param>
        /// <returns>The new HTML node.</returns>
        public HtmlNode CreateElement(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            HtmlNode node = CreateNode(HtmlNodeType.Element);
            node.Name = name;
            return node;
        }

        /// <summary>
        /// Creates an HTML text node.
        /// </summary>
        /// <returns>The new HTML text node.</returns>
        public HtmlTextNode CreateTextNode()
        {
            return (HtmlTextNode) CreateNode(HtmlNodeType.Text);
        }

        /// <summary>
        /// Creates an HTML text node with the specified text.
        /// </summary>
        /// <param name="text">The text of the node. May not be null.</param>
        /// <returns>The new HTML text node.</returns>
        public HtmlTextNode CreateTextNode(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            HtmlTextNode t = CreateTextNode();
            t.Text = text;
            return t;
        }

        /// <summary>
        /// Detects the encoding of an HTML stream.
        /// </summary>
        /// <param name="stream">The input stream. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            return DetectEncoding(new StreamReader(stream));
        }



        /// <summary>
        /// Detects the encoding of an HTML text provided on a TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _onlyDetectEncoding = true;
            if (OptionCheckSyntax)
            {
                Openednodes = new Dictionary<int, HtmlNode>();
            }
            else
            {
                Openednodes = null;
            }

            if (OptionUseIdAttribute)
            {
                Nodesid = new Dictionary<string, HtmlNode>();
            }
            else
            {
                Nodesid = null;
            }

            StreamReader sr = reader as StreamReader;
            if (sr != null)
            {
                Text = sr.ReadToEnd();
                _streamencoding = sr.CurrentEncoding;
                return _streamencoding;
            }

            _streamencoding = null;
            _declaredencoding = null;

            Text = reader.ReadToEnd();
            _documentnode = CreateNode(HtmlNodeType.Document, 0);

            // this is almost a hack, but it allows us not to muck with the original parsing code
            try
            {
                Parse();
            }
            catch (EncodingFoundException ex)
            {
                return ex.Encoding;
            }
            return _streamencoding;
        }





        /// <summary>
        /// Detects the encoding of an HTML text.
        /// </summary>
        /// <param name="html">The input html text. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncodingHtml(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            using (StringReader sr = new StringReader(html))
            {
                Encoding encoding = DetectEncoding(sr);
                return encoding;
            }
        }

        /// <summary>
        /// Gets the HTML node with the specified 'id' attribute value.
        /// </summary>
        /// <param name="id">The attribute id to match. May not be null.</param>
        /// <returns>The HTML node with the matching id or null if not found.</returns>
        public HtmlNode GetElementbyId(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            if (Nodesid == null)
            {
                throw new Exception(HtmlExceptionUseIdAttributeFalse);
            }
            return Nodesid.ContainsKey(id.ToLower()) ? Nodesid[id.ToLower()] : null;
        }

        /// <summary>
        /// Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public void Load(Stream stream)
        {
            Load(new StreamReader(stream, OptionDefaultStreamEncoding));
        }

        /// <summary>
        /// Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        /// Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(Stream stream, Encoding encoding)
        {
            Load(new StreamReader(stream, encoding));
        }

        /// <summary>
        /// Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        /// Loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
        }


        /// <summary>
        /// Loads the HTML document from the specified TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document. May not be null.</param>
        public void Load(TextReader reader)
        {
            // all Load methods pass down to this one
            if (reader == null)
                throw new ArgumentNullException("reader");

            _onlyDetectEncoding = false;

            if (OptionCheckSyntax)
                Openednodes = new Dictionary<int, HtmlNode>();
            else
                Openednodes = null;

            if (OptionUseIdAttribute)
            {
                Nodesid = new Dictionary<string, HtmlNode>();
            }
            else
            {
                Nodesid = null;
            }

            StreamReader sr = reader as StreamReader;
            if (sr != null)
            {
                try
                {
                    // trigger bom read if needed
                    sr.Peek();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
                    // ReSharper restore EmptyGeneralCatchClause
                {
                    // void on purpose
                }
                _streamencoding = sr.CurrentEncoding;
            }
            else
            {
                _streamencoding = null;
            }
            _declaredencoding = null;

            Text = reader.ReadToEnd();
            _documentnode = CreateNode(HtmlNodeType.Document, 0);
            Parse();

            if (!OptionCheckSyntax || Openednodes == null) return;
            foreach (HtmlNode node in Openednodes.Values)
            {
                if (!node._starttag) // already reported
                {
                    continue;
                }

                string html;
                if (OptionExtractErrorSourceText)
                {
                    html = node.OuterHtml;
                    if (html.Length > OptionExtractErrorSourceTextMaxLength)
                    {
                        html = html.Substring(0, OptionExtractErrorSourceTextMaxLength);
                    }
                }
                else
                {
                    html = string.Empty;
                }
                AddError(
                    HtmlParseErrorCode.TagNotClosed,
                    node._line, node._lineposition,
                    node._streamposition, html,
                    "End tag </" + node.Name + "> was not found");
            }

            // we don't need this anymore
            Openednodes.Clear();
        }

        /// <summary>
        /// Loads the HTML document from the specified string.
        /// </summary>
        /// <param name="html">String containing the HTML document to load. May not be null.</param>
        public void LoadHtml(string html)
        {
            if (html == null)
            {
                throw new ArgumentNullException("html");
            }
            using (StringReader sr = new StringReader(html))
            {
                Load(sr);
            }
        }

        /// <summary>
        /// Saves the HTML document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        public void Save(Stream outStream)
        {
            StreamWriter sw = new StreamWriter(outStream, GetOutEncoding());
            Save(sw);
        }

        /// <summary>
        /// Saves the HTML document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Save(Stream outStream, Encoding encoding)
        {
            if (outStream == null)
            {
                throw new ArgumentNullException("outStream");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            StreamWriter sw = new StreamWriter(outStream, encoding);
            Save(sw);
        }


        /// <summary>
        /// Saves the HTML document to the specified StreamWriter.
        /// </summary>
        /// <param name="writer">The StreamWriter to which you want to save.</param>
        public void Save(StreamWriter writer)
        {
            Save((TextWriter) writer);
        }

        /// <summary>
        /// Saves the HTML document to the specified TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save. May not be null.</param>
        public void Save(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            DocumentNode.WriteTo(writer);
            writer.Flush();
        }

        /// <summary>
        /// Saves the HTML document to the specified XmlWriter.
        /// </summary>
        /// <param name="writer">The XmlWriter to which you want to save.</param>
        public void Save(XmlWriter writer)
        {
            DocumentNode.WriteTo(writer);
            writer.Flush();
        }

        #endregion

        #region Internal Methods

        internal HtmlAttribute CreateAttribute()
        {
            return new HtmlAttribute(this);
        }

        internal HtmlNode CreateNode(HtmlNodeType type)
        {
            return CreateNode(type, -1);
        }

        internal HtmlNode CreateNode(HtmlNodeType type, int index)
        {
            switch (type)
            {
                case HtmlNodeType.Comment:
                    return new HtmlCommentNode(this, index);

                case HtmlNodeType.Text:
                    return new HtmlTextNode(this, index);

                default:
                    return new HtmlNode(type, this, index);
            }
        }

        internal Encoding GetOutEncoding()
        {
            // when unspecified, use the stream encoding first
            return _declaredencoding ?? (_streamencoding ?? OptionDefaultStreamEncoding);
        }

        internal HtmlNode GetXmlDeclaration()
        {
            if (!_documentnode.HasChildNodes)
                return null;

            foreach (HtmlNode node in _documentnode._childnodes)
                if (node.Name == "?xml") // it's ok, names are case sensitive
                    return node;

            return null;
        }

        internal void SetIdForNode(HtmlNode node, string id)
        {
            if (!OptionUseIdAttribute)
                return;

            if ((Nodesid == null) || (id == null))
                return;

            if (node == null)
                Nodesid.Remove(id.ToLower());
            else
                Nodesid[id.ToLower()] = node;
        }

        internal void UpdateLastParentNode()
        {
            do
            {
                if (_lastparentnode.Closed)
                    _lastparentnode = _lastparentnode.ParentNode;

            } while ((_lastparentnode != null) && (_lastparentnode.Closed));

            if (_lastparentnode == null)
                _lastparentnode = _documentnode;
        }

        #endregion

        #region Private Methods

        private void AddError(HtmlParseErrorCode code, int line, int linePosition, int streamPosition, string sourceText, string reason)
        {
            HtmlParseError err = new HtmlParseError(code, line, linePosition, streamPosition, sourceText, reason);
            _parseerrors.Add(err);
            return;
        }

        private void CloseCurrentNode()
        {
            if (_currentnode.Closed) // text or document are by def closed
                return;

            bool error = false;
            HtmlNode prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);

            // find last node of this kind
            if (prev == null)
            {
                if (HtmlNode.IsClosedElement(_currentnode.Name))
                {
                    // </br> will be seen as <br>
                    _currentnode.CloseNode(_currentnode);

                    // add to parent node
                    if (_lastparentnode != null)
                    {
                        HtmlNode foundNode = null;
                        Stack<HtmlNode> futureChild = new Stack<HtmlNode>();
                        for (HtmlNode node = _lastparentnode.LastChild; node != null; node = node.PreviousSibling)
                        {
                            if ((node.Name == _currentnode.Name) && (!node.HasChildNodes))
                            {
                                foundNode = node;
                                break;
                            }
                            futureChild.Push(node);
                        }
                        if (foundNode != null)
                        {
                            while (futureChild.Count != 0)
                            {
                                HtmlNode node = futureChild.Pop();
                                _lastparentnode.RemoveChild(node);
                                foundNode.AppendChild(node);
                            }
                        }
                        else
                        {
                            _lastparentnode.AppendChild(_currentnode);
                        }
                    }
                }
                else
                {
                    // node has no parent
                    // node is not a closed node

                    if (HtmlNode.CanOverlapElement(_currentnode.Name))
                    {
                        // this is a hack: add it as a text node
                        HtmlNode closenode = CreateNode(HtmlNodeType.Text, _currentnode._outerstartindex);
                        closenode._outerlength = _currentnode._outerlength;
                        ((HtmlTextNode) closenode).Text = ((HtmlTextNode) closenode).Text.ToLower();
                        if (_lastparentnode != null)
                        {
                            _lastparentnode.AppendChild(closenode);
                        }
                    }
                    else
                    {
                        if (HtmlNode.IsEmptyElement(_currentnode.Name))
                        {
                            AddError(
                                HtmlParseErrorCode.EndTagNotRequired,
                                _currentnode._line, _currentnode._lineposition,
                                _currentnode._streamposition, _currentnode.OuterHtml,
                                "End tag </" + _currentnode.Name + "> is not required");
                        }
                        else
                        {
                            // node cannot overlap, node is not empty
                            AddError(
                                HtmlParseErrorCode.TagNotOpened,
                                _currentnode._line, _currentnode._lineposition,
                                _currentnode._streamposition, _currentnode.OuterHtml,
                                "Start tag <" + _currentnode.Name + "> was not found");
                            error = true;
                        }
                    }
                }
            }
            else
            {


                if (OptionFixNestedTags)
                {
                    if (FindResetterNodes(prev, GetResetters(_currentnode.Name)))
                    {
                        AddError(
                            HtmlParseErrorCode.EndTagInvalidHere,
                            _currentnode._line, _currentnode._lineposition,
                            _currentnode._streamposition, _currentnode.OuterHtml,
                            "End tag </" + _currentnode.Name + "> invalid here");
                        error = true;
                    }
                }

                if (!error)
                {
                    Lastnodes[_currentnode.Name] = prev._prevwithsamename;
                    prev.CloseNode(_currentnode);
                }
            }


            // we close this node, get grandparent
            if (!error)
            {
                if ((_lastparentnode != null) &&
                    ((!HtmlNode.IsClosedElement(_currentnode.Name)) ||
                     (_currentnode._starttag)))
                {
                    UpdateLastParentNode();
                }
            }
        }

        private string CurrentNodeName()
        {
            return Text.Substring(_currentnode._namestartindex, _currentnode._namelength);
        }


        private void DecrementPosition()
        {
            _index--;
            if (_lineposition == 1)
            {
                _lineposition = _maxlineposition;
                _line--;
            }
            else
            {
                _lineposition--;
            }
        }

        private HtmlNode FindResetterNode(HtmlNode node, string name)
        {
            HtmlNode resetter = Utilities.GetDictionaryValueOrNull(Lastnodes, name);
            if (resetter == null)
                return null;

            if (resetter.Closed)
                return null;

            if (resetter._streamposition < node._streamposition)
            {
                return null;
            }

            return resetter;
        }

        private bool FindResetterNodes(HtmlNode node, string[] names)
        {
            if (names == null)
                return false;

            for (int i = 0; i < names.Length; i++)
            {
                if (FindResetterNode(node, names[i]) != null)
                    return true;
            }
            return false;
        }

        private void FixNestedTag(string name, string[] resetters)
        {
            if (resetters == null)
                return;

            HtmlNode prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);
            // if we find a previous unclosed same name node, without a resetter node between, we must close it
            if (prev == null || (Lastnodes[name].Closed)) return;
            // try to find a resetter node, if found, we do nothing
            if (FindResetterNodes(prev, resetters))
            {
                return;
            }

            // ok we need to close the prev now
            // create a fake closer node
            HtmlNode close = new HtmlNode(prev.NodeType, this, -1);
            close._endnode = close;
            prev.CloseNode(close);
        }

        private void FixNestedTags()
        {
            // we are only interested by start tags, not closing tags
            if (!_currentnode._starttag)
                return;

            string name = CurrentNodeName();
            FixNestedTag(name, GetResetters(name));
        }

        private string[] GetResetters(string name)
        {
            switch (name)
            {
                case "li":
                    return new string[] {"ul", "ol"};

                case "tr":
                    return new string[] {"table"};

                case "th":
                case "td":
                    return new string[] {"tr", "table"};

                default:
                    return null;
            }
        }

        private void IncrementPosition()
        {
            if (_crc32 != null)
            {
                // REVIEW: should we add some checksum code in DecrementPosition too?
                _crc32.AddToCRC32(_c);
            }

            _index++;
            _maxlineposition = _lineposition;
            if (_c == 10)
            {
                _lineposition = 1;
                _line++;
            }
            else
            {
                _lineposition++;
            }
        }

        private bool NewCheck()
        {
            if (_c != '<')
            {
                return false;
            }
            if (_index < Text.Length)
            {
                if (Text[_index] == '%')
                {
                    switch (_state)
                    {
                        case ParseState.AttributeAfterEquals:
                            PushAttributeValueStart(_index - 1);
                            break;

                        case ParseState.BetweenAttributes:
                            PushAttributeNameStart(_index - 1);
                            break;

                        case ParseState.WhichTag:
                            PushNodeNameStart(true, _index - 1);
                            _state = ParseState.Tag;
                            break;
                    }
                    _oldstate = _state;
                    _state = ParseState.ServerSideCode;
                    return true;
                }
            }

            if (!PushNodeEnd(_index - 1, true))
            {
                // stop parsing
                _index = Text.Length;
                return true;
            }
            _state = ParseState.WhichTag;
            if ((_index - 1) <= (Text.Length - 2))
            {
                if (Text[_index] == '!')
                {
                    PushNodeStart(HtmlNodeType.Comment, _index - 1);
                    PushNodeNameStart(true, _index);
                    PushNodeNameEnd(_index + 1);
                    _state = ParseState.Comment;
                    if (_index < (Text.Length - 2))
                    {
                        if ((Text[_index + 1] == '-') &&
                            (Text[_index + 2] == '-'))
                        {
                            _fullcomment = true;
                        }
                        else
                        {
                            _fullcomment = false;
                        }
                    }
                    return true;
                }
            }
            PushNodeStart(HtmlNodeType.Element, _index - 1);
            return true;
        }

        private void Parse()
        {
            int lastquote = 0;
            if (OptionComputeChecksum)
            {
                _crc32 = new Crc32();
            }

            Lastnodes = new Dictionary<string, HtmlNode>();
            _c = 0;
            _fullcomment = false;
            _parseerrors = new List<HtmlParseError>();
            _line = 1;
            _lineposition = 1;
            _maxlineposition = 1;

            _state = ParseState.Text;
            _oldstate = _state;
            _documentnode._innerlength = Text.Length;
            _documentnode._outerlength = Text.Length;
            _remainderOffset = Text.Length;

            _lastparentnode = _documentnode;
            _currentnode = CreateNode(HtmlNodeType.Text, 0);
            _currentattribute = null;

            _index = 0;
            PushNodeStart(HtmlNodeType.Text, 0);
            while (_index < Text.Length)
            {
                _c = Text[_index];
                IncrementPosition();

                switch (_state)
                {
                    case ParseState.Text:
                        if (NewCheck())
                            continue;
                        break;

                    case ParseState.WhichTag:
                        if (NewCheck())
                            continue;
                        if (_c == '/')
                        {
                            PushNodeNameStart(false, _index);
                        }
                        else
                        {
                            PushNodeNameStart(true, _index - 1);
                            DecrementPosition();
                        }
                        _state = ParseState.Tag;
                        break;

                    case ParseState.Tag:
                        if (NewCheck())
                            continue;
                        if (IsWhiteSpace(_c))
                        {
                            PushNodeNameEnd(_index - 1);
                            if (_state != ParseState.Tag)
                                continue;
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }
                        if (_c == '/')
                        {
                            PushNodeNameEnd(_index - 1);
                            if (_state != ParseState.Tag)
                                continue;
                            _state = ParseState.EmptyTag;
                            continue;
                        }
                        if (_c == '>')
                        {
                            PushNodeNameEnd(_index - 1);
                            if (_state != ParseState.Tag)
                                continue;
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            if (_state != ParseState.Tag)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                        }
                        break;

                    case ParseState.BetweenAttributes:
                        if (NewCheck())
                            continue;

                        if (IsWhiteSpace(_c))
                            continue;

                        if ((_c == '/') || (_c == '?'))
                        {
                            _state = ParseState.EmptyTag;
                            continue;
                        }

                        if (_c == '>')
                        {
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }

                            if (_state != ParseState.BetweenAttributes)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }

                        PushAttributeNameStart(_index - 1);
                        _state = ParseState.AttributeName;
                        break;

                    case ParseState.EmptyTag:
                        if (NewCheck())
                            continue;

                        if (_c == '>')
                        {
                            if (!PushNodeEnd(_index, true))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }

                            if (_state != ParseState.EmptyTag)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }

                        // we may end up in this state if attributes are incorrectly seperated
                        // by a /-character. If so, start parsing attribute-name immediately.
                        if (!IsWhiteSpace(_c))
                        {
                            // Just do nothing and push to next one!
                            DecrementPosition();
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }
                        else
                        {
                            _state = ParseState.BetweenAttributes;
                        }

                        break;

                    case ParseState.AttributeName:
                        if (NewCheck())
                            continue;

                        if (IsWhiteSpace(_c))
                        {
                            PushAttributeNameEnd(_index - 1);
                            _state = ParseState.AttributeBeforeEquals;
                            continue;
                        }
                        if (_c == '=')
                        {
                            PushAttributeNameEnd(_index - 1);
                            _state = ParseState.AttributeAfterEquals;
                            continue;
                        }
                        if (_c == '>')
                        {
                            PushAttributeNameEnd(_index - 1);
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            if (_state != ParseState.AttributeName)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        break;

                    case ParseState.AttributeBeforeEquals:
                        if (NewCheck())
                            continue;

                        if (IsWhiteSpace(_c))
                            continue;
                        if (_c == '>')
                        {
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            if (_state != ParseState.AttributeBeforeEquals)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        if (_c == '=')
                        {
                            _state = ParseState.AttributeAfterEquals;
                            continue;
                        }
                        // no equals, no whitespace, it's a new attrribute starting
                        _state = ParseState.BetweenAttributes;
                        DecrementPosition();
                        break;

                    case ParseState.AttributeAfterEquals:
                        if (NewCheck())
                            continue;

                        if (IsWhiteSpace(_c))
                            continue;

                        if ((_c == '\'') || (_c == '"'))
                        {
                            _state = ParseState.QuotedAttributeValue;
                            PushAttributeValueStart(_index, _c);
                            lastquote = _c;
                            continue;
                        }
                        if (_c == '>')
                        {
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            if (_state != ParseState.AttributeAfterEquals)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        PushAttributeValueStart(_index - 1);
                        _state = ParseState.AttributeValue;
                        break;

                    case ParseState.AttributeValue:
                        if (NewCheck())
                            continue;

                        if (IsWhiteSpace(_c))
                        {
                            PushAttributeValueEnd(_index - 1);
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }

                        if (_c == '>')
                        {
                            PushAttributeValueEnd(_index - 1);
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            if (_state != ParseState.AttributeValue)
                                continue;
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        break;

                    case ParseState.QuotedAttributeValue:
                        if (_c == lastquote)
                        {
                            PushAttributeValueEnd(_index - 1);
                            _state = ParseState.BetweenAttributes;
                            continue;
                        }
                        if (_c == '<')
                        {
                            if (_index < Text.Length)
                            {
                                if (Text[_index] == '%')
                                {
                                    _oldstate = _state;
                                    _state = ParseState.ServerSideCode;
                                    continue;
                                }
                            }
                        }
                        break;

                    case ParseState.Comment:
                        if (_c == '>')
                        {
                            if (_fullcomment)
                            {
                                if ((Text[_index - 2] != '-') ||
                                    (Text[_index - 3] != '-'))
                                {
                                    continue;
                                }
                            }
                            if (!PushNodeEnd(_index, false))
                            {
                                // stop parsing
                                _index = Text.Length;
                                break;
                            }
                            _state = ParseState.Text;
                            PushNodeStart(HtmlNodeType.Text, _index);
                            continue;
                        }
                        break;

                    case ParseState.ServerSideCode:
                        if (_c == '%')
                        {
                            if (_index < Text.Length)
                            {
                                if (Text[_index] == '>')
                                {
                                    switch (_oldstate)
                                    {
                                        case ParseState.AttributeAfterEquals:
                                            _state = ParseState.AttributeValue;
                                            break;

                                        case ParseState.BetweenAttributes:
                                            PushAttributeNameEnd(_index + 1);
                                            _state = ParseState.BetweenAttributes;
                                            break;

                                        default:
                                            _state = _oldstate;
                                            break;
                                    }
                                    IncrementPosition();
                                }
                            }
                        }
                        break;

                    case ParseState.PcData:
                        // look for </tag + 1 char

                        // check buffer end
                        if ((_currentnode._namelength + 3) <= (Text.Length - (_index - 1)))
                        {
                            if (string.Compare(Text.Substring(_index - 1, _currentnode._namelength + 2),
                                    "</" + _currentnode.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                int c = Text[_index - 1 + 2 + _currentnode.Name.Length];
                                if ((c == '>') || (IsWhiteSpace(c)))
                                {
                                    // add the script as a text node
                                    HtmlNode script = CreateNode(HtmlNodeType.Text,
                                        _currentnode._outerstartindex +
                                        _currentnode._outerlength);
                                    script._outerlength = _index - 1 - script._outerstartindex;
                                    _currentnode.AppendChild(script);


                                    PushNodeStart(HtmlNodeType.Element, _index - 1);
                                    PushNodeNameStart(false, _index - 1 + 2);
                                    _state = ParseState.Tag;
                                    IncrementPosition();
                                }
                            }
                        }
                        break;
                }
            }

            // finish the current work
            if (_currentnode._namestartindex > 0)
            {
                PushNodeNameEnd(_index);
            }
            PushNodeEnd(_index, false);

            // we don't need this anymore
            Lastnodes.Clear();
        }

        private void PushAttributeNameEnd(int index)
        {
            _currentattribute._namelength = index - _currentattribute._namestartindex;
            _currentnode.Attributes.Append(_currentattribute);
        }

        private void PushAttributeNameStart(int index)
        {
            _currentattribute = CreateAttribute();
            _currentattribute._namestartindex = index;
            _currentattribute.Line = _line;
            _currentattribute._lineposition = _lineposition;
            _currentattribute._streamposition = index;
        }

        private void PushAttributeValueEnd(int index)
        {
            _currentattribute._valuelength = index - _currentattribute._valuestartindex;
        }

        private void PushAttributeValueStart(int index)
        {
            PushAttributeValueStart(index, 0);
        }

        private void PushAttributeValueStart(int index, int quote)
        {
            _currentattribute._valuestartindex = index;
            if (quote == '\'')
                _currentattribute.QuoteType = AttributeValueQuote.SingleQuote;
        }

        private bool PushNodeEnd(int index, bool close)
        {
            _currentnode._outerlength = index - _currentnode._outerstartindex;

            if ((_currentnode._nodetype == HtmlNodeType.Text) ||
                (_currentnode._nodetype == HtmlNodeType.Comment))
            {
                // forget about void nodes
                if (_currentnode._outerlength > 0)
                {
                    _currentnode._innerlength = _currentnode._outerlength;
                    _currentnode._innerstartindex = _currentnode._outerstartindex;
                    if (_lastparentnode != null)
                    {
                        _lastparentnode.AppendChild(_currentnode);
                    }
                }
            }
            else
            {
                if ((_currentnode._starttag) && (_lastparentnode != _currentnode))
                {
                    // add to parent node
                    if (_lastparentnode != null)
                    {
                        _lastparentnode.AppendChild(_currentnode);
                    }

                    ReadDocumentEncoding(_currentnode);

                    // remember last node of this kind
                    HtmlNode prev = Utilities.GetDictionaryValueOrNull(Lastnodes, _currentnode.Name);

                    _currentnode._prevwithsamename = prev;
                    Lastnodes[_currentnode.Name] = _currentnode;

                    // change parent?
                    if ((_currentnode.NodeType == HtmlNodeType.Document) ||
                        (_currentnode.NodeType == HtmlNodeType.Element))
                    {
                        _lastparentnode = _currentnode;
                    }

                    if (HtmlNode.IsCDataElement(CurrentNodeName()))
                    {
                        _state = ParseState.PcData;
                        return true;
                    }

                    if ((HtmlNode.IsClosedElement(_currentnode.Name)) ||
                        (HtmlNode.IsEmptyElement(_currentnode.Name)))
                    {
                        close = true;
                    }
                }
            }

            if ((close) || (!_currentnode._starttag))
            {
                if ((OptionStopperNodeName != null) && (_remainder == null) &&
                    (string.Compare(_currentnode.Name, OptionStopperNodeName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    _remainderOffset = index;
                    _remainder = Text.Substring(_remainderOffset);
                    CloseCurrentNode();
                    return false; // stop parsing
                }
                CloseCurrentNode();
            }
            return true;
        }

        private void PushNodeNameEnd(int index)
        {
            _currentnode._namelength = index - _currentnode._namestartindex;
            if (OptionFixNestedTags)
            {
                FixNestedTags();
            }
        }

        private void PushNodeNameStart(bool starttag, int index)
        {
            _currentnode._starttag = starttag;
            _currentnode._namestartindex = index;
        }

        private void PushNodeStart(HtmlNodeType type, int index)
        {
            _currentnode = CreateNode(type, index);
            _currentnode._line = _line;
            _currentnode._lineposition = _lineposition;
            if (type == HtmlNodeType.Element)
            {
                _currentnode._lineposition--;
            }
            _currentnode._streamposition = index;
        }

        private void ReadDocumentEncoding(HtmlNode node)
        {
            if (!OptionReadEncoding)
                return;
            // format is 
            // <meta http-equiv="content-type" content="text/html;charset=iso-8859-1" />

            // when we append a child, we are in node end, so attributes are already populated
            if (node._namelength != 4) // quick check, avoids string alloc
                return;
            if (node.Name != "meta") // all nodes names are lowercase
                return;
            HtmlAttribute att = node.Attributes["http-equiv"];
            if (att == null)
                return;
            if (string.Compare(att.Value, "content-type", StringComparison.OrdinalIgnoreCase) != 0)
                return;
            HtmlAttribute content = node.Attributes["content"];
            if (content != null)
            {
                string charset = NameValuePairList.GetNameValuePairsValue(content.Value, "charset");
                if (!string.IsNullOrEmpty(charset))
                {
                    // The following check fixes the the bug described at: http://htmlagilitypack.codeplex.com/WorkItem/View.aspx?WorkItemId=25273
                    if (string.Equals(charset, "utf8", StringComparison.OrdinalIgnoreCase))
                        charset = "utf-8";
                    try
                    {
                        _declaredencoding = Encoding.GetEncoding(charset);
                    }
                    catch (ArgumentException)
                    {
                        _declaredencoding = null;
                    }
                    if (_onlyDetectEncoding)
                    {
                        throw new EncodingFoundException(_declaredencoding);
                    }

                    if (_streamencoding != null)
                    {
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD
						if (_declaredencoding.WebName != _streamencoding.WebName)
#else
                        if (_declaredencoding != null)
                            if (_declaredencoding.CodePage != _streamencoding.CodePage)
#endif
                            {
                                AddError(
                                    HtmlParseErrorCode.CharsetMismatch,
                                    _line, _lineposition,
                                    _index, node.OuterHtml,
                                    "Encoding mismatch between StreamEncoding: " +
                                    _streamencoding.WebName + " and DeclaredEncoding: " +
                                    _declaredencoding.WebName);
                            }
                    }
                }




            }
        }

        #endregion

        #region Nested type: ParseState

        private enum ParseState
        {
            Text,
            WhichTag,
            Tag,
            BetweenAttributes,
            EmptyTag,
            AttributeName,
            AttributeBeforeEquals,
            AttributeAfterEquals,
            AttributeValue,
            Comment,
            QuotedAttributeValue,
            ServerSideCode,
            PcData
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlDocument.PathMethods.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    public partial class HtmlDocument
    {
        /// <summary>
        /// Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public void DetectEncodingAndLoad(string path)
        {
            DetectEncodingAndLoad(path, true);
        }

        /// <summary>
        /// Detects the encoding of an HTML document from a file first, and then loads the file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncoding">true to detect encoding, false otherwise.</param>
        public void DetectEncodingAndLoad(string path, bool detectEncoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            Encoding enc;
            if (detectEncoding)
            {
                enc = DetectEncoding(path);
            }
            else
            {
                enc = null;
            }

            if (enc == null)
            {
                Load(path);
            }
            else
            {
                Load(path, enc);
            }
        }
        /// <summary>
        /// Detects the encoding of an HTML file.
        /// </summary>
        /// <param name="path">Path for the file containing the HTML document to detect. May not be null.</param>
        /// <returns>The detected encoding.</returns>
        public Encoding DetectEncoding(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
#else
            using (StreamReader sr = new StreamReader(path, OptionDefaultStreamEncoding))
#endif
            {
                Encoding encoding = DetectEncoding(sr);
                return encoding;
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        public void Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), OptionDefaultStreamEncoding))
#else
            using(StreamReader sr = new StreamReader(path, OptionDefaultStreamEncoding))
#endif
            {
                Load(sr);
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
                throw new ArgumentNullException("path");

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), detectEncodingFromByteOrderMarks))
#else
           using(StreamReader sr = new StreamReader(path, detectEncodingFromByteOrderMarks))
#endif
            {
               Load(sr);
           }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Load(string path, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding))
#else
            using(StreamReader sr = new StreamReader(path, encoding))
#endif
            {
                Load(sr);
            }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks))
#else
           using(StreamReader sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks))
#endif
            {
               Load(sr);
           }
        }

        /// <summary>
        /// Loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (encoding == null)
                throw new ArgumentNullException("encoding");

#if NETSTANDARD
            using (StreamReader sr = new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks, buffersize))

#else
           using(StreamReader sr = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize))
#endif
            {
               Load(sr);
           }
        }
        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        public void Save(string filename)
        {
#if NETSTANDARD
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(filename), GetOutEncoding()))
#else
            using(StreamWriter sw = new StreamWriter(filename, false, GetOutEncoding()))
#endif
            {
                Save(sw);
            }
        }

        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document. May not be null.</param>
        /// <param name="encoding">The character encoding to use. May not be null.</param>
        public void Save(string filename, Encoding encoding)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
#if NETSTANDARD
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(filename), encoding))
#else
            using (StreamWriter sw = new StreamWriter(filename, false, encoding))
#endif
            {
                Save(sw);
            }
        }

    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlDocument.Xpath.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    public partial class HtmlDocument : IXPathNavigable
    {
        /// <summary>
        /// Creates a new XPathNavigator object for navigating this HTML document.
        /// </summary>
        /// <returns>An XPathNavigator object. The XPathNavigator is positioned on the root of the document.</returns>
        public XPathNavigator CreateNavigator()
        {
            return new HtmlNodeNavigator(this, _documentnode);
        }
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlElementFlag.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Flags that describe the behavior of an Element node.
    /// </summary>
    [Flags]
    public enum HtmlElementFlag
    {
        /// <summary>
        /// The node is a CDATA node.
        /// </summary>
        CData = 1,

        /// <summary>
        /// The node is empty. META or IMG are example of such nodes.
        /// </summary>
        Empty = 2,

        /// <summary>
        /// The node will automatically be closed during parsing.
        /// </summary>
        Closed = 4,

        /// <summary>
        /// The node can overlap.
        /// </summary>
        CanOverlap = 8
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlEntity.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to replace special characters by entities and vice-versa.
    /// Follows HTML 4.0 specification found at http://www.w3.org/TR/html4/sgml/entities.html
    /// </summary>
    public class HtmlEntity
    {
        #region Static Members

        private static readonly int _maxEntitySize;
        private static Dictionary<int,string> _entityName;
        private static Dictionary<string, int> _entityValue;

        /// <summary>
        /// A collection of entities indexed by name.
        /// </summary>
        public static Dictionary<int, string> EntityName
        {
            get { return _entityName; }
        }

        /// <summary>
        /// A collection of entities indexed by value.
        /// </summary>
        public static Dictionary<string, int> EntityValue
        {
            get { return _entityValue; }
        }

        #endregion

        #region Constructors

        static HtmlEntity()
        {
            _entityName = new Dictionary<int, string>();
            _entityValue = new Dictionary<string, int>();

            #region Entities Definition

            _entityValue.Add("nbsp", 160); // no-break space = non-breaking space, U+00A0 ISOnum 
            _entityName.Add(160, "nbsp");
            _entityValue.Add("iexcl", 161); // inverted exclamation mark, U+00A1 ISOnum 
            _entityName.Add(161, "iexcl");
            _entityValue.Add("cent", 162); // cent sign, U+00A2 ISOnum 
            _entityName.Add(162, "cent");
            _entityValue.Add("pound", 163); // pound sign, U+00A3 ISOnum 
            _entityName.Add(163, "pound");
            _entityValue.Add("curren", 164); // currency sign, U+00A4 ISOnum 
            _entityName.Add(164, "curren");
            _entityValue.Add("yen", 165); // yen sign = yuan sign, U+00A5 ISOnum 
            _entityName.Add(165, "yen");
            _entityValue.Add("brvbar", 166); // broken bar = broken vertical bar, U+00A6 ISOnum 
            _entityName.Add(166, "brvbar");
            _entityValue.Add("sect", 167); // section sign, U+00A7 ISOnum 
            _entityName.Add(167, "sect");
            _entityValue.Add("uml", 168); // diaeresis = spacing diaeresis, U+00A8 ISOdia 
            _entityName.Add(168, "uml");
            _entityValue.Add("copy", 169); // copyright sign, U+00A9 ISOnum 
            _entityName.Add(169, "copy");
            _entityValue.Add("ordf", 170); // feminine ordinal indicator, U+00AA ISOnum 
            _entityName.Add(170, "ordf");
            _entityValue.Add("laquo", 171);
                // left-pointing double angle quotation mark = left pointing guillemet, U+00AB ISOnum 
            _entityName.Add(171, "laquo");
            _entityValue.Add("not", 172); // not sign, U+00AC ISOnum 
            _entityName.Add(172, "not");
            _entityValue.Add("shy", 173); // soft hyphen = discretionary hyphen, U+00AD ISOnum 
            _entityName.Add(173, "shy");
            _entityValue.Add("reg", 174); // registered sign = registered trade mark sign, U+00AE ISOnum 
            _entityName.Add(174, "reg");
            _entityValue.Add("macr", 175); // macron = spacing macron = overline = APL overbar, U+00AF ISOdia 
            _entityName.Add(175, "macr");
            _entityValue.Add("deg", 176); // degree sign, U+00B0 ISOnum 
            _entityName.Add(176, "deg");
            _entityValue.Add("plusmn", 177); // plus-minus sign = plus-or-minus sign, U+00B1 ISOnum 
            _entityName.Add(177, "plusmn");
            _entityValue.Add("sup2", 178); // superscript two = superscript digit two = squared, U+00B2 ISOnum 
            _entityName.Add(178, "sup2");
            _entityValue.Add("sup3", 179); // superscript three = superscript digit three = cubed, U+00B3 ISOnum 
            _entityName.Add(179, "sup3");
            _entityValue.Add("acute", 180); // acute accent = spacing acute, U+00B4 ISOdia 
            _entityName.Add(180, "acute");
            _entityValue.Add("micro", 181); // micro sign, U+00B5 ISOnum 
            _entityName.Add(181, "micro");
            _entityValue.Add("para", 182); // pilcrow sign = paragraph sign, U+00B6 ISOnum 
            _entityName.Add(182, "para");
            _entityValue.Add("middot", 183); // middle dot = Georgian comma = Greek middle dot, U+00B7 ISOnum 
            _entityName.Add(183, "middot");
            _entityValue.Add("cedil", 184); // cedilla = spacing cedilla, U+00B8 ISOdia 
            _entityName.Add(184, "cedil");
            _entityValue.Add("sup1", 185); // superscript one = superscript digit one, U+00B9 ISOnum 
            _entityName.Add(185, "sup1");
            _entityValue.Add("ordm", 186); // masculine ordinal indicator, U+00BA ISOnum 
            _entityName.Add(186, "ordm");
            _entityValue.Add("raquo", 187);
                // right-pointing double angle quotation mark = right pointing guillemet, U+00BB ISOnum 
            _entityName.Add(187, "raquo");
            _entityValue.Add("frac14", 188); // vulgar fraction one quarter = fraction one quarter, U+00BC ISOnum 
            _entityName.Add(188, "frac14");
            _entityValue.Add("frac12", 189); // vulgar fraction one half = fraction one half, U+00BD ISOnum 
            _entityName.Add(189, "frac12");
            _entityValue.Add("frac34", 190); // vulgar fraction three quarters = fraction three quarters, U+00BE ISOnum 
            _entityName.Add(190, "frac34");
            _entityValue.Add("iquest", 191); // inverted question mark = turned question mark, U+00BF ISOnum 
            _entityName.Add(191, "iquest");
            _entityValue.Add("Agrave", 192);
                // latin capital letter A with grave = latin capital letter A grave, U+00C0 ISOlat1 
            _entityName.Add(192, "Agrave");
            _entityValue.Add("Aacute", 193); // latin capital letter A with acute, U+00C1 ISOlat1 
            _entityName.Add(193, "Aacute");
            _entityValue.Add("Acirc", 194); // latin capital letter A with circumflex, U+00C2 ISOlat1 
            _entityName.Add(194, "Acirc");
            _entityValue.Add("Atilde", 195); // latin capital letter A with tilde, U+00C3 ISOlat1 
            _entityName.Add(195, "Atilde");
            _entityValue.Add("Auml", 196); // latin capital letter A with diaeresis, U+00C4 ISOlat1 
            _entityName.Add(196, "Auml");
            _entityValue.Add("Aring", 197);
                // latin capital letter A with ring above = latin capital letter A ring, U+00C5 ISOlat1 
            _entityName.Add(197, "Aring");
            _entityValue.Add("AElig", 198); // latin capital letter AE = latin capital ligature AE, U+00C6 ISOlat1 
            _entityName.Add(198, "AElig");
            _entityValue.Add("Ccedil", 199); // latin capital letter C with cedilla, U+00C7 ISOlat1 
            _entityName.Add(199, "Ccedil");
            _entityValue.Add("Egrave", 200); // latin capital letter E with grave, U+00C8 ISOlat1 
            _entityName.Add(200, "Egrave");
            _entityValue.Add("Eacute", 201); // latin capital letter E with acute, U+00C9 ISOlat1 
            _entityName.Add(201, "Eacute");
            _entityValue.Add("Ecirc", 202); // latin capital letter E with circumflex, U+00CA ISOlat1 
            _entityName.Add(202, "Ecirc");
            _entityValue.Add("Euml", 203); // latin capital letter E with diaeresis, U+00CB ISOlat1 
            _entityName.Add(203, "Euml");
            _entityValue.Add("Igrave", 204); // latin capital letter I with grave, U+00CC ISOlat1 
            _entityName.Add(204, "Igrave");
            _entityValue.Add("Iacute", 205); // latin capital letter I with acute, U+00CD ISOlat1 
            _entityName.Add(205, "Iacute");
            _entityValue.Add("Icirc", 206); // latin capital letter I with circumflex, U+00CE ISOlat1 
            _entityName.Add(206, "Icirc");
            _entityValue.Add("Iuml", 207); // latin capital letter I with diaeresis, U+00CF ISOlat1 
            _entityName.Add(207, "Iuml");
            _entityValue.Add("ETH", 208); // latin capital letter ETH, U+00D0 ISOlat1 
            _entityName.Add(208, "ETH");
            _entityValue.Add("Ntilde", 209); // latin capital letter N with tilde, U+00D1 ISOlat1 
            _entityName.Add(209, "Ntilde");
            _entityValue.Add("Ograve", 210); // latin capital letter O with grave, U+00D2 ISOlat1 
            _entityName.Add(210, "Ograve");
            _entityValue.Add("Oacute", 211); // latin capital letter O with acute, U+00D3 ISOlat1 
            _entityName.Add(211, "Oacute");
            _entityValue.Add("Ocirc", 212); // latin capital letter O with circumflex, U+00D4 ISOlat1 
            _entityName.Add(212, "Ocirc");
            _entityValue.Add("Otilde", 213); // latin capital letter O with tilde, U+00D5 ISOlat1 
            _entityName.Add(213, "Otilde");
            _entityValue.Add("Ouml", 214); // latin capital letter O with diaeresis, U+00D6 ISOlat1 
            _entityName.Add(214, "Ouml");
            _entityValue.Add("times", 215); // multiplication sign, U+00D7 ISOnum 
            _entityName.Add(215, "times");
            _entityValue.Add("Oslash", 216);
                // latin capital letter O with stroke = latin capital letter O slash, U+00D8 ISOlat1 
            _entityName.Add(216, "Oslash");
            _entityValue.Add("Ugrave", 217); // latin capital letter U with grave, U+00D9 ISOlat1 
            _entityName.Add(217, "Ugrave");
            _entityValue.Add("Uacute", 218); // latin capital letter U with acute, U+00DA ISOlat1 
            _entityName.Add(218, "Uacute");
            _entityValue.Add("Ucirc", 219); // latin capital letter U with circumflex, U+00DB ISOlat1 
            _entityName.Add(219, "Ucirc");
            _entityValue.Add("Uuml", 220); // latin capital letter U with diaeresis, U+00DC ISOlat1 
            _entityName.Add(220, "Uuml");
            _entityValue.Add("Yacute", 221); // latin capital letter Y with acute, U+00DD ISOlat1 
            _entityName.Add(221, "Yacute");
            _entityValue.Add("THORN", 222); // latin capital letter THORN, U+00DE ISOlat1 
            _entityName.Add(222, "THORN");
            _entityValue.Add("szlig", 223); // latin small letter sharp s = ess-zed, U+00DF ISOlat1 
            _entityName.Add(223, "szlig");
            _entityValue.Add("agrave", 224);
                // latin small letter a with grave = latin small letter a grave, U+00E0 ISOlat1 
            _entityName.Add(224, "agrave");
            _entityValue.Add("aacute", 225); // latin small letter a with acute, U+00E1 ISOlat1 
            _entityName.Add(225, "aacute");
            _entityValue.Add("acirc", 226); // latin small letter a with circumflex, U+00E2 ISOlat1 
            _entityName.Add(226, "acirc");
            _entityValue.Add("atilde", 227); // latin small letter a with tilde, U+00E3 ISOlat1 
            _entityName.Add(227, "atilde");
            _entityValue.Add("auml", 228); // latin small letter a with diaeresis, U+00E4 ISOlat1 
            _entityName.Add(228, "auml");
            _entityValue.Add("aring", 229);
                // latin small letter a with ring above = latin small letter a ring, U+00E5 ISOlat1 
            _entityName.Add(229, "aring");
            _entityValue.Add("aelig", 230); // latin small letter ae = latin small ligature ae, U+00E6 ISOlat1 
            _entityName.Add(230, "aelig");
            _entityValue.Add("ccedil", 231); // latin small letter c with cedilla, U+00E7 ISOlat1 
            _entityName.Add(231, "ccedil");
            _entityValue.Add("egrave", 232); // latin small letter e with grave, U+00E8 ISOlat1 
            _entityName.Add(232, "egrave");
            _entityValue.Add("eacute", 233); // latin small letter e with acute, U+00E9 ISOlat1 
            _entityName.Add(233, "eacute");
            _entityValue.Add("ecirc", 234); // latin small letter e with circumflex, U+00EA ISOlat1 
            _entityName.Add(234, "ecirc");
            _entityValue.Add("euml", 235); // latin small letter e with diaeresis, U+00EB ISOlat1 
            _entityName.Add(235, "euml");
            _entityValue.Add("igrave", 236); // latin small letter i with grave, U+00EC ISOlat1 
            _entityName.Add(236, "igrave");
            _entityValue.Add("iacute", 237); // latin small letter i with acute, U+00ED ISOlat1 
            _entityName.Add(237, "iacute");
            _entityValue.Add("icirc", 238); // latin small letter i with circumflex, U+00EE ISOlat1 
            _entityName.Add(238, "icirc");
            _entityValue.Add("iuml", 239); // latin small letter i with diaeresis, U+00EF ISOlat1 
            _entityName.Add(239, "iuml");
            _entityValue.Add("eth", 240); // latin small letter eth, U+00F0 ISOlat1 
            _entityName.Add(240, "eth");
            _entityValue.Add("ntilde", 241); // latin small letter n with tilde, U+00F1 ISOlat1 
            _entityName.Add(241, "ntilde");
            _entityValue.Add("ograve", 242); // latin small letter o with grave, U+00F2 ISOlat1 
            _entityName.Add(242, "ograve");
            _entityValue.Add("oacute", 243); // latin small letter o with acute, U+00F3 ISOlat1 
            _entityName.Add(243, "oacute");
            _entityValue.Add("ocirc", 244); // latin small letter o with circumflex, U+00F4 ISOlat1 
            _entityName.Add(244, "ocirc");
            _entityValue.Add("otilde", 245); // latin small letter o with tilde, U+00F5 ISOlat1 
            _entityName.Add(245, "otilde");
            _entityValue.Add("ouml", 246); // latin small letter o with diaeresis, U+00F6 ISOlat1 
            _entityName.Add(246, "ouml");
            _entityValue.Add("divide", 247); // division sign, U+00F7 ISOnum 
            _entityName.Add(247, "divide");
            _entityValue.Add("oslash", 248);
                // latin small letter o with stroke, = latin small letter o slash, U+00F8 ISOlat1 
            _entityName.Add(248, "oslash");
            _entityValue.Add("ugrave", 249); // latin small letter u with grave, U+00F9 ISOlat1 
            _entityName.Add(249, "ugrave");
            _entityValue.Add("uacute", 250); // latin small letter u with acute, U+00FA ISOlat1 
            _entityName.Add(250, "uacute");
            _entityValue.Add("ucirc", 251); // latin small letter u with circumflex, U+00FB ISOlat1 
            _entityName.Add(251, "ucirc");
            _entityValue.Add("uuml", 252); // latin small letter u with diaeresis, U+00FC ISOlat1 
            _entityName.Add(252, "uuml");
            _entityValue.Add("yacute", 253); // latin small letter y with acute, U+00FD ISOlat1 
            _entityName.Add(253, "yacute");
            _entityValue.Add("thorn", 254); // latin small letter thorn, U+00FE ISOlat1 
            _entityName.Add(254, "thorn");
            _entityValue.Add("yuml", 255); // latin small letter y with diaeresis, U+00FF ISOlat1 
            _entityName.Add(255, "yuml");
            _entityValue.Add("fnof", 402); // latin small f with hook = function = florin, U+0192 ISOtech 
            _entityName.Add(402, "fnof");
            _entityValue.Add("Alpha", 913); // greek capital letter alpha, U+0391 
            _entityName.Add(913, "Alpha");
            _entityValue.Add("Beta", 914); // greek capital letter beta, U+0392 
            _entityName.Add(914, "Beta");
            _entityValue.Add("Gamma", 915); // greek capital letter gamma, U+0393 ISOgrk3 
            _entityName.Add(915, "Gamma");
            _entityValue.Add("Delta", 916); // greek capital letter delta, U+0394 ISOgrk3 
            _entityName.Add(916, "Delta");
            _entityValue.Add("Epsilon", 917); // greek capital letter epsilon, U+0395 
            _entityName.Add(917, "Epsilon");
            _entityValue.Add("Zeta", 918); // greek capital letter zeta, U+0396 
            _entityName.Add(918, "Zeta");
            _entityValue.Add("Eta", 919); // greek capital letter eta, U+0397 
            _entityName.Add(919, "Eta");
            _entityValue.Add("Theta", 920); // greek capital letter theta, U+0398 ISOgrk3 
            _entityName.Add(920, "Theta");
            _entityValue.Add("Iota", 921); // greek capital letter iota, U+0399 
            _entityName.Add(921, "Iota");
            _entityValue.Add("Kappa", 922); // greek capital letter kappa, U+039A 
            _entityName.Add(922, "Kappa");
            _entityValue.Add("Lambda", 923); // greek capital letter lambda, U+039B ISOgrk3 
            _entityName.Add(923, "Lambda");
            _entityValue.Add("Mu", 924); // greek capital letter mu, U+039C 
            _entityName.Add(924, "Mu");
            _entityValue.Add("Nu", 925); // greek capital letter nu, U+039D 
            _entityName.Add(925, "Nu");
            _entityValue.Add("Xi", 926); // greek capital letter xi, U+039E ISOgrk3 
            _entityName.Add(926, "Xi");
            _entityValue.Add("Omicron", 927); // greek capital letter omicron, U+039F 
            _entityName.Add(927, "Omicron");
            _entityValue.Add("Pi", 928); // greek capital letter pi, U+03A0 ISOgrk3 
            _entityName.Add(928, "Pi");
            _entityValue.Add("Rho", 929); // greek capital letter rho, U+03A1 
            _entityName.Add(929, "Rho");
            _entityValue.Add("Sigma", 931); // greek capital letter sigma, U+03A3 ISOgrk3 
            _entityName.Add(931, "Sigma");
            _entityValue.Add("Tau", 932); // greek capital letter tau, U+03A4 
            _entityName.Add(932, "Tau");
            _entityValue.Add("Upsilon", 933); // greek capital letter upsilon, U+03A5 ISOgrk3 
            _entityName.Add(933, "Upsilon");
            _entityValue.Add("Phi", 934); // greek capital letter phi, U+03A6 ISOgrk3 
            _entityName.Add(934, "Phi");
            _entityValue.Add("Chi", 935); // greek capital letter chi, U+03A7 
            _entityName.Add(935, "Chi");
            _entityValue.Add("Psi", 936); // greek capital letter psi, U+03A8 ISOgrk3 
            _entityName.Add(936, "Psi");
            _entityValue.Add("Omega", 937); // greek capital letter omega, U+03A9 ISOgrk3 
            _entityName.Add(937, "Omega");
            _entityValue.Add("alpha", 945); // greek small letter alpha, U+03B1 ISOgrk3 
            _entityName.Add(945, "alpha");
            _entityValue.Add("beta", 946); // greek small letter beta, U+03B2 ISOgrk3 
            _entityName.Add(946, "beta");
            _entityValue.Add("gamma", 947); // greek small letter gamma, U+03B3 ISOgrk3 
            _entityName.Add(947, "gamma");
            _entityValue.Add("delta", 948); // greek small letter delta, U+03B4 ISOgrk3 
            _entityName.Add(948, "delta");
            _entityValue.Add("epsilon", 949); // greek small letter epsilon, U+03B5 ISOgrk3 
            _entityName.Add(949, "epsilon");
            _entityValue.Add("zeta", 950); // greek small letter zeta, U+03B6 ISOgrk3 
            _entityName.Add(950, "zeta");
            _entityValue.Add("eta", 951); // greek small letter eta, U+03B7 ISOgrk3 
            _entityName.Add(951, "eta");
            _entityValue.Add("theta", 952); // greek small letter theta, U+03B8 ISOgrk3 
            _entityName.Add(952, "theta");
            _entityValue.Add("iota", 953); // greek small letter iota, U+03B9 ISOgrk3 
            _entityName.Add(953, "iota");
            _entityValue.Add("kappa", 954); // greek small letter kappa, U+03BA ISOgrk3 
            _entityName.Add(954, "kappa");
            _entityValue.Add("lambda", 955); // greek small letter lambda, U+03BB ISOgrk3 
            _entityName.Add(955, "lambda");
            _entityValue.Add("mu", 956); // greek small letter mu, U+03BC ISOgrk3 
            _entityName.Add(956, "mu");
            _entityValue.Add("nu", 957); // greek small letter nu, U+03BD ISOgrk3 
            _entityName.Add(957, "nu");
            _entityValue.Add("xi", 958); // greek small letter xi, U+03BE ISOgrk3 
            _entityName.Add(958, "xi");
            _entityValue.Add("omicron", 959); // greek small letter omicron, U+03BF NEW 
            _entityName.Add(959, "omicron");
            _entityValue.Add("pi", 960); // greek small letter pi, U+03C0 ISOgrk3 
            _entityName.Add(960, "pi");
            _entityValue.Add("rho", 961); // greek small letter rho, U+03C1 ISOgrk3 
            _entityName.Add(961, "rho");
            _entityValue.Add("sigmaf", 962); // greek small letter final sigma, U+03C2 ISOgrk3 
            _entityName.Add(962, "sigmaf");
            _entityValue.Add("sigma", 963); // greek small letter sigma, U+03C3 ISOgrk3 
            _entityName.Add(963, "sigma");
            _entityValue.Add("tau", 964); // greek small letter tau, U+03C4 ISOgrk3 
            _entityName.Add(964, "tau");
            _entityValue.Add("upsilon", 965); // greek small letter upsilon, U+03C5 ISOgrk3 
            _entityName.Add(965, "upsilon");
            _entityValue.Add("phi", 966); // greek small letter phi, U+03C6 ISOgrk3 
            _entityName.Add(966, "phi");
            _entityValue.Add("chi", 967); // greek small letter chi, U+03C7 ISOgrk3 
            _entityName.Add(967, "chi");
            _entityValue.Add("psi", 968); // greek small letter psi, U+03C8 ISOgrk3 
            _entityName.Add(968, "psi");
            _entityValue.Add("omega", 969); // greek small letter omega, U+03C9 ISOgrk3 
            _entityName.Add(969, "omega");
            _entityValue.Add("thetasym", 977); // greek small letter theta symbol, U+03D1 NEW 
            _entityName.Add(977, "thetasym");
            _entityValue.Add("upsih", 978); // greek upsilon with hook symbol, U+03D2 NEW 
            _entityName.Add(978, "upsih");
            _entityValue.Add("piv", 982); // greek pi symbol, U+03D6 ISOgrk3 
            _entityName.Add(982, "piv");
            _entityValue.Add("bull", 8226); // bullet = black small circle, U+2022 ISOpub 
            _entityName.Add(8226, "bull");
            _entityValue.Add("hellip", 8230); // horizontal ellipsis = three dot leader, U+2026 ISOpub 
            _entityName.Add(8230, "hellip");
            _entityValue.Add("prime", 8242); // prime = minutes = feet, U+2032 ISOtech 
            _entityName.Add(8242, "prime");
            _entityValue.Add("Prime", 8243); // double prime = seconds = inches, U+2033 ISOtech 
            _entityName.Add(8243, "Prime");
            _entityValue.Add("oline", 8254); // overline = spacing overscore, U+203E NEW 
            _entityName.Add(8254, "oline");
            _entityValue.Add("frasl", 8260); // fraction slash, U+2044 NEW 
            _entityName.Add(8260, "frasl");
            _entityValue.Add("weierp", 8472); // script capital P = power set = Weierstrass p, U+2118 ISOamso 
            _entityName.Add(8472, "weierp");
            _entityValue.Add("image", 8465); // blackletter capital I = imaginary part, U+2111 ISOamso 
            _entityName.Add(8465, "image");
            _entityValue.Add("real", 8476); // blackletter capital R = real part symbol, U+211C ISOamso 
            _entityName.Add(8476, "real");
            _entityValue.Add("trade", 8482); // trade mark sign, U+2122 ISOnum 
            _entityName.Add(8482, "trade");
            _entityValue.Add("alefsym", 8501); // alef symbol = first transfinite cardinal, U+2135 NEW 
            _entityName.Add(8501, "alefsym");
            _entityValue.Add("larr", 8592); // leftwards arrow, U+2190 ISOnum 
            _entityName.Add(8592, "larr");
            _entityValue.Add("uarr", 8593); // upwards arrow, U+2191 ISOnum
            _entityName.Add(8593, "uarr");
            _entityValue.Add("rarr", 8594); // rightwards arrow, U+2192 ISOnum 
            _entityName.Add(8594, "rarr");
            _entityValue.Add("darr", 8595); // downwards arrow, U+2193 ISOnum 
            _entityName.Add(8595, "darr");
            _entityValue.Add("harr", 8596); // left right arrow, U+2194 ISOamsa 
            _entityName.Add(8596, "harr");
            _entityValue.Add("crarr", 8629); // downwards arrow with corner leftwards = carriage return, U+21B5 NEW 
            _entityName.Add(8629, "crarr");
            _entityValue.Add("lArr", 8656); // leftwards double arrow, U+21D0 ISOtech 
            _entityName.Add(8656, "lArr");
            _entityValue.Add("uArr", 8657); // upwards double arrow, U+21D1 ISOamsa 
            _entityName.Add(8657, "uArr");
            _entityValue.Add("rArr", 8658); // rightwards double arrow, U+21D2 ISOtech 
            _entityName.Add(8658, "rArr");
            _entityValue.Add("dArr", 8659); // downwards double arrow, U+21D3 ISOamsa 
            _entityName.Add(8659, "dArr");
            _entityValue.Add("hArr", 8660); // left right double arrow, U+21D4 ISOamsa 
            _entityName.Add(8660, "hArr");
            _entityValue.Add("forall", 8704); // for all, U+2200 ISOtech 
            _entityName.Add(8704, "forall");
            _entityValue.Add("part", 8706); // partial differential, U+2202 ISOtech 
            _entityName.Add(8706, "part");
            _entityValue.Add("exist", 8707); // there exists, U+2203 ISOtech 
            _entityName.Add(8707, "exist");
            _entityValue.Add("empty", 8709); // empty set = null set = diameter, U+2205 ISOamso 
            _entityName.Add(8709, "empty");
            _entityValue.Add("nabla", 8711); // nabla = backward difference, U+2207 ISOtech 
            _entityName.Add(8711, "nabla");
            _entityValue.Add("isin", 8712); // element of, U+2208 ISOtech 
            _entityName.Add(8712, "isin");
            _entityValue.Add("notin", 8713); // not an element of, U+2209 ISOtech 
            _entityName.Add(8713, "notin");
            _entityValue.Add("ni", 8715); // contains as member, U+220B ISOtech 
            _entityName.Add(8715, "ni");
            _entityValue.Add("prod", 8719); // n-ary product = product sign, U+220F ISOamsb 
            _entityName.Add(8719, "prod");
            _entityValue.Add("sum", 8721); // n-ary sumation, U+2211 ISOamsb 
            _entityName.Add(8721, "sum");
            _entityValue.Add("minus", 8722); // minus sign, U+2212 ISOtech 
            _entityName.Add(8722, "minus");
            _entityValue.Add("lowast", 8727); // asterisk operator, U+2217 ISOtech 
            _entityName.Add(8727, "lowast");
            _entityValue.Add("radic", 8730); // square root = radical sign, U+221A ISOtech 
            _entityName.Add(8730, "radic");
            _entityValue.Add("prop", 8733); // proportional to, U+221D ISOtech 
            _entityName.Add(8733, "prop");
            _entityValue.Add("infin", 8734); // infinity, U+221E ISOtech 
            _entityName.Add(8734, "infin");
            _entityValue.Add("ang", 8736); // angle, U+2220 ISOamso 
            _entityName.Add(8736, "ang");
            _entityValue.Add("and", 8743); // logical and = wedge, U+2227 ISOtech 
            _entityName.Add(8743, "and");
            _entityValue.Add("or", 8744); // logical or = vee, U+2228 ISOtech 
            _entityName.Add(8744, "or");
            _entityValue.Add("cap", 8745); // intersection = cap, U+2229 ISOtech 
            _entityName.Add(8745, "cap");
            _entityValue.Add("cup", 8746); // union = cup, U+222A ISOtech 
            _entityName.Add(8746, "cup");
            _entityValue.Add("int", 8747); // integral, U+222B ISOtech 
            _entityName.Add(8747, "int");
            _entityValue.Add("there4", 8756); // therefore, U+2234 ISOtech 
            _entityName.Add(8756, "there4");
            _entityValue.Add("sim", 8764); // tilde operator = varies with = similar to, U+223C ISOtech 
            _entityName.Add(8764, "sim");
            _entityValue.Add("cong", 8773); // approximately equal to, U+2245 ISOtech 
            _entityName.Add(8773, "cong");
            _entityValue.Add("asymp", 8776); // almost equal to = asymptotic to, U+2248 ISOamsr 
            _entityName.Add(8776, "asymp");
            _entityValue.Add("ne", 8800); // not equal to, U+2260 ISOtech 
            _entityName.Add(8800, "ne");
            _entityValue.Add("equiv", 8801); // identical to, U+2261 ISOtech 
            _entityName.Add(8801, "equiv");
            _entityValue.Add("le", 8804); // less-than or equal to, U+2264 ISOtech 
            _entityName.Add(8804, "le");
            _entityValue.Add("ge", 8805); // greater-than or equal to, U+2265 ISOtech 
            _entityName.Add(8805, "ge");
            _entityValue.Add("sub", 8834); // subset of, U+2282 ISOtech 
            _entityName.Add(8834, "sub");
            _entityValue.Add("sup", 8835); // superset of, U+2283 ISOtech 
            _entityName.Add(8835, "sup");
            _entityValue.Add("nsub", 8836); // not a subset of, U+2284 ISOamsn 
            _entityName.Add(8836, "nsub");
            _entityValue.Add("sube", 8838); // subset of or equal to, U+2286 ISOtech 
            _entityName.Add(8838, "sube");
            _entityValue.Add("supe", 8839); // superset of or equal to, U+2287 ISOtech 
            _entityName.Add(8839, "supe");
            _entityValue.Add("oplus", 8853); // circled plus = direct sum, U+2295 ISOamsb 
            _entityName.Add(8853, "oplus");
            _entityValue.Add("otimes", 8855); // circled times = vector product, U+2297 ISOamsb 
            _entityName.Add(8855, "otimes");
            _entityValue.Add("perp", 8869); // up tack = orthogonal to = perpendicular, U+22A5 ISOtech 
            _entityName.Add(8869, "perp");
            _entityValue.Add("sdot", 8901); // dot operator, U+22C5 ISOamsb 
            _entityName.Add(8901, "sdot");
            _entityValue.Add("lceil", 8968); // left ceiling = apl upstile, U+2308 ISOamsc 
            _entityName.Add(8968, "lceil");
            _entityValue.Add("rceil", 8969); // right ceiling, U+2309 ISOamsc 
            _entityName.Add(8969, "rceil");
            _entityValue.Add("lfloor", 8970); // left floor = apl downstile, U+230A ISOamsc 
            _entityName.Add(8970, "lfloor");
            _entityValue.Add("rfloor", 8971); // right floor, U+230B ISOamsc 
            _entityName.Add(8971, "rfloor");
            _entityValue.Add("lang", 9001); // left-pointing angle bracket = bra, U+2329 ISOtech 
            _entityName.Add(9001, "lang");
            _entityValue.Add("rang", 9002); // right-pointing angle bracket = ket, U+232A ISOtech 
            _entityName.Add(9002, "rang");
            _entityValue.Add("loz", 9674); // lozenge, U+25CA ISOpub 
            _entityName.Add(9674, "loz");
            _entityValue.Add("spades", 9824); // black spade suit, U+2660 ISOpub 
            _entityName.Add(9824, "spades");
            _entityValue.Add("clubs", 9827); // black club suit = shamrock, U+2663 ISOpub 
            _entityName.Add(9827, "clubs");
            _entityValue.Add("hearts", 9829); // black heart suit = valentine, U+2665 ISOpub 
            _entityName.Add(9829, "hearts");
            _entityValue.Add("diams", 9830); // black diamond suit, U+2666 ISOpub 
            _entityName.Add(9830, "diams");
            _entityValue.Add("quot", 34); // quotation mark = APL quote, U+0022 ISOnum 
            _entityName.Add(34, "quot");
            _entityValue.Add("amp", 38); // ampersand, U+0026 ISOnum 
            _entityName.Add(38, "amp");
            _entityValue.Add("lt", 60); // less-than sign, U+003C ISOnum 
            _entityName.Add(60, "lt");
            _entityValue.Add("gt", 62); // greater-than sign, U+003E ISOnum 
            _entityName.Add(62, "gt");
            _entityValue.Add("OElig", 338); // latin capital ligature OE, U+0152 ISOlat2 
            _entityName.Add(338, "OElig");
            _entityValue.Add("oelig", 339); // latin small ligature oe, U+0153 ISOlat2 
            _entityName.Add(339, "oelig");
            _entityValue.Add("Scaron", 352); // latin capital letter S with caron, U+0160 ISOlat2 
            _entityName.Add(352, "Scaron");
            _entityValue.Add("scaron", 353); // latin small letter s with caron, U+0161 ISOlat2 
            _entityName.Add(353, "scaron");
            _entityValue.Add("Yuml", 376); // latin capital letter Y with diaeresis, U+0178 ISOlat2 
            _entityName.Add(376, "Yuml");
            _entityValue.Add("circ", 710); // modifier letter circumflex accent, U+02C6 ISOpub 
            _entityName.Add(710, "circ");
            _entityValue.Add("tilde", 732); // small tilde, U+02DC ISOdia 
            _entityName.Add(732, "tilde");
            _entityValue.Add("ensp", 8194); // en space, U+2002 ISOpub 
            _entityName.Add(8194, "ensp");
            _entityValue.Add("emsp", 8195); // em space, U+2003 ISOpub 
            _entityName.Add(8195, "emsp");
            _entityValue.Add("thinsp", 8201); // thin space, U+2009 ISOpub 
            _entityName.Add(8201, "thinsp");
            _entityValue.Add("zwnj", 8204); // zero width non-joiner, U+200C NEW RFC 2070 
            _entityName.Add(8204, "zwnj");
            _entityValue.Add("zwj", 8205); // zero width joiner, U+200D NEW RFC 2070 
            _entityName.Add(8205, "zwj");
            _entityValue.Add("lrm", 8206); // left-to-right mark, U+200E NEW RFC 2070 
            _entityName.Add(8206, "lrm");
            _entityValue.Add("rlm", 8207); // right-to-left mark, U+200F NEW RFC 2070 
            _entityName.Add(8207, "rlm");
            _entityValue.Add("ndash", 8211); // en dash, U+2013 ISOpub 
            _entityName.Add(8211, "ndash");
            _entityValue.Add("mdash", 8212); // em dash, U+2014 ISOpub 
            _entityName.Add(8212, "mdash");
            _entityValue.Add("lsquo", 8216); // left single quotation mark, U+2018 ISOnum 
            _entityName.Add(8216, "lsquo");
            _entityValue.Add("rsquo", 8217); // right single quotation mark, U+2019 ISOnum 
            _entityName.Add(8217, "rsquo");
            _entityValue.Add("sbquo", 8218); // single low-9 quotation mark, U+201A NEW 
            _entityName.Add(8218, "sbquo");
            _entityValue.Add("ldquo", 8220); // left double quotation mark, U+201C ISOnum 
            _entityName.Add(8220, "ldquo");
            _entityValue.Add("rdquo", 8221); // right double quotation mark, U+201D ISOnum 
            _entityName.Add(8221, "rdquo");
            _entityValue.Add("bdquo", 8222); // double low-9 quotation mark, U+201E NEW 
            _entityName.Add(8222, "bdquo");
            _entityValue.Add("dagger", 8224); // dagger, U+2020 ISOpub 
            _entityName.Add(8224, "dagger");
            _entityValue.Add("Dagger", 8225); // double dagger, U+2021 ISOpub 
            _entityName.Add(8225, "Dagger");
            _entityValue.Add("permil", 8240); // per mille sign, U+2030 ISOtech 
            _entityName.Add(8240, "permil");
            _entityValue.Add("lsaquo", 8249); // single left-pointing angle quotation mark, U+2039 ISO proposed 
            _entityName.Add(8249, "lsaquo");
            _entityValue.Add("rsaquo", 8250); // single right-pointing angle quotation mark, U+203A ISO proposed 
            _entityName.Add(8250, "rsaquo");
            _entityValue.Add("euro", 8364); // euro sign, U+20AC NEW 
            _entityName.Add(8364, "euro");

            _maxEntitySize = 8 + 1; // we add the # char

            #endregion
        }

        private HtmlEntity()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replace known entities by characters.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string DeEntitize(string text)
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return text;

            StringBuilder sb = new StringBuilder(text.Length);
            ParseState state = ParseState.Text;
            StringBuilder entity = new StringBuilder(10);

            for (int i = 0; i < text.Length; i++)
            {
                switch (state)
                {
                    case ParseState.Text:
                        switch (text[i])
                        {
                            case '&':
                                state = ParseState.EntityStart;
                                break;

                            default:
                                sb.Append(text[i]);
                                break;
                        }
                        break;

                    case ParseState.EntityStart:
                        switch (text[i])
                        {
                            case ';':
                                if (entity.Length == 0)
                                {
                                    sb.Append("&;");
                                }
                                else
                                {
                                    if (entity[0] == '#')
                                    {
                                        string e = entity.ToString();
                                        try
 										{
											string codeStr = e.Substring(1).Trim().ToLower();
											int fromBase;
											if (codeStr.StartsWith("x"))
											{
												fromBase = 16;
												codeStr = codeStr.Substring(1);
											}
											else
											{
												fromBase = 10;
											}
											int code = Convert.ToInt32(codeStr, fromBase);
 											sb.Append(Convert.ToChar(code));
 										}
                                        catch
                                        {
                                            sb.Append("&#" + e + ";");
                                        }
                                    }
                                    else
                                    {
                                        // named entity?
                                        int code;
                                        if (!_entityValue.TryGetValue(entity.ToString(), out code))
                                        {
                                            // nope
                                            sb.Append("&" + entity + ";");
                                        }
                                        else
                                        {
                                            // we found one
                                            sb.Append(Convert.ToChar(code));
                                        }
                                    }
                                    entity.Remove(0, entity.Length);
                                }
                                state = ParseState.Text;
                                break;

                            case '&':
                                // new entity start without end, it was not an entity...
                                sb.Append("&" + entity);
                                entity.Remove(0, entity.Length);
                                break;

                            default:
                                entity.Append(text[i]);
                                if (entity.Length > _maxEntitySize)
                                {
                                    // unknown stuff, just don't touch it
                                    state = ParseState.Text;
                                    sb.Append("&" + entity);
                                    entity.Remove(0, entity.Length);
                                }
                                break;
                        }
                        break;
                }
            }

            // finish the work
            if (state == ParseState.EntityStart)
            {
                sb.Append("&" + entity);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clone and entitize an HtmlNode. This will affect attribute values and nodes' text. It will also entitize all child nodes.
        /// </summary>
        /// <param name="node">The node to entitize.</param>
        /// <returns>An entitized cloned node.</returns>
        public static HtmlNode Entitize(HtmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            HtmlNode result = node.CloneNode(true);
            if (result.HasAttributes)
                Entitize(result.Attributes);

            if (result.HasChildNodes)
            {
                Entitize(result.ChildNodes);
            }
            else
            {
                if (result.NodeType == HtmlNodeType.Text)
                {
                    ((HtmlTextNode) result).Text = Entitize(((HtmlTextNode) result).Text, true, true);
                }
            }
            return result;
        }


        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text)
        {
            return Entitize(text, true);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text, bool useNames)
        {
            return Entitize(text, useNames, false);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <param name="entitizeQuotAmpAndLtGt">If set to true, the [quote], [ampersand], [lower than] and [greather than] characters will be entitized.</param>
        /// <returns>The result text</returns>
        public static string Entitize(string text, bool useNames, bool entitizeQuotAmpAndLtGt)
//		_entityValue.Add("quot", 34);	// quotation mark = APL quote, U+0022 ISOnum 
//		_entityName.Add(34, "quot");
//		_entityValue.Add("amp", 38);	// ampersand, U+0026 ISOnum 
//		_entityName.Add(38, "amp");
//		_entityValue.Add("lt", 60);	// less-than sign, U+003C ISOnum 
//		_entityName.Add(60, "lt");
//		_entityValue.Add("gt", 62);	// greater-than sign, U+003E ISOnum 
//		_entityName.Add(62, "gt");
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return text;

            StringBuilder sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                int code = text[i];
                if ((code > 127) ||
                    (entitizeQuotAmpAndLtGt && ((code == 34) || (code == 38) || (code == 60) || (code == 62))))
                {
                    string entity;
                    EntityName.TryGetValue(code, out entity);

                    if ((entity == null) || (!useNames))
                    {
                        sb.Append("&#" + code + ";");
                    }
                    else
                    {
                        sb.Append("&" + entity + ";");
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private static void Entitize(HtmlAttributeCollection collection)
        {
            foreach (HtmlAttribute at in collection)
            {
                at.Value = Entitize(at.Value);
            }
        }

        private static void Entitize(HtmlNodeCollection collection)
        {
            foreach (HtmlNode node in collection)
            {
                if (node.HasAttributes)
                    Entitize(node.Attributes);

                if (node.HasChildNodes)
                {
                    Entitize(node.ChildNodes);
                }
                else
                {
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        ((HtmlTextNode) node).Text = Entitize(((HtmlTextNode) node).Text, true, true);
                    }
                }
            }
        }

        #endregion

        #region Nested type: ParseState

        private enum ParseState
        {
            Text,
            EntityStart
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlNameTable.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class HtmlNameTable : XmlNameTable
    {
        #region Fields

        private NameTable _nametable = new NameTable();

        #endregion

        #region Public Methods

        public override string Add(string array)
        {
            return _nametable.Add(array);
        }

        public override string Add(char[] array, int offset, int length)
        {
            return _nametable.Add(array, offset, length);
        }

        public override string Get(string array)
        {
            return _nametable.Get(array);
        }

        public override string Get(char[] array, int offset, int length)
        {
            return _nametable.Get(array, offset, length);
        }

        #endregion

        #region Internal Methods

        internal string GetOrAdd(string array)
        {
            string s = Get(array);
            if (s == null)
            {
                return Add(array);
            }
            return s;
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlNode.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
	/// <summary>
	/// Represents an HTML node.
	/// </summary>
	[DebuggerDisplay("Name: {OriginalName}")]
	public partial class HtmlNode
	{
        #region Consts
        internal const string DepthLevelExceptionMessage = "The document is too complex to parse";
        #endregion

        #region Fields

        internal HtmlAttributeCollection _attributes;
		internal HtmlNodeCollection _childnodes;
		internal HtmlNode _endnode;

        private bool _changed;
		internal string _innerhtml;
		internal int _innerlength;
		internal int _innerstartindex;
		internal int _line;
		internal int _lineposition;
		private string _name;
		internal int _namelength;
		internal int _namestartindex;
		internal HtmlNode _nextnode;
		internal HtmlNodeType _nodetype;
		internal string _outerhtml;
		internal int _outerlength;
		internal int _outerstartindex;
		private string _optimizedName;
		internal HtmlDocument _ownerdocument;
		internal HtmlNode _parentnode;
		internal HtmlNode _prevnode;
		internal HtmlNode _prevwithsamename;
		internal bool _starttag;
		internal int _streamposition;

		#endregion

		#region Static Members

		/// <summary>
		/// Gets the name of a comment node. It is actually defined as '#comment'.
		/// </summary>
		public static readonly string HtmlNodeTypeNameComment = "#comment";

		/// <summary>
		/// Gets the name of the document node. It is actually defined as '#document'.
		/// </summary>
		public static readonly string HtmlNodeTypeNameDocument = "#document";

		/// <summary>
		/// Gets the name of a text node. It is actually defined as '#text'.
		/// </summary>
		public static readonly string HtmlNodeTypeNameText = "#text";

		/// <summary>
		/// Gets a collection of flags that define specific behaviors for specific element nodes.
		/// The table contains a DictionaryEntry list with the lowercase tag name as the Key, and a combination of HtmlElementFlags as the Value.
		/// </summary>
		public static Dictionary<string, HtmlElementFlag> ElementsFlags;

		#endregion

		#region Constructors

		/// <summary>
		/// Initialize HtmlNode. Builds a list of all tags that have special allowances
		/// </summary>
		static HtmlNode()
		{
			// tags whose content may be anything
			ElementsFlags = new Dictionary<string, HtmlElementFlag>();
			ElementsFlags.Add("script", HtmlElementFlag.CData);
			ElementsFlags.Add("style", HtmlElementFlag.CData);
			ElementsFlags.Add("noxhtml", HtmlElementFlag.CData);

			// tags that can not contain other tags
			ElementsFlags.Add("base", HtmlElementFlag.Empty);
			ElementsFlags.Add("link", HtmlElementFlag.Empty);
			ElementsFlags.Add("meta", HtmlElementFlag.Empty);
			ElementsFlags.Add("isindex", HtmlElementFlag.Empty);
			ElementsFlags.Add("hr", HtmlElementFlag.Empty);
			ElementsFlags.Add("col", HtmlElementFlag.Empty);
			ElementsFlags.Add("img", HtmlElementFlag.Empty);
			ElementsFlags.Add("param", HtmlElementFlag.Empty);
			ElementsFlags.Add("embed", HtmlElementFlag.Empty);
			ElementsFlags.Add("frame", HtmlElementFlag.Empty);
			ElementsFlags.Add("wbr", HtmlElementFlag.Empty);
			ElementsFlags.Add("bgsound", HtmlElementFlag.Empty);
			ElementsFlags.Add("spacer", HtmlElementFlag.Empty);
			ElementsFlags.Add("keygen", HtmlElementFlag.Empty);
			ElementsFlags.Add("area", HtmlElementFlag.Empty);
			ElementsFlags.Add("input", HtmlElementFlag.Empty);
			ElementsFlags.Add("basefont", HtmlElementFlag.Empty);

			ElementsFlags.Add("form",  HtmlElementFlag.CanOverlap);

			// they sometimes contain, and sometimes they don 't...
			ElementsFlags.Add("option", HtmlElementFlag.Empty);

			// tag whose closing tag is equivalent to open tag:
			// <p>bla</p>bla will be transformed into <p>bla</p>bla
			// <p>bla<p>bla will be transformed into <p>bla<p>bla and not <p>bla></p><p>bla</p> or <p>bla<p>bla</p></p>
			//<br> see above
			ElementsFlags.Add("br", HtmlElementFlag.Empty | HtmlElementFlag.Closed);

		    if (!HtmlDocument.DisableBehavaiorTagP)
		    {
		        ElementsFlags.Add("p", HtmlElementFlag.Empty | HtmlElementFlag.Closed);
            }
		}

		/// <summary>
		/// Initializes HtmlNode, providing type, owner and where it exists in a collection
		/// </summary>
		/// <param name="type"></param>
		/// <param name="ownerdocument"></param>
		/// <param name="index"></param>
		public HtmlNode(HtmlNodeType type, HtmlDocument ownerdocument, int index)
		{
			_nodetype = type;
			_ownerdocument = ownerdocument;
			_outerstartindex = index;

			switch (type)
			{
				case HtmlNodeType.Comment:
					Name = HtmlNodeTypeNameComment;
					_endnode = this;
					break;

				case HtmlNodeType.Document:
					Name = HtmlNodeTypeNameDocument;
					_endnode = this;
					break;

				case HtmlNodeType.Text:
					Name = HtmlNodeTypeNameText;
					_endnode = this;
					break;
			}

			if (_ownerdocument.Openednodes != null)
			{
				if (!Closed)
				{
					// we use the index as the key

					// -1 means the node comes from public
					if (-1 != index)
					{
						_ownerdocument.Openednodes.Add(index, this);
					}
				}
			}

			if ((-1 != index) || (type == HtmlNodeType.Comment) || (type == HtmlNodeType.Text)) return;
			// innerhtml and outerhtml must be calculated
            SetChanged();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the collection of HTML attributes for this node. May not be null.
		/// </summary>
		public HtmlAttributeCollection Attributes
		{
			get
			{
				if (!HasAttributes)
				{
					_attributes = new HtmlAttributeCollection(this);
				}
				return _attributes;
			}
			internal set { _attributes = value; }
		}

		/// <summary>
		/// Gets all the children of the node.
		/// </summary>
		public HtmlNodeCollection ChildNodes
		{
			get { return _childnodes ?? (_childnodes = new HtmlNodeCollection(this)); }
			internal set { _childnodes = value; }
		}

		/// <summary>
		/// Gets a value indicating if this node has been closed or not.
		/// </summary>
		public bool Closed
		{
			get { return (_endnode != null); }
		}

		/// <summary>
		/// Gets the collection of HTML attributes for the closing tag. May not be null.
		/// </summary>
		public HtmlAttributeCollection ClosingAttributes
		{
			get
			{
				return !HasClosingAttributes ? new HtmlAttributeCollection(this) : _endnode.Attributes;
			}
		}

		internal HtmlNode EndNode
		{
			get { return _endnode; }
		}

		/// <summary>
		/// Gets the first child of the node.
		/// </summary>
		public HtmlNode FirstChild
		{
			get
			{
				return !HasChildNodes ? null : _childnodes[0];
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node has any attributes.
		/// </summary>
		public bool HasAttributes
		{
			get
			{
				if (_attributes == null)
				{
					return false;
				}

				if (_attributes.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this node has any child nodes.
		/// </summary>
		public bool HasChildNodes
		{
			get
			{
				if (_childnodes == null)
				{
					return false;
				}

				if (_childnodes.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node has any attributes on the closing tag.
		/// </summary>
		public bool HasClosingAttributes
		{
			get
			{
				if ((_endnode == null) || (_endnode == this))
				{
					return false;
				}

				if (_endnode._attributes == null)
				{
					return false;
				}

				if (_endnode._attributes.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets or sets the value of the 'id' HTML attribute. The document must have been parsed using the OptionUseIdAttribute set to true.
		/// </summary>
		public string Id
		{
			get
			{
				if (_ownerdocument.Nodesid == null)
					throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);

				return GetId();
			}
			set
			{
				if (_ownerdocument.Nodesid == null)
					throw new Exception(HtmlDocument.HtmlExceptionUseIdAttributeFalse);

				if (value == null)
					throw new ArgumentNullException("value");

				SetId(value);
			}
		}

		/// <summary>
		/// Gets or Sets the HTML between the start and end tags of the object.
		/// </summary>
		public virtual string InnerHtml
		{
			get
			{
				if (_changed)
				{
                    UpdateHtml();
					return _innerhtml;
				}

				if (_innerhtml != null)
					return _innerhtml;

				if (_innerstartindex < 0)
					return string.Empty;

				return _ownerdocument.Text.Substring(_innerstartindex, _innerlength);
			}
			set
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(value);

				RemoveAllChildren();
				AppendChildren(doc.DocumentNode.ChildNodes);
			}
		}

		/// <summary>
		/// Gets or Sets the text between the start and end tags of the object.
		/// </summary>
		public virtual string InnerText
		{
			get
			{
				if (_nodetype == HtmlNodeType.Text)
					return ((HtmlTextNode)this).Text;

				if (_nodetype == HtmlNodeType.Comment)
					return ((HtmlCommentNode)this).Comment;

				// note: right now, this method is *slow*, because we recompute everything.
				// it could be optimized like innerhtml
				if (!HasChildNodes)
					return string.Empty;

				string s = null;
				foreach (HtmlNode node in ChildNodes)
					s += node.InnerText;
				return s;
			}
		}

		/// <summary>
		/// Gets the last child of the node.
		/// </summary>
		public HtmlNode LastChild
		{
			get
			{
				return !HasChildNodes ? null : _childnodes[_childnodes.Count - 1];
			}
		}

		/// <summary>
		/// Gets the line number of this node in the document.
		/// </summary>
		public int Line
		{
			get { return _line; }
			internal set { _line = value; }
		}

		/// <summary>
		/// Gets the column number of this node in the document.
		/// </summary>
		public int LinePosition
		{
			get { return _lineposition; }
			internal set { _lineposition = value; }
		}

		/// <summary>
		/// Gets or sets this node's name.
		/// </summary>
		public string Name
		{
			get
			{
				if (_optimizedName == null)
				{
					if (_name == null)
						Name = _ownerdocument.Text.Substring(_namestartindex, _namelength);

					if (_name == null)
						_optimizedName = string.Empty;
					else
						_optimizedName = _name.ToLower();
				}
				return _optimizedName;
			}
			set { _name = value; _optimizedName = null; }
		}

		/// <summary>
		/// Gets the HTML node immediately following this element.
		/// </summary>
		public HtmlNode NextSibling
		{
			get { return _nextnode; }
			internal set { _nextnode = value; }
		}

		/// <summary>
		/// Gets the type of this node.
		/// </summary>
		public HtmlNodeType NodeType
		{
			get { return _nodetype; }
			internal set { _nodetype = value; }
		}

		/// <summary>
		/// The original unaltered name of the tag
		/// </summary>
		public string OriginalName
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets or Sets the object and its content in HTML.
		/// </summary>
		public virtual string OuterHtml
		{
			get
			{
				if (_changed)
				{
                    UpdateHtml();
					return _outerhtml;
				}

				if (_outerhtml != null)
				{
					return _outerhtml;
				}

				if (_outerstartindex < 0)
				{
					return string.Empty;
				}

				return _ownerdocument.Text.Substring(_outerstartindex, _outerlength);
			}
		}

		/// <summary>
		/// Gets the <see cref="HtmlDocument"/> to which this node belongs.
		/// </summary>
		public HtmlDocument OwnerDocument
		{
			get { return _ownerdocument; }
			internal set { _ownerdocument = value; }
		}

		/// <summary>
		/// Gets the parent of this node (for nodes that can have parents).
		/// </summary>
		public HtmlNode ParentNode
		{
			get { return _parentnode; }
			internal set { _parentnode = value; }
		}

		/// <summary>
		/// Gets the node immediately preceding this node.
		/// </summary>
		public HtmlNode PreviousSibling
		{
			get { return _prevnode; }
			internal set { _prevnode = value; }
		}

		/// <summary>
		/// Gets the stream position of this node in the document, relative to the start of the document.
		/// </summary>
		public int StreamPosition
		{
			get { return _streamposition; }
		}

		/// <summary>
		/// Gets a valid XPath string that points to this node
		/// </summary>
		public string XPath
		{
			get
			{
				string basePath = (ParentNode == null || ParentNode.NodeType == HtmlNodeType.Document)
									  ? "/"
									  : ParentNode.XPath + "/";
				return basePath + GetRelativeXpath();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Determines if an element node can be kept overlapped.
		/// </summary>
		/// <param name="name">The name of the element node to check. May not be <c>null</c>.</param>
		/// <returns>true if the name is the name of an element node that can be kept overlapped, <c>false</c> otherwise.</returns>
		public static bool CanOverlapElement(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!ElementsFlags.ContainsKey(name.ToLower()))
			{
				return false;
			}

			HtmlElementFlag flag = ElementsFlags[name.ToLower()];
			return (flag & HtmlElementFlag.CanOverlap) != 0;
		}

		/// <summary>
		/// Creates an HTML node from a string representing literal HTML.
		/// </summary>
		/// <param name="html">The HTML text.</param>
		/// <returns>The newly created node instance.</returns>
		public static HtmlNode CreateNode(string html)
		{
			// REVIEW: this is *not* optimum...
			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);
            if(!doc.DocumentNode.IsSingleElementNode())
            {
                throw new Exception("Multiple node elments can't be created.");
            }

            var element = doc.DocumentNode.FirstChild;

            while (element != null)
            {
                if (element.NodeType == HtmlNodeType.Element && element.OuterHtml != "\r\n")
                    return element;

                element = element.NextSibling;
            }
			return doc.DocumentNode.FirstChild;
		}

		/// <summary>
		/// Determines if an element node is a CDATA element node.
		/// </summary>
		/// <param name="name">The name of the element node to check. May not be null.</param>
		/// <returns>true if the name is the name of a CDATA element node, false otherwise.</returns>
		public static bool IsCDataElement(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!ElementsFlags.ContainsKey(name.ToLower()))
			{
				return false;
			}

			HtmlElementFlag flag = ElementsFlags[name.ToLower()];
			return (flag & HtmlElementFlag.CData) != 0;
		}

		/// <summary>
		/// Determines if an element node is closed.
		/// </summary>
		/// <param name="name">The name of the element node to check. May not be null.</param>
		/// <returns>true if the name is the name of a closed element node, false otherwise.</returns>
		public static bool IsClosedElement(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!ElementsFlags.ContainsKey(name.ToLower()))
			{
				return false;
			}

			HtmlElementFlag flag = ElementsFlags[name.ToLower()];
			return (flag & HtmlElementFlag.Closed) != 0;
		}

		/// <summary>
		/// Determines if an element node is defined as empty.
		/// </summary>
		/// <param name="name">The name of the element node to check. May not be null.</param>
		/// <returns>true if the name is the name of an empty element node, false otherwise.</returns>
		public static bool IsEmptyElement(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (name.Length == 0)
			{
				return true;
			}

			// <!DOCTYPE ...
			if ('!' == name[0])
			{
				return true;
			}

			// <?xml ...
			if ('?' == name[0])
			{
				return true;
			}

			if (!ElementsFlags.ContainsKey(name.ToLower()))
			{
				return false;
			}

			HtmlElementFlag flag = ElementsFlags[name.ToLower()];
			return (flag & HtmlElementFlag.Empty) != 0;
		}

		/// <summary>
		/// Determines if a text corresponds to the closing tag of an node that can be kept overlapped.
		/// </summary>
		/// <param name="text">The text to check. May not be null.</param>
		/// <returns>true or false.</returns>
		public static bool IsOverlappedClosingElement(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			// min is </x>: 4
			if (text.Length <= 4)
				return false;

			if ((text[0] != '<') ||
				(text[text.Length - 1] != '>') ||
				(text[1] != '/'))
				return false;

			string name = text.Substring(2, text.Length - 3);
			return CanOverlapElement(name);
		}

		/// <summary>
		/// Returns a collection of all ancestor nodes of this element.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<HtmlNode> Ancestors()
		{
			HtmlNode node = ParentNode;
			if (node != null)
			{
				yield return node;//return the immediate parent node

				//now look at it's parent and walk up the tree of parents
				while (node.ParentNode != null)
				{
					yield return node.ParentNode;
					node = node.ParentNode;
				}
			}
		}

		/// <summary>
		/// Get Ancestors with matching name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlNode> Ancestors(string name)
		{
			for (HtmlNode n = ParentNode; n != null; n = n.ParentNode)
				if (n.Name == name)
					yield return n;
		}

		/// <summary>
		/// Returns a collection of all ancestor nodes of this element.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<HtmlNode> AncestorsAndSelf()
		{
			for (HtmlNode n = this; n != null; n = n.ParentNode)
				yield return n;
		}

		/// <summary>
		/// Gets all anscestor nodes and the current node
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlNode> AncestorsAndSelf(string name)
		{
			for (HtmlNode n = this; n != null; n = n.ParentNode)
				if (n.Name == name)
					yield return n;
		}

		/// <summary>
		/// Adds the specified node to the end of the list of children of this node.
		/// </summary>
		/// <param name="newChild">The node to add. May not be null.</param>
		/// <returns>The node added.</returns>
		public HtmlNode AppendChild(HtmlNode newChild)
		{
			if (newChild == null)
			{
				throw new ArgumentNullException("newChild");
			}

			ChildNodes.Append(newChild);
			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
			return newChild;
		}

		/// <summary>
		/// Adds the specified node to the end of the list of children of this node.
		/// </summary>
		/// <param name="newChildren">The node list to add. May not be null.</param>
		public void AppendChildren(HtmlNodeCollection newChildren)
		{
			if (newChildren == null)
				throw new ArgumentNullException("newChildren");

			foreach (HtmlNode newChild in newChildren)
			{
				AppendChild(newChild);
			}
		}

		/// <summary>
		/// Gets all Attributes with name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlAttribute> ChildAttributes(string name)
		{
			return Attributes.AttributesWithName(name);
		}

		/// <summary>
		/// Creates a duplicate of the node
		/// </summary>
		/// <returns></returns>
		public HtmlNode Clone()
		{
			return CloneNode(true);
		}

		/// <summary>
		/// Creates a duplicate of the node and changes its name at the same time.
		/// </summary>
		/// <param name="newName">The new name of the cloned node. May not be <c>null</c>.</param>
		/// <returns>The cloned node.</returns>
		public HtmlNode CloneNode(string newName)
		{
			return CloneNode(newName, true);
		}

		/// <summary>
		/// Creates a duplicate of the node and changes its name at the same time.
		/// </summary>
		/// <param name="newName">The new name of the cloned node. May not be null.</param>
		/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
		/// <returns>The cloned node.</returns>
		public HtmlNode CloneNode(string newName, bool deep)
		{
			if (newName == null)
			{
				throw new ArgumentNullException("newName");
			}

			HtmlNode node = CloneNode(deep);
			node.Name = newName;
			return node;
		}

		/// <summary>
		/// Creates a duplicate of the node.
		/// </summary>
		/// <param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself.</param>
		/// <returns>The cloned node.</returns>
		public HtmlNode CloneNode(bool deep)
		{
			HtmlNode node = _ownerdocument.CreateNode(_nodetype);
			node.Name = Name;

			switch (_nodetype)
			{
				case HtmlNodeType.Comment:
					((HtmlCommentNode)node).Comment = ((HtmlCommentNode)this).Comment;
					return node;

				case HtmlNodeType.Text:
					((HtmlTextNode)node).Text = ((HtmlTextNode)this).Text;
					return node;
			}

			// attributes
			if (HasAttributes)
			{
				foreach (HtmlAttribute att in _attributes)
				{
					HtmlAttribute newatt = att.Clone();
					node.Attributes.Append(newatt);
				}
			}

			// closing attributes
			if (HasClosingAttributes)
			{
				node._endnode = _endnode.CloneNode(false);
				foreach (HtmlAttribute att in _endnode._attributes)
				{
					HtmlAttribute newatt = att.Clone();
					node._endnode._attributes.Append(newatt);
				}
			}
			if (!deep)
			{
				return node;
			}

			if (!HasChildNodes)
			{
				return node;
			}

			// child nodes
			foreach (HtmlNode child in _childnodes)
			{
				HtmlNode newchild = child.Clone();
				node.AppendChild(newchild);
			}
			return node;
		}

		/// <summary>
		/// Creates a duplicate of the node and the subtree under it.
		/// </summary>
		/// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
		public void CopyFrom(HtmlNode node)
		{
			CopyFrom(node, true);
		}

		/// <summary>
		/// Creates a duplicate of the node.
		/// </summary>
		/// <param name="node">The node to duplicate. May not be <c>null</c>.</param>
		/// <param name="deep">true to recursively clone the subtree under the specified node, false to clone only the node itself.</param>
		public void CopyFrom(HtmlNode node, bool deep)
		{
			if (node == null)
			{
				throw new ArgumentNullException("node");
			}

			Attributes.RemoveAll();
			if (node.HasAttributes)
			{
				foreach (HtmlAttribute att in node.Attributes)
				{
                    HtmlAttribute newatt = att.Clone();
                    Attributes.Append(newatt);
				}
			}

			if (deep)
			{
				RemoveAllChildren();
				if (node.HasChildNodes)
				{
					foreach (HtmlNode child in node.ChildNodes)
					{
						AppendChild(child.CloneNode(true));
					}
				}
			}
		}



		/// <summary>
		/// Gets all Descendant nodes for this node and each of child nodes
		/// </summary>
        /// <param name="level">The depth level of the node to parse in the html tree</param>
		/// <returns>the current element as an HtmlNode</returns>
		[Obsolete("Use Descendants() instead, the results of this function will change in a future version")]
		public IEnumerable<HtmlNode> DescendantNodes(int level=0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
            }

            foreach (HtmlNode node in ChildNodes)
			{
				yield return node;

                foreach (HtmlNode descendant in node.DescendantNodes(level+1))
                {
                    yield return descendant;
                }
			}
		}

		/// <summary>
		/// Returns a collection of all descendant nodes of this element, in document order
		/// </summary>
		/// <returns></returns>
		[Obsolete("Use DescendantsAndSelf() instead, the results of this function will change in a future version")]
		public IEnumerable<HtmlNode> DescendantNodesAndSelf()
		{
			return DescendantsAndSelf();
		}

	    /// <summary>
	    /// Gets all Descendant nodes in enumerated list
	    /// </summary>
	    /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
	    {
            // DO NOT REMOVE, the empty method is required for Fizzler third party library
	        return Descendants(0);
	    }

        /// <summary>
        /// Gets all Descendant nodes in enumerated list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(int level)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
            }

            foreach (HtmlNode node in ChildNodes)
			{
				yield return node;

                foreach (HtmlNode descendant in node.Descendants(level+1))
                {
                    yield return descendant;
                }
			}
		}

		/// <summary>
		/// Get all descendant nodes with matching name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlNode> Descendants(string name)
		{
			name = name.ToLowerInvariant();
			foreach (HtmlNode node in Descendants())
				if (node.Name.Equals(name))
					yield return node;
		}

		/// <summary>
		/// Returns a collection of all descendant nodes of this element, in document order
		/// </summary>
		/// <returns></returns>
		public IEnumerable<HtmlNode> DescendantsAndSelf()
		{
			yield return this;

            foreach (HtmlNode n in Descendants())
            {
                HtmlNode el = n;
                if (el != null)
                    yield return el;
			}
		}

		/// <summary>
		/// Gets all descendant nodes including this node
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlNode> DescendantsAndSelf(string name)
		{
			yield return this;

			foreach (HtmlNode node in Descendants())
				if (node.Name == name)
					yield return node;
		}

		/// <summary>
		/// Gets first generation child node matching name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public HtmlNode Element(string name)
		{
			foreach (HtmlNode node in ChildNodes)
				if (node.Name == name)
					return node;
			return null;
		}

		/// <summary>
		/// Gets matching first generation child nodes matching name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public IEnumerable<HtmlNode> Elements(string name)
		{
			foreach (HtmlNode node in ChildNodes)
				if (node.Name == name)
					yield return node;
		}

		/// <summary>
		/// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
		/// </summary>
		/// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
		/// <param name="def">The default value to return if not found.</param>
		/// <returns>The value of the attribute if found, the default value if not found.</returns>
		public string GetAttributeValue(string name, string def)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!HasAttributes)
			{
				return def;
			}
			HtmlAttribute att = Attributes[name];
			if (att == null)
			{
				return def;
			}
			return att.Value;
		}

		/// <summary>
		/// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
		/// </summary>
		/// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
		/// <param name="def">The default value to return if not found.</param>
		/// <returns>The value of the attribute if found, the default value if not found.</returns>
		public int GetAttributeValue(string name, int def)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!HasAttributes)
			{
				return def;
			}
			HtmlAttribute att = Attributes[name];
			if (att == null)
			{
				return def;
			}
			try
			{
				return Convert.ToInt32(att.Value);
			}
			catch
			{
				return def;
			}
		}

		/// <summary>
		/// Helper method to get the value of an attribute of this node. If the attribute is not found, the default value will be returned.
		/// </summary>
		/// <param name="name">The name of the attribute to get. May not be <c>null</c>.</param>
		/// <param name="def">The default value to return if not found.</param>
		/// <returns>The value of the attribute if found, the default value if not found.</returns>
		public bool GetAttributeValue(string name, bool def)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (!HasAttributes)
			{
				return def;
			}
			HtmlAttribute att = Attributes[name];
			if (att == null)
			{
				return def;
			}
			try
			{
				return Convert.ToBoolean(att.Value);
			}
			catch
			{
				return def;
			}
		}

		/// <summary>
		/// Inserts the specified node immediately after the specified reference node.
		/// </summary>
		/// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
		/// <param name="refChild">The node that is the reference node. The newNode is placed after the refNode.</param>
		/// <returns>The node being inserted.</returns>
		public HtmlNode InsertAfter(HtmlNode newChild, HtmlNode refChild)
		{
			if (newChild == null)
			{
				throw new ArgumentNullException("newChild");
			}

			if (refChild == null)
			{
				return PrependChild(newChild);
			}

			if (newChild == refChild)
			{
				return newChild;
			}

			int index = -1;

			if (_childnodes != null)
			{
				index = _childnodes[refChild];
			}
			if (index == -1)
			{
				throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
			}

			if (_childnodes != null) _childnodes.Insert(index + 1, newChild);

			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
			return newChild;
		}

		/// <summary>
		/// Inserts the specified node immediately before the specified reference node.
		/// </summary>
		/// <param name="newChild">The node to insert. May not be <c>null</c>.</param>
		/// <param name="refChild">The node that is the reference node. The newChild is placed before this node.</param>
		/// <returns>The node being inserted.</returns>
		public HtmlNode InsertBefore(HtmlNode newChild, HtmlNode refChild)
		{
			if (newChild == null)
			{
				throw new ArgumentNullException("newChild");
			}

			if (refChild == null)
			{
				return AppendChild(newChild);
			}

			if (newChild == refChild)
			{
				return newChild;
			}

			int index = -1;

			if (_childnodes != null)
			{
				index = _childnodes[refChild];
			}

			if (index == -1)
			{
				throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
			}

			if (_childnodes != null) _childnodes.Insert(index, newChild);

			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
			return newChild;
		}

		/// <summary>
		/// Adds the specified node to the beginning of the list of children of this node.
		/// </summary>
		/// <param name="newChild">The node to add. May not be <c>null</c>.</param>
		/// <returns>The node added.</returns>
		public HtmlNode PrependChild(HtmlNode newChild)
		{
			if (newChild == null)
			{
				throw new ArgumentNullException("newChild");
			}
			ChildNodes.Prepend(newChild);
			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
			return newChild;
		}

		/// <summary>
		/// Adds the specified node list to the beginning of the list of children of this node.
		/// </summary>
		/// <param name="newChildren">The node list to add. May not be <c>null</c>.</param>
		public void PrependChildren(HtmlNodeCollection newChildren)
		{
			if (newChildren == null)
			{
				throw new ArgumentNullException("newChildren");
			}

			foreach (HtmlNode newChild in newChildren)
			{
				PrependChild(newChild);
			}
		}

		/// <summary>
		/// Removes node from parent collection
		/// </summary>
		public void Remove()
		{
		    if (ParentNode != null)
		    {
                ParentNode.ChildNodes.Remove(this);
            }
        }

		/// <summary>
		/// Removes all the children and/or attributes of the current node.
		/// </summary>
		public void RemoveAll()
		{
			RemoveAllChildren();

			if (HasAttributes)
			{
				_attributes.Clear();
			}

			if ((_endnode != null) && (_endnode != this))
			{
				if (_endnode._attributes != null)
				{
					_endnode._attributes.Clear();
				}
			}
            SetChanged();
		}

		/// <summary>
		/// Removes all the children of the current node.
		/// </summary>
		public void RemoveAllChildren()
		{
			if (!HasChildNodes)
			{
				return;
			}

			if (_ownerdocument.OptionUseIdAttribute)
			{
				// remove nodes from id list
				foreach (HtmlNode node in _childnodes)
				{
					_ownerdocument.SetIdForNode(null, node.GetId());
				}
			}
			_childnodes.Clear();
            SetChanged();
		}

		/// <summary>
		/// Removes the specified child node.
		/// </summary>
		/// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
		/// <returns>The node removed.</returns>
		public HtmlNode RemoveChild(HtmlNode oldChild)
		{
			if (oldChild == null)
			{
				throw new ArgumentNullException("oldChild");
			}

			int index = -1;

			if (_childnodes != null)
			{
				index = _childnodes[oldChild];
			}

			if (index == -1)
			{
				throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
			}

			if (_childnodes != null)
				_childnodes.Remove(index);

			_ownerdocument.SetIdForNode(null, oldChild.GetId());
            SetChanged();
			return oldChild;
		}

		/// <summary>
		/// Removes the specified child node.
		/// </summary>
		/// <param name="oldChild">The node being removed. May not be <c>null</c>.</param>
		/// <param name="keepGrandChildren">true to keep grand children of the node, false otherwise.</param>
		/// <returns>The node removed.</returns>
		public HtmlNode RemoveChild(HtmlNode oldChild, bool keepGrandChildren)
		{
			if (oldChild == null)
			{
				throw new ArgumentNullException("oldChild");
			}

			if ((oldChild._childnodes != null) && keepGrandChildren)
			{
				// get prev sibling
				HtmlNode prev = oldChild.PreviousSibling;

				// reroute grand children to ourselves
				foreach (HtmlNode grandchild in oldChild._childnodes)
				{
					prev = InsertAfter(grandchild, prev);
				}
			}
			RemoveChild(oldChild);
            SetChanged();
			return oldChild;
		}

		/// <summary>
		/// Replaces the child node oldChild with newChild node.
		/// </summary>
		/// <param name="newChild">The new node to put in the child list.</param>
		/// <param name="oldChild">The node being replaced in the list.</param>
		/// <returns>The node replaced.</returns>
		public HtmlNode ReplaceChild(HtmlNode newChild, HtmlNode oldChild)
		{
			if (newChild == null)
			{
				return RemoveChild(oldChild);
			}

			if (oldChild == null)
			{
				return AppendChild(newChild);
			}

			int index = -1;

			if (_childnodes != null)
			{
				index = _childnodes[oldChild];
			}

			if (index == -1)
			{
				throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
			}

			if (_childnodes != null) _childnodes.Replace(index, newChild);

			_ownerdocument.SetIdForNode(null, oldChild.GetId());
			_ownerdocument.SetIdForNode(newChild, newChild.GetId());
            SetChanged();
			return newChild;
		}

		/// <summary>
		/// Helper method to set the value of an attribute of this node. If the attribute is not found, it will be created automatically.
		/// </summary>
		/// <param name="name">The name of the attribute to set. May not be null.</param>
		/// <param name="value">The value for the attribute.</param>
		/// <returns>The corresponding attribute instance.</returns>
		public HtmlAttribute SetAttributeValue(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			HtmlAttribute att = Attributes[name];
			if (att == null)
			{
				return Attributes.Append(_ownerdocument.CreateAttribute(name, value));
			}
			att.Value = value;
			return att;
		}

        /// <summary>
        /// Saves all the children of the node to the specified TextWriter.
        /// </summary>
        /// <param name="outText">The TextWriter to which you want to save.</param>
        /// <param name="level">Identifies the level we are in starting at root with 0</param>
        public void WriteContentTo(TextWriter outText, int level=0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
            }

            if (_childnodes == null)
			{
				return;
			}

			foreach (HtmlNode node in _childnodes)
			{
				node.WriteTo(outText, level+1);
			}
		}

		/// <summary>
		/// Saves all the children of the node to a string.
		/// </summary>
		/// <returns>The saved string.</returns>
		public string WriteContentTo()
		{
			StringWriter sw = new StringWriter();
			WriteContentTo(sw);
			sw.Flush();
			return sw.ToString();
		}

		/// <summary>
		/// Saves the current node to the specified TextWriter.
		/// </summary>
		/// <param name="outText">The TextWriter to which you want to save.</param>
        /// <param name="level">identifies the level we are in starting at root with 0</param>
		public void WriteTo(TextWriter outText, int level=0)
		{
            string html;
			switch (_nodetype)
			{
				case HtmlNodeType.Comment:
					html = ((HtmlCommentNode)this).Comment;
					if (_ownerdocument.OptionOutputAsXml)
						outText.Write("<!--" + GetXmlComment((HtmlCommentNode)this) + " -->");
					else
						outText.Write(html);
					break;

				case HtmlNodeType.Document:
					if (_ownerdocument.OptionOutputAsXml)
					{
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD
						outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().WebName + "\"?>");
#else
						outText.Write("<?xml version=\"1.0\" encoding=\"" + _ownerdocument.GetOutEncoding().BodyName + "\"?>");
#endif
						// check there is a root element
						if (_ownerdocument.DocumentNode.HasChildNodes)
						{
							int rootnodes = _ownerdocument.DocumentNode._childnodes.Count;
							if (rootnodes > 0)
							{
								HtmlNode xml = _ownerdocument.GetXmlDeclaration();
								if (xml != null)
									rootnodes--;

								if (rootnodes > 1)
								{
									if (_ownerdocument.OptionOutputUpperCase)
									{
										outText.Write("<SPAN>");
										WriteContentTo(outText, level);
										outText.Write("</SPAN>");
									}
									else
									{
										outText.Write("<span>");
										WriteContentTo(outText, level);
										outText.Write("</span>");
									}
									break;
								}
							}
						}
					}
					WriteContentTo(outText, level);
					break;

				case HtmlNodeType.Text:
					html = ((HtmlTextNode)this).Text;
					outText.Write(_ownerdocument.OptionOutputAsXml ? HtmlDocument.HtmlEncode(html) : html);
					break;

				case HtmlNodeType.Element:
					string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpper() : Name;

					if (_ownerdocument.OptionOutputOriginalCase)
						name = OriginalName;

					if (_ownerdocument.OptionOutputAsXml)
					{
						if (name.Length > 0)
						{
							if (name[0] == '?')
								// forget this one, it's been done at the document level
								break;

							if (name.Trim().Length == 0)
								break;
							name = HtmlDocument.GetXmlName(name);
						}
						else
							break;
					}

					outText.Write("<" + name);
					WriteAttributes(outText, false);

					if (HasChildNodes)
					{
						outText.Write(">");
						bool cdata = false;
						if (_ownerdocument.OptionOutputAsXml && IsCDataElement(Name))
						{
							// this code and the following tries to output things as nicely as possible for old browsers.
							cdata = true;
							outText.Write("\r\n//<![CDATA[\r\n");
						}


						if (cdata)
						{
							if (HasChildNodes)
								// child must be a text
								ChildNodes[0].WriteTo(outText, level);

							outText.Write("\r\n//]]>//\r\n");
						}
						else
							WriteContentTo(outText, level);

						outText.Write("</" + name);
						if (!_ownerdocument.OptionOutputAsXml)
							WriteAttributes(outText, true);

						outText.Write(">");
					}
					else
					{
						if (IsEmptyElement(Name))
						{
							if ((_ownerdocument.OptionWriteEmptyNodes) || (_ownerdocument.OptionOutputAsXml))
								outText.Write(" />");
							else
							{
								if (Name.Length > 0 && Name[0] == '?')
									outText.Write("?");

								outText.Write(">");
							}
						}
						else
							outText.Write("></" + name + ">");
					}
					break;
			}
		}

		/// <summary>
		/// Saves the current node to the specified XmlWriter.
		/// </summary>
		/// <param name="writer">The XmlWriter to which you want to save.</param>
		public void WriteTo(XmlWriter writer)
		{
			switch (_nodetype)
			{
				case HtmlNodeType.Comment:
					writer.WriteComment(GetXmlComment((HtmlCommentNode)this));
					break;

				case HtmlNodeType.Document:
#if SILVERLIGHT || PocketPC || METRO || NETSTANDARD
                    writer.WriteProcessingInstruction("xml",
													  "version=\"1.0\" encoding=\"" +
													  _ownerdocument.GetOutEncoding().WebName + "\"");
#else
					writer.WriteProcessingInstruction("xml",
												 "version=\"1.0\" encoding=\"" +
												 _ownerdocument.GetOutEncoding().BodyName + "\"");
#endif

					if (HasChildNodes)
					{
						foreach (HtmlNode subnode in ChildNodes)
						{
							subnode.WriteTo(writer);
						}
					}
					break;

				case HtmlNodeType.Text:
					string html = ((HtmlTextNode)this).Text;
					writer.WriteString(html);
					break;

				case HtmlNodeType.Element:
					string name = _ownerdocument.OptionOutputUpperCase ? Name.ToUpper() : Name;

					if (_ownerdocument.OptionOutputOriginalCase)
						name = OriginalName;

					writer.WriteStartElement(name);
					WriteAttributes(writer, this);

					if (HasChildNodes)
					{
						foreach (HtmlNode subnode in ChildNodes)
						{
							subnode.WriteTo(writer);
						}
					}
					writer.WriteEndElement();
					break;
			}
		}

		/// <summary>
		/// Saves the current node to a string.
		/// </summary>
		/// <returns>The saved string.</returns>
		public string WriteTo()
		{
			using (StringWriter sw = new StringWriter())
			{
				WriteTo(sw);
				sw.Flush();
				return sw.ToString();
			}
		}

		#endregion

		#region Internal Methods

        internal void SetChanged()
        {
            _changed = true;
            if (ParentNode != null)
            {
                ParentNode.SetChanged();
            }
        }

        private void UpdateHtml()
        {
            _innerhtml = WriteContentTo();
            _outerhtml = WriteTo();
            _changed = false;
        }

		internal static string GetXmlComment(HtmlCommentNode comment)
		{
			string s = comment.Comment;
			return s.Substring(4, s.Length - 7).Replace("--", " - -");
		}

		internal static void WriteAttributes(XmlWriter writer, HtmlNode node)
		{
			if (!node.HasAttributes)
			{
				return;
			}
			// we use Hashitems to make sure attributes are written only once
			foreach (HtmlAttribute att in node.Attributes.Hashitems.Values)
			{
				writer.WriteAttributeString(att.XmlName, att.Value);
			}
		}

		internal void CloseNode(HtmlNode endnode, int level=0)
        {
            if (level > HtmlDocument.MaxDepthLevel)
            {
                throw new ArgumentException(HtmlNode.DepthLevelExceptionMessage);
            }

            if (!_ownerdocument.OptionAutoCloseOnEnd)
			{
				// close all children
				if (_childnodes != null)
				{
					foreach (HtmlNode child in _childnodes)
					{
						if (child.Closed)
							continue;

						// create a fake closer node
						HtmlNode close = new HtmlNode(NodeType, _ownerdocument, -1);
						close._endnode = close;
						child.CloseNode(close, level + 1);
					}
				}
			}

			if (!Closed)
			{
				_endnode = endnode;

				if (_ownerdocument.Openednodes != null)
					_ownerdocument.Openednodes.Remove(_outerstartindex);

				HtmlNode self = Utilities.GetDictionaryValueOrNull(_ownerdocument.Lastnodes, Name);
				if (self == this)
				{
					_ownerdocument.Lastnodes.Remove(Name);
					_ownerdocument.UpdateLastParentNode();
				}

				if (endnode == this)
					return;

				// create an inner section
				_innerstartindex = _outerstartindex + _outerlength;
				_innerlength = endnode._outerstartindex - _innerstartindex;

				// update full length
				_outerlength = (endnode._outerstartindex + endnode._outerlength) - _outerstartindex;
			}
		}

		internal string GetId()
		{
			HtmlAttribute att = Attributes["id"];
			return att == null ? string.Empty : att.Value;
		}

		internal void SetId(string id)
		{
			HtmlAttribute att = Attributes["id"] ?? _ownerdocument.CreateAttribute("id");
			att.Value = id;
			_ownerdocument.SetIdForNode(this, att.Value);
            SetChanged();
		}

		internal void WriteAttribute(TextWriter outText, HtmlAttribute att)
		{
			string name;
			string quote = att.QuoteType == AttributeValueQuote.DoubleQuote ? "\"" : "'";
			if (_ownerdocument.OptionOutputAsXml)
			{
				name = _ownerdocument.OptionOutputUpperCase ? att.XmlName.ToUpper() : att.XmlName;
				if (_ownerdocument.OptionOutputOriginalCase)
					name = att.OriginalName;

				outText.Write(" " + name + "=" + quote + HtmlDocument.HtmlEncode(att.XmlValue) + quote);
			}
			else
			{
				name = _ownerdocument.OptionOutputUpperCase ? att.Name.ToUpper() : att.Name;
				if (_ownerdocument.OptionOutputOriginalCase)
					name = att.OriginalName;
				if (att.Name.Length >= 4)
				{
					if ((att.Name[0] == '<') && (att.Name[1] == '%') &&
						(att.Name[att.Name.Length - 1] == '>') && (att.Name[att.Name.Length - 2] == '%'))
					{
						outText.Write(" " + name);
						return;
					}
				}
				if (_ownerdocument.OptionOutputOptimizeAttributeValues)
					if (att.Value.IndexOfAny(new char[] { (char)10, (char)13, (char)9, ' ' }) < 0)
						outText.Write(" " + name + "=" + att.Value);
					else
						outText.Write(" " + name + "=" + quote + att.Value + quote);
				else
					outText.Write(" " + name + "=" + quote + att.Value + quote);
			}
		}

		internal void WriteAttributes(TextWriter outText, bool closing)
		{
			if (_ownerdocument.OptionOutputAsXml)
			{
				if (_attributes == null)
				{
					return;
				}
				// we use Hashitems to make sure attributes are written only once
				foreach (HtmlAttribute att in _attributes.Hashitems.Values)
				{
					WriteAttribute(outText, att);
				}
				return;
			}

			if (!closing)
			{
				if (_attributes != null)
					foreach (HtmlAttribute att in _attributes)
						WriteAttribute(outText, att);

				if (!_ownerdocument.OptionAddDebuggingAttributes) return;

				WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
				WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));

				int i = 0;
				foreach (HtmlNode n in ChildNodes)
				{
					WriteAttribute(outText, _ownerdocument.CreateAttribute("_child_" + i,
																		   n.Name));
					i++;
				}
			}
			else
			{
				if (_endnode == null || _endnode._attributes == null || _endnode == this)
					return;

				foreach (HtmlAttribute att in _endnode._attributes)
					WriteAttribute(outText, att);

				if (!_ownerdocument.OptionAddDebuggingAttributes) return;

				WriteAttribute(outText, _ownerdocument.CreateAttribute("_closed", Closed.ToString()));
				WriteAttribute(outText, _ownerdocument.CreateAttribute("_children", ChildNodes.Count.ToString()));
			}
		}

		#endregion

		#region Private Methods

		private string GetRelativeXpath()
		{
			if (ParentNode == null)
				return Name;
			if (NodeType == HtmlNodeType.Document)
				return string.Empty;

			int i = 1;
			foreach (HtmlNode node in ParentNode.ChildNodes)
			{
				if (node.Name != Name) continue;

				if (node == this)
					break;

				i++;
			}
			return Name + "[" + i + "]";
		}

        private bool IsSingleElementNode()
        {
            int count = 0;
            var element = FirstChild;

            while (element != null)
            {
                if (element.NodeType == HtmlNodeType.Element && element.OuterHtml != "\r\n")
                    count++;

                element = element.NextSibling;
            }

            return count <= 1 ? true : false;
        }
        #endregion

        #region Class Helper

	    /// <summary>
	    /// Adds one or more classes to this node.
	    /// </summary>
	    /// <param name="name">The node list to add. May not be null.</param>
	    public void AddClass(string name)
	    {
	        AddClass(name, false);
	    }

	    /// <summary>
	    /// Adds one or more classes to this node.
	    /// </summary>
	    /// <param name="name">The node list to add. May not be null.</param>
	    /// <param name="throwError">true to throw Error if class name exists, false otherwise.</param>
	    public void AddClass(string name, bool throwError)
	    {
	        var classAttributes = Attributes.AttributesWithName("class");

	        if (!IsEmpty(classAttributes))
	        {
	            foreach (HtmlAttribute att in classAttributes)
	            {
	                if (att.Value.Equals(name) || att.Value.Contains(name))
	                {
	                    if (throwError)
	                    {
	                        throw new Exception(HtmlDocument.HtmlExceptionClassExists);
	                    }
	                }
	                else
	                {
	                    SetAttributeValue(att.Name, att.Value + " " + name);
	                }
	            }
	        }
	        else
	        {
	            HtmlAttribute attribute = _ownerdocument.CreateAttribute("class", name);
	            Attributes.Append(attribute);
	        }
	    }

	    /// <summary>
	    /// Removes the class attribute from the node.
	    /// </summary>
	    public void RemoveClass()
	    {
	        RemoveClass(false);
	    }

	    /// <summary>
	    /// Removes the class attribute from the node.
	    /// </summary>
	    /// <param name="throwError">true to throw Error if class name doesn't exist, false otherwise.</param>
	    public void RemoveClass(bool throwError)
	    {
	        var classAttributes = Attributes.AttributesWithName("class");
	        if (IsEmpty(classAttributes) && throwError)
	        {
	            throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
	        }

	        foreach (var att in classAttributes)
	        {
	            Attributes.Remove(att);
	        }
	    }

	    /// <summary>
	    /// Removes the specified class from the node.
	    /// </summary>
	    /// <param name="name">The class being removed. May not be <c>null</c>.</param>
	    public void RemoveClass(string name)
	    {
	        RemoveClass(name, false);
	    }

	    /// <summary>
	    /// Removes the specified class from the node.
	    /// </summary>
	    /// <param name="name">The class being removed. May not be <c>null</c>.</param>
	    /// <param name="throwError">true to throw Error if class name doesn't exist, false otherwise.</param>
	    public void RemoveClass(string name, bool throwError)
	    {
	        var classAttributes = Attributes.AttributesWithName("class");
	        if (IsEmpty(classAttributes) && throwError)
	        {
	            throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
	        }

	        else
	        {
	            foreach (var att in classAttributes)
	            {
	                if (att.Value.Equals(name))
	                {
	                    Attributes.Remove(att);
	                }
	                else if (att.Value.Contains(name))
	                {
	                    string[] classNames = att.Value.Split(' ');

	                    string newClassNames = "";

	                    foreach (string item in classNames)
	                    {
	                        if (!item.Equals(name))
	                            newClassNames += item + " ";
	                    }

	                    newClassNames = newClassNames.Trim();
	                    SetAttributeValue(att.Name, newClassNames);
	                }
	                else
	                {
	                    if (throwError)
	                    {
	                        throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
	                    }
	                }
	                if (string.IsNullOrEmpty(att.Value))
	                {
	                    Attributes.Remove(att);
	                }
	            }
	        }
	    }

	    /// <summary>
	    /// Replaces the class name oldClass with newClass name.
	    /// </summary>
	    /// <param name="newClass">The new class name.</param>
	    /// <param name="oldClass">The class being replaced.</param>
	    public void ReplaceClass(string newClass, string oldClass)
	    {
	        ReplaceClass(newClass, oldClass, false);
	    }

	    /// <summary>
	    /// Replaces the class name oldClass with newClass name.
	    /// </summary>
	    /// <param name="newClass">The new class name.</param>
	    /// <param name="oldClass">The class being replaced.</param>
	    /// <param name="throwError">true to throw Error if class name doesn't exist, false otherwise.</param>
	    public void ReplaceClass(string newClass, string oldClass, bool throwError)
	    {
	        if (string.IsNullOrEmpty(newClass))
	        {
	            RemoveClass(oldClass);
	        }

	        if (string.IsNullOrEmpty(oldClass))
	        {
	            AddClass(newClass);
	        }

	        var classAttributes = Attributes.AttributesWithName("class");

	        if (IsEmpty(classAttributes) && throwError)
	        {
	            throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
	        }

	        foreach (var att in classAttributes)
	        {
                if (att.Value.Equals(oldClass) || att.Value.Contains(oldClass))
                {
                    string newClassNames = att.Value.Replace(oldClass, newClass);
                    SetAttributeValue(att.Name, newClassNames);
                }
                else if (throwError)
                {
                    throw new Exception(HtmlDocument.HtmlExceptionClassDoesNotExist);
                }
	        }
	    }

        private bool IsEmpty(IEnumerable en)
	    {
         
	        foreach (var c in en) { return false; }
	        return true;
	    }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlNode.Xpath.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
	public partial class HtmlNode : IXPathNavigable
	{
		
		/// <summary>
		/// Creates a new XPathNavigator object for navigating this HTML node.
		/// </summary>
		/// <returns>An XPathNavigator object. The XPathNavigator is positioned on the node from which the method was called. It is not positioned on the root of the document.</returns>
		public XPathNavigator CreateNavigator()
		{
			return new HtmlNodeNavigator(OwnerDocument, this);
		}

		/// <summary>
		/// Creates an XPathNavigator using the root of this document.
		/// </summary>
		/// <returns></returns>
		public XPathNavigator CreateRootNavigator()
		{
			return new HtmlNodeNavigator(OwnerDocument, OwnerDocument.DocumentNode);
		}

		/// <summary>
		/// Selects a list of nodes matching the <see cref="XPath"/> expression.
		/// </summary>
		/// <param name="xpath">The XPath expression.</param>
		/// <returns>An <see cref="HtmlNodeCollection"/> containing a collection of nodes matching the <see cref="XPath"/> query, or <c>null</c> if no node matched the XPath expression.</returns>
		public HtmlNodeCollection SelectNodes(string xpath)
		{
			HtmlNodeCollection list = new HtmlNodeCollection(null);

			HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
			XPathNodeIterator it = nav.Select(xpath);
			while (it.MoveNext())
			{
				HtmlNodeNavigator n = (HtmlNodeNavigator)it.Current;
				list.Add(n.CurrentNode);
			}
			if (list.Count == 0 && !OwnerDocument.OptionEmptyCollection)
			{
				return null;
			}
			return list;
		}

		/// <summary>
		/// Selects the first XmlNode that matches the XPath expression.
		/// </summary>
		/// <param name="xpath">The XPath expression. May not be null.</param>
		/// <returns>The first <see cref="HtmlNode"/> that matches the XPath query or a null reference if no matching node was found.</returns>
		public HtmlNode SelectSingleNode(string xpath)
		{
			if (xpath == null)
			{
				throw new ArgumentNullException("xpath");
			}

			HtmlNodeNavigator nav = new HtmlNodeNavigator(OwnerDocument, this);
			XPathNodeIterator it = nav.Select(xpath);
			if (!it.MoveNext())
			{
				return null;
			}

			HtmlNodeNavigator node = (HtmlNodeNavigator)it.Current;
			return node.CurrentNode;
		}

	}
}

//////////////////////////////////////////////////////////////////////////
// File: HtmlNodeCollection.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a combined list and collection of HTML nodes.
    /// </summary>
    public class HtmlNodeCollection : IList<HtmlNode>
    {
        #region Fields

        private readonly HtmlNode _parentnode;
        private readonly List<HtmlNode> _items = new List<HtmlNode>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the HtmlNodeCollection with the base parent node
        /// </summary>
        /// <param name="parentnode">The base node of the collection</param>
        public HtmlNodeCollection(HtmlNode parentnode)
        {
            _parentnode = parentnode; // may be null
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a given node from the list.
        /// </summary>
        public int this[HtmlNode node]
        {
            get
            {
                int index = GetNodeIndex(node);
                if (index == -1)
                    throw new ArgumentOutOfRangeException("node",
                                                          "Node \"" + node.CloneNode(false).OuterHtml +
                                                          "\" was not found in the collection");
                return index;
            }
        }

        /// <summary>
        /// Get node with tag name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public HtmlNode this[string nodeName]
        {
            get
            {
                nodeName = nodeName.ToLower();
                for (int i = 0; i < _items.Count; i++)
                    if (_items[i].Name.Equals(nodeName))
                        return _items[i];

                return null;
            }
        }

        #endregion

        #region IList<HtmlNode> Members

        /// <summary>
        /// Gets the number of elements actually contained in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Is collection read only
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        public HtmlNode this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        /// <summary>
        /// Add node to the collection
        /// </summary>
        /// <param name="node"></param>
        public void Add(HtmlNode node)
        {
            _items.Add(node);
        }

        /// <summary>
        /// Clears out the collection of HtmlNodes. Removes each nodes reference to parentnode, nextnode and prevnode
        /// </summary>
        public void Clear()
        {
            foreach (HtmlNode node in _items)
            {
                node.ParentNode = null;
                node.NextSibling = null;
                node.PreviousSibling = null;
            }
            _items.Clear();
        }

        /// <summary>
        /// Gets existence of node in collection
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(HtmlNode item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Copy collection to array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(HtmlNode[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<HtmlNode> IEnumerable<HtmlNode>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Get Explicit Enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(HtmlNode item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Insert node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Insert(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;

            if (index > 0)
                prev = _items[index - 1];

            if (index < _items.Count)
                next = _items[index];

            _items.Insert(index, node);

            if (prev != null)
            {
                if (node == prev)
                    throw new InvalidProgramException("Unexpected error.");

                prev._nextnode = node;
            }

            if (next != null)
                next._prevnode = node;

            node._prevnode = prev;
            if (next == node)
                throw new InvalidProgramException("Unexpected error.");

            node._nextnode = next;
            node._parentnode = _parentnode;
        }

        /// <summary>
        /// Remove node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(HtmlNode item)
        {
            int i = _items.IndexOf(item);
            RemoveAt(i);
            return true;
        }

        /// <summary>
        /// Remove <see cref="HtmlNode"/> at index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = _items[index];

            // KEEP a reference since it will be set to null
            var parentNode = _parentnode ?? oldnode._parentnode;

            if (index > 0)
                prev = _items[index - 1];

            if (index < (_items.Count - 1))
                next = _items[index + 1];

            _items.RemoveAt(index);

            if (prev != null)
            {
                if (next == prev)
                    throw new InvalidProgramException("Unexpected error.");
                prev._nextnode = next;
            }

            if (next != null)
                next._prevnode = prev;

            oldnode._prevnode = null;
            oldnode._nextnode = null;
            oldnode._parentnode = null;

            if (parentNode != null)
            {
                parentNode.SetChanged();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get first instance of node in supplied collection
        /// </summary>
        /// <param name="items"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static HtmlNode FindFirst(HtmlNodeCollection items, string name)
        {
            foreach (HtmlNode node in items)
            {
                if (node.Name.ToLower().Contains(name))
                    return node;
                if (!node.HasChildNodes) continue;
                HtmlNode returnNode = FindFirst(node.ChildNodes, name);
                if (returnNode != null)
                    return returnNode;
            }
            return null;
        }

        /// <summary>
        /// Add node to the end of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Append(HtmlNode node)
        {
            HtmlNode last = null;
            if (_items.Count > 0)
                last = _items[_items.Count - 1];

            _items.Add(node);
            node._prevnode = last;
            node._nextnode = null;
            node._parentnode = _parentnode;
            if (last == null) return;
            if (last == node)
                throw new InvalidProgramException("Unexpected error.");

            last._nextnode = node;
        }

        /// <summary>
        /// Get first instance of node with name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlNode FindFirst(string name)
        {
            return FindFirst(this, name);
        }

        /// <summary>
        /// Get index of node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(HtmlNode node)
        {
            // TODO: should we rewrite this? what would be the key of a node?
            for (int i = 0; i < _items.Count; i++)
                if (node == _items[i])
                    return i;
            return -1;
        }

        /// <summary>
        /// Add node to the beginning of the collection
        /// </summary>
        /// <param name="node"></param>
        public void Prepend(HtmlNode node)
        {
            HtmlNode first = null;
            if (_items.Count > 0)
                first = _items[0];

            _items.Insert(0, node);

            if (node == first)
                throw new InvalidProgramException("Unexpected error.");
            node._nextnode = first;
            node._prevnode = null;
            node._parentnode = _parentnode;

            if (first != null)
                first._prevnode = node;
        }

        /// <summary>
        /// Remove node at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Remove(int index)
        {
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Replace node at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        public void Replace(int index, HtmlNode node)
        {
            HtmlNode next = null;
            HtmlNode prev = null;
            HtmlNode oldnode = _items[index];

            if (index > 0)
                prev = _items[index - 1];

            if (index < (_items.Count - 1))
                next = _items[index + 1];

            _items[index] = node;

            if (prev != null)
            {
                if (node == prev)
                    throw new InvalidProgramException("Unexpected error.");
                prev._nextnode = node;
            }

            if (next != null)
                next._prevnode = node;

            node._prevnode = prev;

            if (next == node)
                throw new InvalidProgramException("Unexpected error.");

            node._nextnode = next;
            node._parentnode = _parentnode;

            oldnode._prevnode = null;
            oldnode._nextnode = null;
            oldnode._parentnode = null;
        }

        #endregion

        #region LINQ Methods

        /// <summary>
        /// Get all node descended from this collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Descendants())
                    yield return n;
        }

        /// <summary>
        /// Get all node descended from this collection with matching name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Descendants(string name)
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Descendants(name))
                    yield return n;
        }

        /// <summary>
        /// Gets all first generation elements in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
        }

        /// <summary>
        /// Gets all first generation elements matching name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Elements(string name)
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.Elements(name))
                    yield return n;
        }

        /// <summary>
        /// All first generation nodes in collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HtmlNode> Nodes()
        {
            foreach (HtmlNode item in _items)
                foreach (HtmlNode n in item.ChildNodes)
                    yield return n;
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlNodeNavigator.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an HTML navigator on an HTML document seen as a data store.
    /// </summary>
    public class HtmlNodeNavigator : XPathNavigator
    {
        #region Fields

        private int _attindex;
        private HtmlNode _currentnode;
        private readonly HtmlDocument _doc = new HtmlDocument();
        private readonly HtmlNameTable _nametable = new HtmlNameTable();

        internal bool Trace;

        #endregion

        #region Constructors

        internal HtmlNodeNavigator()
        {
            Reset();
        }

        internal HtmlNodeNavigator(HtmlDocument doc, HtmlNode currentNode)
        {
            if (currentNode == null)
            {
                throw new ArgumentNullException("currentNode");
            }
            if (currentNode.OwnerDocument != doc)
            {
                throw new ArgumentException(HtmlDocument.HtmlExceptionRefNotChild);
            }
            InternalTrace(null);

            _doc = doc;
            Reset();
            _currentnode = currentNode;
        }

        private HtmlNodeNavigator(HtmlNodeNavigator nav)
        {
            if (nav == null)
            {
                throw new ArgumentNullException("nav");
            }
            InternalTrace(null);

            _doc = nav._doc;
            _currentnode = nav._currentnode;
            _attindex = nav._attindex;
            _nametable = nav._nametable; // REVIEW: should we do this?
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public HtmlNodeNavigator(Stream stream)
        {
            _doc.Load(stream);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        public HtmlNodeNavigator(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            _doc.Load(stream, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding)
        {
            _doc.Load(stream, encoding);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            _doc.Load(stream, encoding, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the stream.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public HtmlNodeNavigator(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            _doc.Load(stream, encoding, detectEncodingFromByteOrderMarks, buffersize);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
        public HtmlNodeNavigator(TextReader reader)
        {
            _doc.Load(reader);
            Reset();
        }

#if !NETSTANDARD
        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public HtmlNodeNavigator(string path)
        {
            _doc.Load(path);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public HtmlNodeNavigator(string path, bool detectEncodingFromByteOrderMarks)
        {
            _doc.Load(path, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public HtmlNodeNavigator(string path, Encoding encoding)
        {
            _doc.Load(path, encoding);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            _doc.Load(path, encoding, detectEncodingFromByteOrderMarks);
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the HtmlNavigator and loads an HTML document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public HtmlNodeNavigator(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            _doc.Load(path, encoding, detectEncodingFromByteOrderMarks, buffersize);
            Reset();
        }
#endif
#endregion

#region Properties

        /// <summary>
        /// Gets the base URI for the current node.
        /// Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string BaseURI
        {
            get
            {
                InternalTrace(">");
                return _nametable.GetOrAdd(string.Empty);
            }
        }

        /// <summary>
        /// Gets the current HTML document.
        /// </summary>
        public HtmlDocument CurrentDocument
        {
            get { return _doc; }
        }

        /// <summary>
        /// Gets the current HTML node.
        /// </summary>
        public HtmlNode CurrentNode
        {
            get { return _currentnode; }
        }

        /// <summary>
        /// Gets a value indicating whether the current node has child nodes.
        /// </summary>
        public override bool HasAttributes
        {
            get
            {
                InternalTrace(">" + (_currentnode.Attributes.Count > 0));
                return (_currentnode.Attributes.Count > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current node has child nodes.
        /// </summary>
        public override bool HasChildren
        {
            get
            {
                InternalTrace(">" + (_currentnode.ChildNodes.Count > 0));
                return (_currentnode.ChildNodes.Count > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current node is an empty element.
        /// </summary>
        public override bool IsEmptyElement
        {
            get
            {
                InternalTrace(">" + !HasChildren);
                // REVIEW: is this ok?
                return !HasChildren;
            }
        }

        /// <summary>
        /// Gets the name of the current HTML node without the namespace prefix.
        /// </summary>
        public override string LocalName
        {
            get
            {
                if (_attindex != -1)
                {
                    InternalTrace("att>" + _currentnode.Attributes[_attindex].Name);
                    return _nametable.GetOrAdd(_currentnode.Attributes[_attindex].Name);
                }
                InternalTrace("node>" + _currentnode.Name);
                return _nametable.GetOrAdd(_currentnode.Name);
            }
        }

        /// <summary>
        /// Gets the qualified name of the current node.
        /// </summary>
        public override string Name
        {
            get
            {
                InternalTrace(">" + _currentnode.Name);
                return _nametable.GetOrAdd(_currentnode.Name);
            }
        }

        /// <summary>
        /// Gets the namespace URI (as defined in the W3C Namespace Specification) of the current node.
        /// Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string NamespaceURI
        {
            get
            {
                InternalTrace(">");
                return _nametable.GetOrAdd(string.Empty);
            }
        }

        /// <summary>
        /// Gets the <see cref="XmlNameTable"/> associated with this implementation.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get
            {
                InternalTrace(null);
                return _nametable;
            }
        }

        /// <summary>
        /// Gets the type of the current node.
        /// </summary>
        public override XPathNodeType NodeType
        {
            get
            {
                switch (_currentnode.NodeType)
                {
                    case HtmlNodeType.Comment:
                        InternalTrace(">" + XPathNodeType.Comment);
                        return XPathNodeType.Comment;

                    case HtmlNodeType.Document:
                        InternalTrace(">" + XPathNodeType.Root);
                        return XPathNodeType.Root;

                    case HtmlNodeType.Text:
                        InternalTrace(">" + XPathNodeType.Text);
                        return XPathNodeType.Text;

                    case HtmlNodeType.Element:
                        {
                            if (_attindex != -1)
                            {
                                InternalTrace(">" + XPathNodeType.Attribute);
                                return XPathNodeType.Attribute;
                            }
                            InternalTrace(">" + XPathNodeType.Element);
                            return XPathNodeType.Element;
                        }

                    default:
                        throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
                                                          _currentnode.NodeType);
                }
            }
        }

        /// <summary>
        /// Gets the prefix associated with the current node.
        /// Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string Prefix
        {
            get
            {
                InternalTrace(null);
                return _nametable.GetOrAdd(string.Empty);
            }
        }

        /// <summary>
        /// Gets the text value of the current node.
        /// </summary>
        public override string Value
        {
            get
            {
                InternalTrace("nt=" + _currentnode.NodeType);
                switch (_currentnode.NodeType)
                {
                    case HtmlNodeType.Comment:
                        InternalTrace(">" + ((HtmlCommentNode) _currentnode).Comment);
                        return ((HtmlCommentNode) _currentnode).Comment;

                    case HtmlNodeType.Document:
                        InternalTrace(">");
                        return "";

                    case HtmlNodeType.Text:
                        InternalTrace(">" + ((HtmlTextNode) _currentnode).Text);
                        return ((HtmlTextNode) _currentnode).Text;

                    case HtmlNodeType.Element:
                        {
                            if (_attindex != -1)
                            {
                                InternalTrace(">" + _currentnode.Attributes[_attindex].Value);
                                return _currentnode.Attributes[_attindex].Value;
                            }
                            return _currentnode.InnerText;
                        }

                    default:
                        throw new NotImplementedException("Internal error: Unhandled HtmlNodeType: " +
                                                          _currentnode.NodeType);
                }
            }
        }

        /// <summary>
        /// Gets the xml:lang scope for the current node.
        /// Always returns string.Empty in the case of HtmlNavigator implementation.
        /// </summary>
        public override string XmlLang
        {
            get
            {
                InternalTrace(null);
                return _nametable.GetOrAdd(string.Empty);
            }
        }

#endregion

#region Public Methods

        /// <summary>
        /// Creates a new HtmlNavigator positioned at the same node as this HtmlNavigator.
        /// </summary>
        /// <returns>A new HtmlNavigator object positioned at the same node as the original HtmlNavigator.</returns>
        public override XPathNavigator Clone()
        {
            InternalTrace(null);
            return new HtmlNodeNavigator(this);
        }

        /// <summary>
        /// Gets the value of the HTML attribute with the specified LocalName and NamespaceURI.
        /// </summary>
        /// <param name="localName">The local name of the HTML attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
        /// <returns>The value of the specified HTML attribute. String.Empty or null if a matching attribute is not found or if the navigator is not positioned on an element node.</returns>
        public override string GetAttribute(string localName, string namespaceURI)
        {
            InternalTrace("localName=" + localName + ", namespaceURI=" + namespaceURI);
            HtmlAttribute att = _currentnode.Attributes[localName];
            if (att == null)
            {
                InternalTrace(">null");
                return null;
            }
            InternalTrace(">" + att.Value);
            return att.Value;
        }

        /// <summary>
        /// Returns the value of the namespace node corresponding to the specified local name.
        /// Always returns string.Empty for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="name">The local name of the namespace node.</param>
        /// <returns>Always returns string.Empty for the HtmlNavigator implementation.</returns>
        public override string GetNamespace(string name)
        {
            InternalTrace("name=" + name);
            return string.Empty;
        }

        /// <summary>
        /// Determines whether the current HtmlNavigator is at the same position as the specified HtmlNavigator.
        /// </summary>
        /// <param name="other">The HtmlNavigator that you want to compare against.</param>
        /// <returns>true if the two navigators have the same position, otherwise, false.</returns>
        public override bool IsSamePosition(XPathNavigator other)
        {
            HtmlNodeNavigator nav = other as HtmlNodeNavigator;
            if (nav == null)
            {
                InternalTrace(">false");
                return false;
            }
            InternalTrace(">" + (nav._currentnode == _currentnode));
            return (nav._currentnode == _currentnode);
        }

        /// <summary>
        /// Moves to the same position as the specified HtmlNavigator.
        /// </summary>
        /// <param name="other">The HtmlNavigator positioned on the node that you want to move to.</param>
        /// <returns>true if successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
        public override bool MoveTo(XPathNavigator other)
        {
            HtmlNodeNavigator nav = other as HtmlNodeNavigator;
            if (nav == null)
            {
                InternalTrace(">false (nav is not an HtmlNodeNavigator)");
                return false;
            }
            InternalTrace("moveto oid=" + nav.GetHashCode()
                          + ", n:" + nav._currentnode.Name
                          + ", a:" + nav._attindex);

            if (nav._doc == _doc)
            {
                _currentnode = nav._currentnode;
                _attindex = nav._attindex;
                InternalTrace(">true");
                return true;
            }
            // we don't know how to handle that
            InternalTrace(">false (???)");
            return false;
        }

        /// <summary>
        /// Moves to the HTML attribute with matching LocalName and NamespaceURI.
        /// </summary>
        /// <param name="localName">The local name of the HTML attribute.</param>
        /// <param name="namespaceURI">The namespace URI of the attribute. Unsupported with the HtmlNavigator implementation.</param>
        /// <returns>true if the HTML attribute is found, otherwise, false. If false, the position of the navigator does not change.</returns>
        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            InternalTrace("localName=" + localName + ", namespaceURI=" + namespaceURI);
            int index = _currentnode.Attributes.GetAttributeIndex(localName);
            if (index == -1)
            {
                InternalTrace(">false");
                return false;
            }
            _attindex = index;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the first sibling of the current node.
        /// </summary>
        /// <returns>true if the navigator is successful moving to the first sibling node, false if there is no first sibling or if the navigator is currently positioned on an attribute node.</returns>
        public override bool MoveToFirst()
        {
            if (_currentnode.ParentNode == null)
            {
                InternalTrace(">false");
                return false;
            }
            if (_currentnode.ParentNode.FirstChild == null)
            {
                InternalTrace(">false");
                return false;
            }
            _currentnode = _currentnode.ParentNode.FirstChild;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the first HTML attribute.
        /// </summary>
        /// <returns>true if the navigator is successful moving to the first HTML attribute, otherwise, false.</returns>
        public override bool MoveToFirstAttribute()
        {
            if (!HasAttributes)
            {
                InternalTrace(">false");
                return false;
            }
            _attindex = 0;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the first child of the current node.
        /// </summary>
        /// <returns>true if there is a first child node, otherwise false.</returns>
        public override bool MoveToFirstChild()
        {
            if (!_currentnode.HasChildNodes)
            {
                InternalTrace(">false");
                return false;
            }
            _currentnode = _currentnode.ChildNodes[0];
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves the XPathNavigator to the first namespace node of the current element.
        /// Always returns false for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            InternalTrace(null);
            return false;
        }

        /// <summary>
        /// Moves to the node that has an attribute of type ID whose value matches the specified string.
        /// </summary>
        /// <param name="id">A string representing the ID value of the node to which you want to move. This argument does not need to be atomized.</param>
        /// <returns>true if the move was successful, otherwise false. If false, the position of the navigator is unchanged.</returns>
        public override bool MoveToId(string id)
        {
            InternalTrace("id=" + id);
            HtmlNode node = _doc.GetElementbyId(id);
            if (node == null)
            {
                InternalTrace(">false");
                return false;
            }
            _currentnode = node;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves the XPathNavigator to the namespace node with the specified local name. 
        /// Always returns false for the HtmlNavigator implementation.
        /// </summary>
        /// <param name="name">The local name of the namespace node.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToNamespace(string name)
        {
            InternalTrace("name=" + name);
            return false;
        }

        /// <summary>
        /// Moves to the next sibling of the current node.
        /// </summary>
        /// <returns>true if the navigator is successful moving to the next sibling node, false if there are no more siblings or if the navigator is currently positioned on an attribute node. If false, the position of the navigator is unchanged.</returns>
        public override bool MoveToNext()
        {
            if (_currentnode.NextSibling == null)
            {
                InternalTrace(">false");
                return false;
            }
            InternalTrace("_c=" + _currentnode.CloneNode(false).OuterHtml);
            InternalTrace("_n=" + _currentnode.NextSibling.CloneNode(false).OuterHtml);
            _currentnode = _currentnode.NextSibling;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the next HTML attribute.
        /// </summary>
        /// <returns></returns>
        public override bool MoveToNextAttribute()
        {
            InternalTrace(null);
            if (_attindex >= (_currentnode.Attributes.Count - 1))
            {
                InternalTrace(">false");
                return false;
            }
            _attindex++;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves the XPathNavigator to the next namespace node.
        /// Always returns falsefor the HtmlNavigator implementation.
        /// </summary>
        /// <param name="scope">An XPathNamespaceScope value describing the namespace scope.</param>
        /// <returns>Always returns false for the HtmlNavigator implementation.</returns>
        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            InternalTrace(null);
            return false;
        }

        /// <summary>
        /// Moves to the parent of the current node.
        /// </summary>
        /// <returns>true if there is a parent node, otherwise false.</returns>
        public override bool MoveToParent()
        {
            if (_currentnode.ParentNode == null)
            {
                InternalTrace(">false");
                return false;
            }
            _currentnode = _currentnode.ParentNode;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the previous sibling of the current node.
        /// </summary>
        /// <returns>true if the navigator is successful moving to the previous sibling node, false if there is no previous sibling or if the navigator is currently positioned on an attribute node.</returns>
        public override bool MoveToPrevious()
        {
            if (_currentnode.PreviousSibling == null)
            {
                InternalTrace(">false");
                return false;
            }
            _currentnode = _currentnode.PreviousSibling;
            InternalTrace(">true");
            return true;
        }

        /// <summary>
        /// Moves to the root node to which the current node belongs.
        /// </summary>
        public override void MoveToRoot()
        {
            _currentnode = _doc.DocumentNode;
            InternalTrace(null);
        }

#endregion

#region Internal Methods

        [Conditional("TRACE")]
        internal void InternalTrace(object traceValue)
        {
            if (!Trace)
            {
                return;
            }

#if !NETSTANDARD
            StackFrame sf = new StackFrame(1);
            string name = sf.GetMethod().Name;
#else
            string name = "";
#endif
            string nodename = _currentnode == null ? "(null)" : _currentnode.Name;
            string nodevalue;
            if (_currentnode == null)
            {
                nodevalue = "(null)";
            }
            else
            {
                switch (_currentnode.NodeType)
                {
                    case HtmlNodeType.Comment:
                        nodevalue = ((HtmlCommentNode) _currentnode).Comment;
                        break;

                    case HtmlNodeType.Document:
                        nodevalue = "";
                        break;

                    case HtmlNodeType.Text:
                        nodevalue = ((HtmlTextNode) _currentnode).Text;
                        break;

                    default:
                        nodevalue = _currentnode.CloneNode(false).OuterHtml;
                        break;
                }
            }
           
            HtmlAgilityPack.Trace.WriteLine(string.Format("oid={0},n={1},a={2},v={3},{4}", GetHashCode(), nodename, _attindex, nodevalue, traceValue), "N!" + name);
        }

#endregion

#region Private Methods

        private void Reset()
        {
            InternalTrace(null);
            _currentnode = _doc.DocumentNode;
            _attindex = -1;
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlNodeType.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents the type of a node.
    /// </summary>
    public enum HtmlNodeType
    {
        /// <summary>
        /// The root of a document.
        /// </summary>
        Document,

        /// <summary>
        /// An HTML element.
        /// </summary>
        Element,

        /// <summary>
        /// An HTML comment.
        /// </summary>
        Comment,

        /// <summary>
        /// A text node is always the child of an element or a document node.
        /// </summary>
        Text,
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlParseError.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a parsing error found during document parsing.
    /// </summary>
    public class HtmlParseError
    {
        #region Fields

        private HtmlParseErrorCode _code;
        private int _line;
        private int _linePosition;
        private string _reason;
        private string _sourceText;
        private int _streamPosition;

        #endregion

        #region Constructors

        internal HtmlParseError(
            HtmlParseErrorCode code,
            int line,
            int linePosition,
            int streamPosition,
            string sourceText,
            string reason)
        {
            _code = code;
            _line = line;
            _linePosition = linePosition;
            _streamPosition = streamPosition;
            _sourceText = sourceText;
            _reason = reason;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of error.
        /// </summary>
        public HtmlParseErrorCode Code
        {
            get { return _code; }
        }

        /// <summary>
        /// Gets the line number of this error in the document.
        /// </summary>
        public int Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Gets the column number of this error in the document.
        /// </summary>
        public int LinePosition
        {
            get { return _linePosition; }
        }

        /// <summary>
        /// Gets a description for the error.
        /// </summary>
        public string Reason
        {
            get { return _reason; }
        }

        /// <summary>
        /// Gets the the full text of the line containing the error.
        /// </summary>
        public string SourceText
        {
            get { return _sourceText; }
        }

        /// <summary>
        /// Gets the absolute stream position of this error in the document, relative to the start of the document.
        /// </summary>
        public int StreamPosition
        {
            get { return _streamPosition; }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlParseErrorCode.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents the type of parsing error.
    /// </summary>
    public enum HtmlParseErrorCode
    {
        /// <summary>
        /// A tag was not closed.
        /// </summary>
        TagNotClosed,

        /// <summary>
        /// A tag was not opened.
        /// </summary>
        TagNotOpened,

        /// <summary>
        /// There is a charset mismatch between stream and declared (META) encoding.
        /// </summary>
        CharsetMismatch,

        /// <summary>
        /// An end tag was not required.
        /// </summary>
        EndTagNotRequired,

        /// <summary>
        /// An end tag is invalid at this position.
        /// </summary>
        EndTagInvalidHere
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlTextNode.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an HTML text node.
    /// </summary>
    public class HtmlTextNode : HtmlNode
    {
        #region Fields

        private string _text;

        #endregion

        #region Constructors

        internal HtmlTextNode(HtmlDocument ownerdocument, int index)
            :
                base(HtmlNodeType.Text, ownerdocument, index)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the HTML between the start and end tags of the object. In the case of a text node, it is equals to OuterHtml.
        /// </summary>
        public override string InnerHtml
        {
            get { return OuterHtml; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets or Sets the object and its content in HTML.
        /// </summary>
        public override string OuterHtml
        {
            get
            {
                if (_text == null)
                {
                    return base.OuterHtml;
                }
                return _text;
            }
        }

        /// <summary>
        /// Gets or Sets the text of the node.
        /// </summary>
        public string Text
        {
            get
            {
                if (_text == null)
                {
                    return base.OuterHtml;
                }
                return _text;
            }
            set { _text = value; }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlWeb.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to get HTML document from HTTP.
    /// </summary>
    public partial class HtmlWeb
    {
        #region Delegates

#if !NETSTANDARD
        /// <summary>
        /// Represents the method that will handle the PostResponse event.
        /// </summary>
        public delegate void PostResponseHandler(HttpWebRequest request, HttpWebResponse response);
#endif

#if NET45 || NETSTANDARD
/// <summary>
/// Represents the method that will handle the PostResponse event.
/// </summary>
        public delegate void PostResponseHandler(HttpRequestMessage request, HttpResponseMessage response);
#endif
        /// <summary>
        /// Represents the method that will handle the PreHandleDocument event.
        /// </summary>
        public delegate void PreHandleDocumentHandler(HtmlDocument document);

#if !NETSTANDARD
        /// <summary>
        /// Represents the method that will handle the PreRequest event.
        /// </summary>
        public delegate bool PreRequestHandler(HttpWebRequest request);
#endif
#if NET45 || NETSTANDARD
/// <summary>
/// Represents the method that will handle the PostResponse event.
/// </summary>
	    public delegate bool PreRequestHandler(HttpClientHandler handler, HttpRequestMessage request);
#endif

        #endregion

        #region Fields

        private bool _autoDetectEncoding = true;
        private bool _cacheOnly;

        private string _cachePath;
        private bool _fromCache;
        private int _requestDuration;
        private Uri _responseUri;
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private int _streamBufferSize = 1024;
        private bool _useCookies;
        private bool _usingCache;
        private bool _usingCacheAndLoad;
        private string _userAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x";

        /// <summary>
        /// Occurs after an HTTP request has been executed.
        /// </summary>
        public PostResponseHandler PostResponse;

        /// <summary>
        /// Occurs before an HTML document is handled.
        /// </summary>
        public PreHandleDocumentHandler PreHandleDocument;

        /// <summary>
        /// Occurs before an HTTP request is executed.
        /// </summary>
        public PreRequestHandler PreRequest;


        #endregion

        #region Static Members

        private static Dictionary<string, string> _mimeTypes;

        internal static Dictionary<string, string> MimeTypes
        {
            get
            {
                if (_mimeTypes != null)
                    return _mimeTypes;
                //agentsmith spellcheck disable
                _mimeTypes = new Dictionary<string, string>();
                _mimeTypes.Add(".3dm", "x-world/x-3dmf");
                _mimeTypes.Add(".3dmf", "x-world/x-3dmf");
                _mimeTypes.Add(".a", "application/octet-stream");
                _mimeTypes.Add(".aab", "application/x-authorware-bin");
                _mimeTypes.Add(".aam", "application/x-authorware-map");
                _mimeTypes.Add(".aas", "application/x-authorware-seg");
                _mimeTypes.Add(".abc", "text/vnd.abc");
                _mimeTypes.Add(".acgi", "text/html");
                _mimeTypes.Add(".afl", "video/animaflex");
                _mimeTypes.Add(".ai", "application/postscript");
                _mimeTypes.Add(".aif", "audio/aiff");
                _mimeTypes.Add(".aif", "audio/x-aiff");
                _mimeTypes.Add(".aifc", "audio/aiff");
                _mimeTypes.Add(".aifc", "audio/x-aiff");
                _mimeTypes.Add(".aiff", "audio/aiff");
                _mimeTypes.Add(".aiff", "audio/x-aiff");
                _mimeTypes.Add(".aim", "application/x-aim");
                _mimeTypes.Add(".aip", "text/x-audiosoft-intra");
                _mimeTypes.Add(".ani", "application/x-navi-animation");
                _mimeTypes.Add(".aos", "application/x-nokia-9000-communicator-add-on-software");
                _mimeTypes.Add(".aps", "application/mime");
                _mimeTypes.Add(".arc", "application/octet-stream");
                _mimeTypes.Add(".arj", "application/arj");
                _mimeTypes.Add(".arj", "application/octet-stream");
                _mimeTypes.Add(".art", "image/x-jg");
                _mimeTypes.Add(".asf", "video/x-ms-asf");
                _mimeTypes.Add(".asm", "text/x-asm");
                _mimeTypes.Add(".asp", "text/asp");
                _mimeTypes.Add(".asx", "application/x-mplayer2");
                _mimeTypes.Add(".asx", "video/x-ms-asf");
                _mimeTypes.Add(".asx", "video/x-ms-asf-plugin");
                _mimeTypes.Add(".au", "audio/basic");
                _mimeTypes.Add(".au", "audio/x-au");
                _mimeTypes.Add(".avi", "application/x-troff-msvideo");
                _mimeTypes.Add(".avi", "video/avi");
                _mimeTypes.Add(".avi", "video/msvideo");
                _mimeTypes.Add(".avi", "video/x-msvideo");
                _mimeTypes.Add(".avs", "video/avs-video");
                _mimeTypes.Add(".bcpio", "application/x-bcpio");
                _mimeTypes.Add(".bin", "application/mac-binary");
                _mimeTypes.Add(".bin", "application/macbinary");
                _mimeTypes.Add(".bin", "application/octet-stream");
                _mimeTypes.Add(".bin", "application/x-binary");
                _mimeTypes.Add(".bin", "application/x-macbinary");
                _mimeTypes.Add(".bm", "image/bmp");
                _mimeTypes.Add(".bmp", "image/bmp");
                _mimeTypes.Add(".bmp", "image/x-windows-bmp");
                _mimeTypes.Add(".boo", "application/book");
                _mimeTypes.Add(".book", "application/book");
                _mimeTypes.Add(".boz", "application/x-bzip2");
                _mimeTypes.Add(".bsh", "application/x-bsh");
                _mimeTypes.Add(".bz", "application/x-bzip");
                _mimeTypes.Add(".bz2", "application/x-bzip2");
                _mimeTypes.Add(".c", "text/plain");
                _mimeTypes.Add(".c", "text/x-c");
                _mimeTypes.Add(".c++", "text/plain");
                _mimeTypes.Add(".cat", "application/vnd.ms-pki.seccat");
                _mimeTypes.Add(".cc", "text/plain");
                _mimeTypes.Add(".cc", "text/x-c");
                _mimeTypes.Add(".ccad", "application/clariscad");
                _mimeTypes.Add(".cco", "application/x-cocoa");
                _mimeTypes.Add(".cdf", "application/cdf");
                _mimeTypes.Add(".cdf", "application/x-cdf");
                _mimeTypes.Add(".cdf", "application/x-netcdf");
                _mimeTypes.Add(".cer", "application/pkix-cert");
                _mimeTypes.Add(".cer", "application/x-x509-ca-cert");
                _mimeTypes.Add(".cha", "application/x-chat");
                _mimeTypes.Add(".chat", "application/x-chat");
                _mimeTypes.Add(".class", "application/java");
                _mimeTypes.Add(".class", "application/java-byte-code");
                _mimeTypes.Add(".class", "application/x-java-class");
                _mimeTypes.Add(".com", "application/octet-stream");
                _mimeTypes.Add(".com", "text/plain");
                _mimeTypes.Add(".conf", "text/plain");
                _mimeTypes.Add(".cpio", "application/x-cpio");
                _mimeTypes.Add(".cpp", "text/x-c");
                _mimeTypes.Add(".cpt", "application/mac-compactpro");
                _mimeTypes.Add(".cpt", "application/x-compactpro");
                _mimeTypes.Add(".cpt", "application/x-cpt");
                _mimeTypes.Add(".crl", "application/pkcs-crl");
                _mimeTypes.Add(".crl", "application/pkix-crl");
                _mimeTypes.Add(".crt", "application/pkix-cert");
                _mimeTypes.Add(".crt", "application/x-x509-ca-cert");
                _mimeTypes.Add(".crt", "application/x-x509-user-cert");
                _mimeTypes.Add(".csh", "application/x-csh");
                _mimeTypes.Add(".csh", "text/x-script.csh");
                _mimeTypes.Add(".css", "application/x-pointplus");
                _mimeTypes.Add(".css", "text/css");
                _mimeTypes.Add(".cxx", "text/plain");
                _mimeTypes.Add(".dcr", "application/x-director");
                _mimeTypes.Add(".deepv", "application/x-deepv");
                _mimeTypes.Add(".def", "text/plain");
                _mimeTypes.Add(".der", "application/x-x509-ca-cert");
                _mimeTypes.Add(".dif", "video/x-dv");
                _mimeTypes.Add(".dir", "application/x-director");
                _mimeTypes.Add(".dl", "video/dl");
                _mimeTypes.Add(".dl", "video/x-dl");
                _mimeTypes.Add(".doc", "application/msword");
                _mimeTypes.Add(".dot", "application/msword");
                _mimeTypes.Add(".dp", "application/commonground");
                _mimeTypes.Add(".drw", "application/drafting");
                _mimeTypes.Add(".dump", "application/octet-stream");
                _mimeTypes.Add(".dv", "video/x-dv");
                _mimeTypes.Add(".dvi", "application/x-dvi");
                _mimeTypes.Add(".dwf", "model/vnd.dwf");
                _mimeTypes.Add(".dwg", "application/acad");
                _mimeTypes.Add(".dwg", "image/vnd.dwg");
                _mimeTypes.Add(".dwg", "image/x-dwg");
                _mimeTypes.Add(".dxf", "application/dxf");
                _mimeTypes.Add(".dxf", "image/vnd.dwg");
                _mimeTypes.Add(".dxf", "image/x-dwg");
                _mimeTypes.Add(".dxr", "application/x-director");
                _mimeTypes.Add(".el", "text/x-script.elisp");
                _mimeTypes.Add(".elc", "application/x-bytecode.elisp");
                _mimeTypes.Add(".elc", "application/x-elc");
                _mimeTypes.Add(".env", "application/x-envoy");
                _mimeTypes.Add(".eps", "application/postscript");
                _mimeTypes.Add(".es", "application/x-esrehber");
                _mimeTypes.Add(".etx", "text/x-setext");
                _mimeTypes.Add(".evy", "application/envoy");
                _mimeTypes.Add(".evy", "application/x-envoy");
                _mimeTypes.Add(".exe", "application/octet-stream");
                _mimeTypes.Add(".f", "text/plain");
                _mimeTypes.Add(".f", "text/x-fortran");
                _mimeTypes.Add(".f77", "text/x-fortran");
                _mimeTypes.Add(".f90", "text/plain");
                _mimeTypes.Add(".f90", "text/x-fortran");
                _mimeTypes.Add(".fdf", "application/vnd.fdf");
                _mimeTypes.Add(".fif", "application/fractals");
                _mimeTypes.Add(".fif", "image/fif");
                _mimeTypes.Add(".fli", "video/fli");
                _mimeTypes.Add(".fli", "video/x-fli");
                _mimeTypes.Add(".flo", "image/florian");
                _mimeTypes.Add(".flx", "text/vnd.fmi.flexstor");
                _mimeTypes.Add(".fmf", "video/x-atomic3d-feature");
                _mimeTypes.Add(".for", "text/plain");
                _mimeTypes.Add(".for", "text/x-fortran");
                _mimeTypes.Add(".fpx", "image/vnd.fpx");
                _mimeTypes.Add(".fpx", "image/vnd.net-fpx");
                _mimeTypes.Add(".frl", "application/freeloader");
                _mimeTypes.Add(".funk", "audio/make");
                _mimeTypes.Add(".g", "text/plain");
                _mimeTypes.Add(".g3", "image/g3fax");
                _mimeTypes.Add(".gif", "image/gif");
                _mimeTypes.Add(".gl", "video/gl");
                _mimeTypes.Add(".gl", "video/x-gl");
                _mimeTypes.Add(".gsd", "audio/x-gsm");
                _mimeTypes.Add(".gsm", "audio/x-gsm");
                _mimeTypes.Add(".gsp", "application/x-gsp");
                _mimeTypes.Add(".gss", "application/x-gss");
                _mimeTypes.Add(".gtar", "application/x-gtar");
                _mimeTypes.Add(".gz", "application/x-compressed");
                _mimeTypes.Add(".gz", "application/x-gzip");
                _mimeTypes.Add(".gzip", "application/x-gzip");
                _mimeTypes.Add(".gzip", "multipart/x-gzip");
                _mimeTypes.Add(".h", "text/plain");
                _mimeTypes.Add(".h", "text/x-h");
                _mimeTypes.Add(".hdf", "application/x-hdf");
                _mimeTypes.Add(".help", "application/x-helpfile");
                _mimeTypes.Add(".hgl", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hh", "text/plain");
                _mimeTypes.Add(".hh", "text/x-h");
                _mimeTypes.Add(".hlb", "text/x-script");
                _mimeTypes.Add(".hlp", "application/hlp");
                _mimeTypes.Add(".hlp", "application/x-helpfile");
                _mimeTypes.Add(".hlp", "application/x-winhelp");
                _mimeTypes.Add(".hpg", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hpgl", "application/vnd.hp-hpgl");
                _mimeTypes.Add(".hqx", "application/binhex");
                _mimeTypes.Add(".hqx", "application/binhex4");
                _mimeTypes.Add(".hqx", "application/mac-binhex");
                _mimeTypes.Add(".hqx", "application/mac-binhex40");
                _mimeTypes.Add(".hqx", "application/x-binhex40");
                _mimeTypes.Add(".hqx", "application/x-mac-binhex40");
                _mimeTypes.Add(".hta", "application/hta");
                _mimeTypes.Add(".htc", "text/x-component");
                _mimeTypes.Add(".htm", "text/html");
                _mimeTypes.Add(".html", "text/html");
                _mimeTypes.Add(".htmls", "text/html");
                _mimeTypes.Add(".htt", "text/webviewhtml");
                _mimeTypes.Add(".htx", "text/html");
                _mimeTypes.Add(".ice", "x-conference/x-cooltalk");
                _mimeTypes.Add(".ico", "image/x-icon");
                _mimeTypes.Add(".idc", "text/plain");
                _mimeTypes.Add(".ief", "image/ief");
                _mimeTypes.Add(".iefs", "image/ief");
                _mimeTypes.Add(".iges", "application/iges");
                _mimeTypes.Add(".iges", "model/iges");
                _mimeTypes.Add(".igs", "application/iges");
                _mimeTypes.Add(".igs", "model/iges");
                _mimeTypes.Add(".ima", "application/x-ima");
                _mimeTypes.Add(".imap", "application/x-httpd-imap");
                _mimeTypes.Add(".inf", "application/inf");
                _mimeTypes.Add(".ins", "application/x-internett-signup");
                _mimeTypes.Add(".ip", "application/x-ip2");
                _mimeTypes.Add(".isu", "video/x-isvideo");
                _mimeTypes.Add(".it", "audio/it");
                _mimeTypes.Add(".iv", "application/x-inventor");
                _mimeTypes.Add(".ivr", "i-world/i-vrml");
                _mimeTypes.Add(".ivy", "application/x-livescreen");
                _mimeTypes.Add(".jam", "audio/x-jam");
                _mimeTypes.Add(".jav", "text/plain");
                _mimeTypes.Add(".jav", "text/x-java-source");
                _mimeTypes.Add(".java", "text/plain");
                _mimeTypes.Add(".java", "text/x-java-source");
                _mimeTypes.Add(".jcm", "application/x-java-commerce");
                _mimeTypes.Add(".jfif", "image/jpeg");
                _mimeTypes.Add(".jfif", "image/pjpeg");
                _mimeTypes.Add(".jfif-tbnl", "image/jpeg");
                _mimeTypes.Add(".jpe", "image/jpeg");
                _mimeTypes.Add(".jpe", "image/pjpeg");
                _mimeTypes.Add(".jpeg", "image/jpeg");
                _mimeTypes.Add(".jpeg", "image/pjpeg");
                _mimeTypes.Add(".jpg", "image/jpeg");
                _mimeTypes.Add(".jpg", "image/pjpeg");
                _mimeTypes.Add(".jps", "image/x-jps");
                _mimeTypes.Add(".js", "application/x-javascript");
                _mimeTypes.Add(".js", "application/javascript");
                _mimeTypes.Add(".js", "application/ecmascript");
                _mimeTypes.Add(".js", "text/javascript");
                _mimeTypes.Add(".js", "text/ecmascript");
                _mimeTypes.Add(".jut", "image/jutvision");
                _mimeTypes.Add(".kar", "audio/midi");
                _mimeTypes.Add(".kar", "music/x-karaoke");
                _mimeTypes.Add(".ksh", "application/x-ksh");
                _mimeTypes.Add(".ksh", "text/x-script.ksh");
                _mimeTypes.Add(".la", "audio/nspaudio");
                _mimeTypes.Add(".la", "audio/x-nspaudio");
                _mimeTypes.Add(".lam", "audio/x-liveaudio");
                _mimeTypes.Add(".latex", "application/x-latex");
                _mimeTypes.Add(".lha", "application/lha");
                _mimeTypes.Add(".lha", "application/octet-stream");
                _mimeTypes.Add(".lha", "application/x-lha");
                _mimeTypes.Add(".lhx", "application/octet-stream");
                _mimeTypes.Add(".list", "text/plain");
                _mimeTypes.Add(".lma", "audio/nspaudio");
                _mimeTypes.Add(".lma", "audio/x-nspaudio");
                _mimeTypes.Add(".log", "text/plain");
                _mimeTypes.Add(".lsp", "application/x-lisp");
                _mimeTypes.Add(".lsp", "text/x-script.lisp");
                _mimeTypes.Add(".lst", "text/plain");
                _mimeTypes.Add(".lsx", "text/x-la-asf");
                _mimeTypes.Add(".ltx", "application/x-latex");
                _mimeTypes.Add(".lzh", "application/octet-stream");
                _mimeTypes.Add(".lzh", "application/x-lzh");
                _mimeTypes.Add(".lzx", "application/lzx");
                _mimeTypes.Add(".lzx", "application/octet-stream");
                _mimeTypes.Add(".lzx", "application/x-lzx");
                _mimeTypes.Add(".m", "text/plain");
                _mimeTypes.Add(".m", "text/x-m");
                _mimeTypes.Add(".m1v", "video/mpeg");
                _mimeTypes.Add(".m2a", "audio/mpeg");
                _mimeTypes.Add(".m2v", "video/mpeg");
                _mimeTypes.Add(".m3u", "audio/x-mpequrl");
                _mimeTypes.Add(".man", "application/x-troff-man");
                _mimeTypes.Add(".map", "application/x-navimap");
                _mimeTypes.Add(".mar", "text/plain");
                _mimeTypes.Add(".mbd", "application/mbedlet");
                _mimeTypes.Add(".mc$", "application/x-magic-cap-package-1.0");
                _mimeTypes.Add(".mcd", "application/mcad");
                _mimeTypes.Add(".mcd", "application/x-mathcad");
                _mimeTypes.Add(".mcf", "image/vasa");
                _mimeTypes.Add(".mcf", "text/mcf");
                _mimeTypes.Add(".mcp", "application/netmc");
                _mimeTypes.Add(".me", "application/x-troff-me");
                _mimeTypes.Add(".mht", "message/rfc822");
                _mimeTypes.Add(".mhtml", "message/rfc822");
                _mimeTypes.Add(".mid", "application/x-midi");
                _mimeTypes.Add(".mid", "audio/midi");
                _mimeTypes.Add(".mid", "audio/x-mid");
                _mimeTypes.Add(".mid", "audio/x-midi");
                _mimeTypes.Add(".mid", "music/crescendo");
                _mimeTypes.Add(".mid", "x-music/x-midi");
                _mimeTypes.Add(".midi", "application/x-midi");
                _mimeTypes.Add(".midi", "audio/midi");
                _mimeTypes.Add(".midi", "audio/x-mid");
                _mimeTypes.Add(".midi", "audio/x-midi");
                _mimeTypes.Add(".midi", "music/crescendo");
                _mimeTypes.Add(".midi", "x-music/x-midi");
                _mimeTypes.Add(".mif", "application/x-frame");
                _mimeTypes.Add(".mif", "application/x-mif");
                _mimeTypes.Add(".mime", "message/rfc822");
                _mimeTypes.Add(".mime", "www/mime");
                _mimeTypes.Add(".mjf", "audio/x-vnd.audioexplosion.mjuicemediafile");
                _mimeTypes.Add(".mjpg", "video/x-motion-jpeg");
                _mimeTypes.Add(".mm", "application/base64");
                _mimeTypes.Add(".mm", "application/x-meme");
                _mimeTypes.Add(".mme", "application/base64");
                _mimeTypes.Add(".mod", "audio/mod");
                _mimeTypes.Add(".mod", "audio/x-mod");
                _mimeTypes.Add(".moov", "video/quicktime");
                _mimeTypes.Add(".mov", "video/quicktime");
                _mimeTypes.Add(".movie", "video/x-sgi-movie");
                _mimeTypes.Add(".mp2", "audio/mpeg");
                _mimeTypes.Add(".mp2", "audio/x-mpeg");
                _mimeTypes.Add(".mp2", "video/mpeg");
                _mimeTypes.Add(".mp2", "video/x-mpeg");
                _mimeTypes.Add(".mp2", "video/x-mpeq2a");
                _mimeTypes.Add(".mp3", "audio/mpeg3");
                _mimeTypes.Add(".mp3", "audio/x-mpeg-3");
                _mimeTypes.Add(".mp3", "video/mpeg");
                _mimeTypes.Add(".mp3", "video/x-mpeg");
                _mimeTypes.Add(".mpa", "audio/mpeg");
                _mimeTypes.Add(".mpa", "video/mpeg");
                _mimeTypes.Add(".mpc", "application/x-project");
                _mimeTypes.Add(".mpe", "video/mpeg");
                _mimeTypes.Add(".mpeg", "video/mpeg");
                _mimeTypes.Add(".mpg", "audio/mpeg");
                _mimeTypes.Add(".mpg", "video/mpeg");
                _mimeTypes.Add(".mpga", "audio/mpeg");
                _mimeTypes.Add(".mpp", "application/vnd.ms-project");
                _mimeTypes.Add(".mpt", "application/x-project");
                _mimeTypes.Add(".mpv", "application/x-project");
                _mimeTypes.Add(".mpx", "application/x-project");
                _mimeTypes.Add(".mrc", "application/marc");
                _mimeTypes.Add(".ms", "application/x-troff-ms");
                _mimeTypes.Add(".mv", "video/x-sgi-movie");
                _mimeTypes.Add(".my", "audio/make");
                _mimeTypes.Add(".mzz", "application/x-vnd.audioexplosion.mzz");
                _mimeTypes.Add(".nap", "image/naplps");
                _mimeTypes.Add(".naplps", "image/naplps");
                _mimeTypes.Add(".nc", "application/x-netcdf");
                _mimeTypes.Add(".ncm", "application/vnd.nokia.configuration-message");
                _mimeTypes.Add(".nif", "image/x-niff");
                _mimeTypes.Add(".niff", "image/x-niff");
                _mimeTypes.Add(".nix", "application/x-mix-transfer");
                _mimeTypes.Add(".nsc", "application/x-conference");
                _mimeTypes.Add(".nvd", "application/x-navidoc");
                _mimeTypes.Add(".o", "application/octet-stream");
                _mimeTypes.Add(".oda", "application/oda");
                _mimeTypes.Add(".omc", "application/x-omc");
                _mimeTypes.Add(".omcd", "application/x-omcdatamaker");
                _mimeTypes.Add(".omcr", "application/x-omcregerator");
                _mimeTypes.Add(".p", "text/x-pascal");
                _mimeTypes.Add(".p10", "application/pkcs10");
                _mimeTypes.Add(".p10", "application/x-pkcs10");
                _mimeTypes.Add(".p12", "application/pkcs-12");
                _mimeTypes.Add(".p12", "application/x-pkcs12");
                _mimeTypes.Add(".p7a", "application/x-pkcs7-signature");
                _mimeTypes.Add(".p7c", "application/pkcs7-mime");
                _mimeTypes.Add(".p7c", "application/x-pkcs7-mime");
                _mimeTypes.Add(".p7m", "application/pkcs7-mime");
                _mimeTypes.Add(".p7m", "application/x-pkcs7-mime");
                _mimeTypes.Add(".p7r", "application/x-pkcs7-certreqresp");
                _mimeTypes.Add(".p7s", "application/pkcs7-signature");
                _mimeTypes.Add(".part", "application/pro_eng");
                _mimeTypes.Add(".pas", "text/pascal");
                _mimeTypes.Add(".pbm", "image/x-portable-bitmap");
                _mimeTypes.Add(".pcl", "application/vnd.hp-pcl");
                _mimeTypes.Add(".pcl", "application/x-pcl");
                _mimeTypes.Add(".pct", "image/x-pict");
                _mimeTypes.Add(".pcx", "image/x-pcx");
                _mimeTypes.Add(".pdb", "chemical/x-pdb");
                _mimeTypes.Add(".pdf", "application/pdf");
                _mimeTypes.Add(".pfunk", "audio/make");
                _mimeTypes.Add(".pfunk", "audio/make.my.funk");
                _mimeTypes.Add(".pgm", "image/x-portable-graymap");
                _mimeTypes.Add(".pgm", "image/x-portable-greymap");
                _mimeTypes.Add(".pic", "image/pict");
                _mimeTypes.Add(".pict", "image/pict");
                _mimeTypes.Add(".pkg", "application/x-newton-compatible-pkg");
                _mimeTypes.Add(".pko", "application/vnd.ms-pki.pko");
                _mimeTypes.Add(".pl", "text/plain");
                _mimeTypes.Add(".pl", "text/x-script.perl");
                _mimeTypes.Add(".plx", "application/x-pixclscript");
                _mimeTypes.Add(".pm", "image/x-xpixmap");
                _mimeTypes.Add(".pm", "text/x-script.perl-module");
                _mimeTypes.Add(".pm4", "application/x-pagemaker");
                _mimeTypes.Add(".pm5", "application/x-pagemaker");
                _mimeTypes.Add(".png", "image/png");
                _mimeTypes.Add(".pnm", "application/x-portable-anymap");
                _mimeTypes.Add(".pnm", "image/x-portable-anymap");
                _mimeTypes.Add(".pot", "application/mspowerpoint");
                _mimeTypes.Add(".pot", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".pov", "model/x-pov");
                _mimeTypes.Add(".ppa", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppm", "image/x-portable-pixmap");
                _mimeTypes.Add(".pps", "application/mspowerpoint");
                _mimeTypes.Add(".pps", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppt", "application/mspowerpoint");
                _mimeTypes.Add(".ppt", "application/powerpoint");
                _mimeTypes.Add(".ppt", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".ppt", "application/x-mspowerpoint");
                _mimeTypes.Add(".ppz", "application/mspowerpoint");
                _mimeTypes.Add(".pre", "application/x-freelance");
                _mimeTypes.Add(".prt", "application/pro_eng");
                _mimeTypes.Add(".ps", "application/postscript");
                _mimeTypes.Add(".psd", "application/octet-stream");
                _mimeTypes.Add(".pvu", "paleovu/x-pv");
                _mimeTypes.Add(".pwz", "application/vnd.ms-powerpoint");
                _mimeTypes.Add(".py", "text/x-script.phyton");
                _mimeTypes.Add(".pyc", "applicaiton/x-bytecode.python");
                _mimeTypes.Add(".qcp", "audio/vnd.qcelp");
                _mimeTypes.Add(".qd3", "x-world/x-3dmf");
                _mimeTypes.Add(".qd3d", "x-world/x-3dmf");
                _mimeTypes.Add(".qif", "image/x-quicktime");
                _mimeTypes.Add(".qt", "video/quicktime");
                _mimeTypes.Add(".qtc", "video/x-qtc");
                _mimeTypes.Add(".qti", "image/x-quicktime");
                _mimeTypes.Add(".qtif", "image/x-quicktime");
                _mimeTypes.Add(".ra", "audio/x-pn-realaudio");
                _mimeTypes.Add(".ra", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".ra", "audio/x-realaudio");
                _mimeTypes.Add(".ram", "audio/x-pn-realaudio");
                _mimeTypes.Add(".ras", "application/x-cmu-raster");
                _mimeTypes.Add(".ras", "image/cmu-raster");
                _mimeTypes.Add(".ras", "image/x-cmu-raster");
                _mimeTypes.Add(".rast", "image/cmu-raster");
                _mimeTypes.Add(".rexx", "text/x-script.rexx");
                _mimeTypes.Add(".rf", "image/vnd.rn-realflash");
                _mimeTypes.Add(".rgb", "image/x-rgb");
                _mimeTypes.Add(".rm", "application/vnd.rn-realmedia");
                _mimeTypes.Add(".rm", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmi", "audio/mid");
                _mimeTypes.Add(".rmm", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmp", "audio/x-pn-realaudio");
                _mimeTypes.Add(".rmp", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".rng", "application/ringing-tones");
                _mimeTypes.Add(".rng", "application/vnd.nokia.ringing-tone");
                _mimeTypes.Add(".rnx", "application/vnd.rn-realplayer");
                _mimeTypes.Add(".roff", "application/x-troff");
                _mimeTypes.Add(".rp", "image/vnd.rn-realpix");
                _mimeTypes.Add(".rpm", "audio/x-pn-realaudio-plugin");
                _mimeTypes.Add(".rt", "text/richtext");
                _mimeTypes.Add(".rt", "text/vnd.rn-realtext");
                _mimeTypes.Add(".rtf", "application/rtf");
                _mimeTypes.Add(".rtf", "application/x-rtf");
                _mimeTypes.Add(".rtf", "text/richtext");
                _mimeTypes.Add(".rtx", "application/rtf");
                _mimeTypes.Add(".rtx", "text/richtext");
                _mimeTypes.Add(".rv", "video/vnd.rn-realvideo");
                _mimeTypes.Add(".s", "text/x-asm");
                _mimeTypes.Add(".s3m", "audio/s3m");
                _mimeTypes.Add(".saveme", "application/octet-stream");
                _mimeTypes.Add(".sbk", "application/x-tbook");
                _mimeTypes.Add(".scm", "application/x-lotusscreencam");
                _mimeTypes.Add(".scm", "text/x-script.guile");
                _mimeTypes.Add(".scm", "text/x-script.scheme");
                _mimeTypes.Add(".scm", "video/x-scm");
                _mimeTypes.Add(".sdml", "text/plain");
                _mimeTypes.Add(".sdp", "application/sdp");
                _mimeTypes.Add(".sdp", "application/x-sdp");
                _mimeTypes.Add(".sdr", "application/sounder");
                _mimeTypes.Add(".sea", "application/sea");
                _mimeTypes.Add(".sea", "application/x-sea");
                _mimeTypes.Add(".set", "application/set");
                _mimeTypes.Add(".sgm", "text/sgml");
                _mimeTypes.Add(".sgm", "text/x-sgml");
                _mimeTypes.Add(".sgml", "text/sgml");
                _mimeTypes.Add(".sgml", "text/x-sgml");
                _mimeTypes.Add(".sh", "application/x-bsh");
                _mimeTypes.Add(".sh", "application/x-sh");
                _mimeTypes.Add(".sh", "application/x-shar");
                _mimeTypes.Add(".sh", "text/x-script.sh");
                _mimeTypes.Add(".shar", "application/x-bsh");
                _mimeTypes.Add(".shar", "application/x-shar");
                _mimeTypes.Add(".shtml", "text/html");
                _mimeTypes.Add(".shtml", "text/x-server-parsed-html");
                _mimeTypes.Add(".sid", "audio/x-psid");
                _mimeTypes.Add(".sit", "application/x-sit");
                _mimeTypes.Add(".sit", "application/x-stuffit");
                _mimeTypes.Add(".skd", "application/x-koan");
                _mimeTypes.Add(".skm", "application/x-koan");
                _mimeTypes.Add(".skp", "application/x-koan");
                _mimeTypes.Add(".skt", "application/x-koan");
                _mimeTypes.Add(".sl", "application/x-seelogo");
                _mimeTypes.Add(".smi", "application/smil");
                _mimeTypes.Add(".smil", "application/smil");
                _mimeTypes.Add(".snd", "audio/basic");
                _mimeTypes.Add(".snd", "audio/x-adpcm");
                _mimeTypes.Add(".sol", "application/solids");
                _mimeTypes.Add(".spc", "application/x-pkcs7-certificates");
                _mimeTypes.Add(".spc", "text/x-speech");
                _mimeTypes.Add(".spl", "application/futuresplash");
                _mimeTypes.Add(".spr", "application/x-sprite");
                _mimeTypes.Add(".sprite", "application/x-sprite");
                _mimeTypes.Add(".src", "application/x-wais-source");
                _mimeTypes.Add(".ssi", "text/x-server-parsed-html");
                _mimeTypes.Add(".ssm", "application/streamingmedia");
                _mimeTypes.Add(".sst", "application/vnd.ms-pki.certstore");
                _mimeTypes.Add(".step", "application/step");
                _mimeTypes.Add(".stl", "application/sla");
                _mimeTypes.Add(".stl", "application/vnd.ms-pki.stl");
                _mimeTypes.Add(".stl", "application/x-navistyle");
                _mimeTypes.Add(".stp", "application/step");
                _mimeTypes.Add(".sv4cpio", "application/x-sv4cpio");
                _mimeTypes.Add(".sv4crc", "application/x-sv4crc");
                _mimeTypes.Add(".svf", "image/vnd.dwg");
                _mimeTypes.Add(".svf", "image/x-dwg");
                _mimeTypes.Add(".svr", "application/x-world");
                _mimeTypes.Add(".svr", "x-world/x-svr");
                _mimeTypes.Add(".swf", "application/x-shockwave-flash");
                _mimeTypes.Add(".t", "application/x-troff");
                _mimeTypes.Add(".talk", "text/x-speech");
                _mimeTypes.Add(".tar", "application/x-tar");
                _mimeTypes.Add(".tbk", "application/toolbook");
                _mimeTypes.Add(".tbk", "application/x-tbook");
                _mimeTypes.Add(".tcl", "application/x-tcl");
                _mimeTypes.Add(".tcl", "text/x-script.tcl");
                _mimeTypes.Add(".tcsh", "text/x-script.tcsh");
                _mimeTypes.Add(".tex", "application/x-tex");
                _mimeTypes.Add(".texi", "application/x-texinfo");
                _mimeTypes.Add(".texinfo", "application/x-texinfo");
                _mimeTypes.Add(".text", "application/plain");
                _mimeTypes.Add(".text", "text/plain");
                _mimeTypes.Add(".tgz", "application/gnutar");
                _mimeTypes.Add(".tgz", "application/x-compressed");
                _mimeTypes.Add(".tif", "image/tiff");
                _mimeTypes.Add(".tif", "image/x-tiff");
                _mimeTypes.Add(".tiff", "image/tiff");
                _mimeTypes.Add(".tiff", "image/x-tiff");
                _mimeTypes.Add(".tr", "application/x-troff");
                _mimeTypes.Add(".tsi", "audio/tsp-audio");
                _mimeTypes.Add(".tsp", "application/dsptype");
                _mimeTypes.Add(".tsp", "audio/tsplayer");
                _mimeTypes.Add(".tsv", "text/tab-separated-values");
                _mimeTypes.Add(".turbot", "image/florian");
                _mimeTypes.Add(".txt", "text/plain");
                _mimeTypes.Add(".uil", "text/x-uil");
                _mimeTypes.Add(".uni", "text/uri-list");
                _mimeTypes.Add(".unis", "text/uri-list");
                _mimeTypes.Add(".unv", "application/i-deas");
                _mimeTypes.Add(".uri", "text/uri-list");
                _mimeTypes.Add(".uris", "text/uri-list");
                _mimeTypes.Add(".ustar", "application/x-ustar");
                _mimeTypes.Add(".ustar", "multipart/x-ustar");
                _mimeTypes.Add(".uu", "application/octet-stream");
                _mimeTypes.Add(".uu", "text/x-uuencode");
                _mimeTypes.Add(".uue", "text/x-uuencode");
                _mimeTypes.Add(".vcd", "application/x-cdlink");
                _mimeTypes.Add(".vcs", "text/x-vcalendar");
                _mimeTypes.Add(".vda", "application/vda");
                _mimeTypes.Add(".vdo", "video/vdo");
                _mimeTypes.Add(".vew", "application/groupwise");
                _mimeTypes.Add(".viv", "video/vivo");
                _mimeTypes.Add(".viv", "video/vnd.vivo");
                _mimeTypes.Add(".vivo", "video/vivo");
                _mimeTypes.Add(".vivo", "video/vnd.vivo");
                _mimeTypes.Add(".vmd", "application/vocaltec-media-desc");
                _mimeTypes.Add(".vmf", "application/vocaltec-media-file");
                _mimeTypes.Add(".voc", "audio/voc");
                _mimeTypes.Add(".voc", "audio/x-voc");
                _mimeTypes.Add(".vos", "video/vosaic");
                _mimeTypes.Add(".vox", "audio/voxware");
                _mimeTypes.Add(".vqe", "audio/x-twinvq-plugin");
                _mimeTypes.Add(".vqf", "audio/x-twinvq");
                _mimeTypes.Add(".vql", "audio/x-twinvq-plugin");
                _mimeTypes.Add(".vrml", "application/x-vrml");
                _mimeTypes.Add(".vrml", "model/vrml");
                _mimeTypes.Add(".vrml", "x-world/x-vrml");
                _mimeTypes.Add(".vrt", "x-world/x-vrt");
                _mimeTypes.Add(".vsd", "application/x-visio");
                _mimeTypes.Add(".vst", "application/x-visio");
                _mimeTypes.Add(".vsw", "application/x-visio");
                _mimeTypes.Add(".w60", "application/wordperfect6.0");
                _mimeTypes.Add(".w61", "application/wordperfect6.1");
                _mimeTypes.Add(".w6w", "application/msword");
                _mimeTypes.Add(".wav", "audio/wav");
                _mimeTypes.Add(".wav", "audio/x-wav");
                _mimeTypes.Add(".wb1", "application/x-qpro");
                _mimeTypes.Add(".wbmp", "image/vnd.wap.wbmp");
                _mimeTypes.Add(".web", "application/vnd.xara");
                _mimeTypes.Add(".wiz", "application/msword");
                _mimeTypes.Add(".wk1", "application/x-123");
                _mimeTypes.Add(".wmf", "windows/metafile");
                _mimeTypes.Add(".wml", "text/vnd.wap.wml");
                _mimeTypes.Add(".wmlc", "application/vnd.wap.wmlc");
                _mimeTypes.Add(".wmls", "text/vnd.wap.wmlscript");
                _mimeTypes.Add(".wmlsc", "application/vnd.wap.wmlscriptc");
                _mimeTypes.Add(".word", "application/msword");
                _mimeTypes.Add(".wp", "application/wordperfect");
                _mimeTypes.Add(".wp5", "application/wordperfect");
                _mimeTypes.Add(".wp5", "application/wordperfect6.0");
                _mimeTypes.Add(".wp6", "application/wordperfect");
                _mimeTypes.Add(".wpd", "application/wordperfect");
                _mimeTypes.Add(".wpd", "application/x-wpwin");
                _mimeTypes.Add(".wq1", "application/x-lotus");
                _mimeTypes.Add(".wri", "application/mswrite");
                _mimeTypes.Add(".wri", "application/x-wri");
                _mimeTypes.Add(".wrl", "application/x-world");
                _mimeTypes.Add(".wrl", "model/vrml");
                _mimeTypes.Add(".wrl", "x-world/x-vrml");
                _mimeTypes.Add(".wrz", "model/vrml");
                _mimeTypes.Add(".wrz", "x-world/x-vrml");
                _mimeTypes.Add(".wsc", "text/scriplet");
                _mimeTypes.Add(".wsrc", "application/x-wais-source");
                _mimeTypes.Add(".wtk", "application/x-wintalk");
                _mimeTypes.Add(".xbm", "image/x-xbitmap");
                _mimeTypes.Add(".xbm", "image/x-xbm");
                _mimeTypes.Add(".xbm", "image/xbm");
                _mimeTypes.Add(".xdr", "video/x-amt-demorun");
                _mimeTypes.Add(".xgz", "xgl/drawing");
                _mimeTypes.Add(".xif", "image/vnd.xiff");
                _mimeTypes.Add(".xl", "application/excel");
                _mimeTypes.Add(".xla", "application/excel");
                _mimeTypes.Add(".xla", "application/x-excel");
                _mimeTypes.Add(".xla", "application/x-msexcel");
                _mimeTypes.Add(".xlb", "application/excel");
                _mimeTypes.Add(".xlb", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlb", "application/x-excel");
                _mimeTypes.Add(".xlc", "application/excel");
                _mimeTypes.Add(".xlc", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlc", "application/x-excel");
                _mimeTypes.Add(".xld", "application/excel");
                _mimeTypes.Add(".xld", "application/x-excel");
                _mimeTypes.Add(".xlk", "application/excel");
                _mimeTypes.Add(".xlk", "application/x-excel");
                _mimeTypes.Add(".xll", "application/excel");
                _mimeTypes.Add(".xll", "application/vnd.ms-excel");
                _mimeTypes.Add(".xll", "application/x-excel");
                _mimeTypes.Add(".xlm", "application/excel");
                _mimeTypes.Add(".xlm", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlm", "application/x-excel");
                _mimeTypes.Add(".xls", "application/excel");
                _mimeTypes.Add(".xls", "application/vnd.ms-excel");
                _mimeTypes.Add(".xls", "application/x-excel");
                _mimeTypes.Add(".xls", "application/x-msexcel");
                _mimeTypes.Add(".xlt", "application/excel");
                _mimeTypes.Add(".xlt", "application/x-excel");
                _mimeTypes.Add(".xlv", "application/excel");
                _mimeTypes.Add(".xlv", "application/x-excel");
                _mimeTypes.Add(".xlw", "application/excel");
                _mimeTypes.Add(".xlw", "application/vnd.ms-excel");
                _mimeTypes.Add(".xlw", "application/x-excel");
                _mimeTypes.Add(".xlw", "application/x-msexcel");
                _mimeTypes.Add(".xm", "audio/xm");
                _mimeTypes.Add(".xml", "application/xml");
                _mimeTypes.Add(".xml", "text/xml");
                _mimeTypes.Add(".xmz", "xgl/movie");
                _mimeTypes.Add(".xpix", "application/x-vnd.ls-xpix");
                _mimeTypes.Add(".xpm", "image/x-xpixmap");
                _mimeTypes.Add(".xpm", "image/xpm");
                _mimeTypes.Add(".x-png", "image/png");
                _mimeTypes.Add(".xsr", "video/x-amt-showrun");
                _mimeTypes.Add(".xwd", "image/x-xwd");
                _mimeTypes.Add(".xwd", "image/x-xwindowdump");
                _mimeTypes.Add(".xyz", "chemical/x-pdb");
                _mimeTypes.Add(".z", "application/x-compress");
                _mimeTypes.Add(".z", "application/x-compressed");
                _mimeTypes.Add(".zip", "application/x-compressed");
                _mimeTypes.Add(".zip", "application/x-zip-compressed");
                _mimeTypes.Add(".zip", "application/zip");
                _mimeTypes.Add(".zip", "multipart/x-zip");
                _mimeTypes.Add(".zoo", "application/octet-stream");
                _mimeTypes.Add(".zsh", "text/x-script.zsh");
                //agentsmith spellcheck enable
                return _mimeTypes;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets a value indicating if document encoding must be automatically detected.
        /// </summary>
        public bool AutoDetectEncoding
        {
            get { return _autoDetectEncoding; }
            set { _autoDetectEncoding = value; }
        }

        private Encoding _encoding;
        /// <summary>
        /// Gets or sets the Encoding used to override the response stream from any web request
        /// </summary>
        public Encoding OverrideEncoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        /// Gets or Sets a value indicating whether to get document only from the cache.
        /// If this is set to true and document is not found in the cache, nothing will be loaded.
        /// </summary>
        public bool CacheOnly
        {
            get { return _cacheOnly; }
            set
            {
                if ((value) && !UsingCache)
                {
                    throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
                }
                _cacheOnly = value;
            }
        }

        /// <summary>
        /// Gets or Sets the cache path. If null, no caching mechanism will be used.
        /// </summary>
        public string CachePath
        {
            get { return _cachePath; }
            set { _cachePath = value; }
        }

        /// <summary>
        /// Gets a value indicating if the last document was retrieved from the cache.
        /// </summary>
        public bool FromCache
        {
            get { return _fromCache; }
        }

        /// <summary>
        /// Gets the last request duration in milliseconds.
        /// </summary>
        public int RequestDuration
        {
            get { return _requestDuration; }
        }

        /// <summary>
        /// Gets the URI of the Internet resource that actually responded to the request.
        /// </summary>
        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        /// <summary>
        /// Gets the last request status.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary>
        /// Gets or Sets the size of the buffer used for memory operations.
        /// </summary>
        public int StreamBufferSize
        {
            get { return _streamBufferSize; }
            set
            {
                if (_streamBufferSize <= 0)
                {
                    throw new ArgumentException("Size must be greater than zero.");
                }
                _streamBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or Sets a value indicating if cookies will be stored.
        /// </summary>
        public bool UseCookies
        {
            get { return _useCookies; }
            set { _useCookies = value; }
        }

        /// <summary>
        /// Gets or Sets the User Agent HTTP 1.1 header sent on any webrequest
        /// </summary>
        public string UserAgent { get { return _userAgent; } set { _userAgent = value; } }

        /// <summary>
        /// Gets or Sets a value indicating whether the caching mechanisms should be used or not.
        /// </summary>
        public bool UsingCache
        {
            get { return _cachePath != null && _usingCache; }
            set
            {
                if ((value) && (_cachePath == null))
                {
                    throw new HtmlWebException("You need to define a CachePath first.");
                }
                _usingCache = value;
            }
        }

        #endregion

        #region Public Methods
#if !NETSTANDARD
        /// <summary>
        /// Gets the MIME content type for a given path extension.
        /// </summary>
        /// <param name="extension">The input path extension.</param>
        /// <param name="def">The default content type to return if any error occurs.</param>
        /// <returns>The path extension's MIME content type.</returns>
        public static string GetContentTypeForExtension(string extension, string def)
        {
            var helper = new PermissionHelper();
            if (string.IsNullOrEmpty(extension))
            {
                return def;
            }
            string contentType = "";
            if (!helper.GetIsRegistryAvailable())
            {
                if (MimeTypes.ContainsKey(extension))
                    contentType = MimeTypes[extension];
                else
                    contentType = def;
            }

            if (!helper.GetIsDnsAvailable())
            {
                //do something.... not at full trust
                try
                {
                    RegistryKey reg = Registry.ClassesRoot;
                    reg = reg.OpenSubKey(extension, false);
                    if (reg != null) contentType = (string)reg.GetValue("", def);
                }
                catch (Exception)
                {
                    contentType = def;
                }
            }
            return contentType;
        }

        /// <summary>
        /// Gets the path extension for a given MIME content type.
        /// </summary>
        /// <param name="contentType">The input MIME content type.</param>
        /// <param name="def">The default path extension to return if any error occurs.</param>
        /// <returns>The MIME content type's path extension.</returns>
        public static string GetExtensionForContentType(string contentType, string def)
        {
            var helper = new PermissionHelper();

            if (string.IsNullOrEmpty(contentType))
            {
                return def;
            }
            string ext = "";
            if (!helper.GetIsRegistryAvailable())
            {
                if (MimeTypes.ContainsValue(contentType))
                {
                    foreach (KeyValuePair<string, string> pair in MimeTypes)
                        if (pair.Value == contentType)
                            return pair.Value;
                }
                return def;
            }

            if (helper.GetIsRegistryAvailable())
            {
                try
                {
                    RegistryKey reg = Registry.ClassesRoot;
                    reg = reg.OpenSubKey(@"MIME\Database\Content Type\" + contentType, false);
                    if (reg != null) ext = (string)reg.GetValue("Extension", def);
                }
                catch (Exception)
                {
                    ext = def;
                }
            }
            return ext;
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string url, Type type)
        {
            return CreateInstance(url, null, null, type);
        }
#endif


        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        public void Get(string url, string path)
        {
            Get(url, path, "GET");
        }

#if !NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="proxy"></param>
        /// <param name="credentials"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials)
        {
            Get(url, path, proxy, credentials, "GET");
        }
#endif
#if NET45 || NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource and saves it to the specified file. - Proxy aware
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="path">The location of the file where you want to save the document.</param>
/// <param name="proxy"></param>
/// <param name="credentials"></param>
	    public void Get(string url, string path, IWebProxy proxy, ICredentials credentials)
	    {
	        Get(url, path, proxy, credentials, "GET");
	    }
#endif

        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        public void Get(string url, string path, string method)
        {
            Uri uri = new Uri(url);
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
// TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if ((uri.Scheme == "https") ||
		        (uri.Scheme == "http"))
#endif
            {
                Get(uri, method, path, null, null, null);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }

#if !NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="path">The location of the file where you want to save the document.</param>
        /// <param name="credentials"></param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy"></param>
        public void Get(string url, string path, WebProxy proxy, NetworkCredential credentials, string method)
        {
            Uri uri = new Uri(url);
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                Get(uri, method, path, null, proxy, credentials);
            }
            else
            {
                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
            }
        }
#endif

#if NET45 || NETSTANDARD
/// <summary>
/// Gets an HTML document from an Internet resource and saves it to the specified file.  Understands Proxies
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="path">The location of the file where you want to save the document.</param>
/// <param name="credentials"></param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy"></param>
	    public void Get(string url, string path, IWebProxy proxy, ICredentials credentials, string method)
	    {
	        Uri uri = new Uri(url);
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
	        // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if ((uri.Scheme == "https") ||
	            (uri.Scheme == "http"))
#endif
	        {
	            Get(uri, method, path, null, proxy, credentials);
	        }
	        else
	        {
	            throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
	        }
	    }
#endif

        /// <summary>
        /// Gets the cache file path for a specified url.
        /// </summary>
        /// <param name="uri">The url fo which to retrieve the cache path. May not be null.</param>
        /// <returns>The cache file path.</returns>
        public string GetCachePath(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (!UsingCache)
            {
                throw new HtmlWebException("Cache is not enabled. Set UsingCache to true first.");
            }
            string cachePath;
            if (uri.AbsolutePath == "/")
            {
                cachePath = Path.Combine(_cachePath, ".htm");
            }
            else
            {
                cachePath = Path.Combine(_cachePath, (uri.Host + uri.AbsolutePath).Replace('/', '\\'));
            }
            return cachePath;
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url)
        {
            return Load(url, "GET");
        }

        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri)
        {
            return Load(uri, "GET");
        }

#if !NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="proxyHost">Host to use for Proxy</param>
        /// <param name="proxyPort">Port the Proxy is on</param>
        /// <param name="userId">User Id for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string proxyHost, int proxyPort, string userId, string password)
        {
            //Create my proxy
            WebProxy myProxy = new WebProxy(proxyHost, proxyPort);
            myProxy.BypassProxyOnLocal = true;

            //Create my credentials
            NetworkCredential myCreds = null;
            if ((userId != null) && (password != null))
            {
                myCreds = new NetworkCredential(userId, password);
                CredentialCache credCache = new CredentialCache();
                //Add the creds
                credCache.Add(myProxy.Address, "Basic", myCreds);
                credCache.Add(myProxy.Address, "Digest", myCreds);
            }

            return Load(url, "GET", myProxy, myCreds);
        }
#endif

#if !NETSTANDARD
        /// <summary>
        /// Gets an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <param name="proxyHost">Host to use for Proxy</param>
        /// <param name="proxyPort">Port the Proxy is on</param>
        /// <param name="userId">User Id for Authentication</param>
        /// <param name="password">Password for Authentication</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string proxyHost, int proxyPort, string userId, string password)
        {
            //Create my proxy
            WebProxy myProxy = new WebProxy(proxyHost, proxyPort);
            myProxy.BypassProxyOnLocal = true;

            //Create my credentials
            NetworkCredential myCreds = null;
            if ((userId != null) && (password != null))
            {
                myCreds = new NetworkCredential(userId, password);
                CredentialCache credCache = new CredentialCache();
                //Add the creds
                credCache.Add(myProxy.Address, "Basic", myCreds);
                credCache.Add(myProxy.Address, "Digest", myCreds);
            }

            return Load(uri, "GET", myProxy, myCreds);
        }
#endif

        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method)
        {
            Uri uri = new Uri(url);

            return Load(uri, method);
        }
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested URL, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string method)
        {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

            HtmlDocument doc;
#if !NETSTANDARD
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
#else
// TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
		    if ((uri.Scheme == "https") ||
		        (uri.Scheme == "http"))
#endif
            {
                doc = LoadUrl(uri, method, null, null);
            }
            else
            {
#if !NETSTANDARD
                if (uri.Scheme == Uri.UriSchemeFile)
#else
// TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
                if (uri.Scheme == "file")
#endif
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    if (OverrideEncoding != null)
                        doc.Load(uri.OriginalString, OverrideEncoding);
                    else
                        doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }

#if !NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(string url, string method, WebProxy proxy, NetworkCredential credentials)
        {
            Uri uri = new Uri(url);

            return Load(uri, method, proxy, credentials);
        }
#endif

#if !NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource.
        /// </summary>
        /// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
        /// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
        /// <param name="proxy">Proxy to use with this request</param>
        /// <param name="credentials">Credentials to use when authenticating</param>
        /// <returns>A new HTML document.</returns>
        public HtmlDocument Load(Uri uri, string method, WebProxy proxy, NetworkCredential credentials)
        {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

            HtmlDocument doc;
            if ((uri.Scheme == Uri.UriSchemeHttps) ||
                (uri.Scheme == Uri.UriSchemeHttp))
            {
                doc = LoadUrl(uri, method, proxy, credentials);
            }
            else
            {
                if (uri.Scheme == Uri.UriSchemeFile)
                {
                    doc = new HtmlDocument();
                    doc.OptionAutoCloseOnEnd = false;
                    doc.OptionAutoCloseOnEnd = true;
                    doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
                }
                else
                {
                    throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
                }
            }
            if (PreHandleDocument != null)
            {
                PreHandleDocument(doc);
            }
            return doc;
        }
#endif
#if NET45 || NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource.
/// </summary>
/// <param name="url">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy">Proxy to use with this request</param>
/// <param name="credentials">Credentials to use when authenticating</param>
/// <returns>A new HTML document.</returns>
	    public HtmlDocument Load(string url, string method, IWebProxy proxy, ICredentials credentials)
	    {
            Uri uri = new Uri(url);
            return Load(uri, method, proxy, credentials);
	    }
#endif

#if NET45 || NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource.
/// </summary>
/// <param name="uri">The requested Uri, such as new Uri("http://Myserver/Mypath/Myfile.asp").</param>
/// <param name="method">The HTTP method used to open the connection, such as GET, POST, PUT, or PROPFIND.</param>
/// <param name="proxy">Proxy to use with this request</param>
/// <param name="credentials">Credentials to use when authenticating</param>
/// <returns>A new HTML document.</returns>
	    public HtmlDocument Load(Uri uri, string method, IWebProxy proxy, ICredentials credentials)
	    {
            if (UsingCache)
            {
                _usingCacheAndLoad = true;
            }

	        HtmlDocument doc;
#if !NETSTANDARD
            if (uri.Scheme == Uri.UriSchemeFile)
#else
	        // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
            if (uri.Scheme == "file")
#endif
	        {
	            doc = LoadUrl(uri, method, proxy, credentials);
	        }
	        else
	        {
#if !NETSTANDARD
                if (uri.Scheme == Uri.UriSchemeFile)
#else
	            // TODO: Check if UriSchemeHttps is still internal in NETSTANDARD 2.0
                if (uri.Scheme == "file")
#endif
	            {
	                doc = new HtmlDocument();
	                doc.OptionAutoCloseOnEnd = false;
	                doc.OptionAutoCloseOnEnd = true;
	                doc.DetectEncodingAndLoad(uri.OriginalString, _autoDetectEncoding);
	            }
	            else
	            {
	                throw new HtmlWebException("Unsupported uri scheme: '" + uri.Scheme + "'.");
	            }
	        }
	        if (PreHandleDocument != null)
	        {
	            PreHandleDocument(doc);
	        }
	        return doc;
	    }
#endif
#if !NETSTANDARD
        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="writer">The XmlTextWriter to which you want to save to.</param>
        public void LoadHtmlAsXml(string htmlUrl, XmlTextWriter writer)
        {
            HtmlDocument doc = Load(htmlUrl);
            doc.Save(writer);
        }
#endif
#if NET45 || NETSTANDARD
/// <summary>
/// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter.
/// </summary>
/// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
/// <param name="writer">The XmlTextWriter to which you want to save to.</param>
	    public void LoadHtmlAsXml(string htmlUrl, XmlWriter writer)
	    {
	        HtmlDocument doc = Load(htmlUrl);
	        doc.Save(writer);
	    }
#endif



        #endregion

        #region Private Methods

        private static void FilePreparePath(string target)
        {
            if (File.Exists(target))
            {
                FileAttributes atts = File.GetAttributes(target);
                File.SetAttributes(target, atts & ~FileAttributes.ReadOnly);
            }
            else
            {
                string dir = Path.GetDirectoryName(target);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }


        private static DateTime RemoveMilliseconds(DateTime t)
        {
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        private static DateTime RemoveMilliseconds(DateTimeOffset? offset)
        {
            var t = offset ?? DateTimeOffset.Now;
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, 0);
        }

        // ReSharper disable UnusedMethodReturnValue.Local
        private static long SaveStream(Stream stream, string path, DateTime touchDate, int streamBufferSize)
            // ReSharper restore UnusedMethodReturnValue.Local
        {
            FilePreparePath(path);

            long len = 0;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        byte[] buffer;
                        do
                        {
                            buffer = br.ReadBytes(streamBufferSize);
                            len += buffer.Length;
                            if (buffer.Length > 0)
                            {
                                bw.Write(buffer);
                            }
                        } while (buffer.Length > 0);
                        bw.Flush();
                    }
                }
            }

            File.SetLastWriteTime(path, touchDate);
            return len;
        }

#if !NETSTANDARD
        private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
            ICredentials creds)
        {
            string cachePath = null;
            HttpWebRequest req;
            bool oldFile = false;

            req = WebRequest.Create(uri) as HttpWebRequest;
            req.Method = method;
            req.UserAgent = UserAgent;
            if (proxy != null)
            {
                if (creds != null)
                {
                    proxy.Credentials = creds;
                    req.Credentials = creds;
                }
                else
                {
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    req.Credentials = CredentialCache.DefaultCredentials;
                }
                req.Proxy = proxy;
            }

            _fromCache = false;
            _requestDuration = 0;
            int tc = Environment.TickCount;
            if (UsingCache)
            {
                cachePath = GetCachePath(req.RequestUri);
                if (File.Exists(cachePath))
                {
                    req.IfModifiedSince = File.GetLastAccessTime(cachePath);
                    oldFile = true;
                }
            }

            if (_cacheOnly)
            {
                if (!File.Exists(cachePath))
                {
                    throw new HtmlWebException("File was not found at cache path: '" + cachePath + "'");
                }

                if (path != null)
                {
                    IOLibrary.CopyAlways(cachePath, path);
                    // touch the file
                    if (cachePath != null) File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                }
                _fromCache = true;
                return HttpStatusCode.NotModified;
            }

            if (_useCookies)
            {
                req.CookieContainer = new CookieContainer();
            }

            if (PreRequest != null)
            {
                // allow our user to change the request at will
                if (!PreRequest(req))
                {
                    return HttpStatusCode.ResetContent;
                }

                // dump cookie
                //				if (_useCookies)
                //				{
                //					foreach(Cookie cookie in req.CookieContainer.GetCookies(req.RequestUri))
                //					{
                //						HtmlLibrary.Trace("Cookie " + cookie.Name + "=" + cookie.Value + " path=" + cookie.Path + " domain=" + cookie.Domain);
                //					}
                //				}
            }

            HttpWebResponse resp;

            try
            {
                resp = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException we)
            {
                _requestDuration = Environment.TickCount - tc;
                resp = (HttpWebResponse)we.Response;
                if (resp == null)
                {
                    if (oldFile)
                    {
                        if (path != null)
                        {
                            IOLibrary.CopyAlways(cachePath, path);
                            // touch the file
                            File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                        }
                        return HttpStatusCode.NotModified;
                    }
                    throw;
                }
            }
            catch (Exception)
            {
                _requestDuration = Environment.TickCount - tc;
                throw;
            }

            // allow our user to get some info from the response
            if (PostResponse != null)
            {
                PostResponse(req, resp);
            }

            _requestDuration = Environment.TickCount - tc;
            _responseUri = resp.ResponseUri;

            bool html = IsHtmlContent(resp.ContentType);
            bool isUnknown = string.IsNullOrEmpty(resp.ContentType);

            Encoding respenc = !string.IsNullOrEmpty(resp.ContentEncoding)
                ? Encoding.GetEncoding(resp.ContentEncoding)
                : null;
            if (OverrideEncoding != null)
                respenc = OverrideEncoding;

            if (resp.StatusCode == HttpStatusCode.NotModified)
            {
                if (UsingCache)
                {
                    _fromCache = true;
                    if (path != null)
                    {
                        IOLibrary.CopyAlways(cachePath, path);
                        // touch the file
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }
                    return resp.StatusCode;
                }
                // this should *never* happen...
                throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
            }
            Stream s = resp.GetResponseStream();
            if (s != null)
            {
                if (UsingCache)
                {
                    // NOTE: LastModified does not contain milliseconds, so we remove them to the file
                    SaveStream(s, cachePath, RemoveMilliseconds(resp.LastModified), _streamBufferSize);

                    // save headers
                    SaveCacheHeaders(req.RequestUri, resp);

                    if (path != null)
                    {
                        // copy and touch the file
                        IOLibrary.CopyAlways(cachePath, path);
                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
                    }

                    if (_usingCacheAndLoad)
                    {
                        doc.Load(cachePath);
                    }
                }
                else
                {
                    // try to work in-memory
                    if (doc != null && html)
                    {
                        if (respenc == null)
                        {
                            doc.Load(s, true);
                        }
                        else
                        {
                            doc.Load(s, respenc);
                        }
                    }

                    if (doc != null && isUnknown)
                    {
                        try
                        {
                            if (respenc == null)
                            {
                                doc.Load(s, true);
                            }
                            else
                            {
                                doc.Load(s, respenc);
                            }
                        }
                        catch
                        {
                            // That�s fine, the content type was unknown so probably not HTML
                            // Perhaps trying to figure if the content contains some HTML before would be a better idea.
                        }
                    }
                }
                resp.Close();
            }
            return resp.StatusCode;
        }
#else
	    private HttpStatusCode Get(Uri uri, string method, string path, HtmlDocument doc, IWebProxy proxy,
	        ICredentials creds)
	    {
	        string cachePath = null;
	        bool oldFile = false;
	        HttpStatusCode status;
	        using (var request = new HttpRequestMessage(new HttpMethod(method), uri))
	        using (var handler = new HttpClientHandler())
	        using (var client = new HttpClient(handler))
	        {
	            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

	            if (proxy != null)
	            {
	                if (creds != null)
	                {
	                    proxy.Credentials = creds;
	                    handler.Credentials = creds;
	                }
	                else
	                {
	                    proxy.Credentials = CredentialCache.DefaultCredentials;
	                    handler.Credentials = CredentialCache.DefaultCredentials;
	                }
	                handler.Proxy = proxy;
	                handler.UseProxy = true;
	            }

	            _fromCache = false;
	            _requestDuration = 0;
	            int tc = Environment.TickCount;
	            if (UsingCache)
	            {
	                cachePath = GetCachePath(request.RequestUri);
	                if (File.Exists(cachePath))
	                {
	                    client.DefaultRequestHeaders.IfModifiedSince = File.GetLastAccessTime(cachePath);
	                    oldFile = true;
	                }
	            }

	            if (_cacheOnly)
	            {
	                if (!File.Exists(cachePath))
	                {
	                    throw new HtmlWebException("File was not found at cache path: '" + cachePath + "'");
	                }

	                if (path != null)
	                {
	                    IOLibrary.CopyAlways(cachePath, path);
	                    // touch the file
	                    if (cachePath != null) File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
	                }
	                _fromCache = true;
	                return HttpStatusCode.NotModified;
	            }

	            if (_useCookies)
	            {
	                handler.CookieContainer = new CookieContainer();
	            }

	            if (PreRequest != null)
	            {
	                // allow our user to change the request at will
	                if (!PreRequest(handler, request))
	                {
	                    return HttpStatusCode.ResetContent;
	                }

	                // dump cookie
	                //				if (_useCookies)
	                //				{
	                //					foreach(Cookie cookie in req.CookieContainer.GetCookies(req.RequestUri))
	                //					{
	                //						HtmlLibrary.Trace("Cookie " + cookie.Name + "=" + cookie.Value + " path=" + cookie.Path + " domain=" + cookie.Domain);
	                //					}
	                //				}
	            }

	            HttpResponseMessage response;
	            try
	            {
	                response = client.SendAsync(request).Result;
	            }
	            catch (HttpRequestException)
	            {
	                _requestDuration = Environment.TickCount - tc;
	                if (oldFile)
	                {
	                    if (path != null)
	                    {
	                        IOLibrary.CopyAlways(cachePath, path);
	                        // touch the file
	                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
	                    }
	                    return HttpStatusCode.NotModified;
	                }
	                throw;
	            }
	            catch (Exception)
	            {
	                _requestDuration = Environment.TickCount - tc;
	                throw;
	            }

	            // allow our user to get some info from the response
	            if (PostResponse != null)
	            {
	                PostResponse(request, response);
	            }

	            _requestDuration = Environment.TickCount - tc;
	            _responseUri = response.RequestMessage.RequestUri;

                bool isUnknown = response.Content.Headers.ContentType == null;
                bool html = !isUnknown && IsHtmlContent(response.Content.Headers.ContentType.MediaType);

	            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
	            Encoding respenc = !string.IsNullOrEmpty(encoding)
	                ? Encoding.GetEncoding(encoding)
	                : null;

	            if (response.StatusCode == HttpStatusCode.NotModified)
	            {
	                if (UsingCache)
	                {
	                    _fromCache = true;
	                    if (path != null)
	                    {
	                        IOLibrary.CopyAlways(cachePath, path);
	                        // touch the file
	                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
	                    }
	                    return response.StatusCode;
	                }
	                // this should *never* happen...
	                throw new HtmlWebException("Server has send a NotModifed code, without cache enabled.");
	            }
	            Stream s = response.Content.ReadAsStreamAsync().Result;
	            if (s != null)
	            {
	                if (UsingCache)
	                {
	                    // NOTE: LastModified does not contain milliseconds, so we remove them to the file
	                    SaveStream(s, cachePath, RemoveMilliseconds(response.Content.Headers.LastModified), _streamBufferSize);

	                    // save headers
	                    SaveCacheHeaders(request.RequestUri, response);

	                    if (path != null)
	                    {
	                        // copy and touch the file
	                        IOLibrary.CopyAlways(cachePath, path);
	                        File.SetLastWriteTime(path, File.GetLastWriteTime(cachePath));
	                    }

                        if (_usingCacheAndLoad)
                        {
                            doc.Load(cachePath);
                        }
	                }
	                else
	                {
	                    // try to work in-memory
	                    if ((doc != null) && (html))
	                    {
	                        if (respenc != null)
	                        {
	                            doc.Load(s, respenc);
	                        }
	                        else
	                        {
	                            doc.Load(s, true);
	                        }
	                    }

                        else if (doc != null && isUnknown)
                        {
                            try
                            {
                                if (respenc == null)
                                {
                                    doc.Load(s, true);
                                }
                                else
                                {
                                    doc.Load(s, respenc);
                                }
                            }
                            catch
                            {
                                // That�s fine, the content type was unknown so probably not HTML
                                // Perhaps trying to figure if the content contains some HTML before would be a better idea.
                            }
                        }
                    }
	            }
	            status = response.StatusCode;
	        }

	        return status;
	    }
#endif

        private string GetCacheHeader(Uri requestUri, string name, string def)
        {
            // note: some headers are collection (ex: www-authenticate)
            // we don't handle that here
            XmlDocument doc = new XmlDocument();
#if NETSTANDARD
		    doc.Load(File.OpenRead(GetCacheHeadersPath(requestUri)));
#else
            doc.Load(GetCacheHeadersPath(requestUri));
#endif
            XmlNode node =
                doc.SelectSingleNode("//h[translate(@n, 'abcdefghijklmnopqrstuvwxyz','ABCDEFGHIJKLMNOPQRSTUVWXYZ')='" +
                                     name.ToUpper() + "']");
            if (node == null)
            {
                return def;
            }
            // attribute should exist
            return node.Attributes[name].Value;
        }

        private string GetCacheHeadersPath(Uri uri)
        {
            //return Path.Combine(GetCachePath(uri), ".h.xml");
            return GetCachePath(uri) + ".h.xml";
        }

#if !NETSTANDARD
        private bool IsCacheHtmlContent(string path)
        {
            string ct = GetContentTypeForExtension(Path.GetExtension(path), null);
            return IsHtmlContent(ct);
        }
#endif

        private bool IsHtmlContent(string contentType)
        {
            return contentType.ToLower().StartsWith("text/html");
        }

        private bool IsGZipEncoding(string contentEncoding)
        {
            return contentEncoding.ToLower().StartsWith("gzip");
        }

#if !NETSTANDARD
        private HtmlDocument LoadUrl(Uri uri, string method, WebProxy proxy, NetworkCredential creds)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.OptionAutoCloseOnEnd = false;
            doc.OptionFixNestedTags = true;
            _statusCode = Get(uri, method, null, doc, proxy, creds);
            if (_statusCode == HttpStatusCode.NotModified)
            {
                // read cached encoding
                doc.DetectEncodingAndLoad(GetCachePath(uri));
            }
            return doc;
        }
#endif

#if NET45 || NETSTANDARD
	    private HtmlDocument LoadUrl(Uri uri, string method, IWebProxy proxy, ICredentials creds)
	    {
	        HtmlDocument doc = new HtmlDocument();
	        doc.OptionAutoCloseOnEnd = false;
	        doc.OptionFixNestedTags = true;
	        _statusCode = Get(uri, method, null, doc, proxy, creds);
	        if (_statusCode == HttpStatusCode.NotModified)
	        {
	            // read cached encoding
	            doc.DetectEncodingAndLoad(GetCachePath(uri));
	        }
	        return doc;
	    }
#endif
#if !NETSTANDARD
        private void SaveCacheHeaders(Uri requestUri, HttpWebResponse resp)
        {
            // we cache the original headers aside the cached document.
            string file = GetCacheHeadersPath(requestUri);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<c></c>");
            XmlNode cache = doc.FirstChild;
            foreach (string header in resp.Headers)
            {
                XmlNode entry = doc.CreateElement("h");
                XmlAttribute att = doc.CreateAttribute("n");
                att.Value = header;
                entry.Attributes.Append(att);

                att = doc.CreateAttribute("v");
                att.Value = resp.Headers[header];
                entry.Attributes.Append(att);

                cache.AppendChild(entry);
            }
            doc.Save(file);
        }
#endif

#if NET45 || NETSTANDARD
        private void SaveCacheHeaders(Uri requestUri, HttpResponseMessage resp)
	    {
	        // we cache the original headers aside the cached document.
	        string file = GetCacheHeadersPath(requestUri);
	        XmlDocument doc = new XmlDocument();
	        doc.LoadXml("<c></c>");
	        XmlNode cache = doc.FirstChild;
	        foreach (var header in resp.Headers)
	        {
	            XmlNode entry = doc.CreateElement("h");
	            XmlAttribute att = doc.CreateAttribute("n");
	            att.Value = header.Key;
	            entry.Attributes.Append(att);

	            att = doc.CreateAttribute("v");
	            att.Value = string.Join(";", header.Value);
	            entry.Attributes.Append(att);

	            cache.AppendChild(entry);
	        }
	        doc.Save(File.OpenWrite(file));
	    }
#endif

#if NET45 || NETSTANDARD
/// <summary>
/// Begins the process of downloading an internet resource
/// </summary>
/// <param name="url">Url to the html document</param>
	    public Task<HtmlDocument> LoadFromWebAsync(string url)
	    {
	        return LoadFromWebAsync(new Uri(url), null, null);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, null, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding)
	    {
	        return LoadFromWebAsync(new Uri(url), encoding, null, CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, null, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password)
	    {
	        return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password), CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, string domain)
	    {
	        return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password, domain), CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName, string password, string domain, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), encoding, new NetworkCredential(userName, password, domain), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, string domain)
	    {
	        return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password, domain), CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, string domain, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password, domain), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password)
	    {
	        return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password), CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, new NetworkCredential(userName, password), cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, NetworkCredential credentials)
	    {
	        return LoadFromWebAsync(new Uri(url), null, credentials, CancellationToken.None);
	    }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public Task<HtmlDocument> LoadFromWebAsync(string url, NetworkCredential credentials, CancellationToken cancellationToken)
        {
            return LoadFromWebAsync(new Uri(url), null, credentials, cancellationToken);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public Task<HtmlDocument> LoadFromWebAsync(Uri uri, Encoding encoding, NetworkCredential credentials)
        {
            return LoadFromWebAsync(uri, encoding, credentials, CancellationToken.None);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<HtmlDocument> LoadFromWebAsync(Uri uri, Encoding encoding, NetworkCredential credentials, CancellationToken cancellationToken)
	    {
	        var clientHandler = new HttpClientHandler();
	        if (credentials == null)
	            clientHandler.UseDefaultCredentials = true;
	        else
	            clientHandler.Credentials = credentials;

	        var client = new HttpClient(clientHandler);

	        var e = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
	        if (e.StatusCode == HttpStatusCode.OK)
	        {
	            var html = string.Empty;
	            if (encoding != null)
	            {
	                using (var sr = new StreamReader(await e.Content.ReadAsStreamAsync().ConfigureAwait(false), encoding))
	                {
	                    html = sr.ReadToEnd();
	                }
	            }
	            else
	                html = await e.Content.ReadAsStringAsync().ConfigureAwait(false);
	            var doc = new HtmlDocument();
	            if (PreHandleDocument != null)
	                PreHandleDocument(doc);
	            doc.LoadHtml(html);
	            return doc;
	        }
	        throw new Exception("Error downloading html");
	    }
#endif

        #endregion
    }

#if !NETSTANDARD
    /// <summary>
    /// Wraps getting AppDomain permissions
    /// </summary>
    public class PermissionHelper : IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsRegistryAvailable()
        {
#if FX40
            var permissionSet = new PermissionSet(PermissionState.None);    
            var writePermission = new RegistryPermission( PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#else
            return SecurityManager.IsGranted(new RegistryPermission(PermissionState.Unrestricted));
#endif
        }
        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsDnsAvailable()
        {
#if FX40
            var permissionSet = new PermissionSet(PermissionState.None);    
            var writePermission = new DnsPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
#else
            return SecurityManager.IsGranted(new DnsPermission(PermissionState.Unrestricted));
#endif
        }
    }
#endif

    /// <summary>
    /// An interface for getting permissions of the running application
    /// </summary>
    public interface IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsRegistryAvailable();

        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsDnsAvailable();
    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlWeb.Xpath.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    public partial class HtmlWeb
    {
        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An <see cref="XsltArgumentList"/> containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="type">The requested type.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type)
        {
            return CreateInstance(htmlUrl, xsltUrl, xsltArgs, type, null);
        }

        /// <summary>
        /// Creates an instance of the given type from the specified Internet resource.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An <see cref="XsltArgumentList"/> containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="type">The requested type.</param>
        /// <param name="xmlPath">A file path where the temporary XML before transformation will be saved. Mostly used for debugging purposes.</param>
        /// <returns>An newly created instance.</returns>
        public object CreateInstance(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, Type type,
                                     string xmlPath)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            if (xsltUrl == null)
            {
                LoadHtmlAsXml(htmlUrl, writer);
            }
            else
            {
                if (xmlPath == null)
                {
                    LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer);
                }
                else
                {
                    LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, xmlPath);
                }
            }
            writer.Flush();
            StringReader sr = new StringReader(sw.ToString());
            XmlTextReader reader = new XmlTextReader(sr);
            XmlSerializer serializer = new XmlSerializer(type);
            object o;
            try
            {
                o = serializer.Deserialize(reader);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex + ", --- xml:" + sw);
            }
            return o;
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp".</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer)
        {
            LoadHtmlAsXml(htmlUrl, xsltUrl, xsltArgs, writer, null);
        }

        /// <summary>
        /// Loads an HTML document from an Internet resource and saves it to the specified XmlTextWriter, after an XSLT transformation.
        /// </summary>
        /// <param name="htmlUrl">The requested URL, such as "http://Myserver/Mypath/Myfile.asp". May not be null.</param>
        /// <param name="xsltUrl">The URL that specifies the XSLT stylesheet to load.</param>
        /// <param name="xsltArgs">An XsltArgumentList containing the namespace-qualified arguments used as input to the transform.</param>
        /// <param name="writer">The XmlTextWriter to which you want to save.</param>
        /// <param name="xmlPath">A file path where the temporary XML before transformation will be saved. Mostly used for debugging purposes.</param>
        public void LoadHtmlAsXml(string htmlUrl, string xsltUrl, XsltArgumentList xsltArgs, XmlTextWriter writer,
                                  string xmlPath)
        {
            if (htmlUrl == null)
            {
                throw new ArgumentNullException("htmlUrl");
            }

            HtmlDocument doc = Load(htmlUrl);

            if (xmlPath != null)
            {
                XmlTextWriter w = new XmlTextWriter(xmlPath, doc.Encoding);
                doc.Save(w);
                w.Close();
            }
            if (xsltArgs == null)
            {
                xsltArgs = new XsltArgumentList();
            }

            // add some useful variables to the xslt doc
            xsltArgs.AddParam("url", "", htmlUrl);
            xsltArgs.AddParam("requestDuration", "", RequestDuration);
            xsltArgs.AddParam("fromCache", "", FromCache);

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltUrl);
            xslt.Transform(doc, xsltArgs, writer);
        }

    }
}
//////////////////////////////////////////////////////////////////////////
// File: HtmlWebException.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents an exception thrown by the HtmlWeb utility class.
    /// </summary>
    public class HtmlWebException : Exception
    {
#region Constructors

        /// <summary>
        /// Creates an instance of the HtmlWebException.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public HtmlWebException(string message)
            : base(message)
        {
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: IOLibrary.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal struct IOLibrary
    {
#region Internal Methods

        internal static void CopyAlways(string source, string target)
        {
            if (!File.Exists(source))
                return;
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            MakeWritable(target);
            File.Copy(source, target, true);
        }
#if !PocketPC && !WINDOWS_PHONE
        internal static void MakeWritable(string path)
        {
            if (!File.Exists(path))
                return;
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly);
        }
#else
		internal static void MakeWritable(string path)
        {
        }
#endif
#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocument.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a document with mixed code and text. ASP, ASPX, JSP, are good example of such documents.
    /// </summary>
    public class MixedCodeDocument
    {
#region Fields

        private int _c;
        internal MixedCodeDocumentFragmentList _codefragments;
        private MixedCodeDocumentFragment _currentfragment;
        internal MixedCodeDocumentFragmentList _fragments;
        private int _index;
        private int _line;
        private int _lineposition;
        private ParseState _state;
        private Encoding _streamencoding;
        internal string _text;
        internal MixedCodeDocumentFragmentList _textfragments;

        /// <summary>
        /// Gets or sets the token representing code end.
        /// </summary>
        public string TokenCodeEnd = "%>";

        /// <summary>
        /// Gets or sets the token representing code start.
        /// </summary>
        public string TokenCodeStart = "<%";

        /// <summary>
        /// Gets or sets the token representing code directive.
        /// </summary>
        public string TokenDirective = "@";

        /// <summary>
        /// Gets or sets the token representing response write directive.
        /// </summary>
        public string TokenResponseWrite = "Response.Write ";


        private string TokenTextBlock = "TextBlock({0})";

#endregion

#region Constructors

        /// <summary>
        /// Creates a mixed code document instance.
        /// </summary>
        public MixedCodeDocument()
        {
            _codefragments = new MixedCodeDocumentFragmentList(this);
            _textfragments = new MixedCodeDocumentFragmentList(this);
            _fragments = new MixedCodeDocumentFragmentList(this);
        }

#endregion

#region Properties

        /// <summary>
        /// Gets the code represented by the mixed code document seen as a template.
        /// </summary>
        public string Code
        {
            get
            {
                string s = "";
                int i = 0;
                foreach (MixedCodeDocumentFragment frag in _fragments)
                {
                    switch (frag._type)
                    {
                        case MixedCodeDocumentFragmentType.Text:
                            s += TokenResponseWrite + string.Format(TokenTextBlock, i) + "\n";
                            i++;
                            break;

                        case MixedCodeDocumentFragmentType.Code:
                            s += ((MixedCodeDocumentCodeFragment) frag).Code + "\n";
                            break;
                    }
                }
                return s;
            }
        }

        /// <summary>
        /// Gets the list of code fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList CodeFragments
        {
            get { return _codefragments; }
        }

        /// <summary>
        /// Gets the list of all fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList Fragments
        {
            get { return _fragments; }
        }

        /// <summary>
        /// Gets the encoding of the stream used to read the document.
        /// </summary>
        public Encoding StreamEncoding
        {
            get { return _streamencoding; }
        }

        /// <summary>
        /// Gets the list of text fragments in the document.
        /// </summary>
        public MixedCodeDocumentFragmentList TextFragments
        {
            get { return _textfragments; }
        }

#endregion

#region Public Methods

        /// <summary>
        /// Create a code fragment instances.
        /// </summary>
        /// <returns>The newly created code fragment instance.</returns>
        public MixedCodeDocumentCodeFragment CreateCodeFragment()
        {
            return (MixedCodeDocumentCodeFragment) CreateFragment(MixedCodeDocumentFragmentType.Code);
        }

        /// <summary>
        /// Create a text fragment instances.
        /// </summary>
        /// <returns>The newly created text fragment instance.</returns>
        public MixedCodeDocumentTextFragment CreateTextFragment()
        {
            return (MixedCodeDocumentTextFragment) CreateFragment(MixedCodeDocumentFragmentType.Text);
        }

        /// <summary>
        /// Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        public void Load(Stream stream)
        {
            Load(new StreamReader(stream));
        }

        /// <summary>
        /// Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(Stream stream, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        /// Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(Stream stream, Encoding encoding)
        {
            Load(new StreamReader(stream, encoding));
        }

        /// <summary>
        /// Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks));
        }

        /// <summary>
        /// Loads a mixed code document from a stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
            Load(new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, buffersize));
        }

        /// <summary>
        /// Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        public void Load(string path)
        {
#if NETSTANDARD
            Load(new StreamReader(File.OpenRead(path)));
#else
            Load(new StreamReader(path));
#endif
        }

        /// <summary>
        /// Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, bool detectEncodingFromByteOrderMarks)
        {
#if NETSTANDARD
            Load(new StreamReader(File.OpenRead(path), detectEncodingFromByteOrderMarks));
#else
            Load(new StreamReader(path, detectEncodingFromByteOrderMarks));
#endif
        }

        /// <summary>
        /// Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Load(string path, Encoding encoding)
        {
#if NETSTANDARD
            Load(new StreamReader(File.OpenRead(path), encoding));
#else
            Load(new StreamReader(path, encoding));
#endif
        }

        /// <summary>
        /// Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
        {
#if NETSTANDARD
            Load(new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks));
#else
            Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks));
#endif
        }

        /// <summary>
        /// Loads a mixed code document from a file.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file.</param>
        /// <param name="buffersize">The minimum buffer size.</param>
        public void Load(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int buffersize)
        {
#if NETSTANDARD
            Load(new StreamReader(File.OpenRead(path), encoding, detectEncodingFromByteOrderMarks, buffersize));
#else
            Load(new StreamReader(path, encoding, detectEncodingFromByteOrderMarks, buffersize));
#endif
        }

        /// <summary>
        /// Loads the mixed code document from the specified TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the HTML data into the document.</param>
        public void Load(TextReader reader)
        {
            _codefragments.Clear();
            _textfragments.Clear();

            // all pseudo constructors get down to this one
            using (StreamReader sr = reader as StreamReader)
            {
                if (sr != null)
                {
                    _streamencoding = sr.CurrentEncoding;
                }

                _text = reader.ReadToEnd();
            }
                
            Parse();
        }

        /// <summary>
        /// Loads a mixed document from a text
        /// </summary>
        /// <param name="html">The text to load.</param>
        public void LoadHtml(string html)
        {
            Load(new StringReader(html));
        }

        /// <summary>
        /// Saves the mixed document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        public void Save(Stream outStream)
        {
            StreamWriter sw = new StreamWriter(outStream, GetOutEncoding());
            Save(sw);
        }

        /// <summary>
        /// Saves the mixed document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Save(Stream outStream, Encoding encoding)
        {
            StreamWriter sw = new StreamWriter(outStream, encoding);
            Save(sw);
        }

        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        public void Save(string filename)
        {
#if NETSTANDARD
            StreamWriter sw = new StreamWriter(File.OpenWrite(filename), GetOutEncoding());
#else
            StreamWriter sw = new StreamWriter(filename, false, GetOutEncoding());
#endif
            Save(sw);
        }

        /// <summary>
        /// Saves the mixed document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public void Save(string filename, Encoding encoding)
        {
#if NETSTANDARD
            StreamWriter sw = new StreamWriter(File.OpenWrite(filename), encoding);
#else
            StreamWriter sw = new StreamWriter(filename, false, encoding);
#endif
            Save(sw);
        }

        /// <summary>
        /// Saves the mixed document to the specified StreamWriter.
        /// </summary>
        /// <param name="writer">The StreamWriter to which you want to save.</param>
        public void Save(StreamWriter writer)
        {
            Save((TextWriter) writer);
        }

        /// <summary>
        /// Saves the mixed document to the specified TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        public void Save(TextWriter writer)
        {
            writer.Flush();
        }

#endregion

#region Internal Methods

        internal MixedCodeDocumentFragment CreateFragment(MixedCodeDocumentFragmentType type)
        {
            switch (type)
            {
                case MixedCodeDocumentFragmentType.Text:
                    return new MixedCodeDocumentTextFragment(this);

                case MixedCodeDocumentFragmentType.Code:
                    return new MixedCodeDocumentCodeFragment(this);

                default:
                    throw new NotSupportedException();
            }
        }

        internal Encoding GetOutEncoding()
        {
            if (_streamencoding != null)
                return _streamencoding;
            return Encoding.UTF8;
        }

#endregion

#region Private Methods

        private void IncrementPosition()
        {
            _index++;
            if (_c == 10)
            {
                _lineposition = 1;
                _line++;
            }
            else
                _lineposition++;
        }

        private void Parse()
        {
            _state = ParseState.Text;
            _index = 0;
            _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);

            while (_index < _text.Length)
            {
                _c = _text[_index];
                IncrementPosition();

                switch (_state)
                {
                    case ParseState.Text:
                        if (_index + TokenCodeStart.Length < _text.Length)
                        {
                            if (_text.Substring(_index - 1, TokenCodeStart.Length) == TokenCodeStart)
                            {
                                _state = ParseState.Code;
                                _currentfragment.Length = _index - 1 - _currentfragment.Index;
                                _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Code);
                                SetPosition();
                                continue;
                            }
                        }
                        break;

                    case ParseState.Code:
                        if (_index + TokenCodeEnd.Length < _text.Length)
                        {
                            if (_text.Substring(_index - 1, TokenCodeEnd.Length) == TokenCodeEnd)
                            {
                                _state = ParseState.Text;
                                _currentfragment.Length = _index + TokenCodeEnd.Length - _currentfragment.Index;
                                _index += TokenCodeEnd.Length;
                                _lineposition += TokenCodeEnd.Length;
                                _currentfragment = CreateFragment(MixedCodeDocumentFragmentType.Text);
                                SetPosition();
                                continue;
                            }
                        }
                        break;
                }
            }

            _currentfragment.Length = _index - _currentfragment.Index;
        }

        private void SetPosition()
        {
            _currentfragment.Line = _line;
            _currentfragment._lineposition = _lineposition;
            _currentfragment.Index = _index - 1;
            _currentfragment.Length = 0;
        }

#endregion

#region Nested type: ParseState

        private enum ParseState
        {
            Text,
            Code
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocumentCodeFragment.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a fragment of code in a mixed code document.
    /// </summary>
    public class MixedCodeDocumentCodeFragment : MixedCodeDocumentFragment
    {
#region Fields

        private string _code;

#endregion

#region Constructors

        internal MixedCodeDocumentCodeFragment(MixedCodeDocument doc)
            :
                base(doc, MixedCodeDocumentFragmentType.Code)
        {
        }

#endregion

#region Properties

        /// <summary>
        /// Gets the fragment code text.
        /// </summary>
        public string Code
        {
            get
            {
                if (_code == null)
                {
                    _code = FragmentText.Substring(Doc.TokenCodeStart.Length,
                                                   FragmentText.Length - Doc.TokenCodeEnd.Length -
                                                   Doc.TokenCodeStart.Length - 1).Trim();
                    if (_code.StartsWith("="))
                    {
                        _code = Doc.TokenResponseWrite + _code.Substring(1, _code.Length - 1);
                    }
                }
                return _code;
            }
            set { _code = value; }
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocumentFragment.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a base class for fragments in a mixed code document.
    /// </summary>
    public abstract class MixedCodeDocumentFragment
    {
#region Fields

        internal MixedCodeDocument Doc;
        private string _fragmentText;
        internal int Index;
        internal int Length;
        private int _line;
        internal int _lineposition;
        internal MixedCodeDocumentFragmentType _type;

#endregion

#region Constructors

        internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
        {
            Doc = doc;
            _type = type;
            switch (type)
            {
                case MixedCodeDocumentFragmentType.Text:
                    Doc._textfragments.Append(this);
                    break;

                case MixedCodeDocumentFragmentType.Code:
                    Doc._codefragments.Append(this);
                    break;
            }
            Doc._fragments.Append(this);
        }

#endregion

#region Properties

        /// <summary>
        /// Gets the fragement text.
        /// </summary>
        public string FragmentText
        {
            get
            {
                if (_fragmentText == null)
                {
                    _fragmentText = Doc._text.Substring(Index, Length);
                }
                return FragmentText;
            }
            internal set { _fragmentText = value; }
        }

        /// <summary>
        /// Gets the type of fragment.
        /// </summary>
        public MixedCodeDocumentFragmentType FragmentType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the line number of the fragment.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the line position (column) of the fragment.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the fragment position in the document's stream.
        /// </summary>
        public int StreamPosition
        {
            get { return Index; }
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocumentFragmentList.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a list of mixed code fragments.
    /// </summary>
    public class MixedCodeDocumentFragmentList : IEnumerable
    {
#region Fields

        private MixedCodeDocument _doc;
        private IList<MixedCodeDocumentFragment> _items = new List<MixedCodeDocumentFragment>();

#endregion

#region Constructors

        internal MixedCodeDocumentFragmentList(MixedCodeDocument doc)
        {
            _doc = doc;
        }

#endregion

#region Properties

        ///<summary>
        /// Gets the Document
        ///</summary>
        public MixedCodeDocument Doc
        {
            get { return _doc; }
        }

        /// <summary>
        /// Gets the number of fragments contained in the list.
        /// </summary>
        public int Count
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// Gets a fragment from the list using its index.
        /// </summary>
        public MixedCodeDocumentFragment this[int index]
        {
            get { return _items[index] as MixedCodeDocumentFragment; }
        }

#endregion

#region IEnumerable Members

        /// <summary>
        /// Gets an enumerator that can iterate through the fragment list.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#endregion

#region Public Methods

        /// <summary>
        /// Appends a fragment to the list of fragments.
        /// </summary>
        /// <param name="newFragment">The fragment to append. May not be null.</param>
        public void Append(MixedCodeDocumentFragment newFragment)
        {
            if (newFragment == null)
            {
                throw new ArgumentNullException("newFragment");
            }
            _items.Add(newFragment);
        }

        /// <summary>
        /// Gets an enumerator that can iterate through the fragment list.
        /// </summary>
        public MixedCodeDocumentFragmentEnumerator GetEnumerator()
        {
            return new MixedCodeDocumentFragmentEnumerator(_items);
        }

        /// <summary>
        /// Prepends a fragment to the list of fragments.
        /// </summary>
        /// <param name="newFragment">The fragment to append. May not be null.</param>
        public void Prepend(MixedCodeDocumentFragment newFragment)
        {
            if (newFragment == null)
            {
                throw new ArgumentNullException("newFragment");
            }
            _items.Insert(0, newFragment);
        }

        /// <summary>
        /// Remove a fragment from the list of fragments. If this fragment was not in the list, an exception will be raised.
        /// </summary>
        /// <param name="fragment">The fragment to remove. May not be null.</param>
        public void Remove(MixedCodeDocumentFragment fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException("fragment");
            }
            int index = GetFragmentIndex(fragment);
            if (index == -1)
            {
                throw new IndexOutOfRangeException();
            }
            RemoveAt(index);
        }

        /// <summary>
        /// Remove all fragments from the list.
        /// </summary>
        public void RemoveAll()
        {
            _items.Clear();
        }

        /// <summary>
        /// Remove a fragment from the list of fragments, using its index in the list.
        /// </summary>
        /// <param name="index">The index of the fragment to remove.</param>
        public void RemoveAt(int index)
        {
            //MixedCodeDocumentFragment frag = (MixedCodeDocumentFragment) _items[index];
            _items.RemoveAt(index);
        }

#endregion

#region Internal Methods

        internal void Clear()
        {
            _items.Clear();
        }

        internal int GetFragmentIndex(MixedCodeDocumentFragment fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException("fragment");
            }
            for (int i = 0; i < _items.Count; i++)
            {
                if ((_items[i]) == fragment)
                {
                    return i;
                }
            }
            return -1;
        }

#endregion

#region Nested type: MixedCodeDocumentFragmentEnumerator

        /// <summary>
        /// Represents a fragment enumerator.
        /// </summary>
        public class MixedCodeDocumentFragmentEnumerator : IEnumerator
        {
#region Fields

            private int _index;
            private IList<MixedCodeDocumentFragment> _items;

#endregion

#region Constructors

            internal MixedCodeDocumentFragmentEnumerator(IList<MixedCodeDocumentFragment> items)
            {
                _items = items;
                _index = -1;
            }

#endregion

#region Properties

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            public MixedCodeDocumentFragment Current
            {
                get { return (MixedCodeDocumentFragment) (_items[_index]); }
            }

#endregion

#region IEnumerator Members

            /// <summary>
            /// Gets the current element in the collection.
            /// </summary>
            object IEnumerator.Current
            {
                get { return (Current); }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                _index++;
                return (_index < _items.Count);
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = -1;
            }

#endregion
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocumentFragmentType.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents the type of fragment in a mixed code document.
    /// </summary>
    public enum MixedCodeDocumentFragmentType
    {
        /// <summary>
        /// The fragment contains code.
        /// </summary>
        Code,

        /// <summary>
        /// The fragment contains text.
        /// </summary>
        Text,
    }
}
//////////////////////////////////////////////////////////////////////////
// File: MixedCodeDocumentTextFragment.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a fragment of text in a mixed code document.
    /// </summary>
    public class MixedCodeDocumentTextFragment : MixedCodeDocumentFragment
    {
#region Constructors

        internal MixedCodeDocumentTextFragment(MixedCodeDocument doc)
            :
                base(doc, MixedCodeDocumentFragmentType.Text)
        {
        }

#endregion

#region Properties

        /// <summary>
        /// Gets the fragment text.
        /// </summary>
        public string Text
        {
            get { return FragmentText; }
            set { FragmentText = value; }
        }

#endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: NameValuePair.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class NameValuePair
    {
        #region Fields

        internal readonly string Name;
        internal string Value;

        #endregion

        #region Constructors

        internal NameValuePair()
        {
        }

        internal NameValuePair(string name)
            :
                this()
        {
            Name = name;
        }

        internal NameValuePair(string name, string value)
            :
                this(name)
        {
            Value = value;
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: NameValuePairList.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal class NameValuePairList
    {
        #region Fields

        internal readonly string Text;
        private List<KeyValuePair<string, string>> _allPairs;
        private Dictionary<string,List<KeyValuePair<string,string>>> _pairsWithName;

        #endregion

        #region Constructors

        internal NameValuePairList() :
            this(null)
        {
        }

        internal NameValuePairList(string text)
        {
            Text = text;
            _allPairs = new List<KeyValuePair<string, string>>();
            _pairsWithName = new Dictionary<string, List<KeyValuePair<string, string>>>();

            Parse(text);
        }

        #endregion

        #region Internal Methods

        internal static string GetNameValuePairsValue(string text, string name)
        {
            NameValuePairList l = new NameValuePairList(text);
            return l.GetNameValuePairValue(name);
        }

        internal List<KeyValuePair<string,string>> GetNameValuePairs(string name)
        {
            if (name == null)
                return _allPairs;
            return _pairsWithName.ContainsKey(name) ? _pairsWithName[name] : new List<KeyValuePair<string,string>>();
        }

        internal string GetNameValuePairValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            List<KeyValuePair<string,string>> al = GetNameValuePairs(name);
            if (al.Count==0)
                return string.Empty;

            // return first item
             return al[0].Value.Trim();
        }

        #endregion

        #region Private Methods

        private void Parse(string text)
        {
            _allPairs.Clear();
            _pairsWithName.Clear();
            if (text == null)
                return;

            string[] p = text.Split(';');
            foreach (string pv in p)
            {
                if (pv.Length == 0)
                    continue;
                string[] onep = pv.Split(new[] {'='}, 2);
                if (onep.Length==0)
                    continue;
                KeyValuePair<string, string> nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLower(),
                                                                                    onep.Length < 2 ? "" : onep[1]);

                _allPairs.Add(nvp);

                // index by name
                List<KeyValuePair<string, string>> al;
                if (!_pairsWithName.ContainsKey(nvp.Key))
                {
                    al = new List<KeyValuePair<string, string>>();
                    _pairsWithName[nvp.Key] = al;
                }
                else
                    al = _pairsWithName[nvp.Key];

                al.Add(nvp);
            }
        }

        #endregion
    }
}
//////////////////////////////////////////////////////////////////////////
// File: Trace.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal partial class Trace
    {
        internal static Trace _current;
        internal static Trace Current
    {
        get
        {
            if(_current == null)
                _current = new Trace();
            return _current;
        }
    }
        partial void WriteLineIntern(string message,string category);
        public static void WriteLine(string message,string category)
        {
            Current.WriteLineIntern(message,category);
        }
    }
}

//////////////////////////////////////////////////////////////////////////
// File: Trace.FullFramework.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
   partial class Trace
    {
       partial void WriteLineIntern(string message,string category)
       {
           System.Diagnostics.Debug.WriteLine(message,category);
       }
    }
}

//////////////////////////////////////////////////////////////////////////
// File: Utilities.cs
//////////////////////////////////////////////////////////////////////////
namespace HtmlAgilityPack
{
    internal static class Utilities
    {
        public static TValue GetDictionaryValueOrNull<TKey,TValue>(Dictionary<TKey,TValue> dict, TKey key) where TKey: class
        {
            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }
    }
}

namespace HtmlAgilityPack
{
    public static class Extensions
    {
        // https://stackoverflow.com/questions/29995333/convert-render-html-to-text-with-correct-line-breaks/30086182#30086182

        public static string FormatLineBreaks(string html)
        {
            //first - remove all the existing '\n' from HTML
            //they mean nothing in HTML, but break our logic
            html = html.Replace("\t", "").Replace("\r", "").Replace("\n", " ");

            //now create an Html Agile Doc object
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            //remove comments, head, style and script tags
            foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//comment() | //script | //style | //head"))
            {
                node.ParentNode.RemoveChild(node);
            }

            //now remove all "meaningless" inline elements like "span"
            foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//span | //label")) //add "b", "i" if required
            {
                node.ParentNode.ReplaceChild(HtmlNode.CreateNode(node.InnerHtml), node);
            }

            //block-elements - convert to line-breaks
            foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//p | //div")) //you could add more tags here
            {
                //we add a "\n" ONLY if the node contains some plain text as "direct" child
                //meaning - text is not nested inside children, but only one-level deep

                //use XPath to find direct "text" in element
                var txtNode = node.SelectSingleNode("text()");

                //no "direct" text - NOT ADDDING the \n !!!!
                if (txtNode == null || txtNode.InnerHtml.Trim() == "") continue;

                //"surround" the node with line breaks
                node.ParentNode.InsertBefore(doc.CreateTextNode("\r\n"), node);
                node.ParentNode.InsertAfter(doc.CreateTextNode("\r\n"), node);
            }

            //todo: might need to replace multiple "\n\n" into one here, I'm still testing...

            //now BR tags - simply replace with "\n" and forget
            foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//br"))
                node.ParentNode.ReplaceChild(doc.CreateTextNode("\r\n"), node);

            //finally - return the text which will have our inserted line-breaks in it
            return doc.DocumentNode.InnerText.Trim();

            //todo - you should probably add "&code;" processing, to decode all the &nbsp; and such
        }

        //here's the extension method I use
        private static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string selector)
        {
            HtmlNodeCollection result = node.SelectNodes(selector);
            return result != null ? result : new HtmlNodeCollection(node);
        }
    }
}
#endif