using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectParser
{

    class ObjectAttribute
    {
        private string cAttributeName;
        protected dynamic cValue;

        public string AttributeName { get { return cAttributeName; } set { cAttributeName = value; } }
        public dynamic Value { get { return cValue; } set { cValue = value; } }

        public virtual bool ComapreAttributes(ObjectAttribute RemoteAttribute)
        { 
            return false; 
        }

        public static bool operator ==(ObjectAttribute attribute1, ObjectAttribute attribute2)
        {
            return attribute1.ComapreAttributes(attribute2)? true: false;
        }

        public static bool operator !=(ObjectAttribute attribute1, ObjectAttribute attribute2)
        {
            return attribute1.ComapreAttributes(attribute2) ? false : true;
        }
    }

    class ObjectAttributeBool : ObjectAttribute
    {
        public override bool ComapreAttributes(ObjectAttribute lRemoteAttribute)
        {
            if (lRemoteAttribute.GetType() != typeof(ObjectAttributeBool))
                return false;

            if (lRemoteAttribute.Value != Value)
                return false;

            return true;
        }

        public ObjectAttributeBool(string name, bool lValue)
        {
            AttributeName = name;
            Value = lValue;
        }
    }

    class ObjectAttributeInt : ObjectAttribute
    {
        public override bool ComapreAttributes(ObjectAttribute lRemoteAttribute)
        {
            if (lRemoteAttribute.GetType() != typeof(ObjectAttributeInt))
                return false;

            if (lRemoteAttribute.Value != Value)
                return false;

            return true;
        }

        public ObjectAttributeInt(string name, int lValue)
        {
            AttributeName=name;
            Value = lValue;
        }
    }

    class ObjectAttributeString : ObjectAttribute
    {
        public override bool ComapreAttributes(ObjectAttribute lRemoteAttribute)
        {
            if (lRemoteAttribute.GetType() != typeof(ObjectAttributeString))
                return false;

            if (lRemoteAttribute.Value != Value)
                return false;

            return true;
        }

        public ObjectAttributeString( string lAttributeName, string lAttributeValue )
        {
            AttributeName= lAttributeName;
            Value= lAttributeValue;
        }

        public ObjectAttributeString(){}
    }

    class ObjectAttributeRegex: ObjectAttribute
    {
        public override bool ComapreAttributes(ObjectAttribute lRemoteAttribute)
        {
            if (lRemoteAttribute == null)
                return false;

            if (lRemoteAttribute.GetType() != typeof(ObjectAttributeString))
                return false;

            if (lRemoteAttribute.Value == null)
                return false;

            if (Value == "")//Regex makes this wrong
                if (lRemoteAttribute.Value != Value)
                    return false;

            Match m = Regex.Match(lRemoteAttribute.Value, Value);
            if (m.Success)
                return true;

            return false;
        }

        public ObjectAttributeRegex( string Name, string Pattern )
        {
            AttributeName= Name;
            Value= Pattern;
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
                if (lAttribute.AttributeName == AttributeName)
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

        public virtual int GetObjectCount()
        {
            return 0;
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
