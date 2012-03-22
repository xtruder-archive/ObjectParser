using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectParser
{
    class FunctionIO
    {
        List<ObjectAttribute> Parameters= new List<ObjectAttribute>();

        public void AddFunctionParameter(ObjectAttribute lParameter)
        {
            Parameters.Add(lParameter);
        }

        public ObjectAttribute GetFunctionParameterByName(string name)
        {
            foreach (ObjectAttribute Parameter in Parameters)
            {
                if (Parameter.GetAttributeName() == name)
                    return Parameter;
            }

            return null;
        }
    }

    enum EvaluationMode
    {
        Max,
        Min
    }

    class ObjectAttributeEvaluation: ObjectAttribute
    {
        int MinCount = -1;
        int MaxCount = -1;

        int Current = -1;
        EvaluationMode cEvaluationMode = EvaluationMode.Max;

        public void EvaluateFirst()
        {
            if (MinCount == -1)
                MinCount = 0;
            if (MaxCount == -1)
                MaxCount = 1000000000;

            Current = 1;
        }

        public bool EvaluateNext( int Value )
        {
            if (Value > MinCount && Value < MaxCount)
            {
                if (cEvaluationMode == EvaluationMode.Max)
                    if (Value > Current)
                        Current = Value;
                if (cEvaluationMode == EvaluationMode.Min)
                    if (Value < Current)
                        Current = Value;

                return true;
            }

            return false;
        }

        public bool SubEvaluate(ObjectAttributeEvaluation Evaluation)
        {
            return EvaluateNext(Evaluation.GetEvaluationValue());
        }

        public int GetEvaluationValue()
        {
            return Current;
        }

        public ObjectAttributeEvaluation(EvaluationMode lEvaluationMode, int Min, int Max)
        {
            cEvaluationMode = lEvaluationMode;
            if (Min != Max)
            {
                MinCount = Min;
                MaxCount = Max;
            }
            SetAttribueType(AttributeType.Evalution);
            EvaluateFirst();
        }

        public ObjectAttributeEvaluation()
        {
            SetAttribueType(AttributeType.Evalution);
            EvaluateFirst();
        }
    }

    //Function to check relations betwene objects
    class RelationFunction
    {
        protected ParserObjectRelation cParserObjectRelation = null;
        protected FunctionIO cFunctionParameters;

        public virtual FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            return null;
        }

        public RelationFunction(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters)
        {
            cParserObjectRelation = lParserObjectRelation;
            cFunctionParameters = lFunctionParameters;
        }
    }

    class RelationFunctionOR: RelationFunction
    {
        public RelationFunctionOR(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            FunctionIO ReturnContainer = new FunctionIO();
            ObjectAttributeInt ReturnValue1 = new ObjectAttributeInt("ret", 1);
            ObjectAttributeInt ReturnValue0 = new ObjectAttributeInt("ret", 0);
            ObjectAttributeEvaluation Evaluation= null;
            if (cFunctionParameters == null || cFunctionParameters.GetFunctionParameterByName("ParseCount") == null)
            {
                Evaluation = new ObjectAttributeEvaluation();
                Evaluation.SetAttributeName("ParseCount");
            }
            else
                Evaluation = (ObjectAttributeEvaluation)cFunctionParameters.GetFunctionParameterByName("ParseCount");

            bool result = false;
            List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
            for (int x = 0; x < SubPrimitives.Count; x++)
            {
                ParserPrimitives tPrimitives;
                if (SubPrimitives.Count == x + 1)
                    tPrimitives = null;
                else
                    tPrimitives = SubPrimitives[x + 1];
                FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, tPrimitives,CallCalback);
                if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() == 1)
                {
                    result = true;
                    Evaluation.SubEvaluate((ObjectAttributeEvaluation)ret.GetFunctionParameterByName("ParseCount"));
                }
            }

            if (result)
            {
                ReturnContainer.AddFunctionParameter((ObjectAttribute)Evaluation);
                ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue1);
                return ReturnContainer;
            }

            ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
            return ReturnContainer;
        }
    }

    class RelationFunctionAND : RelationFunction
    {
        public RelationFunctionAND(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            FunctionIO ReturnContainer = new FunctionIO();
            ObjectAttributeInt ReturnValue1 = new ObjectAttributeInt("ret", 1);
            ObjectAttributeInt ReturnValue0 = new ObjectAttributeInt("ret", 0);
            ObjectAttributeEvaluation Evaluation = null;
            if (cFunctionParameters == null || cFunctionParameters.GetFunctionParameterByName("ParseCount") == null)
            {
                Evaluation = new ObjectAttributeEvaluation();
                Evaluation.SetAttributeName("ParseCount");
            }
            else
                Evaluation = (ObjectAttributeEvaluation)cFunctionParameters.GetFunctionParameterByName("ParseCount");

            int Positives = 0;
            List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
            for (int x = 0; x < SubPrimitives.Count; x++)
            {
                ParserPrimitives tPrimitives;
                if (SubPrimitives.Count == x + 1)
                    tPrimitives = null;
                else
                    tPrimitives = SubPrimitives[x + 1];
                FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, tPrimitives, CallCalback);
                if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() == 1)
                {
                    Positives++;
                    if (!Evaluation.SubEvaluate((ObjectAttributeEvaluation)ret.GetFunctionParameterByName("ParseCount")))
                    {
                        ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
                        return ReturnContainer;
                    }
                }
            }

            if (Positives == SubPrimitives.Count)
            {
                ReturnContainer.AddFunctionParameter((ObjectAttribute)Evaluation);
                ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue1);
                return ReturnContainer;
            }

            ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
            return ReturnContainer;
        }
    }

    class RelationFunctionMORE : RelationFunction
    {
        public RelationFunctionMORE(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            FunctionIO ReturnContainer = new FunctionIO();
            ObjectAttributeInt ReturnValue1 = new ObjectAttributeInt("ret", 1);
            ObjectAttributeInt ReturnValue0 = new ObjectAttributeInt("ret", 0);
            ObjectAttributeEvaluation Evaluation = null;
            if (cFunctionParameters == null || cFunctionParameters.GetFunctionParameterByName("ParseCount") == null)
            {
                Evaluation = new ObjectAttributeEvaluation();
                Evaluation.SetAttributeName("ParseCount");
            }
            else
                Evaluation = (ObjectAttributeEvaluation)cFunctionParameters.GetFunctionParameterByName("ParseCount");

            bool More = true;
            int TotalMove= 0;
            int StartOffset = lObjectInterface.GetOffset();
            bool eof = false;

            while (More)
            {
                int Positives = 0;
                int CurrentMove = 0;

                if (NextElement != null)
                {
                    FunctionIO ret = NextElement.CheckRelations(ref lObjectInterface, NextElement.GetNextPrimitive(), false);
                    if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() == 1)
                    {
                        break;
                    }
                }

                List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
                for (int x = 0; x < SubPrimitives.Count; x++)
                {
                    ParserPrimitives tPrimitives;
                    if (SubPrimitives.Count == x + 1)
                        tPrimitives = null;
                    else
                        tPrimitives = SubPrimitives[x + 1];
                    FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, tPrimitives, CallCalback);
                    if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() == 1)
                    {
                        CurrentMove += ((ObjectAttributeEvaluation)ret.GetFunctionParameterByName("ParseCount")).GetEvaluationValue();
                        Positives++;
                    }
                    eof=lObjectInterface.MoveByOffset(CurrentMove);
                }

                TotalMove += CurrentMove;

                if (Positives != SubPrimitives.Count || eof)
                {
                    More = false;
                }
            }

            if (!Evaluation.EvaluateNext(TotalMove))
            {
                ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
                return ReturnContainer;
            }

            lObjectInterface.SetOffset(StartOffset);

            ReturnContainer.AddFunctionParameter(Evaluation);
            ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue1);
            return ReturnContainer;
        }
    }

    delegate void Callback ( ObjectInterfacePrototipe lObjectInterface );

    enum ParserPrimitiveTypes
    {
        ObjectRelationType,
        ObjectType
    }

    class ParserPrimitives
    {
        //Subelements of this primitive
        protected List<ParserPrimitives> cParserPrimitives = new List<ParserPrimitives>();
        protected ParserPrimitiveTypes ParserPrimitiveType;
        protected ParserPrimitives NextPrimitive = null;
        protected Callback cCallback= null;

        public void SetCallback(Callback lCallback)
        {
            cCallback = lCallback;
        }

        public void SetNextPrimitive(ParserPrimitives lNextPrimitive)
        {
            NextPrimitive = lNextPrimitive;
        }

        public ParserPrimitives GetNextPrimitive()
        {
            return NextPrimitive;
        }

        public List<ParserPrimitives> GetSubParserPrimitives()
        {
            return cParserPrimitives;
        }

        public void AddSubParserPrimitive(ParserPrimitives lParserPrimitive)
        {
            cParserPrimitives.Add(lParserPrimitive);
        }

        public void AddSubParserPrimitives(List<ParserPrimitives> lParserPrimitives)
        {
            foreach (ParserPrimitives lParserPrimitive in lParserPrimitives)
            {
                cParserPrimitives.Add(lParserPrimitive);
            }
        }

        public ParserPrimitiveTypes GetParserPrimitiveType()
        {
            return ParserPrimitiveType;
        }

        public virtual FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            return null;
        }
    }

    //Class calculating relations betwene objects
    class ParserObjectRelation : ParserPrimitives
    {
        RelationFunction cRelationFunction= null;

        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            FunctionIO ret= cRelationFunction.CheckRelations(ref lObjectInterface, NextElement, CallCalback);
            if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() == 1)
                if (cCallback != null && CallCalback)
                    cCallback(lObjectInterface);
            return ret;
        }

        public void SetRelationFunction(RelationFunction lRelationFunction)
        {
            cRelationFunction = lRelationFunction;
        }

        public ParserObjectRelation( RelationFunction lRelationFunction )
        {
            ParserPrimitiveType = ParserPrimitiveTypes.ObjectRelationType;
            cRelationFunction = lRelationFunction;
        }

        public ParserObjectRelation()
        {
            ParserPrimitiveType = ParserPrimitiveTypes.ObjectRelationType;
        }
    }

    //representing object defining one entry for object relation
    class ParserObject : ParserPrimitives
    {
        //Attributi tega objekta
        List<ObjectAttribute> cObjectAttributes = new List<ObjectAttribute>();

        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            FunctionIO ReturnContainer= new FunctionIO();
            ObjectAttributeInt ReturnValue1 = new ObjectAttributeInt("ret",1);
            ObjectAttributeInt ReturnValue0 = new ObjectAttributeInt("ret",0);
            ObjectAttributeEvaluation Evaluation =  new ObjectAttributeEvaluation();
            Evaluation.SetAttributeName("ParseCount");

            if (!CheckCurrentObject(ref lObjectInterface))
            {
                ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
                return ReturnContainer;
            }

            if (cParserPrimitives.Count == 0)
            {
                if (cCallback != null && CallCalback)
                    cCallback(lObjectInterface);

                ReturnContainer.AddFunctionParameter((ObjectAttribute)Evaluation);
                ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue1);
                return ReturnContainer;
            }

            //Here we preset next primitives
            for (int x = 0; x < cParserPrimitives.Count-1; x++)
            {
                cParserPrimitives[x].SetNextPrimitive(cParserPrimitives[x + 1]);
            }

            ObjectInterfacePrototipe SubObjects= lObjectInterface.GetSubObjects();
            SubObjects.GetFirstObject();
            ObjectAttributeEvaluation ParseCountEvaluation = new ObjectAttributeEvaluation( EvaluationMode.Max, 1, 100000000);
            for( int x=0; x< cParserPrimitives.Count; x++ )
            {
                ParserPrimitives tPrimitives;
                if (cParserPrimitives.Count == x + 1)
                    tPrimitives = null;
                else
                    tPrimitives = cParserPrimitives[x + 1];
                FunctionIO ret = cParserPrimitives[x].CheckRelations(ref SubObjects, tPrimitives, CallCalback);
                if (((ObjectAttributeInt)ret.GetFunctionParameterByName("ret")).GetAttributeValue() != 1)
                {
                    ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue0);
                    return ReturnContainer;
                }

                SubObjects.GetOffsetObject(((ObjectAttributeEvaluation)ret.GetFunctionParameterByName("ParseCount")).GetEvaluationValue());
            }

            if (cCallback != null && CallCalback)
                cCallback(lObjectInterface);
             
            ReturnContainer.AddFunctionParameter((ObjectAttribute)Evaluation);
            ReturnContainer.AddFunctionParameter((ObjectAttribute)ReturnValue1);
            return ReturnContainer;
        }

        //Singleton check
        bool CheckCurrentObject(ref ObjectInterfacePrototipe lObjectInterface)
        {
            ObjectPrototipe lObjectPrototipe= lObjectInterface.GetCurrentObject();

            if (GetAttributeByName("name") != null)
            {
                ObjectAttributeString lObjectAttributeString = new ObjectAttributeString();
                lObjectAttributeString.SetAttributeName("name");
                lObjectAttributeString.SetAttributeValue(lObjectPrototipe.GetObjectName());
                if (!GetAttributeByName("name").ComapreAttributes((ObjectAttribute)lObjectAttributeString))
                    return false;
            }

            foreach (ObjectAttribute lObjectAttribute in cObjectAttributes)
            {
                if (lObjectAttribute.GetAttributeName() == "name")
                    continue;
                if(!lObjectAttribute.ComapreAttributes(lObjectPrototipe.GetAttributeByName(lObjectAttribute.GetAttributeName())))
                    return false;
            }

            return true;
        }

        public ObjectAttribute GetAttributeByName(string AttributeName)
        {
            foreach (ObjectAttribute lAttribute in cObjectAttributes)
            {
                if (lAttribute.GetAttributeName() == AttributeName)
                {
                    return lAttribute;
                }
            }

            return null;
        }

        public void SetObjectAttributes(List<ObjectAttribute> lObjectAttributes)
        {
            cObjectAttributes = lObjectAttributes;
        }

        public List<ObjectAttribute> GetObjectAttributes()
        {
            return cObjectAttributes;
        }

        public ParserObject()
        {
            ParserPrimitiveType = ParserPrimitiveTypes.ObjectType;
        }

        public ParserObject(List<ObjectAttribute> lObjectAttributes)
        {
            ParserPrimitiveType = ParserPrimitiveTypes.ObjectType;
            SetObjectAttributes(lObjectAttributes);
        }
    }
}
