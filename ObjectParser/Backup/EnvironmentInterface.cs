using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectParser
{
    enum AttributeType
    {
        Undefined,
        String,
        Integer,
        Regex,
        Evalution
    }

    class ObjectAttribute
    {
        string AttributeName;
        AttributeType cAttributeType= AttributeType.Undefined;

        public string GetAttributeName()
        {
            return AttributeName;
        }

        public void SetAttributeName(string lAttributeName)
        {
            AttributeName = lAttributeName;
        }

        public AttributeType GetAttributeType()
        {
            return cAttributeType;
        }

        public void SetAttribueType(AttributeType lAttributeType)
        {
            cAttributeType = lAttributeType;
        }

        public virtual bool ComapreAttributes(ObjectAttribute RemoteAttribute)
        {
            return false;
        }
    }

    class ObjectAttributeInt : ObjectAttribute
    {
        int AttributeValue;

        public override bool ComapreAttributes(ObjectAttribute RemoteAttribute)
        {
            if (RemoteAttribute.GetAttributeType() != AttributeType.Integer)
                return false;

            ObjectAttributeInt lRemoteAttribute = (ObjectAttributeInt)RemoteAttribute;

            if (lRemoteAttribute.GetAttributeValue() != AttributeValue)
                return false;

            return true;
        }

        public int GetAttributeValue()
        {
            return AttributeValue;
        }

        public void SetAttributeValue(int lAttributeValue)
        {
            AttributeValue = lAttributeValue;
        }

        public ObjectAttributeInt()
        {
            SetAttribueType(AttributeType.Integer);
        }

        public ObjectAttributeInt(string name, int value)
        {
            SetAttributeName(name);
            AttributeValue = value;
        }
    }

    class ObjectAttributeString : ObjectAttribute
    {
        string AttributeValue;

        public override bool ComapreAttributes(ObjectAttribute RemoteAttribute)
        {
            if (RemoteAttribute.GetAttributeType() != AttributeType.String)
                return false;

            ObjectAttributeString lRemoteAttribute = (ObjectAttributeString)RemoteAttribute;

            if (lRemoteAttribute.GetAttributeValue() != AttributeValue)
                return false;

            return true;
        }

        public string GetAttributeValue()
        {
            return AttributeValue;
        }

        public void SetAttributeValue(string lAttributeValue)
        {
            AttributeValue = lAttributeValue;
        }

        public ObjectAttributeString()
        {
            SetAttribueType(AttributeType.String);
        }

        public ObjectAttributeString( string lAttributeName, string lAttributeValue )
        {
            SetAttribueType(AttributeType.String);
            SetAttributeName(lAttributeName);
            SetAttributeValue(lAttributeValue);
        }
    }

    class ObjectAttributeRegex: ObjectAttribute
    {
        string Match;

        public override bool ComapreAttributes(ObjectAttribute RemoteAttribute)
        {
            if (RemoteAttribute == null)
                return false;

            if (RemoteAttribute.GetAttributeType() != AttributeType.String)
                return false;

            ObjectAttributeString lRemoteAttribute = (ObjectAttributeString)RemoteAttribute;

            if (lRemoteAttribute.GetAttributeValue() == null)
                return false;

            Match m = Regex.Match(lRemoteAttribute.GetAttributeValue(), Match);
            if (m.Success)
                return true;

            return false;
        }

        public string GetStringValue()
        {
            return Match;
        }

        public void SetAttributeValue( string lMatch )
        {
            Match= lMatch;
        }

        public ObjectAttributeRegex()
        {
            SetAttribueType(AttributeType.Regex);
        }

        public ObjectAttributeRegex( string Name, string Pattern )
        {
            SetAttribueType(AttributeType.Regex);
            SetAttributeName(Name);
            SetAttributeValue(Pattern);
        }
    }

    //Base class with prototipe functions
    class ObjectPrototipe
    {
        public virtual List<ObjectAttribute> GetObjectAttributes()
        {
            return null;
        }

        public virtual string GetObjectName()
        {
            return null;
        }

        public virtual ObjectAttribute GetAttributeByName(string AttributeName)
        {
            return null;
        }

        public virtual void SetAttribute(ObjectAttribute lObjectAttribute)
        {
            return;
        }

        public virtual void AddAttribute(ObjectAttribute lObjectAttribute)
        {
            return;
        }
    }

    //Representing real object
    class RealObject: ObjectPrototipe
    {
        string ObjectName="undefined";
        List<ObjectAttribute> Attributes = new List<ObjectAttribute>();

        public override List<ObjectAttribute> GetObjectAttributes()
        {
            return Attributes;
        }

        public override string GetObjectName()
        {
            return ObjectName;
        }

        public override ObjectAttribute GetAttributeByName( string AttributeName )
        {
            foreach( ObjectAttribute lAttribute in Attributes )
            {
                if (lAttribute.GetAttributeName() == AttributeName)
                {
                    return lAttribute;
                }
            }

            return null;
        }

        public override void AddAttribute(ObjectAttribute lObjectAttribute)
        {
            Attributes.Add(lObjectAttribute);
        }
    }

    class ObjectInterfacePrototipe
    {
        public virtual ObjectPrototipe GetFirstObject()
        {
            return null;
        }

        public virtual ObjectPrototipe GetNextObject()
        {
            return null;
        }

        public virtual ObjectPrototipe GetCurrentObject()
        {
            return null;
        }

        public virtual ObjectPrototipe GetPreviousObject()
        {
            return null;
        }

        public virtual ObjectPrototipe GetOffsetObject(int offset)
        {
            return null;
        }

        public virtual int GetOffset()
        {
            return -1;
        }

        public virtual void SetOffset( int offset )
        {
            return;
        }

        public virtual bool MoveByOffset(int offset)
        {
            return false;
        }

        //Return objectinterface to subobjects
        public virtual ObjectInterfacePrototipe GetSubObjects()
        {
            return null;
        }
    }
}
