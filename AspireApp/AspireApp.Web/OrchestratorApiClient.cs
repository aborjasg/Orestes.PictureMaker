using AspireApp.Libraries.Models;
using AspireApp.ServiceDefaults.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace AspireApp.Web
{
    /// <summary>
    /// 
    /// </summary>
    
    public class OrchestratorApiClient(HttpClient httpClient)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ActionResponse?> getSourceData(DerivedDataFilter filter, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/getSourceData", filter);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken!);
                else
                    return new ActionResponse() { Type = response.StatusCode.ToString(), Message = "Data cannot be found" };
            }
            catch (Exception ex)
            {
                return new ActionResponse() { Type = "Error", Message = ex.Message };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ActionResponse?> processData(DerivedDataFilter filter, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
            {
                var response = await httpClient.PostAsJsonAsync("/processData", filter);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken!);
                else
                    return new ActionResponse() { Type = response.StatusCode.ToString(), Message = "Data cannot be processed" };
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ActionResponse?> saveRunImage(RunImage record, CancellationToken cancellationToken = default)
        {
            var response = await httpClient.PostAsJsonAsync("/saveRunImage", record);
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken!);
            else
                return new ActionResponse() { Type = response.StatusCode.ToString(), Message = "The record cannot be saved" };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ActionResponse?> getRunImage(int id, CancellationToken cancellationToken = default)
        {            
            var response = await httpClient.GetAsync($"/getRunImage/{id}");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)            
                return await response.Content.ReadFromJsonAsync<ActionResponse>(cancellationToken!);
            else
                return new ActionResponse() { Type = response.StatusCode.ToString(), Message = "The record cannot  be loaded" };            
        }
    }
}
