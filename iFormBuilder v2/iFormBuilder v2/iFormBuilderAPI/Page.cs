using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
//using System.Threading.Tasks;

namespace iFormBuilderAPI
{
    public class Page : IPage
    {
        public Page()
        {
            this.Subforms = new List<Page>();
        }
        /// <summary>
        /// Gets or sets the _ s_ ICO n_ LINK.
        /// </summary>
        /// <value>
        /// The _ s_ ICO n_ LINK.
        /// </value>
        public string _S_ICON_LINK { get; set; }
        /// <summary>
        /// Gets or sets the CREATE d_ BY.
        /// </summary>
        /// <value>
        /// The CREATE d_ BY.
        /// </value>
        public string CREATED_BY { get; set; }
        /// <summary>
        /// Gets or sets the CREATE d_ DATE.
        /// </summary>
        /// <value>
        /// The CREATE d_ DATE.
        /// </value>
        public string CREATED_DATE { get; set; }
        /// <summary>
        /// Gets or sets the DESCRIPTION.
        /// </summary>
        /// <value>
        /// The DESCRIPTION.
        /// </value>
        public string DESCRIPTION { get; set; }
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public int ID { get; set; }
        /// <summary>
        /// Gets or sets the LABEL.
        /// </summary>
        /// <value>
        /// The LABEL.
        /// </value>
        public string LABEL { get; set; }
        /// <summary>
        /// Gets or sets the MODIFIE d_ BY.
        /// </summary>
        /// <value>
        /// The MODIFIE d_ BY.
        /// </value>
        public string MODIFIED_BY { get; set; }
        /// <summary>
        /// Gets or sets the MODIFIE d_ DATE.
        /// </summary>
        /// <value>
        /// The MODIFIE d_ DATE.
        /// </value>
        public string MODIFIED_DATE { get; set; }
        /// <summary>
        /// Gets or sets the NAME.
        /// </summary>
        /// <value>
        /// The NAME.
        /// </value>
        public string NAME { get; set; }
        /// <summary>
        /// Gets or sets the PAG e_ TYPE.
        /// </summary>
        /// <value>
        /// The PAG e_ TYPE.
        /// </value>
        public string PAGE_TYPE { get; set; }
        /// <summary>
        /// Gets or sets the STATUS.
        /// </summary>
        /// <value>
        /// The STATUS.
        /// </value>
        public string STATUS { get; set; }
        /// <summary>
        /// Gets or sets the VERSION.
        /// </summary>
        /// <value>
        /// The VERSION.
        /// </value>
        public string VERSION { get; set; }
        /// <summary>
        /// Gets or sets the elements.
        /// </summary>
        /// <value>
        /// The elements.
        /// </value>
        public List<Element> Elements { get; set; }

        public List<Page> Subforms { get; set; }

        public bool HasSubForms { get { return this.Subforms.Count != 0; } }

        public List<Element> SubformElements
        {
            get
            {
                List<Element> _subformelements = new List<Element>();
                foreach (Element ele in this.Elements)
                {
                    if (ele.DATA_TYPE == 18)
                        _subformelements.Add(ele);
                }
                return _subformelements;
            }
        }

        public Page GetSubformPage(Element ele)
        {
            if (this.HasSubForms)
            {
                foreach (Page p in this.Subforms)
                {
                    if (ele.DATA_SIZE == p.ID)
                        return p;
                }
            }
            else
                return null;

            return this;
        }

        public Element LocationElement
        {
            get
            {
                foreach (Element ele in this.Elements)
                {
                    if (ele.WidgetType == Element.Widget.Location)
                        return ele;
                }
                return null;
            }
        }

        public bool HasLocationWidget
        {
            get
            {
                return LocationElement != null;
            }

        }
    }
}
