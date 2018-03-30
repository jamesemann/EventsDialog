using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac.Util;
using EventsBot.Dialogs.Framework;
using EventsBot.Dialogs.ThirdParty.Null;
using EventsBot.Interfaces;

namespace EventsBot.Dialogs
{
    public static class ServiceLocator
    {
        public static EventRegistrationDialog EventRegistration
        {
            get
            {
                var eventsBotServiceAttribute = GetEventsBotServiceAttribute();

                var instanceType = eventsBotServiceAttribute.eventRegistrationDialog;
                var ctor = instanceType.GetConstructor(new Type[0]);
                return (EventRegistrationDialog)ctor.Invoke(new object[0]);
            }
        }

        public static EventDiscoveryService EventDiscovery
        {
            get
            {
                var eventsBotServiceAttribute = GetEventsBotServiceAttribute();

                var instanceType = eventsBotServiceAttribute.eventDiscoveryService;
                var ctor = instanceType.GetConstructor(new Type[0]);
                return (EventDiscoveryService)ctor.Invoke(new object[0]);
            }
        }

        private static (Type eventRegistrationDialog, Type eventDiscoveryService) GetEventsBotServiceAttribute()
        {
            var foundTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetExportedTypes())
                    {
                        if (typeof(EventsDialog).IsAssignableFrom(type) && typeof(EventsDialog) != type)
                        {
                            foundTypes.Add(type);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            if (foundTypes.Count != 1)
            {
                throw new Exception("must have one and only one subtype of EventsDialog");
            }

            var eventsBotServiceAttribute = foundTypes.FirstOrDefault().CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(EventsBotServiceAttribute));

            var ctorArgs = eventsBotServiceAttribute.ConstructorArguments.ToList();

            return (ctorArgs.Count == 1 ? typeof(NullRegistrationDialog) : (Type)ctorArgs[1].Value, (Type)ctorArgs[0].Value);
        }

        private static Assembly GetWebEntryAssembly()
        { 
            var frames = new StackTrace().GetFrames();
            var i = frames.FirstOrDefault(c => Assembly.GetAssembly(c.GetMethod().DeclaringType).FullName != Assembly.GetExecutingAssembly().FullName).GetMethod().DeclaringType;
            return Assembly.GetAssembly(i);
        }
    }
}