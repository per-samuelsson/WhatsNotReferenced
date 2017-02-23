
using System;
using Starcounter;
using Starcounter.Metadata;
using System.Collections.Generic;
using System.Linq;
using Starcounter.Internal.Metadata;

namespace WhatsNotReferenced
{
    public class DatabaseClass
    {
        public string FullName { get; set; }
    }

    public class DatabaseFieldOrProperty
    {
        public string ClassFullName { get; set; }
        public string Name { get; set; }
    }

    public enum ClassInlusion
    {
        UserDefined = 1,
        Simplified = 2,
        Starcounter = 4,
        Metadata = 8,
        Default = UserDefined
    }

    public class Assets
    {
        /// <summary>
        /// All classes not referenced
        /// </summary>
        public IEnumerable<DatabaseClass> Classes { get; private set; }

        /// <summary>
        /// All database fields/properties not referenced.
        /// </summary>
        public IEnumerable<DatabaseFieldOrProperty> FieldsAndProperties { get; private set; }

        public static Assets Detect(ClassInlusion inclusion = ClassInlusion.Default)
        {
            var result = new Assets();

            var classes = Db.SQL<RawView>("SELECT v FROM RawView v");

            // Filter on inclusion
            // TODO:

            var classesNotReferenced = classes.Where(c =>
            {
                // var clr = Db.SQL<ClrClass>("SELECT clr FROM ClrClass clr WHERE clr.UniqueIdentifier = ?", c.UniqueIdentifier).SingleOrDefault();
                var clr = Db.SQL<ClrClass>("SELECT clr FROM ClrClass clr WHERE clr.Mapper = ?", c).SingleOrDefault();
                return clr == null;
            });

            result.Classes = classesNotReferenced.Select(c => new DatabaseClass() { FullName = c.FullName });

            // For columns:
            // Get each RawColumn that is not referenced by any MappedProperty.Column
            //
            // If the column is part of a dropped table, it's not interesting: it's aready
            // part of that set.

            var columns = Db.SQL<RawColumn>("SELECT c FROM RawColumn c WHERE c.Inherited = ?", false);
            var columnsNotReferenced = columns.Where(c =>
            {
                var mapped = Db.SQL<MappedProperty>("SELECT m FROM MappedProperty m WHERE m.Column = ?", c).SingleOrDefault();
                return mapped == null;
            });

            result.FieldsAndProperties = columnsNotReferenced.Select(c => new DatabaseFieldOrProperty() { ClassFullName = c.Table.FullName, Name = c.Name });

            return result;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // args[n]: please:[action]

            Handle.GET("/whatsnotreferenced", () =>
            {
                var assets = Assets.Detect();
                var classes = string.Join(Environment.NewLine, assets.Classes.Select(c => c.FullName));
                var fieldsAndProperties = string.Join(Environment.NewLine, assets.FieldsAndProperties.Select(c => c.ClassFullName + "." + c.Name));

                var result = "Tables:";
                result += Environment.NewLine + classes;
                result += Environment.NewLine + Environment.NewLine + "Columns:";
                result += Environment.NewLine + fieldsAndProperties;

                return result;
            });
        }
    }
}