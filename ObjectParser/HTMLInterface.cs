using System;
using System.Collections.Generic;
using System.Text;
using Microsoft;
using mshtml;

namespace ObjectParser
{
    class HTMLObject : ObjectPrototipe
    {
        IHTMLElement HtmlElement= null;
        public override List<ObjectAttribute> GetObjectAttributes()
        {
            return null;
        }

        public override string GetObjectName()
        {
            return HtmlElement.tagName;
        }

        public override ObjectAttribute GetAttributeByName(string AttributeName)
        {
            ObjectAttributeString lAttribute = new ObjectAttributeString();
            lAttribute.AttributeName= AttributeName;

            switch(AttributeName)
            {
                case "innerText":
                    if(HtmlElement.innerText==null)
                        lAttribute.Value="";
                    else
                        lAttribute.Value=HtmlElement.innerText;
                    return lAttribute;
                case "innerHTML":
                    if (HtmlElement.innerHTML == null)
                        lAttribute.Value="";
                    else
                        lAttribute.Value=HtmlElement.innerHTML;
                    return lAttribute;
                case "outerHTML":
                    if (HtmlElement.outerHTML == null)
                        lAttribute.Value="";
                    else
                        lAttribute.Value=HtmlElement.outerHTML;
                    return lAttribute;
                case "style":
                    if (HtmlElement.style == null)
                        lAttribute.Value="";
                    else
                        lAttribute.Value=HtmlElement.style.cssText;
                    return lAttribute;
            }

            lAttribute.Value=HtmlElement.getAttribute(AttributeName, 0).ToString();

            return lAttribute;
        }

        public override void SetAttribute(ObjectAttribute lObjectAttribute)
        {
            ObjectAttributeString StringAttribute= (ObjectAttributeString)lObjectAttribute;
            if (HtmlElement.getAttribute(lObjectAttribute.AttributeName, 0)!=null)
            {
                HtmlElement.setAttribute(lObjectAttribute.AttributeName, StringAttribute.Value, 0);
            }
        }

        public override void AddAttribute(ObjectAttribute lObjectAttribute)
        {
            return;
        }

        public IHTMLElement GetHtmlElement()
        {
            return HtmlElement;
        }

        public HTMLObject(IHTMLElement lHtmlElement)
        {
            HtmlElement = lHtmlElement;
        }
    }

    class HTMLInterface: ObjectInterfacePrototipe
    {
        int CurrentObjectId=0;
        IHTMLElementCollection HtmlDocument = null;
        List<HTMLObject> HtmlElements= new List<HTMLObject>();
        IHTMLDocument2 doc = null;

        public override ObjectPrototipe GetFirstObject()
        {
            CurrentObjectId = 0;
            GetAllObjects();

            if (HtmlElements.Count == 0)
                return null;

            return HtmlElements[0];
        }

        public override ObjectPrototipe GetNextObject()
        {
            CurrentObjectId++;
            if (CurrentObjectId >= HtmlElements.Count)
                return null;
            return HtmlElements[CurrentObjectId];
        }

        public override ObjectPrototipe GetCurrentObject()
        {
            if (CurrentObjectId >= HtmlElements.Count)
                return null;
            return HtmlElements[CurrentObjectId];
        }

        public override ObjectPrototipe GetPreviousObject()
        {
            if (CurrentObjectId-1 < 0)
                return null;
            return HtmlElements[CurrentObjectId-1];
        }

        public override ObjectPrototipe GetOffsetObject( int offset )
        {
            if (CurrentObjectId + offset >= HtmlElements.Count || CurrentObjectId+offset<0)
                return null;

            CurrentObjectId += offset;

            return HtmlElements[CurrentObjectId];
        }

        public override int GetObjectCount()
        {
            return HtmlElements.Count;
        }

        public override bool MoveByOffset(int offset)
        {
            if (CurrentObjectId + offset >= HtmlElements.Count || CurrentObjectId + offset<0)
                return true;

            CurrentObjectId += offset;
            return false;
        }

        public override int GetOffset()
        {
            return CurrentObjectId;
        }

        public override void SetOffset(int offset)
        {
            CurrentObjectId = offset;
        }

        private void GetAllObjects()
        {
            HtmlElements = new List<HTMLObject>();

            foreach( IHTMLElement lHtmlElement in HtmlDocument)
            {
                HtmlElements.Add(new HTMLObject(lHtmlElement));
            }
        }

        //Return objectinterface to subobjects
        public override ObjectInterfacePrototipe GetSubObjects()
        {
            if (HtmlElements.Count==0 || HtmlElements[CurrentObjectId].GetHtmlElement().children == null)
                return null;

            return new HTMLInterface((IHTMLElementCollection)HtmlElements[CurrentObjectId].GetHtmlElement().children);
        }

        private IHTMLDocument2 HTMLToDom(string html)
        {
            doc = new HTMLDocumentClass();
            doc.write(new object[] { html });
            doc.close();

            return doc;
        }

        public HTMLInterface(string html)
        {
            HtmlDocument = (IHTMLElementCollection)HTMLToDom(html).body.children;
        }

        public HTMLInterface(IHTMLElementCollection lElementCollection)
        {
            HtmlDocument = lElementCollection;
        }
    }
}
