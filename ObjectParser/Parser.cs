using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Dynamic;

namespace ObjectParser
{
    /// <summary>
    /// Calss for handling input and output function parameters for relation functions and parser objects.
    /// </summary>
    class FunctionIO
    {
        /// <summary>
        /// List of parameters.
        /// </summary>
        List<ObjectAttribute> Parameters= new List<ObjectAttribute>();

        /// <summary>
        /// Adds function parameter.
        /// </summary>
        /// <param name="lParameter">Parameter as any object attribute.</param>
        public void AddFunctionParameter(ObjectAttribute lParameter)
        {
            Parameters.Add(lParameter);
        }

        /// <summary>
        /// Gets function parameter by its name.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <returns>Function parameter.</returns>
        public ObjectAttribute GetFunctionParameterByName(string name)
        {
            return (from n in Parameters where n.AttributeName == name select n).DefaultIfEmpty(null).First();
        }

        /// <summary>
        /// Gets ObjectAttribute by passing index.
        /// </summary>
        /// <param name="name">Name of the parameter.</param>
        /// <returns>Function parameter.</returns>
        public ObjectAttribute this[string name]
        {
            get
            {
                return GetFunctionParameterByName(name);
            }
        }

        /// <summary>
        /// Operator to add ObjectAttribute to FunctionIO.
        /// </summary>
        /// <param name="me">FunctionIO to which we want to add ObjectAttribute.</param>
        /// <param name="lAttribute">ObjectAttribute we want to add.</param>
        /// <returns>The same FunctionIO as passed to function.</returns>
        public static FunctionIO operator +(FunctionIO me, ObjectAttribute lAttribute)
        {
            me.AddFunctionParameter( lAttribute );
            return me;
        }

        /// <summary>
        /// FunctionIO constructor.
        /// </summary>
        public FunctionIO() { }

        /// <summary>
        /// FunctionIO constructor.
        /// </summary>
        /// <param name="Attributes">ObjectAttributes as params we want to add.</param>
        public FunctionIO(params ObjectAttribute[] Attributes)
        {
            Parameters.AddRange(Attributes);
        }
    }

    /// <summary>
    /// Type of the evaluation.
    /// Are we evaluating for biggest or are we looking for smallest value.
    /// </summary>
    enum EvaluationMode
    {
        /// <summary>
        /// We are evaluating the biggest value.
        /// </summary>
        Max,
        /// <summary>
        /// We are evaluating the smallest value.
        /// </summary>
        Min
    }
    
    /// <summary>
    /// ObjectAttribute for evaluating the biggest or the smallest value.
    /// Primarily used for selecting how mouch objects do w have to parse.
    /// </summary>
    class ObjectAttributeEvaluation: ObjectAttribute
    {
        /// <summary>
        /// Minimal value that we accept.
        /// </summary>
        int MinValue = 0;
        /// <summary>
        /// Max value that we accept.
        /// </summary>
        int MaxValue = int.MaxValue;

        /// <summary>
        /// Default mode is max mode where are we evaluating the biggest value.
        /// </summary>
        EvaluationMode cEvaluationMode = EvaluationMode.Max;

