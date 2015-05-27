using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace OneDrive
{
    /// <summary>
    /// Defines the public interface methods for ODConnection
    /// </summary>
    public partial class ODConnection
    {
        public Http.IHttpFactory HttpRequestFactory { get; set; }

        /// <summary>
        /// Retrieve an ODItem for the root of the current user's OneDrive
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ODItem> GetRootItemAsync(ItemRetrievalOptions options)
        {
            return await GetItemAsync(new ODItemReference { Id = ApiConstants.RootFolderItemId }, options);
        }

        /// <summary>
        /// Retrieve an ODItem for an item with a particular item-id
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="itemRetrievalOptions"></param>
        /// <returns></returns>
        public async Task<ODItem> GetItemAsync(ODItemReference itemReference, ItemRetrievalOptions itemRetrievalOptions)
        {
            var command = GetItemCommand(itemReference, itemRetrievalOptions);
            return await DataModelForRequest<ODItem>(command);
        }

        /// <summary>
        /// Returns a service command object to retrieve an item from the service
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public ServiceCommand GetItemCommand(ODItemReference itemReference, ItemRetrievalOptions options)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");

            var queryParams = ODataOptionsToQueryString(options);
            var serviceUri = UriForItemReference(itemReference, null, queryParams);

            return new ServiceCommand { ServiceUrl = serviceUri, HttpVerb = ApiConstants.HttpGet, ResponseType = typeof(ODItem) };
        }

        /// <summary>
        /// Return a collection of children of an item referenced by item-id
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="childrenRetrievalOptions"></param>
        /// <returns></returns>
        public async Task<ODItemCollection> GetChildrenOfItemAsync(ODItemReference itemReference, ChildrenRetrievalOptions childrenRetrievalOptions)
        {
            var command = GetChildrenOfItemCommand(itemReference, childrenRetrievalOptions);
            return await DataModelForRequest<ODItemCollection>(command);
        }

        public ServiceCommand GetChildrenOfItemCommand(ODItemReference itemReference, ChildrenRetrievalOptions childrenRetrievalOptions)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");

            var queryParams = ODataOptionsToQueryString(childrenRetrievalOptions);
            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.ChildrenRelationshipName, queryParams);

            return new ServiceCommand { ServiceUrl = serviceUri, HttpVerb = ApiConstants.HttpGet, ResponseType = typeof(ODItemCollection) };
        }

        /// <summary>
        /// Return a collection of thumbnails available for an item referenced by item-id
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="thumbnailOptions"></param>
        /// <returns></returns>
        public async Task<ODThumbnailSet[]> GetThumbnailsForItemAsync(ODItemReference itemReference, ThumbnailRetrievalOptions thumbnailOptions)
        {
            var command = GetThumbnailsForItemCommand(itemReference, thumbnailOptions);
            var results = await DataModelForRequest<ODCollectionResponse<ODThumbnailSet>>(command);

            if (null != results && results.Collection != null)
            {
                return results.Collection;
            }

            return null;
        }

        public ServiceCommand GetThumbnailsForItemCommand(ODItemReference itemReference, ThumbnailRetrievalOptions thumbnailOptions)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");

            var queryParams = ODataOptionsToQueryString(thumbnailOptions);
            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.ThumbnailsRelationshipName, queryParams);
            return new ServiceCommand { ServiceUrl = serviceUri, HttpVerb = ApiConstants.HttpGet, ResponseType = typeof(ODThumbnailSet) };
        }

        /// <summary>
        /// Generates a service request to download the binary contents of a file based on an item ID.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name = "options"></param>
        /// <returns></returns>
        public async Task<Stream> DownloadStreamForItemAsync(ODItemReference itemReference, StreamDownloadOptions options)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");

            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.ContentRelationshipName);
            var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpGet);
            options.ModifyRequest(request);

            var response = await GetHttpResponseAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                Uri downloadUri = new Uri(response.Headers[ApiConstants.LocationHeaderName]);
                request = await CreateHttpRequestAsync(downloadUri, ApiConstants.HttpGet);
                options.ModifyRequest(request);
                response = await GetHttpResponseAsync(request);
            }

            var responseStream = await response.GetResponseStreamAsync();
            return responseStream;
        }

        /// <summary>
        /// Uploads the contents of sourceFileStream to the service as the contents for itemId. This method can
        /// be used to replace the contents of an existing file or create a new file if the itemReference property
        /// is set to a path-only.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="sourceFileStream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ODItem> PutContentsAsync(ODItemReference itemReference, Stream sourceFileStream, ItemUploadOptions options)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");
            if (null == options)
                throw new ArgumentNullException("options");
            if (null == sourceFileStream) 
                throw new ArgumentNullException("sourceFileStream");
            if (sourceFileStream.Length > MaxRegularUploadSize)
                throw new ODException("Stream is longer than the maximum upload size allowed.");

            long localItemSize;
            if (!sourceFileStream.TryGetLength(out localItemSize))
            {
                throw new ODException("Couldn't get length of sourceFileStream.");
            }

            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.ContentRelationshipName);
            return await UploadToUrl(sourceFileStream, options, localItemSize, serviceUri);
        }

        public async Task<ODItem> PutItemAsync(ODItemReference itemReference, ODItem itemProperties)
        {
            var command = PutItemCommand(itemReference, itemProperties);
            var item = await DataModelForRequest<ODItem>(command);
            return item;
        }

        public ServiceCommand PutItemCommand(ODItemReference itemReference, ODItem itemProperties)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("ItemReference was invalid. Requires either an ID or Path");

            Uri serviceUri = UriForItemReference(itemReference);

            var command = new ServiceCommand
            {
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpPut,
                ResponseType = typeof(ODItem),
                ContentType = ApiConstants.ContentTypeJson,
                Body = SerializeObjectToString(itemProperties)
            };

            return command;
        }

        /// <summary>
        /// Upload a new file to a parent folder item.
        /// </summary>
        /// <param name="parentItemReference">Item ID for the parent folder.</param>
        /// <param name="filename">Filename of the new file to create</param>
        /// <param name="sourceFileStream">Data stream for the new file</param>
        /// <param name="options">Upload options</param>
        /// <returns></returns>
        public async Task<ODItem> PutNewFileToParentItemAsync(ODItemReference parentItemReference, string filename, Stream sourceFileStream, ItemUploadOptions options)
        {
            if (!filename.ValidFilename())
                throw new ArgumentException("Filename contains invalid characters.");
            if (!parentItemReference.IsValid())
                throw new ArgumentException("parentItemReference was invalid. Requires either an ID or Path");
            if (null == options)
                throw new ArgumentNullException("options");
            if (null == sourceFileStream)
                throw new ArgumentNullException("sourceFileStream");
            if (sourceFileStream.Length > MaxRegularUploadSize)
                throw new ODException("Stream is longer than the maximum upload size allowed.");

            long localItemSize;
            if (!sourceFileStream.TryGetLength(out localItemSize))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Couldn't determine length of sourceFileStream.");
            }

            string navigationValue = string.Concat(ApiConstants.ChildrenRelationshipName, UrlSeperator, filename, UrlSeperator, ApiConstants.ContentRelationshipName);
            Uri serviceUri = UriForItemReference(parentItemReference, navigationValue);
            await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpPut);
            return await UploadToUrl(sourceFileStream, options, localItemSize, serviceUri);
        }


        /// <summary>
        /// Uploads a file using the resumable fragment upload API, splitting the file into smaller pieces to upload.
        /// </summary>
        /// <param name="parentItemReference"></param>
        /// <param name="filename"></param>
        /// <param name="sourceFileStream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ODItem> UploadLargeFileAsync(ODItemReference parentItemReference, string filename, Stream sourceFileStream, ItemUploadOptions options)
        {
            if (!parentItemReference.IsValid())
                throw new ArgumentException("parentItemReference isn't valid");
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!filename.ValidFilename())
                throw new ArgumentException("filename contains invalid characters");
            if (null == sourceFileStream)
                throw new ArgumentNullException("sourceFileStream");
            if (null == options)
                throw new ArgumentNullException("options");

            string navigation = string.Concat(":/", filename, ":/", ApiConstants.UploadCreateSessionAction);
            Uri serviceUri = UriForItemReference(parentItemReference, navigation);

            return await StartLargeFileTransfer(serviceUri, sourceFileStream, options);
        }

        /// <summary>
        /// Uploads a file using the resumable fragment upload API, splitting the file into smaller pieces to upload.
        /// 
        /// You can use this method to replace an existing item (using itemReference.Id) or to upload a new item
        /// (using itemReference.Path).
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="sourceFileStream"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ODItem> UploadLargeFileAsync(ODItemReference itemReference, Stream sourceFileStream, ItemUploadOptions options)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("parentItemReference isn't valid");
            if (null == sourceFileStream)
                throw new ArgumentNullException("sourceFileStream");
            if (null == options)
                throw new ArgumentNullException("options");

            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.UploadCreateSessionAction);
            return await StartLargeFileTransfer(serviceUri, sourceFileStream, options);
        }

        /// <summary>
        /// Upload a file from a URL.
        /// </summary>
        /// <param name="parentItemReference"></param>
        /// <param name="sourceUrl"></param>
        /// <param name="destinationFilename"></param>
        /// <returns></returns>
        public async Task<ODAsyncTask> UploadFromUrlAsync(ODItemReference parentItemReference, string sourceUrl, string destinationFilename)
        {
            if (!destinationFilename.ValidFilename())
                throw new ArgumentException("destinationFilename contains invalid characters.");
            if (!parentItemReference.IsValid())
                throw new ArgumentException("parentItemReference was invalid. Requires either an ID or Path");
            if (string.IsNullOrEmpty(sourceUrl))
                throw new ArgumentNullException("sourceUrl");

            Uri serviceUri = UriForItemReference(parentItemReference, ApiConstants.ChildrenRelationshipName);
            var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpPost);
            AddAsyncHeaders(request);

            var uploadItem = new ODItem { Name = destinationFilename, File = new Facets.FileFacet(), SourceUrlAnnotation = sourceUrl };
            await SerializeObjectToRequestBody(uploadItem, request);
            return await AsyncTaskResultForRequest(request, serviceUri);
        }

        /// <summary>
        /// Create a new folder as a child of an existing folder, speciifed by parentFolderId.
        /// </summary>
        /// <param name="parentItemReference"></param>
        /// <param name="newFolderName"></param>
        /// <returns></returns>
        public async Task<ODItem> CreateFolderAsync(ODItemReference parentItemReference, string newFolderName)
        {
            var command = CreateFolderCommand(parentItemReference, newFolderName);
            var item = await DataModelForRequest<ODItem>(command);
            return item;
        }

        public ServiceCommand CreateFolderCommand(ODItemReference parentItemReference, string folderName)
        {
            if (!parentItemReference.IsValid())
                throw new ArgumentException("parentItemReference was invalid. Requires either an ID or Path");
            if (string.IsNullOrEmpty(folderName))
                throw new ArgumentNullException("folderName");

            Uri serviceUri = UriForItemReference(parentItemReference, ApiConstants.ChildrenRelationshipName);
            var body = new { name = folderName, folder = new { } };

            var command = new ServiceCommand {
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpPost,
                ContentType = ApiConstants.ContentTypeJson,
                Body = SerializeObjectToString(body),
                ResponseType = typeof(ODItem)
            };
            return command;
        }

        /// <summary>
        /// Deletes the item on the server with the specified item-id.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<bool> DeleteItemAsync(ODItemReference itemReference, ItemDeleteOptions options)
        {
            var command = DeleteItemCommand(itemReference, options);
            var request = await CreateHttpRequestAsync(command);
            var response = await GetHttpResponseAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                throw await response.ToException();
            }
        }

        public ServiceCommand DeleteItemCommand(ODItemReference itemReference, ItemDeleteOptions options)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("itemReference was invalid. Requires either an ID or Path");
            if (null == options)
                throw new ArgumentNullException("options");

            Uri serviceUri = UriForItemReference(itemReference);

            var command = new ServiceCommand
            {
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpDelete
            };
            options.ModifyCommand(command);

            return command;
        }

        /// <summary>
        /// Update an item referenced by itemReference with the changes in the changes parameter.
        /// All fields that are not changed in an Item should be set to null before passing the item
        /// into this call.
        /// </summary>
        /// <param name="itemReference"></param>
        /// <param name="changes"></param>
        /// <returns></returns>
        public async Task<ODItem> PatchItemAsync(ODItemReference itemReference, ODItem changes)
        {
            var command = PatchItemCommand(itemReference, changes);
            var request = await CreateHttpRequestAsync(command);
            var response = await GetHttpResponseAsync(request);
            if (response.StatusCode.IsSuccess())
            {
                return await response.ConvertToDataModel<ODItem>();
            }
            else
            {
                throw await response.ToException();
            }
        }

        public ServiceCommand PatchItemCommand(ODItemReference itemReference, ODItem changes)
        {
            if (!itemReference.IsValid()) throw new ArgumentException("itemReference is not a valid reference.");
            if (null == changes) throw new ArgumentNullException("changes");

            Uri serviceUri = UriForItemReference(itemReference);
            var command = new ServiceCommand
            {
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpPatch,
                ContentType = ApiConstants.ContentTypeJson,
                Body = SerializeObjectToString(changes),
                ResponseType = typeof(ODItem)
            };

            return command;

        }

        /// <summary>
        /// Copy an item to a new location, optionally providing a new name.
        /// </summary>
        /// <param name="originalItemReference"></param>
        /// <param name="destinationParentItemReference"></param>
        /// <param name="optionalFilename"></param>
        /// <returns></returns>
        public async Task<ODAsyncTask> CopyItemAsync(ODItemReference originalItemReference, ODItemReference destinationParentItemReference, string optionalFilename = null)
        {
            if (!originalItemReference.IsValid())
                throw new ArgumentException("originalItemReference");
            if (!destinationParentItemReference.IsValid())
                throw new ArgumentException("destinationParentItemReference");

            Uri serviceUri = UriForItemReference(originalItemReference, ApiConstants.CopyItemAction);
            var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpPost);
            AddAsyncHeaders(request);

            var postBody = new
            {
                parentReference = destinationParentItemReference,
                name = optionalFilename
            };
            await SerializeObjectToRequestBody(postBody, request);
            return await AsyncTaskResultForRequest(request, serviceUri);
        }

        public ServiceCommand CopyItemCommand(ODItemReference originalItemReference, ODItemReference destinationParentItemReference, string optionalFilename = null)
        {
            if (!originalItemReference.IsValid())
                throw new ArgumentException("originalItemReference");
            if (!destinationParentItemReference.IsValid())
                throw new ArgumentException("destinationParentItemReference");

            Uri serviceUri = UriForItemReference(originalItemReference, ApiConstants.CopyItemAction);

            var postBody = new
            {
                parentReference = destinationParentItemReference,
                name = optionalFilename
            };


            var command = new ServiceCommand
            {
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpPost,
                ContentType = ApiConstants.ContentTypeJson,
                Body = SerializeObjectToString(postBody),
                ResponseType = typeof(ODAsyncTask)
            };
            AddAsyncHeaders(command);

            return command;
        }

        public async Task<ODPermission> CreateSharingLinkAsync(ODItemReference itemReference, OneDrive.Facets.LinkType type)
        {
            var command = CreateSharingLinkCommand(itemReference, type);
            return await DataModelForRequest<ODPermission>(command);
        }

        public ServiceCommand CreateSharingLinkCommand(ODItemReference itemReference, OneDrive.Facets.LinkType type)
        {
            if (!itemReference.IsValid())
                throw new ArgumentException("itemReference");

            Uri serviceUri = UriForItemReference(itemReference, ApiConstants.CreateLinkAction);
            var postBody = new
            {
                type = type
            };

            return new ServiceCommand { 
                ServiceUrl = serviceUri,
                HttpVerb = ApiConstants.HttpPost,
                ContentType = ApiConstants.ContentTypeJson,
                Body = SerializeObjectToString(postBody),
                ResponseType = typeof(ODPermission)
            };
        }

        /// <summary>
        /// Returns change information for items at and below the specified item-id.
        /// </summary>
        /// <param name="rootItemReference"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ODViewChangesResult> ViewChangesAsync(ODItemReference rootItemReference, ViewChangesOptions options)
        {
            if (!rootItemReference.IsValid())
                throw new ArgumentException("rootItemReference was invalid. Requires either an ID or Path");
            if (null == options)
                throw new ArgumentNullException("options");

            var queryParams = new QueryStringBuilder();
            options.ModifyQueryString(queryParams);

            Uri serviceUri = UriForItemReference(rootItemReference, ApiConstants.ViewChangesServiceAction, queryParams);
            var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpGet);

            return await DataModelForRequest<ODViewChangesResult>(request);
        }


        /// <summary>
        /// Search for items matching a given search query
        /// </summary>
        /// <param name="rootItemReference"></param>
        /// <param name="searchQuery"></param>
        /// <param name="itemRetrievalOptions"></param>
        /// <returns></returns>
        public async Task<ODItemCollection> SearchForItemsAsync(ODItemReference rootItemReference, string searchQuery, ItemRetrievalOptions itemRetrievalOptions)
        {
            if (!rootItemReference.IsValid())
                throw new ArgumentException("rootItemReference was invalid. Requires either an ID or Path");
            if (null == searchQuery)
                throw new ArgumentNullException("searchQuery");
            if (null == itemRetrievalOptions)
                throw new ArgumentNullException("itemRetrievalOptions");

            var queryParams = ODataOptionsToQueryString(itemRetrievalOptions);
            queryParams.Add(ApiConstants.SearchQueryParameterKey, searchQuery);

            Uri serviceUri = UriForItemReference(rootItemReference, ApiConstants.SearchServiceAction, queryParams);
            var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpGet);
            return await DataModelForRequest<ODItemCollection>(request);
        }


        public async Task<ODDrive> GetDrive(ODItemReference driveReference = null)
        {
            if (null == driveReference)
                driveReference = new ODItemReference { DriveId = "me" };
            if (string.IsNullOrEmpty(driveReference.DriveId))
                throw new ArgumentException("driveReference must include a DriveId");
            if (!string.IsNullOrEmpty(driveReference.Id) || !string.IsNullOrEmpty(driveReference.Path))
                throw new ArgumentException("driveReference must only contain a value for DriveId");

            var serviceUri = UriForItemReference(driveReference);
            return await DataModelForRequest<ODDrive>(serviceUri, ApiConstants.HttpGet);
        }

        /// <summary>
        /// Execute a series of commands and return the result.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public async Task<ServiceResponse[]> BatchCommandsAsync(IEnumerable<ServiceCommand> commands)
        {
            Uri batchEndpoint = new Uri(RootUrl + "/$batch");

            MultipartBuilder builder = new MultipartBuilder { Format = "multipart/mixed" };
            var batchCommandRequests = from c in commands
                                       select new MultipartContent { ContentType = "application/http", TextContent = c.RawHttpRequest() };
            builder.Parts.AddRange(batchCommandRequests);
            
            var request = await CreateHttpRequestAsync(batchEndpoint, ApiConstants.HttpPost);
            request.ContentType = builder.ContentType;

            var requestStream = await request.GetRequestStreamAsync();
            await builder.WriteToStreamAsync(requestStream);

            var response = await request.GetResponseAsync();

            var responseStream = await response.GetResponseStreamAsync();

            var parsedResponse = await MultipartParser.ParseStreamAsync(response.ContentType, responseStream);
            
            // TODO: Finish this implementation

            return null;

        }
    }
}
