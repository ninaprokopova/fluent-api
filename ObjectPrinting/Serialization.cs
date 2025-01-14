﻿using System;

namespace ObjectPrinting
{
    public class Serialization<TOwner>
    {
        public String EditingPropertyInfoName;
        public PrintingConfig<TOwner> PrintingConfig; 
        public PrintingConfig<TOwner> SetSerialization(Func<object, string> func)
        {
            PrintingConfig.PropertySerialization.Add(EditingPropertyInfoName, func);
            return PrintingConfig;
        }
    }
}