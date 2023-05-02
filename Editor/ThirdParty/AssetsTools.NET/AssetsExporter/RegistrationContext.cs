using AssetsExporter.YAMLExporters;
using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetsExporter
{
    public sealed partial class YAMLExportManager
    {
        private interface IRegistrationContext
        {
            IYAMLExporter ExporterInstance { get; set; }
            Type ExporterType { get; set; }
            int Priority { get; set; }
            HashSet<string> TypeNames { get; }
            HashSet<Regex> RegexTypeNames { get; }
            HashSet<string> ParentTypeNames { get; }
            HashSet<Regex> RegexParentTypeNames { get; }
            uint ValueType { get; set; }
            bool OnlyValueTypes { get; set; }
        }

        //Sort by descending
        private class RegistrationContextComparer : IComparer<IRegistrationContext>
        {
            public int Compare(IRegistrationContext x, IRegistrationContext y)
            {
                return x.Priority > y.Priority ? -1 : 1;
            }
        }

        public class RegistrationContext : IRegistrationContext
        {
            Type IRegistrationContext.ExporterType { get; set; }
            IYAMLExporter IRegistrationContext.ExporterInstance { get; set; }
            int IRegistrationContext.Priority { get; set; }
            HashSet<string> IRegistrationContext.TypeNames { get; } = new HashSet<string>();
            HashSet<Regex> IRegistrationContext.RegexTypeNames { get; } = new HashSet<Regex>();
            HashSet<string> IRegistrationContext.ParentTypeNames { get; } = new HashSet<string>();
            HashSet<Regex> IRegistrationContext.RegexParentTypeNames { get; } = new HashSet<Regex>();
            uint IRegistrationContext.ValueType { get; set; }
            bool IRegistrationContext.OnlyValueTypes { get; set; }

            private IRegistrationContext ThisIRC => this;

            public RegistrationContext WhenOnlyValueTypes()
            {
                ThisIRC.OnlyValueTypes = true;
                return this;
            }

            public RegistrationContext WhenAnyValueType(IEnumerable<AssetValueType> except = null)
            {
                ThisIRC.ValueType = uint.MaxValue;
                if (except != null)
                {
                    foreach (var exceptValue in except)
                    {
                        if (exceptValue != AssetValueType.None)
                        {
                            ThisIRC.ValueType &= ~(1u << (int)exceptValue - 1);
                        }
                    }
                }
                return this;
            }

            public RegistrationContext WhenValueType(AssetValueType valueType)
            {
                if (valueType != AssetValueType.None)
                {
                    ThisIRC.ValueType |= 1u << (int)valueType - 1;
                }
                return this;
            }

            public RegistrationContext WhenTypeName(string typeName)
            {
                ThisIRC.TypeNames.Add(typeName);
                return this;
            }

            public RegistrationContext WhenTypeNameRegex(string typeNameRegex)
            {
                ThisIRC.RegexTypeNames.Add(new Regex(typeNameRegex, RegexOptions.Compiled));
                return this;
            }

            public RegistrationContext WhenParentTypeName(string typeName)
            {
                ThisIRC.ParentTypeNames.Add(typeName);
                return this;
            }

            public RegistrationContext WhenParentTypeNameRegex(string typeNameRegex)
            {
                ThisIRC.RegexParentTypeNames.Add(new Regex(typeNameRegex, RegexOptions.Compiled));
                return this;
            }

            public RegistrationContext WithPriority(int priority)
            {
                ThisIRC.Priority = priority;
                return this;
            }
        }
    }
}
