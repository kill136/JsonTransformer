﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JsonTransformer
{
    internal class JsonArrayFilterResolver : Resolver
    {
        internal JsonArrayFilterResolver(JProperty property) : base(property) { }
        public override JToken ProcessJson(string jTokenValue, JToken inputObject)
        {
            string[] conditions = jTokenValue.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<JToken> tokens = (inputObject as JArray);

            foreach(var condition in conditions)
            {
                string[] splitKeyValues = condition.Split(new char[] { '=' });
                tokens = tokens.Where(x => x.Value<string>(splitKeyValues[0].Trim()) == splitKeyValues[1].Trim().Replace("'", string.Empty));
            }

            var jArray = new JArray(tokens);
            return jArray.Count() == 1 ? jArray[0] : jArray;

        }
    }
}
