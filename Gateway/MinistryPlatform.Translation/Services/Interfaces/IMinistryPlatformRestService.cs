﻿using System.Collections.Generic;

namespace MinistryPlatform.Translation.Services.Interfaces
{
    public interface IMinistryPlatformRestService
    {
        /// <summary>
        /// This fluent method allows you to call various service methods using a particular MP OAuth token.  For instance, service.UsingAuthenticationToken(token).Get&lt;Event&gt;(2).
        /// </summary>
        /// <param name="authToken">The authentication token to use for subsequent calls to the service.</param>
        /// <returns>the instance of the service, to use with other method calls</returns>
        IMinistryPlatformRestService UsingAuthenticationToken(string authToken);

        /// <summary>
        /// Get a particular record, by the primary key ID column, from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T">The type of record to get.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="recordId">The primary key ID of the record to retrieve</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <returns>An object representing the MP row for the ID, if found.</returns>
        T Get<T>(int recordId, string selectColumns = null);

        /// <summary>
        /// Get a list of records for a given type from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T">The type of record to get.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="searchString">An "MP SQL" WHERE clause, for instance "Payment_Type_Id > 5 AND Payment_Type_Id &lt; 9".  If not specified, all rows will be returned.</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <returns>An List of objects representing the matching MP rows for the search, if found.</returns>
        List<T> Search<T>(string searchString = null, string selectColumns = null);
    }
}
