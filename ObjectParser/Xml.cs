using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ObjectParser
{
    class CallbackEntry
    {
        string CallbackName;
        Callback CallbacFunction;

        public string GetCallbackName()
        {
            return CallbackName;
        }

        public Callback GetCallbackFunction()
        {
            return CallbacFunction;
        }

        public CallbackEntry(string lCallbackName, Callback lCallbackFunction)
        {
            CallbackName = lCallbackName;
            CallbacFunction = lCallbackFunction;
        }
    }

    class CallbackStore
    {
        List<CallbackEntry> CallbackEntries = new List<CallbackEntry>();

        public void AddCallback(CallbackEntry lCallbackEntry)
        {
            CallbackEntries.Add(lCallbackEntry);
        }

        public CallbackEntry GetCallbackEntryByName(string lCallbackName)
        {
            foreach (CallbackEntry lCallbackEntry in CallbackEntries)
            {
                if (lCallbackEntry.GetCallbackName() == lCallbackName)
                    return lCallbackEntry;
            }

            return null;
        }
    }

    class Xml
    {
        CallbackStore cCallbackStore;

        public ParserPrimitives ParseXML(string XmlData)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(XmlData);

            ParserPrimitives FirstPrimitive = ParseNode(XmlDoc.DocumentElement);
            FirstPrimitive.AddSubParserPrimitives(ParseNodeLevel(XmlDoc.DocumentElement));

            return FirstPrimitive;
        }

        public ParserPrimitives ParseXMLFromFile(string Filename)
        {
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(Filename);

            ParserPrimitives FirstPrimitive = ParseNode(XmlDoc.DocumentElement);
            FirstPrimitive.AddSubParserPrimitives(ParseNodeLevel(XmlDoc.DocumentElement));

            return FirstPrimitive;
        }

        private List<ParserPrimitives> ParseNodeLevel(XmlElement Element)
        {
            List<ParserPrimitives> CurrentNodePrimitives= new List<ParserPrimitives>();
            foreach( XmlNode el in Element.ChildNodes )
            {
                ParserPrimitives ElementPrimitive = ParseNode(el);
                ElementPrimitive.AddSubParserPrimitives(ParseNodeLevel((XmlElement)el));

                CurrentNodePrimitives.Add(ElementPrimitive);
            }

            return CurrentNodePrimitives;
        }

        private ParserPrimitives ParseNode( XmlNode ElementToParse )
        {
            XmlNodeType NodeType= ElementToParse.NodeType;
            if (NodeType != XmlNodeType.Element)
                return null;

            ParserPrimitives ParserPrimitive = null;

            if (ElementToParse.Name == "object")
            {
                ParserPrimitive = new ParserObject();
                List<ObjectAttribute> Attributes= new List<ObjectAttribute>();

                foreach( XmlAttribute Attribute in ElementToParse.Attributes )
                {
                    if (Attribute.Name == "callback")
                    {
                        ParserPrimitive.SetCallback(cCallbackStore.GetCallbackEntryByName(Attribute.Value).GetCallbackFunction());
                        continue;
                    }
                    Attributes.Add(new ObjectAttributeRegex(Attribute.Name, Attribute.Value));
                }

                ((ParserObject)ParserPrimitive).SetObjectAttributes(Attributes);
            }
            else if (ElementToParse.Name == "relation")
            {
                ParserPrimitive = new ParserObjectRelation();
                string RelationFunctionName="";
                int min_evaluation = 0, max_evaluation = 0;
                EvaluationMode evaluation_mode= EvaluationMode.Max;
                FunctionIO FunctionInput = new FunctionIO();
                foreach (XmlAttribute Attribute in ElementToParse.Attributes)
                {
                    if (Attribute.Name == "callback")
                    {
                        ParserPrimitive.SetCallback(cCallbackStore.GetCallbackEntryByName(Attribute.Value).GetCallbackFunction());
                        continue;
                    }
                    else if (Attribute.Name == "relation_function")
                    {
                        RelationFunctionName = Attribute.Value;
                        continue;
                    }
                    else if (Attribute.Name == "evaluation_min")
                    {
                        min_evaluation = System.Int32.Parse(Attribute.Value);
                        continue;
                    }
                    else if (Attribute.Name == "evaluation_max")
                    {
                        min_evaluation = System.Int32.Parse(Attribute.Value);
                        continue;
                    }
                    else if (Attribute.Name == "evaluation_mode")
                    {
                        switch (Attribute.Value)
                        {
                            case "min":
                                evaluation_mode = EvaluationMode.Min;
                                break;
                            case "max":
                                evaluation_mode = EvaluationMode.Max;
                                break;
                        }
                        continue;
                    }
                    else if (Attribute.Name == "depth_max")
                    {
                        FunctionInput.AddFunctionParameter( new ObjectAttributeInt("MaxDepth",System.Int32.Parse(Attribute.Value)) );
                    }
                    else if (Attribute.Name == "ocurencies_min")
                    {
                        FunctionInput.AddFunctionParameter( new ObjectAttributeInt("MinOccurencies",System.Int32.Parse(Attribute.Value)) );
                    }
                    else if (Attribute.Name == "occurencies_max")
                    {
                        FunctionInput.AddFunctionParameter( new ObjectAttributeInt("MaxOccurencies",System.Int32.Parse(Attribute.Value)) );
                    }

                    FunctionInput.AddFunctionParameter( new ObjectAttributeString(Attribute.Name, Attribute.Value) );
                }

                ObjectAttributeEvaluation evaluation= new ObjectAttributeEvaluation( evaluation_mode, min_evaluation, max_evaluation );
                evaluation.AttributeName = "ParseCount";
                FunctionInput.AddFunctionParameter( evaluation );
                RelationFunction tRelationFunction= null;
                ParserObjectRelation Relation = (ParserObjectRelation)ParserPrimitive;
                switch( RelationFunctionName )
                {
                    case "cross":
                        tRelationFunction = new RelationFunctionCROSS(ref Relation, FunctionInput);
                        break;
                    case "more":
                        tRelationFunction = new RelationFunctionMORE(ref Relation, FunctionInput);
                        break;
                    case "and":
                        tRelationFunction = new RelationFunctionAND(ref Relation, FunctionInput);
                        break;
                    case "or":
                        tRelationFunction = new RelationFunctionOR(ref Relation, FunctionInput);
                        break;
                }

                Relation.SetRelationFunction(tRelationFunction);
            }

            return ParserPrimitive;
        }

        private XmlTextReader XmlAsString(string XmlData)
        {
            byte[] tArray = new byte[XmlData.Length];
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            tArray = encoding.GetBytes(XmlData);

            System.IO.MemoryStream MemoryStream = new System.IO.MemoryStream(tArray);
            MemoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            XmlTextReader XmlReader = new XmlTextReader(MemoryStream);

            return XmlReader;
        }

        public CallbackStore GetCallbackStore()
        {
            return cCallbackStore;
        }

        public Xml()
        {
            cCallbackStore = new CallbackStore();
        }

        public Xml(CallbackStore lCallbackStore)
        {
            cCallbackStore = lCallbackStore;
        }
    }
}
