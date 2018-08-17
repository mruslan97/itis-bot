using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace DAL.Common
{
    /// <summary>
    /// Base entity.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Идентификатор сущности.
        /// </summary>
        [Column("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long Id { get; set; }


        /// <summary>
        /// Версия сущности.
        /// </summary>
        [Column("version")]
        [JsonIgnore] public virtual long Version { get; set; }

        /// <summary>
        /// Дата создания.
        /// </summary>
        [Column("created")]
        [JsonIgnore] public virtual DateTime Created { get; set; }

        /// <summary>
        /// Дата изменения.
        /// </summary>
        [Column("edited")]
        [JsonIgnore] public virtual DateTime Edited { get; set; }
    }
}