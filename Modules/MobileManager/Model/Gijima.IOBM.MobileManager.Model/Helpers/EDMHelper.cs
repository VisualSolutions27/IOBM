using Gijima.IOBM.MobileManager.Model.Data;
using System;
using System.ComponentModel;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Helpers
{
    public class EDMHelper
    {
        /// <summary>
        /// Get the base sql structure of the entity.
        /// </summary>
        /// <typeparam name="T">The Entity</typeparam>
        /// <returns>Entity Properties</returns>
        public static PropertyDescriptor[] GetEntityStructure<T>()
        {
            // Get the entity structure
            Type entityType = typeof(T);
            PropertyDescriptorCollection edmxProperties = TypeDescriptor.GetProperties(typeof(T));
            PropertyDescriptor[] sqlProperties = null;
            int idx = 0;

            // Get the sql table structure
            using (var db = MobileManagerEntities.GetContext())
            {
                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var storageMetadata = ((EntityConnection)objectContext.Connection).GetMetadataWorkspace().GetItems(DataSpace.SSpace);
                var entityProperties = (from s in storageMetadata where s.BuiltInTypeKind == BuiltInTypeKind.EntityType select s as EntityType);
                var tableProperties = (from m in entityProperties where m.Name == entityType.Name select m).Single();
                sqlProperties = new PropertyDescriptor[tableProperties.Properties.Count];

                // Add only the properties found in the sql table structure
                foreach (System.ComponentModel.PropertyDescriptor prop in edmxProperties)
                {
                    if (tableProperties.Properties.Contains(prop.Name))
                    {
                        sqlProperties[idx] = prop;
                        idx++;
                    }
                }

                return sqlProperties;
            }
        }
    }
}
