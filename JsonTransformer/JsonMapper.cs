﻿using Newtonsoft.Json.Linq;
using ObjectToObjectMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonTransformer
{
    public class JsonMapper : ITransform
    {
        public JToken Transform(JToken jObject, JToken targetObjectMapper)
        {
            var targetObject = new  JObject();
            var originalObject = jObject;
            
            foreach (var child in targetObjectMapper.Children())
            {
                JProperty prop = child as JProperty;
                if (prop?.Value.Type == JTokenType.Object)
                {
                    targetObject[prop.Name] = Transform(jObject, prop.Value);
                }
                else 
                {
                    var mapperObject = originalObject;
                    if (prop != null)
                    {
                        string propValue = Convert.ToString(prop.Value);
                        var transformers = GetListOfResolvers(Convert.ToString(prop.Value));
                        
                        foreach (var transformer in transformers)
                        {
                            if (!string.IsNullOrWhiteSpace(transformer.Item1))
                                mapperObject = transformer.Item2.ProcessJson(transformer.Item1, mapperObject);
                        }
                    }
                        targetObject[prop.Name] = DecorateValue(prop, mapperObject);
                }
            }

            return targetObject;
        }

        private JToken DecorateValue(JProperty property, JToken tokenValue)
        {
            if (property != null)
            {
                switch (property.Value.Type)
                {
                    case JTokenType.Boolean: 
                    case JTokenType.Integer: return property.Value;
                    default:
                        return tokenValue;
                }
            }

            return tokenValue;
        }

        private List<Tuple<string, Resolver>> GetListOfResolvers(string mappedJsonValues)
        {
            char[] keys = { Resolver.DotOperator, Resolver.OpeningBrace, Resolver.OpeningRectBracket};
            var list = new List<Tuple<string, Resolver>>();
            Dictionary<char, char> dic = new Dictionary<char, char>();
            dic.Add(Resolver.OpeningBrace, Resolver.ClosingBrace);
            dic.Add(Resolver.OpeningRectBracket, Resolver.ClosingRectBracket);

            while(mappedJsonValues.Any( x => keys.Contains(x)))
            {
                var firstIndexofAny = mappedJsonValues.IndexOfAny(keys);
                var charAtMatchingIndex = mappedJsonValues[firstIndexofAny];
                if(list.Count() == 0 && firstIndexofAny > 0)
                {
                    list.Add(new Tuple<string, Resolver>(mappedJsonValues.Substring(0,firstIndexofAny), new DefaultResolver()));
                    mappedJsonValues =  mappedJsonValues.Remove(0, firstIndexofAny);
                }
                else
                {
                    if(charAtMatchingIndex != Resolver.DotOperator)
                    {
                        var wholevalue = mappedJsonValues.Substring(firstIndexofAny+ 1, mappedJsonValues.IndexOf(dic[charAtMatchingIndex]) -1);
                        list.Add(new Tuple<string, Resolver>(wholevalue, Resolver.ResolveMapper(charAtMatchingIndex.ToString())));
                        mappedJsonValues = mappedJsonValues.Remove(firstIndexofAny, mappedJsonValues.IndexOf(dic[charAtMatchingIndex])+ 1);
                    }
                    else
                    {
                        if (mappedJsonValues.IndexOfAny(keys, firstIndexofAny + 1) == 0) continue;
                        var val = mappedJsonValues.Substring(firstIndexofAny + 1, mappedJsonValues.IndexOfAny(keys, firstIndexofAny + 1) -1);
                        list.Add(new Tuple<string, Resolver>(val, Resolver.ResolveMapper(charAtMatchingIndex.ToString())));
                        mappedJsonValues =mappedJsonValues.Remove(firstIndexofAny, mappedJsonValues.IndexOfAny(keys, firstIndexofAny + 1));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(mappedJsonValues))
                list.Add(new Tuple<string, Resolver>(mappedJsonValues, new DirectValuesResolver()));

            return list;
        }
        
    }
}

