using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace ObjectParser
{
    [TestFixture]
    class HTMLInterfaceTest
    {
        [Test]
        public static void TestHtmlInterface()
        {
            string HtmlCode = "<html><head></head><body><a>Test</a><p><a>test2<a/>test3</p></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            ObjectParser.HTMLObject lHtmlObject = new HTMLObject(null);
            lHtmlObject = (ObjectParser.HTMLObject)TestInterface.GetFirstObject();
            if (lHtmlObject.GetObjectName() != "A" || ((ObjectAttributeString)lHtmlObject.GetAttributeByName("innerText")).Value != "Test")
                return;
            lHtmlObject = (ObjectParser.HTMLObject)TestInterface.GetNextObject();
            Assert.AreNotEqual(lHtmlObject.GetObjectName(), "P");
            HTMLInterface SubInterface = (HTMLInterface)TestInterface.GetSubObjects();
            if (SubInterface == null)
                return;
            lHtmlObject = (ObjectParser.HTMLObject)SubInterface.GetFirstObject();
            if (lHtmlObject.GetObjectName() != "A")
                return;
            if (((ObjectAttributeString)lHtmlObject.GetAttributeByName("innerText")).Value != "test2")
                return;
            ObjectParser.HTMLObject lHtmlObject2 = (ObjectParser.HTMLObject)TestInterface.GetCurrentObject();
            if (((ObjectAttributeString)lHtmlObject2.GetAttributeByName("innerText")).Value != "test2test3")
                return;

            return;
        }
    }

    class TestCallback
    {
        bool called = false;

        public bool Call(ObjectInterfacePrototipe lObjectInterface)
        {
            return true;
        }

        public bool moredivs(ObjectInterfacePrototipe lObjectInterface)
        {
            return true;
        }

        public bool moreps(ObjectInterfacePrototipe lObjectInterface)
        {
            return true;
        }

        public bool moremors(ObjectInterfacePrototipe lObjectInterface)
        {
            return true;
        }

        public bool moretables(ObjectInterfacePrototipe lObjectInterface)
        {
            return true;
        }
    }

    class ParserTest
    {
        public static void Test1()
        {
            string HtmlCode = "<html><head></head><body><div><a id='abcd'>Test</a><p id='defgh'><a>test2<a/>test3</p><a>1</a><a>2</a><a>3</a><a>4</a></div></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            TestInterface.GetFirstObject();

            List<ObjectAttribute> RepresentAAttributes1 = new List<ObjectAttribute>();
            RepresentAAttributes1.Add(new ObjectAttributeString("name", "A"));
            RepresentAAttributes1.Add(new ObjectAttributeRegex("id", "[a-z]+"));
            RepresentAAttributes1.Add(new ObjectAttributeRegex("innerText", "[A-Z]+"));

            List<ObjectAttribute> RepresentAAttributes2 = new List<ObjectAttribute>();
            RepresentAAttributes2.Add(new ObjectAttributeString("name", "A"));
            RepresentAAttributes2.Add(new ObjectAttributeRegex("id", "[a-z]+"));
            RepresentAAttributes2.Add(new ObjectAttributeRegex("innerText", "Test"));

            List<ObjectAttribute> RepresentAAttributes3 = new List<ObjectAttribute>();
            RepresentAAttributes3.Add(new ObjectAttributeString("name", "A"));
            RepresentAAttributes3.Add(new ObjectAttributeRegex("innerText", "test2"));

            List<ObjectAttribute> RepresentAAttributes4 = new List<ObjectAttribute>();
            RepresentAAttributes4.Add(new ObjectAttributeString("name", "A"));
            RepresentAAttributes4.Add(new ObjectAttributeRegex("innerText", "\\d+"));

            List<ObjectAttribute> RepresentPAttributes = new List<ObjectAttribute>();
            RepresentPAttributes.Add(new ObjectAttributeString("name", "P"));
            RepresentPAttributes.Add(new ObjectAttributeRegex("id", "def[a-z]+"));

            ParserObject RepresentA1 = new ParserObject(RepresentAAttributes1);
            ParserObject RepresentA2 = new ParserObject(RepresentAAttributes2);
            ParserObject RepresentA3 = new ParserObject(RepresentAAttributes3);
            ParserObject RepresentA4 = new ParserObject(RepresentAAttributes4);
            ParserObject RepresentP = new ParserObject(RepresentPAttributes);
            RepresentP.AddSubParserPrimitive(RepresentA3);

            List<ObjectAttribute> RepresentDIVAttributes = new List<ObjectAttribute>();
            RepresentDIVAttributes.Add(new ObjectAttributeString("name", "DIV"));

            ParserObject RepresentDIV = new ParserObject(RepresentDIVAttributes);

            ParserObjectRelation RelationAND = new ParserObjectRelation();
            RelationAND.SetRelationFunction(new RelationFunctionAND(ref RelationAND, null));
            RelationAND.AddSubParserPrimitive(RepresentA1);
            RelationAND.AddSubParserPrimitive(RepresentA2);

            ParserObjectRelation RelationOR = new ParserObjectRelation();
            RelationOR.SetRelationFunction(new RelationFunctionOR(ref RelationOR, null));
            RelationOR.AddSubParserPrimitive(RelationAND);
            RelationOR.AddSubParserPrimitive(RepresentP);

            ParserObjectRelation RelationMORE = new ParserObjectRelation();
            RelationMORE.SetRelationFunction(new RelationFunctionMORE(ref RelationMORE, null));
            RelationMORE.AddSubParserPrimitive(RepresentA4);

            RepresentDIV.AddSubParserPrimitive(RelationOR);
            RepresentDIV.AddSubParserPrimitive(RepresentP);
            RepresentDIV.AddSubParserPrimitive(RelationMORE);

            ObjectInterfacePrototipe tInterface = (ObjectInterfacePrototipe)TestInterface;
            FunctionIO Result = RepresentDIV.CheckRelations(ref tInterface, null, true);
            Result.GetType();
        }

        public static void Test2()
        {
            string HtmlCode = "<html><head></head><body><div><div>a</div><div>b</div><div>c</div><p>a</p><p>b</p><p>c</p><a>test</a></div></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            TestInterface.GetFirstObject();

            List<ObjectAttribute> RepresentAAttributes1 = new List<ObjectAttribute>();
            RepresentAAttributes1.Add(new ObjectAttributeRegex("name", ".+"));
            //RepresentAAttributes1.Add(new ObjectAttributeRegex("innerText", "[a-z]"));
            ParserObject RepresentA1 = new ParserObject(RepresentAAttributes1);

            List<ObjectAttribute> RepresentPAttributes = new List<ObjectAttribute>();
            RepresentPAttributes.Add(new ObjectAttributeRegex("name", "P"));
            RepresentPAttributes.Add(new ObjectAttributeRegex("innerText", "[a-z]"));
            ParserObject RepresentP = new ParserObject(RepresentPAttributes);

            List<ObjectAttribute> RepresentAAttributes2 = new List<ObjectAttribute>();
            RepresentAAttributes2.Add(new ObjectAttributeString("name", "A"));
            RepresentAAttributes2.Add(new ObjectAttributeRegex("innerText", "[a-z]+"));
            ParserObject RepresentA2 = new ParserObject(RepresentAAttributes2);

            ObjectAttributeEvaluation evaluation = new ObjectAttributeEvaluation(EvaluationMode.Max, 1, 4);
            evaluation.AttributeName="ParseCount";
            FunctionIO RelationMOREInput = new FunctionIO();
            RelationMOREInput.AddFunctionParameter(evaluation);

            TestCallback CallbackClass = new TestCallback();

            ParserObjectRelation RelationMORE = new ParserObjectRelation();
            RelationMORE.SetRelationFunction(new RelationFunctionMORE(ref RelationMORE, RelationMOREInput));
            RelationMORE.AddSubParserPrimitive(RepresentA1);
            RelationMORE.SetCallback(CallbackClass.Call);

            ParserObjectRelation RelationMORE2 = new ParserObjectRelation();
            RelationMORE2.SetRelationFunction(new RelationFunctionMORE(ref RelationMORE2, RelationMOREInput));
            RelationMORE2.AddSubParserPrimitive(RepresentP);
            RelationMORE2.SetCallback(CallbackClass.Call);

            List<ObjectAttribute> RepresentDIVAttributes = new List<ObjectAttribute>();
            RepresentDIVAttributes.Add(new ObjectAttributeString("name", "DIV"));

            ParserObject RepresentDIV = new ParserObject(RepresentDIVAttributes);
            RepresentDIV.AddSubParserPrimitive(RelationMORE);
            RepresentDIV.AddSubParserPrimitive(RelationMORE2);
            RepresentDIV.AddSubParserPrimitive(RepresentA2);

            ObjectInterfacePrototipe tInterface = (ObjectInterfacePrototipe)TestInterface;
            FunctionIO Result = RepresentDIV.CheckRelations(ref tInterface, null, true);
            Result.GetType();
        }

        public static void Test3()
        {
            string HtmlCode = "<html><head></head><body><div><div>a</div><div>b</div><div>c</div><p>a</p><p>b</p><p>c</p><a>test</a></div></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            TestInterface.GetFirstObject();

            TestCallback CallbackClass= new TestCallback();
            CallbackStore lCallbackStore= new CallbackStore();
            lCallbackStore.AddCallback( new CallbackEntry("moredivs",CallbackClass.moredivs) );
            lCallbackStore.AddCallback( new CallbackEntry("moreps",CallbackClass.moreps) );
            Xml XmlParse = new Xml(lCallbackStore);
            ParserPrimitives lPrimitive= XmlParse.ParseXMLFromFile("Test.xml");

            ObjectInterfacePrototipe tInterface = (ObjectInterfacePrototipe)TestInterface;
            lPrimitive.CheckRelations(ref tInterface, null, true);
        }

        public static void Test4()
        {
            string HtmlCode = "<html><head></head><body><div><div>a</div><div><a><p>Test</p><p>Test2</p></a><p>1a</p></div><p>m</p><div><div><a><bold><p>Test</p><p>Test2</p></bold></a><p>1a</p></div><p>a</p></div><p>b</p><p>c</p><a>test</a></div></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            TestInterface.GetFirstObject();

            TestCallback CallbackClass = new TestCallback();
            CallbackStore lCallbackStore = new CallbackStore();
            lCallbackStore.AddCallback(new CallbackEntry("moredivs", CallbackClass.moredivs));
            lCallbackStore.AddCallback(new CallbackEntry("moreps", CallbackClass.moreps));
            lCallbackStore.AddCallback(new CallbackEntry("moremors", CallbackClass.moremors));
            Xml XmlParse = new Xml(lCallbackStore);
            ParserPrimitives lPrimitive = XmlParse.ParseXMLFromFile("Test2.xml");

            ObjectInterfacePrototipe tInterface = (ObjectInterfacePrototipe)TestInterface;
            lPrimitive.CheckRelations(ref tInterface, null, true);
        }

        class OffsetRelation
        {
            int Offset = 0;
            string OffsetName = "";

            public int GetOffset()
            {
                return Offset;
            }

            public string GetOffsetName()
            {
                return OffsetName;
            }

            OffsetRelation(int loffset, string lOffsetName)
            {
                Offset = loffset;
                OffsetName = lOffsetName;
            }
        }

        class OffsetDescriptor
        {
            List<OffsetRelation> OffsetDescriptors = new List<OffsetRelation>();

            public void AddOffset( OffsetRelation lOffsetDescriptor )
            {
                OffsetDescriptors.Add( lOffsetDescriptor );
            }

            public OffsetRelation GetOffsetByName(string lOffsetName)
            {
                foreach( OffsetRelation lOffsetRelation in OffsetDescriptors )
                {
                    if (lOffsetRelation.GetOffsetName() == lOffsetName)
                        return lOffsetRelation;
                }

                return null;
            }

            public OffsetRelation GetOffsetByOffset(int lOffset)
            {
                foreach (OffsetRelation lOffsetRelation in OffsetDescriptors)
                {
                    if (lOffsetRelation.GetOffset() == lOffset)
                        return lOffsetRelation;
                }

                return null;
            }
        }

        class ExtractTablesCallback
        {
            List<string> ExtractedTables = new List<string>();

            public bool NewTable(ObjectInterfacePrototipe lObjectInterface)
            {
                ObjectAttributeString lObjectAttribute = (ObjectAttributeString)lObjectInterface.GetCurrentObject().GetAttributeByName("outerHTML");
                ExtractedTables.Add(lObjectAttribute.Value);

                return true;
            }

            public bool Preheader1(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Preheader1Element(ObjectInterfacePrototipe lObjectInterface)
            {
                if(lObjectInterface.GetObjectCount() < 32)
                    return false;

                return true;
            }

            public bool Preheader2(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Preheader2Element(ObjectInterfacePrototipe lObjectInterface)
            {
                if (lObjectInterface.GetObjectCount() < 32)
                    return false;
                return true;
            }

            public bool NewBitfields(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool NewBitfield(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool NewBlankBitfield(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader1(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader1Element(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader2(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader2Bits(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader2Access(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader2Name(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool Postheader2Description(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool NewBitfieldDescription(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public bool NewBitfieldDescriptionField(ObjectInterfacePrototipe lObjectInterface)
            {
                return true;
            }

            public void WriteTablesAsHtml( string filename )
            {
                string output="<html><head></head><body>";
                foreach (string Table in ExtractedTables)
                {
                    output += "<div>" + Table + "</div>";
                }
                output += "</body></html>";

                ReadFile.WriteStringToFile(filename, output);
            }
        }

        public static void MarvellExtract()
        {
            string HtmlCode = ReadFile.ReadFileAsString("test.html");

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            TestInterface.GetFirstObject();

            ExtractTablesCallback CallbackClass = new ExtractTablesCallback();
            CallbackStore lCallbackStore = new CallbackStore();
            lCallbackStore.AddCallback(new CallbackEntry("NewTable", CallbackClass.NewTable));
            lCallbackStore.AddCallback(new CallbackEntry("Preheader1", CallbackClass.Preheader1));
            lCallbackStore.AddCallback(new CallbackEntry("Preheader1Element", CallbackClass.Preheader1Element));
            lCallbackStore.AddCallback(new CallbackEntry("Preheader2", CallbackClass.Preheader2));
            lCallbackStore.AddCallback(new CallbackEntry("Preheader2Element", CallbackClass.Preheader2Element));
            lCallbackStore.AddCallback(new CallbackEntry("NewBitfield", CallbackClass.NewBitfield));
            lCallbackStore.AddCallback(new CallbackEntry("NewBlankBitfield", CallbackClass.NewBlankBitfield));
            lCallbackStore.AddCallback(new CallbackEntry("NewBitfields", CallbackClass.NewBitfields));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader1", CallbackClass.Postheader1));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader1Element", CallbackClass.Postheader1Element));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader2", CallbackClass.Postheader2));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader2Bits", CallbackClass.Postheader2Bits));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader2Access", CallbackClass.Postheader2Access));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader2Name", CallbackClass.Postheader2Name));
            lCallbackStore.AddCallback(new CallbackEntry("Postheader2Description", CallbackClass.Postheader2Description));
            lCallbackStore.AddCallback(new CallbackEntry("NewBitfieldDescription", CallbackClass.NewBitfieldDescription));
            lCallbackStore.AddCallback(new CallbackEntry("NewBitfieldDescriptionField", CallbackClass.NewBitfieldDescriptionField));

            Xml XmlParse = new Xml(lCallbackStore);
            ParserPrimitives lPrimitive = XmlParse.ParseXMLFromFile("Test3.xml");

            ObjectInterfacePrototipe tInterface = (ObjectInterfacePrototipe)TestInterface;
            lPrimitive.CheckRelations(ref tInterface, null, true);
            CallbackClass.WriteTablesAsHtml("tables.html");
        }
    }
}
