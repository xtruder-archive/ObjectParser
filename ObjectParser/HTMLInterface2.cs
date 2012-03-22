using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace ObjectParser
{
    class HTMLObject2: ObjectPrototipe
    {
    }

    //Html interface using HtmlAgilityPack(Doesn't have memory leaks like mshtml over com)
    class HTMLInterface2: ObjectInterfacePrototipe
    {
        HtmlDocument MainDocument = null;
        HtmlNode CurrentNode= null;
        List<HTMLObject> HtmlElements= new List<HTMLObject>();

        public override ObjectPrototipe GetFirstObject()
        {
            return null;
        }

        public HTMLInterface2(string html)
        {
            MainDocument = new HtmlDocument();
            MainDocument.LoadHtml(html);
            CurrentNode= MainDocument.DocumentNode;
        }
    }
}
