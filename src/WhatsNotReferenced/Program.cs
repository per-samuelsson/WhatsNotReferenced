
using System;
using Starcounter;
using Starcounter.Metadata;
using System.Collections.Generic;
using System.Linq;
using Starcounter.Internal.Metadata;

namespace WhatsNotReferenced
{
    public class UnreferencedTable
    {
        public string FullName { get; set; }
    }

    public class UnreferencedColumn
    {
        public string TableName { get; set; }
        public string Name { get; set; }
    }

    public class Assets
    {
        /// <summary>
        /// All apps running when current assets where detected.
        /// </summary>
        public IEnumerable<Application> Apps { get; private set; }

        /// <summary>
        /// All tables not referenced
        /// </summary>
        public IEnumerable<UnreferencedTable> Tables { get; private set; }

        /// <summary>
        /// All columns not referenced.
        /// </summary>
        public IEnumerable<UnreferencedColumn> Columns { get; private set; }

        public static Assets Detect()
        {
            return new Assets().DetectApps().DetectTables().DetectColumns();
        }

        Assets DetectApps()
        {
            Apps = Db.Applications.Where(a => a != Application.Current);
            return this;
        }

        Assets DetectTables()
        {
            var views = Db.SQL<RawView>("SELECT v FROM RawView v");
            
            var viewsNotReferenced = views.Where(c =>
            {
                var clr = Db.SQL<ClrClass>("SELECT clr FROM ClrClass clr WHERE clr.Mapper = ?", c).SingleOrDefault();
                return clr == null;
            });

            Tables = viewsNotReferenced.Select(c => new UnreferencedTable() { FullName = c.FullName });

            return this;
        }

        Assets DetectColumns()
        {
            var droppedTables = Tables.ToDictionary(c => c.FullName);
            
            // Always approach the deepest declaration. Dropping columns and their
            // data will be cascaded.
            var columns = Db.SQL<RawColumn>("SELECT c FROM RawColumn c WHERE c.Inherited = ?", false);

            var columnsNotReferenced = columns.Where(c =>
            {
                var mapped = Db.SQL<MappedProperty>("SELECT m FROM MappedProperty m WHERE m.Column = ?", c).SingleOrDefault();
                return mapped == null && !droppedTables.ContainsKey(c.Table.FullName);
            });

            Columns = columnsNotReferenced.Select(c => new UnreferencedColumn() { TableName = c.Table.FullName, Name = c.Name });
            return this;
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

                var apps = string.Join(Environment.NewLine, assets.Apps.Select(a => a.DisplayName));
                var tables = string.Join(Environment.NewLine, assets.Tables.Select(c => c.FullName));
                var columns = string.Join(Environment.NewLine, assets.Columns.Select(c => c.TableName + "." + c.Name));

                var result = "Apps (exluding this):";
                result += Environment.NewLine + apps;
                result += Environment.NewLine + Environment.NewLine + "Tables:";
                result += Environment.NewLine + tables;
                result += Environment.NewLine + Environment.NewLine + "Columns:";
                result += Environment.NewLine + columns;

                return result;
            });
        }
    }
}