        /// <summary>
        /// Evaluates new value
        /// </summary>
        /// <param name="Value">Value</param>
        /// <returns>RetTrue if evaluation was a success, false if not.</returns>
        public bool Evaluate( int lValue )
        {
            if (lValue > MinValue && lValue < MaxValue)
            {
                if (cEvaluationMode == EvaluationMode.Max)
                {
                    if (lValue > Value)
                    {
                        Value = lValue;
                    }
                }
                if (cEvaluationMode == EvaluationMode.Min)
                {
                    if (lValue < Value)
                    {
                        Value = lValue;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Evaluate value from other evaluation.
        /// </summary>
        /// <param name="Evaluation">Another evaluation.</param>
        /// <returns>RetTrue if evaluation was a success, false if not.</returns>
        public bool Evaluate(ObjectAttributeEvaluation Evaluation)
        {
            return Evaluate(Evaluation.Value);
        }

        /// <summary>
        /// ObjectAttributeEvaluation constructor.
        /// </summary>
        /// <param name="lEvaluationMode">Are we evaluationg biggest or smallest value?</param>
        /// <param name="Min">Minimal accepted value. Default 0.</param>
        /// <param name="Max">Maximal accepted value. Default int.MaxValue.</param>
        public ObjectAttributeEvaluation(EvaluationMode lEvaluationMode, int Min=0, int Max=int.MaxValue): this()
        {
            cEvaluationMode = lEvaluationMode;
            if (Min != Max)
            {
                MinValue = Min;
                MaxValue = Max;
            }
        }

        /// <summary>
        /// Object attribute evaluation constructor.
        /// </summary>
        public ObjectAttributeEvaluation()
        {
            Value = 1;
        }
    }

    /// <summary>
    /// Used for creating return value while parsing.
    /// </summary>
    class RetVal
    {
        /// <summary>
        /// Attribute returning ObjectAttribute with name of "RetVal" and value of true.
        /// </summary>
        public static ObjectAttributeBool True
        {
            get { return new ObjectAttributeBool("RetVal", true); }
        }

        /// <summary>
        /// Attribute returning ObjectAttribute with name of "RetVal" and value of true.
        /// </summary>
        public static ObjectAttributeBool False
        {
            get { return new ObjectAttributeBool("RetVal", false); }
        }
    }

    /// <summary>
    /// Function to check relations betwene objects.
    /// </summary>
    class RelationFunction
    {
        /// <summary>
        /// Parent ParserObjectRelation primarily for getting sub parser primitives.
        /// </summary>
        protected ParserObjectRelation cParserObjectRelation = null;
        /// <summary>
        /// Input parameters of a function.
        /// </summary>
        protected FunctionIO cFunctionParameters;

        /// <summary>
        /// Gets evaluation based on if ParseCount function parameter is set or creates new ObjectAttrubteEvaluation.
        /// </summary>
        /// <returns></returns>
        protected ObjectAttributeEvaluation GetEvaluation()
        {
            ObjectAttributeEvaluation Evaluation = null;
            ObjectAttribute lParseCount;

            if (cFunctionParameters == null || (lParseCount=cFunctionParameters["ParseCount"]) == null)
            {
                Evaluation = new ObjectAttributeEvaluation();
                Evaluation.AttributeName= "ParseCount";
            }
            else
            {
                Evaluation = (ObjectAttributeEvaluation)lParseCount;
            }

            return Evaluation;
        }

        /// <summary>
        /// Checks relations.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="NextElement">Next element we are going to parse.</param>
        /// <param name="CallCalback">Do we have to call callback?</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public virtual FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            return null;
        }

        /// <summary>
        /// Relation function constructor.
        /// </summary>
        /// <param name="lParserObjectRelation">Parent ObjectRelation.</param>
        /// <param name="lFunctionParameters">Input parameters for relation function.</param>
        public RelationFunction(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters)
        {
            cParserObjectRelation = lParserObjectRelation;
            cFunctionParameters = lFunctionParameters;
        }
    }

    delegate bool Callback ( ObjectInterfacePrototipe lObjectInterface );

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
                AddSubParserPrimitive(lParserPrimitive);
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
            ObjectAttribute RetVal=ret["RetVal"];
            if (RetVal.Value)
            {
                if (cCallback != null && CallCalback)
                {
                    if (!cCallback(lObjectInterface))
                    {
                        RetVal.Value = false;
                    }
                }
            }

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
            ObjectAttributeEvaluation Evaluation =  new ObjectAttributeEvaluation();
            Evaluation.AttributeName="ParseCount";
            bool negation = GetAttributeByName("negation")==null? false: true; 

            bool Result = CheckCurrentObject(ref lObjectInterface);
            if (negation)
            {
                if (Result)
                    return new FunctionIO(RetVal.False);
            }
            else
            {
                if (!Result)
                    return new FunctionIO(RetVal.False);
            }

            if (cParserPrimitives.Count == 0)
            {
                if (cCallback != null && CallCalback)
                    if (!cCallback(lObjectInterface))
                        return new FunctionIO(Evaluation, RetVal.False);

                return new FunctionIO(Evaluation, RetVal.True);
            }

            //Here we preset next primitives
            for (int x = 0; x < cParserPrimitives.Count-1; x++)
                cParserPrimitives[x].SetNextPrimitive(cParserPrimitives[x + 1]);

            ObjectInterfacePrototipe SubObjects= lObjectInterface.GetSubObjects();
            if (SubObjects.GetFirstObject() == null)
                return new FunctionIO(Evaluation, RetVal.True);

            ObjectAttributeEvaluation ParseCountEvaluation = new ObjectAttributeEvaluation( EvaluationMode.Max, 1, int.MaxValue);
            for( int x=0; x< cParserPrimitives.Count; x++ )
            {
                ParserPrimitives tPrimitives;
                if (cParserPrimitives.Count == x + 1)
                    tPrimitives = null;
                else
                    tPrimitives = cParserPrimitives[x + 1];
                FunctionIO ret = cParserPrimitives[x].CheckRelations(ref SubObjects, tPrimitives, CallCalback);
                bool tResult= ret["RetVal"].Value;

                if (!negation)
                    if (!tResult)
                        return new FunctionIO(RetVal.False);
                else
                    if (tResult)
                        return new FunctionIO(RetVal.False);

                SubObjects.GetOffsetObject(ret["ParseCount"].Value);
            }

            if (cCallback != null && CallCalback)
                cCallback(lObjectInterface);

            return new FunctionIO(Evaluation, RetVal.True);
        }

        //Singleton check
        bool CheckCurrentObject(ref ObjectInterfacePrototipe lObjectInterface)
        {
            ObjectPrototipe lObjectPrototipe= lObjectInterface.GetCurrentObject();

            if (GetAttributeByName("name") != null)
            {
                ObjectAttributeString lObjectAttributeString = new ObjectAttributeString("name", lObjectPrototipe.GetObjectName());
                if (GetAttributeByName("name")!=lObjectAttributeString)
                    return false;
            }

            foreach (ObjectAttribute lObjectAttribute in (from n in cObjectAttributes where n.AttributeName!="name" && n.AttributeName!="negation" select n))
            {
                if(lObjectAttribute!=lObjectPrototipe.GetAttributeByName(lObjectAttribute.AttributeName))
                    return false;
            }

            return true;
        }

        public void SetObjectAttributes(List<ObjectAttribute> lObjectAttributes)
        {
            cObjectAttributes = lObjectAttributes;
        }

        public List<ObjectAttribute> GetObjectAttributes()
        {
            return cObjectAttributes;
        }

        public ObjectAttribute GetAttributeByName(string name)
        {
            return (from n in cObjectAttributes where n.AttributeName == "negation" select n).DefaultIfEmpty(null).FirstOrDefault();
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
