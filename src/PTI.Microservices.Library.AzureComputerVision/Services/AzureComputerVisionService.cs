using Microsoft.Extensions.Logging;
using PTI.Microservices.Library.Configuration;
using PTI.Microservices.Library.Interceptors;
using PTI.Microservices.Library.Models.AzureComputerVision.DescribeImage;
using PTI.Microservices.Library.Models.AzureComputerVision.AnalyzeImage;
using PTI.Microservices.Library.Models.AzureComputerVision.DetectText;
using PTI.Microservices.Library.Models.AzureComputerVision.Read;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PTI.Microservices.Library.AzureComputerVision.Models.DetectText;

namespace PTI.Microservices.Library.Services
{
    /// <summary>
    /// Services in charge of exposing access to Azure Computer Vision functionality
    /// </summary>
    public sealed class AzureComputerVisionService
    {
        private ILogger<AzureComputerVisionService> Logger { get; set; }
        private AzureComputerVisionConfiguration AzureComputerVisionConfiguration { get; set; }
        private CustomHttpClient CustomHttpClient { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="AzureComputerVisionService"/>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="azureComputerVisionConfiguration"></param>
        /// <param name="customHttpClient"></param>
        public AzureComputerVisionService(ILogger<AzureComputerVisionService> logger,
            AzureComputerVisionConfiguration azureComputerVisionConfiguration,
            CustomHttpClient customHttpClient)
        {
            this.Logger = logger;
            this.AzureComputerVisionConfiguration = azureComputerVisionConfiguration;
            this.CustomHttpClient = customHttpClient;
        }

        /// <summary>
        /// Visual Features
        /// </summary>
        public enum VisualFeature
        {
            /// <summary>
            /// Adult
            /// </summary>
            Adult,
            /// <summary>
            /// Brands
            /// </summary>
            Brands,
            /// <summary>
            /// Categories
            /// </summary>
            Categories,
            /// <summary>
            /// Color
            /// </summary>
            Color,
            /// <summary>
            /// Description
            /// </summary>
            Description,
            /// <summary>
            /// Faces
            /// </summary>
            Faces,
            /// <summary>
            /// ImageType
            /// </summary>
            ImageType,
            /// <summary>
            /// Objects
            /// </summary>
            Objects,
            /// <summary>
            /// Tags
            /// </summary>
            Tags
        }

        /// <summary>
        /// Details
        /// </summary>
        public enum Details
        {
            /// <summary>
            /// Celebrities
            /// </summary>
            Celebrities,
            /// <summary>
            /// Landmarks
            /// </summary>
            Landmarks
        }

        /// <summary>
        /// Language
        /// </summary>
        public enum Language
        {
            /// <summary>
            /// English Language
            /// </summary>
            English,
            /// <summary>
            /// Spanish Language
            /// </summary>
            Spanish
        }

        private string GetLanguageString(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "en";
                case Language.Spanish:
                    return "es";
                default:
                    return "en";
            }
        }
        /// <summary>
        /// Image Detection languages
        /// </summary>
        public enum DetectImageLanguage
        {
            /// <summary>
            /// Spanish Language
            /// </summary>
            Spanish,
            /// <summary>
            /// English Language
            /// </summary>
            English
        }

