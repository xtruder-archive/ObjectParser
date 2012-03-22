using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectParser
{
    class HTMLInterfaceTest
    {
        public static bool TestHtmlInterface()
        {
            string HtmlCode = "<html><head></head><body><a>Test</a><p><a>test2<a/>test3</p></body></html>";

            HTMLInterface TestInterface = new HTMLInterface(HtmlCode);
            ObjectParser.HTMLObject lHtmlObject = new HTMLObject(null);
            lHtmlObject = (ObjectParser.HTMLObject)TestInterface.GetFirstObject();
            if (lHtmlObject.GetObjectName() != "A" || ((ObjectAttributeString)lHtmlObject.GetAttributeByName("innerText")).GetAttributeValue() != "Test")
                return false;
            lHtmlObject = (ObjectParser.HTMLObject)TestInterface.GetNextObject();
            if (lHtmlObject.GetObjectName() != "P")
                return false;
            HTMLInterface SubInterface = (HTMLInterface)TestInterface.GetSubObjects();
            if (SubInterface == null)
                return false;
            lHtmlObject = (ObjectParser.HTMLObject)SubInterface.GetFirstObject();
            if (lHtmlObject.GetObjectName() != "A")
                return false;
            if (((ObjectAttributeString)lHtmlObject.GetAttributeByName("innerText")).GetAttributeValue() != "test2")
                return false;
            ObjectParser.HTMLObject lHtmlObject2 = (ObjectParser.HTMLObject)TestInterface.GetCurrentObject();
            if (((ObjectAttributeString)lHtmlObject2.GetAttributeByName("innerText")).GetAttributeValue() != "test2test3")
                return false;

            return true;
        }
    }

    class TestCallback
    {
        bool called = false;

        public void Call(ObjectInterfacePrototipe lObjectInterface)
        {
            called = true;
        }

        public void moredivs(ObjectInterfacePrototipe lObjectInterface)
        {
            called = true;
        }

        public void moreps(ObjectInterfacePrototipe lObjectInterface)
        {
            called = true;
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
            evaluation.SetAttributeName("ParseCount");
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
    }
}
