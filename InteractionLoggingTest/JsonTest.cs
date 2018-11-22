using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ViretTool.InteractionLogging.DataObjects;
using ViretTool.InteractionLogging.JsonSerialization;
using Action = ViretTool.InteractionLogging.DataObjects.Action;

namespace InteractionLoggingTest
{
    [TestClass]
    public class JsonTest
    {
        

        [TestMethod]
        public void TestConversion()
        {
            Log log = GenerateTestLog();
            string output = LowercaseJsonSerializer.SerializeObject(log);

        }



        private static Log GenerateTestLog()
        {
            Log log = new Log();
            log.TeamId = 1;
            log.MemberId = 2;


            Event event1 = new Event();
            event1.Timestamp = 1541074352;

            Action e1Action1 = new Action();
            e1Action1.Category = "text";
            e1Action1.Type = "ASR";
            e1Action1.Value = "how are you";
            e1Action1.Attributes = "1000 NN";

            Action e1Action2 = new Action();
            e1Action2.Category = "image";
            e1Action2.Type = "dataset";
            e1Action2.Value = "[10001, 10002, 10003]";
            e1Action2.Attributes = "1000 NN";

            List<Action> e1Interactions = new List<Action>{ e1Action1, e1Action2 };
            event1.Actions = e1Interactions;


            Event event2 = new Event();
            event2.Timestamp = 1541079978;

            Action e2Action1 = new Action();
            e2Action1.Category = "Post";
            e2Action1.Type = "submit";
            
            List<Action> e2Interactions = new List<Action> { e2Action1 };
            event2.Actions = e2Interactions;


            List<Event> events = new List<Event> { event1, event2 };
            log.Events = events;

            return log;
        }
    }
}
