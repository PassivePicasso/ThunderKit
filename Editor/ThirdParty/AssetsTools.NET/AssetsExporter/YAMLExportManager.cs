using AssetsExporter.YAMLExporters;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.Text;
using AssetsExporter.YAML;
using System.Text.RegularExpressions;
using AssetsExporter.Collection;
using System.Linq;

namespace AssetsExporter
{
    public sealed partial class YAMLExportManager
    {
        private readonly SortedSet<IRegistrationContext> exporters = new SortedSet<IRegistrationContext>(new RegistrationContextComparer());

        public YAMLNode Export(ExportContext context, AssetTypeValueField parentField, AssetTypeValueField field, bool raw = false, params Type[] ignoreExporterTypes)
        {
            var exporter = PickExporter(parentField, field, ignoreExporterTypes);

            return exporter.Export(context, parentField, field, raw);
        }

        public IEnumerable<YAMLDocument> Export(BaseAssetCollection collection, AssetsManager manager, UnityVersion unityVersion, Dictionary<string, object> extraInfo = null)
        {
            foreach (var asset in collection.Assets)
            {
                var baseField = asset.baseField;
                if (baseField.IsDummy)
                {
                    yield return new YAMLDocument();
                    continue;
                }

                var context = new ExportContext(this, manager, collection, asset, unityVersion, extraInfo);

                var doc = new YAMLDocument();
                var root = doc.CreateMappingRoot();
                (root.Tag, root.Anchor) = collection.GetTagAndAnchor(asset);

                root.Add(baseField.TypeName, context.Export(null, baseField));
                yield return doc;
            }
        }

        public IYAMLExporter PickExporter(AssetTypeValueField parentField, AssetTypeValueField field, params Type[] ignoreExporterTypes)
        {
            var template = field.TemplateField;
            foreach (var exporter in exporters)
            {
                if (ignoreExporterTypes.Contains(exporter.ExporterType)) continue;
                if (template.HasValue && ((1u << (int)template.ValueType - 1) & exporter.ValueType) == 0) continue;
                if (!template.HasValue && exporter.OnlyValueTypes) continue;
                if (parentField != null)
                {
                    if (!TypeMatch(exporter.ParentTypeNames, parentField.TypeName, StringComparer)) continue;
                    if (!TypeMatch(exporter.RegexParentTypeNames, parentField.TypeName, RegexComparer)) continue;
                }
                if (!TypeMatch(exporter.TypeNames, template.Type, StringComparer)) continue;
                if (!TypeMatch(exporter.RegexTypeNames, template.Type, RegexComparer)) continue;

                return exporter.ExporterInstance;
            }
            throw new NotSupportedException("Not found suitable exporter");

            bool TypeMatch<T>(HashSet<T> collection, string value, Func<T, string, bool> comparer)
            {
                if (collection.Count == 0)
                {
                    return true;
                }
                var match = false;
                foreach (var item in collection)
                {
                    if (comparer(item, value))
                    {
                        match = true;
                        break;
                    }
                }
                return match;
            }
            bool StringComparer(string first, string second) => first == second;
            bool RegexComparer(Regex first, string second) => first.IsMatch(second);
        }

        public YAMLExportManager RegisterExporter<T>(Action<RegistrationContext> action) where T : IYAMLExporter, new()
        {
            var registration = new RegistrationContext() as IRegistrationContext;
            registration.ExporterInstance = new T();
            registration.ExporterType = typeof(T);
            action?.Invoke(registration as RegistrationContext);
            exporters.Add(registration);
            return this;
        }

        public static YAMLExportManager CreateDefault()
        {
            return new YAMLExportManager()
                .RegisterExporter<ValueTypeExporter>(x => x
                    .WithPriority(int.MaxValue)
                    .WhenAnyValueType(new[] { AssetValueType.Array, AssetValueType.ByteArray })
                    .WhenOnlyValueTypes())
                .RegisterExporter<PPtrExporter>(x => x
                    .WhenTypeNameRegex(/* language=regex */ @"\APPtr<(.*)>\z"))
                .RegisterExporter<ComponentPairExporter>(x => x
                    .WhenTypeName("ComponentPair"))
                .RegisterExporter<PairExporter>(x => x
                    .WhenTypeName("pair"))
                .RegisterExporter<TypelessDataExporter>(x => x
                    .WhenTypeName("TypelessData")
                    .WhenValueType(AssetValueType.ByteArray))
                .RegisterExporter<StreamingInfoExporter>(x => x
                    .WhenTypeName("StreamingInfo"))
                .RegisterExporter<GUIDExporter>(x => x
                    .WhenTypeName("GUID"))
                .RegisterExporter<GraphicsSettingsExporter>(x => x
                    .WhenTypeName("TierGraphicsSettings"))
                .RegisterExporter<GenericExporter>(x => x
                    .WithPriority(int.MinValue)
                    .WhenValueType(AssetValueType.Array)
                    .WhenValueType(AssetValueType.ByteArray));
        }
    }
}
