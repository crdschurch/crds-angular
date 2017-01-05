﻿using System.Collections.Generic;

namespace MinistryPlatform.Translation.Repositories.Interfaces
{
    public interface IMinistryPlatformRestRepository
    {
        /// <summary>
        /// This fluent method allows you to call various service methods using a particular MP OAuth token.  For instance, service.UsingAuthenticationToken(token).Get&lt;Event&gt;(2).
        /// </summary>
        /// <param name="authToken">The authentication token to use for subsequent calls to the service.</param>
        /// <returns>the instance of the service, to use with other method calls</returns>
        IMinistryPlatformRestRepository UsingAuthenticationToken(string authToken);

        /// <summary>
        /// Get a particular record, by the primary key ID column, from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T">The type of record to get.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="recordId">The primary key ID of the record to retrieve</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <returns>An object representing the MP row for the ID, if found.</returns>
        T Get<T>(int recordId, string selectColumns = null);

        T Get<T>(string tableName, int recordId, string columnName);

        List<T> Get<T>(string tableName, Dictionary<string, object> filter);

        /// <summary>
        /// Get results from a stored procedure in Ministry Platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute</param>
        /// <returns></returns>
        List<List<T>> GetFromStoredProc<T>(string procedureName);

        /// <summary>
        /// Get results from a stored procedure in Ministry Platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procedureName">The name of the stored procedure to execute</param>
        /// <param name="parameters">Parameters for the stored procedure. Key is paramter name and Value is the value.</param>
        /// <returns></returns>
        List<List<T>> GetFromStoredProc<T>(string procedureName, Dictionary<string, object> parameters);

        /// <summary>
        /// Get a list of records for a given type from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T">The type of record to get.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="searchString">An "MP SQL" WHERE clause, for instance "Payment_Type_Id > 5 AND Payment_Type_Id &lt; 9".  If not specified, all rows will be returned.</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <param name="orderByString">Optionally specify how to order retrieved columns. </param>
        /// <param name="distinct">Specify whether only distinct records should be returned.  defaults to false.</param>
        /// <returns>An List of objects representing the matching MP rows for the search, if found.</returns>
        List<T> Search<T>(string searchString = null, string selectColumns = null, string orderByString = null, bool distinct = false);

        /// <summary>
        /// Get a list of records searching by one table but returning another (joining) a given type from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T1">The type to search on.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <typeparam name="T2">The type to return.  This allows us to return a joined table.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="searchString">An "MP SQL" WHERE clause, for instance "Payment_Type_Id > 5 AND Payment_Type_Id &lt; 9".  If not specified, all rows will be returned.</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <param name="orderByString">Optionally specify how to order retrieved columns. </param>
        /// <param name="distinct">Specify whether only distinct records should be returned.  defaults to false.</param>
        /// <returns>An List of objects representing the matching MP rows for the search, if found.</returns>
        List<T> Search<T>(string searchString, List<string> columns, string orderByString = null, bool distinct = false);

        /// <summary>
        /// Get a list of records searching by one table but returning another (joining) a given type from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T1">The type to search on.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <typeparam name="T2">The type to return.  This allows us to return a joined table.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="searchString">An "MP SQL" WHERE clause, for instance "Payment_Type_Id > 5 AND Payment_Type_Id &lt; 9".  If not specified, all rows will be returned.</param>
        /// <param name="selectColumns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.  If not specified, all columns will be retrieved.</param>
        /// <param name="orderByString">Optionally specify how to order retrieved columns. </param>
        /// <param name="distinct">Specify whether only distinct records should be returned.  defaults to false.</param>
        /// <returns>An List of objects representing the matching MP rows for the search, if found.</returns>
        List<T2> Search<T1, T2>(string searchString = null, string selectColumns = null, string orderByString = null, bool distinct = false);

        /// <summary>
        /// Get a list of records for a given type from MinistryPlatform.
        /// </summary>
        /// <typeparam name="T">The type of record to get.  This should correspond to an appropriately annotated model class, so that MP columns can be properly mapped (using NewtonSoft.Json) from MP to the model object.  The model class must also be annotated with the RestApiTable attribute, specifying the actual MP table name.</typeparam>
        /// <param name="searchString">An "MP SQL" WHERE clause, for instance "Payment_Type_Id > 5 AND Payment_Type_Id &lt; 9".</param>
        /// <param name="columns">Optionally specify which columns to retrieve from MP.  This is a comma-separated list of column names.</param>
        /// <param name="orderByString">Optionally specify how to order retrieved columns. </param>
        /// <param name="distinct">Specify whether only distinct records should be returned.  defaults to false.</param>
        /// <returns>An List of objects representing the matching MP rows for the search, if found.</returns>
        List<T2> Search<T1, T2>(string searchString, List<string> columns, string orderByString = null, bool distinct = false);

        T Search<T>(string tableName, string searchString, string column);

        /// <summary>
        /// Update a set of records of the given type in MP. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        int Post<T>(List<T> records);

        /// <summary>
        /// Create a record and return a new record with it's ID 
        /// </summary>
        /// <typeparam name="T">The expected Return type</typeparam>
        /// <typeparam name="M"> The shape of the data in Ministry Platform</typeparam>
        /// <param name="records"></param>
        /// <returns> The list of records with their new ID </returns>
        List<T> PostWithReturn<M, T>(List<M> records);

        int Put<T>(List<T> records);
        int Put(string tableName, List<Dictionary<string, object>> records);
        int PostStoredProc(string procedureName, Dictionary<string, object> parameters);

        void Delete<T>(IEnumerable<int> recordIds);

        void UpdateRecord(string tableName, int recordId, Dictionary<string, object> fields);
    }
}
