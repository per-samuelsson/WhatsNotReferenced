
using Starcounter;
using Starcounter.Metadata;
using System.Collections.Generic;
using System.Linq;
using Starcounter.Internal.Metadata;

namespace WhatsNotReferenced
{
    public class UnreferencedAssets
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

        public static UnreferencedAssets Detect()
        {
            return new UnreferencedAssets().DetectApps().DetectTables().DetectColumns();
        }

        UnreferencedAssets DetectApps()
        {
            Apps = Db.Applications.Where(a => a != Application.Current);
            return this;
        }

        UnreferencedAssets DetectTables()
        {
            var views = Db.SQL<RawView>("SELECT v FROM RawView v WHERE Updatable = ?", true);

            var viewsNotReferenced = views.Where(c =>
            {
                var clr = Db.SQL<ClrClass>("SELECT clr FROM ClrClass clr WHERE clr.Mapper = ?", c).SingleOrDefault();
                return clr == null;
            });

            Tables = viewsNotReferenced.Select(c => new UnreferencedTable() { FullName = c.FullName });

            return this;
        }

        UnreferencedAssets DetectColumns()
        {
            var droppedTables = Tables.ToDictionary(c => c.FullName);

            // Always approach the deepest declaration. Dropping columns and their
            // data will be cascaded.
            var columns = Db.SQL<RawColumn>("SELECT c FROM RawColumn c WHERE c.Inherited = ? AND c.Table.Updatable = ?", false, true);

            var columnsNotReferenced = columns.Where(c =>
            {
                var mapped = Db.SQL<MappedProperty>("SELECT m FROM MappedProperty m WHERE m.Column = ?", c).SingleOrDefault();
                return mapped == null && !droppedTables.ContainsKey(c.Table.FullName);
            });

            Columns = columnsNotReferenced.Select(c => new UnreferencedColumn() { TableName = c.Table.FullName, Name = c.Name });
            return this;
        }
    }
}
