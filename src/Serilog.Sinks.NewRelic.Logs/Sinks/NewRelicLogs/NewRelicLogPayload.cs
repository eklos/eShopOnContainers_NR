using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Sinks.NewRelic.Logs.Sinks.NewRelicLogs;
using System;
using System.Collections.Generic;

namespace Serilog.Sinks.NewRelic.Logs
{
    public class NewRelicLogPayload
    {
        public NewRelicLogPayload() { }

        public NewRelicLogPayload(string applicationName)
        {
            this.Common.Attributes.Add("application", applicationName);
        }

        [JsonProperty("common")]
        public NewRelicLogCommon Common { get; set; } = new NewRelicLogCommon();

        [JsonProperty("logs")]
        public IList<NewRelicLogItem> Logs { get; set; } = new List<NewRelicLogItem>();
    }

    public class NewRelicLogCommon
    {
        [JsonProperty("attributes")]
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }

    public class NewRelicLogItem
    {
        private const string NewRelicLinkingMetadata = "newrelic.linkingmetadata";

        public NewRelicLogItem() { }

        public NewRelicLogItem(LogEvent logEvent, IFormatProvider formatProvider, bool logsInContext = false)
        {
            //Console.WriteLine("{0} ### in NewRelicLogItem .... logsInContext:{1}", DateTime.Now.ToLocalTime().ToString(), logsInContext);

            try {
                if (logsInContext) {
                    global::NewRelic.Api.Agent.IAgent agent = global::NewRelic.Api.Agent.NewRelic.GetAgent();                    
                    var linkingMetadata = agent.GetLinkingMetadata();

                    //Console.WriteLine("{0} *** linkingMetadata... message = {1}",  DateTime.Now.ToLocalTime().ToString(), logEvent.RenderMessage(formatProvider));
                    //foreach (KeyValuePair<string, string> kvp in linkingMetadata)
                    //{
                    //    Console.WriteLine("{0} *** Key = {1}, Value = {2}", DateTime.Now.ToLocalTime().ToString(), kvp.Key, kvp.Value);
                    //}
 
                    var traceId = "null";
                    var spanId = "null";
                    var entityName = "null";
                    var entityType = "null";
                    foreach (KeyValuePair<string, string> kvp in linkingMetadata)
                    {
                        if (kvp.Key == "trace.id") traceId = kvp.Value;
                        else if (kvp.Key == "span.id") spanId = kvp.Value;
                        else if (kvp.Key == "entity.name") entityName = kvp.Value;
                        else if (kvp.Key == "entity.type") entityType = kvp.Value;
                        //Console.WriteLine("{0} @1@ Key = {1}, Value = {2}", DateTime.Now.ToLocalTime().ToString(), kvp.Key, kvp.Value);
                    }
                    Console.WriteLine("{0} *** linkingMetadata... message = {1}\n     entity.name = {2}({3}) trace.id = {4} span.id = {5}", 
                        DateTime.Now.ToLocalTime().ToString(), logEvent.RenderMessage(formatProvider), entityName, entityType, traceId, spanId);

                }
            }
            catch (Exception ex) {
                Console.WriteLine("*** Exception caught... {0)", ex.ToString());
            }
           
            this.Timestamp = logEvent.Timestamp.UtcDateTime.ToUnixTimestamp();
            this.Message = logEvent.RenderMessage(formatProvider);
            this.Attributes.Add("klos-logsInContext", logsInContext);
            this.Attributes.Add("level", logEvent.Level.ToString());
            this.Attributes.Add("stack_trace", logEvent.Exception?.StackTrace ?? "");
            if (logEvent.Exception != null)
            {
                this.Attributes.Add("exception", logEvent.Exception.ToString() ?? "");
            }

            foreach (var property in logEvent.Properties)
            {
                this.AddProperty(property.Key, property.Value);
            }
        }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("attributes")]
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        private void AddProperty(string key, LogEventPropertyValue value)
        {
            try {
                if (key.Equals(NewRelicLinkingMetadata, StringComparison.InvariantCultureIgnoreCase))
                {
                    // unroll new relic distributed trace attributes
                    if (value is DictionaryValue newRelicProperties)
                    {
                        var traceId = "";
                        var spanId = "";
                        var entityName = "";
                        var entityType = "";

                        foreach (var property in newRelicProperties.Elements)
                        {
                            this.Attributes.Add(
                                NewRelicPropertyFormatter.Simplify(property.Key).ToString(),
                                NewRelicPropertyFormatter.Simplify(property.Value));

                            //Console.WriteLine("{0} *** AddProperty -> key: {1} , value: {2}",
                            //    DateTime.Now.ToLocalTime().ToString(),  
                            //    NewRelicPropertyFormatter.Simplify(property.Key).ToString(),
                            //    NewRelicPropertyFormatter.Simplify(property.Value));

                            if (NewRelicPropertyFormatter.Simplify(property.Key).ToString().Equals("trace.id")) traceId = NewRelicPropertyFormatter.Simplify(property.Value).ToString();
                            else if (NewRelicPropertyFormatter.Simplify(property.Key).ToString().Equals("span.id")) spanId = NewRelicPropertyFormatter.Simplify(property.Value).ToString();
                            else if (NewRelicPropertyFormatter.Simplify(property.Key).ToString().Equals("entity.name")) entityName = NewRelicPropertyFormatter.Simplify(property.Value).ToString();
                            else if (NewRelicPropertyFormatter.Simplify(property.Key).ToString().Equals("entity.type")) entityType = NewRelicPropertyFormatter.Simplify(property.Value).ToString();
                        }
                        Console.WriteLine("{0} *** AddProperty... entity.name = {1}({2}) trace.id = {3} span.id = {4}", 
                            DateTime.Now.ToLocalTime().ToString(), entityName, entityType, traceId, spanId);
                    }
                }
                else
                {
                    this.Attributes.Add(key, NewRelicPropertyFormatter.Simplify(value));
                }
            }
            catch (Exception ex) {
                Console.WriteLine("### Exception caught... {0)", ex.ToString());
            }
        }
    }
}
