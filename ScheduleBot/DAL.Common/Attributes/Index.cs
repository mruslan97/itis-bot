using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DAL.Common
{
    /// <summary>
    /// Индекс.
    /// </summary>
    public class IndexAttribute : Attribute
    {
        public IndexAttribute(bool unique = false)
        {
            IsUnique = unique;

        }
        public IndexAttribute(string name, bool unique = false)
        {
            IsUnique = unique;
            Name = name;

        }

        /// <summary>
        /// Требование уникальности индекса.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Имя индекса.
        /// </summary>
        public string Name { get; set; }
    }
}
