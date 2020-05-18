using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Services.Seo;
using Amazon.S3;
using Amazon.Runtime;
using Amazon;
using Amazon.S3.Model;
using System.IO;

namespace Nop.Plugin.Misc.Skoohii.Services
{
    /// <summary>
    /// Picture service for Windows Azure
    /// </summary>
    public partial class Aws3PictureService : PictureService, IDisposable
    {
        #region Fields
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly MediaSettings _mediaSettings;
        private IAmazonS3 _amazonS3Client;
        private string _bucketName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _isisposed;
        #endregion

        #region Ctor

        public Aws3PictureService(INopDataProvider dataProvider,
            IDownloadService downloadService,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            SkoohiiAwsS3Settings skoohiiAwsS3Settings)
            : base(dataProvider,
                  downloadService,
                  eventPublisher,
                  httpContextAccessor,
                  fileProvider,
                  productAttributeParser,
                  pictureRepository,
                  pictureBinaryRepository,
                  productPictureRepository,
                  settingService,
                  urlRecordService,
                  webHelper,
                  mediaSettings)
        {
            _cacheKeyService = cacheKeyService;
            _staticCacheManager = staticCacheManager;
            _mediaSettings = mediaSettings;
            _httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrEmpty(skoohiiAwsS3Settings.AccessKeyId))
                throw new Exception("AWS S3 access key is not specified");

            if (string.IsNullOrEmpty(skoohiiAwsS3Settings.SecretAccessKey))
                throw new Exception("AWS S3 secret access key is not specified");

            if (string.IsNullOrEmpty(skoohiiAwsS3Settings.Bucket))
                throw new Exception("AWS S3 bucket is not specified");

            if (string.IsNullOrEmpty(skoohiiAwsS3Settings.Region))
                throw new Exception("AWS S3 region is not specified");

            var awsCredentials = new BasicAWSCredentials(skoohiiAwsS3Settings.AccessKeyId, skoohiiAwsS3Settings.SecretAccessKey);
            var s3Config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(skoohiiAwsS3Settings.Region) };
            _amazonS3Client = new AmazonS3Client(awsCredentials, s3Config);
            _bucketName = skoohiiAwsS3Settings.Bucket;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected override async void DeletePictureThumbs(Picture picture)
        {
            await DeletePictureThumbsAsync(picture);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbLocalPath(string thumbFileName)
        {
            return _amazonS3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = thumbFileName,
            });
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>Local picture thumb path</returns>
        protected override string GetThumbUrl(string thumbFileName, string storeLocation = null)
        {
            return GetThumbLocalPath(thumbFileName);
        }

        /// <summary>
        /// Get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected override bool GeneratedThumbExists(string thumbFilePath, string thumbFileName)
        {
            return GeneratedThumbExistsAsync(thumbFilePath, thumbFileName).Result;
        }

        /// <summary>
        /// Save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected override async void SaveThumb(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            await SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);
        }

        /// <summary>
        /// Initiates an asynchronous operation to delete picture thumbs
        /// </summary>
        /// <param name="picture">Picture</param>
        protected virtual async Task DeletePictureThumbsAsync(Picture picture)
        {
            //create a string containing the blob name prefix
            var prefix = $"{picture.Id:0000000}";

            string continuationToken = null;

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken,
                };
                var result = await _amazonS3Client.ListObjectsV2Async(request, _httpContextAccessor.HttpContext.RequestAborted);

                //delete files in result segment
                await Task.WhenAll(result.S3Objects.Select(blobItem =>
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = blobItem.BucketName,
                        Key = blobItem.Key
                    };

                    return _amazonS3Client.DeleteObjectAsync(deleteObjectRequest);
                }));

                //get the continuation token.
                continuationToken = result.ContinuationToken;
            }
            while (continuationToken != null);

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefixCacheKey);
        }

        /// <summary>
        /// Initiates an asynchronous operation to get a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <returns>Result</returns>
        protected virtual async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
        {
            try
            {
                var key = _cacheKeyService.PrepareKeyForDefaultCache(NopMediaDefaults.ThumbExistsCacheKey, thumbFileName);

                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    //var result = await _amazonS3Client.GetObjectAsync(
                    //        new GetObjectRequest
                    //        {
                    //            BucketName = _bucketName,
                    //            Key = thumbFileName,
                    //            ByteRange = new ByteRange("bytes=0-3")
                    //        },
                    //        _httpContextAccessor.HttpContext.RequestAborted);

                    var result = await _amazonS3Client.GetObjectMetadataAsync(
                           new GetObjectMetadataRequest
                           {
                               BucketName = _bucketName,
                               Key = thumbFileName,
                           }, _httpContextAccessor.HttpContext.RequestAborted);

                    return result.HttpStatusCode != System.Net.HttpStatusCode.NotFound;
                });
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initiates an asynchronous operation to save a value indicating whether some file (thumb) already exists
        /// </summary>
        /// <param name="thumbFilePath">Thumb file path</param>
        /// <param name="thumbFileName">Thumb file name</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="binary">Picture binary</param>
        protected virtual async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            await _amazonS3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = thumbFileName,
                InputStream = new MemoryStream(binary),
                ContentType = mimeType,
            }, _httpContextAccessor.HttpContext.RequestAborted);

            _staticCacheManager.RemoveByPrefix(NopMediaDefaults.ThumbsExistsPrefixCacheKey);
        }

        #endregion

        #region Disposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isisposed)
                return;
            if (disposing)
            {
                _amazonS3Client?.Dispose();
            }
            _isisposed = true;
        }
        ~Aws3PictureService()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        #endregion
        //public async Task<DataStoreRespnse> GetAsync(string key, CancellationToken cancellationToken = default)
        //{
        //    using (var response = await _amazonS3Client.GetObjectAsync(_bucketName, key, cancellationToken))
        //    {
        //        using (var responseStream = response.ResponseStream)
        //        {
        //            var stream = new MemoryStream();
        //            await responseStream.CopyToAsync(stream, cancellationToken);
        //            stream.Position = 0;
        //            return new DataStoreRespnse
        //            {
        //                ContentLength = response.ContentLength,
        //                StatusCode = response.HttpStatusCode,
        //                Headers = response.Headers.Keys.ToDictionary(k => k, v => response.Headers[v]),
        //                ResponseStream = stream,
        //                ETag = response.ETag,
        //                LastModified = response.LastModified
        //            };
        //        }
        //    }
        //}

    }
}