        /// <summary>
        /// Detects the text in the specified image
        /// </summary>
        /// <param name="imageStream"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Analyzeresult> ReadAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}" +
                    $"/vision/v3.2/read/analyze?overload=stream";
                //$"[?language]" +
                //$"[&pages]";
                var imageBytes = memoryStream.ToArray();
                imageStream.Read(imageBytes, 0, imageBytes.Length);
                ByteArrayContent byteArrayContent = new ByteArrayContent(imageBytes);
                byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(System.Net.Mime.MediaTypeNames.Application.Octet);
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                var response = await this.CustomHttpClient.PostAsync(requestUrl, byteArrayContent, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var operationLocation = response.Headers.GetValues("Operation-Location").Single();
                    bool shouldStop = false;
                    GetReadResultResponse getReadResultResponse = null;
                    do
                    {
                        getReadResultResponse = await this.CustomHttpClient.GetFromJsonAsync<GetReadResultResponse>(operationLocation);
                        shouldStop = getReadResultResponse.status == "failed" || getReadResultResponse.status == "succeeded";
                    } while (!shouldStop);
                    return getReadResultResponse.analyzeResult;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Detects the text in the specified image
        /// </summary>
        /// <param name="imageUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Analyzeresult> ReadAsync(Uri imageUri, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}" +
                    $"/vision/v3.2/read/analyze";
                //$"[?language]" +
                //$"[&pages]";
                DetectTextRequest model = new DetectTextRequest()
                {
                    url = imageUri.ToString()
                };
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                var response = await this.CustomHttpClient.PostAsJsonAsync<DetectTextRequest>(requestUrl,
                    model, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var operationLocation = response.Headers.GetValues("Operation-Location").Single();
                    bool shouldStop = false;
                    GetReadResultResponse getReadResultResponse = null;
                    do
                    {
                        getReadResultResponse = await this.CustomHttpClient.GetFromJsonAsync<GetReadResultResponse>(operationLocation);
                        shouldStop = getReadResultResponse.status == "failed" || getReadResultResponse.status == "succeeded";
                    } while (!shouldStop);
                    return getReadResultResponse.analyzeResult;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }
        }
        /// <summary>
        /// Detects the text in the specified image
        /// </summary>
        /// <param name="imageUri"></param>
        /// <param name="language"></param>
        /// <param name="detectOrientation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DetectTextResponse> DetectTextAsync(Uri imageUri, DetectImageLanguage language,
            bool detectOrientation = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string languageString = language == DetectImageLanguage.Spanish ? "es" : "en";
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}" +
                    $"/vision/v3.1/ocr" +
                $"?language={languageString}" +
                $"&detectOrientation={detectOrientation}";
                DetectTextRequest model = new DetectTextRequest()
                {
                    url = imageUri.ToString()
                };
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                var response = await this.CustomHttpClient.PostAsJsonAsync<DetectTextRequest>(requestUrl,
                    model, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DetectTextResponse>();
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }

        }

        /// <summary>
        /// Analyzes the given image
        /// </summary>
        /// <param name="imageUri"></param>
        /// <param name="visualFeatures"></param>
        /// <param name="details"></param>
        /// <param name="language"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AnalyzeImageResponse> AnalyzeImageAsync(Uri imageUri, List<VisualFeature> visualFeatures = null,
            List<Details> details = null, Language language = Language.English, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}/vision/v3.0/analyze" +
                    $"?language={GetLanguageString(language)}";
                if (visualFeatures != null)
                {
                    requestUrl += "&visualFeatures=" + String.Join(",", visualFeatures);
                }
                if (details != null)
                {
                    requestUrl += "&details=" + String.Join(",", details);
                }
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                var requestModel = new AnalyzeImageRequest()
                {
                    Url = imageUri.ToString()
                };
                var response = await this.CustomHttpClient.PostAsJsonAsync<AnalyzeImageRequest>(requestUrl, requestModel,
                    cancellationToken: cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    AnalyzeImageResponse result = await response.Content.ReadFromJsonAsync<AnalyzeImageResponse>();
                    return result;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Analyzes the given image and describes it
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DescribeImageResponse> DescribeImageAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}/vision/v3.1/describe" +
                $"?maxCandidates={10}";
                //$"&language={language}" +
                //$"&descriptionExclude={descriptionExclude}";
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                MultipartFormDataContent multipartContent =
                    new MultipartFormDataContent();

                multipartContent.Add(
                    new StreamContent(stream),
                    "file", fileName);

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await this.CustomHttpClient.PostAsync(requestUrl, multipartContent, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DescribeImageResponse>(cancellationToken: cancellationToken);
                    return result;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Analyzes the given image and describes it
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DescribeImageResponse> DescribeImageAsync(Uri imageUrl, string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                var imageBytes = await this.CustomHttpClient.GetByteArrayAsync(imageUrl, cancellationToken);
                var stream = new MemoryStream(imageBytes);
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}/vision/v3.1/describe" +
                $"?maxCandidates={10}";
                //$"&language={language}" +
                //$"&descriptionExclude={descriptionExclude}";
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                MultipartFormDataContent multipartContent =
                    new MultipartFormDataContent();

                multipartContent.Add(
                    new StreamContent(stream),
                    "file", fileName);

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await this.CustomHttpClient.PostAsync(requestUrl, multipartContent, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DescribeImageResponse>(cancellationToken: cancellationToken);
                    return result;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Analyzes the specified image
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileName"></param>
        /// <param name="visualFeatures"></param>
        /// <param name="details"></param>
        /// <param name="language"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AnalyzeImageResponse> AnalyzeImageAsync(Stream stream, string fileName, List<VisualFeature> visualFeatures = null,
    List<Details> details = null, Language language = Language.English, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUrl = $"{this.AzureComputerVisionConfiguration.Endpoint}/vision/v3.0/analyze" +
                    $"?language={GetLanguageString(language)}";
                if (visualFeatures != null)
                {
                    requestUrl += "&visualFeatures=" + String.Join(",", visualFeatures);
                }
                if (details != null)
                {
                    requestUrl += "&details=" + String.Join(",", details);
                }
                this.CustomHttpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.AzureComputerVisionConfiguration.Key);
                MultipartFormDataContent multipartContent =
                    new MultipartFormDataContent();

                multipartContent.Add(
                    new StreamContent(stream),
                    "file", fileName);

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await this.CustomHttpClient.PostAsync(requestUrl, multipartContent, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    AnalyzeImageResponse result = await response.Content.ReadFromJsonAsync<AnalyzeImageResponse>(cancellationToken: cancellationToken);
                    return result;
                }
                else
                {
                    string reason = response.ReasonPhrase;
                    string detailedError = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                    throw new Exception($"Reason: {reason}. Details: {detailedError}");
                }
            }
            catch (Exception ex)
            {
                this.Logger?.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}
