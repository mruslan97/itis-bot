using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.Common
{
    public class BaseDbContext : DbContext
    {

        protected override void OnModelCreating(ModelBuilder db)
        {
            #region Indexes.

            foreach (var type in this.GetType().GetTypeInfo().Assembly.GetTypes())
            {
                foreach (var property in type.GetProperties())
                {
                    var indexes = new Dictionary<string, IndexAttribute>();

                    var index = property.GetCustomAttribute<IndexAttribute>();
                    if (index != null)
                    {
                        indexes.Add(property.Name, index);
                    }

                    foreach (var i in indexes)
                    {
                        var v = db.Entity(type).HasIndex(i.Key);
                        if (i.Value.IsUnique)
                            v.IsUnique();

                        if (null != i.Value.Name && i.Value.Name.Length > 0)
                            v.HasName(i.Value.Name);
                    }
                }
            }

            #endregion

            base.OnModelCreating(db);
        }
       

        #region Save.

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnSave();

            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            OnSave();

            return base.SaveChanges();
        }
        

        /// <summary>
        /// Общая обработка изменяемых сущностей в базе.
        /// </summary>
        void OnSave()
        {
            var date = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(p => p.Version).CurrentValue = 1;
                        entry.Property(p => p.Created).CurrentValue = date;
                        entry.Property(p => p.Edited).CurrentValue = date;
                        break;

                    case EntityState.Modified:
                        entry.Property(p => p.Version).CurrentValue++;
                        entry.Property(p => p.Edited).CurrentValue = date;
                        break;
                }
            }
            
        }

        #endregion

        #region Drop.

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        {
            return base.Remove(entity);
        }

        #endregion
    }
    
}