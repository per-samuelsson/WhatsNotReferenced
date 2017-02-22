
using System;
using Starcounter;
using Starcounter.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace WhatsNotReferenced
{
    public class DatabaseClass
    {
        public string FullName { get; set; }
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

        public static Assets Detect(ClassInlusion inclusion = ClassInlusion.Default)
        {
            var result = new Assets();

            var classes = Db.SQL<RawView>("SELECT v FROM RawView v");
            var notReferenced = classes.Where(c =>
            {
                var clr = Db.SQL<ClrClass>("SELECT clr FROM ClrClass clr WHERE clr.UniqueIdentifier = ?", c.UniqueIdentifier).SingleOrDefault();
                return clr == null;
            });

            result.Classes = notReferenced.Select(c => new DatabaseClass() { FullName = c.FullName });
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
                return string.Join(Environment.NewLine, assets.Classes.Select(c => c.FullName));
            });
        }
    }
}