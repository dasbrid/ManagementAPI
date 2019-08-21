using System;

namespace TodoApi
{
    public class DtAccess
    {
        public const string DEFAULT_DB_SCHEMA = "api.";

        /// <summary>
        /// Builds a stored procedure name by concatenating the name with the default database schema dbo in case the schema is not provided.
        /// </summary>
        /// <remarks>
        /// When executing a user-defined procedure, it is recommended qualifying the procedure name with the schema name. 
        /// This practice gives a small performance boost because the Database Engine does not have to search multiple schemas. 
        /// It also prevents executing the wrong procedure if a database has procedures with the same name in multiple schemas.
        /// </remarks>
        /// <param name="name">The name of the stored procedure.</param>
        /// <returns>A correctly formatted stored procedure name.</returns>
        public static string BuildStoredProcedureName(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            if (!name.Contains("."))
            {
                return name.Insert(0, DEFAULT_DB_SCHEMA);
            }
            return name;
        }
    }
}