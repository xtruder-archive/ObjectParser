using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectParser
{
    class RelationFunctions
    {
        Dictionary<string, RfnEntry> cFunctionEntries = new Dictionary<string, RfnEntry>();

        public RelationFunction CreateRelationFunctionByName( string RelationFunctionName, ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionInput )
        {
            if (cFunctionEntries[RelationFunctionName] != null)
            {
                //Creates a new instance of specifficed class.
                return (RelationFunction)Activator.CreateInstance(cFunctionEntries[RelationFunctionName].AssociatedType, new object[] { lParserObjectRelation, lFunctionInput });
            }

            return null;
        }

        public RelationFunctions()
        {
            //At the startup we get all needed parameters and create a new list.
            List<Type> cTypesWithRfnAttribute = GetAllRelationFunctions();
            foreach (Type FunctionType in cTypesWithRfnAttribute)
            {
                RelationFunctionName lRfName= (RelationFunctionName)Attribute.GetCustomAttribute(FunctionType, typeof(RelationFunctionName));
                List<RelationFunctionParameter> lFunctionParameters= new List<RelationFunctionParameter>
                    (from n in Attribute.GetCustomAttributes(FunctionType)
                    where n!=null && n.GetType()==typeof(RelationFunctionParameter)
                    select n as RelationFunctionParameter);

                cFunctionEntries.Add(lRfName.FunctionName, new RfnEntry(FunctionType,lFunctionParameters));
            }
        }

        private List<Type> GetAllRelationFunctions()
        {
            //Hehe some kung-fu code for getting all types with speciffic attribute.
            IEnumerable<Type> typesWithRfnAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(RelationFunctionName), true)
                where attributes != null && attributes.Length > 0
                select t;

            return new List<Type>(typesWithRfnAttribute);
        }

        class RfnEntry
        {
            public Type AssociatedType;
            public List<RelationFunctionParameter> FunctionParameters;

            public RfnEntry(Type lAssociatedType, List<RelationFunctionParameter> lFunctionParameters)
            {
                AssociatedType = lAssociatedType;
                FunctionParameters = lFunctionParameters;
            }
        }
    }

    /// <summary>
    /// RelationFunctionName attribute which must be applied to every RelationFunction.
    /// ObjectParses used it for getting all RelationFunctions.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class)]
    public class RelationFunctionName : System.Attribute
    {
        /// <summary>
        /// Name of relation function
        /// </summary>
        public string FunctionName;

        /// <summary>
        /// RelationFunctionName attribute constructor.
        /// </summary>
        /// <param name="lRelationFunctionName">Name of the relation function.</param>
        public RelationFunctionName(string lRelationFunctionName)
        {
            FunctionName = lRelationFunctionName;
        }
    }

    /// <summary>
    /// RelationFunctionParameter attribute for function input parameters,
    /// so that parser knows which are input parameters.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class)]
    public class RelationFunctionParameter : System.Attribute
    {
        /// <summary>
        /// Name of parameter.
        /// </summary>
        string ParameterName;
        /// <summary>
        /// Type of the parameter.
        /// </summary>
        Type ParameterType;

        /// <summary>
        /// RelationFunctionParameter attribute constructor.
        /// </summary>
        /// <param name="lParameterName">Name of the input parameter.</param>
        /// <param name="lParameterType">Type of the input parameter.</param>
        public RelationFunctionParameter( string lParameterName, Type lParameterType )
        {
            ParameterName = lParameterName;
            ParameterType = lParameterType;
        }
    }

    /// <summary>
    /// Or relation function. At least one of SubParsePrimitives must return true while parsing.
    /// </summary>
    [RelationFunctionName("or")]
    [RelationFunctionParameter("ParseCount",typeof(ObjectAttributeEvaluation))]
    class RelationFunctionOR : RelationFunction
    {
        /// <summary>
        /// Relation function constructor.
        /// </summary>
        /// <param name="lParserObjectRelation">Parent ObjectRelation.</param>
        /// <param name="lFunctionParameters">Input parameters for relation function.</param>
        public RelationFunctionOR(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        /// <summary>
        /// Checks relations.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="NextElement">Next element we are going to parse.</param>
        /// <param name="CallCalback">Do we have to call callback?</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            ObjectAttributeEvaluation Evaluation = GetEvaluation();

            bool result = false;
            List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
            for (int x = 0; x < SubPrimitives.Count; x++)
            {
                FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], CallCalback);
                if (ret["RetVal"].Value)
                {
                    result = true;
                    Evaluation.Evaluate((ObjectAttributeEvaluation)ret["ParseCount"]);

                    break;//Proper logic is to break if parser primitives relations are correct,
                    //other logic is to select parser primitive where ParseCount is the biggest.
                    //we use proper logic.
                }
            }

            if (result)
                return new FunctionIO(Evaluation, RetVal.True);

            return new FunctionIO(RetVal.False);
        }
    }

    /// <summary>
    /// AND relation function. All of the SubParserPrimitives must return true.
    /// </summary>
    [RelationFunctionName("and")]
    [RelationFunctionParameter("ParseCount", typeof(ObjectAttributeEvaluation))]
    class RelationFunctionAND : RelationFunction
    {
        /// <summary>
        /// Relation function constructor.
        /// </summary>
        /// <param name="lParserObjectRelation">Parent ObjectRelation.</param>
        /// <param name="lFunctionParameters">Input parameters for relation function.</param>
        public RelationFunctionAND(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        /// <summary>
        /// Checks relations.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="NextElement">Next element we are going to parse.</param>
        /// <param name="CallCalback">Do we have to call callback?</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            ObjectAttributeEvaluation Evaluation = GetEvaluation();

            int Positives = 0;
            List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
            for (int x = 0; x < SubPrimitives.Count; x++)
            {
                FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], CallCalback);
                if (ret["RetVal"].Value)
                {
                    Positives++;
                    if (!Evaluation.Evaluate((ObjectAttributeEvaluation)ret["ParseCount"]))
                        return new FunctionIO(RetVal.False);
                }
            }

            if (Positives == SubPrimitives.Count)
                return new FunctionIO(Evaluation, RetVal.True);

            return new FunctionIO(RetVal.False);
        }
    }

    /// <summary>
    /// CROSS relation function. Scans all across and throught all depths of ObjectInterface looking for SubParserPrimitives.
    /// </summary>
    [RelationFunctionName("cross")]
    [RelationFunctionParameter("ParseCount", typeof(ObjectAttributeEvaluation))]
    [RelationFunctionParameter("MaxDepth", typeof(ObjectAttributeInt))]
    [RelationFunctionParameter("MinOccurencies", typeof(ObjectAttributeInt))]
    [RelationFunctionParameter("MaxOccurencies", typeof(ObjectAttributeInt))]
    class RelationFunctionCROSS : RelationFunction
    {
        /// <summary>
        /// Max depth we can cross scan.
        /// </summary>
        int MaxDepth = int.MaxValue;

        /// <summary>
        /// Relation function constructor.
        /// </summary>
        /// <param name="lParserObjectRelation">Parent ObjectRelation.</param>
        /// <param name="lFunctionParameters">Input parameters for relation function.</param>
        public RelationFunctionCROSS(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        /// <summary>
        /// Checks relations.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="NextElement">Next element we are going to parse.</param>
        /// <param name="CallCalback">Do we have to call callback?</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            ObjectAttribute lMaxDepth = null;
            if (cFunctionParameters != null && (lMaxDepth = cFunctionParameters["MaxDepth"]) != null)
                MaxDepth = lMaxDepth.Value;

            FunctionIO ret = CheckDeepRelations(ref lObjectInterface, CallCalback, 1);
            if (!ret["RetVal"].Value)
                return new FunctionIO(RetVal.False);

            int occurencies = ret["Occurencies"].Value;
            ObjectAttribute MinOccurencies;
            ObjectAttribute MaxOccurencies;
            if (cFunctionParameters != null && (MinOccurencies = cFunctionParameters["MinOccurencies"]) != null && (MaxOccurencies = cFunctionParameters["MaxOccurencies"]) != null)
                if (occurencies < MinOccurencies.Value || occurencies > MaxOccurencies.Value)
                    return new FunctionIO(RetVal.False);

            return ret;
        }

        /// <summary>
        /// Recursively check all depths.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="CallCallback">Do we have to call callback?</param>
        /// <param name="depth">Current depth.</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public FunctionIO CheckDeepRelations(ref ObjectInterfacePrototipe lObjectInterface, bool CallCallback, int depth)
        {
            ObjectAttributeEvaluation Evaluation = new ObjectAttributeEvaluation();
            Evaluation.AttributeName = "ParseCount";

            if (depth > MaxDepth)
                return new FunctionIO(RetVal.False);

            bool More = true;
            int TotalMove = 0;
            int OccurenciesFound = 0;
            int StartOffset = lObjectInterface.GetOffset();
            bool eof = false;

            List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
            while (More)
            {
                int CurrentMove = 0;
                int Positives = 0;

                ObjectInterfacePrototipe tInterface = lObjectInterface.GetSubObjects();
                FunctionIO ret2 = null;
                if (tInterface != null)
                {
                    tInterface.GetFirstObject();
                    if (tInterface.GetCurrentObject() != null)
                    {
                        ret2 = CheckDeepRelations(ref tInterface, CallCallback, depth + 1);
                        if (ret2["RetVal"].Value)
                            OccurenciesFound += ret2["Occurencies"].Value;
                    }
                }

                for (int x = 0; x < SubPrimitives.Count; x++)
                {
                    CurrentMove = 0;

                    FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], false);
                    if (ret["RetVal"].Value)
                    {
                        CurrentMove = ((ObjectAttributeEvaluation)ret["ParseCount"]).Value;
                        Positives++;
                    }
                    else
                        break;//Stop execution if we fail.
                    eof = lObjectInterface.MoveByOffset(CurrentMove);

                    if (eof)
                        break;
                }

                //if (eof)
                //    break;

                lObjectInterface.SetOffset(StartOffset);//We go back to start offset
                eof = lObjectInterface.MoveByOffset(TotalMove);//and move for one step and continue checking from there
                if (Positives == SubPrimitives.Count)
                {
                    OccurenciesFound++;//We find a new occurency and incrise counter
                    //We recall our callbacks
                    if (CallCallback)
                    {
                        for (int x = 0; x < SubPrimitives.Count; x++)
                        {
                            CurrentMove = 0;

                            FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], CallCallback);
                            CurrentMove = ((ObjectAttributeEvaluation)ret["ParseCount"]).Value;
                            eof = lObjectInterface.MoveByOffset(CurrentMove);
                        }
                    }
                }

                TotalMove++;//We incrise move
                lObjectInterface.SetOffset(StartOffset);//We go back to start offset
                eof = lObjectInterface.MoveByOffset(TotalMove);//and move for one step and continue checking from there

                if (eof)
                    break;
            }

            if (OccurenciesFound == 0)
                return new FunctionIO(RetVal.False);

            Evaluation.Evaluate(TotalMove);

            lObjectInterface.SetOffset(StartOffset);

            ObjectAttributeInt ParamOccurrenciesFound = new ObjectAttributeInt("Occurencies", OccurenciesFound);

            return new FunctionIO(ParamOccurrenciesFound, Evaluation, RetVal.False);
        }
    }

    /// <summary>
    /// MORE relation function. Evaluates more ParserPrimitives till next ParserPrimitive is found.
    /// We can control how many objects do we parse with Evaluation input parameters.
    /// </summary>
    [RelationFunctionName("more")]
    [RelationFunctionParameter("ParseCount", typeof(ObjectAttributeEvaluation))]
    class RelationFunctionMORE : RelationFunction
    {
        /// <summary>
        /// Relation function constructor.
        /// </summary>
        /// <param name="lParserObjectRelation">Parent ObjectRelation.</param>
        /// <param name="lFunctionParameters">Input parameters for relation function.</param>
        public RelationFunctionMORE(ref ParserObjectRelation lParserObjectRelation, FunctionIO lFunctionParameters) : base(ref lParserObjectRelation, lFunctionParameters) { }

        /// <summary>
        /// Checks relations.
        /// </summary>
        /// <param name="lObjectInterface">ObjectInterface.</param>
        /// <param name="NextElement">Next element we are going to parse.</param>
        /// <param name="CallCalback">Do we have to call callback?</param>
        /// <returns>FunctionIO with return parameters like ret value and parse count.</returns>
        public override FunctionIO CheckRelations(ref ObjectInterfacePrototipe lObjectInterface, ParserPrimitives NextElement, bool CallCalback)
        {
            ObjectAttributeEvaluation Evaluation = GetEvaluation();

            bool More = true;
            int TotalMove = 0;
            int StartOffset = lObjectInterface.GetOffset();
            bool eof = false;

            while (More)
            {
                int Positives = 0;
                int CurrentMove = 0;

                if (NextElement != null)
                {
                    FunctionIO ret = NextElement.CheckRelations(ref lObjectInterface, NextElement.GetNextPrimitive(), false);
                    if (ret["RetVal"].Value)
                        break;
                }

                int RecallOffset = lObjectInterface.GetOffset();
                List<ParserPrimitives> SubPrimitives = cParserObjectRelation.GetSubParserPrimitives();
                for (int x = 0; x < SubPrimitives.Count; x++)
                {
                    CurrentMove = 0;

                    FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], false);
                    if (ret["RetVal"].Value)
                    {
                        CurrentMove = ((ObjectAttributeEvaluation)ret["ParseCount"]).Value;
                        Positives++;
                    }
                    else//End execution if we fail
                        break;
                    eof = lObjectInterface.MoveByOffset(CurrentMove);

                    TotalMove += CurrentMove;

                    if (eof)
                        break;
                }

                if (CallCalback && Positives == SubPrimitives.Count)
                {
                    lObjectInterface.SetOffset(RecallOffset);
                    for (int x = 0; x < SubPrimitives.Count; x++)
                    {
                        CurrentMove = 0;

                        FunctionIO ret = SubPrimitives[x].CheckRelations(ref lObjectInterface, x + 1 == SubPrimitives.Count ? null : SubPrimitives[x + 1], CallCalback);
                        CurrentMove = ((ObjectAttributeEvaluation)ret["ParseCount"]).Value;

                        eof = lObjectInterface.MoveByOffset(CurrentMove);
                    }
                }

                if (eof || Positives != SubPrimitives.Count)
                {
                    break;
                }
            }

            if (!Evaluation.Evaluate(TotalMove))
                return new FunctionIO(RetVal.False);

            lObjectInterface.SetOffset(StartOffset);

            return new FunctionIO(Evaluation, RetVal.True);
        }
    }
}
